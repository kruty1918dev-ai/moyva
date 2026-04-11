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

        [Tooltip("Радіус дії ратуші для будівництва (в тайлах).\nНе-ратуші можна ставити лише в цій зоні.\n0 = вимкнути правило.")]
        [SerializeField] private int _townHallBuildRadius = 12;

        public override void InstallBindings()
        {
            if (buildingRegistry == null)
            {
                Debug.LogError("[ConstructionInstaller] Поле 'buildingRegistry' не призначено.", this);
                return;
            }

            Container.BindInstance(buildingRegistry).AsSingle();
            Container.Bind<IBuildingRegistry>().FromInstance(buildingRegistry).AsSingle();

            Container.BindInstance(_minSpacingBetweenBuildings).WithId("minSpacing");
            Container.BindInstance(_townHallBuildRadius).WithId("townHallBuildRadius");

            Container.Bind<IScreenToGridConverter>()
                .To<ScreenToGridConverter>()
                .AsSingle();

            Container.Bind<IAutoTileVariantResolver>()
                .To<AutoTileVariantResolver>()
                .AsSingle();

            Container.Bind<IObjectTypePicker>()
                .To<ObjectTypePickerService>()
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
