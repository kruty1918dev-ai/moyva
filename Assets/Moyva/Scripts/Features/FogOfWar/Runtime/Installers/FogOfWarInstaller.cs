using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.JsonConfig;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using Kruty1918.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;
using Kruty1918.Moyva.SaveSystem;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Zenject installer для FogOfWar runtime підсистеми.
    /// </summary>
    public class FogOfWarInstaller : MonoInstaller
    {
        public static void InstallSimulationBindings(DiContainer container)
        {
            container.Bind<IFogSaveDataProvider>().To<FogSaveDataStub>().AsSingle();
            container.Bind<HeightAwareVisionEngine>().AsSingle();
            container.BindInterfacesAndSelfTo<HeightAwareVisionService>().AsSingle();
            container.Bind<IFogVisibilityResolver>().To<FogVisibilityResolver>().AsSingle();
            container.BindInterfacesAndSelfTo<FogOfWarService>().FromMethod(ctx => new FogOfWarService(
                ctx.Container.Resolve<IFogVisibilityResolver>(), ctx.Container.Resolve<IHeightAwareVisionService>(),
                null, ctx.Container.Resolve<IFogSaveDataProvider>(), ctx.Container.Resolve<SignalBus>(),
                ctx.Container.TryResolve<FogOfWarSettings>(), ctx.Container.Resolve<IWorldGenerationSignalState>())).AsSingle();
            container.BindInterfacesAndSelfTo<FogIntelStore>().AsSingle();
        }

        public override void InstallBindings()
        {
            FogOfWarSettings resolvedSettings = TryResolveSettings();
            if (resolvedSettings != null)
            {
                Container.BindInstance(resolvedSettings)
                    .AsSingle();
            }

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

            Container.Bind<FogScreenSpaceTextureUpdater>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<FogVisualUpdaterRouter>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<FogOfWarService>()
                .AsSingle()
                .NonLazy();

            // Remembered-entity intel: last-known unit/building snapshots per
            // owner. Bound after the fog service so the visibility feed is
            // available at construction time.
            Container
                .BindInterfacesAndSelfTo<FogIntelStore>()
                .AsSingle()
                .NonLazy();

            // Ghost markers for remembered entities — pure presentation,
            // pooled sprites, never gameplay objects.
            Container
                .BindInterfacesAndSelfTo<FogIntelGhostPresenter>()
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

        private static FogOfWarSettings TryResolveSettings()
        {
            return JsonConfigRuntime.Get<FogOfWarSettings>("fogofwarsettings");
        }
    }
}
