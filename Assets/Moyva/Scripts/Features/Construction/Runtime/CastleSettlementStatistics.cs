using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Утиліта для обчислення статистики поселення та замку.
    /// Користується для розширеної інформації при кліку на замок.
    /// </summary>
    internal static class CastleSettlementStatistics
    {
        public struct CastleStatistics
        {
            public int TotalBuildings;
            public int TotalWarehouses;
            public int TotalBarns;
            public int TotalHouses;
            public int TotalPopulation;
            public int TotalHousingCapacity;
            public Dictionary<string, float> TotalResources;
            public Dictionary<Vector2Int, BuildingInfo> Buildings;

            public CastleStatistics()
            {
                TotalBuildings = 0;
                TotalWarehouses = 0;
                TotalBarns = 0;
                TotalHouses = 0;
                TotalPopulation = 0;
                TotalHousingCapacity = 0;
                TotalResources = new Dictionary<string, float>(System.StringComparer.Ordinal);
                Buildings = new Dictionary<Vector2Int, BuildingInfo>();
            }
        }

        public struct BuildingInfo
        {
            public string BuildingId;
            public string BuildingName;
            public Vector2Int Position;
            public bool IsWarehouse;
            public bool IsBarn;
            public bool IsHouse;
        }

        public static CastleStatistics CalculateStatistics(
            EconomySettlementState settlementState,
            IBuildingRegistry buildingRegistry,
            Dictionary<Vector2Int, string> positionToBuilding,
            string ownerId)
        {
            var stats = new CastleStatistics();

            if (settlementState == null || buildingRegistry == null)
                return stats;

            // Обчислити загальну кількість населення
            stats.TotalPopulation = settlementState.Residents?.Count ?? 0;
            stats.TotalHousingCapacity = settlementState.TotalHousingCapacity;

            // Обчислити статистику будівель
            foreach (var building in settlementState.Buildings)
            {
                var definition = buildingRegistry.GetById(building.BuildingId);
                if (definition == null)
                    continue;

                stats.TotalBuildings++;

                if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                    stats.TotalWarehouses++;
                else if (BuildingDefinitionCapabilities.IsBarn(definition))
                    stats.TotalBarns++;
                else if (BuildingDefinitionCapabilities.IsHouse(definition))
                    stats.TotalHouses++;
            }

            // Знайти позиції будівель для детальної інформації
            if (positionToBuilding != null)
            {
                foreach (var positionEntry in positionToBuilding)
                {
                    var position = positionEntry.Key;
                    var buildingId = positionEntry.Value;

                    var definition = buildingRegistry.GetById(buildingId);
                    if (definition == null)
                        continue;

                    var displayName = string.IsNullOrWhiteSpace(definition.DisplayName)
                        ? definition.Id
                        : definition.DisplayName;

                    var buildingInfo = new BuildingInfo
                    {
                        BuildingId = buildingId,
                        BuildingName = displayName,
                        Position = position,
                        IsWarehouse = BuildingDefinitionCapabilities.IsWarehouse(definition),
                        IsBarn = BuildingDefinitionCapabilities.IsBarn(definition),
                        IsHouse = BuildingDefinitionCapabilities.IsHouse(definition),
                    };

                    stats.Buildings[position] = buildingInfo;
                }
            }

            // Скопіювати ресурси
            if (settlementState.ResourcePool != null)
            {
                foreach (var resource in settlementState.ResourcePool)
                {
                    stats.TotalResources[resource.Key] = resource.Value;
                }
            }

            return stats;
        }

        public static string FormatCastleInfoDetailed(
            EconomySettlementState settlementState,
            IBuildingRegistry buildingRegistry,
            Dictionary<Vector2Int, string> positionToBuilding)
        {
            var stats = CalculateStatistics(settlementState, buildingRegistry, positionToBuilding, settlementState.OwnerId);

            var sb = new System.Text.StringBuilder();

            // Заголовок
            sb.AppendLine($"═══ ЗАМОК: {settlementState.SettlementName} ═══");
            sb.AppendLine();

            // Основна статистика
            sb.AppendLine("📊 ЗАГАЛЬНА СТАТИСТИКА:");
            sb.AppendLine($"  • Населення: {stats.TotalPopulation} / {stats.TotalHousingCapacity}");
            sb.AppendLine($"  • Всього будівель: {stats.TotalBuildings}");
            sb.AppendLine($"  • Складів: {stats.TotalWarehouses}");
            sb.AppendLine($"  • Амбарів: {stats.TotalBarns}");
            sb.AppendLine($"  • Домів: {stats.TotalHouses}");
            sb.AppendLine();

            // Ресурси
            if (stats.TotalResources.Count > 0)
            {
                sb.AppendLine("📦 РЕСУРСИ:");
                foreach (var resource in stats.TotalResources)
                {
                    sb.AppendLine($"  • {resource.Key}: {resource.Value:0.#}");
                }
                sb.AppendLine();
            }

            // Список будівель для навігації
            if (stats.Buildings.Count > 0)
            {
                sb.AppendLine("🏗️ БУДІВЛІ (натисніть для переміщення):");
                foreach (var buildingEntry in stats.Buildings)
                {
                    var info = buildingEntry.Value;
                    var icon = info.IsWarehouse ? "📦" : info.IsBarn ? "🌾" : info.IsHouse ? "🏠" : "🏢";
                    sb.AppendLine($"  {icon} {info.BuildingName} ({info.Position.x}, {info.Position.y})");
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}
