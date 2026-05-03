using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class DirectGameplayLaunchModeInitializer : IInitializable
    {
        public void Initialize()
        {
            if (GameLaunchContext.Mode != GameLaunchMode.Unknown)
                return;

#if UNITY_EDITOR
            GameLaunchContext.ConfigureDirectGameplayTest();
            Debug.Log("[Bootstrap] Direct gameplay start detected -> solo/no-save test mode enabled.");
#endif
        }
    }
}