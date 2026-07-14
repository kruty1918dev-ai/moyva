using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    public static class BuildingDefinitionCapabilities
    {
        public static bool IsTownHall(BuildingDefinition definition)
        {
            if (definition == null)
                return false;

            return HasEnabledModule<TownHallBuildingModule>(definition);
        }

        public static bool IsCastle(BuildingDefinition definition)
        {
            if (definition == null)
                return false;

            return HasEnabledModule<CastleBuildingModule>(definition);
        }

        public static int GetInfluenceRadius(BuildingDefinition definition, int fallbackRadius)
        {
            int normalizedFallback = Math.Max(0, fallbackRadius);
            if (definition == null)
                return normalizedFallback;

            if (definition.TownHallProximityRadiusOverride > 0)
                return definition.TownHallProximityRadiusOverride;

            if (TryGetEnabledModule(definition, out SettlementCenterBuildingModule settlementCenter)
                && settlementCenter.InfluenceRadius > 0)
            {
                return settlementCenter.InfluenceRadius;
            }

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

        public static int GetEconomyPriority(BuildingDefinition definition)
        {
            if (definition == null)
                return 0;

            if (TryGetEnabledModule(definition, out ProductionBuildingModule production))
                return Math.Max(0, production.Priority);

            if (TryGetEnabledModule(definition, out WorkforceBuildingModule workforce))
                return Math.Max(0, workforce.Priority);

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
        public static int GetMaxBuildingsPerPlayer(BuildingDefinition definition)
        {
            return TryGetEnabledModule(definition, out BuildingPerPlayerLimitModule module)
                ? Math.Max(0, module.MaxBuildingsPerPlayer)
                : 0;
        }

        public static IReadOnlyList<BuildingDefinition.BuildingConstructionCostEntry> GetConstructionCost(BuildingDefinition definition)
        {
            if (definition?.ConstructionCost == null)
                return Array.Empty<BuildingDefinition.BuildingConstructionCostEntry>();

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

        public static bool TryGetEnabledModule<TModule>(BuildingDefinition definition, out TModule module)
            where TModule : BuildingModuleDefinition
        {
            module = null;
            if (definition?.Modules == null)
                return false;

            List<BuildingModuleDefinition> source = definition.Modules;
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] is TModule typed && typed.IsEnabled)
                {
                    module = typed;
                    return true;
                }
            }

            return false;
        }
    }
}
