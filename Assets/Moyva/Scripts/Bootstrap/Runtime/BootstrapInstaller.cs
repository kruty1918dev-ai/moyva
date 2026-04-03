using Zenject;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.SaveSystem;

namespace Kruty1918.Moyva.Bootstrap
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Модуль збереження юнітів — реєструється як ISaveModule, автоматично
            // потрапляє в List<ISaveModule> при ініціалізації SaveService.
            Container.Bind<ISaveModule>()
                .To<UnitsSaveModule>()
                .AsSingle();

            // Автозбереження при виході з програми.
            Container.BindInterfacesTo<GameExitSaver>()
                .AsSingle()
                .NonLazy();

            // TestUnitSpawner: перевіряє наявність сейву —
            // якщо є сейв, завантажує юнітів; інакше спавнить тестові.
            Container.BindInterfacesTo<TestUnitSpawner>().AsSingle().NonLazy();

            // TestUnitSpawner має ініціалізуватись ОСТАННІМ — після усіх сервісів,
            // щоб усі підписки на сигнали (ObjectsMapService, UnitService тощо) були готові.
            Container.BindExecutionOrder<TestUnitSpawner>(100);
        }
    }
}
