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

            // Housing is orthogonal to settlement/worker semantics. A town
            // hall may provide housing, and a workerless building may still
            // contribute population capacity.

            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    context.Definition,
                    out GarrisonBuildingModule garrison))
            {
                if (garrison.Capacity <= 0)
                {
                    collector.AddError(
                        "INV_GARRISON_CAPACITY",
                        "Garrison Capacity має бути більшою за 0.");
                }

                bool hasLegacyGarrison = false;
                if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                        context.Definition,
                        out CastleBuildingModule castle))
                    hasLegacyGarrison |= castle.GarrisonCapacity > 0;

                if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                        context.Definition,
                        out DefenseBuildingModule defense))
                    hasLegacyGarrison |= defense.GarrisonCapacity > 0;

                if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                        context.Definition,
                        out HousingBuildingModule housing))
                    hasLegacyGarrison |= housing.IsGarrisonCapable;

                if (hasLegacyGarrison)
                {
                    collector.AddWarning(
                        "INV_GARRISON_LEGACY_DUPLICATE",
                        "Garrison module вже authoritative; старі garrison-поля в Housing/Castle/Defense ігноруються. Скористайтесь «Виправити модулі».");
                }
            }

            if (snapshot.HasWall && snapshot.HasGate)
            {
                collector.AddError(
                    "INV_WALL_GATE_CONFLICT",
                    "Будівля не може одночасно бути стіною і воротами.");
            }

            if ((snapshot.HasWall || snapshot.HasGate)
                && BuildingDefinitionCapabilities.HasEnabledModule<
                    GarrisonBuildingModule>(context.Definition))
            {
                collector.AddError(
                    "INV_GARRISON_WALL_GATE_CONFLICT",
                    "Окремий Garrison module не використовується на сегменті стіни або воротах.");
            }

            if (snapshot.HasWorkerlessSemantics
                && snapshot.HasProduction
                && BuildingDefinitionCapabilities
                    .ProductionRequiresWorkers(context.Definition))
            {
                collector.AddError(
                    "INV_WORKERLESS_PRODUCTION",
                    "Будівля без робітників не може мати рецепт, який потребує робітників.");
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
            ValidateRecipes(production, collector);
            if (string.IsNullOrWhiteSpace(production.ResourceId) && !HasAnyRecipeOutput(production))
            {
                collector.AddError("INV_PRODUCTION_RESOURCE", "Виробничий модуль потребує основний ресурс або вихідний ресурс у рецепті.");
            }

            if (BuildingDefinitionCapabilities
                    .ProductionRequiresWorkers(context.Definition)
                && BuildingDefinitionCapabilities
                    .GetRequiredWorkers(context.Definition) <= 0)
            {
                collector.AddError(
                    "INV_PRODUCTION_WORKERS_REQUIRED",
                    "Хоча б один рецепт потребує робітників, але ефективна кількість WorkersRequired дорівнює 0. Додайте Workforce module.");
            }

            if (snapshot.HasWorkerlessSemantics && production.WorkersRequired > 0)
            {
                collector.AddWarning("INV_WORKERS_AUTOFIX", "Кількість робітників буде примусово встановлена в 0 через модуль без робітників, стіну або ворота.");
            }
        }

        private static void ValidateRecipes(
            ProductionBuildingModule production,
            BuildingModuleValidationCollector collector)
        {
            if (production?.Recipes == null
                || production.Recipes.Count == 0)
            {
                return;
            }

            var recipeIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int recipeIndex = 0;
                 recipeIndex < production.Recipes.Count;
                 recipeIndex++)
            {
                ProductionRecipeDefinition recipe =
                    production.Recipes[recipeIndex];
                if (recipe == null)
                {
                    collector.AddError(
                        "INV_PRODUCTION_RECIPE_NULL",
                        $"Рецепт [{recipeIndex}] порожній.");
                    continue;
                }

                string recipeId =
                    recipe.RecipeId?.Trim();
                if (string.IsNullOrWhiteSpace(recipeId))
                {
                    collector.AddError(
                        "INV_PRODUCTION_RECIPE_ID",
                        $"Рецепт [{recipeIndex}] потребує стабільний RecipeId.");
                }
                else if (!recipeIds.Add(recipeId))
                {
                    collector.AddError(
                        "INV_PRODUCTION_RECIPE_DUPLICATE_ID",
                        $"RecipeId '{recipeId}' використано більше одного разу.");
                }

                if (recipe.TurnsPerCycle < 1)
                {
                    collector.AddError(
                        "INV_PRODUCTION_RECIPE_TURNS",
                        $"Рецепт '{recipeId ?? recipeIndex.ToString()}' має TurnsPerCycle < 1.");
                }

                ValidateRecipeResources(
                    recipe.Inputs,
                    "input",
                    recipeId,
                    collector,
                    requireAny: false);
                ValidateRecipeResources(
                    recipe.Outputs,
                    "output",
                    recipeId,
                    collector,
                    requireAny: true);
            }
        }

        private static void ValidateRecipeResources(
            IReadOnlyList<BuildingResourceAmount> entries,
            string kind,
            string recipeId,
            BuildingModuleValidationCollector collector,
            bool requireAny)
        {
            if (entries == null || entries.Count == 0)
            {
                if (requireAny)
                {
                    collector.AddError(
                        "INV_PRODUCTION_RECIPE_OUTPUT_EMPTY",
                        $"Рецепт '{recipeId ?? "<без id>"}' не має вихідних ресурсів.");
                }
                return;
            }

            var ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
            int valid = 0;

            for (int index = 0; index < entries.Count; index++)
            {
                BuildingResourceAmount entry = entries[index];
                if (entry == null)
                {
                    collector.AddError(
                        "INV_PRODUCTION_RECIPE_RESOURCE_NULL",
                        $"Рецепт '{recipeId ?? "<без id>"}': {kind}[{index}] порожній.");
                    continue;
                }

                string id = entry.ResourceId?.Trim();
                if (string.IsNullOrWhiteSpace(id))
                {
                    collector.AddError(
                        "INV_PRODUCTION_RECIPE_RESOURCE_ID",
                        $"Рецепт '{recipeId ?? "<без id>"}': {kind}[{index}] не має ResourceId.");
                    continue;
                }

                if (entry.Amount <= 0)
                {
                    collector.AddError(
                        "INV_PRODUCTION_RECIPE_RESOURCE_AMOUNT",
                        $"Рецепт '{recipeId ?? "<без id>"}': {kind} '{id}' має Amount <= 0.");
                    continue;
                }

                valid++;
                if (!ids.Add(id))
                {
                    collector.AddWarning(
                        "INV_PRODUCTION_RECIPE_RESOURCE_DUPLICATE",
                        $"Рецепт '{recipeId ?? "<без id>"}' має дубльований {kind} ресурс '{id}'.");
                }
            }

            if (requireAny && valid == 0)
            {
                collector.AddError(
                    "INV_PRODUCTION_RECIPE_OUTPUT_INVALID",
                    $"Рецепт '{recipeId ?? "<без id>"}' не має жодного валідного output.");
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
            int storageKinds = 0;
            if (BuildingDefinitionCapabilities.HasEnabledModule<
                    StorageBuildingModule>(context.Definition))
                storageKinds++;
            if (BuildingDefinitionCapabilities.HasEnabledModule<
                    WarehouseBuildingModule>(context.Definition))
                storageKinds++;
            if (BuildingDefinitionCapabilities.HasEnabledModule<
                    BarnBuildingModule>(context.Definition))
                storageKinds++;

            if (storageKinds > 1)
            {
                context.Collector.AddWarning(
                    "INV_STORAGE_MULTIPLE_MODELS",
                    "Будівля має кілька storage-моделей. StorageBuildingModule authoritative; Warehouse/Barn залишені лише для compatibility.");
            }

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
            if (snapshot.TileRequirement.MergeMode
                != PlacementRuleMergeMode.Override)
            {
                return;
            }

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

                if ((!string.IsNullOrWhiteSpace(requirement.TileId)
                     || !string.IsNullOrWhiteSpace(requirement.TerrainTag))
                    && requirement.MinimumTileCount >= 1)
                    validCount++;
            }

            if (validCount == 0)
            {
                context.Collector.AddError("INV_TILE_REQUIREMENTS_INVALID", "Модуль вимог не містить коректного ID тайла/terrain-тега з мінімальною кількістю від 1.");
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

    internal sealed class BuildingModuleRuntimeContractValidator
        : IBuildingModuleValidator
    {
        public void Validate(
            BuildingModuleValidationContext context)
        {
            List<BuildingModuleDefinition> modules =
                context.Definition?.Modules;
            if (modules == null)
                return;

            for (int index = 0;
                 index < modules.Count;
                 index++)
            {
                BuildingModuleDefinition module =
                    modules[index];
                if (module == null || !module.IsEnabled)
                    continue;

                Type type = module.GetType();
                if (!BuildingDefinitionCapabilities
                        .HasRuntimeConsumer(type))
                {
                    context.Collector.AddError(
                        "INV_MODULE_NO_RUNTIME_CONSUMER",
                        $"Модуль '{type.Name}' не має runtime consumer. " +
                        "Його не можна лишати активним як silent no-op.");
                    continue;
                }

                ValidateMeaningfulConfiguration(
                    module,
                    context.Definition,
                    context.Collector);
            }
        }

        private static void ValidateMeaningfulConfiguration(
            BuildingModuleDefinition module,
            BuildingDefinition definition,
            BuildingModuleValidationCollector collector)
        {
            switch (module)
            {
                case HousingBuildingModule housing
                    when housing.Capacity <= 0:
                    collector.AddWarning(
                        "INV_HOUSING_NO_EFFECT",
                        "Housing module має Capacity <= 0 і не збільшує житло.");
                    break;



                case WarehouseBuildingModule warehouse
                    when warehouse.MaxCapacity < -1:
                    collector.AddError(
                        "INV_WAREHOUSE_CAPACITY",
                        "Warehouse MaxCapacity має бути -1 або >= 0.");
                    break;

                case WorkforceBuildingModule workforce
                    when workforce.WorkersRequired < 0:
                    collector.AddError(
                        "INV_WORKFORCE_COUNT",
                        "WorkersRequired не може бути від'ємним.");
                    break;


                case StorageBuildingModule storage
                    when storage.Capacity < -1:
                    collector.AddError(
                        "INV_STORAGE_CAPACITY_RUNTIME",
                        "Storage Capacity має бути -1 або >= 0.");
                    break;

                case StorageBuildingModule storage
                    when (storage.StorageKind
                              == BuildingStorageKind.Custom
                          || storage.StorageKind
                              == BuildingStorageKind.Military)
                         && (storage.AcceptedResourceIds == null
                             || storage.AcceptedResourceIds.Length == 0):
                    collector.AddError(
                        "INV_STORAGE_EXPLICIT_FILTER",
                        "Custom/Military Storage потребує явні AcceptedResourceIds. " +
                        "Food/Material автоматично використовують category EconomyDatabase.");
                    break;

                case DefenseBuildingModule defense
                    when (defense.AttackRange > 0)
                         != (defense.AttackDamage > 0):
                    collector.AddError(
                        "INV_DEFENSE_ATTACK_PAIR",
                        "Defense AttackRange і AttackDamage мають бути або обидва > 0, або обидва 0.");
                    break;

                case DefenseBuildingModule defense
                    when defense.Armor <= 0
                         && defense.AttackRange <= 0
                         && defense.AttackDamage <= 0
                         && defense.VisionRevealBonus <= 0
                         && defense.GarrisonCapacity <= 0:
                    collector.AddWarning(
                        "INV_DEFENSE_NO_EFFECT",
                        "Defense module має лише нульові значення і не змінює gameplay.");
                    break;




                case FogRevealBuildingModule fog
                    when !fog.RevealOnBuilt
                         && !fog.RevealWhileActive:
                    collector.AddError(
                        "INV_FOG_REVEAL_NO_EFFECT",
                        "Fog Reveal увімкнений, але не має ефекту: оберіть одноразове відкриття або постійну видимість.");
                    break;

                case FogRevealBuildingModule fog
                    when fog.RevealOnBuilt
                         && fog.RevealWhileActive:
                    collector.AddWarning(
                        "INV_FOG_REVEAL_AMBIGUOUS",
                        "Fog Reveal має обидва режими одночасно; runtime використовує WhileActive. У простому редакторі оберіть один режим.");
                    break;



                case SettlementCenterBuildingModule center
                    when center.InfluenceRadius <= 0:
                    collector.AddWarning(
                        "INV_CENTER_RADIUS_ZERO",
                        "SettlementCenter має нульовий InfluenceRadius.");
                    break;

                case SpacingPlacementRuleModule spacing
                    when spacing.MinimumSpacing < 0:
                    collector.AddError(
                        "INV_SPACING_NEGATIVE",
                        "MinimumSpacing не може бути від'ємним.");
                    break;

                case ReplacementPlacementRuleModule replacement
                    when replacement.MergeMode == PlacementRuleMergeMode.Override
                         && !HasAny(replacement.ReplaceableBuildingIds)
                         && !HasAny(replacement.ReplaceableBuildingTags):
                    collector.AddError(
                        "INV_REPLACEMENT_EMPTY",
                        "Replacement Override потребує building ID або tag.");
                    break;

                case BuildingPrerequisiteModule prerequisite
                    when prerequisite.MergeMode == PlacementRuleMergeMode.Override
                         && !HasAny(prerequisite.BuildingIds)
                         && !HasAny(prerequisite.BuildingTags):
                    collector.AddError(
                        "INV_PREREQUISITE_EMPTY",
                        "Prerequisite Override потребує хоча б один building ID або tag.");
                    break;

                case BuildingPerPlayerLimitModule limit
                    when limit.MaxBuildingsPerPlayer == 0:
                    collector.AddWarning(
                        "INV_LIMIT_DISABLED",
                        "Per-player limit має MaxBuildingsPerPlayer=0 і нічого не обмежує.");
                    break;

                case WallBuildingModule wall
                    when wall.HitPoints <= 0:
                    collector.AddWarning(
                        "INV_WALL_HP",
                        "Wall HitPoints <= 0: буде використано базове MaxHp.");
                    break;

                case GateBuildingModule gate
                    when gate.HitPoints <= 0:
                    collector.AddWarning(
                        "INV_GATE_HP",
                        "Gate HitPoints <= 0: буде використано базове MaxHp.");
                    break;


                case GateBuildingModule gate
                    when gate.OpenSpeed > 0f
                         && definition?.Prefab != null
                         && definition.Prefab
                             .GetComponentInChildren<UnityEngine.Animator>(true)
                             == null:
                    collector.AddWarning(
                        "INV_GATE_ANIMATOR_MISSING",
                        "Gate має OpenSpeed, але prefab не містить Animator. " +
                        "Логічний стан воріт працюватиме, але видимої анімації не буде.");
                    break;

                case GateBuildingModule gate
                    when gate.OpenSpeed < 0f:
                    collector.AddError(
                        "INV_GATE_OPEN_SPEED",
                        "Gate OpenSpeed не може бути від'ємним.");
                    break;
            }
        }

        private static bool HasAny(IReadOnlyList<string> values)
        {
            if (values == null)
                return false;

            for (int index = 0; index < values.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(values[index]))
                    return true;
            }

            return false;
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
