using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C03 acceptance: need consumption spends only available (unreserved)
    /// concrete resources, keeps all-or-nothing semantics, and keeps modeled
    /// zero-stock needs distinct from unmodeled ones.
    /// </summary>
    public class C03AcceptanceTests
    {
        private const float Epsilon = 0.0001f;

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
        public void ConsumeNeed_RefusesWhenReservedStockExceedsDemand_WithoutWriteOff()
        {
            var database = Database(Resource("bread-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.EnsureWarehousePool("0:0");
            state.AddResource("bread-food-resources", 10f, "0:0");
            state.ReserveResourceAt("0:0", "bread-food-resources", 7f);

            Assert.AreEqual(3f, _consumption.GetAvailableNeedAmount(state, database, "Food"), Epsilon);
            Assert.IsFalse(_consumption.ConsumeNeed(state, database, "Food", 4f),
                "Demand above the unreserved stock must be refused.");

            Assert.AreEqual(10f, state.GetResource("bread-food-resources"), Epsilon,
                "Refused consumption must not write anything off.");
            Assert.AreEqual(7f, state.GetTotalReservedResource("bread-food-resources"), Epsilon);
            Assert.AreEqual(10f, state.WarehouseResourcePools["0:0"]["bread-food-resources"], Epsilon);
        }

        [Test]
        public void ConsumeNeed_SpendsUnreservedStock_KeepingReservationIntact()
        {
            var database = Database(Resource("bread-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.EnsureWarehousePool("0:0");
            state.AddResource("bread-food-resources", 10f, "0:0");
            state.ReserveResourceAt("0:0", "bread-food-resources", 7f);

            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", 3f));

            Assert.AreEqual(7f, state.GetResource("bread-food-resources"), Epsilon);
            Assert.AreEqual(7f, state.GetTotalReservedResource("bread-food-resources"), Epsilon,
                "Reservation must survive generic consumption.");
            Assert.AreEqual(7f, state.WarehouseResourcePools["0:0"]["bread-food-resources"], Epsilon,
                "Warehouse keeps the physically reserved stock.");
            Assert.AreEqual(0f, state.GetAvailableResource("bread-food-resources"), Epsilon);
        }

        [Test]
        public void ConsumeNeed_SplitsAcrossFoodIds_InDatabaseOrder()
        {
            var database = Database(
                Resource("bread-food-resources", EconomyResourceCategory.Food),
                Resource("steak-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.AddResource("bread-food-resources", 2f);
            state.AddResource("steak-food-resources", 5f);

            Assert.AreEqual(7f, _consumption.GetAvailableNeedAmount(state, database, "Food"), Epsilon);
            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", 4f),
                "Combined stock across two food ids covers the demand.");

            Assert.AreEqual(0f, state.GetResource("bread-food-resources"), Epsilon,
                "The id listed first in the database is drained first.");
            Assert.AreEqual(3f, state.GetResource("steak-food-resources"), Epsilon);
        }

        [Test]
        public void ConsumeNeed_DatabaseOrderDecidesWhichIdDrainsFirst()
        {
            var database = Database(
                Resource("steak-food-resources", EconomyResourceCategory.Food),
                Resource("bread-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.AddResource("bread-food-resources", 2f);
            state.AddResource("steak-food-resources", 5f);

            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", 4f));

            Assert.AreEqual(1f, state.GetResource("steak-food-resources"), Epsilon,
                "Steak is listed first, so it drains first.");
            Assert.AreEqual(2f, state.GetResource("bread-food-resources"), Epsilon);
        }

        [Test]
        public void ConsumeNeed_CombinedShortage_IsAllOrNothingAcrossIds()
        {
            var database = Database(
                Resource("bread-food-resources", EconomyResourceCategory.Food),
                Resource("steak-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.AddResource("bread-food-resources", 2f);
            state.AddResource("steak-food-resources", 2f);

            Assert.IsFalse(_consumption.ConsumeNeed(state, database, "Food", 5f));

            Assert.AreEqual(2f, state.GetResource("bread-food-resources"), Epsilon);
            Assert.AreEqual(2f, state.GetResource("steak-food-resources"), Epsilon);
        }

        [Test]
        public void ConsumeNeed_DuplicateResourceIds_DoNotDoubleCountAvailable()
        {
            // A malformed database can list the same concrete id twice; the
            // available amount must reflect the pool once, not per definition.
            var database = Database(
                Resource("bread-food-resources", EconomyResourceCategory.Food),
                Resource("bread-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.AddResource("bread-food-resources", 5f);

            Assert.AreEqual(5f, _consumption.GetAvailableNeedAmount(state, database, "Food"), Epsilon,
                "Duplicate definitions must not double-count the same pool entry.");
            Assert.IsFalse(_consumption.ConsumeNeed(state, database, "Food", 6f));
            Assert.AreEqual(5f, state.GetResource("bread-food-resources"), Epsilon,
                "A refused consumption must stay all-or-nothing even with duplicate ids.");
            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", 5f));
            Assert.AreEqual(0f, state.GetResource("bread-food-resources"), Epsilon);
        }

        [Test]
        public void ConsumeNeed_ExactIdAndCategoryMatches_BothSatisfyTheNeed()
        {
            var database = Database(
                Resource("Food", EconomyResourceCategory.None),
                Resource("bread-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.AddResource("Food", 2f);
            state.AddResource("bread-food-resources", 2f);

            Assert.AreEqual(4f, _consumption.GetAvailableNeedAmount(state, database, "Food"), Epsilon);
            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", 4f));
            Assert.AreEqual(0f, state.GetResource("Food"), Epsilon);
            Assert.AreEqual(0f, state.GetResource("bread-food-resources"), Epsilon);
        }

        [Test]
        public void ConsumeNeed_ZeroNegativeOrUnmodeledInput_IsVacuouslySatisfied()
        {
            var database = Database(Resource("bread-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.AddResource("bread-food-resources", 5f);

            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", 0f));
            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", -3f));
            Assert.IsTrue(_consumption.ConsumeNeed(state, database, null, 5f));
            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "", 5f));
            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "   ", 5f));
            Assert.IsTrue(_consumption.ConsumeNeed(state, null, "Food", 5f),
                "A missing database means nothing is modeled, so the need is satisfied.");

            Assert.AreEqual(5f, state.GetResource("bread-food-resources"), Epsilon,
                "Vacuous satisfaction must not touch the pool.");
        }

        [Test]
        public void ConsumeNeed_NonFiniteAmount_NeverWritesOff()
        {
            var database = Database(Resource("bread-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.AddResource("bread-food-resources", 5f);

            Assert.IsFalse(_consumption.ConsumeNeed(state, database, "Food", float.NaN));
            Assert.IsFalse(_consumption.ConsumeNeed(state, database, "Food", float.PositiveInfinity));

            Assert.AreEqual(5f, state.GetResource("bread-food-resources"), Epsilon);
        }

        [Test]
        public void ConsumeNeed_FractionalAmounts_AndToleranceBoundaries()
        {
            var database = Database(Resource("bread-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.AddResource("bread-food-resources", 3f);

            Assert.IsFalse(_consumption.ConsumeNeed(state, database, "Food", 3.0002f),
                "Demand beyond available plus tolerance must fail.");
            Assert.AreEqual(3f, state.GetResource("bread-food-resources"), Epsilon);

            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", 1.5f));
            Assert.AreEqual(1.5f, state.GetResource("bread-food-resources"), Epsilon);

            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", 1.50005f),
                "Demand within the 0.0001 tolerance of the remainder succeeds.");
            Assert.AreEqual(0f, state.GetResource("bread-food-resources"), Epsilon);
        }

        [Test]
        public void NeedQueries_DoNotMutateSettlementState()
        {
            var database = Database(
                Resource("bread-food-resources", EconomyResourceCategory.Food),
                Resource("steak-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.EnsureWarehousePool("0:0");
            state.AddResource("bread-food-resources", 10f, "0:0");
            state.AddResource("steak-food-resources", 4f, "0:0");
            state.ReserveResourceAt("0:0", "bread-food-resources", 7f);

            var poolBefore = new Dictionary<string, float>(state.ResourcePool);
            var reservedBefore = state.GetReservedSnapshot();
            var warehouseBefore = state.GetWarehouseSnapshot("0:0");

            _consumption.ResolveNeedResourceIds(database, "Food");
            _consumption.ResolveNeedResourceIds(database, "Water");
            _consumption.GetAvailableNeedAmount(state, database, "Food");
            _consumption.GetAvailableNeedAmount(state, database, "Water");
            _consumption.HasNeedDeficit(state, database, "Food");
            _consumption.HasNeedDeficit(state, database, "Water");
            _consumption.ConsumeNeed(state, database, "Food", 100f); // refused, must not mutate

            CollectionAssert.AreEquivalent(poolBefore, state.ResourcePool);
            CollectionAssert.AreEquivalent(reservedBefore, state.GetReservedSnapshot());
            CollectionAssert.AreEquivalent(warehouseBefore, state.GetWarehouseSnapshot("0:0"));
        }

        [Test]
        public void ModeledZeroStock_ReportsDeficit_UnmodeledStaysSatisfied()
        {
            var database = Database(Resource("bread-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();

            Assert.IsTrue(_consumption.HasNeedDeficit(state, database, "Food"),
                "A modeled need with zero stock reports a deficit.");
            Assert.IsFalse(_consumption.HasNeedDeficit(state, database, "Water"),
                "An unmodeled need never reports a deficit.");
            Assert.IsFalse(_consumption.ConsumeNeed(state, database, "Food", 1f));
            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Water", 1f));
        }

        [Test]
        public void ModeledFullyReserved_ReportsDeficit_AndRefusesConsumption()
        {
            var database = Database(Resource("bread-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.EnsureWarehousePool("0:0");
            state.AddResource("bread-food-resources", 5f, "0:0");
            state.ReserveResourceAt("0:0", "bread-food-resources", 5f);

            Assert.AreEqual(0f, _consumption.GetAvailableNeedAmount(state, database, "Food"), Epsilon);
            Assert.IsTrue(_consumption.HasNeedDeficit(state, database, "Food"),
                "Fully reserved modeled stock counts as a deficit for generic needs.");
            Assert.IsFalse(_consumption.ConsumeNeed(state, database, "Food", 1f));
            Assert.AreEqual(5f, state.GetResource("bread-food-resources"), Epsilon);
        }

        [Test]
        public void ConsumeNeed_NeverSpendsOrCreatesAbstractNeedIdInPool()
        {
            var database = Database(
                Resource("bread-food-resources", EconomyResourceCategory.Food),
                Resource("steak-food-resources", EconomyResourceCategory.Food));
            var state = Settlement();
            state.AddResource("Food", 100f); // stray abstract entry, not modeled by the database
            state.AddResource("bread-food-resources", 3f);
            state.AddResource("steak-food-resources", 3f);

            Assert.IsTrue(_consumption.ConsumeNeed(state, database, "Food", 4f));

            Assert.AreEqual(100f, state.GetResource("Food"), Epsilon,
                "The abstract need id in the pool is not spendable unless modeled as a resource.");
            Assert.AreEqual(2f, state.GetResource("bread-food-resources") + state.GetResource("steak-food-resources"), Epsilon);
        }
    }
}
