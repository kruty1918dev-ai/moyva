using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Visuals;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Tests.Visuals
{
    /// <summary>
    /// Far-view atmosphere: JSON preset resolution, driver global pushes and
    /// renderer feature lifecycle/shader binding.
    /// </summary>
    public sealed class FarViewAtmosphereTests
    {
        private sealed class StubZoomState : ICameraZoomState
        {
            public float CurrentZoom { get; set; }
            public float MinZoom => 4f;
            public float MaxZoom => 50f;
            public float NormalizedZoom { get; set; }
            public float SmoothedNormalizedZoom { get; set; }
            public float FarViewWeight { get; set; }
            public bool IsPerspective { get; set; }
        }

        private FarViewAtmosphereDriver _driver;
        private float _weightBefore;

        [SetUp]
        public void SetUp()
        {
            _weightBefore = Shader.GetGlobalFloat(
                Shader.PropertyToID("_MoyvaFarViewWeight"));
        }

        [TearDown]
        public void TearDown()
        {
            _driver?.Dispose();
            _driver = null;
            Shader.SetGlobalFloat(
                Shader.PropertyToID("_MoyvaFarViewWeight"), _weightBefore);
        }

        [Test]
        public void Preset_Resolves_From_Generated_Resources()
        {
            var cfg = MoyvaJsonRuntime.GetLegacyResource<FarViewAtmosphereConfig>(
                nameof(FarViewAtmosphereConfig));

            Assert.NotNull(cfg, "far-view-atmosphere preset must resolve");
            Assert.IsTrue(cfg.enabled);
            Assert.Greater(cfg.haze.depthEnd, cfg.haze.depthStart);
            Assert.GreaterOrEqual(cfg.veil.coverage, 0f);
            Assert.LessOrEqual(cfg.veil.coverage, 1f);
        }

        [Test]
        public void Config_Normalize_Clamps_All_Ranges()
        {
            var cfg = new FarViewAtmosphereConfig
            {
                haze = new FarViewHazeSettings
                {
                    strength = 4f,
                    depthStart = 100f,
                    depthEnd = 10f,
                    gamma = 99f,
                    skyFill = -1f,
                },
                veil = new FarViewVeilSettings
                {
                    scale = 0f,
                    coverage = 7f,
                    windDirection = Vector2.zero,
                },
            }.Normalize();

            Assert.LessOrEqual(cfg.haze.strength, 1f);
            Assert.Greater(cfg.haze.depthEnd, cfg.haze.depthStart);
            Assert.LessOrEqual(cfg.haze.gamma, 4f);
            Assert.AreEqual(0f, cfg.haze.skyFill);
            Assert.Greater(cfg.veil.scale, 0f);
            Assert.LessOrEqual(cfg.veil.coverage, 1f);
            Assert.Greater(cfg.veil.windDirection.sqrMagnitude, 0.5f);
        }

        [Test]
        public void Driver_Pushes_FarViewWeight_Global()
        {
            var zoom = new StubZoomState { FarViewWeight = 0.42f };
            _driver = new FarViewAtmosphereDriver(zoom, new FarViewAtmosphereConfig());
            _driver.Initialize();
            _driver.Tick();

            float pushed = Shader.GetGlobalFloat(
                Shader.PropertyToID("_MoyvaFarViewWeight"));
            Assert.AreEqual(0.42f, pushed, 1e-4f);

            zoom.FarViewWeight = 0.9f;
            _driver.Tick();
            pushed = Shader.GetGlobalFloat(
                Shader.PropertyToID("_MoyvaFarViewWeight"));
            Assert.AreEqual(0.9f, pushed, 1e-4f);
        }

        [Test]
        public void Driver_Disabled_Config_Weights_Zero()
        {
            var zoom = new StubZoomState { FarViewWeight = 0.8f };
            var cfg = new FarViewAtmosphereConfig { enabled = false };
            _driver = new FarViewAtmosphereDriver(zoom, cfg);
            _driver.Initialize();
            _driver.Tick();

            Assert.AreEqual(0f, Shader.GetGlobalFloat(
                Shader.PropertyToID("_MoyvaFarViewWeight")), 1e-4f);
        }

        [Test]
        public void Driver_Without_ZoomState_Weights_Zero()
        {
            _driver = new FarViewAtmosphereDriver(null, new FarViewAtmosphereConfig());
            _driver.Initialize();
            _driver.Tick();

            Assert.AreEqual(0f, Shader.GetGlobalFloat(
                Shader.PropertyToID("_MoyvaFarViewWeight")), 1e-4f);
        }

        [Test]
        public void Driver_Dispose_Resets_Weight()
        {
            var zoom = new StubZoomState { FarViewWeight = 1f };
            _driver = new FarViewAtmosphereDriver(zoom, new FarViewAtmosphereConfig());
            _driver.Initialize();
            _driver.Tick();
            _driver.Dispose();
            _driver = null;

            Assert.AreEqual(0f, Shader.GetGlobalFloat(
                Shader.PropertyToID("_MoyvaFarViewWeight")), 1e-4f);
        }

        [Test]
        public void Feature_Resolves_Shader_And_Creates()
        {
            var shader = Shader.Find(FarViewAtmosphereRendererFeature.ShaderName);
            Assert.NotNull(shader,
                "Hidden/Moyva/FarViewAtmosphere must be loadable");

            var feature = ScriptableObject.CreateInstance<FarViewAtmosphereRendererFeature>();
            try
            {
                feature.Create();
                Assert.IsTrue(feature.isActive);
            }
            finally
            {
                feature.Dispose();
                Object.DestroyImmediate(feature);
            }
        }
    }
}
