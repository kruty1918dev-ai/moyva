namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainSideWallEdgeAppender
    {
        void TryAppend(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallConfig config,
            TileWorldCreatorTerrainSideWallEdge edge,
            int currentLevel,
            int edgeLevel,
            int width,
            int height,
            ref int wallCount);
    }
}
