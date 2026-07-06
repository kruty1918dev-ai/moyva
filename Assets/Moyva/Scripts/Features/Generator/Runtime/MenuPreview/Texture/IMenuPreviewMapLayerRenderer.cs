namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewMapLayerRenderer
    {
        void DrawTerrain(MenuPreviewTextureBuildContext context);
        void DrawObjects(MenuPreviewTextureBuildContext context);
        void DrawBuildings(MenuPreviewTextureBuildContext context);
    }
}
