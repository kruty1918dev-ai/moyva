using System;
using Kruty1918.Moyva.Economy.API;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Handles per-turn population changes: new arrivals, aging, natural death, deficit penalties.
    /// Pure logic — no MonoBehaviour dependency.
    /// </summary>
    public sealed class EconomyPopulationService
    {
        private readonly EconomyComfortAndMortalityService _mortalityService = new EconomyComfortAndMortalityService();
        private readonly EconomyConsumptionService _consumptionService = new EconomyConsumptionService();

        /// <summary>
        /// Process one turn of population: arrivals, aging, consumption, death checks.
        /// Mutates <paramref name="state"/> in place.
        /// Returns (arrivals, deaths, foodConsumed, waterConsumed).
        /// </summary>
        public (int arrivals, int deaths, float foodConsumed, float waterConsumed) Tick(
            EconomySettlementState state, EconomyRulesConfigSO rules)
        {
            if (state == null || rules == null)
                return (0, 0, 0f, 0f);

            var pop = rules.Population;
            int arrivals = 0;
            int deaths = 0;
            float foodConsumed = 0f;
            float waterConsumed = 0f;

            // 1. Arrivals
            if (pop.NewResidentsArrivalIntervalTurns > 0 &&
                state.CurrentTurn > 0 &&
                state.CurrentTurn % pop.NewResidentsArrivalIntervalTurns == 0)
            {
                bool hasHousing = state.Residents.Count < state.TotalHousingCapacity;
                if (hasHousing || !pop.RequireHousingForFamilyCreation)
                {
                    var newResident = new EconomyResidentState(age: 20, hp: 100, comfort: 50f, houseCollapsed: false);
                    state.Residents.Add(newResident);
                    arrivals = 1;
                }
            }

            // 2. Age each resident, consume, check death
            for (int i = state.Residents.Count - 1; i >= 0; i--)
            {
                var resident = state.Residents[i];

                // Aging (1 turn = 1 age unit; designer can adjust meaning via rules)
                var aged = new EconomyResidentState(
                    age: resident.Age + 1,
                    hp: resident.Hp,
                    comfort: resident.Comfort,
                    houseCollapsed: resident.HouseCollapsed);

                // Consumption
                var consumption = _consumptionService.ResolveConsumption(rules, aged.Age);
                float foodNeeded = consumption.FoodPerTurn;
                float waterNeeded = consumption.WaterPerTurn;

                bool fedFood = state.ConsumeResource("Food", foodNeeded);
                bool fedWater = state.ConsumeResource("Water", waterNeeded);

                if (fedFood) foodConsumed += foodNeeded;
                if (fedWater) waterConsumed += waterNeeded;

                // Firewood + Clothing consumption
                float firewoodNeeded = consumption.FirewoodPerTurn;
                float clothingNeeded = consumption.ClothingPerTurn;
                bool hasFirewood = state.ConsumeResource("Firewood", firewoodNeeded);
                bool hasClothing = state.ConsumeResource("Clothing", clothingNeeded);

                // Build need snapshot from deficits
                float foodSeverity = fedFood ? 0f : 1f;
                float coldSeverity = hasFirewood ? 0f : 0.5f;

                var needs = new EconomyNeedSnapshot(
                    foodSeverity: foodSeverity,
                    coldSeverity: coldSeverity,
                    diseaseSeverity: 0f,
                    warSeverity: 0f);

                // HP/Comfort penalties from deficits
                int hpDelta = 0;
                float comfortDelta = 0f;
                var penalties = rules.Consumption.DeficitPenalties;

                if (!fedFood || !fedWater)
                {
                    hpDelta += penalties.FoodOrWaterHpDelta;
                    comfortDelta += penalties.FoodOrWaterComfortDelta;
                }
                if (!hasFirewood)
                {
                    hpDelta += penalties.FirewoodHpDelta;
                    comfortDelta += penalties.FirewoodComfortDelta;
                }
                if (!hasClothing)
                {
                    comfortDelta += penalties.ClothingComfortDelta;
                }

                float newHp = Mathf.Clamp(aged.Hp + hpDelta, 0f, 100f);
                float newComfort = Mathf.Clamp(aged.Comfort + comfortDelta, 0f, 100f);

                var updated = new EconomyResidentState(
                    age: aged.Age,
                    hp: newHp,
                    comfort: newComfort,
                    houseCollapsed: aged.HouseCollapsed);

                // Death check
                float deathChance = _mortalityService.CalculateDeathChance(rules, updated, needs);
                bool dies = newHp <= 0f || UnityEngine.Random.value < deathChance;

                if (dies)
                {
                    state.Residents.RemoveAt(i);
                    deaths++;
                }
                else
                {
                    state.Residents[i] = updated;
                }
            }

            return (arrivals, deaths, foodConsumed, waterConsumed);
        }
    }
}
