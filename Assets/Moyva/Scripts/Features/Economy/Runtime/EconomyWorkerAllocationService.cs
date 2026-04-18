using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Distributes available workers across buildings by EconomyPriority (descending).
    /// Higher priority buildings get workers first.
    /// </summary>
    public sealed class EconomyWorkerAllocationService
    {
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

            // Sort buildings by priority desc; inactive buildings get 0 workers
            var sorted = state.Buildings
                .Where(b => b.IsActive && b.RequiredWorkers > 0)
                .OrderByDescending(b => b.EconomyPriority)
                .ToList();

            int remaining = available;
            int assigned = 0;

            for (int i = 0; i < sorted.Count && remaining > 0; i++)
            {
                var building = sorted[i];
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
    }
}
