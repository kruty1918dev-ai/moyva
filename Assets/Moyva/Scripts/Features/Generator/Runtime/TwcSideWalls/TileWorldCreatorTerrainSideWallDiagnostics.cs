using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorTerrainSideWallDiagnostics : ITileWorldCreatorTerrainSideWallDiagnostics
    {
        private const string LogTag = "[MoyvaTWCHeight:SideWalls]";
        private const string ArtifactLogTag = "[MoyvaTWCHeight:SideWallArtifact]";

        public void LogConfigure(TileWorldCreatorTerrainSideWallBuilder owner, TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallConfig config)
        {
            Transform root = config.TargetRoot != null ? config.TargetRoot : owner.transform.parent;
            string material = state.MeshRenderer != null && state.MeshRenderer.sharedMaterial != null ? state.MeshRenderer.sharedMaterial.name : "<null>";
        }

        public void LogDelayedRebuild(TileWorldCreatorTerrainSideWallState state, string reason)
        {
            var config = state.LastConfig;
        }

        public void LogSkipped(string reason)
        {
        }

        public void LogBuildResult(TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallBuildResult result)
        {
            var stats = result.Stats;
            if (stats.WallCount == 0)
            {
                return;
            }
        }

        public void LogCleared(string reason)
        {
        }
    }
}
