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
        {
            QueueId = queueId;
            OwnerId = ownerId ?? string.Empty;
            RecruitingBuildingPosition = recruitingBuildingPosition;
            UnitTypeId = unitTypeId ?? string.Empty;
            CompletedTurns = completedTurns < 0 ? 0 : completedTurns;
            TrainingTurns = trainingTurns < 1 ? 1 : trainingTurns;
            EnqueuedGlobalTurn = enqueuedGlobalTurn < 1 ? 1 : enqueuedGlobalTurn;
            Status = status;
        }

        public long QueueId { get; }
        public string OwnerId { get; }
        public Vector2Int RecruitingBuildingPosition { get; }
        public string UnitTypeId { get; }
        public int CompletedTurns { get; }
        public int TrainingTurns { get; }
        public long EnqueuedGlobalTurn { get; }
        public UnitRecruitmentQueueStatus Status { get; }
        public bool IsReady => Status == UnitRecruitmentQueueStatus.Ready;
        public int RemainingTurns => IsReady
            ? 0
            : System.Math.Max(0, TrainingTurns - CompletedTurns);
    }

    /// <summary>
    /// Turn-authoritative, data-driven recruitment queue. P07 owns enqueue,
    /// economy consumption and training progress. Deployment/removal of ready
    /// entries is intentionally deferred to P08.
    /// </summary>
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
}
