using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.WorldCreation.UI
{
    /// <summary>
    /// Zenject-інсталер для модуля WorldCreation UI.
    /// Реєструє <see cref="WorldCreationUIController"/> для отримання ін'єкцій
    /// та участі у lifecycle IInitializable / IDisposable.
    ///
    /// ЯК ДОДАТИ ДО СЦЕНИ:
    /// 1. Додай компонент до того ж GameObject, що й SceneContext
    ///    (або до будь-якого GameObject зі списку Installers).
    /// 2. Перетягни <see cref="WorldCreationUIController"/> зі сцени у поле <b>uiController</b>.
    /// 3. Додай цей інсталер до списку <b>Mono Installers</b> у SceneContext.
    ///
    /// Залежності, що вирішуються автоматично з контейнера:
    ///   - <c>IWorldCreationService</c>  (надається WorldCreationInstaller)
    ///   - <c>SignalBus</c>              (надається SignalBusInstaller)
    /// </summary>
    public sealed class WorldCreationUIInstaller : MonoInstaller
    {
        [Tooltip("Компонент WorldCreationUIController зі сцени.")]
        [SerializeField] private WorldCreationUIController uiController;

        public override void InstallBindings()
        {
            if (uiController == null)
            {
                Debug.LogWarning("[WorldCreationUIInstaller] Поле 'uiController' не призначено. " +
                                 "Перетягни WorldCreationUIController у поле цього інсталера.");
                return;
            }

            Container.BindInterfacesAndSelfTo<WorldCreationUIController>()
                .FromComponentOn(uiController.gameObject)
                .AsSingle()
                .NonLazy();
        }
    }
}
