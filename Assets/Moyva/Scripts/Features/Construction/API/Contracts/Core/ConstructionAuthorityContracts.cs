using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionPlacedBuildingDestruction
    {
        bool TryDestroyPlacedBuilding(
            Vector2Int position,
            string cause = null);
    }

    public interface IConstructionRuntimeAuthorityQuery
    {
        bool IsAuthoritativeRuntime { get; }
    }

    public interface IConstructionBuildingOwnershipQuery
    {
        bool TryGetPlacedBuildingOwner(
            Vector2Int position,
            out string ownerId);
    }

    public interface IConstructionOwnershipTransfer
    {
        bool TryTransferPlacedBuildingOwner(
            Vector2Int position,
            string previousOwnerId,
            string nextOwnerId,
            out string reason);
    }

    public readonly struct ConstructionBuildingCombatTarget
    {
        public ConstructionBuildingCombatTarget(
            string entityId,
            string buildingId,
            Vector2Int position,
            string ownerId)
        {
            EntityId = entityId ?? string.Empty;
            BuildingId = buildingId ?? string.Empty;
            Position = position;
            OwnerId = ownerId ?? string.Empty;
        }

        public string EntityId { get; }
        public string BuildingId { get; }
        public Vector2Int Position { get; }
        public string OwnerId { get; }
    }

    public interface IConstructionBuildingCombatTargetQuery
    {
        bool TryGetCombatTarget(
            string entityId,
            out ConstructionBuildingCombatTarget target);
    }

    public interface IConfirmedConstructionPlacementApplier
    {
        bool TryApplyConfirmedPlacement(
            string buildingId,
            Vector2Int position,
            string ownerId);
    }

    public interface IConfirmedConstructionDemolitionApplier
    {
        bool TryApplyConfirmedDemolition(
            Vector2Int position,
            string ownerId);
    }

    public readonly struct ConstructionPlacementCommitIntent
    {
        public ConstructionPlacementCommitIntent(
            Vector2Int? relocationSourcePosition = null,
            string satisfiedReplacementBuildingId = null,
            ConstructionRotation rotation = ConstructionRotation.Degrees0)
        {
            RelocationSourcePosition = relocationSourcePosition;
            SatisfiedReplacementBuildingId = satisfiedReplacementBuildingId;
            Rotation = ConstructionRotationUtility.Normalize((int)rotation);
        }

        public Vector2Int? RelocationSourcePosition { get; }
        public bool HasRelocationSource => RelocationSourcePosition.HasValue;
        public string SatisfiedReplacementBuildingId { get; }
        public ConstructionRotation Rotation { get; }

        public static ConstructionPlacementCommitIntent None =>
            new ConstructionPlacementCommitIntent();
    }

    public interface IConstructionPendingPlacementIntentSource
    {
        bool TryGetPendingPlacementIntent(
            Vector2Int position,
            out ConstructionPlacementCommitIntent intent);
    }

    public interface IAuthoritativeConstructionPlacementExecutor
    {
        bool TryPlaceAuthoritatively(
            string buildingId,
            Vector2Int position,
            string ownerId,
            ConstructionPlacementCommitIntent intent);
    }

    public interface IConstructionPrepaidPlacementExecutor
    {
        bool TryPlacePrepaidAuthoritatively(
            string buildingId,
            Vector2Int position,
            string ownerId,
            ConstructionPlacementCommitIntent intent);
    }

    public interface IConfirmedConstructionPlacementIntentApplier
    {
        bool TryApplyConfirmedPlacement(
            string buildingId,
            Vector2Int position,
            string ownerId,
            ConstructionPlacementCommitIntent intent);
    }
}
