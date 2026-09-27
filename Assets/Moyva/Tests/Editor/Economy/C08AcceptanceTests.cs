using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C08 — read-model purity: snapshots are a pure view that mirrors tick
    /// eligibility (unreserved, aggregated inputs) and never advance recipes,
    /// progress, RNG, or resource ledgers.
    /// </summary>
    public class C08AcceptanceTests
    {
        private const float Epsilon = 0.0001f;

        private static int _settlementCounter;

        private static EconomySettlementState Settlement(
            string owner = "owner-1", bool active = true)
        {
            return new EconomySettlementState
            {
                SettlementId = $"s-{owner}-{++_settlementCounter}",
                OwnerId = owner,
                IsActive = active
            };
        }

        private static ProductionRecipeDefinition Recipe(
            string id,
            int turns,
            List<BuildingResourceAmount> inputs,
            List<BuildingResourceAmount> outputs,
            float successChance = 1f)
        {
            return new ProductionRecipeDefinition
            {
                RecipeId = id,
                TurnsPerCycle = turns,
                RequiresWorkers = false,
                RequiresStorageSpace = false,
                SuccessChance = successChance,
                Inputs = inputs ?? new List<BuildingResourceAmount>(),
                Outputs = outputs ?? new List<BuildingResourceAmount>()
            };
        }

        private static List<BuildingResourceAmount> Amounts(
            params (string id, int amount)[] pairs)
        {
            var list = new List<BuildingResourceAmount>();
            foreach (var p in pairs)
                list.Add(new BuildingResourceAmount
                    { ResourceId = p.id, Amount = p.amount });
            return list;
        }

        private static EconomyBuildingState Building(
            string key, params ProductionRecipeDefinition[] recipes)
        {
            var b = new EconomyBuildingState
            {
                InstanceKey = key,
                BuildingId = key,
                IsActive = true
            };
            foreach (var r in recipes)
                b.ProductionRecipes.Add(r);
            return b;
        }

        private static Dictionary<string, EconomySettlementState> Wrap(
            params EconomySettlementState[] settlements)
        {
            var map = new Dictionary<string, EconomySettlementState>();
            foreach (var s in settlements)
                map[s.SettlementId] = s;
            return map;
        }

        [Test]
        public void Snapshot_ReportsExpectedRate_PerTurn()
        {
            var state = Settlement();
            state.Buildings.Add(Building("farm-0", Recipe(
                "r1", 2, null, Amounts(("food", 6)), successChance: 0.5f)));

            var snapshot = EconomyProductionReadModel.Capture(
                Wrap(state), "owner-1");

            Assert.AreEqual(
                1.5f, snapshot.ProductionPerTurn["food"], Epsilon,
                "6 per 2 turns at 50% = 1.5 food/turn expected rate");
            Assert.AreEqual(1, snapshot.ActiveProducerBuildingsByType["farm-0"]);
        }

        [Test]
        public void Snapshot_MirrorsTick_UnreservedInputGate()
        {
            var state = Settlement();
            state.Buildings.Add(Building("mill-0", Recipe(
                "r1", 1, Amounts(("wood", 10)), Amounts(("food", 4)))));
            state.EnsureWarehousePool("wh-0");
            state.AddResource("wood", 10f, "wh-0");
            state.ReserveResourceAt("wh-0", "wood", 10f);

            var snapshot = EconomyProductionReadModel.Capture(
                Wrap(state), "owner-1");

            Assert.IsFalse(
                snapshot.ProductionPerTurn.ContainsKey("food"),
                "Fully reserved inputs cannot produce — the tick would not run either");
        }

        [Test]
        public void Snapshot_AggregatesDuplicateInputs_LikeTick()
        {
            var state = Settlement();
            state.Buildings.Add(Building("mill-0", Recipe(
                "r1", 1,
                Amounts(("wood", 5), ("wood", 5)),
                Amounts(("food", 4)))));
            state.AddResource("wood", 7f);

            var snapshot = EconomyProductionReadModel.Capture(
                Wrap(state), "owner-1");

            Assert.IsFalse(snapshot.ProductionPerTurn.ContainsKey("food"));
        }

        [Test]
        public void Snapshot_IsPure_NoProgress_NoLedgerChange()
        {
            var state = Settlement();
            var building = Building("mill-0", Recipe(
                "r1", 3, Amounts(("wood", 2)), Amounts(("food", 4))));
            state.Buildings.Add(building);
            state.AddResource("wood", 10f);
            var map = Wrap(state);

            var first = EconomyProductionReadModel.Capture(map, "owner-1");
            var second = EconomyProductionReadModel.Capture(map, "owner-1");

            Assert.AreEqual(
                first.ProductionPerTurn["food"],
                second.ProductionPerTurn["food"],
                Epsilon);
            Assert.IsFalse(
                building.RecipeProgress.ContainsKey("r1"),
                "Forecast must not advance recipe progress");
            Assert.AreEqual(10f, state.GetResource("wood"), Epsilon);
            Assert.AreEqual(0f, state.GetResource("food"), Epsilon,
                "Forecast must not materialize outputs");
        }

        [Test]
        public void Snapshot_OnlyIncludes_OwnerAndActive()
        {
            var mine = Settlement("owner-1");
            mine.Buildings.Add(Building("farm-0", Recipe(
                "r1", 1, null, Amounts(("food", 5)))));
            var foreign = Settlement("owner-2");
            foreign.Buildings.Add(Building("farm-x", Recipe(
                "r1", 1, null, Amounts(("food", 9)))));
            var dead = Settlement("owner-1", active: false);
            dead.Buildings.Add(Building("farm-z", Recipe(
                "r1", 1, null, Amounts(("food", 7)))));
            var idle = Settlement("owner-1");
            idle.Buildings.Add(new EconomyBuildingState
            {
                InstanceKey = "dead-shop",
                BuildingId = "dead-shop",
                IsActive = false,
                ProductionRecipes = { Recipe("r1", 1, null, Amounts(("food", 3))) }
            });

            var snapshot = EconomyProductionReadModel.Capture(
                Wrap(mine, foreign, dead, idle), "owner-1");

            Assert.AreEqual(
                5f, snapshot.ProductionPerTurn["food"], Epsilon,
                "Only the active owner-1 producer counts");
            Assert.IsFalse(
                snapshot.ActiveProducerBuildingsByType.ContainsKey("farm-x"));
            Assert.IsFalse(
                snapshot.ActiveProducerBuildingsByType.ContainsKey("farm-z"));
            Assert.IsFalse(
                snapshot.ActiveProducerBuildingsByType.ContainsKey("dead-shop"));
        }

        [Test]
        public void Snapshots_DontLeak_LiveReferences()
        {
            var state = Settlement();
            state.Buildings.Add(Building("farm-0", Recipe(
                "r1", 1, null, Amounts(("food", 5)))));

            var snapshot = EconomyProductionReadModel.Capture(
                Wrap(state), "owner-1");
            state.AddResource("food", 99f);
            state.Buildings.Clear();

            Assert.AreEqual(
                5f, snapshot.ProductionPerTurn["food"], Epsilon,
                "The snapshot is a frozen copy, not a live view");
        }
    }
}
