using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// Population needs ("Food", "Water", ...) are abstract ids — the
    /// settlement pool only stores concrete resource ids like
    /// 'steak-food-resources'. Exact-id consumption starved every resident,
    /// deactivated the settlement and left construction funding empty, which
    /// disabled the whole construction list.
    /// </summary>
    public class EconomyPopulationNeedTests
    {
        private readonly EconomyConsumptionService _consumption = new EconomyConsumptionService();

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(
                name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Field '{name}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static EconomyResourceDefinition Resource(string id, EconomyResourceCategory category)
        {
            var definition = new EconomyResourceDefinition();
            SetField(definition, "_id", id);
            SetField(definition, "_category", category);
            return definition;
        }

        private static EconomyDatabaseSO Database(params EconomyResourceDefinition[] resources)
        {
            var database = new EconomyDatabaseSO();
            SetField(database, "_resources", new List<EconomyResourceDefinition>(resources));
            return database;
        }

        private static EconomySettlementState Settlement()
            => new EconomySettlementState
            {
                SettlementId = "settlement-a",
                SettlementName = "Alpha",
                OwnerId = "player_0",
            };

        [Test]
        public void ResolveNeedResourceIds_FoodNeed_MatchesFoodCategoryResources()
        {
            var database = Database(
                Resource("steak-food-resources", EconomyResourceCategory.Food),
                Resource("bread-food-resources", EconomyResourceCategory.Food),
                Resource("walnut-wood-materials-resources", EconomyResourceCategory.Materials));

            CollectionAssert.AreEquivalent(
                new[] { "steak-food-resources", "bread-food-resources" },
                _consumption.ResolveNeedResourceIds(database, "Food"));
        }

        [Test]
        public void ResolveNeedResourceIds_ExactId_MatchesResourceWithoutCategory()
        {
            var database = Database(
                Resource("Food", EconomyResourceCategory.None),
                Resource("walnut-wood-materials-resources", EconomyResourceCategory.Materials));

            CollectionAssert.AreEquivalent(
                new[] { "Food" },
                _consumption.ResolveNeedResourceIds(database, "Food"));
        }

        [Test]
        public void ResolveNeedResourceIds_UnmodeledNeeds_ReturnEmpty()
        {
            var database = Database(
                Resource("steak-food-resources", EconomyResourceCategory.Food));

            Assert.IsEmpty(_consumption.ResolveNeedResourceIds(database, "Water"));
            Assert.IsEmpty(_consumption.ResolveNeedResourceIds(database, "Firewood"));
            Assert.IsEmpty(_consumption.ResolveNeedResourceIds(database, "Clothing"));
        }

        [Test]
        public void ConsumeNeed_SpendsOnlyResourcesSatisfyingTheNeed()
        {
            var database = Database(
                Resource("steak-food-resources", EconomyResourceCategory.Food),
                Resource("walnut-wood-materials-resources", EconomyResourceCategory.Materials));
            var state = Settlement();
            state.AddResource("steak-food-resources", 10f);
            state.AddResource("walnut-wood-materials-resources", 50f);

            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", 4f));
            Assert.AreEqual(6f, state.GetResource("steak-food-resources"), 0.001f);
            Assert.AreEqual(50f, state.GetResource("walnut-wood-materials-resources"), 0.001f,
                "Materials must not be spent on a food need.");
        }

        [Test]
        public void ConsumeNeed_ModeledShortage_IsAllOrNothing()
        {
            var database = Database(
                Resource("steak-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.AddResource("steak-food-resources", 3f);

            Assert.IsFalse(_consumption.ConsumeNeed(state, database, "Food", 5f));
            Assert.AreEqual(3f, state.GetResource("steak-food-resources"), 0.001f);
        }

        [Test]
        public void ConsumeNeed_UnmodeledNeed_IsVacuouslySatisfied()
        {
            var database = Database(
                Resource("steak-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();

            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Water", 5f));
            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Firewood", 5f));
            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Clothing", 5f));
        }

        [Test]
        public void HasNeedDeficit_TrueOnlyForModeledEmptyNeeds()
        {
            var database = Database(
                Resource("steak-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();

            Assert.IsTrue(_consumption.HasNeedDeficit(state, database, "Food"),
                "A modeled need with an empty pool must report a deficit.");
            Assert.IsFalse(_consumption.HasNeedDeficit(state, database, "Water"),
                "An unmodeled need must never report a deficit.");

            state.AddResource("steak-food-resources", 1f);
            Assert.IsFalse(_consumption.HasNeedDeficit(state, database, "Food"));
        }

        [Test]
        public void Tick_ResidentsFedByCategoryResources_SettlementStaysActive()
        {
            // Regression: starter resources use concrete ids/categories, so a
            // stocked warehouse must keep residents alive.
            var database = Database(
                Resource("steak-food-resources", EconomyResourceCategory.Food));
            var rules = new EconomyRulesConfigSO();
            var state = Settlement();
            state.AddResource("steak-food-resources", 100f);
            for (int i = 0; i < 15; i++)
                state.Residents.Add(new EconomyResidentState(20, 100f, 50f, false));

            var result = new EconomyTickOrchestrator().Tick(state, database, rules);

            Assert.AreEqual(0, result.Deaths);
            Assert.AreEqual(15, state.Residents.Count);
            Assert.AreEqual(15f, result.TotalFoodConsumed, 0.001f,
                "15 adults consume 1 food per turn.");
            Assert.IsTrue(state.IsActive);
            Assert.LessOrEqual(state.GetResource("steak-food-resources"), 85f);
            for (int i = 0; i < state.Residents.Count; i++)
                Assert.AreEqual(100f, state.Residents[i].Hp, 0.001f,
                    "Unmodeled needs (Water/Firewood/Clothing) must not drain HP.");
        }

        [Test]
        public void Tick_ModeledFoodMissing_ResidentStillStarves()
        {
            var database = Database(
                Resource("steak-food-resources", EconomyResourceCategory.Food));
            var rules = new EconomyRulesConfigSO();
            var state = Settlement();
            state.Residents.Add(new EconomyResidentState(20, 1f, 50f, false));

            var result = new EconomyTickOrchestrator().Tick(state, database, rules);

            Assert.AreEqual(1, result.Deaths);
            Assert.AreEqual(0, state.Residents.Count);
            Assert.IsFalse(state.IsActive,
                "DeactivateSettlementWhenPopulationIsZero defaults to true.");
        }

        [Test]
        public void Tick_UnmodeledNeedsOnly_LowHpResidentSurvives()
        {
            // Water/Firewood/Clothing are not in the resource database — they
            // must not apply starvation/cold/clothing penalties.
            var database = Database(
                Resource("steak-food-resources", EconomyResourceCategory.Food));
            var rules = new EconomyRulesConfigSO();
            var state = Settlement();
            state.AddResource("steak-food-resources", 10f);
            state.Residents.Add(new EconomyResidentState(20, 1f, 50f, false));

            var result = new EconomyTickOrchestrator().Tick(state, database, rules);

            Assert.AreEqual(0, result.Deaths);
            Assert.AreEqual(1, state.Residents.Count);
            Assert.AreEqual(1f, state.Residents[0].Hp, 0.001f);
            Assert.IsTrue(state.IsActive);
        }
    }
}
