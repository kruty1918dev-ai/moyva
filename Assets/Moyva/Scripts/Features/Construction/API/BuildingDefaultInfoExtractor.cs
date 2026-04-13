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

            if (TryGetMaintenanceFlag(definition, out var requiresMaintenance))
                output.AppendLine($"Потребує обслуговування: {(requiresMaintenance ? "так" : "ні")}");

            if (definition.RequiredWorkers > 0)
                output.AppendLine($"Потрібно робітників: {definition.RequiredWorkers}");

            if (definition.EconomyPriority > 0)
                output.AppendLine($"Економічний пріоритет: {definition.EconomyPriority}");

            if (definition.IsHousing && definition.HousingCapacity > 0)
                output.AppendLine($"Житло: +{definition.HousingCapacity}");

            if (definition.IsWarehouse)
                output.AppendLine("Тип: склад");

            if (definition.IsTownHall)
                output.AppendLine("Тип: ратуша");

            if (definition.IsCastle)
                output.AppendLine("Тип: замок");

            if (!string.IsNullOrWhiteSpace(definition.IndustrialResourceId))
                output.AppendLine($"Промисловий ресурс: {definition.IndustrialResourceId}");

            if (definition.RequiresTiles)
            {
                var requirements = definition.TileRequirements;
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