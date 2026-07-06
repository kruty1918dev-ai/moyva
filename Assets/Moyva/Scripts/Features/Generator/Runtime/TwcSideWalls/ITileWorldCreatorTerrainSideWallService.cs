namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainSideWallService
    {
        void Configure(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallBuilder owner,
            TileWorldCreatorTerrainSideWallConfig config);

        void RebuildFromLastConfiguration(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallBuilder owner,
            string reason);

        void ClearWalls(TileWorldCreatorTerrainSideWallState state, string reason);
        void Dispose(TileWorldCreatorTerrainSideWallState state);
    }
}
