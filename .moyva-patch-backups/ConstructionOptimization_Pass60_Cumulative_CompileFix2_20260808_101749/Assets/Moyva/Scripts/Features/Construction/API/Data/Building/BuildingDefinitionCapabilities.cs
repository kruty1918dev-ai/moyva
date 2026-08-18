using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Kruty1918.Moyva.Construction.API
{
    public static class BuildingDefinitionCapabilities
    {
        public const int ModuleSchemaVersion = 3;
        public const int RuntimeStateSchemaVersion = 1;

        public static bool IsLegacyCompatibilityModule(
            Type moduleType)
            => moduleType == typeof(WarehouseBuildingModule)
                || moduleType == typeof(BarnBuildingModule)
                || moduleType == typeof(WorkerlessBuildingModule);

        private sealed class ModuleLookupCache
        {
            private readonly Dictionary<Type, BuildingModuleDefinition>
                _enabledByType =
                    new Dictionary<Type, BuildingModuleDefinition>();

            public ModuleLookupCache(BuildingDefinition definition)
            {
                List<BuildingModuleDefinition> modules =
                    definition?.Modules;
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
                    if (!_enabledByType.ContainsKey(type))
                        _enabledByType.Add(type, module);
                }
            }

            public bool TryGet<TModule>(out TModule module)
                where TModule : BuildingModuleDefinition
            {
                if (_enabledByType.TryGetValue(
                        typeof(TModule),
                        out BuildingModuleDefinition value)
                    && value is TModule typed)
                {
                    module = typed;
                    return true;
                }

                module = null;
                return false;
            }
        }

        private static readonly ConditionalWeakTable<
            BuildingDefinition,
            ModuleLookupCache> AssetModuleLookup =
                new ConditionalWeakTable<
                    BuildingDefinition,
                    ModuleLookupCache>();
        public static bool IsTownHall(BuildingDefinition definition)
        {
            return TryGetEnabledModule(
                    definition,
                    out TownHallBuildingModule townHall)
                && townHall.IsCentral;
        }

        public static bool IsCastle(BuildingDefinition definition)
        {
            return TryGetEnabledModule(
                    definition,
                    out CastleBuildingModule castle)
                && castle.IsCapital;
        }

        public static int GetInfluenceRadius(BuildingDefinition definition, int fallbackRadius)
        {
            int normalizedFallback = Math.Max(0, fallbackRadius);
            if (definition == null)
                return normalizedFallback;

            if (TryGetEnabledModule(
                    definition,
                    out SettlementCenterBuildingModule settlementCenter))
            {
                // The marker module is authoritative even when its authored
                // radius is zero. Falling back here would silently turn a
                // disabled/misconfigured center into an active influence source.
                return Math.Max(0, settlementCenter.InfluenceRadius);
            }

            if (definition.PlacementRules?.InfluenceRadius > 0)
                return definition.PlacementRules.InfluenceRadius;

            if (definition.TownHallProximityRadiusOverride > 0)
                return definition.TownHallProximityRadiusOverride;

            if (TryGetEnabledModule(definition, out TownHallBuildingModule townHall)
                && townHall.BuildRadius > 0)
            {
                return townHall.BuildRadius;
            }

            if (TryGetEnabledModule(definition, out CastleBuildingModule castle)
                && castle.ExclusionRadius > 0)
            {
                return castle.ExclusionRadius;
            }

            return normalizedFallback;
        }

        public static bool IsSettlementCenter(BuildingDefinition definition)
        {
            if (definition == null)
                return false;

            if (HasEnabledModule<SettlementCenterBuildingModule>(definition)
                || IsTownHall(definition)
                || IsCastle(definition))
            {
                return true;
            }

            if (definition.PlacementRules != null)
                return definition.PlacementRules.CreatesSettlementInfluence;

            return false;
        }

        public static bool IsWarehouse(BuildingDefinition definition)
        {
            if (definition == null)
                return false;

            return HasEnabledModule<WarehouseBuildingModule>(definition)
                || HasEnabledModule<BarnBuildingModule>(definition)
                || HasEnabledModule<StorageBuildingModule>(definition);
        }

        public static bool IsHousing(BuildingDefinition definition)
        {
            if (definition == null)
                return false;

            return HasEnabledModule<HousingBuildingModule>(definition);
        }

        public static int GetHousingCapacity(BuildingDefinition definition)
        {
            if (definition == null)
                return 0;

            if (TryGetEnabledModule(definition, out HousingBuildingModule housing))
                return Math.Max(0, housing.Capacity);

            return 0;
        }

        public static int GetRequiredWorkers(BuildingDefinition definition)
        {
            if (definition == null)
                return 0;

            if (HasEnabledModule<WorkerlessBuildingModule>(definition)
                || HasEnabledModule<WallBuildingModule>(definition)
                || HasEnabledModule<GateBuildingModule>(definition))
                return 0;

            if (TryGetEnabledModule(definition, out WorkforceBuildingModule workforce))
                return Math.Max(0, workforce.WorkersRequired);

            if (TryGetEnabledModule(definition, out ProductionBuildingModule production))
                return Math.Max(0, production.WorkersRequired);

            return 0;
        }

        public static bool ProductionRequiresWorkers(
            BuildingDefinition definition)
        {
            if (!TryGetEnabledModule(
                    definition,
                    out ProductionBuildingModule production))
            {
                return false;
            }

            if (production.Recipes != null
                && production.Recipes.Count > 0)
            {
                for (int index = 0;
                     index < production.Recipes.Count;
                     index++)
                {
                    ProductionRecipeDefinition recipe =
                        production.Recipes[index];
                    if (recipe != null && recipe.RequiresWorkers)
                        return true;
                }

                return false;
            }

            return production.WorkersRequired > 0;
        }

        public static int GetEconomyPriority(BuildingDefinition definition)
        {
            if (definition == null)
                return 0;

            // Workforce is canonical when present; Production fields remain a
            // compatibility fallback for older assets.
            if (TryGetEnabledModule(
                    definition,
                    out WorkforceBuildingModule workforce))
            {
                return Math.Max(0, workforce.Priority);
            }

            if (TryGetEnabledModule(
                    definition,
                    out ProductionBuildingModule production))
            {
                return Math.Max(0, production.Priority);
            }

            return 0;
        }

        public static string GetIndustrialResourceId(BuildingDefinition definition)
        {
            if (definition == null)
                return string.Empty;

            if (TryGetEnabledModule(definition, out ProductionBuildingModule production)
                && !string.IsNullOrWhiteSpace(production.ResourceId))
            {
                return production.ResourceId;
            }

            if (production?.Recipes != null)
            {
                for (int recipeIndex = 0; recipeIndex < production.Recipes.Count; recipeIndex++)
                {
                    var recipe = production.Recipes[recipeIndex];
                    if (recipe?.Outputs == null)
                        continue;

                    for (int outputIndex = 0; outputIndex < recipe.Outputs.Count; outputIndex++)
                    {
                        string resourceId = recipe.Outputs[outputIndex]?.ResourceId;
                        if (!string.IsNullOrWhiteSpace(resourceId))
                            return resourceId;
                    }
                }
            }

            return string.Empty;
        }

        public static bool TryGetFogReveal(BuildingDefinition definition, out FogRevealBuildingModule module)
        {
            return TryGetEnabledModule(definition, out module)
                && module != null
                && module.RevealRadius > 0;
        }

        public static int GetFogRevealRadius(BuildingDefinition definition)
        {
            return TryGetFogReveal(definition, out var module)
                ? Math.Max(0, module.RevealRadius)
                : 0;
        }

        /// <summary>
        /// Повертає ліміт копій цієї будівлі для одного власника.
        /// 0 означає, що обмеження вимкнене або модуль відсутній.
        /// </summary>
        public static int GetMaxBuildingsPerPlayer(
            BuildingDefinition definition)
        {
            if (IsCastle(definition))
                return 1;

            return TryGetEnabledModule(
                    definition,
                    out BuildingPerPlayerLimitModule module)
                ? Math.Max(0, module.MaxBuildingsPerPlayer)
                : 0;
        }

        public static bool IsStrictPerOwnerUnique(
            BuildingDefinition definition)
            => IsCastle(definition);

        public static IReadOnlyList<
            BuildingDefinition.BuildingConstructionCostEntry>
            GetConstructionCost(BuildingDefinition definition)
        {
            if (definition?.ConstructionCost == null)
            {
                return Array.Empty<
                    BuildingDefinition.BuildingConstructionCostEntry>();
            }

            return definition.ConstructionCost;
        }

        public static bool RequiresTiles(BuildingDefinition definition)
        {
            if (definition == null)
                return false;

            if (TryGetEnabledModule(definition, out TileRequirementBuildingModule tileRequirements)
                && tileRequirements.Requirements != null)
            {
                return tileRequirements.Requirements.Length > 0;
            }

            return false;
        }

        public static TileRequirementDefinition[] GetTileRequirements(BuildingDefinition definition)
        {
            if (definition == null)
                return Array.Empty<TileRequirementDefinition>();

            if (TryGetEnabledModule(definition, out TileRequirementBuildingModule tileRequirements)
                && tileRequirements.Requirements != null)
            {
                return tileRequirements.Requirements;
            }

            return Array.Empty<TileRequirementDefinition>();
        }

        public static int GetMinimumSettlementCenterDistance(
            BuildingDefinition definition)
        {
            int distance = 0;

            if (TryGetEnabledModule(
                    definition,
                    out SettlementCenterBuildingModule center))
            {
                distance = Math.Max(
                    distance,
                    Math.Max(
                        0,
                        center.MinimumDistanceFromOtherCenters));
            }

            if (TryGetEnabledModule(
                    definition,
                    out CastleBuildingModule castle))
            {
                distance = Math.Max(
                    distance,
                    Math.Max(0, castle.ExclusionRadius));
            }

            return distance;
        }

        public static int GetStorageCapacity(
            BuildingDefinition definition)
        {
            if (TryGetEnabledModule(
                    definition,
                    out StorageBuildingModule storage))
            {
                return storage.Capacity;
            }

            if (TryGetEnabledModule(
                    definition,
                    out WarehouseBuildingModule warehouse))
            {
                return warehouse.MaxCapacity;
            }

            return -1;
        }

        public static IReadOnlyList<string>
            GetAcceptedStorageResourceIds(
                BuildingDefinition definition)
        {
            if (TryGetEnabledModule(
                    definition,
                    out StorageBuildingModule storage))
            {
                return storage.AcceptedResourceIds
                    ?? Array.Empty<string>();
            }

            if (TryGetEnabledModule(
                    definition,
                    out WarehouseBuildingModule warehouse)
                && warehouse.ResourceIds != null
                && warehouse.ResourceIds.Length > 0)
            {
                return warehouse.ResourceIds;
            }

            if (TryGetEnabledModule(
                    definition,
                    out BarnBuildingModule barn)
                && barn.FoodResourceIds != null
                && barn.FoodResourceIds.Length > 0)
            {
                return barn.FoodResourceIds;
            }

            return Array.Empty<string>();
        }

        public static string GetWorkerTypeId(
            BuildingDefinition definition)
            => TryGetEnabledModule(
                    definition,
                    out WorkforceBuildingModule workforce)
                ? workforce.WorkerTypeId?.Trim()
                : null;

        public static int GetEffectiveMaxHp(
            BuildingDefinition definition)
        {
            int maxHp = Math.Max(1, definition?.MaxHp ?? 1);

            if (TryGetEnabledModule(
                    definition,
                    out WallBuildingModule wall)
                && wall.HitPoints > 0)
            {
                maxHp = Math.Max(maxHp, wall.HitPoints);
            }

            if (TryGetEnabledModule(
                    definition,
                    out GateBuildingModule gate)
                && gate.HitPoints > 0)
            {
                maxHp = Math.Max(maxHp, gate.HitPoints);
            }

            return maxHp;
        }

        public static int GetEffectiveArmor(
            BuildingDefinition definition)
        {
            int armor = Math.Max(0, definition?.Armor ?? 0);
            if (TryGetEnabledModule(
                    definition,
                    out DefenseBuildingModule defense))
            {
                armor = Math.Max(armor, Math.Max(0, defense.Armor));
            }

            return armor;
        }

        public static int GetDefenseAttackRange(
            BuildingDefinition definition)
            => TryGetEnabledModule(
                    definition,
                    out DefenseBuildingModule defense)
                ? Math.Max(0, defense.AttackRange)
                : 0;

        public static int GetDefenseAttackDamage(
            BuildingDefinition definition)
            => TryGetEnabledModule(
                    definition,
                    out DefenseBuildingModule defense)
                ? Math.Max(0, defense.AttackDamage)
                : 0;

        public static int GetDefenseVisionRevealBonus(
            BuildingDefinition definition)
            => TryGetEnabledModule(
                    definition,
                    out DefenseBuildingModule defense)
                ? Math.Max(0, defense.VisionRevealBonus)
                : 0;

        public static int GetGarrisonCapacity(
            BuildingDefinition definition)
        {
            if (TryGetEnabledModule(
                    definition,
                    out GarrisonBuildingModule garrison))
            {
                return Math.Max(0, garrison.Capacity);
            }

            int capacity = 0;
            if (TryGetEnabledModule(
                    definition,
                    out CastleBuildingModule castle))
            {
                capacity = Math.Max(
                    capacity,
                    Math.Max(0, castle.GarrisonCapacity));
            }

            if (TryGetEnabledModule(
                    definition,
                    out DefenseBuildingModule defense))
            {
                capacity = Math.Max(
                    capacity,
                    Math.Max(0, defense.GarrisonCapacity));
            }

            if (TryGetEnabledModule(
                    definition,
                    out HousingBuildingModule housing)
                && housing.IsGarrisonCapable)
            {
                capacity = Math.Max(
                    capacity,
                    Math.Max(1, housing.Capacity));
            }

            return capacity;
        }

        public static bool HasCanonicalGarrison(
            BuildingDefinition definition)
            => TryGetEnabledModule(
                definition,
                out GarrisonBuildingModule _);

        public static bool IsWallPassable(
            BuildingDefinition definition)
            => TryGetEnabledModule(
                    definition,
                    out WallBuildingModule wall)
               && wall.IsPassable;

        public static bool HasRuntimeConsumer(Type moduleType)
        {
            if (moduleType == null)
                return false;

            return moduleType == typeof(HousingBuildingModule)
                || moduleType == typeof(TownHallBuildingModule)
                || moduleType == typeof(CastleBuildingModule)
                || moduleType == typeof(WarehouseBuildingModule)
                || moduleType == typeof(BarnBuildingModule)
                || moduleType == typeof(ProductionBuildingModule)
                || moduleType == typeof(WorkforceBuildingModule)
                || moduleType == typeof(GarrisonBuildingModule)
                || moduleType == typeof(StorageBuildingModule)
                || moduleType == typeof(DefenseBuildingModule)
                || moduleType == typeof(FogRevealBuildingModule)
                || moduleType == typeof(SettlementCenterBuildingModule)
                || moduleType == typeof(TerrainPlacementRuleModule)
                || moduleType == typeof(FogPlacementRuleModule)
                || moduleType == typeof(SettlementInfluenceRequirementBuildingModule)
                || moduleType == typeof(SpacingPlacementRuleModule)
                || moduleType == typeof(ReplacementPlacementRuleModule)
                || moduleType == typeof(BuildingPrerequisiteModule)
                || moduleType == typeof(TileRequirementBuildingModule)
                || moduleType == typeof(BuildingPerPlayerLimitModule)
                || moduleType == typeof(WorkerlessBuildingModule)
                || moduleType == typeof(WallBuildingModule)
                || moduleType == typeof(GateBuildingModule);
        }

        public static string GetModuleRuntimeEffectDescription(
            BuildingModuleDefinition module)
        {
            if (module == null)
                return "Немає runtime-модуля.";
            if (!module.IsEnabled)
                return "Вимкнений: runtime повністю ігнорує цей модуль.";
            if (module is HousingBuildingModule)
                return "Змінює житлову місткість поселення.";
            if (module is TownHallBuildingModule)
                return "Створює/позначає ратушу та її радіус будівництва.";
            if (module is CastleBuildingModule)
                return "Стартовий центр фракції; жорстко один замок на owner.";
            if (module is SettlementCenterBuildingModule)
                return "Створює authoritative зону впливу поселення.";
            if (module is WarehouseBuildingModule)
                return "Склад із resource filter та capacity.";
            if (module is BarnBuildingModule)
                return "Харчове сховище з явним набором ресурсів.";
            if (module is StorageBuildingModule)
                return "Тип, capacity та accepted resources сховища.";
            if (module is ProductionBuildingModule)
                return "Data-driven production recipes.";
            if (module is WorkforceBuildingModule)
                return "Кількість, професія і пріоритет робітників.";
            if (module is GarrisonBuildingModule)
                return "Authoritative місткість гарнізону будівлі.";
            if (module is WorkerlessBuildingModule)
                return "Примусово 0 потрібних робітників.";
            if (module is DefenseBuildingModule)
                return "Armor, tower damage/range та bonus vision.";
            if (module is WallBuildingModule)
                return "Wall topology, HP та прохідність.";
            if (module is GateBuildingModule)
                return "Gate topology, HP та прохід.";
            if (module is FogRevealBuildingModule)
                return "Одноразове або постійне відкриття Fog of War.";
            if (module is TerrainPlacementRuleModule)
                return "Індивідуальні terrain placement rules.";
            if (module is FogPlacementRuleModule)
                return "Індивідуальні fog placement rules.";
            if (module is SettlementInfluenceRequirementBuildingModule)
                return "Вимоги/overlap settlement influence.";
            if (module is SpacingPlacementRuleModule)
                return "Індивідуальний мінімальний відступ.";
            if (module is ReplacementPlacementRuleModule)
                return "Transactional replacement інших споруд.";
            if (module is BuildingPrerequisiteModule)
                return "Prerequisite building IDs/tags.";
            if (module is TileRequirementBuildingModule)
                return "Nearby tile/terrain requirements.";
            if (module is BuildingPerPlayerLimitModule)
                return "Owner/global building count limit.";

            return "Немає зареєстрованого runtime consumer.";
        }

        public static bool HasEnabledModule<TModule>(BuildingDefinition definition)
            where TModule : BuildingModuleDefinition
        {
            return TryGetEnabledModule(definition, out TModule _);
        }

        public static bool IsGlobalSingleton(BuildingDefinition definition)
        {
            if (definition?.Modules == null)
                return false;

            List<BuildingModuleDefinition> source = definition.Modules;
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] == null || !source[i].IsEnabled)
                    continue;

                if (source[i].SingletonScope == BuildingModuleScope.Global)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Resolves whether selecting the same unique building should move the existing
        /// preview/placement instead of trying to create another copy.
        /// A generic one-per-player cap remains a hard cap; only settlement centers are
        /// treated as movable per-owner uniques.
        /// </summary>
        public static BuildingPlacementUniquenessScope GetPlacementUniquenessScope(
            BuildingDefinition definition)
        {
            if (definition == null)
                return BuildingPlacementUniquenessScope.None;

            if (IsGlobalSingleton(definition))
                return BuildingPlacementUniquenessScope.Global;

            if (!TryGetEnabledModule(
                    definition,
                    out BuildingPerPlayerLimitModule limit)
                || Math.Max(0, limit.MaxBuildingsPerPlayer) <= 0)
            {
                return BuildingPlacementUniquenessScope.None;
            }

            if (limit.OverflowPolicy
                == BuildingLimitOverflowPolicy.RelocateExisting)
            {
                return limit.LimitScope == BuildingLimitScope.Global
                    ? BuildingPlacementUniquenessScope.Global
                    : BuildingPlacementUniquenessScope.PerOwner;
            }

            if (Math.Max(0, limit.MaxBuildingsPerPlayer) != 1)
                return BuildingPlacementUniquenessScope.None;

            // Serialized assets created before OverflowPolicy existed deserialize
            // as Legacy. Preserve their behavior until the editor migration
            // writes an explicit policy.
            return limit.OverflowPolicy
                       == BuildingLimitOverflowPolicy.Legacy
                   && IsSettlementCenter(definition)
                ? BuildingPlacementUniquenessScope.PerOwner
                : BuildingPlacementUniquenessScope.None;
        }

        public static bool SupportsUniqueRelocation(
            BuildingDefinition definition)
        {
            return GetPlacementUniquenessScope(definition)
                   != BuildingPlacementUniquenessScope.None;
        }

        public static bool TryGetEnabledModule<TModule>(
            BuildingDefinition definition,
            out TModule module)
            where TModule : BuildingModuleDefinition
        {
            module = null;
            if (definition?.Modules == null)
                return false;

            if (definition.IsAssetRuntimeSnapshot)
            {
                ModuleLookupCache cache =
                    AssetModuleLookup.GetValue(
                        definition,
                        static value =>
                            new ModuleLookupCache(value));
                return cache.TryGet(out module);
            }

            // Legacy/runtime-created definitions may be mutated in place by
            // tests or compatibility code, so keep their lookup uncached.
            List<BuildingModuleDefinition> source =
                definition.Modules;
            for (int index = 0;
                 index < source.Count;
                 index++)
            {
                if (source[index] is TModule typed
                    && typed.IsEnabled)
                {
                    module = typed;
                    return true;
                }
            }

            return false;
        }
    }
}
