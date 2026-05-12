using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Economy.Runtime;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Утиліта для обчислення статистики амбара та інформації про жителів.
    /// Користується для розширеної інформації при кліку на амбар.
    /// </summary>
    internal static class BarnSettlementStatistics
    {
        public struct BarnStatistics
        {
            public int TotalPopulation;
            public int WorkersAvailable;
            public int ChildrenCount;
            public int AdultsCount;
            public int ElderlyCount;
            public Dictionary<string, int> WorkersByBuilding;
            public List<ResidentInfo> Residents;

            public BarnStatistics()
            {
                TotalPopulation = 0;
                WorkersAvailable = 0;
                ChildrenCount = 0;
                AdultsCount = 0;
                ElderlyCount = 0;
                WorkersByBuilding = new Dictionary<string, int>(StringComparer.Ordinal);
                Residents = new List<ResidentInfo>();
            }
        }

        public struct ResidentInfo
        {
            public int Age;
            public float Hp;
            public float Comfort;
            public bool HouseCollapsed;
            public string Status;

            public ResidentInfo(EconomyResidentState resident)
            {
                Age = resident.Age;
                Hp = resident.Hp;
                Comfort = resident.Comfort;
                HouseCollapsed = resident.HouseCollapsed;
                Status = DetermineStatus(resident);
            }

            private static string DetermineStatus(EconomyResidentState resident)
            {
                if (resident.HouseCollapsed)
                    return "Без дома";
                if (resident.Age < 16)
                    return "Дитина";
                if (resident.Age >= 60)
                    return "Пенсіонер";
                return "Робітник";
            }
        }

        public static BarnStatistics CalculateStatistics(EconomySettlementState settlementState)
        {
            var stats = new BarnStatistics();

            if (settlementState == null)
                return stats;

            // Обчислити загальну кількість населення
            stats.TotalPopulation = settlementState.Residents?.Count ?? 0;

            // Класифікувати жителів по категоріям
            foreach (var resident in settlementState.Residents)
            {
                var info = new ResidentInfo(resident);
                stats.Residents.Add(info);

                if (resident.Age < 16)
                {
                    stats.ChildrenCount++;
                }
                else if (resident.Age >= 60)
                {
                    stats.ElderlyCount++;
                }
                else
                {
                    stats.AdultsCount++;
                    if (resident.Age >= 16 && resident.Age < 60)
                        stats.WorkersAvailable++;
                }
            }

            // Копіювати розподіл робітників по будівлях
            if (settlementState.WorkerAssignments != null)
            {
                foreach (var assignment in settlementState.WorkerAssignments)
                {
                    stats.WorkersByBuilding[assignment.Key] = assignment.Value;
                }
            }

            return stats;
        }

        public static string FormatBarnInfoDetailed(EconomySettlementState settlementState)
        {
            var stats = CalculateStatistics(settlementState);

            var sb = new StringBuilder();

            // Заголовок
            sb.AppendLine($"═══ АМБАР: {settlementState.SettlementName} ═══");
            sb.AppendLine();

            // Основна статистика населення
            sb.AppendLine("👥 НАСЕЛЕННЯ:");
            sb.AppendLine($"  • Всього: {stats.TotalPopulation}");
            sb.AppendLine($"  • Дітей: {stats.ChildrenCount}");
            sb.AppendLine($"  • Дорослих: {stats.AdultsCount}");
            sb.AppendLine($"  • Пенсіонерів: {stats.ElderlyCount}");
            sb.AppendLine($"  • Доступних робітників: {stats.WorkersAvailable}");
            sb.AppendLine();

            // Розподіл робітників
            if (stats.WorkersByBuilding.Count > 0)
            {
                sb.AppendLine("🏗️ РОЗПОДІЛ РОБІТНИКІВ:");
                foreach (var assignment in stats.WorkersByBuilding)
                {
                    if (assignment.Value > 0)
                    {
                        sb.AppendLine($"  • {assignment.Key}: {assignment.Value} робітників");
                    }
                }
                sb.AppendLine();
            }

            // Деталі жителів
            if (stats.Residents.Count > 0)
            {
                sb.AppendLine("📋 ДЕТАЛІ ЖИТЕЛІВ:");
                int displayCount = Math.Min(stats.Residents.Count, 10); // Показуємо перших 10
                for (int i = 0; i < displayCount; i++)
                {
                    var resident = stats.Residents[i];
                    var icon = GetIconForStatus(resident.Status);
                    sb.AppendLine($"  {icon} {resident.Age}р. • {resident.Status} • HP: {resident.Hp:0.#}");
                }

                if (stats.Residents.Count > 10)
                {
                    sb.AppendLine($"  ... та ще {stats.Residents.Count - 10} більше");
                }
            }

            return sb.ToString().TrimEnd();
        }

        private static string GetIconForStatus(string status)
        {
            return status switch
            {
                "Дитина" => "👧",
                "Робітник" => "👨",
                "Пенсіонер" => "👴",
                "Без дома" => "🏚️",
                _ => "👤"
            };
        }
    }
}
