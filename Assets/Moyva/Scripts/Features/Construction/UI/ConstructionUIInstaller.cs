using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Zenject installer for the Construction UI module.
    /// Registers <see cref="ConstructionUIController"/> so it receives injection
    /// and participates in the IInitializable / IDisposable lifecycle.
    ///
    /// HOW TO ADD TO THE SCENE:
    /// 1. Add this component to the same GameObject as your SceneContext
    ///    (or to any GameObject referenced by the SceneContext's Installers list).
    /// 2. Drag the <see cref="ConstructionUIController"/> from the scene into
    ///    the <b>uiController</b> field.
    /// 3. Add this installer to the SceneContext's <b>Mono Installers</b> list.
    ///
    /// Dependencies resolved automatically from the scene container:
    ///   - <c>IConstructionService</c>  (provided by ConstructionInstaller)
    ///   - <c>IBuildingRegistry</c>     (provided by ConstructionInstaller)
    ///   - <c>SignalBus</c>             (provided by the SceneContext signal declarations)
    /// </summary>
    public class ConstructionUIInstaller : MonoInstaller
    {
        [Tooltip("The ConstructionUIController component from the scene.")]
        [SerializeField] private ConstructionUIController uiController;

        public override void InstallBindings()
        {
            if (uiController == null)
            {
                Debug.LogWarning("[ConstructionUIInstaller] uiController is not assigned. " +
                                 "Drag ConstructionUIController into the field on this installer.");
                return;
            }

            Container.BindInterfacesAndSelfTo<ConstructionUIController>()
                .FromComponentOn(uiController.gameObject)
                .AsSingle();
        }
    }
}
