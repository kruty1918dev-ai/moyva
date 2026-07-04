using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Calculates map tiles covered by fog reveal shapes.
    /// Contains no fog state, counters, signals, or visual update logic.
    /// </summary>
    internal static class FogRevealShapeTileCalculator
    {
        public static IReadOnlyList<Vector2Int> ComputePixelCircleTiles(Vector2Int origin, int radius, int width, int height)
            => ComputeShapeTiles(origin, radius, FogRevealShape.PixelCircle, width, height);

        public static IReadOnlyList<Vector2Int> ComputeShapeTiles(Vector2Int origin, int radius, FogRevealShape shape, int width, int height)
        {
            var result = new List<Vector2Int>();
            int safeRadius = Mathf.Max(0, radius);
            float radiusWithCellCoverage = safeRadius + 0.5f;
            float sqrRadius = radiusWithCellCoverage * radiusWithCellCoverage;

            for (int dx = -safeRadius; dx <= safeRadius; dx++)
            {
                for (int dy = -safeRadius; dy <= safeRadius; dy++)
                {
                    if (!IsInsideShape(dx, dy, safeRadius, sqrRadius, shape))
                        continue;

                    var tile = new Vector2Int(origin.x + dx, origin.y + dy);
                    if (IsInBounds(tile, width, height))
                        result.Add(tile);
                }
            }

            return result;
        }

        private static bool IsInsideShape(int dx, int dy, int radius, float sqrRadius, FogRevealShape shape)
        {
            return shape switch
            {
                FogRevealShape.Square => Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= radius,
                FogRevealShape.Diamond => Mathf.Abs(dx) + Mathf.Abs(dy) <= radius,
                _ => dx * dx + dy * dy <= sqrRadius,
            };
        }

        private static bool IsInBounds(Vector2Int tile, int width, int height)
            => tile.x >= 0 && tile.x < width && tile.y >= 0 && tile.y < height;
    }
}
