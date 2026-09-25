using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>EconomyTurnProcessorService — class: економіки ходу Processor сервісу.</summary>
    internal sealed class EconomyTurnProcessorService
    {
        private readonly EconomyTickOrchestrator _orchestrator = new EconomyTickOrchestrator();
        private readonly EconomyConsumptionService _consumptionService = new EconomyConsumptionService();

        /// <summary>Виконує ProcessTurn.</summary>
        public void ProcessTurn(EconomySettlementRegistryService registry, SignalBus signalBus, EconomyDatabaseSO database, float turnDurationSeconds)
        {
            if (database == null || database.RulesConfig == null)
                return;

            var rules = database.RulesConfig;

            foreach (var kvp in registry.AllSettlements)
            {
                var state = kvp.Value;
                if (!state.IsActive)
                    continue;

                // Run full economy tick
                var result = _orchestrator.Tick(state, database, rules, turnDurationSeconds);

                // Fire signal so UI and other systems can react
                signalBus.Fire(new EconomyTickCompletedSignal
                {
                    SettlementId = state.SettlementId,
                    OwnerId = NormalizeOwnerId(state.OwnerId),
                    Turn = result.Turn,
                    TotalPopulation = result.TotalPopulation,
                    Arrivals = result.Arrivals,
                    Deaths = result.Deaths,
                    ProductionCyclesCompleted = result.ProductionCyclesCompleted,
                });

                // Check deactivation
                if (!state.IsActive)
                {
                    signalBus.Fire(new SettlementDeactivatedSignal
                    {
                        SettlementId = state.SettlementId,
                        OwnerId = NormalizeOwnerId(state.OwnerId),
                        Reason = "Населення = 0",
                    });
                }

                // Check resource deficits
                CheckDeficits(state, database, signalBus);
            }
        }

        private void CheckDeficits(EconomySettlementState state, EconomyDatabaseSO database, SignalBus signalBus)
        {
            CheckSingleDeficit(state, database, "Food", signalBus);
            CheckSingleDeficit(state, database, "Water", signalBus);
            CheckSingleDeficit(state, database, "Firewood", signalBus);
        }

        private void CheckSingleDeficit(EconomySettlementState state, EconomyDatabaseSO database, string needId, SignalBus signalBus)
        {
            // Deficits fire only for needs the resource set actually models;
            // the pool stores concrete ids (e.g. 'steak-food-resources').
            if (state.Residents.Count > 0
                && _consumptionService.HasNeedDeficit(state, database, needId))
            {
                signalBus.Fire(new ResourceDeficitSignal
                {
                    SettlementId = state.SettlementId,
                    OwnerId = NormalizeOwnerId(state.OwnerId),
                    ResourceId = needId,
                });
            }
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? EconomyManager.DefaultOwnerId : ownerId.Trim();
        }
    }
}
