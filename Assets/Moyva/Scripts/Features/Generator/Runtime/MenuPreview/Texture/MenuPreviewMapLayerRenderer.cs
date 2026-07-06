namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewMapLayerRenderer : IMenuPreviewMapLayerRenderer
    {
        private readonly IMenuPreviewTerrainLayerRenderer _terrain;
        private readonly IMenuPreviewOverlayLayerRenderer _overlay;

        public MenuPreviewMapLayerRenderer(
            IMenuPreviewTerrainLayerRenderer terrain,
            IMenuPreviewOverlayLayerRenderer overlay)
        {
            _terrain = terrain;
            _overlay = overlay;
        }

        public void DrawTerrain(MenuPreviewTextureBuildContext context)
        {
            _terrain.Draw(context);
        }

        public void DrawObjects(MenuPreviewTextureBuildContext context)
        {
            _overlay.DrawObjects(context);
        }

        public void DrawBuildings(MenuPreviewTextureBuildContext context)
        {
            _overlay.DrawBuildings(context);
        }
    }
}
