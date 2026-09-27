using System.Collections.Generic;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C07 — resource conservation across owner pool, settlement aggregate
    /// and warehouse views: transfer not copy, no double-counting, capacity
    /// and whitelist enforcement, removal/relocation, owner isolation.
    /// </summary>
    public class C07AcceptanceTests
    {
        private const float Epsilon = 0.0001f;

        private static EconomySettlementState Settlement(
            string id = "s1", string owner = "owner-1")
        {
            return new EconomySettlementState
            {
                SettlementId = id,
                OwnerId = owner,
                IsActive = true
            };
        }

        [Test]
        public void FirstWarehouse_Transfers_NotCopies_OwnerPool()
        {
            var pools = new EconomyOwnerResourcePoolService();
            pools.AddOwnerResource("owner-1", "wood", 40f, null);
            pools.AddOwnerResource("owner-1", "food", 25f, null);

            var state = Settlement();
            state.EnsureWarehousePool("wh-0");
            var settlements = new Dictionary<string, EconomySettlementState>
                { ["s1"] = state };

            Assert.IsTrue(pools.TransferOwnerResourcesToFirstWarehouse(
                "owner-1", settlements, null, "test"));

            Assert.IsEmpty(
                pools.GetOwnerPoolResourceTotals("owner-1"),
                "Owner pool must be emptied — stock moved, not duplicated");
            Assert.AreEqual(40f, state.GetResource("wood"), Epsilon);
            Assert.AreEqual(25f, state.GetResource("food"), Epsilon);
            Assert.AreEqual(
                40f,
                state.GetWarehouseSnapshot("wh-0")["wood"],
                Epsilon,
                "Warehouse view must back the settlement aggregate");
        }

        [Test]
        public void OwnerTotals_NeverDoubleCount_WarehouseViews()
        {
            var pools = new EconomyOwnerResourcePoolService();
            var state = Settlement();
            state.EnsureWarehousePool("wh-0");
            state.AddResource("wood", 10f, "wh-0");
            var settlements = new Dictionary<string, EconomySettlementState>
                { ["s1"] = state };

            var totals = pools.GetOwnerResourceTotals(settlements, "owner-1");

            Assert.AreEqual(
                10f, totals["wood"], Epsilon,
                "Physical stock is counted once — warehouse pools are views");
        }

        [Test]
        public void FiniteCapacity_BlocksOverflow_Reservation()
        {
            var state = Settlement();
            state.EnsureWarehousePool("wh-0");
            state.ConfigureWarehousePolicy(
                "wh-0", capacity: 10, acceptedResourceIds: null);
            state.AddResource("wood", 8f, "wh-0");

            Assert.IsFalse(
                state.CanStoreResource("wood", 5f),
                "8 stored + 5 incoming exceeds capacity 10");
            Assert.IsTrue(state.CanStoreResource("wood", 2f));
            Assert.IsTrue(
                state.CanStoreResource("stone", 2f),
                "Different resource still fits within the same capacity");
            Assert.IsFalse(
                state.CanStoreResource("stone", 3f),
                "Capacity is shared across resource ids");
        }

        [Test]
        public void UnlimitedCapacity_AcceptsAnyAmount()
        {
            var state = Settlement();
            state.EnsureWarehousePool("wh-0");
            state.ConfigureWarehousePolicy(
                "wh-0", capacity: -1, acceptedResourceIds: null);

            Assert.IsTrue(state.CanStoreResource("wood", 100000f));
        }

        [Test]
        public void WhitelistedWarehouse_RejectsOtherResources()
        {
            var state = Settlement();
            state.EnsureWarehousePool("wh-0");
            state.ConfigureWarehousePolicy(
                "wh-0",
                capacity: -1,
                acceptedResourceIds: new[] { "food" });

            Assert.IsFalse(state.CanStoreResource("wood", 1f));
            Assert.IsTrue(state.CanStoreResource("food", 1f));
        }

        [Test]
        public void WarehouseRemoval_ConservesPhysicalStock()
        {
            var state = Settlement();
            state.EnsureWarehousePool("wh-0");
            state.EnsureWarehousePool("wh-1");
            state.AddResource("wood", 30f, "wh-0");

            state.RemoveWarehousePool("wh-0");
            state.EnsureWarehouseConsistency();

            Assert.AreEqual(
                30f, state.GetResource("wood"), Epsilon,
                "Demolishing a warehouse must not erase physical stock");
            Assert.AreEqual(
                30f,
                state.GetWarehouseSnapshot("wh-1").GetValueOrDefault("wood"),
                Epsilon,
                "Remaining warehouse absorbs the orphan stock");
        }

        [Test]
        public void OwnerPools_AreIsolated_BetweenOwners()
        {
            var pools = new EconomyOwnerResourcePoolService();
            pools.AddOwnerResource("owner-a", "wood", 10f, null);
            pools.AddOwnerResource("owner-b", "wood", 99f, null);

            var costs = new Dictionary<string, float> { ["wood"] = 10f };
            Assert.IsTrue(pools.TryConsumeOwnerPoolResources(
                "owner-a", costs, null, null, out _));

            Assert.IsEmpty(pools.GetOwnerPoolResourceTotals("owner-a"));
            Assert.AreEqual(
                99f,
                pools.GetOwnerPoolResourceTotals("owner-b")["wood"],
                Epsilon,
                "Consuming owner-a's pool must not touch owner-b");
        }

        [Test]
        public void OwnerPoolConsume_IsAllOrNothing()
        {
            var pools = new EconomyOwnerResourcePoolService();
            pools.AddOwnerResource("owner-1", "wood", 10f, null);
            pools.AddOwnerResource("owner-1", "stone", 5f, null);

            var costs = new Dictionary<string, float>
            {
                ["wood"] = 8f,
                ["stone"] = 50f // beyond stock
            };

            Assert.IsFalse(pools.TryConsumeOwnerPoolResources(
                "owner-1", costs, null, null, out var error));
            Assert.IsNotEmpty(error);
            Assert.AreEqual(
                10f, pools.GetOwnerPoolResourceTotals("owner-1")["wood"],
                Epsilon,
                "A failed bill must leave every resource untouched");
            Assert.AreEqual(
                5f, pools.GetOwnerPoolResourceTotals("owner-1")["stone"],
                Epsilon);
        }

        [Test]
        public void Transfer_OnlyTouches_OwningSettlement()
        {
            var pools = new EconomyOwnerResourcePoolService();
            pools.AddOwnerResource("owner-1", "wood", 20f, null);

            var mine = Settlement("s1", "owner-1");
            mine.EnsureWarehousePool("wh-a");
            var foreign = Settlement("s2", "owner-2");
            foreign.EnsureWarehousePool("wh-b");
            var settlements = new Dictionary<string, EconomySettlementState>
            {
                ["s1"] = mine,
                ["s2"] = foreign
            };

            Assert.IsTrue(pools.TransferOwnerResourcesToFirstWarehouse(
                "owner-1", settlements, null, "test"));

            Assert.AreEqual(20f, mine.GetResource("wood"), Epsilon);
            Assert.AreEqual(
                0f, foreign.GetResource("wood"), Epsilon,
                "Owner-1 stock must never land in owner-2's warehouse");
        }

        [Test]
        public void ReserveRelease_KeepsStockConserved()
        {
            var state = Settlement();
            state.EnsureWarehousePool("wh-0");
            state.AddResource("wood", 20f, "wh-0");

            state.ReserveResourceAt("wh-0", "wood", 12f);
            Assert.AreEqual(8f, state.GetAvailableResource("wood"), Epsilon);
            Assert.IsFalse(
                state.ConsumeResource("wood", 10f),
                "Only 8 of 20 are spendable while 12 are reserved");

            state.ReleaseResourceAt("wh-0", "wood", 12f);
            Assert.AreEqual(20f, state.GetAvailableResource("wood"), Epsilon);
            Assert.IsTrue(state.ConsumeResource("wood", 10f));
            Assert.AreEqual(10f, state.GetResource("wood"), Epsilon);
        }
    }
}
