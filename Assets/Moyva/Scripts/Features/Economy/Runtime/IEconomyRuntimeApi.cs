using System.Collections.Generic;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public readonly struct EconomyFormattedCategoryTotals
    {
        public EconomyFormattedCategoryTotals(string foodText, string materialsText, string moneyText)
        {
            FoodText = foodText;
            MaterialsText = materialsText;
            MoneyText = moneyText;
        }

        public string FoodText { get; }
        public string MaterialsText { get; }
        public string MoneyText { get; }
    }

    public readonly struct EconomyCategoryTotals
    {
        public EconomyCategoryTotals(float foodTotal, float materialsTotal, float moneyTotal)
        {
            FoodTotal = foodTotal;
            MaterialsTotal = materialsTotal;
            MoneyTotal = moneyTotal;
        }

        public float FoodTotal { get; }
        public float MaterialsTotal { get; }
        public float MoneyTotal { get; }
    }

    public interface IEconomyRuntimeApi
    {
        IReadOnlyList<string> GetSettlementIdsForOwner(string ownerId);
        EconomyCategoryTotals GetOwnerCategoryTotals(string ownerId);
        EconomyFormattedCategoryTotals GetFormattedOwnerCategoryTotals(string ownerId);
        Dictionary<string, float> GetOwnerResourceTotals(string ownerId);
        EconomyCategoryTotals GetSettlementCategoryTotals(string settlementId);
        EconomyFormattedCategoryTotals GetFormattedSettlementCategoryTotals(string settlementId);
        Dictionary<string, float> GetSettlementResourceTotals(string settlementId);
    }
}
