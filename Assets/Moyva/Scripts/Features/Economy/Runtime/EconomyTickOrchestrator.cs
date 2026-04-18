using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Runs one full economy turn for a settlement.
    /// Pipeline order: Population → Worker Allocation → Production → (Consumption included in Population).
    /// Pure logic, no MonoBehaviour. Call <see cref="Tick"/> from your game loop (e.g. TurnManager).
    /// </summary>
    public sealed class EconomyTickOrchestrator
    {
        private readonly EconomyPopulationService _populationService = new EconomyPopulationService();
        private readonly EconomyWorkerAllocationService _workerService = new EconomyWorkerAllocationService();
        private readonly EconomyProductionTickService _productionService = new EconomyProductionTickService();

        /// <summary>
        /// Execute one turn. Mutates <paramref name="state"/> in place.
        /// </summary>
        /// <param name="state">Mutable settlement state.</param>
        /// <param name="database">Economy database (for production profiles).</param>
        /// <param name="rules">Rules configuration.</param>
        /// <param name="turnDurationSeconds">How many in-game seconds one turn represents (for production cycles).</param>
        public EconomyTickResult Tick(
            EconomySettlementState state,
            EconomyDatabaseSO database,
            EconomyRulesConfigSO rules,
            float turnDurationSeconds = 60f)
        {
            if (state == null || rules == null)
                return new EconomyTickResult();

            state.CurrentTurn++;

            var result = new EconomyTickResult { Turn = state.CurrentTurn };

            // 1. Population tick: arrivals, aging, consumption, death
            var (arrivals, deaths, foodConsumed, waterConsumed) = _populationService.Tick(state, rules);
            result.Arrivals = arrivals;
            result.Deaths = deaths;
            result.TotalFoodConsumed = foodConsumed;
            result.TotalWaterConsumed = waterConsumed;
            result.TotalPopulation = state.Residents.Count;

            // 2. Worker allocation
            var (available, assigned) = _workerService.Allocate(state, rules);
            result.AvailableWorkers = available;
            result.AssignedWorkers = assigned;

            // 3. Production tick
            var profiles = database != null ? database.ProductionProfiles : null;
            int cycles = _productionService.Tick(state, rules, profiles, turnDurationSeconds);
            result.ProductionCyclesCompleted = cycles;

            // 4. Settlement deactivation check
            if (rules.Settlement.DeactivateSettlementWhenPopulationIsZero && state.Residents.Count == 0)
                state.IsActive = false;

            return result;
        }
    }
}
