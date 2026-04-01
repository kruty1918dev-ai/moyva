using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionInstaller : MonoInstaller
    {
        [SerializeField] private BuildingRegistrySO buildingRegistry;

        public override void InstallBindings()
        {
            Container.BindInstance(buildingRegistry).AsSingle();

            Container.Bind<IScreenToGridConverter>()
                .To<ScreenToGridConverter>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ConstructionService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<WallPlacementService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ConstructionInputService>()
                .AsSingle();
        }
    }
}
