using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    public class ConstructionInstaller : MonoInstaller
    {
        [SerializeField] private BuildingRegistrySO _buildingRegistry;

        public override void InstallBindings()
        {
            Container.BindInstance(_buildingRegistry).AsSingle();

            Container.BindInterfacesAndSelfTo<BuildingConstructionService>()
                .AsSingle();

            Container.Bind<IWallConnectionService>()
                .To<WallConnectionService>()
                .AsSingle();
        }
    }
}
