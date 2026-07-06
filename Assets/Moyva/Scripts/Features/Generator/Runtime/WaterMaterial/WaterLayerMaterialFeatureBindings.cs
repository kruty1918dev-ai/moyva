using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class WaterLayerMaterialFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            container.Bind<IWaterLayerMaterialPropertyWriter>()
                .To<WaterLayerMaterialPropertyWriter>()
                .AsSingle();
            container.Bind<IWaterLayerMaterialPresetService>()
                .To<WaterLayerMaterialPresetService>()
                .AsSingle();
            container.Bind<IWaterLayerMaterialApplier>()
                .To<WaterLayerMaterialApplier>()
                .AsSingle();
        }
    }
}
