using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Fired when a recruitment queue changes in a way that UI/AI may need to refresh.
    /// QueueId is zero only for whole-building invalidation such as demolition.
    /// </summary>
    public struct UnitRecruitmentQueueChangedSignal
    {
        public string OwnerId;
        public Vector2Int BuildingPosition;
        public long QueueId;
        public string UnitTypeId;
        public int CompletedTurns;
        public int TrainingTurns;
        public bool IsReady;
    }

    /// <summary>
    /// Fired once when a queue head transitions from Training to Ready.
    /// The unit is not spawned automatically.
    /// </summary>
    public struct UnitRecruitmentReadySignal
    {
        public string OwnerId;
        public Vector2Int BuildingPosition;
        public long QueueId;
        public string UnitTypeId;
    }

    /// <summary>
    /// Fired after explicit recruitment deployment successfully creates a unit
    /// and removes the corresponding ready queue head.
    /// </summary>
    public struct UnitRecruitmentDeployedSignal
    {
        public string OwnerId;
        public Vector2Int BuildingPosition;
        public long QueueId;
        public string UnitTypeId;
        public string UnitId;
        public Vector2Int Position;
    }
}
