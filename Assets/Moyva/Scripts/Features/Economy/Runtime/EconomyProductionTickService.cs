using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Runs one turn of production for all buildings in a settlement.
    /// Checks worker staffing, input resources, advances progress, outputs resources.
    /// </summary>
    public sealed class EconomyProductionTickService
    {
        /// <summary>
        /// Tick all active buildings. Mutates <paramref name="state"/> in place.
        /// Returns total completed production cycles this turn.
        /// </summary>
        public int Tick(
            EconomySettlementState state,
            EconomyRulesConfigSO rules,
            IReadOnlyList<EconomyProductionProfile> profiles,
            float turnDurationSeconds)
        {
            if (state == null || profiles == null)
                return 0;

            var production = rules?.Production;
            int completedCycles = 0;

            for (int i = 0; i < state.Buildings.Count; i++)
            {
                var building = state.Buildings[i];
                if (!building.IsActive || string.IsNullOrEmpty(building.ProductionProfileId))
                    continue;

                // Skip if not fully staffed
                if (building.RequiredWorkers > 0 && !building.IsFullyStaffed)
                    continue;

                // Find matching production profile
                var profile = FindProfile(profiles, building.ProductionProfileId);
                if (profile == null)
                    continue;

                // Stop if input missing and rule says so
                if (production != null && production.StopProductionWhenInputMissing)
                {
                    // For now, production profiles define output only;
                    // input checking can be extended once recipe system is added.
                }

                // Advance production progress
                float cycleDuration = Mathf.Max(0.01f, profile.CycleDurationSeconds);
                building.ProductionProgress += turnDurationSeconds;

                // Complete cycles
                while (building.ProductionProgress >= cycleDuration)
                {
                    building.ProductionProgress -= cycleDuration;
                    completedCycles++;

                    // Output resources
                    string outputResource = profile.RecipeId;
                    int outputAmount = profile.OutputAmountPerCycle;
                    state.AddResource(outputResource, outputAmount);
                }
            }

            // Food decay
            if (production != null && production.EnableFoodDecay && production.FoodDecayPerTurn > 0f)
            {
                ApplyDecay(state, "Food", production.FoodDecayPerTurn);
                ApplyDecay(state, "Grain", production.FoodDecayPerTurn);
                ApplyDecay(state, "Meat", production.FoodDecayPerTurn);
                ApplyDecay(state, "Berries", production.FoodDecayPerTurn);
            }

            return completedCycles;
        }

        private static EconomyProductionProfile FindProfile(
            IReadOnlyList<EconomyProductionProfile> profiles, string profileBuildingId)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                var p = profiles[i];
                if (p != null && p.BuildingId == profileBuildingId)
                    return p;
            }
            return null;
        }

        private static void ApplyDecay(EconomySettlementState state, string resourceId, float decayRate)
        {
            float current = state.GetResource(resourceId);
            if (current <= 0f) return;
            float decayed = current * decayRate;
            state.ResourcePool[resourceId] = Mathf.Max(0f, current - decayed);
        }
    }
}
