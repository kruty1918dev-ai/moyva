namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainSideWallMeshBuilder
    {
        TileWorldCreatorTerrainSideWallBuildResult Build(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallConfig config);
    }
}
