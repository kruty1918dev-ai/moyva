using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public interface IFogOwnerStateReader
    {
        FogStateType GetFogState(string ownerId, Vector2Int position);
        bool IsVisible(string ownerId, Vector2Int position);
        bool IsExplored(string ownerId, Vector2Int position);
    }

    public interface IFogOwnerVisionSourceRegistry
    {
        void RegisterUnit(string ownerId, string unitId, Vector2Int position, int visionRange);
        void RegisterFixedVisionArea(string ownerId, string areaId, Vector2Int position, int visionRange, FogRevealShape shape);
        void RevealArea(string ownerId, Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null);
        void UpdateUnitPosition(string ownerId, string unitId, Vector2Int newPosition);
        void UpdateUnitVisionRange(string ownerId, string unitId, int visionRange);
        void UnregisterUnit(string ownerId, string unitId);
        void TransferFixedVisionAreaOwner(string areaId, string previousOwnerId, string newOwnerId);
    }

    public interface IFogOwnerExplorationSnapshotStore
    {
        IReadOnlyCollection<string> GetKnownFogOwnerIds();
        bool[,] GetExploredSnapshot(string ownerId);
        void LoadFromSnapshot(string ownerId, bool[,] explored);
    }
}
