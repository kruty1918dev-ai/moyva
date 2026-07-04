using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Zenject installer для FogOfWar runtime підсистеми.
    /// Біндить gameplay fog state, save module, visual update path, validation/preview helpers
    /// та renderer culling services. Ці bindings runtime-critical для нормальної роботи туману.
    /// </summary>
    public class FogOfWarInstaller : MonoInstaller
    {
        /// <summary>
        /// Реєструє всі FogOfWar сервіси в контейнері сцени.
        /// Під час інсталяції також знаходить на сцені <see cref="FogOfWarVolumeController"/> і ставить їх у чергу на inject.
        /// </summary>
        public override void InstallBindings()
        {
            var fogVolumes = Object.FindObjectsByType<FogOfWarVolumeController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var resolvedSettings = ResolveSettings(fogVolumes);
            if (resolvedSettings != null)
                Container.BindInstance(resolvedSettings).AsSingle();
            else
                Debug.LogWarning("[FogOfWar] FogOfWarInstaller did not find FogOfWarSettings on any FogOfWarVolumeController.");

            Debug.Log($"[FogOfWar] Installer found {fogVolumes?.Length ?? 0} FogOfWarVolumeController(s); settings={(resolvedSettings != null ? resolvedSettings.name : "null")}. Diagnostics build=2026-06-30-fog-volume-logging.");
            LogControllerDiagnostics(fogVolumes);

            Container.Bind<IFogSaveDataProvider>().To<FogSaveDataStub>().AsSingle();
            Container.Bind<HeightAwareVisionEngine>().AsSingle();
            Container.BindInterfacesAndSelfTo<HeightAwareVisionService>().AsSingle();
            Container.Bind<IFogVisibilityResolver>().To<FogVisibilityResolver>().AsSingle();
            Container.Bind<IFogVolumePreviewBuilder>().To<FogVolumePreviewBuilder>().AsSingle();
            Container.Bind<IFogVolumeSceneContextBuilder>().To<FogVolumeSceneContextBuilder>().AsSingle();
            Container.Bind<IFogVolumeOutputCleaner>().To<FogVolumeOutputCleaner>().AsSingle();
            Container.Bind<IFogVolumeValidationService>().To<FogVolumeValidationService>().AsSingle();
            Container.Bind<IFogVolumeStateCache>().To<FogVolumeStateCache>().AsSingle();
            Container.Bind<IFogStartupFogServiceFactory>().To<FogStartupFogServiceFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<FogVolumePendingWorkQueue>().AsSingle();
            Container.Bind<IFogVisualUpdateSchedulerFactory>().To<FogVisualUpdateSchedulerFactory>().AsSingle();
            Container.Bind<IFogDirtyClusterTracker>().To<FogDirtyClusterTracker>().AsSingle();
            Container.Bind<IFogClusterGeometryBuilder>().To<FogClusterGeometryBuilder>().AsSingle();
            Container.Bind<IFogClusterMaterialProvider>().To<FogClusterMaterialProvider>().AsSingle();
            Container.Bind<IFogClusterMeshPresenter>().To<FogClusterMeshPresenter>().AsSingle();
            Container.Bind<IFogClusterMeshRegistry>().To<FogClusterMeshRegistry>().AsSingle();
            Container.Bind<IFogClusterMeshBuilder>().To<FogClusterMeshBuilder>().AsSingle();
            Container.Bind<IFogClusteredVolumeRenderer>().To<FogClusteredVolumeRenderer>().AsSingle();
            Container.Bind<FogVolumeVisualUpdateEngine>().AsSingle();
            Container.BindInterfacesAndSelfTo<FogOfWarVolumeUpdater>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<FogOfWarService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<FogRendererCullingEngine>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<FogRendererCullingService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<FogOfWarSaveModule>()
                .AsSingle();

            Container.BindInterfacesTo<SaveModuleRegistrar<FogOfWarSaveModule>>()
                .AsSingle()
                .NonLazy();

            foreach (var fogVolume in fogVolumes)
            {
                Container.QueueForInject(fogVolume);
            }

            Container.Bind<IFogOfWarServiceRegistry>()
                .To<FogOfWarServiceRegistry>()
                .AsSingle();

            Container.BindExecutionOrder<FogOfWarService>(-5);
            Container.BindExecutionOrder<FogOfWarVolumeUpdater>(-4);
            Container.BindExecutionOrder<FogRendererCullingService>(-3);
        }

        /// <summary>
        /// Пише у лог короткий діагностичний знімок controller-ів, знайдених у сцені.
        /// Використовується лише для валідації setup і не впливає на gameplay logic.
        /// </summary>
        /// <param name="fogVolumes">Масив знайдених fog volume controller-ів.</param>
        private static void LogControllerDiagnostics(FogOfWarVolumeController[] fogVolumes)
        {
            if (fogVolumes == null || fogVolumes.Length == 0)
                return;

            for (int i = 0; i < fogVolumes.Length; i++)
            {
                var fogVolume = fogVolumes[i];
                if (fogVolume == null)
                    continue;

                var manager = fogVolume.TileWorldCreatorManager;
                Debug.Log(
                    $"[FogOfWar] Controller[{i}] name='{fogVolume.name}', active={fogVolume.gameObject.activeInHierarchy}, enabled={fogVolume.enabled}, " +
                    $"settings={(fogVolume.Settings != null ? fogVolume.Settings.name : "null")}, manager={(manager != null ? manager.name : "null")}, " +
                    $"managerConfig={(manager != null && manager.configuration != null ? manager.configuration.name : "null")}, " +
                    $"updateMode={fogVolume.EffectiveUpdateMode}, logSummary={fogVolume.LogBuildSummary}, logEveryUpdate={fogVolume.LogEveryVolumeUpdate}, logValidation={fogVolume.LogValidationWarnings}.",
                    fogVolume);
            }
        }

        /// <summary>
        /// Повертає перший доступний <see cref="FogOfWarSettings"/>, знайдений через fog volume controllers у сцені.
        /// </summary>
        /// <param name="fogVolumes">Контролери, які були знайдені у сцені.</param>
        /// <returns>Знайдений settings asset або <see langword="null"/>.</returns>
        private FogOfWarSettings ResolveSettings(FogOfWarVolumeController[] fogVolumes)
        {
            if (fogVolumes == null)
                return null;

            for (int i = 0; i < fogVolumes.Length; i++)
            {
                if (fogVolumes[i] != null && fogVolumes[i].Settings != null)
                    return fogVolumes[i].Settings;
            }

            return null;
        }
    }
}
