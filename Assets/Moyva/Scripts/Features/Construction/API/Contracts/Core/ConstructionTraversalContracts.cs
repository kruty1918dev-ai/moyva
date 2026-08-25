using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionGateStateService
    {
        bool IsGateOpen(Vector2Int position);
        bool TrySetGateOpen(
            Vector2Int position,
            bool isOpen,
            out float transitionSeconds,
            out string reason);
        bool CanUnitPassGate(
            Vector2Int position,
            string unitOwnerId,
            out string reason);
        bool TryEnsureOpenForUnit(
            Vector2Int position,
            string unitOwnerId,
            out string reason);
    }

    public interface IConstructionUnitTraversalQuery
    {
        bool CanTraverseOccupiedConstructionCell(
            string unitId,
            Vector2Int position,
            bool openGateIfNeeded,
            out string reason);
    }

    public interface IConstructionUnitGarrisonRuntime
    {
        bool TryEnterGarrison(
            string unitId,
            Vector2Int buildingPosition,
            out string reason);
        bool TryExitGarrison(
            string unitId,
            Vector2Int targetPosition,
            out string reason);
        bool TryRestoreGarrison(
            string unitId,
            Vector2Int buildingPosition,
            out string reason);
        bool TryExitGarrisonNear(
            string unitId,
            Vector2Int origin,
            int maxRadius,
            out Vector2Int targetPosition,
            out string reason);
        bool IsGarrisoned(string unitId);
        string GetUnitOwnerId(string unitId);
    }

    public interface IBuildingGarrisonService
    {
        bool TryGarrisonUnit(
            Vector2Int buildingPosition,
            string unitId,
            out string reason);
        bool TryUngarrisonUnit(
            Vector2Int buildingPosition,
            string unitId,
            Vector2Int targetPosition,
            out string reason);
        IReadOnlyList<string> GetGarrisonedUnits(
            Vector2Int buildingPosition);
        bool TryGetGarrisonStatus(
            Vector2Int buildingPosition,
            out int occupied,
            out int capacity);
    }
}
