using Zenject;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Units.API;

namespace Kruty1918.Moyva.Bootstrap
{
    public class BootstrapInstaller : MonoInstaller
    {
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        [SerializeField] private BootstrapInstallerConfigSO _config;

        // Fallback для старих сцен, де налаштування були інлайн у Installer.
        [SerializeField, HideInInspector] private BootstrapGameSettings _legacyGameSettings = new();
        [SerializeField, HideInInspector] private StartingPositionInitializerSettings _legacyStartingPositionSettings = new();

        public override void InstallBindings()
        {
            Debug.Log($"{WorldGenDiagTag} BootstrapInstaller.InstallBindings scene={gameObject.scene.name}, mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}.");
            Debug.Log($"{DirectDiagTag} BootstrapInstaller.InstallBindings scene={gameObject.scene.name}, mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}.");
            var gameSettings = _config != null ? _config.GameSettings : _legacyGameSettings;
            var startingPositionSettings = _config != null ? _config.StartingPositionSettings : _legacyStartingPositionSettings;

            if (_config == null)
                Debug.LogWarning("[Bootstrap] BootstrapInstallerConfigSO не призначено. Використано legacy inline settings із BootstrapInstaller.");
            else
                Debug.Log($"[Bootstrap] Використано BootstrapInstallerConfigSO '{_config.name}'.");

            // Спільний стан стартової позиції (читається BootstrapGameInitializer після того,
            // як StartingPositionInitializer запише значення при обробці WorldGeneratedDataSignal).
            Container.BindInterfacesAndSelfTo<BootstrapStartingPositionState>().AsSingle();
            Container.Bind<BootstrapStarterPackState>().AsSingle();

            // Гра-bootstrap готує owner-контекст і видає стартові ресурси на старті нового світу.
            Container.BindInstance(gameSettings).AsSingle();
            Container.Bind<IBootstrapOwnerIdResolver>().To<BootstrapOwnerIdResolver>().AsSingle();
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

            // Модуль збереження юнітів — реєструється як ISaveModule, автоматично
            // потрапляє в List<ISaveModule> при ініціалізації SaveService.
            Container.BindInterfacesAndSelfTo<UnitsSaveModule>()
                .AsSingle();

            Container.BindInterfacesTo<SaveModuleRegistrar<UnitsSaveModule>>()
                .AsSingle()
                .NonLazy();

            // Автозбереження при виході з програми.
            Container.BindInterfacesTo<GameExitSaver>()
                .AsSingle()
                .NonLazy();

            // Ініціалізатор запуску: перевіряє наявність сейву і завантажує його.
            // Має ініціалізуватись після усіх сервісів.
            Container.BindInterfacesTo<DirectGameplayLaunchModeInitializer>().AsSingle().NonLazy();
            Container.BindExecutionOrder<DirectGameplayLaunchModeInitializer>(90);

            Container.BindInterfacesTo<TestUnitSpawner>().AsSingle().NonLazy();
            Container.BindExecutionOrder<TestUnitSpawner>(100);

            // Розкриває туман навколо стартової позиції і телепортує камеру туди.
            // Виконується після TestUnitSpawner, щоб знати чи є збереження.
            Container.BindInstance(startingPositionSettings).AsSingle();
            BindStartingPositionServices();
            Debug.Log($"{DirectDiagTag} BootstrapInstaller bound StartingPositionInitializer NonLazy=true.");
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
            Debug.Log($"{DirectDiagTag} BootstrapInstaller bound StartingPositionSpawnSetupService.");

            Container.BindInterfacesAndSelfTo<StartingPositionRevealPresentationService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionAutoloadRecoveryService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StartingPositionWorkflowState>()
                .AsSingle();
            Debug.Log($"{DirectDiagTag} BootstrapInstaller bound BootstrapStartingPositionState.");

            Container.BindInterfacesAndSelfTo<StartingPositionWorkflowService>()
                .AsSingle();
            Debug.Log($"{DirectDiagTag} BootstrapInstaller bound StartingPositionWorkflowService lifetime=AsSingle rootCreated=false.");
            Debug.Log($"{DirectDiagTag} BootstrapInstaller bound StartingPositionFogRevealService.");
        }

        private static T TryResolveOptional<T>(DiContainer container)
            where T : class
        {
            return container.HasBinding(typeof(T))
                ? container.Resolve<T>()
                : null;
        }
    }

    internal sealed class DirectGameplayLaunchModeInitializer : IInitializable
    {
        private const string PolicyDiagTag = "[MoyvaStartPolicyDiag]";
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";

        public void Initialize()
        {
            bool expiredBeforeInitialize = GameLaunchContext.IsExpired;
            GameLaunchContext.EnsureNotExpired();
            Debug.Log($"{WorldGenDiagTag} DirectLaunch.Initialize BEFORE mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, saveSlot={GameLaunchContext.SaveSlot}.");
            Debug.Log($"{DirectDiagTag} DirectLaunch.Initialize BEFORE mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, saveSlot={GameLaunchContext.SaveSlot}.");

            Debug.Log(
                $"{PolicyDiagTag} DirectLaunch.Initialize before mode={GameLaunchContext.Mode}, expiredBefore={expiredBeforeInitialize}, " +
                $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, " +
                $"autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, slot={GameLaunchContext.SaveSlot}.");

            if (GameLaunchContext.Mode != GameLaunchMode.Unknown)
            {
                Debug.Log($"{WorldGenDiagTag} DirectLaunch.Initialize SKIP reason=mode-not-unknown mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}.");
                Debug.Log($"{DirectDiagTag} DirectLaunch.Initialize skip reason=mode-not-unknown currentMode={GameLaunchContext.Mode}.");
                Debug.Log(
                    $"{PolicyDiagTag} DirectLaunch.Initialize skip mode={GameLaunchContext.Mode}, " +
                    $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}.");
                Debug.Log($"{DirectDiagTag} DirectLaunch.Initialize AFTER mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, saveSlot={GameLaunchContext.SaveSlot}.");
                return;
            }

#if UNITY_EDITOR
            Debug.Log($"{DirectDiagTag} DirectLaunch.Initialize APPLY direct fallback reason=editor-direct-gameplay-mode-unknown.");
            GameLaunchContext.ConfigureDirectGameplayTest();
            Debug.Log($"{WorldGenDiagTag} DirectLaunch.Initialize CONFIGURED mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, saveSlot={GameLaunchContext.SaveSlot}.");
            Debug.Log(
                $"{PolicyDiagTag} DirectLaunch.Initialize configured mode={GameLaunchContext.Mode}, " +
                $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, " +
                $"autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, slot={GameLaunchContext.SaveSlot}.");
            Debug.Log("[Bootstrap] Direct gameplay start detected -> solo/no-save test mode enabled.");
#endif
            Debug.Log($"{WorldGenDiagTag} DirectLaunch.Initialize AFTER mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, saveSlot={GameLaunchContext.SaveSlot}.");
            Debug.Log($"{DirectDiagTag} DirectLaunch.Initialize AFTER mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, saveSlot={GameLaunchContext.SaveSlot}.");
        }
    }

    internal sealed class InitialWorldSaveService : IInitializable, System.IDisposable
    {
        private readonly ISaveService _saveService;
        private readonly SignalBus _signalBus;
        private bool _saved;

        public InitialWorldSaveService(ISaveService saveService, SignalBus signalBus)
        {
            _saveService = saveService;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<Kruty1918.Moyva.Signals.WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<Kruty1918.Moyva.Signals.WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        private void OnWorldGenerated(Kruty1918.Moyva.Signals.WorldGeneratedDataSignal signal)
        {
            if (_saved || GameLaunchContext.Mode != GameLaunchMode.MenuNewGame || !GameLaunchContext.IsAutoSaveEnabled())
                return;

            _saved = true;
            int slot = GameLaunchContext.SaveSlot;
            Debug.Log($"[Bootstrap] Новий світ створено — первинне збереження у слот {slot:D2}.");
            _saveService.Save(slot);
        }
    }
}
