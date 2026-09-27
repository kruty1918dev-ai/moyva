using System.Collections.Generic;
using System.Reflection;
using Kruty1918.JsonConfig;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C04 acceptance: population changes are explainable — deaths attributed
    /// per cause, unmodeled needs never fake consumption in the ledger,
    /// enlisted residents are untouched, and age tiers/arrivals behave as
    /// configured.
    /// </summary>
    public class C04AcceptanceTests
    {
        private const string FoodId = "wheat-bundle-food-resources";
        private const float Epsilon = 0.0001f;

        private EconomyPopulationService _population;

        [SetUp]
        public void SetUp()
        {
            _population = new EconomyPopulationService();
            Random.InitState(12345); // fixed RNG fixture — repeatable mortality rolls
        }

        private static EconomyRulesConfigSO Rules()
        {
            JsonConfigRuntime.EnsureLoaded();
            var rules = JsonConfigRuntime.Get<EconomyRulesConfigSO>("economyrulesconfig");
            Assert.NotNull(rules, "economyrulesconfig preset must resolve.");
            return rules;
        }

        private static EconomyDatabaseSO FoodOnlyDatabase()
        {
            var database = new EconomyDatabaseSO();
            var wheat = new EconomyResourceDefinition();
            typeof(EconomyResourceDefinition)
                .GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(wheat, FoodId);
            typeof(EconomyResourceDefinition)
                .GetField("_category", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(wheat, EconomyResourceCategory.Food);
            typeof(EconomyDatabaseSO)
                .GetField("_resources", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(database, new List<EconomyResourceDefinition> { wheat });
            return database;
        }

        private static EconomySettlementState Settlement(
            params EconomyResidentState[] residents)
        {
            var state = new EconomySettlementState
            {
                SettlementId = "c04-settlement",
                SettlementName = "C04",
                OwnerId = "player_0",
                TotalHousingCapacity = 0,
            };
            state.Residents.AddRange(residents);
            return state;
        }

        private static EconomyResidentState Adult(float hp = 100f)
            => new EconomyResidentState(age: 30, hp: hp, comfort: 50f, houseCollapsed: false);

        [Test]
        public void UnmodeledNeed_ReportsNoPhantomConsumption()
        {
            // Database models only Food — Water resolves no resource ids, so
            // it is vacuously satisfied but must not enter the ledger.
            var state = Settlement(Adult());
            state.AddResource(FoodId, 50f);

            var result = _population.Tick(state, Rules(), FoodOnlyDatabase());

            Assert.AreEqual(1f, result.foodConsumed, Epsilon,
                "Adult eats exactly the configured 1.0 food/turn.");
            Assert.AreEqual(0f, result.waterConsumed, Epsilon,
                "Unmodeled Water need is satisfied but consumes nothing — no phantom ledger entry.");
            Assert.AreEqual(50f - 1f, state.GetResource(FoodId), Epsilon);
        }

        [Test]
        public void WellFedVersusStarving_SameAgeAndRng_OnlyStarvingLosesHp()
        {
            var rules = Rules();
            var database = FoodOnlyDatabase();

            var fed = Settlement(Adult());
            fed.AddResource(FoodId, 50f);
            _population.Tick(fed, rules, database);

            var starving = Settlement(Adult());
            _population.Tick(starving, rules, database);

            Assert.AreEqual(100f, fed.Residents[0].Hp, Epsilon,
                "A fed resident keeps full HP.");
            Assert.AreEqual(98f, starving.Residents[0].Hp, Epsilon,
                "A starving adult loses the configured -2 HP/turn.");
            Assert.AreEqual(fed.Residents[0].Age, starving.Residents[0].Age);
        }

        [Test]
        public void StarvingDeath_IsAttributedToHunger_NotOtherCauses()
        {
            var state = Settlement(Adult(hp: 1f)); // hp<=0 after one deficit tick
            var causes = new Dictionary<string, int>();

            var result = _population.Tick(state, Rules(), FoodOnlyDatabase(), causes);

            Assert.AreEqual(1, result.deaths);
            Assert.AreEqual(1, causes["hunger"], "Death by food deficit is hunger, not a generic loss.");
            Assert.AreEqual(1, causes.Count, "No other cause may be recorded for a hunger death.");
        }

        [Test]
        public void ColdDeath_IsAttributedToCold()
        {
            // Firewood is modeled (a resource literally id-matched to the
            // need) but the pool is empty → cold deficit; food is fed so the
            // cause must not be hunger.
            var database = FoodOnlyDatabase();
            var firewood = new EconomyResourceDefinition();
            typeof(EconomyResourceDefinition)
                .GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(firewood, "Firewood");
            typeof(EconomyResourceDefinition)
                .GetField("_category", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(firewood, EconomyResourceCategory.Materials);
            ((List<EconomyResourceDefinition>)typeof(EconomyDatabaseSO)
                .GetField("_resources", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(database)).Add(firewood);

            var state = Settlement(Adult(hp: 1f));
            state.AddResource(FoodId, 50f);

            var causes = new Dictionary<string, int>();
            var result = _population.Tick(state, Rules(), database, causes);

            Assert.AreEqual(1, result.deaths);
            Assert.AreEqual(1, causes["cold"], "Missing firewood with fed food is a cold death.");
        }

        [Test]
        public void CollapsedHouse_DeathIsAttributedToCollapse()
        {
            var state = Settlement(
                new EconomyResidentState(age: 30, hp: 100f, comfort: 50f, houseCollapsed: true));
            state.AddResource(FoodId, 50f);

            var causes = new Dictionary<string, int>();
            var result = _population.Tick(state, Rules(), FoodOnlyDatabase(), causes);

            Assert.AreEqual(1, result.deaths, "Collapse is a guaranteed death.");
            Assert.AreEqual(1, causes["collapse"]);
        }

        [Test]
        public void ElderlyFedResident_EventuallyDiesOfAge_Only()
        {
            // Age-tier mortality is the only nonzero component; over enough
            // ticks the death must be attributed to age, never hunger/cold.
            var state = Settlement(new EconomyResidentState(
                age: 76, hp: 100f, comfort: 50f, houseCollapsed: false));
            var database = FoodOnlyDatabase();
            var causes = new Dictionary<string, int>();

            for (int i = 0; i < 2000 && state.Residents.Count > 0; i++)
            {
                state.ResourcePool[FoodId] = 50f; // keep fed forever
                _population.Tick(state, Rules(), database, causes);
            }

            Assert.AreEqual(0, state.Residents.Count, "Age-tier mortality must eventually claim the elder.");
            Assert.IsFalse(causes.ContainsKey("hunger"), "A fed elder must never die of hunger.");
            Assert.IsFalse(causes.ContainsKey("cold"));
            Assert.IsTrue(causes.ContainsKey("age"), "The death must be attributed to age.");
        }

        [Test]
        public void EnlistedResidents_AreSkippedEntirely()
        {
            var enlisted = new EconomyResidentState(
                age: 30, hp: 0f, comfort: 0f, houseCollapsed: false,
                recruitmentQueueId: 42);
            var state = Settlement(enlisted);

            var result = _population.Tick(state, Rules(), FoodOnlyDatabase(),
                new Dictionary<string, int>());

            Assert.AreEqual(0, result.deaths,
                "Enlisted population is owned by recruitment/combat — not economy mortality.");
            Assert.AreEqual(1, state.Residents.Count);
            Assert.AreEqual(30, state.Residents[0].Age,
                "Enlisted residents must not age on economy ticks.");
        }

        [Test]
        public void Arrivals_RespectIntervalAndHousing()
        {
            var rules = Rules();
            var database = FoodOnlyDatabase();

            // Turn % interval == 0 only, and only with spare housing.
            var housed = Settlement(Adult());
            housed.TotalHousingCapacity = 5;
            housed.CurrentTurn = rules.Population.NewResidentsArrivalIntervalTurns;
            housed.AddResource(FoodId, 50f);
            var r1 = _population.Tick(housed, rules, database);
            Assert.AreEqual(1, r1.arrivals, "Spare housing admits the scheduled arrival.");

            var packed = Settlement(Adult());
            packed.TotalHousingCapacity = 1; // 1 resident, capacity 1 → full
            packed.CurrentTurn = rules.Population.NewResidentsArrivalIntervalTurns;
            packed.AddResource(FoodId, 50f);
            var r2 = _population.Tick(packed, rules, database);
            Assert.AreEqual(0, r2.arrivals, "Full housing blocks the arrival.");

            var offTurn = Settlement(Adult());
            offTurn.TotalHousingCapacity = 5;
            offTurn.CurrentTurn = rules.Population.NewResidentsArrivalIntervalTurns + 1;
            offTurn.AddResource(FoodId, 50f);
            var r3 = _population.Tick(offTurn, rules, database);
            Assert.AreEqual(0, r3.arrivals, "Arrivals only fire on the interval turn.");
        }

        [Test]
        public void ZeroResidents_TickIsSafeAndSilent()
        {
            var state = Settlement();
            var result = _population.Tick(state, Rules(), FoodOnlyDatabase(),
                new Dictionary<string, int>());
            Assert.AreEqual(0, result.arrivals);
            Assert.AreEqual(0, result.deaths);
            Assert.AreEqual(0f, result.foodConsumed, Epsilon);
        }

        [Test]
        public void DeathChance_AgeTierBoundariesMatchConfig()
        {
            var mortality = new EconomyComfortAndMortalityService();
            var rules = Rules();
            var none = new EconomyNeedSnapshot(0f, 0f, 0f, 0f);

            float at44 = mortality.CalculateDeathChance(rules,
                new EconomyResidentState(44, 100f, 50f, false), none);
            float at45 = mortality.CalculateDeathChance(rules,
                new EconomyResidentState(45, 100f, 50f, false), none);
            float at75 = mortality.CalculateDeathChance(rules,
                new EconomyResidentState(75, 100f, 50f, false), none);

            Assert.AreEqual(0f, at44, Epsilon, "Tier 0–44 carries no base death chance.");
            Assert.AreEqual(0.0005f, at45, Epsilon, "Tier 45–54 base chance from config.");
            Assert.AreEqual(0.01f, at75, Epsilon, "Tier 75+ base chance from config.");
        }
    }
}
