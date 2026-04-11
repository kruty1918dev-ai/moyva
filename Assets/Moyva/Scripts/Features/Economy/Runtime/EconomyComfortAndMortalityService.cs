using Kruty1918.Moyva.Economy.API;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public readonly struct EconomyComfortInput
    {
        public EconomyComfortInput(float foodScore, float heatScore, float lightScore, float clothesScore, float taxScore, float lawScore)
        {
            FoodScore = foodScore;
            HeatScore = heatScore;
            LightScore = lightScore;
            ClothesScore = clothesScore;
            TaxScore = taxScore;
            LawScore = lawScore;
        }

        public float FoodScore { get; }
        public float HeatScore { get; }
        public float LightScore { get; }
        public float ClothesScore { get; }
        public float TaxScore { get; }
        public float LawScore { get; }
    }

    public sealed class EconomyComfortAndMortalityService
    {
        public float CalculateComfort(EconomyRulesConfigSO rules, EconomyComfortInput input)
        {
            var weights = rules?.Consumption?.ComfortWeights;
            if (weights == null)
                return 0f;

            var comfort =
                Mathf.Clamp(input.FoodScore, 0f, 100f) * weights.FoodWeight +
                Mathf.Clamp(input.HeatScore, 0f, 100f) * weights.HeatWeight +
                Mathf.Clamp(input.LightScore, 0f, 100f) * weights.LightWeight +
                Mathf.Clamp(input.ClothesScore, 0f, 100f) * weights.ClothingWeight +
                Mathf.Clamp(input.TaxScore, 0f, 100f) * weights.TaxWeight +
                Mathf.Clamp(input.LawScore, 0f, 100f) * weights.LawWeight;

            return Mathf.Clamp(comfort, 0f, 100f);
        }

        public float CalculateDeathChance(EconomyRulesConfigSO rules, EconomyResidentState resident, EconomyNeedSnapshot needs)
        {
            var mortality = rules?.Mortality;
            if (mortality == null)
                return 0f;

            if (resident.HouseCollapsed)
                return Mathf.Clamp01(mortality.CollapseDeathChance);

            var ageComponent = ResolveAgeComponent(mortality, resident.Age);
            var hungerComponent = Mathf.Clamp(needs.FoodSeverity * mortality.HungerWeight, 0f, mortality.HungerCap);
            var coldComponent = Mathf.Clamp(needs.ColdSeverity * mortality.ColdWeight, 0f, mortality.ColdCap);
            var diseaseComponent = Mathf.Clamp(needs.DiseaseSeverity * mortality.DiseaseWeight, 0f, mortality.DiseaseCap);
            var warComponent = Mathf.Clamp(needs.WarSeverity * mortality.WarWeight, 0f, mortality.WarCap);

            return Mathf.Clamp01(ageComponent + hungerComponent + coldComponent + diseaseComponent + warComponent);
        }

        private static float ResolveAgeComponent(EconomyMortalityRules mortality, int age)
        {
            var tiers = mortality.AgeTiers;
            if (tiers == null)
                return 0f;

            for (var i = 0; i < tiers.Count; i++)
            {
                var tier = tiers[i];
                if (tier == null)
                    continue;

                if (age >= tier.MinAge && age <= tier.MaxAge)
                    return Mathf.Max(0f, tier.BaseDeathChance);
            }

            return 0f;
        }
    }
}
