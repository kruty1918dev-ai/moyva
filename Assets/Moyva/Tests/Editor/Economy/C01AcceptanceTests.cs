using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kruty1918.JsonConfig;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C01 acceptance: farm, windmill and watermill are honest direct food
    /// producers — coherent role/cost/workers/yield, no "processing" claims on
    /// input-less recipes, and a sustainable 15-resident food chain over a
    /// 50-tick ledger with identical 1X/2X per-tick outcomes.
    /// </summary>
    public class C01AcceptanceTests
    {
        private const string FoodId = "wheat-bundle-food-resources";
        private const float Epsilon = 0.0001f;

        private static readonly string[] ProducerIds = { "farm", "windmill-01", "watermill" };

        private static BuildingDefinition Producer(string id)
        {
            JsonConfigRuntime.EnsureLoaded();
            var asset = JsonConfigRuntime.Get<BuildingDefinitionAsset>(id);
            Assert.NotNull(asset, $"Building preset '{id}' must resolve.");
            return asset.ToRuntimeDefinition();
        }

        private static ProductionBuildingModule ProductionModule(BuildingDefinition def)
            => def.Modules?.OfType<ProductionBuildingModule>().FirstOrDefault();

        private static WorkforceBuildingModule WorkforceModule(BuildingDefinition def)
            => def.Modules?.OfType<WorkforceBuildingModule>().FirstOrDefault();

        private static EconomyRulesConfigSO Rules()
        {
            var rules = JsonConfigRuntime.Get<EconomyRulesConfigSO>("economyrulesconfig");
            Assert.NotNull(rules, "economyrulesconfig preset must resolve.");
            return rules;
        }

        // A detached copy of the authored rules with every mortality component
        // zeroed: the food-chain ledger tests isolate production/consumption
        // arithmetic from demographic noise. The shared frozen instance is
        // never mutated.
        private static EconomyRulesConfigSO IsolatedFoodLedgerRules()
        {
            var clone = JsonUtility.FromJson<EconomyRulesConfigSO>(
                JsonUtility.ToJson(Rules()));
            Assert.NotNull(clone, "Rules clone must succeed.");
            var mortality = clone.Mortality;
            SetPrivate("_ageTiers", mortality,
                new List<EconomyMortalityAgeTier>());
            SetPrivate("_hungerWeight", mortality, 0f);
            SetPrivate("_coldWeight", mortality, 0f);
            SetPrivate("_diseaseWeight", mortality, 0f);
            SetPrivate("_warWeight", mortality, 0f);
            SetPrivate("_collapseDeathChance", mortality, 0f);
            return clone;
        }

        private static void SetPrivate(string name, object target, object value)
            => target.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(target, value);

        private static float AdultFoodPerTurn(EconomyRulesConfigSO rules)
            => rules.Consumption.AgeConsumption
                .First(r => r.Label == "Adult").FoodPerTurn;

        private static bool ClaimsProcessing(string description)
        {
            if (string.IsNullOrEmpty(description))
                return false;
            string lower = description.ToLowerInvariant();
            return lower.Contains("перероб")
                   || lower.Contains("grind")
                   || lower.Contains("process");
        }

        [TestCase("farm")]
        [TestCase("windmill-01")]
        [TestCase("watermill")]
        public void Producer_Recipe_IsHonestAndSane(string buildingId)
        {
            var def = Producer(buildingId);
            var module = ProductionModule(def);
            Assert.NotNull(module, $"{buildingId} must carry a production module.");
            Assert.IsNotEmpty(module.Recipes, $"{buildingId} must define at least one recipe.");
            Assert.AreEqual(BuildingRole.Production, def.Role, $"{buildingId} role must be Production.");
            Assert.Greater(def.BuildTurns, 0, $"{buildingId} buildTurns must be positive.");
            Assert.Greater(def.ConstructionCost.Sum(c => c.Amount), 0,
                $"{buildingId} must cost something.");
            int workers = WorkforceModule(def)?.WorkersRequired ?? 0;
            Assert.Greater(workers, 0, $"{buildingId} must require workers.");

            foreach (var recipe in module.Recipes)
            {
                Assert.IsNotEmpty(recipe.Outputs, $"{buildingId} recipe must produce output.");
                Assert.GreaterOrEqual(recipe.TurnsPerCycle, 1);
                Assert.IsTrue(recipe.RequiresWorkers,
                    $"{buildingId} recipe must not run workerless.");
                foreach (var output in recipe.Outputs)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(output.ResourceId));
                    Assert.Greater(output.Amount, 0f);
                }
                // A mill/farm whose recipe has no inputs cannot claim to
                // process grain — text must match actual behavior.
                bool inputless = recipe.Inputs == null || recipe.Inputs.Count == 0
                    || recipe.Inputs.All(i => i == null || i.Amount <= 0f);
                if (inputless)
                {
                    Assert.IsFalse(ClaimsProcessing(def.Description),
                        $"{buildingId} description claims processing but the recipe has no inputs.");
                }
            }
        }

        [Test]
        public void Producers_HaveMeaningfulDistinctCosts()
        {
            // The windmill 1+1+1 cost was a placeholder; industrial machinery
            // must cost at least the farm's baseline.
            int farm = Producer("farm").ConstructionCost.Sum(c => c.Amount);
            int windmill = Producer("windmill-01").ConstructionCost.Sum(c => c.Amount);
            int watermill = Producer("watermill").ConstructionCost.Sum(c => c.Amount);

            Assert.GreaterOrEqual(windmill, farm,
                "Windmill must not undercut the farm's baseline cost.");
            Assert.Greater(watermill, windmill,
                "Watermill is the premium water-locked producer.");
        }

        [Test]
        public void Producers_NetFoodPerTurn_IsPositive()
        {
            float foodRate = AdultFoodPerTurn(Rules());
            foreach (string id in ProducerIds)
            {
                var def = Producer(id);
                var module = ProductionModule(def);
                int workers = WorkforceModule(def)?.WorkersRequired ?? 0;
                float grossPerTurn = module.Recipes
                    .SelectMany(r => r.Outputs)
                    .Where(o => o != null && o.ResourceId == FoodId)
                    .Sum(o => o.Amount) / module.Recipes.Max(r => r.TurnsPerCycle);
                float net = grossPerTurn - workers * foodRate;
                Assert.Greater(net, 0f,
                    $"{id}: net food/turn {net} must stay positive after feeding its own workers.");
            }
        }

        // --- 50-tick ledger simulation ---

        private static EconomyDatabaseSO MinimalFoodDatabase()
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

        private static EconomyBuildingState BuildingFromPreset(
            string key, BuildingDefinition def)
        {
            var module = ProductionModule(def);
            return new EconomyBuildingState
            {
                InstanceKey = key,
                BuildingId = def.Id,
                RequiredWorkers = WorkforceModule(def)?.WorkersRequired ?? module.WorkersRequired,
                ProductionRecipes = new List<ProductionRecipeDefinition>(module.Recipes),
                IsActive = true,
            };
        }

        private static EconomySettlementState Settlement(int residents, int farms, float seedFood, int? housing = null)
        {
            var state = new EconomySettlementState
            {
                SettlementId = "c01-settlement",
                SettlementName = "C01",
                OwnerId = "player_0",
                IsActive = true,
                // Headroom above the resident count lets the authored arrival
                // cadence (1 resident / 10 turns) backfill workers that age
                // out of the 16..59 workforce window during a 50-tick run.
                TotalHousingCapacity = housing ?? residents + 8,
            };
            // Mixed ages spread workforce retirement instead of a mass exit.
            for (int i = 0; i < residents; i++)
                state.Residents.Add(new EconomyResidentState(age: 18 + (i % 20), hp: 100f, comfort: 50f, houseCollapsed: false));
            if (seedFood > 0f)
                state.AddResource(FoodId, seedFood);
            var farm = Producer("farm");
            for (int i = 0; i < farms; i++)
                state.Buildings.Add(BuildingFromPreset($"farm-{i}", farm));
            return state;
        }

        private static List<float> RunTicks(
            EconomySettlementState state,
            EconomyDatabaseSO database,
            EconomyRulesConfigSO rules,
            int ticks,
            float turnDurationSeconds,
            out (int cycles, float consumed, int deaths) totals)
        {
            UnityEngine.Random.InitState(0xC01);
            var orchestrator = new EconomyTickOrchestrator();
            var ledger = new List<float>(ticks);
            int cycles = 0, deaths = 0;
            float consumed = 0f;
            for (int i = 0; i < ticks; i++)
            {
                var result = orchestrator.Tick(state, database, rules, turnDurationSeconds);
                cycles += result.ProductionCyclesCompleted;
                deaths += result.Deaths;
                consumed += result.TotalFoodConsumed;
                ledger.Add(state.GetResource(FoodId));
            }
            totals = (cycles, consumed, deaths);
            return ledger;
        }

        [Test]
        public void FoodChain_TwoFarms_Sustains15ResidentsOver50Ticks()
        {
            var rules = IsolatedFoodLedgerRules();
            var database = MinimalFoodDatabase();
            var state = Settlement(residents: 15, farms: 2, seedFood: 20f);

            var ledger = RunTicks(state, database, rules, 50, 60f, out var totals);

            Assert.AreEqual(0, totals.deaths,
                "A fed settlement must not starve anyone.");
            Assert.Greater(state.Residents.Count, 0,
                "A fed settlement must not collapse.");
            Assert.GreaterOrEqual(totals.cycles, 90,
                "Two 1-turn-cycle farms must complete ~100 cycles over 50 ticks " +
                "(worker retirement only briefly interrupts staffing).");
            Assert.Greater(ledger[^1], 0f,
                "Two farms (16 wheat/turn vs ~15 consumption) never flatline.");
            Assert.Greater(totals.consumed, 0f,
                "Consumption is modeled — residents ate from the pool.");
        }

        [Test]
        public void NoFoodChain_StarvesPopulation()
        {
            var rules = Rules();
            var database = MinimalFoodDatabase();
            var state = Settlement(residents: 15, farms: 0, seedFood: 0f, housing: 0);

            int before = state.Residents.Count;
            int deaths = 0;
            var orchestrator = new EconomyTickOrchestrator();
            for (int i = 0; i < 60; i++)
                deaths += orchestrator.Tick(state, database, rules).Deaths;

            Assert.Greater(deaths, 0,
                "Residents with no food must starve — deaths by HP loss are guaranteed.");
            Assert.Less(state.Residents.Count, before);
        }

        [Test]
        public void FoodChain_OneVersusTwoFarms_BreakEvenIsHonest()
        {
            // Break-even: 15 adults need ~15 food/turn; one farm nets 7,
            // two farms net 14 — plus foraging. Measure the actual ledger
            // instead of asserting invented target numbers.
            var rules = IsolatedFoodLedgerRules();
            var database = MinimalFoodDatabase();

            var one = Settlement(15, 1, 20f);
            var ledger1 = RunTicks(one, database, rules, 50, 60f, out _);
            var two = Settlement(15, 2, 20f);
            var ledger2 = RunTicks(two, database, rules, 50, 60f, out _);

            Assert.Less(ledger1[^1], ledger2[^1],
                "A second farm must measurably improve the food position.");
            Assert.AreEqual(rules.Population.InitialResidents, 15,
                "Test assumes the authored 15-resident start.");
        }

        [Test]
        public void FoodChain_SameOutcomeAt1xAnd2x_ForEqualTickCount()
        {
            // Speed changes wall-time per tick, not per-tick simulation
            // outcome — equal tick counts must give equal ledgers.
            var rules = IsolatedFoodLedgerRules();
            var database = MinimalFoodDatabase();
            var fast = RunTicks(Settlement(15, 2, 20f), database, rules, 50, 30f, out _);
            var slow = RunTicks(Settlement(15, 2, 20f), database, rules, 50, 60f, out _);

            Assert.AreEqual(fast.Count, slow.Count);
            for (int i = 0; i < fast.Count; i++)
                Assert.AreEqual(slow[i], fast[i], Epsilon,
                    $"Tick {i} ledger differs between 1X and 2X speed.");
        }
    }
}
