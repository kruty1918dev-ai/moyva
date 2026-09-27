using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C21: one orchestrator tick explains every resource/population change.
    /// Authored order: turn++ → population (aging/consumption/death) → worker
    /// allocation → foraging → production → deactivation. These tests lock the
    /// order in: consumption precedes production output, and foraged stock is
    /// visible to recipes in the same tick.
    /// </summary>
    public sealed class C21AcceptanceTests
    {
        private EconomyTickOrchestrator _orchestrator;
        private EconomyRulesConfigSO _rules;
        private EconomyDatabaseSO _database;

        [SetUp]
        public void SetUp()
        {
            _orchestrator = new EconomyTickOrchestrator();
            _rules = new EconomyRulesConfigSO();
            _database = new EconomyDatabaseSO();
            var grain = new EconomyResourceDefinition();
            SetField(grain, "_id", "grain-food-resources");
            SetField(grain, "_category", EconomyResourceCategory.Food);
            SetField(_database, "_resources",
                new List<EconomyResourceDefinition> { grain });
        }

        private static EconomySettlementState Settlement(int residents, int housing = 0)
        {
            var state = new EconomySettlementState
            {
                SettlementId = "s-1",
                OwnerId = "owner-a",
                IsActive = true,
                TotalHousingCapacity = housing,
            };
            // An unpolicied warehouse accepts any deposit — recipes keep their
            // default RequiresStorageSpace=true semantics.
            state.WarehouseResourcePools["wh-1"] = new Dictionary<string, float>();
            for (int i = 0; i < residents; i++)
                state.Residents.Add(new EconomyResidentState(
                    age: 30, hp: 100, comfort: 60f, houseCollapsed: false));
            return state;
        }

        private static EconomyBuildingState Building(string id,
            string outputResource, float outputAmount, string inputResource = null)
        {
            var building = new EconomyBuildingState
            {
                InstanceKey = id,
                BuildingId = id,
                IsActive = true,
            };
            building.ProductionRecipes.Add(new ProductionRecipeDefinition
            {
                RecipeId = id + "-recipe",
                TurnsPerCycle = 1,
                RequiresWorkers = false,
                Inputs = inputResource == null
                    ? new List<BuildingResourceAmount>()
                    : new List<BuildingResourceAmount>
                    {
                        new BuildingResourceAmount { ResourceId = inputResource, Amount = 1 },
                    },
                Outputs = new List<BuildingResourceAmount>
                {
                    new BuildingResourceAmount { ResourceId = outputResource, Amount = (int)outputAmount },
                },
            });
            return building;
        }

        [Test]
        public void Tick_IncrementsTurn_AndReturnsTrace()
        {
            var state = Settlement(residents: 2);
            var result = _orchestrator.Tick(state, _database, _rules);
            Assert.AreEqual(1, state.CurrentTurn);
            Assert.AreEqual(1, result.Turn);
            Assert.AreEqual(state.Residents.Count, result.TotalPopulation);
            Assert.IsNotNull(result.ForagedResources);
        }

        [Test]
        public void Consumption_RunsBeforeProductionOutput()
        {
            // Decisive order probe: a starving resident (hp 1) dies from the
            // food deficit even though a farm outputs 50 grain this same tick.
            // If production ran before consumption, the food would feed them.
            var state = Settlement(residents: 1, housing: 10);
            state.Residents[0] = new EconomyResidentState(
                age: 30, hp: 1, comfort: 60f, houseCollapsed: false);
            state.Buildings.Add(Building("farm", "grain-food-resources", 50f));

            var result = _orchestrator.Tick(state, _database, _rules);

            Assert.AreEqual(1, result.Deaths,
                "Consumption precedes production: the empty pool starves the resident "
                + "before this tick's harvest exists.");
            float expected = 50f;
            var production = _rules?.Production;
            if (production != null && production.EnableFoodDecay
                && production.FoodDecayPerTurn > 0f)
                expected *= 1f - production.FoodDecayPerTurn;
            Assert.AreEqual(expected, state.GetResource("grain-food-resources"), 0.001f,
                "The harvest still landed (minus same-tick decay) — order is "
                + "consumption-then-production.");
        }

        [Test]
        public void Foraging_OutputFeedsSameTickProduction()
        {
            // Foraging (step 3) precedes production (step 4): a recipe whose
            // input only arrives via foraging completes in the same tick.
            var state = Settlement(residents: 1);
            state.CurrentTurn = 4; // next tick = 5 → walnut interval hits
            state.Buildings.Add(Building("mill",
                "grain-food-resources", 4f,
                inputResource: "walnut-wood-materials-resources"));

            var result = _orchestrator.Tick(state, _database, _rules);

            Assert.IsTrue(result.ForagedResources.Exists(
                kv => kv.Key == "walnut-wood-materials-resources"));
            Assert.AreEqual(1, result.ProductionCyclesCompleted,
                "Foraged walnut became recipe input in the same tick.");
        }

        [Test]
        public void ZeroResidents_DeactivatesSettlement()
        {
            var state = Settlement(residents: 0);
            var result = _orchestrator.Tick(state, _database, _rules);
            Assert.AreEqual(0, result.TotalPopulation);
            Assert.IsFalse(state.IsActive,
                "DeactivateSettlementWhenPopulationIsZero defaults to true.");
        }

        [Test]
        public void Tick_NullState_IsSafe()
        {
            var result = _orchestrator.Tick(null, _database, _rules);
            Assert.AreEqual(0, result.Turn);
        }

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field {name} missing on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
