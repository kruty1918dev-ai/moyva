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
                return;

            bool hasInstantiator = _mapVisualInstantiator != null;
            bool hasCurrentWorld = hasInstantiator
                && _mapVisualInstantiator.TryGetCurrentWorldData(out _);
            bool shouldBuild = ShouldBuildWorldOnStartup(
                hasInstantiator,
                hasCurrentWorld);

            if (!shouldBuild)
            {
                if (GameLaunchContext.Mode == GameLaunchMode.Unknown)
                {
                    Debug.LogError(
                        "[GeneratorStartup] World build rejected because launch " +
                        $"mode is unknown. source={GameLaunchContext.Source}");
                }

                return;
            }

            _buildTriggered = true;
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
