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
