using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public sealed class GeneratorTerrainLevelService : IGeneratorTerrainLevelService
    {
        private int[,] _levelMap;

        public bool HasLevelMap => _levelMap != null;
        public int Width => _levelMap?.GetLength(0) ?? 0;
        public int Height => _levelMap?.GetLength(1) ?? 0;
        public HillLevelDataMap CurrentHillLevelData { get; private set; }

        public void Clear()
        {
            _levelMap = null;
            CurrentHillLevelData = null;
        }

        public void SetLevelMap(int[,] levelMap)
        {
            _levelMap = CloneLevelMap(levelMap);
            CurrentHillLevelData = null;
        }

        public void SetHillLevelData(HillLevelDataMap data)
        {
            CurrentHillLevelData = data?.Clone();
            if (data == null)
            {
                _levelMap = null;
                return;
            }

            var levels = new int[data.Width, data.Height];
            for (int x = 0; x < data.Width; x++)
            for (int y = 0; y < data.Height; y++)
                levels[x, y] = Mathf.Max(0, data.GetTile(x, y).Level);

            _levelMap = levels;
        }

        public bool TryGetLevel(Vector2Int position, out int level)
        {
            if (_levelMap == null
                || position.x < 0 || position.x >= Width
                || position.y < 0 || position.y >= Height)
            {
                level = 0;
                return false;
            }

            level = Mathf.Max(0, _levelMap[position.x, position.y]);
            return true;
        }

        public int GetLevelOrDefault(Vector2Int position, int fallback = 0)
            => TryGetLevel(position, out int level) ? level : Mathf.Max(0, fallback);

        public int[,] CopyLevelMap() => CloneLevelMap(_levelMap);

        private static int[,] CloneLevelMap(int[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var clone = new int[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                clone[x, y] = Mathf.Max(0, source[x, y]);

            return clone;
        }
    }
}