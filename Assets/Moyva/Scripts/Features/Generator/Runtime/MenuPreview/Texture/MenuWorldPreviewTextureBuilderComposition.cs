namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MenuWorldPreviewTextureBuilderComposition
    {
        public static IMenuWorldPreviewTextureBuilderService Create()
        {
            var settings = new MenuPreviewTextureSettingsResolver();
            var layout = new MenuPreviewTextureLayoutService();
            var source = new MenuPreviewSpritePixelSource();
            var cache = new MenuPreviewSpriteCacheFactory(source);
            var spritePainter = new MenuPreviewSpritePainter();
            var shapePainter = new MenuPreviewProjectedShapePainter();
            var terrain = new MenuPreviewTerrainLayerRenderer(spritePainter, shapePainter);
            var overlay = new MenuPreviewOverlayLayerRenderer(cache, spritePainter, shapePainter);
            var layerRenderer = new MenuPreviewMapLayerRenderer(terrain, overlay);
            return new MenuWorldPreviewTextureBuilderService(settings, layout, cache, layerRenderer);
        }
    }
}
