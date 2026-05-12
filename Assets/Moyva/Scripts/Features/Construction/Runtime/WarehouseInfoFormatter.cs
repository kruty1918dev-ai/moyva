using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Economy.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Утиліта для форматування інформації про ресурси складу.
    /// Групує ресурси по категоріям (Провізія, Матеріали) і показує підсумки.
    /// </summary>
    internal static class WarehouseInfoFormatter
    {
        /// <summary>
        /// Форматує ресурси складу з категоризацією і підсумками.
        /// 
        /// Формат:
        /// ═══════════════════════
        /// Ресурси складу
        /// ───────────────────────
        /// 📦 ПРОВІЗІЯ: 250
        ///   • Пшениця: 100
        ///   • М'ясо: 150
        /// 
        /// 🔨 МАТЕРІАЛИ: 500
        ///   • Дерево: 300
        ///   • Камінь: 200
        /// ═══════════════════════
        /// </summary>
        public static string FormatWarehouseResources(
            IReadOnlyDictionary<string, float> resources,
            EconomyDatabaseSO database,
            string title = "Ресурси складу")
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════");
            sb.AppendLine(title);
            sb.AppendLine("───────────────────────");

            if (resources == null || resources.Count == 0)
            {
                sb.Append("Немає ресурсів.");
                sb.AppendLine();
                sb.Append("═══════════════════════");
                return sb.ToString();
            }

            // Групуємо ресурси по категоріям
            var foodResources = new Dictionary<string, float>();
            var materialResources = new Dictionary<string, float>();
            var otherResources = new Dictionary<string, float>();

            foreach (var kvp in resources)
            {
                var category = GetResourceCategory(kvp.Key, database);
                
                switch (category)
                {
                    case EconomyResourceCategory.Food:
                        foodResources[kvp.Key] = kvp.Value;
                        break;
                    case EconomyResourceCategory.Materials:
                        materialResources[kvp.Key] = kvp.Value;
                        break;
                    default:
                        otherResources[kvp.Key] = kvp.Value;
                        break;
                }
            }

            // Показуємо провізію
            if (foodResources.Count > 0)
            {
                float foodTotal = foodResources.Values.Sum();
                sb.AppendLine($"📦 ПРОВІЗІЯ: {foodTotal:0.#}");
                
                foreach (var kvp in foodResources.OrderByDescending(x => x.Value))
                {
                    var displayName = GetResourceDisplayName(kvp.Key, database);
                    sb.AppendLine($"  • {displayName}: {kvp.Value:0.#}");
                }

                if (materialResources.Count > 0 || otherResources.Count > 0)
                    sb.AppendLine();
            }

            // Показуємо матеріали
            if (materialResources.Count > 0)
            {
                float materialsTotal = materialResources.Values.Sum();
                sb.AppendLine($"🔨 МАТЕРІАЛИ: {materialsTotal:0.#}");
                
                foreach (var kvp in materialResources.OrderByDescending(x => x.Value))
                {
                    var displayName = GetResourceDisplayName(kvp.Key, database);
                    sb.AppendLine($"  • {displayName}: {kvp.Value:0.#}");
                }

                if (otherResources.Count > 0)
                    sb.AppendLine();
            }

            // Показуємо інші
            if (otherResources.Count > 0)
            {
                float otherTotal = otherResources.Values.Sum();
                sb.AppendLine($"❓ ІНШІ: {otherTotal:0.#}");
                
                foreach (var kvp in otherResources.OrderByDescending(x => x.Value))
                {
                    var displayName = GetResourceDisplayName(kvp.Key, database);
                    sb.AppendLine($"  • {displayName}: {kvp.Value:0.#}");
                }
            }

            sb.AppendLine("═══════════════════════");
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Отримує категорію ресурсу з бази даних.
        /// </summary>
        private static EconomyResourceCategory GetResourceCategory(string resourceId, EconomyDatabaseSO database)
        {
            if (database == null || database.Resources == null)
                return EconomyResourceCategory.None;

            var resource = database.Resources.FirstOrDefault(r => 
                r != null && string.Equals(r.Id, resourceId, StringComparison.Ordinal));

            return resource?.Category ?? EconomyResourceCategory.None;
        }

        /// <summary>
        /// Отримує відображуване ім'я ресурсу з бази даних.
        /// Якщо DisplayName не задано, повертає ID.
        /// </summary>
        private static string GetResourceDisplayName(string resourceId, EconomyDatabaseSO database)
        {
            if (database == null || database.Resources == null)
                return resourceId;

            var resource = database.Resources.FirstOrDefault(r => 
                r != null && string.Equals(r.Id, resourceId, StringComparison.Ordinal));

            if (resource != null && !string.IsNullOrWhiteSpace(resource.DisplayName))
                return resource.DisplayName;

            return resourceId;
        }
    }
}
