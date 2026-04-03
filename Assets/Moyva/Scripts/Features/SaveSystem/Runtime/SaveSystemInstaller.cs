using System.Collections.Generic;
using Zenject;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Встановлює SaveSystem у Zenject-контейнер сцени.
    ///
    /// Встановлює два сервіси:
    /// 1. SaveService — для ігрових слотів (slot00-99)
    /// 2. ConfigService — для глобального конфіга (config.mvs)
    ///
    /// Додайте цей MonoInstaller до Scene Context після SignalBusInstaller.
    /// Будь-який клас, що реалізує ISaveModule і зареєстрований в контейнері,
    /// автоматично підхопиться обома сервісами.
    ///
    /// ExecutionOrder=-8 гарантує, що SaveService підпишеться на сигнали
    /// до того, як інші сервіси починають їх стріляти.
    /// </summary>
    public sealed class SaveSystemInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SaveService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<ISaveInspectorService>()
                .To<SaveInspectorService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ConfigService>()
                .AsSingle()
                .NonLazy();

            Container.BindExecutionOrder<SaveService>(-8);
        }
    }
}
