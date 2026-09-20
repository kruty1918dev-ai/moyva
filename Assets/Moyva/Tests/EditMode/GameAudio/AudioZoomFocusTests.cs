using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.GameAudio.API;
using Kruty1918.Moyva.GameAudio.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.GameAudio.Tests
{
    /// <summary>
    /// AudioZoomFocusService: consumes the shared camera zoom state so audio
    /// and far-view visuals transition in lockstep; per-bed cutoff override.
    /// </summary>
    public sealed class AudioZoomFocusTests
    {
        private sealed class StubZoomState : ICameraZoomState
        {
            public float CurrentZoom => 0f;
            public float MinZoom => 4f;
            public float MaxZoom => 50f;
            public float NormalizedZoom => SmoothedNormalizedZoom;
            public float SmoothedNormalizedZoom { get; set; }
            public float FarViewWeight => SmoothedNormalizedZoom;
            public bool IsPerspective => false;
        }

        [Test]
        public void Tick_Uses_Shared_Smoothed_Zoom_Directly()
        {
            var zoom = new StubZoomState { SmoothedNormalizedZoom = 0.63f };
            var service = new AudioZoomFocusService(
                new AudioAmbienceConfig(), zoom);
            service.Initialize();

            // First tick must already reflect the shared value — no extra
            // local smoothing on top of the shared state's smoothing.
            service.Tick();
            Assert.AreEqual(0.63f, service.ZoomT, 1e-4f);

            zoom.SmoothedNormalizedZoom = 0.1f;
            service.Tick();
            Assert.AreEqual(0.1f, service.ZoomT, 1e-4f);
        }

        [Test]
        public void EmitterFactor_Fades_Across_Configured_Window()
        {
            var zoom = new StubZoomState();
            var cfg = new AudioAmbienceConfig();
            cfg.zoom.emitterFadeStart = 0.45f;
            cfg.zoom.emitterFadeEnd = 0.9f;
            var service = new AudioZoomFocusService(cfg, zoom);

            zoom.SmoothedNormalizedZoom = 0f;
            service.Tick();
            Assert.AreEqual(1f, service.EvaluateEmitterFactor(-1f, -1f), 1e-4f);

            zoom.SmoothedNormalizedZoom = 1f;
            service.Tick();
            Assert.AreEqual(0f, service.EvaluateEmitterFactor(-1f, -1f), 1e-4f);

            // Ground sounds fully gone at far zoom: buildings/water silent.
            zoom.SmoothedNormalizedZoom = 0.95f;
            service.Tick();
            Assert.AreEqual(0f, service.EvaluateEmitterFactor(-1f, -1f), 1e-4f);
        }

        [Test]
        public void BedCutoff_Lerps_Near_To_Far()
        {
            var zoom = new StubZoomState();
            var cfg = new AudioAmbienceConfig();
            cfg.zoom.nearCutoff = 22000f;
            cfg.zoom.farCutoff = 900f;
            var service = new AudioZoomFocusService(cfg, zoom);

            zoom.SmoothedNormalizedZoom = 0f;
            service.Tick();
            Assert.AreEqual(22000f, service.EvaluateBedCutoff(), 1f);

            zoom.SmoothedNormalizedZoom = 1f;
            service.Tick();
            Assert.AreEqual(900f, service.EvaluateBedCutoff(), 1f);
        }

        [Test]
        public void BedCutoff_PerBed_Override_Darkens_Further()
        {
            var zoom = new StubZoomState { SmoothedNormalizedZoom = 1f };
            var cfg = new AudioAmbienceConfig();
            cfg.zoom.nearCutoff = 22000f;
            cfg.zoom.farCutoff = 900f;
            var service = new AudioZoomFocusService(cfg, zoom);
            service.Tick();

            // Per-bed farCutoff=400 → at full zoom the bed is darker than
            // the shared 900 cutoff.
            Assert.AreEqual(400f, service.EvaluateBedCutoff(400f), 1f);
            // <=0 falls back to the shared value.
            Assert.AreEqual(900f, service.EvaluateBedCutoff(0f), 1f);
        }

        [Test]
        public void Disabled_Zoom_Keeps_Emitters_Audible()
        {
            var zoom = new StubZoomState { SmoothedNormalizedZoom = 1f };
            var cfg = new AudioAmbienceConfig();
            cfg.zoom.enabled = false;
            var service = new AudioZoomFocusService(cfg, zoom);
            service.Tick();

            Assert.AreEqual(1f, service.EvaluateEmitterFactor(-1f, -1f));
            Assert.AreEqual(22000f, service.EvaluateBedCutoff(), 1f);
        }
    }
}
