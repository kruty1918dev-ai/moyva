using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    internal sealed class BuildingModuleCompatibilityValidator : IBuildingModuleValidator
    {
        public void Validate(BuildingModuleValidationContext context)
        {
            var snapshot = context.Snapshot;
            var collector = context.Collector;

            if (snapshot.HasTownHall && snapshot.HasHousing)
            {
                collector.AddError("INV_TOWNHALL_HOUSING", "TownHallModule несумісний з HousingModule.");
            }

            if (snapshot.HasWorkerlessSemantics && snapshot.HasHousing)
            {
                collector.AddError("INV_WORKERLESS_HOUSING", "Workerless/Wall/Gate несумісні з HousingModule.");
            }

            if (snapshot.HasWorkerlessSemantics && snapshot.HasProduction)
            {
                collector.AddError("INV_WORKERLESS_PRODUCTION", "Workerless/Wall/Gate несумісні з ProductionModule.");
            }
        }
    }

    internal sealed class BuildingModuleConstructionCostValidator : IBuildingModuleValidator
    {
        public void Validate(BuildingModuleValidationContext context)
        {
            var entries = context.Definition.ConstructionCost;
            if (entries == null || entries.Count == 0)
                return;

            var usedResourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null)
                {
                    context.Collector.AddError("INV_BUILD_COST_NULL", $"ConstructionCost[{i}] має порожній запис.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.ResourceId))
                    context.Collector.AddError("INV_BUILD_COST_RESOURCE", $"ConstructionCost[{i}] має порожній ResourceId.");

                if (entry.Amount <= 0)
                    context.Collector.AddError("INV_BUILD_COST_AMOUNT", $"ConstructionCost[{i}] має Amount <= 0.");

                if (!string.IsNullOrWhiteSpace(entry.ResourceId) && !usedResourceIds.Add(entry.ResourceId))
                    context.Collector.AddWarning("INV_BUILD_COST_DUPLICATE", $"ConstructionCost містить дубльований ресурс '{entry.ResourceId}'.");
            }
        }
    }

    internal sealed class BuildingModuleProductionValidator : IBuildingModuleValidator
    {
        public void Validate(BuildingModuleValidationContext context)
        {
            var snapshot = context.Snapshot;
            if (!snapshot.HasProduction || snapshot.Production == null)
                return;

            var production = snapshot.Production;
            var collector = context.Collector;
            if (string.IsNullOrWhiteSpace(production.ResourceId) && !HasAnyRecipeOutput(production))
            {
                collector.AddError("INV_PRODUCTION_RESOURCE", "ProductionModule потребує валідний ResourceId або recipe output.");
            }

            if (snapshot.HasWorkerlessSemantics && production.WorkersRequired > 0)
            {
                collector.AddWarning("INV_WORKERS_AUTOFIX", "WorkersRequired буде примусово встановлено в 0 через Workerless/Wall/Gate семантику.");
            }
        }

        private static bool HasAnyRecipeOutput(ProductionBuildingModule production)
        {
            if (production?.Recipes == null)
                return false;

            for (int recipeIndex = 0; recipeIndex < production.Recipes.Count; recipeIndex++)
            {
                var recipe = production.Recipes[recipeIndex];
                if (recipe?.Outputs == null)
                    continue;

                for (int outputIndex = 0; outputIndex < recipe.Outputs.Count; outputIndex++)
                {
                    if (!string.IsNullOrWhiteSpace(recipe.Outputs[outputIndex]?.ResourceId))
                        return true;
                }
            }

            return false;
        }
    }

    internal sealed class BuildingModuleStorageValidator : IBuildingModuleValidator
    {
        public void Validate(BuildingModuleValidationContext context)
        {
            var snapshot = context.Snapshot;
            if (snapshot.HasStorage && snapshot.Storage != null && snapshot.Storage.Capacity < -1)
            {
                context.Collector.AddError("INV_STORAGE_CAPACITY", "StorageModule capacity має бути -1 або >= 0.");
            }
        }
    }

    internal sealed class BuildingModuleFogRevealValidator : IBuildingModuleValidator
    {
        public void Validate(BuildingModuleValidationContext context)
        {
            var snapshot = context.Snapshot;
            if (snapshot.HasFogReveal && snapshot.FogReveal != null && snapshot.FogReveal.RevealRadius <= 0)
            {
                context.Collector.AddError("INV_FOG_REVEAL_RADIUS", "FogRevealModule потребує RevealRadius > 0.");
            }
        }
    }

    internal sealed class BuildingModuleTileRequirementValidator : IBuildingModuleValidator
    {
        public void Validate(BuildingModuleValidationContext context)
        {
            var snapshot = context.Snapshot;
            if (!snapshot.HasTileRequirement || snapshot.TileRequirement == null)
                return;

            var requirements = snapshot.TileRequirement.Requirements ?? Array.Empty<TileRequirementDefinition>();
            if (requirements.Length == 0)
            {
                context.Collector.AddError("INV_TILE_REQUIREMENTS_EMPTY", "TileRequirementModule повинен містити хоча б один запис.");
                return;
            }

            int validCount = 0;
            for (int i = 0; i < requirements.Length; i++)
            {
                var requirement = requirements[i];
                if (requirement == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(requirement.TileId) && requirement.MinimumTileCount >= 1)
                    validCount++;
            }

            if (validCount == 0)
            {
                context.Collector.AddError("INV_TILE_REQUIREMENTS_INVALID", "TileRequirementModule не містить валідних TileRequirementDefinition (TileId + MinimumTileCount>=1).");
            }
        }
    }

    internal sealed class BuildingModuleSingletonValidator : IBuildingModuleValidator
    {
        public void Validate(BuildingModuleValidationContext context)
        {
            var counters = new Dictionary<Type, int>();
            var modules = context.Definition.Modules;

            for (int i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                if (module == null || !module.IsEnabled)
                    continue;

                if (module.SingletonScope == BuildingModuleScope.None)
                    continue;

                var type = module.GetType();
                if (counters.ContainsKey(type))
                    counters[type]++;
                else
                    counters[type] = 1;
            }

            foreach (var pair in counters)
            {
                if (pair.Value <= 1)
                    continue;

                context.Collector.AddError(
                    "INV_SINGLETON_DUPLICATE",
                    $"Модуль '{pair.Key.Name}' має singleton-семантику і не може бути доданий більше одного разу.");
            }
        }
    }
}
