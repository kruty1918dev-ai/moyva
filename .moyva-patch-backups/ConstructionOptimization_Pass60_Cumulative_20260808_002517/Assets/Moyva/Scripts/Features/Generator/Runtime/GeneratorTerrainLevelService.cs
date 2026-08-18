using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public sealed class GeneratorTerrainLevelService : IGeneratorTerrainLevelService
    {
        private int[,] _levelMap;
        private float[,] _surfaceHeightMap;
        private bool _hasExplicitSurfaceHeightMap;

        public bool HasLevelMap => _levelMap != null;
        public bool HasSurfaceHeightMap => _surfaceHeightMap != null;
        public bool HasExplicitSurfaceHeightMap => _surfaceHeightMap != null && _hasExplicitSurfaceHeightMap;
        public int Width => _levelMap?.GetLength(0) ?? _surfaceHeightMap?.GetLength(0) ?? 0;
        public int Height => _levelMap?.GetLength(1) ?? _surfaceHeightMap?.GetLength(1) ?? 0;
        public HillLevelDataMap CurrentHillLevelData { get; private set; }

        public void Clear()
        {
            _levelMap = null;
            _surfaceHeightMap = null;
            _hasExplicitSurfaceHeightMap = false;
            CurrentHillLevelData = null;
        }

        public void SetLevelMap(int[,] levelMap)
        {
            _levelMap = CloneLevelMap(levelMap);
            _surfaceHeightMap = BuildSurfaceHeightMapFromLevels(_levelMap);
            _hasExplicitSurfaceHeightMap = false;
            CurrentHillLevelData = null;
        }

        public void SetSurfaceHeightMap(float[,] surfaceHeightMap)
        {
            _surfaceHeightMap = CloneSurfaceHeightMap(surfaceHeightMap);
            _hasExplicitSurfaceHeightMap = _surfaceHeightMap != null;
        }

        public void SetHillLevelData(HillLevelDataMap data)
        {
            CurrentHillLevelData = data?.Clone();
            if (data == null)
            {
                _levelMap = null;
                _surfaceHeightMap = null;
                _hasExplicitSurfaceHeightMap = false;
                return;
            }

            var levels = new int[data.Width, data.Height];
            for (int x = 0; x < data.Width; x++)
            for (int y = 0; y < data.Height; y++)
                levels[x, y] = Mathf.Max(0, data.GetTile(x, y).Level);

            _levelMap = levels;
            _surfaceHeightMap = BuildSurfaceHeightMapFromLevels(_levelMap);
            _hasExplicitSurfaceHeightMap = false;
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

        public bool TryGetSurfaceHeight(Vector2Int position, out float surfaceY)
        {
            if (_surfaceHeightMap == null
                || position.x < 0 || position.x >= _surfaceHeightMap.GetLength(0)
                || position.y < 0 || position.y >= _surfaceHeightMap.GetLength(1))
            {
                surfaceY = 0f;
                return false;
            }

            surfaceY = _surfaceHeightMap[position.x, position.y];
            return true;
        }

        public float[,] CopySurfaceHeightMap() => CloneSurfaceHeightMap(_surfaceHeightMap);

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

        private static float[,] BuildSurfaceHeightMapFromLevels(int[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var clone = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                clone[x, y] = Mathf.Max(0, source[x, y]);

            return clone;
        }

        private static float[,] CloneSurfaceHeightMap(float[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var clone = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float value = source[x, y];
                clone[x, y] = float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
            }

            return clone;
        }
    }
}
