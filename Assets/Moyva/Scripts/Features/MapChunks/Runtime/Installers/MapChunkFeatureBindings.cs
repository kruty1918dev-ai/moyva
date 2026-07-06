using Kruty1918.Moyva.MapChunks.API;
using Zenject;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    public static class MapChunkFeatureBindings
    {
        public static void Install(DiContainer container, MapChunkSettingsSO settings = null)
        {
            if (container == null)
                return;

            if (!container.HasBinding<IMapChunkSettingsProvider>())
            {
                if (settings != null)
                    container.Bind<IMapChunkSettingsProvider>().FromInstance(settings).AsSingle();
                else if (TryFindSceneSettings(out MapChunkSceneSettings sceneSettings))
                    container.Bind<IMapChunkSettingsProvider>().FromInstance(sceneSettings).AsSingle();
                else
                    container.Bind<IMapChunkSettingsProvider>().To<DefaultMapChunkSettingsProvider>().AsSingle();
            }

            BindCore(container);
            BindRuntime(container);
        }

        private static void BindCore(DiContainer container)
        {
            if (!container.HasBinding<IMapChunkLayoutService>())
                container.Bind<IMapChunkLayoutService>().To<MapChunkLayoutService>().AsSingle();
            if (!container.HasBinding<IChunkedTileStore>())
                container.Bind<IChunkedTileStore>().To<ChunkedTileStore>().AsSingle();
            if (!container.HasBinding<IChunkedObjectStore>())
                container.Bind<IChunkedObjectStore>().To<ChunkedObjectStore>().AsSingle();
            if (!container.HasBinding<IMapVisualChunkRegistry>())
                container.Bind<IMapVisualChunkRegistry>().To<MapVisualChunkRegistry>().AsSingle();
            if (!container.HasBinding<IMapVisualChunkRootService>())
                container.Bind<IMapVisualChunkRootService>().To<MapVisualChunkRootService>().AsSingle();
            if (!container.HasBinding<IMapChunkVisibilityService>())
                container.Bind<IMapChunkVisibilityService>().To<MapChunkVisibilityService>().AsSingle();
            if (!container.HasBinding<IMapVisualRendererCollector>())
                container.Bind<IMapVisualRendererCollector>().To<MapVisualRendererCollector>().AsSingle();
            if (!container.HasBinding<IMapVisualRendererFilter>())
                container.Bind<IMapVisualRendererFilter>().To<MapVisualRendererFilter>().AsSingle();
            if (!container.HasBinding<IMapVisualChunkDiscoveryRebuildService>())
                container.Bind<IMapVisualChunkDiscoveryRebuildService>().To<MapVisualChunkDiscoveryRebuildService>().AsSingle();
        }

        private static void BindRuntime(DiContainer container)
        {
            if (!container.HasBinding<IMapVisualChunkDiscoveryService>())
                container.BindInterfacesAndSelfTo<MapVisualChunkDiscoveryService>().AsSingle().NonLazy();

            if (!container.HasBinding<MapChunkCameraCullingService>())
                container.BindInterfacesAndSelfTo<MapChunkCameraCullingService>().AsSingle().NonLazy();

            if (!container.HasBinding<IMapVisualChunkPartitionService>())
                container.BindInterfacesAndSelfTo<MapVisualChunkPartitionService>().AsSingle().NonLazy();

            if (!container.HasBinding<MapChunkWorldSignalService>())
                container.BindInterfacesAndSelfTo<MapChunkWorldSignalService>().AsSingle().NonLazy();
        }

        private static bool TryFindSceneSettings(out MapChunkSceneSettings settings)
        {
            settings = UnityEngine.Object.FindFirstObjectByType<MapChunkSceneSettings>(UnityEngine.FindObjectsInactive.Include);
            return settings != null;
        }
    }
}
