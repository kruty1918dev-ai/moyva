using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator
{
    internal sealed class GeneratorWorldStartupBuilder : IInitializable
    {
        private readonly MapVisualInstantiator _mapVisualInstantiator;
        private bool _buildTriggered;

        public GeneratorWorldStartupBuilder(MapVisualInstantiator mapVisualInstantiator)
        {
            _mapVisualInstantiator = mapVisualInstantiator;
        }

        public void Initialize()
        {
            AttemptStartup();
        }

        public void EnsureStartedFromFallback()
        {
            AttemptStartup();
        }

        private void AttemptStartup()
        {
            if (_buildTriggered)
            {
                Debug.Log("[GeneratorStartup] Attempt ignored because build was already triggered.");
                return;
            }

            bool hasInstantiator = _mapVisualInstantiator != null;
            bool hasCurrentWorld = hasInstantiator
                && _mapVisualInstantiator.TryGetCurrentWorldData(out _);
            bool shouldBuild = ShouldBuildWorldOnStartup(
                hasInstantiator,
                hasCurrentWorld);

            if (!shouldBuild)
            {
                Debug.LogWarning(
                    "[GeneratorStartup] World build skipped. " +
                    $"hasInstantiator={hasInstantiator}, hasCurrentWorld={hasCurrentWorld}, " +
                    $"mode={GameLaunchContext.Mode}, source={GameLaunchContext.Source}, " +
                    $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, world='{GameLaunchContext.WorldName}', " +
                    $"seed={GameLaunchContext.Seed}, size={GameLaunchContext.Size}, " +
                    $"dimensions={GameLaunchContext.Width}x{GameLaunchContext.Height}");

                if (GameLaunchContext.Mode == GameLaunchMode.Unknown)
                {
                    Debug.LogError(
                        "[GeneratorStartup] World build rejected because launch " +
                        $"mode is unknown. source={GameLaunchContext.Source}");
                }

                return;
            }

            _buildTriggered = true;
            Debug.Log(
                "[GeneratorStartup] Building world. " +
                $"mode={GameLaunchContext.Mode}, source={GameLaunchContext.Source}, " +
                $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, world='{GameLaunchContext.WorldName}', " +
                $"seed={GameLaunchContext.Seed}, size={GameLaunchContext.Size}, " +
                $"dimensions={GameLaunchContext.Width}x{GameLaunchContext.Height}");
            _mapVisualInstantiator.BuildWorld();
        }

        private static bool ShouldBuildWorldOnStartup(
            bool hasInstantiator,
            bool hasCurrentWorld)
        {
            if (!hasInstantiator)
                return false;
            if (hasCurrentWorld)
                return false;

            GameLaunchContext.EnsureNotExpired();
            switch (GameLaunchContext.Mode)
            {
                case GameLaunchMode.DirectGameplayTest:
                case GameLaunchMode.MenuNewGame:
                case GameLaunchMode.MenuLoadGame:
                case GameLaunchMode.MenuMultiplayerGame:
                    return true;
                default:
                    return false;
            }
        }
    }
}
