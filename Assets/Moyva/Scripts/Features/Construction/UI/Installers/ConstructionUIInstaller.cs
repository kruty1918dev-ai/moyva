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
    ///   - <c>SignalBus</c>             (надається SignalBusInstaller)
    /// </summary>
    public class ConstructionUIInstaller : MonoInstaller
    {
        [Tooltip("Компонент ConstructionUIController зі сцени.")]
        [SerializeField] private ConstructionUIController uiController;

        public override void InstallBindings()
        {
            Debug.Log("[ConstructionUIInstaller] InstallBindings РОЗПОЧАТО...");
            
            if (uiController == null)
            {
                Debug.LogError("[ConstructionUIInstaller] КРИТИЧНА ПОМИЛКА: Поле 'uiController' НЕ ПРИСВОЄНО. " +
                                 "Перетягни ConstructionUIController у поле цього інсталера.");
                return;
            }
            Debug.Log($"[ConstructionUIInstaller] ✓ uiController знайдено: {uiController.name}");

            // Реєстрація ConstructionUIController
            try
            {
                Container.BindInterfacesAndSelfTo<ConstructionUIController>()
                    .FromComponentOn(uiController.gameObject)
                    .AsSingle()
                    .NonLazy(); // Ініціалізується одразу, ОСТАННІМ
                Debug.Log("[ConstructionUIInstaller] ✓ ConstructionUIController зареєстрований у контейнері");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConstructionUIInstaller] ПОМИЛКА реєстрації ConstructionUIController: {ex.Message}");
                return;
            }

            Debug.Log("[ConstructionUIInstaller] ✅ InstallBindings УСПІШНО ЗАВЕРШЕНО");
        }
    }
}
