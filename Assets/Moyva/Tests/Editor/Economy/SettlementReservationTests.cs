using NUnit.Framework;
using Kruty1918.Moyva.Economy.Runtime;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// Settlement-local resource reservations: reserved stock stays in the
    /// pool but is not spendable by generic consumers (production, upkeep,
    /// caravan loading) until released.
    /// </summary>
    public class SettlementReservationTests
    {
        private static EconomySettlementState CreateState()
            => new EconomySettlementState
            {
                SettlementId = "settlement-a",
                SettlementName = "Alpha",
                OwnerId = "player_0",
            };

        [Test]
        public void Reserve_ReducesAvailable_ButKeepsPoolTotal()
        {
            var state = CreateState();
            state.EnsureWarehousePool("0:0");
            state.AddResource("wood", 100f, "0:0");

            state.ReserveResourceAt("0:0", "wood", 40f);

            Assert.AreEqual(100f, state.GetResource("wood"), 0.0001f);
            Assert.AreEqual(60f, state.GetAvailableResource("wood"), 0.0001f);
            Assert.AreEqual(40f, state.GetTotalReservedResource("wood"), 0.0001f);
        }

        [Test]
        public void ConsumeResource_CannotSpendReservedStock()
        {
            var state = CreateState();
            state.EnsureWarehousePool("0:0");
            state.AddResource("wood", 100f, "0:0");
            state.ReserveResourceAt("0:0", "wood", 40f);

            Assert.IsFalse(state.ConsumeResource("wood", 80f),
                "Consuming past the unreserved stock must fail.");
            Assert.AreEqual(100f, state.GetResource("wood"), 0.0001f);
        }

        [Test]
        public void ConsumeResource_SpendsOnlyUnreservedStock()
        {
            var state = CreateState();
            state.EnsureWarehousePool("0:0");
            state.AddResource("wood", 100f, "0:0");
            state.ReserveResourceAt("0:0", "wood", 40f);

            Assert.IsTrue(state.ConsumeResource("wood", 60f));
            Assert.AreEqual(40f, state.GetResource("wood"), 0.0001f);
            Assert.AreEqual(40f,
                state.WarehouseResourcePools["0:0"]["wood"], 0.0001f,
                "Warehouse keeps the reserved amount after consumption.");
            Assert.AreEqual(40f, state.GetTotalReservedResource("wood"), 0.0001f);
        }

        [Test]
        public void ReleaseResourceAt_RestoresAvailability()
        {
            var state = CreateState();
            state.EnsureWarehousePool("0:0");
            state.AddResource("wood", 100f, "0:0");
            state.ReserveResourceAt("0:0", "wood", 40f);

            state.ReleaseResourceAt("0:0", "wood", 40f);

            Assert.AreEqual(100f, state.GetAvailableResource("wood"), 0.0001f);
            Assert.AreEqual(0f, state.GetTotalReservedResource("wood"), 0.0001f);
            Assert.IsTrue(state.ConsumeResource("wood", 100f));
        }

        [Test]
        public void ReleaseResourceAt_PartialRelease_KeepsRemainderReserved()
        {
            var state = CreateState();
            state.EnsureWarehousePool("0:0");
            state.AddResource("wood", 100f, "0:0");
            state.ReserveResourceAt("0:0", "wood", 40f);

            state.ReleaseResourceAt("0:0", "wood", 10f);

            Assert.AreEqual(70f, state.GetAvailableResource("wood"), 0.0001f);
        }

        [Test]
        public void Reservations_AreTrackedPerWarehouse()
        {
            var state = CreateState();
            state.EnsureWarehousePool("0:0");
            state.EnsureWarehousePool("4:4");
            state.AddResource("wood", 50f, "0:0");
            state.AddResource("wood", 50f, "4:4");
            state.ReserveResourceAt("0:0", "wood", 50f);

            // All of warehouse 0:0 is reserved; only warehouse 4:4 is free.
            Assert.IsFalse(state.ConsumeResource("wood", 60f));
            Assert.IsTrue(state.ConsumeResource("wood", 50f));
            Assert.AreEqual(50f,
                state.WarehouseResourcePools["0:0"]["wood"], 0.0001f);
            Assert.IsFalse(state.WarehouseResourcePools["4:4"].ContainsKey("wood"));
        }

        [Test]
        public void RemoveWarehousePool_DropsItsReservations()
        {
            var state = CreateState();
            state.EnsureWarehousePool("0:0");
            state.AddResource("wood", 100f, "0:0");
            state.ReserveResourceAt("0:0", "wood", 40f);

            state.RemoveWarehousePool("0:0");

            Assert.AreEqual(0f, state.GetTotalReservedResource("wood"), 0.0001f);
            Assert.AreEqual(0f, state.GetReservedResourceAt("0:0", "wood"), 0.0001f);
        }

        [Test]
        public void GetReservedSnapshot_AggregatesAcrossWarehouses()
        {
            var state = CreateState();
            state.EnsureWarehousePool("0:0");
            state.EnsureWarehousePool("4:4");
            state.ReserveResourceAt("0:0", "wood", 10f);
            state.ReserveResourceAt("4:4", "wood", 15f);
            state.ReserveResourceAt("4:4", "gold", 5f);

            var snapshot = state.GetReservedSnapshot();

            Assert.AreEqual(25f, snapshot["wood"], 0.0001f);
            Assert.AreEqual(5f, snapshot["gold"], 0.0001f);
        }
    }
}
