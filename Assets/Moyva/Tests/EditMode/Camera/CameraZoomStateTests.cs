using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Camera.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Camera
{
    /// <summary>
    /// Shared normalized-zoom state: normalization math, far-view window
    /// evaluation and the smoothing behavior of CameraZoomStateService.
    /// </summary>
    public sealed class CameraZoomStateTests
    {
        [Test]
        public void NormalizeZoom_Clamps_To_Configured_Range()
        {
            Assert.AreEqual(0f, CameraZoomMath.NormalizeZoom(4f, 4f, 50f), 1e-4f);
            Assert.AreEqual(1f, CameraZoomMath.NormalizeZoom(50f, 4f, 50f), 1e-4f);
            Assert.AreEqual(0.5f, CameraZoomMath.NormalizeZoom(27f, 4f, 50f), 1e-4f);
            Assert.AreEqual(0f, CameraZoomMath.NormalizeZoom(-10f, 4f, 50f));
            Assert.AreEqual(1f, CameraZoomMath.NormalizeZoom(999f, 4f, 50f));
        }

        [Test]
        public void NormalizeZoom_Degenerate_Range_Returns_Zero()
        {
            Assert.AreEqual(0f, CameraZoomMath.NormalizeZoom(10f, 50f, 50f));
            Assert.AreEqual(0f, CameraZoomMath.NormalizeZoom(10f, 50f, 4f));
        }

        [Test]
        public void FarViewWeight_Is_Zero_Below_Start_And_One_Above_Full()
        {
            Assert.AreEqual(0f, CameraZoomMath.EvaluateFarViewWeight(0.29f, 0.30f, 0.85f));
            Assert.AreEqual(0f, CameraZoomMath.EvaluateFarViewWeight(0.0f, 0.30f, 0.85f));
            Assert.AreEqual(1f, CameraZoomMath.EvaluateFarViewWeight(0.85f, 0.30f, 0.85f));
            Assert.AreEqual(1f, CameraZoomMath.EvaluateFarViewWeight(1.0f, 0.30f, 0.85f));
        }

        [Test]
        public void FarViewWeight_Is_Monotonic_And_Smooth_Inside_Window()
        {
            float prev = -1f;
            for (int i = 0; i <= 20; i++)
            {
                float t = Mathf.Lerp(0.30f, 0.85f, i / 20f);
                float w = CameraZoomMath.EvaluateFarViewWeight(t, 0.30f, 0.85f);
                Assert.GreaterOrEqual(w, prev, $"weight must be monotonic, t={t}");
                prev = w;
            }
            Assert.AreEqual(0.5f, CameraZoomMath.EvaluateFarViewWeight(0.575f, 0.30f, 0.85f), 1e-3f);
        }

        [Test]
        public void FarViewWeight_Shape_Biases_Curve()
        {
            float neutral = CameraZoomMath.EvaluateFarViewWeight(0.5f, 0.30f, 0.85f, 1f);
            float late = CameraZoomMath.EvaluateFarViewWeight(0.5f, 0.30f, 0.85f, 2f);
            float early = CameraZoomMath.EvaluateFarViewWeight(0.5f, 0.30f, 0.85f, 0.5f);
            Assert.Less(late, neutral);
            Assert.Greater(early, neutral);
        }

        [Test]
        public void FarViewWeight_Degenerate_Window_Is_Step()
        {
            Assert.AreEqual(0f, CameraZoomMath.EvaluateFarViewWeight(0.4f, 0.5f, 0.5f));
            Assert.AreEqual(1f, CameraZoomMath.EvaluateFarViewWeight(0.6f, 0.5f, 0.5f));
        }

        [Test]
        public void SmoothingFactor_Is_FrameRate_Independent()
        {
            // Two half-steps at dt=1/120 should land close to one step at dt=1/60.
            float one = CameraZoomMath.SmoothingFactor(4f, 1f / 60f);
            float half = CameraZoomMath.SmoothingFactor(4f, 1f / 120f);
            float composed = half + (1f - half) * half;
            Assert.AreEqual(one, composed, 1e-4f);
        }

        [Test]
        public void Service_Reports_Normalized_Zoom_From_Configured_Range()
        {
            var go = new GameObject("cam");
            try
            {
                var cam = go.AddComponent<UnityEngine.Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 27f; // middle of 4..50

                var settings = new CameraSettingsSO();
                settings.controlProfile.minZoom = 4f;
                settings.controlProfile.maxZoom = 50f;
                settings.farView = new CameraFarViewSettings
                {
                    start = 0.30f,
                    full = 0.85f,
                };

                var service = new CameraZoomStateService(cam, settings);
                service.Initialize();

                Assert.AreEqual(0.5f, service.NormalizedZoom, 1e-3f);
                Assert.AreEqual(0.5f, service.SmoothedNormalizedZoom, 1e-3f);
                Assert.AreEqual(27f, service.CurrentZoom, 1e-3f);
                Assert.IsFalse(service.IsPerspective);

                // 0.5 normalized sits inside the configured far-view window
                // (start 0.30 / full 0.85) → partial weight.
                Assert.Greater(service.FarViewWeight, 0.05f);
                Assert.Less(service.FarViewWeight, 0.95f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Service_Smoothing_Converges_Towards_Target()
        {
            var go = new GameObject("cam");
            try
            {
                var cam = go.AddComponent<UnityEngine.Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 4f;

                var settings = new CameraSettingsSO();
                settings.controlProfile.minZoom = 4f;
                settings.controlProfile.maxZoom = 50f;
                settings.farView = new CameraFarViewSettings
                {
                    start = 0.3f,
                    full = 0.85f,
                    smoothing = 4f,
                    shape = 1f,
                };

                var service = new CameraZoomStateService(cam, settings);
                service.Initialize();
                Assert.AreEqual(0f, service.SmoothedNormalizedZoom, 1e-4f);

                cam.orthographicSize = 50f;
                for (int i = 0; i < 120; i++)
                    service.TickForTest(1f / 60f);

                Assert.AreEqual(1f, service.SmoothedNormalizedZoom, 1e-2f);
                Assert.AreEqual(1f, service.FarViewWeight, 1e-3f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void FarViewSettings_Normalize_Guards_Window()
        {
            var broken = new CameraFarViewSettings
            {
                start = -1f,
                full = -2f,
                smoothing = 0f,
                shape = 0f,
            }.Normalize();

            Assert.AreEqual(0f, broken.start);
            Assert.Greater(broken.full, broken.start);
            Assert.GreaterOrEqual(broken.smoothing, 0.05f);
            Assert.GreaterOrEqual(broken.shape, 0.2f);
        }
    }
}
