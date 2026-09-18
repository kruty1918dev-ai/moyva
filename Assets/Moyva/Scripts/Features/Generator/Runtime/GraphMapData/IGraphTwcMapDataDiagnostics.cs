using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcMapDataDiagnostics
    {
        IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; }
        GraphLogicalTileMap LastLogicalMap { get; }
        float LastCellSize { get; }
        int[,] LastTerrainLevelMap { get; }
        bool LastForceChunkFirst { get; }
        bool LastHasAuthoredGeography { get; }
        Vector2Int[] LastSpawnHints { get; }
        Geography.WorldGenerationReport LastGeographyReport { get; }
        bool TryGetLastBaseMapWorldBounds(out Bounds bounds);
    }
}
