using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface IChunkBuildAreaPlanner
    {
        IReadOnlyList<ChunkBuildArea> Build(int width, int height, float cellSize, bool hasWorldBounds, Bounds worldBounds, int halo);
    }
}
