using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface IChunkFirstObjectSpawner
    {
        int Spawn(GeneratedWorldData worldData);
        void Clear();
        /// <summary>
        /// Destroys spawned non-building prop objects anchored to the given
        /// cells and excludes those cells from future spawns until the next
        /// world build. Building samples are never cleared.
        /// </summary>
        int ClearPropsInCells(IReadOnlyList<Vector2Int> cells);
    }
}
