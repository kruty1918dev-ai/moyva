using Zenject;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Kruty1918.Moyva.Bootstrap.Runtime;

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
            Container.BindInterfacesTo<TestUnitSpawner>().AsSingle().NonLazy();
            Container.BindExecutionOrder<TestUnitSpawner>(100);

            // Розкриває туман навколо стартової позиції і телепортує камеру туди.
            // Виконується після TestUnitSpawner, щоб знати чи є збереження.
            Container.BindInstance(startingPositionSettings).AsSingle();
            Container.BindInterfacesTo<StartingPositionInitializer>().AsSingle().NonLazy();
            Container.BindExecutionOrder<StartingPositionInitializer>(101);
        }
    }
}
