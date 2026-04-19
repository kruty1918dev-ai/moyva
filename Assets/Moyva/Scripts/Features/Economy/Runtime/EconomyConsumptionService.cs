using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public readonly struct EconomyConsumptionProfile
    {
        public EconomyConsumptionProfile(float foodPerTurn, float waterPerTurn, float firewoodPerTurn, float clothingPerTurn)
        {
            FoodPerTurn = foodPerTurn;
            WaterPerTurn = waterPerTurn;
            FirewoodPerTurn = firewoodPerTurn;
            ClothingPerTurn = clothingPerTurn;
        }

        public float FoodPerTurn { get; }
        public float WaterPerTurn { get; }
        public float FirewoodPerTurn { get; }
        public float ClothingPerTurn { get; }
    }

    public sealed class EconomyConsumptionService
    {
        public EconomyConsumptionProfile ResolveConsumption(EconomyRulesConfigSO rules, int age)
        {
            var ageRules = rules?.Consumption?.AgeConsumption;
            if (ageRules != null)
            {
                for (var i = 0; i < ageRules.Count; i++)
                {
                    var rule = ageRules[i];
                    if (rule == null)
                        continue;

                    if (age >= rule.MinAge && age <= rule.MaxAge)
                        return new EconomyConsumptionProfile(rule.FoodPerTurn, rule.WaterPerTurn, rule.FirewoodPerTurn, rule.ClothingPerTurn);
                }
            }

            return new EconomyConsumptionProfile(1f, 1f, 0.5f, 0.25f);
        }
    }
}
