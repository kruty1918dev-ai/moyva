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

    /// <summary>
    /// One recruitment deficit: a missing resource or free settlement
    /// population. Available already excludes active reservations; Missing is
    /// the amount still required for the action to succeed.
    /// </summary>
    public readonly struct UnitRecruitmentShortage
    {
        public UnitRecruitmentShortage(
            string resourceId,
            bool isPopulation,
            float required,
            float available,
            float reserved)
        {
            ResourceId = resourceId;
            IsPopulation = isPopulation;
            Required = required;
            Available = available;
            Reserved = reserved;
        }

        public string ResourceId { get; }
        public bool IsPopulation { get; }
        public float Required { get; }
        public float Available { get; }
        public float Reserved { get; }
        public float Missing => Required > Available ? Required - Available : 0f;
    }

    /// <summary>Read-only recruitment eligibility; never advances queues or reserves resources.</summary>
    public interface IUnitRecruitmentQuery
    {
        IReadOnlyList<UnitRecruitmentOption> GetOptions(string ownerId);
        bool CanEnqueue(string ownerId, Vector2Int source, string unitTypeId, out string reason);

        /// <summary>
        /// Evaluates every population/resource deficit for one enqueue request
        /// instead of stopping at the first failure. Returns true when the
        /// enqueue is possible. Non-shortage blockers (eligibility, missing
        /// funding settlement) are reported via reason; shortages may still
        /// contain already-collected deficits in that case.
        /// </summary>
        bool TryGetEnqueueShortages(
            string ownerId,
            Vector2Int source,
            string unitTypeId,
            out IReadOnlyList<UnitRecruitmentShortage> shortages,
            out string reason);
    }
}
