using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public readonly struct FogTileVisibility
    {
        public FogTileVisibility(Vector2Int tile, float visibility)
        {
            Tile = tile;
            Visibility = Mathf.Clamp01(visibility);
        }

        public Vector2Int Tile { get; }
        public float Visibility { get; }
        public bool IsVisible(float threshold) => Visibility >= Mathf.Clamp01(threshold);
    }

    public interface IFogVisibilityResolver
    {
        void SetHeightMap(float[,] heightMap);

        IReadOnlyList<Vector2Int> ComputeVisibleTiles(
            Vector2Int origin, int visionRange, int mapWidth, int mapHeight, FogVisionModifiers observerModifiers = default);

        IReadOnlyList<FogTileVisibility> ComputeVisibility(
            Vector2Int origin, int visionRange, int mapWidth, int mapHeight, FogVisionModifiers observerModifiers = default);
    }
}
