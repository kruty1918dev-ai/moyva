using Zenject;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Multiplayer.Runtime;

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
            var gameSettings = _config != null ? _config.GameSettings : _legacyGameSettings;
            var startingPositionSettings = _config != null ? _config.StartingPositionSettings : _legacyStartingPositionSettings;

            if (_config == null)
                Debug.LogWarning("[Bootstrap] BootstrapInstallerConfigSO не призначено. Використано legacy inline settings із BootstrapInstaller.");

            // Спільний стан стартової позиції (читається BootstrapGameInitializer після того,
            // як StartingPositionInitializer запише значення при обробці WorldGeneratedDataSignal).
            Container.Bind<BootstrapStartingPositionState>().AsSingle();

            // Гра-bootstrap робить дефолтну будівлю та видає стартові ресурси
            Container.BindInstance(gameSettings).AsSingle();
            Container.BindInterfacesTo<BootstrapGameInitializer>().AsSingle().NonLazy();
            Container.BindExecutionOrder<BootstrapGameInitializer>(102); // після StartingPositionInitializer (101)

            // Модуль збереження юнітів — реєструється як ISaveModule, автоматично
            // потрапляє в List<ISaveModule> при ініціалізації SaveService.
            Container.BindInterfacesAndSelfTo<UnitsSaveModule>()
                .AsSingle();

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
            Container.BindInterfacesTo<StartingPositionSyncService>().AsSingle().NonLazy();
            Container.BindExecutionOrder<StartingPositionSyncService>(100);
            Container.BindInterfacesTo<StartingPositionInitializer>().AsSingle().NonLazy();
            Container.BindExecutionOrder<StartingPositionInitializer>(101);

            Container.BindInterfacesTo<InitialWorldSaveService>().AsSingle().NonLazy();
            Container.BindExecutionOrder<InitialWorldSaveService>(103);
        }
    }

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
