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
            Vector2Int origin, int visionRange, int mapWidth, int mapHeight, FogVisionModifiers observerModifiers = default)
        {
            var visibility = ComputeVisibility(origin, visionRange, mapWidth, mapHeight, observerModifiers);
            var result = new List<Vector2Int>(visibility.Count);
            float threshold = GetVisibilityThreshold();

            for (int i = 0; i < visibility.Count; i++)
            {
                if (visibility[i].IsVisible(threshold))
                    result.Add(visibility[i].Tile);
            }

            return result;
        }

        public IReadOnlyList<FogTileVisibility> ComputeVisibility(
            Vector2Int origin, int visionRange, int mapWidth, int mapHeight, FogVisionModifiers observerModifiers = default)
        {
            var result = new List<FogTileVisibility>();
            int maxRange = _settings != null ? _settings.MaxVisionRange : 12;
            int safeRange = Mathf.Max(1, visionRange);

            if (IsInBounds(origin, mapWidth, mapHeight))
                result.Add(new FogTileVisibility(origin, 1f));

            if (safeRange <= 0)
                return result;

            int searchRadius = _heightVisionService != null
                ? _heightVisionService.GetSearchRadius(origin, safeRange, maxRange, observerModifiers)
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
                        float visibility = _heightVisionService.GetVisibilityFactor(origin, target, safeRange, maxRange, observerModifiers);
                        if (visibility > 0f)
                            result.Add(new FogTileVisibility(target, visibility));

                        continue;
                    }

                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= safeRange)
                        result.Add(new FogTileVisibility(target, 1f));
                }
            }

            return result;
        }

        private static bool IsInBounds(Vector2Int pos, int w, int h)
            => pos.x >= 0 && pos.x < w && pos.y >= 0 && pos.y < h;

        private float GetVisibilityThreshold()
            => _settings != null ? Mathf.Clamp(_settings.TerrainVisibilityThreshold, 0.01f, 1f) : 0.5f;
    }
}
