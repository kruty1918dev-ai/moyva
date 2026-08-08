using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Distributes available workers across buildings by EconomyPriority (descending).
    /// Higher priority buildings get workers first.
    /// </summary>
    public sealed class EconomyWorkerAllocationService
    {
        private readonly List<BuildingAllocationCandidate> _allocationCandidates = new List<BuildingAllocationCandidate>();

        /// <summary>
        /// Allocate workers among buildings. Mutates <paramref name="state"/> in place.
        /// Returns (totalAvailable, totalAssigned).
        /// </summary>
        public (int available, int assigned) Allocate(EconomySettlementState state, EconomyRulesConfigSO rules)
        {
            if (state == null)
                return (0, 0);

            // Available workers = adult residents (age 16..59)
            int available = 0;
            for (int i = 0; i < state.Residents.Count; i++)
            {
                int age = state.Residents[i].Age;
                if (age >= 16 && age < 60)
                    available++;
            }

            // Clear previous assignments
            state.WorkerAssignments.Clear();
            for (int i = 0; i < state.Buildings.Count; i++)
                state.Buildings[i].AssignedWorkers = 0;

            _allocationCandidates.Clear();
            for (int i = 0; i < state.Buildings.Count; i++)
            {
                var building = state.Buildings[i];
                if (building.IsActive && building.RequiredWorkers > 0)
                    _allocationCandidates.Add(new BuildingAllocationCandidate(building, i));
            }

            _allocationCandidates.Sort(CompareAllocationCandidates);

            int remaining = available;
            int assigned = 0;

            for (int i = 0; i < _allocationCandidates.Count && remaining > 0; i++)
            {
                var building = _allocationCandidates[i].Building;
                int toAssign = building.RequiredWorkers;
                if (toAssign > remaining)
                    toAssign = remaining;

                building.AssignedWorkers = toAssign;
                state.WorkerAssignments[building.BuildingId] = toAssign;
                remaining -= toAssign;
                assigned += toAssign;
            }

            return (available, assigned);
        }

        private static int CompareAllocationCandidates(BuildingAllocationCandidate left, BuildingAllocationCandidate right)
        {
            int priorityComparison = right.Building.EconomyPriority.CompareTo(left.Building.EconomyPriority);
            return priorityComparison != 0
                ? priorityComparison
                : left.SourceIndex.CompareTo(right.SourceIndex);
        }

        private readonly struct BuildingAllocationCandidate
        {
            public BuildingAllocationCandidate(EconomyBuildingState building, int sourceIndex)
            {
                Building = building;
                SourceIndex = sourceIndex;
            }

            public EconomyBuildingState Building { get; }
            public int SourceIndex { get; }
        }
    }
}
