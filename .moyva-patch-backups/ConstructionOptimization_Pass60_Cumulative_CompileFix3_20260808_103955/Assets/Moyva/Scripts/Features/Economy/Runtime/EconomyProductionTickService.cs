using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
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
            if (state == null)
                return 0;

            var production = rules?.Production;
            int completedCycles = 0;

            for (int i = 0; i < state.Buildings.Count; i++)
            {
                var building = state.Buildings[i];
                if (!building.IsActive)
                    continue;

                if (building.ProductionRecipes != null
                    && building.ProductionRecipes.Count > 0)
                {
                    completedCycles += TickModuleRecipes(
                        state,
                        building);
                    continue;
                }

                if (profiles == null
                    || string.IsNullOrEmpty(
                        building.ProductionProfileId))
                {
                    continue;
                }

                // Legacy profile fallback.
                if (building.RequiredWorkers > 0
                    && !building.IsFullyStaffed)
                {
                    continue;
                }

                var profile = FindProfile(
                    profiles,
                    building.ProductionProfileId);
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

        private static int TickModuleRecipes(
            EconomySettlementState state,
            EconomyBuildingState building)
        {
            int completed = 0;

            for (int recipeIndex = 0;
                 recipeIndex < building.ProductionRecipes.Count;
                 recipeIndex++)
            {
                ProductionRecipeDefinition recipe =
                    building.ProductionRecipes[recipeIndex];
                if (recipe == null)
                    continue;

                if (recipe.RequiresWorkers
                    && (building.RequiredWorkers <= 0
                        || !building.IsFullyStaffed))
                {
                    // Invalid/understaffed worker recipe must never run as a
                    // free workerless recipe.
                    continue;
                }

                string recipeId =
                    string.IsNullOrWhiteSpace(recipe.RecipeId)
                        ? $"recipe-{recipeIndex}"
                        : recipe.RecipeId.Trim();
                float progress =
                    building.RecipeProgress.TryGetValue(
                        recipeId,
                        out float stored)
                        ? stored
                        : 0f;

                progress += 1f;
                int turnsPerCycle =
                    Mathf.Max(1, recipe.TurnsPerCycle);

                while (progress >= turnsPerCycle)
                {
                    if (recipe.RequiresStorageSpace
                        && !state.CanStoreResources(
                            recipe.Outputs))
                    {
                        progress = turnsPerCycle;
                        break;
                    }

                    if (!HasRecipeInputs(
                            state,
                            recipe.Inputs))
                    {
                        // Preserve a ready cycle instead of accumulating an
                        // unbounded backlog while inputs are unavailable.
                        progress = turnsPerCycle;
                        break;
                    }

                    ConsumeRecipeInputs(
                        state,
                        recipe.Inputs);
                    AddRecipeOutputs(
                        state,
                        recipe.Outputs);

                    progress -= turnsPerCycle;
                    completed++;
                }

                building.RecipeProgress[recipeId] =
                    progress;
            }

            return completed;
        }

        private static bool HasRecipeInputs(
            EconomySettlementState state,
            IReadOnlyList<BuildingResourceAmount> inputs)
        {
            if (inputs == null)
                return true;

            for (int index = 0; index < inputs.Count; index++)
            {
                BuildingResourceAmount input = inputs[index];
                if (input == null
                    || string.IsNullOrWhiteSpace(input.ResourceId)
                    || input.Amount <= 0)
                {
                    continue;
                }

                if (state.GetResource(input.ResourceId)
                    < input.Amount)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ConsumeRecipeInputs(
            EconomySettlementState state,
            IReadOnlyList<BuildingResourceAmount> inputs)
        {
            if (inputs == null)
                return;

            for (int index = 0; index < inputs.Count; index++)
            {
                BuildingResourceAmount input = inputs[index];
                if (input == null
                    || string.IsNullOrWhiteSpace(input.ResourceId)
                    || input.Amount <= 0)
                {
                    continue;
                }

                state.ConsumeResource(
                    input.ResourceId,
                    input.Amount);
            }
        }

        private static void AddRecipeOutputs(
            EconomySettlementState state,
            IReadOnlyList<BuildingResourceAmount> outputs)
        {
            if (outputs == null)
                return;

            for (int index = 0; index < outputs.Count; index++)
            {
                BuildingResourceAmount output = outputs[index];
                if (output == null
                    || string.IsNullOrWhiteSpace(output.ResourceId)
                    || output.Amount <= 0)
                {
                    continue;
                }

                state.AddResource(
                    output.ResourceId,
                    output.Amount);
            }
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
