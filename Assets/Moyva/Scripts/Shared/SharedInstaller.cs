using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Shared.Connectivity;

namespace Kruty1918.Moyva.Shared
{
    /// <summary>
    /// Installer for shared services (Connectivity, etc.).
    /// </summary>
    public sealed class SharedInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IConnectivityService>()
                .To<ConnectivityService>()
                .AsSingle();
        }

        // Helper for programmatic installation from other installers
        public static void Install(DiContainer container)
        {
            container.Bind<IConnectivityService>()
                .To<ConnectivityService>()
                .AsSingle();
        }
    }
}
