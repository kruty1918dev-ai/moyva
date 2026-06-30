using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    public enum BuildingValidationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
    }

    [Serializable]
    public sealed class BuildingValidationIssue
    {
        public BuildingValidationSeverity Severity;
        public string Code;
        public string Message;
    }

    public static class BuildingModuleValidation
    {
        public static IReadOnlyList<BuildingValidationIssue> Validate(BuildingDefinition definition)
        {
            var issues = new List<BuildingValidationIssue>();
            if (definition == null)
                return issues;

            if (definition.Modules == null || definition.Modules.Count == 0)
                return issues;

            bool hasTownHall = BuildingDefinitionCapabilities.HasEnabledModule<TownHallBuildingModule>(definition);
            bool hasHousing = BuildingDefinitionCapabilities.HasEnabledModule<HousingBuildingModule>(definition);
            bool hasWorkerless = BuildingDefinitionCapabilities.HasEnabledModule<WorkerlessBuildingModule>(definition);
            bool hasWall = BuildingDefinitionCapabilities.HasEnabledModule<WallBuildingModule>(definition);
            bool hasGate = BuildingDefinitionCapabilities.HasEnabledModule<GateBuildingModule>(definition);
            bool hasProduction = BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out ProductionBuildingModule production);
            bool hasStorage = BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out StorageBuildingModule storage);
            bool hasFogReveal = BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out FogRevealBuildingModule fogReveal);

            if (hasTownHall && hasHousing)
            {
                AddError(issues, "INV_TOWNHALL_HOUSING", "TownHallModule несумісний з HousingModule.");
            }

            if ((hasWorkerless || hasWall || hasGate) && hasHousing)
            {
                AddError(issues, "INV_WORKERLESS_HOUSING", "Workerless/Wall/Gate несумісні з HousingModule.");
            }

            if ((hasWorkerless || hasWall || hasGate) && hasProduction)
            {
                AddError(issues, "INV_WORKERLESS_PRODUCTION", "Workerless/Wall/Gate несумісні з ProductionModule.");
            }

            ValidateConstructionCost(definition, issues);

            if (hasProduction)
            {
                if (string.IsNullOrWhiteSpace(production.ResourceId))
                {
                    bool hasRecipeOutput = HasAnyRecipeOutput(production);
                    if (!hasRecipeOutput)
                        AddError(issues, "INV_PRODUCTION_RESOURCE", "ProductionModule потребує валідний ResourceId або recipe output.");
                }

                if ((hasWorkerless || hasWall || hasGate) && production.WorkersRequired > 0)
                {
                    AddWarning(issues, "INV_WORKERS_AUTOFIX", "WorkersRequired буде примусово встановлено в 0 через Workerless/Wall/Gate семантику.");
                }
            }

            if (hasStorage && storage.Capacity < -1)
                AddError(issues, "INV_STORAGE_CAPACITY", "StorageModule capacity має бути -1 або >= 0.");

            if (hasFogReveal && fogReveal.RevealRadius <= 0)
                AddError(issues, "INV_FOG_REVEAL_RADIUS", "FogRevealModule потребує RevealRadius > 0.");

            if (BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out TileRequirementBuildingModule tileReq))
            {
                var requirements = tileReq.Requirements ?? Array.Empty<TileRequirementDefinition>();
                if (requirements.Length == 0)
                {
                    AddError(issues, "INV_TILE_REQUIREMENTS_EMPTY", "TileRequirementModule повинен містити хоча б один запис.");
                }
                else
                {
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
                        AddError(issues, "INV_TILE_REQUIREMENTS_INVALID", "TileRequirementModule не містить валідних TileRequirementDefinition (TileId + MinimumTileCount>=1).");
                    }
                }
            }

            ValidateSingletonDuplicates(definition, issues);

            return issues;
        }

        public static bool HasErrors(IReadOnlyList<BuildingValidationIssue> issues)
        {
            if (issues == null)
                return false;

            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i] != null && issues[i].Severity == BuildingValidationSeverity.Error)
                    return true;
            }

            return false;
        }

        private static void ValidateSingletonDuplicates(BuildingDefinition definition, List<BuildingValidationIssue> issues)
        {
            var counters = new Dictionary<Type, int>();
            var modules = definition.Modules;

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

                AddError(
                    issues,
                    "INV_SINGLETON_DUPLICATE",
                    $"Модуль '{pair.Key.Name}' має singleton-семантику і не може бути доданий більше одного разу.");
            }
        }

        private static void ValidateConstructionCost(BuildingDefinition definition, List<BuildingValidationIssue> issues)
        {
            if (definition == null)
                return;

            var entries = definition.ConstructionCost;
            if (entries == null || entries.Count == 0)
                return;

            var usedResourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null)
                {
                    AddError(issues, "INV_BUILD_COST_NULL", $"ConstructionCost[{i}] має порожній запис.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.ResourceId))
                    AddError(issues, "INV_BUILD_COST_RESOURCE", $"ConstructionCost[{i}] має порожній ResourceId.");

                if (entry.Amount <= 0)
                    AddError(issues, "INV_BUILD_COST_AMOUNT", $"ConstructionCost[{i}] має Amount <= 0.");

                if (!string.IsNullOrWhiteSpace(entry.ResourceId) && !usedResourceIds.Add(entry.ResourceId))
                    AddWarning(issues, "INV_BUILD_COST_DUPLICATE", $"ConstructionCost містить дубльований ресурс '{entry.ResourceId}'.");
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

        private static void AddError(List<BuildingValidationIssue> issues, string code, string message)
        {
            issues.Add(new BuildingValidationIssue
            {
                Severity = BuildingValidationSeverity.Error,
                Code = code,
                Message = message,
            });
        }

        private static void AddWarning(List<BuildingValidationIssue> issues, string code, string message)
        {
            issues.Add(new BuildingValidationIssue
            {
                Severity = BuildingValidationSeverity.Warning,
                Code = code,
                Message = message,
            });
        }
    }
}
