using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.HomeMenu.Runtime.Startup;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Shared.Performance;
using Kruty1918.Moyva.WorldCreation.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    // P082: the loading overlay must be visible before synchronous init stages
    // (graphics profile, prewarm) run — a single Task.Yield resumes before the
    // render pass and is not a first-frame guarantee.
    public sealed class GameplayStartupFirstFrameTests
    {
        [SetUp]
        public void SetUp() => GameLaunchContext.Reset();

        [Test]
        public async Task RunAsync_ShowsOverlayBeforeSynchronousInitStages()
        {
            var order = new List<string>();
            var overlay = new RecordingOverlay(order);
            var graphics = new RecordingGraphics(order);
            var prewarm = new RecordingPrewarm(order);
<<<<<<< Updated upstream
            var config = new HomeMenuConfigSO { gameplaySceneName = "__p082_missing_scene__" };
=======
            var config = ScriptableObject.CreateInstance<HomeMenuConfigSO>();
            config.gameplaySceneName = "__p082_missing_scene__";
>>>>>>> Stashed changes

            var pipeline = new GameplayStartupPipeline(
                config,
                overlay,
                new FakeSession(),
                graphicsSettingsService: graphics,
                startupPrewarmService: prewarm);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                await pipeline.RunAsync(cts.Token);
            }
            catch (Exception)
            {
                // EditMode has no scene to load — the ordering is already recorded.
            }

            int overlayShown = order.IndexOf("overlay:load");
            Assert.That(overlayShown, Is.GreaterThanOrEqualTo(0),
                "the loading overlay was never shown");
            Assert.That(order.IndexOf("graphics:setProfile"), Is.GreaterThan(overlayShown),
                "graphics profile applied before the loading UI was shown");
            Assert.That(order.IndexOf("prewarm"), Is.GreaterThan(overlayShown),
                "prewarm ran before the loading UI was shown");
            Assert.That(order.IndexOf("overlay:lock"), Is.LessThan(overlayShown));
        }

        private sealed class RecordingOverlay : IOverlayLoader
        {
            private readonly List<string> _order;
            public RecordingOverlay(List<string> order) => _order = order;
            public OverlayLoaderResult LoadOverlay(float value, float maxValue = 100, string sufix = "%")
            {
                _order.Add("overlay:load");
                return null;
            }
            public void UpdateOverlay(float value, float maxValue = 100, string sufix = "%") { }
            public void SetOverlayStatus(string status) { }
            public void StopOverlay(bool forceImmediate = false) { }
            public void LockOverlay() => _order.Add("overlay:lock");
            public void UnlockOverlay() { }
        }

        private sealed class RecordingGraphics : IGraphicsSettingsService
        {
            private readonly List<string> _order;
            public RecordingGraphics(List<string> order) => _order = order;
            // Non-Auto, non-Custom so the gameplay policy actually applies a change.
            public GraphicsSettingsData Settings { get; } =
                new GraphicsSettingsData { Profile = GraphicsQualityProfile.Performance };
            public event Action<GraphicsSettingsData> OnSettingsChanged { add { } remove { } }
            public void SetProfile(GraphicsQualityProfile profile) => _order.Add("graphics:setProfile");
            public void SetTargetFrameRate(int frameRate) { }
            public void SetRenderScale(float renderScale) { }
            public void SetDynamicRenderScale(bool enabled) { }
            public void SetCloseZoomOptimization(bool enabled) { }
            public void SetTextureMipmapLimit(int mipmapLimit) { }
            public void SetAntiAliasing(int antiAliasing) { }
            public void SetVSync(bool enabled) { }
            public void SetShadows(bool enabled) { }
            public void SetAnisotropicFiltering(bool enabled) { }
            public void SetLodBias(float lodBias) { }
            public void ResetToDefaults() { }
            public void ApplyCurrentSettings() { }
        }

        private sealed class RecordingPrewarm : IStartupPrewarmService
        {
            private readonly List<string> _order;
            public RecordingPrewarm(List<string> order) => _order = order;
            public Task PrewarmAsync(CancellationToken ct = default)
            {
                _order.Add("prewarm");
                return Task.CompletedTask;
            }
        }

        private sealed class FakeSession : IGameplaySession
        {
            public bool IsHost => true;
            public NetworkProviderType Mode => NetworkProviderType.Offline;
            public WorldSettingsDto WorldSettings => default;
            public IReadOnlyList<GameplayPlayer> Players => Array.Empty<GameplayPlayer>();
            public GameplayPlayer LocalPlayer => default;
            public GameplayPlayer Host => default;
            public void Apply(NetworkProviderType mode, WorldSettingsDto worldSettings,
                IReadOnlyList<GameplayPlayer> players, string localPlayerId) { }
            public void Clear() { }
        }
    }
}
