using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IGeneratorTerrainLevelService
    {
        bool HasLevelMap { get; }
        int Width { get; }
        int Height { get; }
        HillLevelDataMap CurrentHillLevelData { get; }

        void Clear();
        void SetLevelMap(int[,] levelMap);
        void SetHillLevelData(HillLevelDataMap data);
        bool TryGetLevel(Vector2Int position, out int level);
        int GetLevelOrDefault(Vector2Int position, int fallback = 0);
        int[,] CopyLevelMap();
    }
}