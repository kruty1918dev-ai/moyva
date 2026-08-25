using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Interactive construction session surface. All mutations are handled by
    /// the canonical ConstructionService singleton.
    /// </summary>
    public interface IConstructionSessionCommands
    {
        BuildingPlacementState State { get; }
        bool IsDemolishMode { get; }

        void SelectBuilding(string buildingId);
        string GetSelectedBuildingId();
        void SetActiveOwner(string ownerId);
        string GetActiveOwner();

        bool TryPreviewAt(Vector2Int position);
        bool HasPendingPlacementAt(Vector2Int position);
        bool TryGetPendingBuildingIdAt(
            Vector2Int position,
            out string buildingId);
        IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements();
        bool TryMovePendingPlacement(
            Vector2Int fromPosition,
            Vector2Int toPosition);
        bool RemovePendingAt(Vector2Int position);
        bool TryGetPendingPlacementStatus(
            Vector2Int position,
            out ConstructionPendingPlacementStatus status);
        ConstructionResourceProjection GetResourceProjection(
            Vector2Int position);

        void Confirm();
        void Cancel();
        void UndoLast();
        void RedoLast();
        void ToggleDemolishMode();
        bool TryDemolishAt(Vector2Int position);
        string GetLastActionMessage();
    }

    public interface IConstructionPendingUndoBatch
    {
        void BeginPendingUndoBatch(string reason = null);
        void EndPendingUndoBatch();
    }

    public interface IConstructionRotationService
    {
        ConstructionRotation SelectedRotation { get; }
        bool RotateSelectedClockwise();
        bool TryGetPendingRotation(
            Vector2Int position,
            out ConstructionRotation rotation);
    }

    public interface IConstructionBootstrapQuery
    {
        bool RequiresInitialCastle(
            string ownerId,
            out string castleBuildingId);

        bool IsCastleBuilding(string buildingId);
    }
}
