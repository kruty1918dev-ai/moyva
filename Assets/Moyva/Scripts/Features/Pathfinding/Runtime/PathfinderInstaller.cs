using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

#if UNITY_EDITOR
using UnityEditor;
using Kruty1918.Moyva.Jsonization;
#endif

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
    public class PathfinderInstaller : MonoInstaller
    {
        [SerializeField] private MoyvaProjectSettingsSO _projectSettings;

        public override void InstallBindings()
        {
            if (!Container.HasBinding<INeighborhoodStrategy>())
            {
                Container.Bind<INeighborhoodStrategy>()
                    .FromMethod(context => NeighborhoodStrategyFactory.Create(ResolveProjectSettings(context.Container)))
                    .AsSingle();
            }

            Container.Bind<IPathfinder>().To<Pathfinder>().AsSingle();
        }

        private MoyvaProjectSettingsSO ResolveProjectSettings(DiContainer container)
        {
            MoyvaProjectSettingsSO resolved = container.HasBinding<MoyvaProjectSettingsSO>()
                ? container.Resolve<MoyvaProjectSettingsSO>()
                : _projectSettings != null
                    ? _projectSettings
                    : LoadProjectSettingsAssetOrDefault();
            resolved.Normalize();
            return resolved;
        }

        private static MoyvaProjectSettingsSO LoadProjectSettingsAssetOrDefault()
        {
#if UNITY_EDITOR
            var settings = MoyvaJsonRuntime.GetLegacyResource<MoyvaProjectSettingsSO>(MoyvaProjectSettingsSO.DefaultAssetPath);
            if (settings != null)
                return settings;
#endif

            return MoyvaProjectSettingsSO.CreateRuntimeDefault();
        }
    }
}
