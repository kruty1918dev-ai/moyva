using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphTwcMapDataState : IGraphTwcMapDataState
    {
        private bool _hasLastBaseMapWorldBounds;
        private Bounds _lastBaseMapWorldBounds;

        public IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; private set; }
        public GraphLogicalTileMap LastLogicalMap { get; private set; }
        public float LastCellSize { get; private set; } = 1f;
        public int[,] LastTerrainLevelMap { get; private set; }
        public bool LastForceChunkFirst { get; private set; }
        public bool LastHasAuthoredGeography { get; private set; }
        public Vector2Int[] LastSpawnHints { get; private set; }
        public Geography.WorldGenerationReport LastGeographyReport { get; private set; }
        public bool TryGetLastBaseMapWorldBounds(out Bounds bounds)
        {
            bounds = _lastBaseMapWorldBounds;
            return _hasLastBaseMapWorldBounds;
        }

        public void Apply(GraphTwcMapGenerationResult result)
        {
            if (result == null)
                return;

            if (result.CompiledLayers != null)
                LastCompiledLayers = result.CompiledLayers;
            LastLogicalMap = result.LogicalMap;
            LastCellSize = result.CellSize > 0.0001f ? result.CellSize : 1f;
            _hasLastBaseMapWorldBounds = result.HasBaseMapWorldBounds;
            _lastBaseMapWorldBounds = result.BaseMapWorldBounds;

            // Geography-authored data is cleared again when a plain graph
            // result arrives, so a disabled config can never leak stale levels.
            LastTerrainLevelMap = result.TerrainLevelMap;
            LastForceChunkFirst = result.ForceChunkFirst;
            LastHasAuthoredGeography = result.HasAuthoredGeography;
            LastSpawnHints = result.SpawnHints;
            LastGeographyReport = result.GeographyReport;
        }
    }
}
