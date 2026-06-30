using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    public class FogOfWarInstaller : MonoInstaller
    {
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
            Container.Bind<IHeightAwareVisionService>().To<HeightAwareVisionService>().AsSingle();
            Container.Bind<IFogVisibilityResolver>().To<FogVisibilityResolver>().AsSingle();
            Container.BindInterfacesAndSelfTo<FogOfWarVolumeUpdater>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<FogOfWarService>()
                .AsSingle()
                .NonLazy();

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
