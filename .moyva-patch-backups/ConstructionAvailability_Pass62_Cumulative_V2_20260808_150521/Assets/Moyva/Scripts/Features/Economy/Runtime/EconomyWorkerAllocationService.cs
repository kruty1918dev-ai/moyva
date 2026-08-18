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
        private readonly List<BuildingAllocationCandidate> _allocationCandidates =
            new List<BuildingAllocationCandidate>();
        private readonly Dictionary<string, int> _availableWorkersByProfession =
            new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _professionOrderScratch =
            new List<string>();

        /// <summary>
        /// Allocate workers among buildings. Mutates <paramref name="state"/> in place.
        /// Returns (totalAvailable, totalAssigned).
        /// </summary>
        public (int available, int assigned) Allocate(EconomySettlementState state, EconomyRulesConfigSO rules)
        {
            if (state == null)
                return (0, 0);

            // Available workers = adult residents (age 16..59), grouped by profession.
            int available = 0;
            int untypedAvailable = 0;
            _availableWorkersByProfession.Clear();
            for (int i = 0; i < state.Residents.Count; i++)
            {
                EconomyResidentState resident = state.Residents[i];
                int age = resident.Age;
                if (age < 16 || age >= 60)
                    continue;

                available++;
                string profession = resident.ProfessionId?.Trim();
                if (string.IsNullOrWhiteSpace(profession))
                {
                    untypedAvailable++;
                    continue;
                }

                if (_availableWorkersByProfession.ContainsKey(profession))
                    _availableWorkersByProfession[profession]++;
                else
                    _availableWorkersByProfession[profession] = 1;
            }

            _professionOrderScratch.Clear();
            foreach (string profession in _availableWorkersByProfession.Keys)
                _professionOrderScratch.Add(profession);
            _professionOrderScratch.Sort(
                System.StringComparer.OrdinalIgnoreCase);

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

            for (int i = 0;
                 i < _allocationCandidates.Count && remaining > 0;
                 i++)
            {
                var building = _allocationCandidates[i].Building;
                int requested = building.RequiredWorkers;
                int toAssign;

                if (!string.IsNullOrWhiteSpace(building.WorkerTypeId))
                {
                    string profession = building.WorkerTypeId.Trim();
                    int professionAvailable =
                        _availableWorkersByProfession.TryGetValue(
                            profession,
                            out int count)
                            ? count
                            : 0;
                    toAssign = System.Math.Min(
                        requested,
                        professionAvailable);
                    if (toAssign > 0)
                    {
                        _availableWorkersByProfession[profession] =
                            professionAvailable - toAssign;
                    }
                }
                else
                {
                    toAssign = ConsumeAnyWorkers(
                        requested,
                        ref untypedAvailable);
                }

                building.AssignedWorkers = toAssign;
                string assignmentKey =
                    string.IsNullOrWhiteSpace(building.InstanceKey)
                        ? building.BuildingId
                        : building.InstanceKey;
                state.WorkerAssignments[assignmentKey] = toAssign;
                remaining -= toAssign;
                assigned += toAssign;
            }

            return (available, assigned);
        }

        private int ConsumeAnyWorkers(
            int requested,
            ref int untypedAvailable)
        {
            int remainingRequest = System.Math.Max(0, requested);
            int consumed = 0;

            int fromUntyped = System.Math.Min(
                remainingRequest,
                untypedAvailable);
            untypedAvailable -= fromUntyped;
            remainingRequest -= fromUntyped;
            consumed += fromUntyped;

            if (remainingRequest <= 0)
                return consumed;

            for (int index = 0;
                 index < _professionOrderScratch.Count
                 && remainingRequest > 0;
                 index++)
            {
                string profession = _professionOrderScratch[index];
                int available =
                    _availableWorkersByProfession[profession];
                int take = System.Math.Min(
                    remainingRequest,
                    available);
                if (take <= 0)
                    continue;

                _availableWorkersByProfession[profession] =
                    available - take;
                remainingRequest -= take;
                consumed += take;
            }

            return consumed;
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
