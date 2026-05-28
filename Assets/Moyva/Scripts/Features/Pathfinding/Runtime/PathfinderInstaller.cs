using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
    public class PathfinderInstaller : MonoInstaller
    {
        [SerializeField] private MoyvaProjectSettingsSO _projectSettings;

        public override void InstallBindings()
        {
            var projectSettings = ResolveProjectSettings();

            if (!Container.HasBinding<INeighborhoodStrategy>())
            {
                var strategy = NeighborhoodStrategyFactory.Create(projectSettings);
                Container.Bind<INeighborhoodStrategy>().FromInstance(strategy).AsSingle();
            }

            Container.Bind<IPathfinder>().To<Pathfinder>().AsSingle();
        }

        private MoyvaProjectSettingsSO ResolveProjectSettings()
        {
            MoyvaProjectSettingsSO resolved = Container.HasBinding<MoyvaProjectSettingsSO>()
                ? Container.Resolve<MoyvaProjectSettingsSO>()
                : _projectSettings != null
                    ? _projectSettings
                    : LoadProjectSettingsAssetOrDefault();
            resolved.Normalize();
            return resolved;
        }

        private static MoyvaProjectSettingsSO LoadProjectSettingsAssetOrDefault()
        {
#if UNITY_EDITOR
            var settings = AssetDatabase.LoadAssetAtPath<MoyvaProjectSettingsSO>(MoyvaProjectSettingsSO.DefaultAssetPath);
            if (settings != null)
                return settings;
#endif

            return MoyvaProjectSettingsSO.CreateRuntimeDefault();
        }
    }
}
