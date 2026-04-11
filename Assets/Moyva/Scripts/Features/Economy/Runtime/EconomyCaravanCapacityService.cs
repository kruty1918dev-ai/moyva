using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Перевіряє чи вміщується вантаж у повозку за обмеженнями ваги та розміру.
    /// </summary>
    public sealed class EconomyCaravanCapacityService
    {
        public bool CanLoad(
            EconomyCaravanTemplate template,
            IReadOnlyList<CaravanCargoEntry> currentCargo,
            EconomyResourceDefinition resource,
            int addQuantity,
            out string reason)
        {
            reason = string.Empty;

            if (template == null)
            {
                reason = "Шаблон повозки не задано.";
                return false;
            }

            if (resource == null)
            {
                reason = "Ресурс не задано.";
                return false;
            }

            if (addQuantity <= 0)
            {
                reason = "Кількість має бути більшою за 0.";
                return false;
            }

            if (!IsResourceAllowed(template, resource.Id))
            {
                reason = "Цей ресурс не дозволено для даної повозки.";
                return false;
            }

            int currentUnits = 0;
            long currentWeight = 0;
            float currentSize = 0f;
            int currentFullSizeItems = 0;

            if (currentCargo != null)
            {
                for (int i = 0; i < currentCargo.Count; i++)
                {
                    var entry = currentCargo[i];
                    if (entry.Resource == null || entry.Quantity <= 0)
                        continue;

                    currentUnits += entry.Quantity;
                    currentWeight += (long)entry.Resource.WeightGrams * entry.Quantity;
                    currentSize += entry.Resource.SizeNormalized * entry.Quantity;

                    if (entry.Resource.SizeNormalized >= 1f)
                        currentFullSizeItems += entry.Quantity;
                }
            }

            int targetUnits = currentUnits + addQuantity;
            long targetWeight = currentWeight + (long)resource.WeightGrams * addQuantity;
            float targetSize = currentSize + resource.SizeNormalized * addQuantity;
            int targetFullSizeItems = currentFullSizeItems + (resource.SizeNormalized >= 1f ? addQuantity : 0);

            if (targetUnits > template.Capacity)
            {
                reason = $"Перевищено ліміт кількості: {targetUnits}/{template.Capacity}.";
                return false;
            }

            if (targetWeight > template.MaxWeightGrams)
            {
                reason = $"Перевищено вагу: {targetWeight}г/{template.MaxWeightGrams}г.";
                return false;
            }

            if (targetSize > template.MaxTotalSizeUnits + 0.0001f)
            {
                reason = $"Перевищено сумарний розмір: {targetSize:0.###}/{template.MaxTotalSizeUnits:0.###}.";
                return false;
            }

            if (template.AllowOnlySingleFullSizeItem && targetFullSizeItems > 1)
            {
                reason = "Повозка може перевозити лише один предмет розміру 1.0.";
                return false;
            }

            // Якщо є хоча б один full-size предмет, інші предмети вже не повинні поміститись при MaxTotalSizeUnits=1.
            // Але перевірку залишаємо явно, щоб правило було очевидне навіть при інших місткостях.
            if (template.AllowOnlySingleFullSizeItem && targetFullSizeItems > 0 && targetUnits > targetFullSizeItems)
            {
                reason = "Предмет розміру 1.0 не можна комбінувати з іншими предметами в одній повозці.";
                return false;
            }

            return true;
        }

        private static bool IsResourceAllowed(EconomyCaravanTemplate template, string resourceId)
        {
            var allowed = template.AllowedResourceIds;
            if (allowed == null || allowed.Count == 0)
                return true;

            for (int i = 0; i < allowed.Count; i++)
            {
                if (string.Equals(allowed[i], resourceId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }

    [Serializable]
    public sealed class CaravanCargoEntry
    {
        public EconomyResourceDefinition Resource;
        public int Quantity;
    }
}
