using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface IChunkTerrainMeshBuilder
    {
        int Build(
            Transform chunkRoot,
            ChunkBuildArea area,
            IReadOnlyDictionary<Vector2Int, ResolvedTileComposition> resolvedCells,
            IResolvedTileMeshSource meshSource);
    }
}
