using System;
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
        private readonly EconomyConsumptionService _consumptionService = new EconomyConsumptionService();

        /// <summary>
        /// Tick all active buildings. Mutates <paramref name="state"/> in place.
        /// Returns total completed production cycles this turn.
        /// </summary>
        public int Tick(
            EconomySettlementState state,
            EconomyRulesConfigSO rules,
            EconomyDatabaseSO database,
            float turnDurationSeconds)
        {
            if (state == null)
                return 0;

            var production = rules?.Production;
            IReadOnlyList<EconomyProductionProfile> profiles =
                database?.ProductionProfiles;
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

            // Food decay applies to every resource satisfying the "Food" need
            // (concrete ids like 'steak-food-resources'), not literal need ids.
            if (production != null && production.EnableFoodDecay && production.FoodDecayPerTurn > 0f)
            {
                var foodIds = _consumptionService.ResolveNeedResourceIds(database, "Food");
                for (int index = 0; index < foodIds.Count; index++)
                    ApplyDecay(state, foodIds[index], production.FoodDecayPerTurn);
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

                    if (UnityEngine.Random.value
                        >= Mathf.Clamp01(recipe.SuccessChance))
                    {
                        progress -= turnsPerCycle;
                        completed++;
                        continue;
                    }

                    if (!ConsumeRecipeInputs(
                            state,
                            recipe.Inputs))
                    {
                        // A committed cycle never ships outputs for inputs it
                        // could not actually take (reserved stock, races).
                        progress = turnsPerCycle;
                        break;
                    }

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

        /// <summary>
        /// Aggregates a recipe's inputs per resource id (duplicate entries
        /// must sum, not pass the same pool twice) and preflights against
        /// <see cref="EconomySettlementState.GetAvailableResource"/> —
        /// the same spendable view <see cref="EconomySettlementState.ConsumeResource"/>
        /// enforces, so reserved stock can never satisfy a check it cannot pay.
        /// </summary>
        private static bool HasRecipeInputs(
            EconomySettlementState state,
            IReadOnlyList<BuildingResourceAmount> inputs)
        {
            var required = AggregateInputs(inputs);
            if (required == null)
                return true;

            foreach (var pair in required)
            {
                if (state.GetAvailableResource(pair.Key)
                    < pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// All-or-nothing input consumption matching
        /// <see cref="HasRecipeInputs"/> exactly: one aggregated consume per
        /// resource id, so a recipe cannot strip part of its bill.
        /// </summary>
        private static bool ConsumeRecipeInputs(
            EconomySettlementState state,
            IReadOnlyList<BuildingResourceAmount> inputs)
        {
            var required = AggregateInputs(inputs);
            if (required == null)
                return true;

            foreach (var pair in required)
            {
                if (!state.ConsumeResource(pair.Key, pair.Value))
                    return false;
            }

            return true;
        }

        private static List<KeyValuePair<string, int>> AggregateInputs(
            IReadOnlyList<BuildingResourceAmount> inputs)
        {
            if (inputs == null)
                return null;

            var required = new List<KeyValuePair<string, int>>();
            for (int index = 0; index < inputs.Count; index++)
            {
                BuildingResourceAmount input = inputs[index];
                if (input == null
                    || string.IsNullOrWhiteSpace(input.ResourceId)
                    || input.Amount <= 0)
                {
                    continue;
                }

                string id = input.ResourceId.Trim();
                int existing = required.FindIndex(
                    e => string.Equals(
                        e.Key, id, StringComparison.Ordinal));
                if (existing >= 0)
                {
                    var entry = required[existing];
                    required[existing] =
                        new KeyValuePair<string, int>(
                            id, entry.Value + input.Amount);
                }
                else
                {
                    required.Add(
                        new KeyValuePair<string, int>(id, input.Amount));
                }
            }

            return required.Count == 0 ? null : required;
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
