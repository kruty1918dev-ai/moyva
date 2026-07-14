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
                collector.AddError("INV_TOWNHALL_HOUSING", "Модуль ратуші несумісний із житловим модулем.");
            }

            if (snapshot.HasWorkerlessSemantics && snapshot.HasHousing)
            {
                collector.AddError("INV_WORKERLESS_HOUSING", "Модуль без робітників, стіна або ворота несумісні з житловим модулем.");
            }

            if (snapshot.HasWorkerlessSemantics && snapshot.HasProduction)
            {
                collector.AddError("INV_WORKERLESS_PRODUCTION", "Модуль без робітників, стіна або ворота несумісні з виробничим модулем.");
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
                    context.Collector.AddError("INV_BUILD_COST_NULL", $"Вартість будівництва [{i}] має порожній запис.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.ResourceId))
                    context.Collector.AddError("INV_BUILD_COST_RESOURCE", $"Вартість будівництва [{i}] не має ID ресурсу.");

                if (entry.Amount <= 0)
                    context.Collector.AddError("INV_BUILD_COST_AMOUNT", $"Кількість у вартості будівництва [{i}] має бути більшою за 0.");

                if (!string.IsNullOrWhiteSpace(entry.ResourceId) && !usedResourceIds.Add(entry.ResourceId))
                    context.Collector.AddWarning("INV_BUILD_COST_DUPLICATE", $"Вартість будівництва містить дубльований ресурс «{entry.ResourceId}».");
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
                collector.AddError("INV_PRODUCTION_RESOURCE", "Виробничий модуль потребує основний ресурс або вихідний ресурс у рецепті.");
            }

            if (snapshot.HasWorkerlessSemantics && production.WorkersRequired > 0)
            {
                collector.AddWarning("INV_WORKERS_AUTOFIX", "Кількість робітників буде примусово встановлена в 0 через модуль без робітників, стіну або ворота.");
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
                context.Collector.AddError("INV_STORAGE_CAPACITY", "Місткість сховища має бути -1 або невід'ємним значенням.");
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
                context.Collector.AddError("INV_FOG_REVEAL_RADIUS", "Модуль відкриття туману потребує радіус більший за 0.");
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
                context.Collector.AddError("INV_TILE_REQUIREMENTS_EMPTY", "Модуль вимог до тайлів повинен містити хоча б один запис.");
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
                context.Collector.AddError("INV_TILE_REQUIREMENTS_INVALID", "Модуль вимог не містить коректного ID тайла з мінімальною кількістю від 1.");
            }
        }
    }

    internal sealed class BuildingModulePerPlayerLimitValidator : IBuildingModuleValidator
    {
        public void Validate(BuildingModuleValidationContext context)
        {
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    context.Definition,
                    out BuildingPerPlayerLimitModule module)
                && module.MaxBuildingsPerPlayer < 0)
            {
                context.Collector.AddError(
                    "INV_PER_PLAYER_BUILDING_LIMIT",
                    "Максимальна кількість будівель на гравця не може бути від'ємною.");
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
