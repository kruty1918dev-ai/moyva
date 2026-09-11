using Kruty1918.Moyva.Shared.Common;
using Kruty1918.Moyva.Shared.Connectivity;
using Kruty1918.Moyva.Shared.Diagnostics;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Shared.Performance;
using Kruty1918.Moyva.Shared.Controls;
using Kruty1918.Moyva.Shared.UI;
using Zenject;

namespace Kruty1918.Moyva.Shared
{
    /// <summary>
    /// Installer for shared services (Connectivity, etc.).
    /// </summary>
    public sealed class SharedInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // All bindings are configured via the static Install(container) method,
            // which is called from ProjectServicesInstaller to avoid duplicate registrations.
        }

        // Helper for programmatic installation from other installers
        public static void Install(DiContainer container)
        {
            if (!container.HasBinding<IClientInstanceScope>())
            {
                container.Bind<IClientInstanceScope>()
                    .FromInstance(ClientInstanceScope.Default)
                    .AsSingle();
            }

            container.Bind<IConnectivityService>()
                .To<ConnectivityService>()
                .AsSingle();

            container.Bind<IServiceModeProfileProvider>()
                .To<ServiceModeProfileProvider>()
                .AsSingle();

            container.BindInterfacesAndSelfTo<GraphicsSettingsService>()
                .AsSingle()
                .NonLazy();

            container.BindInterfacesAndSelfTo<InputDeviceContext>().AsSingle();

            container.BindInterfacesAndSelfTo<PlayerControlSettingsService>()
                .AsSingle()
                .NonLazy();

            container.Bind<IFrameBudgetMonitorService>()
                .To<FrameBudgetMonitorService>()
                .AsSingle()
                .NonLazy();

            container.Bind<StartupPrewarmService>()
                .AsSingle();

            container.Bind<IStartupPrewarmService>()
                .To<StartupPrewarmService>()
                .FromResolve();

            container.BindInterfacesTo<AsyncGlobalErrorHandlerService>().AsSingle().NonLazy();

            container.BindInterfacesTo<UiMotionService>().AsSingle();
            container.BindInterfacesTo<UiTooltipService>().AsSingle();
            container.BindInterfacesTo<SceneTransitionService>().AsSingle();

            container.BindInterfacesAndSelfTo<InternetConnectivityHealthReporter>().AsSingle();
            container.Bind<IHealthCheckService>().To<HealthCheckService>().AsSingle().NonLazy();
        }
    }
}
