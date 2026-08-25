using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Zenject installer для FogOfWar runtime підсистеми.
    /// </summary>
    public class FogOfWarInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            FogOfWarVolumeController[] fogVolumes =
                Object.FindObjectsByType<FogOfWarVolumeController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            FogOfWarSettings resolvedSettings =
                ResolveSettings(fogVolumes);

            if (resolvedSettings != null)
            {
                Container.BindInstance(resolvedSettings)
                    .AsSingle();
            }
            else
            {
            }

            int controllerCount =
                fogVolumes != null
                    ? fogVolumes.Length
                    : 0;

            string settingsName =
                resolvedSettings != null
                    ? resolvedSettings.name
                    : "null";

            string presentationName =
                resolvedSettings != null
                    ? resolvedSettings.PresentationMode.ToString()
                    : "LegacyTwcVolume fallback";

            string installerMessage =
                "[FogOfWar] Installer found " +
                controllerCount +
                " FogOfWarVolumeController(s); settings=" +
                settingsName +
                "; presentation=" +
                presentationName +
                ".";

            MapChunkFeatureBindings.Install(Container);

            Container.Bind<IFogSaveDataProvider>()
                .To<FogSaveDataStub>()
                .AsSingle();

            Container.Bind<HeightAwareVisionEngine>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<HeightAwareVisionService>()
                .AsSingle();

            Container.Bind<IFogVisibilityResolver>()
                .To<FogVisibilityResolver>()
                .AsSingle();

            /*
             * Legacy TWC volume services залишаються
             * зареєстрованими для fallback mode.
             */
            Container.Bind<IFogVolumePreviewBuilder>()
                .To<FogVolumePreviewBuilder>()
                .AsSingle();

            Container.Bind<IFogVolumeSceneContextBuilder>()
                .To<FogVolumeSceneContextBuilder>()
                .AsSingle();

            Container.Bind<IFogVolumeOutputCleaner>()
                .To<FogVolumeOutputCleaner>()
                .AsSingle();

            Container.Bind<IFogVolumeValidationService>()
                .To<FogVolumeValidationService>()
                .AsSingle();

            Container.Bind<IFogVolumeStateCache>()
                .To<FogVolumeStateCache>()
                .AsSingle();

            Container.Bind<IFogStartupFogServiceFactory>()
                .To<FogStartupFogServiceFactory>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<FogVolumePendingWorkQueue>()
                .AsSingle();

            Container.Bind<IFogVisualUpdateSchedulerFactory>()
                .To<FogVisualUpdateSchedulerFactory>()
                .AsSingle();

            Container.Bind<IFogDirtyClusterTracker>()
                .To<FogDirtyClusterTracker>()
                .AsSingle();

            Container.Bind<IFogClusterGeometryBuilder>()
                .To<FogClusterGeometryBuilder>()
                .AsSingle();

            Container.Bind<IFogClusterMaterialProvider>()
                .To<FogClusterMaterialProvider>()
                .AsSingle();

            Container.Bind<IFogClusterMeshPresenter>()
                .To<FogClusterMeshPresenter>()
                .AsSingle();

            Container.Bind<IFogClusterMeshRegistry>()
                .To<FogClusterMeshRegistry>()
                .AsSingle();

            Container.Bind<IFogClusterMeshBuilder>()
                .To<FogClusterMeshBuilder>()
                .AsSingle();

            Container.Bind<IFogClusteredVolumeRenderer>()
                .To<FogClusteredVolumeRenderer>()
                .AsSingle();

            Container.Bind<FogVolumeVisualUpdateEngine>()
                .AsSingle();

            /*
             * Concrete visual implementations.
             * Їхні interfaces напряму не реєструються.
             */
            Container.Bind<FogOfWarVolumeUpdater>()
                .AsSingle();

            Container.Bind<FogScreenSpaceTextureUpdater>()
                .AsSingle();

            /*
             * Router є єдиним:
             *
             * IFogVisualUpdater
             * IFogVolumeRuntimeUpdater
             * ITickable
             * IDisposable
             */
            Container
                .BindInterfacesAndSelfTo<FogVisualUpdaterRouter>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<FogOfWarService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<FogRendererCullingEngine>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<FogRendererCullingService>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<FogOfWarSaveModule>()
                .AsSingle();

            Container
                .BindInterfacesTo<
                    SaveModuleRegistrar<FogOfWarSaveModule>>()
                .AsSingle()
                .NonLazy();

            if (fogVolumes != null)
            {
                for (int i = 0;
                     i < fogVolumes.Length;
                     i++)
                {
                    FogOfWarVolumeController fogVolume =
                        fogVolumes[i];

                    if (fogVolume == null)
                        continue;

                    Container.QueueForInject(fogVolume);
                }
            }

            Container.Bind<IFogOfWarServiceRegistry>()
                .To<FogOfWarServiceRegistry>()
                .AsSingle();

            if (!Container.HasBinding<
                    IMapFogChunkCoverageService>())
            {
                Container.Bind<IMapFogChunkCoverageService>()
                    .To<MapFogChunkCoverageService>()
                    .AsSingle();
            }

            Container
                .BindInterfacesAndSelfTo<
                    MapFogChunkCoverageRefreshService>()
                .AsSingle()
                .NonLazy();

            Container.BindExecutionOrder<
                FogOfWarService>(-5);

            Container.BindExecutionOrder<
                FogVisualUpdaterRouter>(-4);

            Container.BindExecutionOrder<
                FogRendererCullingService>(-3);

            Container.BindExecutionOrder<
                MapFogChunkCoverageRefreshService>(260);
        }

        private FogOfWarSettings ResolveSettings(
            FogOfWarVolumeController[] fogVolumes)
        {
            if (fogVolumes == null)
                return null;

            for (int i = 0;
                 i < fogVolumes.Length;
                 i++)
            {
                FogOfWarVolumeController controller =
                    fogVolumes[i];

                if (controller != null
                    && controller.Settings != null)
                {
                    return controller.Settings;
                }
            }

            return null;
        }
    }
}
