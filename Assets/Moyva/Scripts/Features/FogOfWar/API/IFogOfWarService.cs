using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public interface IFogOfWarService
    {
        void Initialize(int width, int height);
        void RegisterUnit(string unitId, Vector2Int position, int visionRange);
        void UpdateUnitVisionRange(string unitId, int visionRange);
        void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape);
        void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null);
        void UpdateUnitPosition(string unitId, Vector2Int newPosition);
        void UnregisterUnit(string unitId);
        FogStateType GetFogState(Vector2Int position);
        bool IsVisible(Vector2Int position);
        bool IsExplored(Vector2Int position);
        bool[,] GetExploredSnapshot();
        void LoadFromSnapshot(bool[,] explored);
        IReadOnlyCollection<Vector2Int> GetLastDirtyTiles();
    }
}
