using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
    public class PathfinderInstaller : MonoInstaller
    {
        [SerializeField] private MoyvaProjectSettingsSO _projectSettings;

        public override void InstallBindings()
        {
            if (!Container.HasBinding<INeighborhoodStrategy>())
                Container.Bind<INeighborhoodStrategy>().FromInstance(NeighborhoodStrategyFactory.Create(ResolveProjectSettings())).AsSingle();

            Container.Bind<IPathfinder>().To<Pathfinder>().AsSingle();
        }

        private MoyvaProjectSettingsSO ResolveProjectSettings()
        {
            MoyvaProjectSettingsSO resolved = Container.HasBinding<MoyvaProjectSettingsSO>()
                ? Container.Resolve<MoyvaProjectSettingsSO>()
                : _projectSettings != null
                    ? _projectSettings
                    : MoyvaProjectSettingsSO.CreateRuntimeDefault();
            resolved.Normalize();
            return resolved;
        }
    }
}