using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Zenject-інсталер для модуля Construction UI.
    /// Реєструє <see cref="ConstructionUIController"/> для отримання ін'єкцій
    /// та участі у lifecycle IInitializable / IDisposable.
    ///
    /// ЯК ДОДАТИ ДО СЦЕНИ:
    /// 1. Додай компонент до того ж GameObject, що й SceneContext
    ///    (або до будь-якого GameObject зі списку Installers).
    /// 2. Перетягни <see cref="ConstructionUIController"/> зі сцени у поле <b>uiController</b>.
    /// 3. Додай цей інсталер до списку <b>Mono Installers</b> у SceneContext.
    ///
    /// Залежності, що вирішуються автоматично з контейнера:
    ///   - <c>IConstructionService</c>  (надається ConstructionInstaller)
    ///   - <c>IBuildingRegistry</c>     (надається ConstructionInstaller)
    ///   - <c>IGameModeService</c>      (надається GameModeInstaller)
    ///   - <c>SignalBus</c>             (надається SignalBusInstaller)
    /// </summary>
    public class ConstructionUIInstaller : MonoInstaller
    {
        [Tooltip("Компонент ConstructionUIController зі сцени.")]
        [SerializeField] private ConstructionUIController uiController;

        public override void InstallBindings()
        {
            if (uiController == null)
            {
                Debug.LogWarning("[ConstructionUIInstaller] Поле 'uiController' не призначено. " +
                                 "Перетягни ConstructionUIController у поле цього інсталера.");
                return;
            }

            Container.BindInterfacesAndSelfTo<ConstructionUIController>()
                .FromComponentOn(uiController.gameObject)
                .AsSingle();
        }
    }
}
