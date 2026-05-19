using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Runtime query API для UI/ботів/мультиплеєрної логіки.
    /// Повертає агрегати ресурсів по власнику (owner) і по конкретному поселенню.
    /// </summary>
    public sealed class EconomyRuntimeApi : IEconomyRuntimeApi
    {
        private const string MaterialsFormatParameterId = "ui-summary-materials-format";
        private const string FoodFormatParameterId = "ui-summary-food-format";
        private const string MoneyFormatParameterId = "ui-summary-money-format";
        private const string DefaultMaterialsFormat = "Матеріали: {0:0.#}";
        private const string DefaultFoodFormat = "Їжа: {0:0.#}";
        private const string DefaultMoneyFormat = "Гроші: {0:0.#}";

        private readonly EconomyManager _economyManager;
        private readonly EconomyDatabaseSO _database;
        private readonly EconomyRulesConfiguration _rulesTemplate;

        public EconomyRuntimeApi(
            EconomyManager economyManager,
            EconomyDatabaseSO database,
            [Zenject.InjectOptional] EconomyRulesConfiguration rulesTemplate)
        {
            _economyManager = economyManager;
            _database = database;
            _rulesTemplate = rulesTemplate;
        }

        public IReadOnlyList<string> GetSettlementIdsForOwner(string ownerId)
        {
            var normalizedOwnerId = NormalizeOwnerId(ownerId);
            var result = new List<string>();

            foreach (var kvp in _economyManager.Settlements)
            {
                var state = kvp.Value;
                if (state == null)
                    continue;

                if (!string.Equals(NormalizeOwnerId(state.OwnerId), normalizedOwnerId, StringComparison.Ordinal))
                    continue;

                result.Add(state.SettlementId);
            }

            return result;
        }

        public EconomyCategoryTotals GetOwnerCategoryTotals(string ownerId)
        {
            var normalizedOwnerId = NormalizeOwnerId(ownerId);
            float food = 0f;
            float materials = 0f;
            float money = 0f;

            foreach (var kvp in _economyManager.Settlements)
            {
                var state = kvp.Value;
                if (state == null)
                    continue;

                if (!string.Equals(NormalizeOwnerId(state.OwnerId), normalizedOwnerId, StringComparison.Ordinal))
                    continue;

                AccumulateCategoryTotals(state.ResourcePool, ref food, ref materials, ref money);
            }

            return new EconomyCategoryTotals(food, materials, money);
        }

        public EconomyFormattedCategoryTotals GetFormattedOwnerCategoryTotals(string ownerId)
        {
            var totals = GetOwnerCategoryTotals(ownerId);
            return new EconomyFormattedCategoryTotals(
                FormatFood(totals.FoodTotal),
                FormatMaterials(totals.MaterialsTotal),
                FormatMoney(totals.MoneyTotal));
        }

        public Dictionary<string, float> GetOwnerResourceTotals(string ownerId)
        {
            var normalizedOwnerId = NormalizeOwnerId(ownerId);
            var totals = new Dictionary<string, float>(StringComparer.Ordinal);

            foreach (var kvp in _economyManager.Settlements)
            {
                var state = kvp.Value;
                if (state == null)
                    continue;

                if (!string.Equals(NormalizeOwnerId(state.OwnerId), normalizedOwnerId, StringComparison.Ordinal))
                    continue;

                AccumulatePerResource(state.ResourcePool, totals);
            }

            return totals;
        }

        public EconomyCategoryTotals GetSettlementCategoryTotals(string settlementId)
        {
            var state = _economyManager.GetSettlement(settlementId);
            if (state == null)
                return new EconomyCategoryTotals(0f, 0f, 0f);

            float food = 0f;
            float materials = 0f;
            float money = 0f;
            AccumulateCategoryTotals(state.ResourcePool, ref food, ref materials, ref money);
            return new EconomyCategoryTotals(food, materials, money);
        }

        public EconomyFormattedCategoryTotals GetFormattedSettlementCategoryTotals(string settlementId)
        {
            var totals = GetSettlementCategoryTotals(settlementId);
            return new EconomyFormattedCategoryTotals(
                FormatFood(totals.FoodTotal),
                FormatMaterials(totals.MaterialsTotal),
                FormatMoney(totals.MoneyTotal));
        }

        public Dictionary<string, float> GetSettlementResourceTotals(string settlementId)
        {
            var state = _economyManager.GetSettlement(settlementId);
            var totals = new Dictionary<string, float>(StringComparer.Ordinal);

            if (state == null)
                return totals;

            AccumulatePerResource(state.ResourcePool, totals);
            return totals;
        }

        private static void AccumulatePerResource(
            Dictionary<string, float> source,
            Dictionary<string, float> destination)
        {
            if (source == null)
                return;

            foreach (var resource in source)
            {
                if (destination.ContainsKey(resource.Key))
                    destination[resource.Key] += resource.Value;
                else
                    destination[resource.Key] = resource.Value;
            }
        }

        private void AccumulateCategoryTotals(
            Dictionary<string, float> source,
            ref float food,
            ref float materials,
            ref float money)
        {
            if (source == null)
                return;

            foreach (var resource in source)
            {
                switch (ResolveCategory(resource.Key))
                {
                    case EconomyResourceCategory.Food:
                        food += resource.Value;
                        break;
                    case EconomyResourceCategory.Materials:
                        materials += resource.Value;
                        break;
                    case EconomyResourceCategory.Money:
                        money += resource.Value;
                        break;
                }
            }
        }

        private EconomyResourceCategory ResolveCategory(string resourceId)
        {
            if (_database == null || _database.Resources == null)
                return EconomyResourceCategory.None;

            for (int i = 0; i < _database.Resources.Count; i++)
            {
                var definition = _database.Resources[i];
                if (definition == null)
                    continue;

                if (string.Equals(definition.Id, resourceId, StringComparison.Ordinal))
                    return definition.Category;
            }

            return EconomyResourceCategory.None;
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId)
                ? EconomyManager.DefaultOwnerId
                : ownerId.Trim();
        }

        private string FormatMaterials(float value)
        {
            var template = GetFormatTemplate(MaterialsFormatParameterId, DefaultMaterialsFormat);
            return FormatValue(template, value, DefaultMaterialsFormat);
        }

        private string FormatFood(float value)
        {
            var template = GetFormatTemplate(FoodFormatParameterId, DefaultFoodFormat);
            return FormatValue(template, value, DefaultFoodFormat);
        }

        private string FormatMoney(float value)
        {
            var template = GetFormatTemplate(MoneyFormatParameterId, DefaultMoneyFormat);
            return FormatValue(template, value, DefaultMoneyFormat);
        }

        private string GetFormatTemplate(string parameterId, string fallback)
        {
            if (_rulesTemplate == null)
                return fallback;

            var parameter = _rulesTemplate.GetParameterById(parameterId);
            if (parameter == null || string.IsNullOrWhiteSpace(parameter.DefaultValue))
                return fallback;

            return parameter.DefaultValue;
        }

        private static string FormatValue(string template, float value, string fallbackTemplate)
        {
            try
            {
                return string.Format(template, value);
            }
            catch
            {
                return string.Format(fallbackTemplate, value);
            }
        }
    }
}
