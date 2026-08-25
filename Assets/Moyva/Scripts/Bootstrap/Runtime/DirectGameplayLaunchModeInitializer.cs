using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap
{
    internal sealed class DirectGameplayLaunchModeInitializer : IInitializable
    {
        public void Initialize()
        {
            GameLaunchContext.EnsureNotExpired();

            if (GameLaunchContext.Mode == GameLaunchMode.Unknown)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLaunchContext.ConfigureDirectGameplayTest();
#endif
            }

            if (GameLaunchContext.Mode == GameLaunchMode.Unknown)
                Debug.LogError("Gameplay launch context is missing.");
        }
    }
}
