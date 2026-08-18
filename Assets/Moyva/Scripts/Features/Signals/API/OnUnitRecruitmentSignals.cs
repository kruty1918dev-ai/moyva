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

    /// <summary>
    /// Fired when the player clicks the ready-unit indicator above the
    /// recruiting building. This signal only identifies the ready job; later
    /// deployment flow owns selection, preview, and confirmation.
    /// </summary>
    public struct UnitRecruitmentReadyIndicatorClickedSignal
    {
        public string OwnerId;
        public long QueueId;
        public string UnitTypeId;
        public string RecruitingBuildingId;
        public Vector2Int RecruitingBuildingPosition;
    }
}
