using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IRiverPathfinder
    {
        /// <summary>
        /// Шукає шлях для річки від старту до фінішу, базуючись на перепадах висот.
        /// </summary>
        List<Vector2Int> FindRiverPath(Vector2Int startPoint, Vector2Int endPoint, float[,] heightMap, int width, int height);

        /// <summary>
        /// Шукає шлях для річки з урахуванням уже прокладених русел, щоб нові річки рідше перетиналися.
        /// </summary>
        List<Vector2Int> FindRiverPath(
            Vector2Int startPoint,
            Vector2Int endPoint,
            float[,] heightMap,
            int width,
            int height,
            int[,] riverUsageMap,
            float usedCellPenalty,
            float nearRiverPenalty,
            int nearRiverRadius);
    }
}