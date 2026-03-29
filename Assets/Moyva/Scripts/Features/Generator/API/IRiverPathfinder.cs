using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IRiverPathfinder
    {
        /// <summary>
        /// Шукає шлях для річки від старту до фінішу, базуючись на перепадах висот.
        /// </summary>
        List<Vector2Int> FindRiverPath(Vector2Int startPoint, Vector2Int endPoint, string[,] biomeMap, float[,] heightMap, int width, int height, RiverDataConfig riverConfig);
    }
}