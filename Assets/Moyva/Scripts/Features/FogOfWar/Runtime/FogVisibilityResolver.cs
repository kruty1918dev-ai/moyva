using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Height-aware visibility resolver.
    /// Computes a non-uniform field of view based on base unit vision,
    /// terrain height bonus/penalty, and line-of-sight over the heightmap.
    /// </summary>
    internal sealed class FogVisibilityResolver : IFogVisibilityResolver
    {
        private readonly IGridService _gridService;
        private readonly IHeightAwareVisionService _heightVisionService;
        private readonly FogOfWarSettings _settings;

        public FogVisibilityResolver(
            IGridService gridService,
            IHeightAwareVisionService heightVisionService,
            [Zenject.InjectOptional] FogOfWarSettings settings = null)
        {
            _gridService = gridService;
            _heightVisionService = heightVisionService;
            _settings = settings;
            if (_gridService == null)
                Debug.LogWarning("[FogOfWar] FogVisibilityResolver: IGridService is null. Using provided map bounds only.");
        }

        public void SetHeightMap(float[,] heightMap)
        {
            _heightVisionService?.SetHeightMap(heightMap);
        }

        public IReadOnlyList<Vector2Int> ComputeVisibleTiles(
            Vector2Int origin, int visionRange, int mapWidth, int mapHeight)
        {
            var result = new HashSet<Vector2Int>();
            int maxRange = _settings != null ? _settings.MaxVisionRange : 12;
            int safeRange = Mathf.Max(1, visionRange);

            if (IsInBounds(origin, mapWidth, mapHeight))
                result.Add(origin);

            if (safeRange <= 0)
                return new List<Vector2Int>(result);

            int searchRadius = _heightVisionService != null
                ? _heightVisionService.GetSearchRadius(origin, safeRange, maxRange)
                : Mathf.Clamp(safeRange, 1, maxRange);

            for (int dx = -searchRadius; dx <= searchRadius; dx++)
            {
                for (int dy = -searchRadius; dy <= searchRadius; dy++)
                {
                    var target = new Vector2Int(origin.x + dx, origin.y + dy);
                    if (!IsInBounds(target, mapWidth, mapHeight))
                        continue;

                    if (target == origin)
                        continue;

                    if (_heightVisionService != null)
                    {
                        if (_heightVisionService.IsTargetVisible(origin, target, safeRange, maxRange))
                            result.Add(target);

                        continue;
                    }

                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= safeRange)
                        result.Add(target);
                }
            }

            return new List<Vector2Int>(result);
        }

        private static bool IsInBounds(Vector2Int pos, int w, int h)
            => pos.x >= 0 && pos.x < w && pos.y >= 0 && pos.y < h;
    }
}
