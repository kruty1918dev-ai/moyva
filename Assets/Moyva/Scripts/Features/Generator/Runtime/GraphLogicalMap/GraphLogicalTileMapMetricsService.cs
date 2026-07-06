using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMapMetricsService : IGraphLogicalTileMapMetricsService
    {
        public Dictionary<string, int> CountValues(string[,] grid, out int emptyCount, out int occupiedCount)
        {
            emptyCount = 0;
            occupiedCount = 0;
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int x = 0; x < grid.GetLength(0); x++)
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                string value = GraphLogicalTileMapText.Normalize(grid[x, y]);
                if (value == "<empty>")
                    emptyCount++;
                else
                    occupiedCount++;
                counts.TryGetValue(value, out int count);
                counts[value] = count + 1;
            }

            return counts;
        }

        public ulong ComputeHash(string[,] grid)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            for (int y = 0; y < grid.GetLength(1); y++)
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                string value = GraphLogicalTileMapText.Normalize(grid[x, y]);
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= prime;
                }

                hash ^= 31;
                hash *= prime;
            }

            return hash;
        }

        public int CountRegions(string[,] grid, Func<string, bool> include)
        {
            var visited = new bool[grid.GetLength(0), grid.GetLength(1)];
            var queue = new Queue<Vector2Int>();
            int regions = 0;
            for (int x = 0; x < grid.GetLength(0); x++)
            for (int y = 0; y < grid.GetLength(1); y++)
                if (TryStartRegion(grid, include, visited, queue, x, y))
                    regions++;
            return regions;
        }

        public int CountEdgeTransitions(string[,] grid)
        {
            int transitions = 0;
            for (int x = 0; x < grid.GetLength(0); x++)
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                string value = GraphLogicalTileMapText.Normalize(grid[x, y]);
                if (x + 1 < grid.GetLength(0) && value != GraphLogicalTileMapText.Normalize(grid[x + 1, y]))
                    transitions++;
                if (y + 1 < grid.GetLength(1) && value != GraphLogicalTileMapText.Normalize(grid[x, y + 1]))
                    transitions++;
            }

            return transitions;
        }

        private static bool TryStartRegion(string[,] grid, Func<string, bool> include,
            bool[,] visited, Queue<Vector2Int> queue, int x, int y)
        {
            if (visited[x, y] || !include(grid[x, y]))
                return false;

            string value = GraphLogicalTileMapText.Normalize(grid[x, y]);
            visited[x, y] = true;
            queue.Enqueue(new Vector2Int(x, y));
            Flood(grid, include, visited, queue, value);
            return true;
        }

        private static void Flood(string[,] grid, Func<string, bool> include,
            bool[,] visited, Queue<Vector2Int> queue, string value)
        {
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                TryVisit(grid, include, visited, queue, value, cell.x + 1, cell.y);
                TryVisit(grid, include, visited, queue, value, cell.x - 1, cell.y);
                TryVisit(grid, include, visited, queue, value, cell.x, cell.y + 1);
                TryVisit(grid, include, visited, queue, value, cell.x, cell.y - 1);
            }
        }

        private static void TryVisit(string[,] grid, Func<string, bool> include, bool[,] visited,
            Queue<Vector2Int> queue, string value, int x, int y)
        {
            if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1))
                return;
            if (visited[x, y] || !include(grid[x, y]))
                return;
            if (GraphLogicalTileMapText.Normalize(grid[x, y]) != value)
                return;
            visited[x, y] = true;
            queue.Enqueue(new Vector2Int(x, y));
        }
    }
}
