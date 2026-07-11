using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IGeneratorTerrainLevelService
    {
        bool HasLevelMap { get; }
        bool HasSurfaceHeightMap { get; }
        bool HasExplicitSurfaceHeightMap { get; }
        int Width { get; }
        int Height { get; }
        HillLevelDataMap CurrentHillLevelData { get; }

        void Clear();
        void SetLevelMap(int[,] levelMap);
        void SetSurfaceHeightMap(float[,] surfaceHeightMap);
        void SetHillLevelData(HillLevelDataMap data);
        bool TryGetLevel(Vector2Int position, out int level);
        bool TryGetSurfaceHeight(Vector2Int position, out float surfaceY);
        int GetLevelOrDefault(Vector2Int position, int fallback = 0);
        int[,] CopyLevelMap();
        float[,] CopySurfaceHeightMap();
    }
}
