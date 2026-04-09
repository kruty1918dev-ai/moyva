using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public interface IHeightAwareVisionService
    {
        void SetHeightMap(float[,] heightMap);
        int GetSearchRadius(Vector2Int origin, int baseVisionRange, int maxVisionRange);
        bool IsTargetVisible(Vector2Int origin, Vector2Int target, int baseVisionRange, int maxVisionRange);
    }
}