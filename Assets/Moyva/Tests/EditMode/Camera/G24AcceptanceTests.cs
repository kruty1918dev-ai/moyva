using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Camera.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Camera
{
    /// <summary>
    /// G24 acceptance invariants for camera zoom: wheel deltas clamp into the
    /// configured [min,max] range and never accumulate past it, pinch zoom
    /// rejects invalid scale factors and caps per-call target movement,
    /// programmatic zoom (set/restore across menus) lands immediately without
    /// a visible jump, and the perspective FOV guard keeps the camera usable.
    /// </summary>
    public sealed class G24AcceptanceTests
    {
        private GameObject _go;
        private UnityEngine.Camera _cam;
        private CameraSettingsSO _settings;
        private CameraZoom _zoom;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("g24cam");
            _cam = _go.AddComponent<UnityEngine.Camera>();
            _cam.orthographic = true;
            _cam.orthographicSize = 20f;

            _settings = new CameraSettingsSO();
            _settings.controlProfile = new CameraControlProfile
            {
                smoothTime = 0.4f,
                zoomSpeed = 4f,
                minZoom = 4f,
                maxZoom = 50f,
                touchPinchZoomSensitivity = 1f,
            };
            _zoom = new CameraZoom(_cam, _settings, null);
            _zoom.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        /// <summary>
        /// Applies the current zoom target without depending on
        /// Time.unscaledDeltaTime: a near-identity pinch moves the target past
        /// the early-out epsilon and writes it to the camera immediately.
        /// Down-flush nudges toward zoom-out, up-flush toward zoom-in; pick the
        /// one whose direction is not blocked by a saturated clamp.
        /// </summary>
        private void ApplyTargetUp()
            => _zoom.ZoomCameraByScale(1.0005f, immediate: true);

        private void ApplyTargetDown()
            => _zoom.ZoomCameraByScale(0.9995f, immediate: true);

        [Test]
        public void WheelZoom_NeverPushesTarget_BeyondConfiguredRange()
        {
            // Spam zoom-in far past min — the target must saturate at minZoom.
            for (int i = 0; i < 100; i++)
                _zoom.ZoomCamera(120f); // one wheel notch toward zoom-in
            ApplyTargetUp();
            Assert.AreEqual(4f, _zoom.CurrentZoom, 0.05f,
                "zoom-in must clamp at minZoom");

            for (int i = 0; i < 100; i++)
                _zoom.ZoomCamera(-120f);
            ApplyTargetDown();
            Assert.AreEqual(50f, _zoom.CurrentZoom, 0.05f,
                "zoom-out must clamp at maxZoom");
        }

        [Test]
        public void WheelZoom_RawDelta_Normalizes_ToStepFraction()
        {
            // A 120-unit raw wheel notch = one step; fractions of it scale.
            _zoom.SetZoomImmediate(27f);
            _zoom.ZoomCamera(120f);
            ApplyTargetDown();
            Assert.AreEqual(27f - 4f, _zoom.CurrentZoom, 0.05f,
                "one notch = zoomSpeed worth of target movement");

            // Sub-notch deltas still work (trackpads/UI scroll passthrough).
            _zoom.SetZoomImmediate(27f);
            _zoom.ZoomCamera(0.5f);
            ApplyTargetDown();
            Assert.AreEqual(27f - 0.5f * 4f, _zoom.CurrentZoom, 0.05f,
                "fractional delta passes through un-normalized");
        }

        [Test]
        public void WheelZoom_ExtremeDelta_ClampsToMaxStep()
        {
            _zoom.SetZoomImmediate(27f);
            _zoom.ZoomCamera(36000f); // absurd delta
            ApplyTargetDown();
            // Normalized delta clamps to ±3 → step = 3 * zoomSpeed = 12.
            Assert.AreEqual(27f - 12f, _zoom.CurrentZoom, 0.05f,
                "a single wheel event can never jump more than the step cap");
        }

        [Test]
        public void PinchZoom_RejectsInvalidScale_AndCapsPerCallStep()
        {
            _zoom.SetZoomImmediate(27f);
            _zoom.ZoomCameraByScale(0f, true);
            _zoom.ZoomCameraByScale(float.NaN, true);
            _zoom.ZoomCameraByScale(float.PositiveInfinity, true);
            Assert.AreEqual(27f, _zoom.CurrentZoom, 0.001f,
                "invalid pinch scales must be ignored");

            // A giant pinch factor moves the target by at most 10% of range.
            _zoom.ZoomCameraByScale(1000f, true);
            float cap = 0.1f * (50f - 4f);
            Assert.LessOrEqual(_zoom.CurrentZoom, 27f + cap + 0.01f,
                "one pinch call may not exceed the per-call step cap");
            Assert.Greater(_zoom.CurrentZoom, 27f,
                "a valid pinch must still zoom");
        }

        [Test]
        public void PinchZoom_NonImmediate_MovesTarget_NotCurrent()
        {
            _zoom.SetZoomImmediate(27f);
            _zoom.ZoomCameraByScale(0.5f, immediate: false);
            Assert.AreEqual(27f, _zoom.CurrentZoom, 0.001f,
                "non-immediate pinch must not touch the applied zoom");
            ApplyTargetDown();
            Assert.Less(_zoom.CurrentZoom, 27f,
                "the target change still applies on the next immediate call");
        }

        [Test]
        public void ProgrammaticZoom_ResetsAndPersists_WithoutJump()
        {
            // Menu open/close cycle: state service must be converged from the
            // first sample so no smoothing ramp re-animates the zoom.
            _zoom.SetZoomImmediate(30f);
            Assert.AreEqual(30f, _zoom.CurrentZoom, 0.001f);

            var state = new CameraZoomStateService(_cam, _settings);
            state.Initialize();
            Assert.AreEqual(state.NormalizedZoom, state.SmoothedNormalizedZoom, 1e-5f,
                "a fresh zoom state starts converged — no menu-open jump");

            // Restore a previous zoom programmatically — clamps apply too.
            _zoom.ForceZoomCamera(999f);
            ApplyTargetDown();
            Assert.AreEqual(50f, _zoom.CurrentZoom, 0.05f,
                "restoring an out-of-range zoom clamps instead of breaking the camera");
        }

        [Test]
        public void PerspectiveCamera_FovClamps_StayInValidRange()
        {
            _cam.orthographic = false;
            _cam.fieldOfView = 60f;
            var zoom = new CameraZoom(_cam, _settings, null);
            zoom.Initialize();

            for (int i = 0; i < 100; i++)
                zoom.ZoomCamera(120f);
            zoom.ZoomCameraByScale(1.0005f, immediate: true);
            Assert.GreaterOrEqual(_cam.fieldOfView, 4f - 0.01f,
                "perspective zoom-in clamps at minZoom (>= FOV floor)");
            Assert.LessOrEqual(_cam.fieldOfView, 179f);

            for (int i = 0; i < 100; i++)
                zoom.ZoomCamera(-120f);
            zoom.ZoomCameraByScale(0.9995f, immediate: true);
            Assert.AreEqual(50f, _cam.fieldOfView, 0.05f,
                "perspective zoom-out clamps at the resolved max");
            Assert.LessOrEqual(_cam.fieldOfView, 179f,
                "the resolved max must stay under the FOV ceiling");
        }

        [Test]
        public void TargetZoom_Clamped_OnEveryMutationPath()
        {
            // Any programmatic write above/below range lands inside [min,max].
            _zoom.SetZoomImmediate(20f);
            _zoom.SetTargetZoom(999f);
            ApplyTargetDown();
            Assert.AreEqual(50f, _zoom.CurrentZoom, 0.05f);

            _zoom.SetTargetZoom(-3f);
            ApplyTargetUp();
            Assert.AreEqual(4f, _zoom.CurrentZoom, 0.05f);
        }
    }
}
