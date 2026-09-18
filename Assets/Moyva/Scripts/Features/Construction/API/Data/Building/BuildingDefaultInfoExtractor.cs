using System;
using System.Reflection;
using System.Text;

namespace Kruty1918.Moyva.Construction.API
{
    public static class BuildingDefaultInfoExtractor
    {
        private static readonly string[] MaintenanceMemberNames =
        {
            "RequiresMaintenance",
            "NeedsMaintenance",
            "RequireMaintenance",
            "UseMaintenance",
            "HasMaintenance",
        };

        private static readonly Func<BuildingDefinition, bool?> MaintenanceAccessor = BuildMaintenanceAccessor();

        public static bool AppendMeaningfulFacts(
            BuildingDefinition definition,
            StringBuilder output,
            Func<string, string> resourceDisplayNameResolver = null,
            Kruty1918.Moyva.Shared.Localization.ILocalizationService loca = null)
        {
            if (definition == null || output == null)
                return false;

            int startLength = output.Length;

            bool isWall = definition.Category == BuildingCategory.Walls;
            bool isTownHall = BuildingDefinitionCapabilities.IsTownHall(definition);
            bool isCastle = BuildingDefinitionCapabilities.IsCastle(definition);
            bool isWarehouse = BuildingDefinitionCapabilities.IsWarehouse(definition);
            bool isHousing = BuildingDefinitionCapabilities.IsHousing(definition);
            int requiredWorkers = BuildingDefinitionCapabilities.GetRequiredWorkers(definition);
            int housingCapacity = BuildingDefinitionCapabilities.GetHousingCapacity(definition);
            string industrialResourceId = BuildingDefinitionCapabilities.GetIndustrialResourceId(definition);
            bool requiresTiles = BuildingDefinitionCapabilities.RequiresTiles(definition);
            var tileRequirements = BuildingDefinitionCapabilities.GetTileRequirements(definition);
            var constructionCost = BuildingDefinitionCapabilities.GetConstructionCost(definition);

            bool isCentral = isTownHall || isCastle;
            bool disablesEconomyService = isWall || isCentral;

            if (isTownHall)
                output.AppendLine(L(loca, "Type: town hall"));

            if (isCastle)
                output.AppendLine(L(loca, "Type: castle"));

            if (isWarehouse && !disablesEconomyService)
                output.AppendLine(L(loca, "Purpose: resource storage"));

            if (isHousing && !disablesEconomyService)
                output.AppendLine(L(loca, "Purpose: housing"));

            if (TryGetMaintenanceFlag(definition, out var requiresMaintenance))
            {
                string flag = disablesEconomyService ? L(loca, "no") : (requiresMaintenance ? L(loca, "yes") : L(loca, "no"));
                output.AppendLine(LF(loca, "Requires maintenance: {0}", flag));
            }

            if (!disablesEconomyService && requiredWorkers > 0)
                output.AppendLine(LF(loca, "Workers required: {0}", requiredWorkers));

            if (!disablesEconomyService && isHousing && housingCapacity > 0)
                output.AppendLine(LF(loca, "Housing: +{0}", housingCapacity));

            if (!disablesEconomyService && !string.IsNullOrWhiteSpace(industrialResourceId))
                output.AppendLine(LF(loca, "Produces: {0}", ResolveResourceDisplayName(industrialResourceId, resourceDisplayNameResolver)));

            if (!disablesEconomyService)
            {
                if (constructionCost != null && constructionCost.Count > 0)
                {
                    output.AppendLine(LF(loca, "Construction cost: {0} resource(s)", constructionCost.Count));
                    for (int i = 0; i < constructionCost.Count; i++)
                    {
                        var cost = constructionCost[i];
                        if (cost == null || string.IsNullOrWhiteSpace(cost.ResourceId) || cost.Amount <= 0)
                            continue;

                        output.AppendLine($"- {ResolveResourceDisplayName(cost.ResourceId, resourceDisplayNameResolver)}: {cost.Amount}");
                    }
                }
                else
                {
                    output.AppendLine(L(loca, "Construction cost: free"));
                }
            }

            if (definition.RequireTownHallInRange)
                output.AppendLine(L(loca, "Requires a town hall nearby."));

            if (definition.BlockIfTownHallAlreadyInRange)
                output.AppendLine(L(loca, "Cannot build near another settlement center."));

            if (!disablesEconomyService && requiresTiles)
            {
                var requirements = tileRequirements;
                if (requirements != null)
                {
                    int validCount = 0;
                    for (int i = 0; i < requirements.Length; i++)
                    {
                        var req = requirements[i];
                        if (req == null)
                            continue;

                        if (string.IsNullOrWhiteSpace(req.TileId))
                            continue;

                        if (req.Radius <= 0 || req.MinimumTileCount <= 0)
                            continue;

                        validCount++;
                    }

                    if (validCount > 0)
                    {
                        output.AppendLine(validCount == 1
                            ? L(loca, "Requires suitable terrain nearby.")
                            : LF(loca, "Requires suitable terrain tiles: {0}.", validCount));
                    }
                }
            }

            return output.Length > startLength;
        }

        private static string L(Kruty1918.Moyva.Shared.Localization.ILocalizationService loca, string key)
            => loca?.T(key) ?? key;

        private static string LF(Kruty1918.Moyva.Shared.Localization.ILocalizationService loca, string key, params object[] args)
            => loca?.TF(key, args) ?? string.Format(System.Globalization.CultureInfo.CurrentCulture, key, args);

        private static string ResolveResourceDisplayName(string resourceId, Func<string, string> resolver)
        {
            string fallback = string.IsNullOrWhiteSpace(resourceId) ? string.Empty : resourceId.Trim();
            if (resolver == null || string.IsNullOrEmpty(fallback))
                return fallback;

            string resolved = resolver(fallback);
            return string.IsNullOrWhiteSpace(resolved) ? fallback : resolved;
        }

        private static bool TryGetMaintenanceFlag(BuildingDefinition definition, out bool value)
        {
            value = false;
            if (MaintenanceAccessor == null || definition == null)
                return false;

            var result = MaintenanceAccessor(definition);
            if (!result.HasValue)
                return false;

            value = result.Value;
            return true;
        }

        private static Func<BuildingDefinition, bool?> BuildMaintenanceAccessor()
        {
            var type = typeof(BuildingDefinition);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            for (int i = 0; i < MaintenanceMemberNames.Length; i++)
            {
                string memberName = MaintenanceMemberNames[i];

                var field = type.GetField(memberName, flags);
                if (field != null && field.FieldType == typeof(bool))
                    return definition => (bool)field.GetValue(definition);

                var property = type.GetProperty(memberName, flags);
                if (property != null && property.PropertyType == typeof(bool) && property.GetMethod != null)
                    return definition => (bool)property.GetValue(definition);
            }

            return null;
        }
    }
}
