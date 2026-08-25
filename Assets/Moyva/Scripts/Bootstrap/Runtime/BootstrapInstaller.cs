using Zenject;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.UIActions.Runtime;
using Kruty1918.Moyva.Units.API;

namespace Kruty1918.Moyva.Bootstrap
{
    public class BootstrapInstaller : MonoInstaller
    {
        [SerializeField] private BootstrapInstallerConfigSO _config;

        // Fallback для старих сцен, де налаштування були інлайн у Installer.
        [SerializeField, HideInInspector] private BootstrapGameSettings _legacyGameSettings = new();
        [SerializeField, HideInInspector] private StartingPositionInitializerSettings _legacyStartingPositionSettings = new();

        public override void InstallBindings()
        {
            UiActionsInstaller.Install(Container);
            var gameSettings = _config != null ? _config.GameSettings : _legacyGameSettings;
            var startingPositionSettings = _config != null ? _config.StartingPositionSettings : _legacyStartingPositionSettings;

            // Спільний стан стартової позиції (читається BootstrapGameInitializer після того,
            // як StartingPositionInitializer запише значення при обробці WorldGeneratedDataSignal).
            Container.BindInterfacesAndSelfTo<BootstrapStartingPositionState>().AsSingle();
            Container.Bind<BootstrapStarterPackState>().AsSingle();

            // Гра-bootstrap готує owner-контекст і видає стартові ресурси на старті нового світу.
            Container.BindInstance(gameSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<BootstrapOwnerIdResolver>().AsSingle();
            Container.Bind<IBootstrapStarterPackDecisionService>().To<BootstrapStarterPackDecisionService>().AsSingle();
            Container.Bind<IBootstrapStarterPackPersistenceService>().To<BootstrapStarterPackPersistenceService>().AsSingle();
            Container.Bind<IBootstrapStarterPackGrantService>().To<BootstrapStarterPackGrantService>().AsSingle();
            Container.BindInterfacesTo<BootstrapGameInitializer>().AsSingle().NonLazy();
            Container.BindExecutionOrder<BootstrapGameInitializer>(102); // після StartingPositionInitializer (101)

            Container.BindInterfacesAndSelfTo<BootstrapStarterPackSaveModule>()
                .AsSingle();

            Container.BindInterfacesTo<SaveModuleRegistrar<BootstrapStarterPackSaveModule>>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<TurnSaveModule>().AsSingle();
            Container.BindInterfacesTo<SaveModuleRegistrar<TurnSaveModule>>()
                .AsSingle()
                .NonLazy();

            // Автозбереження при виході з програми.
            Container.BindInterfacesTo<GameExitSaver>()
                .AsSingle()
                .NonLazy();

            // Ініціалізатор запуску: перевіряє наявність сейву і завантажує його.
            // Має ініціалізуватись після усіх сервісів.
            Container.BindInterfacesTo<DirectGameplayLaunchModeInitializer>()
                .AsSingle()
                .NonLazy();
            Container.BindExecutionOrder<DirectGameplayLaunchModeInitializer>(90);

            Container.BindInterfacesTo<TestUnitSpawner>().AsSingle().NonLazy();
            Container.BindExecutionOrder<TestUnitSpawner>(100);

            GameplayHudBindings.Install(Container);

            // Розкриває туман навколо стартової позиції і телепортує камеру туди.
            // Виконується після TestUnitSpawner, щоб знати чи є збереження.
            Container.BindInstance(startingPositionSettings).AsSingle();
            BindStartingPositionServices();
            Container.BindInterfacesTo<StartingPositionInitializer>().AsSingle().NonLazy();
            Container.BindExecutionOrder<StartingPositionInitializer>(101);

            Container.BindInterfacesTo<InitialWorldSaveService>().AsSingle().NonLazy();
            Container.BindExecutionOrder<InitialWorldSaveService>(103);

            // Плавний reveal світу (чорний overlay -> прозорий) після готовності карти.
            Container.BindInterfacesTo<WorldLoadRevealOverlayService>().AsSingle().NonLazy();
            Container.BindExecutionOrder<WorldLoadRevealOverlayService>(104);
        }

        private void BindStartingPositionServices()
        {
            Container.BindInterfacesAndSelfTo<StartingPositionSelector>()
                .FromMethod(ctx => new StartingPositionSelector(
                    ctx.Container.Resolve<StartingPositionInitializerSettings>(),
                    TryResolveOptional<IPathfinder>(ctx.Container)))
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionAssignmentFactory>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionPolicy>()
                .FromMethod(ctx => new StartingPositionPolicy(
                    ctx.Container.Resolve<StartingPositionInitializerSettings>(),
                    TryResolveOptional<ISessionManager>(ctx.Container),
                    ctx.Container.Resolve<IStartingPositionState>()))
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionLocalSpawnResolver>()
                .FromMethod(ctx => new StartingPositionLocalSpawnResolver(
                    TryResolveOptional<ISessionManager>(ctx.Container),
                    ctx.Container.Resolve<IStartingPositionState>()))
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionFogRevealService>()
                .FromMethod(ctx => new StartingPositionFogRevealService(
                    ctx.Container.Resolve<IFogOfWarService>(),
                    TryResolveOptional<IFogVisualUpdater>(ctx.Container),
                    ctx.Container.Resolve<StartingPositionInitializerSettings>(),
                    StartingPositionInitializer.StartVisionAnchorId,
                    StartingPositionInitializer.StartRevealAnchorId,
                    StartingPositionInitializer.DebugTag))
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionLoadedFogRepairService>()
                .FromMethod(ctx => new StartingPositionLoadedFogRepairService(
                    ctx.Container.Resolve<IFogOfWarService>(),
                    ctx.Container.Resolve<StartingPositionInitializerSettings>(),
                    ctx.Container.Resolve<IStartingPositionFogRevealService>(),
                    StartingPositionInitializer.StartRevealAnchorId,
                    StartingPositionInitializer.DebugTag))
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionCameraService>()
                .FromMethod(ctx => new StartingPositionCameraService(
                    ctx.Container.Resolve<ICameraMovement>(),
                    TryResolveOptional<ICameraZoom>(ctx.Container),
                    TryResolveOptional<IGridProjection>(ctx.Container),
                    TryResolveOptional<UnityEngine.Camera>(ctx.Container),
                    TryResolveOptional<CameraSettingsSO>(ctx.Container),
                    TryResolveOptional<MoyvaProjectSettingsSO>(ctx.Container),
                    ctx.Container.Resolve<StartingPositionInitializerSettings>()))
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionCameraTargetResolver>()
                .FromMethod(ctx => new StartingPositionCameraTargetResolver(
                    ctx.Container.Resolve<IFogOfWarService>(),
                    TryResolveOptional<IUnitService>(ctx.Container),
                    ctx.Container.Resolve<IStartingPositionState>()))
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionSpawnSetupService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionRevealPresentationService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionAutoloadRecoveryService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionWorkflowState>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionWorkflowService>()
                .AsSingle();
        }

        private static T TryResolveOptional<T>(DiContainer container)
            where T : class
        {
            return container.HasBinding(typeof(T))
                ? container.Resolve<T>()
                : null;
        }

    }

}
