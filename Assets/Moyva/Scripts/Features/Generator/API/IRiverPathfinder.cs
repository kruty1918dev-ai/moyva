using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IRiverPathfinder
    {
        /// <summary>
        /// Шукає шлях для річки від старту до фінішу, базуючись на перепадах висот.
        /// </summary>
        List<Vector2Int> FindRiverPath(Vector2Int start, Vector2Int end, float[,] heightMap, int width, int height);
    }
}