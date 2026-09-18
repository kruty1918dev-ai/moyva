using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Geography;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphTwcMapGenerationResult
    {
        public string[,] BiomeMap;
        public string[,] ObjectMap;
        public float[,] HeightMap;
        public string[,] BuildingMap;
        public GraphLogicalTileMap LogicalMap;
        public IReadOnlyList<CompiledLayerMap> CompiledLayers;
        public float CellSize = 1f;
        public bool HasBaseMapWorldBounds;
        public Bounds BaseMapWorldBounds;

        // Geography engine output. TerrainLevelMap is authored per-cell and
        // must reach GeneratedWorldData unmodified; HasAuthoredGeography tells
        // downstream post-processing (level normalisation, shore-band expand)
        // to leave the authored data alone.
        public int[,] TerrainLevelMap;
        public bool ForceChunkFirst;
        public bool HasAuthoredGeography;
        public Vector2Int[] SpawnHints;
        public WorldGenerationReport GeographyReport;
    }
}
