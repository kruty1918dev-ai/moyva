namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainSideWallDiagnostics
    {
        void LogConfigure(TileWorldCreatorTerrainSideWallBuilder owner, TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallConfig config);
        void LogDelayedRebuild(TileWorldCreatorTerrainSideWallState state, string reason);
        void LogSkipped(string reason);
        void LogBuildResult(TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallBuildResult result);
        void LogCleared(string reason);
    }
}
