using Kruty1918.Localization;
using Kruty1918.Moyva.Shared.Common;
using Kruty1918.Connectivity;
using Kruty1918.Diagnostics;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Shared.Performance;
using Kruty1918.Performance;
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

            container.Bind<ConnectivityService>()
                .AsSingle()
                .OnInstantiated<ConnectivityService>((_, service) => service.Initialize());

            container.Bind<IConnectivityService>()
                .To<ConnectivityService>()
                .FromResolve();

            container.Bind<IDisposable>()
                .To<ConnectivityService>()
                .FromResolve();

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

            container.Bind<FrameBudgetSettings>()
                .FromMethod(_ => AdaptivePerformanceDefaultsProvider.LoadFrameBudget())
                .AsSingle();
            container.Bind<FrameBudgetMonitorService>()
                .AsSingle()
                .NonLazy();
            container.Bind<IFrameBudgetMonitorService>()
                .To<FrameBudgetMonitorService>()
                .FromResolve();
            container.Bind<ITickable>()
                .To<FrameBudgetMonitorTickable>()
                .AsSingle()
                .NonLazy();

            container.Bind<PrewarmSettings>()
                .FromMethod(_ => AdaptivePerformanceDefaultsProvider.LoadPrewarmSettings())
                .AsSingle();
            container.Bind<StartupPrewarmService>()
                .AsSingle();

            container.Bind<IStartupPrewarmService>()
                .To<StartupPrewarmService>()
                .FromResolve();

            container.Bind<AsyncGlobalErrorHandlerService>()
                .AsSingle()
                .NonLazy()
                .OnInstantiated<AsyncGlobalErrorHandlerService>((_, service) => service.Initialize());
            container.Bind<IDisposable>()
                .To<AsyncGlobalErrorHandlerService>()
                .FromResolve();

            container.BindInterfacesTo<UiMotionService>().AsSingle();
            container.BindInterfacesTo<UiTooltipService>().AsSingle();
            container.BindInterfacesTo<SceneTransitionService>().AsSingle();

            // Localization: project-scope singletons so language state survives scene changes.
            container.Bind<LocalizationOptions>()
                .FromMethod(_ => MoyvaLocalizationDefaults.CreateOptions())
                .AsSingle();
            container.Bind<LocalizationFontOptions>()
                .FromMethod(_ => MoyvaLocalizationDefaults.CreateFontOptions())
                .AsSingle();
            container.BindInterfacesAndSelfTo<LocalizationService>().AsSingle().NonLazy();
            container.Bind<LocalizationFontService>().AsSingle();

            container.BindInterfacesAndSelfTo<InternetConnectivityHealthReporter>().AsSingle();
            container.Bind<HealthCheckService>()
                .AsSingle()
                .NonLazy()
                .OnInstantiated<HealthCheckService>((_, service) => service.Initialize());
            container.Bind<IHealthCheckService>().To<HealthCheckService>().FromResolve();
        }

        private sealed class FrameBudgetMonitorTickable : ITickable
        {
            private readonly FrameBudgetMonitorService _monitor;

            public FrameBudgetMonitorTickable(FrameBudgetMonitorService monitor)
            {
                _monitor = monitor;
            }

            public void Tick() => _monitor.Tick();
        }
    }
}
