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
            Debug.Log($"{LogTag} Configure root='{(root != null ? root.name : "<null>")}', map={TileWorldCreatorMapFormatUtility.FormatLevelStats(config.TerrainLevelMap)}, cellSize={TileWorldCreatorTerrainSideWallFormat.Number(config.CellSize)}, heightStep={config.HeightStep}, baseY={TileWorldCreatorTerrainSideWallFormat.Number(config.BaseY)}, includeMapBoundaryWalls={config.IncludeMapBoundaryWalls}, material='{material}', color={TileWorldCreatorMapFormatUtility.FormatColor(config.WallColor)}.");
            Debug.Log($"{ArtifactLogTag} Configure artifact diagnostics. builder='{owner.name}', root='{(root != null ? root.name : "<null>")}', rootTransform={TileWorldCreatorTerrainSideWallFormat.Transform(root)}, builderTransform={TileWorldCreatorTerrainSideWallFormat.Transform(owner.transform)}, map={TileWorldCreatorMapFormatUtility.FormatLevelStats(config.TerrainLevelMap)}, cellSize={TileWorldCreatorTerrainSideWallFormat.Number(config.CellSize)}, heightStep={config.HeightStep}, baseY={TileWorldCreatorTerrainSideWallFormat.Number(config.BaseY)}, includeMapBoundaryWalls={config.IncludeMapBoundaryWalls}, material='{material}', materialCull={TileWorldCreatorTerrainSideWallFormat.FormatMaterialCull(state.MeshRenderer?.sharedMaterial)}.");
        }

        public void LogDelayedRebuild(TileWorldCreatorTerrainSideWallState state, string reason)
        {
            var config = state.LastConfig;
            Debug.Log($"{LogTag} Delayed rebuild requested. reason='{reason}', map={TileWorldCreatorMapFormatUtility.FormatLevelStats(config.TerrainLevelMap)}, cellSize={TileWorldCreatorTerrainSideWallFormat.Number(config.CellSize)}, heightStep={config.HeightStep}, baseY={TileWorldCreatorTerrainSideWallFormat.Number(config.BaseY)}, includeMapBoundaryWalls={config.IncludeMapBoundaryWalls}.");
            Debug.Log($"{ArtifactLogTag} Delayed artifact-diagnostics rebuild. reason='{reason}', builderTransform=<state>, map={TileWorldCreatorMapFormatUtility.FormatLevelStats(config.TerrainLevelMap)}, cellSize={TileWorldCreatorTerrainSideWallFormat.Number(config.CellSize)}, heightStep={config.HeightStep}, baseY={TileWorldCreatorTerrainSideWallFormat.Number(config.BaseY)}, includeMapBoundaryWalls={config.IncludeMapBoundaryWalls}.");
        }

        public void LogSkipped(string reason)
        {
            Debug.LogWarning($"{LogTag} Rebuild skipped: {reason}");
        }

        public void LogBuildResult(TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallBuildResult result)
        {
            var stats = result.Stats;
            if (stats.WallCount == 0)
            {
                Debug.LogWarning($"{LogTag} Rebuild produced no side walls. map={stats.Width}x{stats.Height}, levelRange={stats.MinLevel}..{stats.MaxLevel}, edgeLevel={stats.EdgeLevel}, skippedBoundaryWalls={stats.SkippedBoundaryWalls}.");
                Debug.LogWarning($"{ArtifactLogTag} Rebuild produced no side-wall artifact mesh. diagnostics={result.Artifacts.Format()}, samples={TileWorldCreatorTerrainSideWallFormat.Samples(state.ArtifactSamples)}.");
                return;
            }

            Debug.Log($"{LogTag} Rebuild complete. map={stats.Width}x{stats.Height}, levelRange={stats.MinLevel}..{stats.MaxLevel}, edgeLevel={stats.EdgeLevel}, includeMapBoundaryWalls={state.LastConfig.IncludeMapBoundaryWalls}, skippedBoundaryWalls={stats.SkippedBoundaryWalls}, walls={stats.WallCount} (E={stats.EastWalls}, W={stats.WestWalls}, N={stats.NorthWalls}, S={stats.SouthWalls}), totalLevelDiff={stats.TotalLevelDifference}, maxLevelDiff={stats.MaxLevelDifference}, diffHistogram={TileWorldCreatorTerrainSideWallFormat.Histogram(stats.DifferenceHistogram)}, vertices={stats.VertexCount}, triangles={stats.TriangleCount}, indexFormat={result.IndexFormat}, bounds={TileWorldCreatorTerrainSideWallFormat.Bounds(state.Mesh.bounds)}, samples={TileWorldCreatorTerrainSideWallFormat.Samples(state.Samples)}.");
            Debug.Log($"{ArtifactLogTag} Rebuild diagnostics. meshLocalBounds={TileWorldCreatorTerrainSideWallFormat.Bounds(state.Mesh.bounds)}, rendererWorldBounds={(state.MeshRenderer != null ? TileWorldCreatorTerrainSideWallFormat.Bounds(state.MeshRenderer.bounds) : "<no renderer>")}, includeMapBoundaryWalls={state.LastConfig.IncludeMapBoundaryWalls}, walls={stats.WallCount}, vertices={stats.VertexCount}, triangles={stats.TriangleCount}, diagnostics={result.Artifacts.Format()}, samples={TileWorldCreatorTerrainSideWallFormat.Samples(state.ArtifactSamples)}.");
        }

        public void LogCleared(string reason)
        {
            Debug.Log($"{LogTag} Cleared generated side walls. reason='{reason}'.");
        }
    }
}
