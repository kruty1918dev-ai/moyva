using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed class EconomyTurnProcessorService : IEconomyTurnProcessor
    {
        private readonly EconomyTickOrchestrator _orchestrator = new EconomyTickOrchestrator();

        public void ProcessTurn(ISettlementRegistry registry, SignalBus signalBus, EconomyDatabaseSO database, float turnDurationSeconds)
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
                CheckDeficits(state, signalBus);
            }
        }

        private static void CheckDeficits(EconomySettlementState state, SignalBus signalBus)
        {
            CheckSingleDeficit(state, "Food", signalBus);
            CheckSingleDeficit(state, "Water", signalBus);
            CheckSingleDeficit(state, "Firewood", signalBus);
        }

        private static void CheckSingleDeficit(EconomySettlementState state, string resourceId, SignalBus signalBus)
        {
            if (state.GetResource(resourceId) <= 0f && state.Residents.Count > 0)
            {
                signalBus.Fire(new ResourceDeficitSignal
                {
                    SettlementId = state.SettlementId,
                    OwnerId = NormalizeOwnerId(state.OwnerId),
                    ResourceId = resourceId,
                });
            }
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? EconomyManager.DefaultOwnerId : ownerId.Trim();
        }
    }
}
