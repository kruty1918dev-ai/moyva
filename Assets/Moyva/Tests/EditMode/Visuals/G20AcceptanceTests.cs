using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Visuals
{
    /// <summary>
    /// G20 acceptance invariants for the far-view atmosphere: the effect
    /// weight is strictly driven by camera zoom state (clamped 0..1), Tick
    /// pushes only on real change (hysteresis), Dispose returns global shader
    /// state to zero so a scene reset leaves no residue, and the quality tier
    /// keyword tracks the graphics profile without touching fog state.
    /// </summary>
    public sealed class G20AcceptanceTests
    {
        private sealed class StubZoom : ICameraZoomState
        {
            public float CurrentZoom { get; set; }
            public float MinZoom => 4f;
            public float MaxZoom => 50f;
            public float NormalizedZoom { get; set; }
            public float SmoothedNormalizedZoom { get; set; }
            public float FarViewWeight { get; set; }
            public bool IsPerspective { get; set; }
        }

        private sealed class StubGraphicsSettings : IGraphicsSettingsService
        {
            public GraphicsSettingsData Settings { get; private set; } =
                new GraphicsSettingsData();
            public event System.Action<GraphicsSettingsData> OnSettingsChanged;
            public void SetProfile(GraphicsQualityProfile profile)
            {
                var d = Settings;
                d.Profile = profile;
                Settings = d;
                OnSettingsChanged?.Invoke(Settings);
            }
            public void SetTargetFrameRate(int v) { }
            public void SetRenderScale(float v) { }
            public void SetDynamicRenderScale(bool v) { }
            public void SetCloseZoomOptimization(bool v) { }
            public void SetTextureMipmapLimit(int v) { }
            public void SetAntiAliasing(int v) { }
            public void SetVSync(bool v) { }
            public void SetShadows(bool v) { }
            public void SetAnisotropicFiltering(bool v) { }
            public void SetLodBias(float v) { }
            public void ResetToDefaults() { }
            public void ApplyCurrentSettings() { }
        }

        private static readonly int WeightId =
            Shader.PropertyToID("_MoyvaFarViewWeight");

        private FarViewAtmosphereDriver _driver;
        private float _weightBefore;

        [SetUp]
        public void SetUp()
        {
            _weightBefore = Shader.GetGlobalFloat(WeightId);
        }

        [TearDown]
        public void TearDown()
        {
            _driver?.Dispose();
            _driver = null;
            Shader.SetGlobalFloat(WeightId, _weightBefore);
        }

        [Test]
        public void Weight_FollowsZoom_AndClamps()
        {
            var zoom = new StubZoom { FarViewWeight = 0.6f };
            _driver = new FarViewAtmosphereDriver(zoom, new FarViewAtmosphereConfig());
            _driver.Initialize();
            Assert.AreEqual(0.6f, Shader.GetGlobalFloat(WeightId), 0.0001f);

            zoom.FarViewWeight = 3f;
            _driver.Tick();
            Assert.AreEqual(1f, Shader.GetGlobalFloat(WeightId), 0.0001f,
                "weight clamps at 1");
            zoom.FarViewWeight = -2f;
            _driver.Tick();
            Assert.AreEqual(0f, Shader.GetGlobalFloat(WeightId), 0.0001f,
                "weight clamps at 0");
        }

        [Test]
        public void Tick_PushesWeight_OnChange_Only()
        {
            var zoom = new StubZoom { FarViewWeight = 0.4f };
            _driver = new FarViewAtmosphereDriver(zoom, new FarViewAtmosphereConfig());
            _driver.Initialize();
            Assert.AreEqual(0.4f, Shader.GetGlobalFloat(WeightId), 0.001f,
                "initialize pushes the current zoom weight");

            zoom.FarViewWeight = 0.9f;
            _driver.Tick();
            Assert.AreEqual(0.9f, Shader.GetGlobalFloat(WeightId), 0.001f,
                "a real zoom change pushes the new weight");
        }

        [Test]
        public void Dispose_ZeroesWeight_SceneResetLeavesNoResidue()
        {
            var zoom = new StubZoom { FarViewWeight = 0.8f };
            _driver = new FarViewAtmosphereDriver(zoom, new FarViewAtmosphereConfig());
            _driver.Initialize();
            Assert.AreEqual(0.8f, Shader.GetGlobalFloat(WeightId), 0.001f);

            _driver.Dispose();
            Assert.AreEqual(0f, Shader.GetGlobalFloat(WeightId),
                "dispose must reset the global weight for scene teardown");

            // After disposal Tick is inert — no resurrected pushes.
            zoom.FarViewWeight = 1f;
            _driver.Tick();
            Assert.AreEqual(0f, Shader.GetGlobalFloat(WeightId));
            _driver = null;
        }

        [Test]
        public void DisabledConfig_OrMissingZoom_WeightsZero()
        {
            var zoom = new StubZoom { FarViewWeight = 0.9f };
            var cfg = new FarViewAtmosphereConfig { enabled = false };
            _driver = new FarViewAtmosphereDriver(zoom, cfg);
            _driver.Initialize();
            Assert.AreEqual(0f, Shader.GetGlobalFloat(WeightId),
                "disabled config → zero weight");
            _driver.Dispose();
            _driver = null;

            _driver = new FarViewAtmosphereDriver(null, new FarViewAtmosphereConfig());
            _driver.Initialize();
            Assert.AreEqual(0f, Shader.GetGlobalFloat(WeightId),
                "no zoom state → zero weight");
        }

        [Test]
        public void QualityTier_LowProfile_EnablesKeyword_AndChangeApplies()
        {
            var graphics = new StubGraphicsSettings();
            _driver = new FarViewAtmosphereDriver(
                new StubZoom(), new FarViewAtmosphereConfig(), graphics);
            graphics.SetProfile(GraphicsQualityProfile.Performance);
            _driver.Initialize();
            Assert.IsTrue(Shader.IsKeywordEnabled(
                FarViewAtmosphereDriver.LowQualityKeyword),
                "Performance profile must enable the low-quality keyword");

            graphics.SetProfile(GraphicsQualityProfile.Quality);
            Assert.IsFalse(Shader.IsKeywordEnabled(
                FarViewAtmosphereDriver.LowQualityKeyword),
                "settings change must re-apply the tier");
        }
    }
}
