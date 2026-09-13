using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public readonly struct UnitRecruitmentOption
    {
        public UnitRecruitmentOption(Vector2Int source, string unitTypeId, float totalCost = 0,
            int queueSlots = 0, int population = 0, int trainingTurns = 0)
        { Source = source; UnitTypeId = unitTypeId; TotalCost = totalCost;
            QueueSlots = queueSlots; AvailablePopulation = population; TrainingTurns = trainingTurns; }
        public Vector2Int Source { get; }
        public string UnitTypeId { get; }
        public float TotalCost { get; }
        public int QueueSlots { get; }
        public int AvailablePopulation { get; }
        public int TrainingTurns { get; }
    }

    /// <summary>Read-only recruitment eligibility; never advances queues or reserves resources.</summary>
    public interface IUnitRecruitmentQuery
    {
        IReadOnlyList<UnitRecruitmentOption> GetOptions(string ownerId);
        bool CanEnqueue(string ownerId, Vector2Int source, string unitTypeId, out string reason);
    }
}
