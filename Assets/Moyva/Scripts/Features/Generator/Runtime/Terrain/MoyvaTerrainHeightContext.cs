using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    [DisallowMultipleComponent]
    public sealed class MoyvaTerrainHeightContext : MonoBehaviour
    {
        private const string LogTag = "[MoyvaTWCChunks]";
        private int[,] _terrainLevelMap;
        private int _heightStep = 1;

        public bool IsConfigured => _terrainLevelMap != null;

        public void Configure(int[,] terrainLevelMap, int heightStep)
        {
            _terrainLevelMap = terrainLevelMap;
            _heightStep = Mathf.Max(1, heightStep);
            Debug.Log($"{LogTag} Height context configured. map={FormatMap(terrainLevelMap)}, heightStep={_heightStep}.");
        }

        public void Clear(string reason)
        {
            _terrainLevelMap = null;
            Debug.Log($"{LogTag} Height context cleared. reason='{reason}'.");
        }

        public bool TryGetTileHeight(Vector2 tilePosition, bool useDualGrid, out float height)
            => TryGetTileHeight(tilePosition, useDualGrid, null, out height);

        public bool TryGetTileHeight(Vector2 tilePosition, bool useDualGrid, Vector2[] contributingCells, out float height)
        {
            height = 0f;
            if (_terrainLevelMap == null)
                return false;

            int width = _terrainLevelMap.GetLength(0);
            int mapHeight = _terrainLevelMap.GetLength(1);
            if (width <= 0 || mapHeight <= 0)
                return false;

            int level = ResolveLevel(tilePosition, useDualGrid, contributingCells, width, mapHeight);
            height = level * _heightStep;
            return true;
        }

        private int ResolveLevel(Vector2 tilePosition, bool useDualGrid, Vector2[] contributingCells, int width, int height)
        {
            if (contributingCells != null && contributingCells.Length > 0)
                return ResolveMaxContributingLevel(contributingCells, width, height);

            if (useDualGrid)
            {
                float floorX = Mathf.Floor(tilePosition.x);
                float floorY = Mathf.Floor(tilePosition.y);
                return ResolveMaxContributingLevel(
                    new[]
                    {
                        new Vector2(floorX, floorY),
                        new Vector2(floorX + 1f, floorY),
                        new Vector2(floorX, floorY + 1f),
                        new Vector2(floorX + 1f, floorY + 1f)
                    },
                    width,
                    height);
            }

            return _terrainLevelMap[
                Mathf.Clamp(Mathf.RoundToInt(tilePosition.x), 0, width - 1),
                Mathf.Clamp(Mathf.RoundToInt(tilePosition.y), 0, height - 1)];
        }

        private int ResolveMaxContributingLevel(Vector2[] cells, int width, int height)
        {
            int maxLevel = int.MinValue;
            for (int i = 0; i < cells.Length; i++)
            {
                int x = Mathf.Clamp(Mathf.RoundToInt(cells[i].x), 0, width - 1);
                int y = Mathf.Clamp(Mathf.RoundToInt(cells[i].y), 0, height - 1);
                maxLevel = Mathf.Max(maxLevel, _terrainLevelMap[x, y]);
            }

            return maxLevel == int.MinValue ? 0 : maxLevel;
        }

        private static string FormatMap(int[,] terrainLevelMap)
            => terrainLevelMap == null ? "null" : $"{terrainLevelMap.GetLength(0)}x{terrainLevelMap.GetLength(1)}";
    }
}
