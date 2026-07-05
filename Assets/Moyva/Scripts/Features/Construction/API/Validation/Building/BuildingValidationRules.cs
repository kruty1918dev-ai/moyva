using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    internal sealed class BuildingIdentityValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Definition.Id))
                context.Collector.AddError("ID_MISSING", "Building ID is required.");

            if (string.IsNullOrWhiteSpace(context.Definition.DisplayName))
                context.Collector.AddWarning("DISPLAY_NAME_MISSING", "Display name is empty.");
        }
    }

    internal sealed class BuildingPresentationValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            if (context.Definition.Prefab == null)
                context.Collector.AddError("PREFAB_MISSING", $"Building '{context.Collector.BuildingLabel}' has no prefab.");

            if (context.Definition.Icon == null && context.Definition.RuntimePreview == null)
                context.Collector.AddWarning("ICON_MISSING", $"Building '{context.Collector.BuildingLabel}' has no icon/runtime preview.");
        }
    }

    internal sealed class BuildingConstructionValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            var costs = context.Definition.ConstructionCost;
            if (costs == null)
                return;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < costs.Count; i++)
            {
                var entry = costs[i];
                if (entry == null)
                {
                    context.Collector.AddError("COST_NULL", $"Cost entry [{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.ResourceId))
                {
                    context.Collector.AddError("COST_RESOURCE_MISSING", $"Cost entry [{i}] has no resource ID.");
                }
                else
                {
                    if (!seen.Add(entry.ResourceId))
                        context.Collector.AddWarning("COST_RESOURCE_DUPLICATE", $"Resource '{entry.ResourceId}' appears more than once in construction cost.");

                    BuildingResourceValidationUtility.ValidateResourceId(
                        context.Options,
                        entry.ResourceId,
                        $"Cost resource '{entry.ResourceId}'",
                        context.Collector);
                }

                if (entry.Amount <= 0)
                    context.Collector.AddError("COST_AMOUNT_INVALID", $"Cost entry [{i}] has amount <= 0.");
            }
        }
    }

    internal sealed class BuildingPlacementValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            if (context.Definition.RequireTownHallInRange && context.Definition.BlockIfTownHallAlreadyInRange)
            {
                context.Collector.AddWarning("PLACEMENT_RULE_CONFLICT", "Building both requires and blocks a settlement center in range.");
            }
        }
    }

    internal sealed class BuildingRuntimeStatsValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            if (context.Definition.MaxHp <= 0)
                context.Collector.AddError("HP_INVALID", "MaxHp must be greater than 0.");
        }
    }

    internal sealed class BuildingRegistryInclusionValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            if (context.Options?.Registry == null
                || !context.Options.RequireRegistryInclusion
                || string.IsNullOrWhiteSpace(context.Definition.Id))
            {
                return;
            }

            if (context.Options.Registry.GetById(context.Definition.Id) == null)
            {
                context.Collector.AddWarning("REGISTRY_MISSING_BUILDING", $"Registry does not include building '{context.Definition.Id}'.");
            }
        }
    }

    internal sealed class BuildingResourceReferenceValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            var modules = context.Definition.Modules;
            if (modules == null)
                return;

            for (int moduleIndex = 0; moduleIndex < modules.Count; moduleIndex++)
            {
                if (modules[moduleIndex] is ProductionBuildingModule production)
                {
                    if (!string.IsNullOrWhiteSpace(production.ResourceId))
                    {
                        BuildingResourceValidationUtility.ValidateResourceId(
                            context.Options,
                            production.ResourceId,
                            $"Production resource '{production.ResourceId}'",
                            context.Collector);
                    }

                    ValidateRecipes(production.Recipes, context);
                }

                if (modules[moduleIndex] is StorageBuildingModule storage && storage.AcceptedResourceIds != null)
                {
                    for (int i = 0; i < storage.AcceptedResourceIds.Length; i++)
                    {
                        BuildingResourceValidationUtility.ValidateResourceId(
                            context.Options,
                            storage.AcceptedResourceIds[i],
                            $"Storage resource '{storage.AcceptedResourceIds[i]}'",
                            context.Collector);
                    }
                }
            }
        }

        private static void ValidateRecipes(
            IReadOnlyList<ProductionRecipeDefinition> recipes,
            BuildingDefinitionValidationContext context)
        {
            if (recipes == null)
                return;

            for (int recipeIndex = 0; recipeIndex < recipes.Count; recipeIndex++)
            {
                var recipe = recipes[recipeIndex];
                if (recipe == null)
                {
                    context.Collector.AddError("RECIPE_NULL", $"Recipe [{recipeIndex}] is null.");
                    continue;
                }

                if (recipe.TurnsPerCycle <= 0)
                    context.Collector.AddError("RECIPE_TURNS_INVALID", $"Recipe '{recipe.RecipeId}' has TurnsPerCycle <= 0.");

                BuildingResourceValidationUtility.ValidateResourceAmounts(recipe.Inputs, context.Options, $"Recipe '{recipe.RecipeId}' input", context.Collector);
                BuildingResourceValidationUtility.ValidateResourceAmounts(recipe.Outputs, context.Options, $"Recipe '{recipe.RecipeId}' output", context.Collector);

                if (recipe.Outputs == null || recipe.Outputs.Count == 0)
                    context.Collector.AddWarning("RECIPE_OUTPUT_EMPTY", $"Recipe '{recipe.RecipeId}' has no outputs.");
            }
        }
    }
}
