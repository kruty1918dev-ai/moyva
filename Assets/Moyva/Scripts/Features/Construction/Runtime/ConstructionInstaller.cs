using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    public sealed class ConstructionInstaller : MonoInstaller
    {
        [SerializeField] private BuildingRegistrySO buildingRegistry;

        [Header("Будівництво — налаштування")]
        [Tooltip("Мінімальна відстань (в тайлах) між будівлями. 0 = можна ставити впритул.")]
        [SerializeField] private int _minSpacingBetweenBuildings = 0;

        public override void InstallBindings()
        {
            if (buildingRegistry == null)
            {
                Debug.LogError("[ConstructionInstaller] Поле 'buildingRegistry' не призначено.", this);
                return;
            }

            Container.BindInstance(buildingRegistry).AsSingle();
            Container.Bind<IBuildingRegistry>().FromInstance(buildingRegistry).AsSingle();

            Container.Bind<int>().WithId("minSpacing").FromInstance(_minSpacingBetweenBuildings).AsSingle();

            Container.Bind<IScreenToGridConverter>()
                .To<ScreenToGridConverter>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ConstructionService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<WallPlacementService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<ConstructionInputService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<ConstructionVisualService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<ISaveModule>()
                .To<ConstructionSaveModule>()
                .AsSingle();

            // Явний порядок Initialize() — виконується ПІСЛЯ GameMode (-10).
            Container.BindExecutionOrder<ConstructionService>(0);
            Container.BindExecutionOrder<ConstructionInputService>(5);
            Container.BindExecutionOrder<ConstructionVisualService>(10);
        }
    }
}
