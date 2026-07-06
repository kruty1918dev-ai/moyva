using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public static class MenuWorldPreviewTextureBuilderFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            if (container.HasBinding<IMenuWorldPreviewTextureBuilderService>())
                return;

            container.Bind<IMenuPreviewTextureSettingsResolver>().To<MenuPreviewTextureSettingsResolver>().AsSingle();
            container.Bind<IMenuPreviewTextureLayoutService>().To<MenuPreviewTextureLayoutService>().AsSingle();
            container.Bind<IMenuPreviewSpritePixelSource>().To<MenuPreviewSpritePixelSource>().AsSingle();
            container.Bind<IMenuPreviewSpriteCacheFactory>().To<MenuPreviewSpriteCacheFactory>().AsSingle();
            container.Bind<IMenuPreviewSpritePainter>().To<MenuPreviewSpritePainter>().AsSingle();
            container.Bind<IMenuPreviewProjectedShapePainter>().To<MenuPreviewProjectedShapePainter>().AsSingle();
            container.Bind<IMenuPreviewTerrainLayerRenderer>().To<MenuPreviewTerrainLayerRenderer>().AsSingle();
            container.Bind<IMenuPreviewOverlayLayerRenderer>().To<MenuPreviewOverlayLayerRenderer>().AsSingle();
            container.Bind<IMenuPreviewMapLayerRenderer>().To<MenuPreviewMapLayerRenderer>().AsSingle();
            container.Bind<IMenuWorldPreviewTextureBuilderService>().To<MenuWorldPreviewTextureBuilderService>().AsSingle();
        }
    }
}
