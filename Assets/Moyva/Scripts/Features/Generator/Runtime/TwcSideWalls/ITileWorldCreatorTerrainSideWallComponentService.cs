namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainSideWallComponentService
    {
        void Ensure(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallBuilder owner,
            TileWorldCreatorTerrainSideWallConfig config);
    }
}
