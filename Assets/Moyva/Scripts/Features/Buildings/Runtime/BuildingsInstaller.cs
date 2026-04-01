using Kruty1918.Moyva.Buildings.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller для системи будівель.
    /// Прив'язує <see cref="IBuildingPlacementService"/> до <see cref="BuildingPlacementService"/>.
    /// </summary>
    public class BuildingsInstaller : MonoInstaller
    {
        [SerializeField] private BuildingRegistrySO _buildingRegistry;

        public override void InstallBindings()
        {
            Container.BindInstance(_buildingRegistry).AsSingle();

            Container.BindInterfacesAndSelfTo<BuildingPlacementService>()
                .AsSingle();
        }
    }
}
