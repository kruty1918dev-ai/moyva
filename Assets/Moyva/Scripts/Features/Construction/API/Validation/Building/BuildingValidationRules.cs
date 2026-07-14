using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    internal sealed class BuildingIdentityValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Definition.Id))
                context.Collector.AddError("ID_MISSING", "ID будівлі є обов'язковим.");

            if (string.IsNullOrWhiteSpace(context.Definition.DisplayName))
                context.Collector.AddWarning("DISPLAY_NAME_MISSING", "Назва будівлі для гравця не задана.");
        }
    }

    internal sealed class BuildingPresentationValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            if (context.Definition.Prefab == null)
                context.Collector.AddError("PREFAB_MISSING", $"Будівля «{context.Collector.BuildingLabel}» не має префаба.");

            if (context.Definition.Icon == null && context.Definition.RuntimePreview == null)
                context.Collector.AddWarning("ICON_MISSING", $"Будівля «{context.Collector.BuildingLabel}» не має іконки або runtime-прев'ю.");
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
                    context.Collector.AddError("COST_NULL", $"Запис вартості [{i}] порожній.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.ResourceId))
                {
                    context.Collector.AddError("COST_RESOURCE_MISSING", $"Запис вартості [{i}] не має ID ресурсу.");
                }
                else
                {
                    if (!seen.Add(entry.ResourceId))
                        context.Collector.AddWarning("COST_RESOURCE_DUPLICATE", $"Ресурс «{entry.ResourceId}» повторюється у вартості будівництва.");

                    BuildingResourceValidationUtility.ValidateResourceId(
                        context.Options,
                        entry.ResourceId,
                        $"Ресурс вартості «{entry.ResourceId}»",
                        context.Collector);
                }

                if (entry.Amount <= 0)
                    context.Collector.AddError("COST_AMOUNT_INVALID", $"Кількість у записі вартості [{i}] має бути більшою за 0.");
            }
        }
    }

    internal sealed class BuildingPlacementValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            if (context.Definition.RequireTownHallInRange && context.Definition.BlockIfTownHallAlreadyInRange)
            {
                context.Collector.AddWarning("PLACEMENT_RULE_CONFLICT", "Будівля одночасно потребує центр поселення в радіусі та блокується ним.");
            }
        }
    }

    internal sealed class BuildingRuntimeStatsValidator : IBuildingDefinitionValidator
    {
        public void Validate(BuildingDefinitionValidationContext context)
        {
            if (context.Definition.MaxHp <= 0)
                context.Collector.AddError("HP_INVALID", "Максимальна міцність має бути більшою за 0.");
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
                context.Collector.AddWarning("REGISTRY_MISSING_BUILDING", $"Реєстр не містить будівлю «{context.Definition.Id}».");
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
                            $"Ресурс виробництва «{production.ResourceId}»",
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
                            $"Ресурс сховища «{storage.AcceptedResourceIds[i]}»",
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
                    context.Collector.AddError("RECIPE_NULL", $"Рецепт [{recipeIndex}] порожній.");
                    continue;
                }

                if (recipe.TurnsPerCycle <= 0)
                    context.Collector.AddError("RECIPE_TURNS_INVALID", $"Рецепт «{recipe.RecipeId}» повинен тривати щонайменше 1 хід.");

                BuildingResourceValidationUtility.ValidateResourceAmounts(recipe.Inputs, context.Options, $"Вхід рецепта «{recipe.RecipeId}»", context.Collector);
                BuildingResourceValidationUtility.ValidateResourceAmounts(recipe.Outputs, context.Options, $"Вихід рецепта «{recipe.RecipeId}»", context.Collector);

                if (recipe.Outputs == null || recipe.Outputs.Count == 0)
                    context.Collector.AddWarning("RECIPE_OUTPUT_EMPTY", $"Рецепт «{recipe.RecipeId}» не має вихідних ресурсів.");
            }
        }
    }
}
