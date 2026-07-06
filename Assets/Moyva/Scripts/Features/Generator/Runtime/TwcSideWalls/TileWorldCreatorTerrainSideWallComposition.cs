namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorTerrainSideWallComposition
    {
        public static ITileWorldCreatorTerrainSideWallService Create()
        {
            var materials = new TileWorldCreatorTerrainSideWallMaterialService();
            var components = new TileWorldCreatorTerrainSideWallComponentService(materials);
            var edgeAppender = new TileWorldCreatorTerrainSideWallEdgeAppender();
            var meshBuilder = new TileWorldCreatorTerrainSideWallMeshBuilder(edgeAppender);
            var diagnostics = new TileWorldCreatorTerrainSideWallDiagnostics();
            return new TileWorldCreatorTerrainSideWallService(components, meshBuilder, diagnostics);
        }
    }
}
