using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator
{
    internal sealed class GeneratorWorldStartupBuilder : IInitializable
    {
        private readonly MapVisualInstantiator _mapVisualInstantiator;
        private readonly IWorldGenerationDiagnostics _worldDiagnostics;
        private bool _buildTriggered;

        public GeneratorWorldStartupBuilder(
            MapVisualInstantiator mapVisualInstantiator,
            [InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null)
        {
            _mapVisualInstantiator = mapVisualInstantiator;
            _worldDiagnostics = worldDiagnostics;
        }

        public void Initialize()
        {
            AttemptStartup("IInitializable");
        }

        public void EnsureStartedFromFallback()
        {
            AttemptStartup("GeneratorInstaller.Start");
        }

        private void AttemptStartup(string trigger)
        {
            if (_buildTriggered)
                return;

            _worldDiagnostics?.GeneratorStartupInitialized(
                $"source={trigger}, frame={Time.frameCount}, " +
                $"mode={GameLaunchContext.Mode}");

            bool hasInstantiator = _mapVisualInstantiator != null;
            bool hasCurrentWorld = hasInstantiator
                && _mapVisualInstantiator.TryGetCurrentWorldData(out _);
            bool hasPendingWorld = hasInstantiator
                && _mapVisualInstantiator.HasPendingWorldData;
            bool shouldBuild = ShouldBuildWorldOnStartup(
                hasInstantiator,
                hasCurrentWorld,
                out string reason);
            string graphName = hasInstantiator
                ? _mapVisualInstantiator.DiagnosticGraphName
                : "null";
            string generatorType = hasInstantiator
                ? _mapVisualInstantiator.DiagnosticMapDataGeneratorTypeName
                : "null";

            Debug.Log(
                $"[GeneratorStartup] trigger={trigger} mode={GameLaunchContext.Mode} " +
                $"shouldBuild={shouldBuild} reason={reason} " +
                $"current={hasCurrentWorld} pending={hasPendingWorld} " +
                $"graph={graphName} generator={generatorType}");

            if (!shouldBuild)
            {
                if (GameLaunchContext.Mode == GameLaunchMode.Unknown)
                {
                    Debug.LogError(
                        "[GeneratorStartup] World build rejected because launch " +
                        $"mode is unknown. source={GameLaunchContext.Source}");
                }

                _worldDiagnostics?.ReportStartup();
                return;
            }

            _buildTriggered = true;
            string buildSource = hasPendingWorld
                ? "pending-save"
                : GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest
                    ? "direct-test"
                    : "new";
            _worldDiagnostics?.MapVisualBuildWorldCalled(
                $"source={buildSource}, caller=GeneratorStartup");
            _mapVisualInstantiator.BuildWorld();
        }

        private static bool ShouldBuildWorldOnStartup(
            bool hasInstantiator,
            bool hasCurrentWorld,
            out string reason)
        {
            if (!hasInstantiator)
            {
                reason = "no-map-visual-instantiator";
                return false;
            }
            if (hasCurrentWorld)
            {
                reason = "world-already-present";
                return false;
            }

            GameLaunchContext.EnsureNotExpired();
            switch (GameLaunchContext.Mode)
            {
                case GameLaunchMode.DirectGameplayTest:
                    reason = "mode-direct-gameplay-test";
                    return true;
                case GameLaunchMode.MenuNewGame:
                    reason = "mode-menu-new-game";
                    return true;
                case GameLaunchMode.MenuLoadGame:
                    reason = "mode-menu-load-game";
                    return true;
                case GameLaunchMode.MenuMultiplayerGame:
                    reason = "mode-menu-multiplayer-game";
                    return true;
                default:
                    reason = $"mode-not-supported:{GameLaunchContext.Mode}";
                    return false;
            }
        }
    }
}
