using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Read-only authoritative view of production that mirrors the eligibility rules used by
    /// EconomyProductionTickService. It does not mutate settlement, building, recipe or turn state.
    /// </summary>
    public sealed class EconomyProductionReadSnapshot
    {
        public IReadOnlyDictionary<string, float> ProductionPerTurn { get; }
        public IReadOnlyDictionary<string, int> ActiveProducerBuildingsByType { get; }

        internal EconomyProductionReadSnapshot(
            Dictionary<string, float> production,
            Dictionary<string, int> producers)
        {
            ProductionPerTurn = production;
            ActiveProducerBuildingsByType = producers;
        }
    }

    internal static class EconomyProductionReadModel
    {
        public static EconomyProductionReadSnapshot Capture(IReadOnlyDictionary<string, EconomySettlementState> settlements, string ownerId)
        {
            var production = new Dictionary<string, float>(StringComparer.Ordinal);
            var producers = new Dictionary<string, int>(StringComparer.Ordinal);
            if (settlements == null || string.IsNullOrWhiteSpace(ownerId))
                return new EconomyProductionReadSnapshot(production, producers);

            foreach (var pair in settlements)
            {
                var settlement = pair.Value;
                if (settlement == null || !settlement.IsActive
                    || !string.Equals(settlement.OwnerId, ownerId, StringComparison.Ordinal))
                    continue;

                foreach (var building in settlement.Buildings)
                {
                    if (building == null || !building.IsActive
                        || building.ProductionRecipes == null || building.ProductionRecipes.Count == 0)
                        continue;

                    bool buildingProduces = false;
                    foreach (var recipe in building.ProductionRecipes)
                    {
                        if (recipe == null) continue;
                        if (recipe.RequiresWorkers && (building.RequiredWorkers <= 0 || !building.IsFullyStaffed))
                            continue;
                        if (!HasInputs(settlement, recipe.Inputs)) continue;
                        if (recipe.RequiresStorageSpace && !settlement.CanStoreResources(recipe.Outputs)) continue;

                        int turns = Math.Max(1, recipe.TurnsPerCycle);
                        if (recipe.Outputs == null) continue;
                        foreach (var output in recipe.Outputs)
                        {
                            if (output == null || string.IsNullOrWhiteSpace(output.ResourceId) || output.Amount <= 0)
                                continue;
                            float rate = output.Amount / (float)turns;
                            production[output.ResourceId] = production.TryGetValue(output.ResourceId, out var current)
                                ? current + rate : rate;
                            buildingProduces = true;
                        }
                    }

                    if (buildingProduces && !string.IsNullOrWhiteSpace(building.BuildingId))
                        producers[building.BuildingId] = producers.TryGetValue(building.BuildingId, out var count)
                            ? count + 1 : 1;
                }
            }

            return new EconomyProductionReadSnapshot(production, producers);
        }

        private static bool HasInputs(EconomySettlementState settlement, IReadOnlyList<BuildingResourceAmount> inputs)
        {
            if (inputs == null) return true;
            foreach (var input in inputs)
            {
                if (input == null || string.IsNullOrWhiteSpace(input.ResourceId) || input.Amount <= 0) continue;
                if (settlement.GetResource(input.ResourceId) < input.Amount) return false;
            }
            return true;
        }
    }
}
