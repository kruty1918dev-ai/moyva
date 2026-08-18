using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public enum UnitRecruitmentQueueStatus
    {
        Training = 0,
        Ready = 1,
    }

    public readonly struct UnitRecruitmentQueueItemSnapshot
    {
        public UnitRecruitmentQueueItemSnapshot(
            long queueId,
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            string unitTypeId,
            int completedTurns,
            int trainingTurns,
            long enqueuedGlobalTurn,
            UnitRecruitmentQueueStatus status)
            : this(
                queueId,
                ownerId,
                recruitingBuildingPosition,
                string.Empty,
                unitTypeId,
                completedTurns,
                trainingTurns,
                enqueuedGlobalTurn,
                enqueuedGlobalTurn,
                status)
        {
        }

        public UnitRecruitmentQueueItemSnapshot(
            long queueId,
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            string recruitingBuildingId,
            string unitTypeId,
            int completedTurns,
            int trainingTurns,
            long enqueuedGlobalTurn,
            long lastProgressGlobalTurn,
            UnitRecruitmentQueueStatus status)
        {
            QueueId = queueId;
            OwnerId = ownerId ?? string.Empty;
            RecruitingBuildingPosition = recruitingBuildingPosition;
            RecruitingBuildingId = recruitingBuildingId ?? string.Empty;
            UnitTypeId = unitTypeId ?? string.Empty;
            CompletedTurns = completedTurns < 0 ? 0 : completedTurns;
            TrainingTurns = trainingTurns < 1 ? 1 : trainingTurns;
            EnqueuedGlobalTurn = enqueuedGlobalTurn < 1 ? 1 : enqueuedGlobalTurn;
            LastProgressGlobalTurn = lastProgressGlobalTurn < EnqueuedGlobalTurn
                ? EnqueuedGlobalTurn
                : lastProgressGlobalTurn;
            Status = CompletedTurns >= TrainingTurns
                ? UnitRecruitmentQueueStatus.Ready
                : UnitRecruitmentQueueStatus.Training;
        }

        public long QueueId { get; }
        public string OwnerId { get; }
        public Vector2Int RecruitingBuildingPosition { get; }
        public string RecruitingBuildingId { get; }
        public string UnitTypeId { get; }
        public int CompletedTurns { get; }
        public int TrainingTurns { get; }
        public long EnqueuedGlobalTurn { get; }
        public long LastProgressGlobalTurn { get; }
        public UnitRecruitmentQueueStatus Status { get; }
        public bool IsReady => Status == UnitRecruitmentQueueStatus.Ready;
        public int RemainingTurns => IsReady
            ? 0
            : System.Math.Max(0, TrainingTurns - CompletedTurns);
    }

    public readonly struct UnitRecruitmentDeploymentTileSnapshot
    {
        public UnitRecruitmentDeploymentTileSnapshot(
            Vector2Int position,
            bool isValid,
            string reason)
        {
            Position = position;
            IsValid = isValid;
            Reason = reason ?? string.Empty;
        }

        public Vector2Int Position { get; }
        public bool IsValid { get; }
        public string Reason { get; }
    }

    public interface IUnitRecruitmentService
    {
        bool TryEnqueue(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            string unitTypeId,
            out string reason);

        IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetQueue(
            string ownerId,
            Vector2Int recruitingBuildingPosition);

        bool TryPeekReady(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            out UnitRecruitmentQueueItemSnapshot item);

        /// <summary>
        /// Returns the ready head from each recruitment building owned by
        /// <paramref name="ownerId"/>. Ready jobs remain queued until an explicit
        /// deployment succeeds.
        /// </summary>
        IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetReadyItems(
            string ownerId);

        /// <summary>
        /// Returns every tile inside the recruiting building deployment radius,
        /// including invalid tiles with a player-facing rejection reason.
        /// </summary>
        IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> GetDeploymentTiles(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            long queueId);

        /// <summary>
        /// Explicitly deploys a ready queue head onto a selected valid tile.
        /// This is the only recruitment path that creates a unit after P24A.
        /// </summary>
        bool TryDeployReady(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            long queueId,
            Vector2Int targetPosition,
            out string unitId,
            out string reason);
    }

    /// <summary>
    /// Save/load boundary for already-paid recruitment queues. Restore must never
    /// re-run economy consumption or enqueue authority checks.
    /// </summary>
    public interface IUnitRecruitmentStateStore
    {
        IReadOnlyList<UnitRecruitmentQueueItemSnapshot> CaptureState();
        void RestoreState(IReadOnlyList<UnitRecruitmentQueueItemSnapshot> items);
    }
}
