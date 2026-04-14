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

        public static bool AppendMeaningfulFacts(BuildingDefinition definition, StringBuilder output)
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
            int economyPriority = BuildingDefinitionCapabilities.GetEconomyPriority(definition);
            int housingCapacity = BuildingDefinitionCapabilities.GetHousingCapacity(definition);
            string industrialResourceId = BuildingDefinitionCapabilities.GetIndustrialResourceId(definition);
            bool requiresTiles = BuildingDefinitionCapabilities.RequiresTiles(definition);
            var tileRequirements = BuildingDefinitionCapabilities.GetTileRequirements(definition);
            var constructionCost = BuildingDefinitionCapabilities.GetConstructionCost(definition);

            bool isCentral = isTownHall || isCastle;
            bool disablesEconomyService = isWall || isCentral;

            if (isTownHall)
                output.AppendLine("Прапорець: ратуша");

            if (isCastle)
                output.AppendLine("Прапорець: замок");

            if (isWarehouse && !disablesEconomyService)
                output.AppendLine("Прапорець: склад");

            if (isHousing && !disablesEconomyService)
                output.AppendLine("Прапорець: житло");

            if (TryGetMaintenanceFlag(definition, out var requiresMaintenance))
                output.AppendLine($"Потребує обслуговування: {(disablesEconomyService ? "ні" : (requiresMaintenance ? "так" : "ні"))}");

            if (!disablesEconomyService && requiredWorkers > 0)
                output.AppendLine($"Потрібно робітників: {requiredWorkers}");

            if (!disablesEconomyService && economyPriority > 0)
                output.AppendLine($"Економічний пріоритет: {economyPriority}");

            if (!disablesEconomyService && isHousing && housingCapacity > 0)
                output.AppendLine($"Житло: +{housingCapacity}");

            if (!disablesEconomyService && !string.IsNullOrWhiteSpace(industrialResourceId))
            {
                output.AppendLine("Прапорець: виготовляє/працює з ресурсом");
                output.AppendLine($"Промисловий ресурс: {industrialResourceId}");
            }

            if (!disablesEconomyService && definition.UseCustomTownHallRules)
                output.AppendLine("Прапорець: кастомні правила ратуші");

            if (!disablesEconomyService)
            {
                if (constructionCost != null && constructionCost.Count > 0)
                {
                    output.AppendLine($"Вартість будівництва: {constructionCost.Count} ресурс(ів)");
                    for (int i = 0; i < constructionCost.Count; i++)
                    {
                        var cost = constructionCost[i];
                        if (cost == null || string.IsNullOrWhiteSpace(cost.ResourceId) || cost.Amount <= 0)
                            continue;

                        output.AppendLine($"- {cost.ResourceId}: {cost.Amount}");
                    }
                }
                else
                {
                    output.AppendLine("Вартість будівництва: безкоштовно");
                }
            }

            if (definition.RequireTownHallInRange)
                output.AppendLine("Прапорець: потребує ратушу в радіусі");

            if (definition.BlockIfTownHallAlreadyInRange)
                output.AppendLine("Прапорець: блокує другу ратушу/замок у радіусі");

            if (!disablesEconomyService && requiresTiles)
            {
                output.AppendLine("Прапорець: потребує тайли");
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
                        output.AppendLine($"Вимоги до тайлів: {validCount}");
                        for (int i = 0; i < requirements.Length; i++)
                        {
                            var req = requirements[i];
                            if (req == null)
                                continue;

                            if (string.IsNullOrWhiteSpace(req.TileId))
                                continue;

                            if (req.Radius <= 0 || req.MinimumTileCount <= 0)
                                continue;

                            output.AppendLine($"- {req.TileId}: >= {req.MinimumTileCount} в радіусі {req.Radius}");
                        }
                    }
                }
            }

            return output.Length > startLength;
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