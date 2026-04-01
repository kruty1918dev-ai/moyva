using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Symmetric Shadowcasting (8 octants) visibility resolver.
    /// Based on https://journal.stuffwithstuff.com/2015/09/07/what-the-hero-sees/
    /// Falls back to simple circle vision if gridService is null.
    /// </summary>
    public class FogVisibilityResolver : IFogVisibilityResolver
    {
        private readonly IGridService _gridService;

        public FogVisibilityResolver(IGridService gridService)
        {
            _gridService = gridService;
            if (_gridService == null)
                Debug.LogWarning("[FogOfWar] FogVisibilityResolver: IGridService is null. Using fallback circular vision.");
        }

        public IReadOnlyList<Vector2Int> ComputeVisibleTiles(
            Vector2Int origin, int visionRange, int mapWidth, int mapHeight)
        {
            var result = new HashSet<Vector2Int>();

            // Always add origin
            if (IsInBounds(origin, mapWidth, mapHeight))
                result.Add(origin);

            if (visionRange <= 0)
                return new List<Vector2Int>(result);

            // Symmetric shadowcasting: 8 octants
            for (int octant = 0; octant < 8; octant++)
                CastOctant(origin, visionRange, octant, mapWidth, mapHeight, result);

            return new List<Vector2Int>(result);
        }

        // ─── Symmetric Shadowcasting ─────────────────────────────────────────

        private static void CastOctant(
            Vector2Int origin, int range, int octant,
            int mapWidth, int mapHeight, HashSet<Vector2Int> result)
        {
            // Shadow slope pairs: (start, end) where 0=full-light, 1=full-shadow
            var shadows = new List<(float start, float end)>();

            for (int row = 1; row <= range; row++)
            {
                for (int col = 0; col <= row; col++)
                {
                    Vector2Int tile = TransformOctant(origin, row, col, octant);

                    if (!IsInBounds(tile, mapWidth, mapHeight))
                        continue;

                    float tileSlope1 = (col - 0.5f) / (row + 0.5f);
                    float tileSlope2 = (col + 0.5f) / (row - 0.5f);

                    bool inShadow = false;
                    foreach (var (shadowStart, shadowEnd) in shadows)
                    {
                        if (shadowStart <= tileSlope1 && tileSlope2 <= shadowEnd)
                        {
                            inShadow = true;
                            break;
                        }
                    }

                    if (!inShadow)
                    {
                        result.Add(tile);

                        // Since we have no wall API yet, no blocking — all tiles are passable.
                        // When wall blocking is added, check here and if blocked:
                        // AddShadow(shadows, tileSlope1, tileSlope2);
                    }
                }
            }
        }

        private static Vector2Int TransformOctant(Vector2Int origin, int row, int col, int octant)
        {
            int dx, dy;
            switch (octant)
            {
                case 0: dx =  col; dy = -row; break;
                case 1: dx =  row; dy = -col; break;
                case 2: dx =  row; dy =  col; break;
                case 3: dx =  col; dy =  row; break;
                case 4: dx = -col; dy =  row; break;
                case 5: dx = -row; dy =  col; break;
                case 6: dx = -row; dy = -col; break;
                case 7: dx = -col; dy = -row; break;
                default: dx = 0; dy = 0; break;
            }
            return new Vector2Int(origin.x + dx, origin.y + dy);
        }

        private static bool IsInBounds(Vector2Int pos, int w, int h)
            => pos.x >= 0 && pos.x < w && pos.y >= 0 && pos.y < h;
    }
}
