using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public sealed class BuildingValidationContext
    {
        public IBuildingRegistry Registry;
        public ISet<string> ResourceIds;
        public bool RequireRegistryInclusion;
    }

    public static class BuildingValidator
    {
        public static IReadOnlyList<BuildingValidationIssue> Validate(
            BuildingDefinition definition,
            BuildingValidationContext context = null)
        {
            var issues = new List<BuildingValidationIssue>();
            if (definition == null)
            {
                AddError(issues, "BUILDING_NULL", "BuildingDefinition is null.");
                return issues;
            }

            ValidateIdentity(definition, issues);
            ValidatePresentation(definition, issues);
            ValidateConstruction(definition, context, issues);
            ValidatePlacement(definition, issues);
            ValidateRuntimeStats(definition, issues);
            ValidateRegistryInclusion(definition, context, issues);
            issues.AddRange(BuildingModuleValidation.Validate(definition));
            ValidateResourceReferences(definition, context, issues);
            return issues;
        }

        public static IReadOnlyList<BuildingValidationIssue> ValidateRegistry(IBuildingRegistry registry, ISet<string> resourceIds = null)
        {
            var issues = new List<BuildingValidationIssue>();
            if (registry == null)
            {
                AddError(issues, "REGISTRY_NULL", "BuildingRegistry is missing.");
                return issues;
            }

            var definitions = registry.GetAll() ?? Array.Empty<BuildingDefinition>();
            var ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                if (definition == null)
                {
                    AddError(issues, "REGISTRY_NULL_ENTRY", $"Registry entry [{i}] is null.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(definition.Id))
                {
                    if (ids.ContainsKey(definition.Id))
                        AddError(issues, "REGISTRY_DUPLICATE_ID", $"Duplicate building ID '{definition.Id}'.");
                    else
                        ids.Add(definition.Id, i);
                }

                issues.AddRange(Validate(definition, new BuildingValidationContext
                {
                    Registry = registry,
                    ResourceIds = resourceIds,
                    RequireRegistryInclusion = false,
                }));
            }

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

        private static void ValidateIdentity(BuildingDefinition definition, List<BuildingValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(definition.Id))
                AddError(issues, "ID_MISSING", "Building ID is required.");
            if (string.IsNullOrWhiteSpace(definition.DisplayName))
                AddWarning(issues, "DISPLAY_NAME_MISSING", "Display name is empty.");
        }

        private static void ValidatePresentation(BuildingDefinition definition, List<BuildingValidationIssue> issues)
        {
            if (definition.Prefab == null)
                AddError(issues, "PREFAB_MISSING", $"Building '{definition.Id}' has no prefab.");
            if (definition.Icon == null && definition.RuntimePreview == null)
                AddWarning(issues, "ICON_MISSING", $"Building '{definition.Id}' has no icon/runtime preview.");
        }

        private static void ValidateConstruction(
            BuildingDefinition definition,
            BuildingValidationContext context,
            List<BuildingValidationIssue> issues)
        {
            var costs = definition.ConstructionCost;
            if (costs == null)
                return;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < costs.Count; i++)
            {
                var entry = costs[i];
                if (entry == null)
                {
                    AddError(issues, "COST_NULL", $"Cost entry [{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.ResourceId))
                {
                    AddError(issues, "COST_RESOURCE_MISSING", $"Cost entry [{i}] has no resource ID.");
                }
                else
                {
                    if (!seen.Add(entry.ResourceId))
                        AddWarning(issues, "COST_RESOURCE_DUPLICATE", $"Resource '{entry.ResourceId}' appears more than once in construction cost.");
                    ValidateResourceId(context, entry.ResourceId, $"Cost resource '{entry.ResourceId}'", issues);
                }

                if (entry.Amount <= 0)
                    AddError(issues, "COST_AMOUNT_INVALID", $"Cost entry [{i}] has amount <= 0.");
            }
        }

        private static void ValidatePlacement(BuildingDefinition definition, List<BuildingValidationIssue> issues)
        {
            if (definition.RequireTownHallInRange && definition.BlockIfTownHallAlreadyInRange)
                AddWarning(issues, "PLACEMENT_RULE_CONFLICT", "Building both requires and blocks a settlement center in range.");
        }

        private static void ValidateRuntimeStats(BuildingDefinition definition, List<BuildingValidationIssue> issues)
        {
            if (definition.MaxHp <= 0)
                AddError(issues, "HP_INVALID", "MaxHp must be greater than 0.");
        }

        private static void ValidateRegistryInclusion(
            BuildingDefinition definition,
            BuildingValidationContext context,
            List<BuildingValidationIssue> issues)
        {
            if (context?.Registry == null || !context.RequireRegistryInclusion || string.IsNullOrWhiteSpace(definition.Id))
                return;

            if (context.Registry.GetById(definition.Id) == null)
                AddWarning(issues, "REGISTRY_MISSING_BUILDING", $"Registry does not include building '{definition.Id}'.");
        }

        private static void ValidateResourceReferences(
            BuildingDefinition definition,
            BuildingValidationContext context,
            List<BuildingValidationIssue> issues)
        {
            if (definition?.Modules == null)
                return;

            for (int moduleIndex = 0; moduleIndex < definition.Modules.Count; moduleIndex++)
            {
                if (definition.Modules[moduleIndex] is ProductionBuildingModule production)
                {
                    if (!string.IsNullOrWhiteSpace(production.ResourceId))
                        ValidateResourceId(context, production.ResourceId, $"Production resource '{production.ResourceId}'", issues);

                    ValidateRecipes(production.Recipes, context, issues);
                }

                if (definition.Modules[moduleIndex] is StorageBuildingModule storage && storage.AcceptedResourceIds != null)
                {
                    for (int i = 0; i < storage.AcceptedResourceIds.Length; i++)
                        ValidateResourceId(context, storage.AcceptedResourceIds[i], $"Storage resource '{storage.AcceptedResourceIds[i]}'", issues);
                }
            }
        }

        private static void ValidateRecipes(
            IReadOnlyList<ProductionRecipeDefinition> recipes,
            BuildingValidationContext context,
            List<BuildingValidationIssue> issues)
        {
            if (recipes == null)
                return;

            for (int recipeIndex = 0; recipeIndex < recipes.Count; recipeIndex++)
            {
                var recipe = recipes[recipeIndex];
                if (recipe == null)
                {
                    AddError(issues, "RECIPE_NULL", $"Recipe [{recipeIndex}] is null.");
                    continue;
                }

                if (recipe.TurnsPerCycle <= 0)
                    AddError(issues, "RECIPE_TURNS_INVALID", $"Recipe '{recipe.RecipeId}' has TurnsPerCycle <= 0.");

                ValidateResourceAmounts(recipe.Inputs, context, $"Recipe '{recipe.RecipeId}' input", issues);
                ValidateResourceAmounts(recipe.Outputs, context, $"Recipe '{recipe.RecipeId}' output", issues);

                if (recipe.Outputs == null || recipe.Outputs.Count == 0)
                    AddWarning(issues, "RECIPE_OUTPUT_EMPTY", $"Recipe '{recipe.RecipeId}' has no outputs.");
            }
        }

        private static void ValidateResourceAmounts(
            IReadOnlyList<BuildingResourceAmount> amounts,
            BuildingValidationContext context,
            string label,
            List<BuildingValidationIssue> issues)
        {
            if (amounts == null)
                return;

            for (int i = 0; i < amounts.Count; i++)
            {
                var amount = amounts[i];
                if (amount == null)
                {
                    AddError(issues, "RESOURCE_AMOUNT_NULL", $"{label} [{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(amount.ResourceId))
                    AddError(issues, "RESOURCE_ID_MISSING", $"{label} [{i}] has no resource ID.");
                else
                    ValidateResourceId(context, amount.ResourceId, $"{label} '{amount.ResourceId}'", issues);

                if (amount.Amount <= 0)
                    AddError(issues, "RESOURCE_AMOUNT_INVALID", $"{label} [{i}] has amount <= 0.");
            }
        }

        private static void ValidateResourceId(
            BuildingValidationContext context,
            string resourceId,
            string label,
            List<BuildingValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(resourceId) || context?.ResourceIds == null || context.ResourceIds.Count == 0)
                return;

            if (!context.ResourceIds.Contains(resourceId))
                AddError(issues, "RESOURCE_UNKNOWN", $"{label} is not present in the resource database.");
        }

        private static void AddError(List<BuildingValidationIssue> issues, string code, string message)
            => issues.Add(new BuildingValidationIssue { Severity = BuildingValidationSeverity.Error, Code = code, Message = message });

        private static void AddWarning(List<BuildingValidationIssue> issues, string code, string message)
            => issues.Add(new BuildingValidationIssue { Severity = BuildingValidationSeverity.Warning, Code = code, Message = message });
    }
}
