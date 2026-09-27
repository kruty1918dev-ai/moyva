using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C06 — production atomicity: a completed recipe cycle consumes its
    /// inputs exactly once (aggregated per resource id, all-or-nothing,
    /// unreserved stock only) and one resident never staffs two workplaces.
    /// </summary>
    public class C06AcceptanceTests
    {
        private static EconomySettlementState NewSettlement()
        {
            return new EconomySettlementState
            {
                SettlementId = "s1",
                OwnerId = "owner-1"
            };
        }

        private static EconomyBuildingState AddBuilding(
            EconomySettlementState state,
            string key,
            ProductionRecipeDefinition recipe = null)
        {
            var building = new EconomyBuildingState
            {
                InstanceKey = key,
                BuildingId = key,
                IsActive = true
            };
            if (recipe != null)
                building.ProductionRecipes.Add(recipe);
            state.Buildings.Add(building);
            return building;
        }

        private static ProductionRecipeDefinition Recipe(
            string id,
            int turnsPerCycle,
            List<BuildingResourceAmount> inputs,
            List<BuildingResourceAmount> outputs,
            bool requiresWorkers = false,
            bool requiresStorageSpace = false,
            float successChance = 1f)
        {
            return new ProductionRecipeDefinition
            {
                RecipeId = id,
                TurnsPerCycle = turnsPerCycle,
                RequiresWorkers = requiresWorkers,
                RequiresStorageSpace = requiresStorageSpace,
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

        private static int Tick(EconomySettlementState state)
            => new EconomyProductionTickService()
                .Tick(state, null, null, 60f);

        [Test]
        public void DuplicateInputIds_AreSummed_NotDoubleCounted()
        {
            var state = NewSettlement();
            AddBuilding(state, "mill-0", Recipe(
                "r1", 1,
                Amounts(("wood", 5), ("wood", 5)),
                Amounts(("food", 4))));
            state.AddResource("wood", 7f);

            Tick(state);

            Assert.AreEqual(0f, state.GetResource("food"), 0.0001f,
                "5+5 wood required but only 7 present — cycle must not run");
            Assert.AreEqual(7f, state.GetResource("wood"), 0.0001f,
                "No inputs may be consumed for a blocked cycle");

            state.AddResource("wood", 3f);
            Assert.AreEqual(1, Tick(state));
            Assert.AreEqual(4f, state.GetResource("food"), 0.0001f);
            Assert.AreEqual(0f, state.GetResource("wood"), 0.0001f,
                "All 10 aggregated wood consumed exactly once");
        }

        [Test]
        public void ReservedStock_CannotPayRecipeInputs()
        {
            var state = NewSettlement();
            AddBuilding(state, "mill-0", Recipe(
                "r1", 1,
                Amounts(("wood", 10)),
                Amounts(("food", 4))));
            state.EnsureWarehousePool("wh-0");
            state.AddResource("wood", 10f, "wh-0");
            state.ReserveResourceAt("wh-0", "wood", 6f);

            Tick(state);

            Assert.AreEqual(0f, state.GetResource("food"), 0.0001f);
            Assert.AreEqual(10f, state.GetResource("wood"), 0.0001f,
                "Reserved wood is not spendable — nothing may be consumed");
        }

        [Test]
        public void CompletedCycle_ConsumesInputs_ThenAddsOutputs()
        {
            var state = NewSettlement();
            AddBuilding(state, "mill-0", Recipe(
                "r1", 1,
                Amounts(("wood", 3), ("stone", 2)),
                Amounts(("food", 5), ("tools", 1))));
            state.AddResource("wood", 10f);
            state.AddResource("stone", 10f);

            Assert.AreEqual(1, Tick(state));
            Assert.AreEqual(7f, state.GetResource("wood"), 0.0001f);
            Assert.AreEqual(8f, state.GetResource("stone"), 0.0001f);
            Assert.AreEqual(5f, state.GetResource("food"), 0.0001f);
            Assert.AreEqual(1f, state.GetResource("tools"), 0.0001f);
        }

        [Test]
        public void MissingInput_HoldsReadyCycle_ThenCompletesExactlyOnce()
        {
            var state = NewSettlement();
            var building = AddBuilding(state, "mill-0", Recipe(
                "r1", 1,
                Amounts(("wood", 5)),
                Amounts(("food", 4))));

            for (int i = 0; i < 3; i++)
                Tick(state);

            Assert.AreEqual(0f, state.GetResource("food"), 0.0001f);
            Assert.AreEqual(
                1f,
                building.RecipeProgress["r1"],
                0.0001f,
                "Ready-cycle progress pinned at the cap, not accumulated");

            state.AddResource("wood", 5f);
            Assert.AreEqual(1, Tick(state));
            Assert.AreEqual(4f, state.GetResource("food"), 0.0001f);
            Assert.AreEqual(0f, state.GetResource("wood"), 0.0001f);
        }

        [Test]
        public void ZeroSuccessChance_CycleCounts_ButInputsStay()
        {
            var state = NewSettlement();
            AddBuilding(state, "lottery-0", Recipe(
                "r1", 1,
                Amounts(("wood", 5)),
                Amounts(("food", 100)),
                successChance: 0f));
            state.AddResource("wood", 5f);

            Assert.AreEqual(1, Tick(state));
            Assert.AreEqual(0f, state.GetResource("food"), 0.0001f);
            Assert.AreEqual(5f, state.GetResource("wood"), 0.0001f,
                "A failed cycle must not consume inputs");
        }

        [Test]
        public void WorkerRecipe_WithoutStaff_NeverRunsAsFreeRecipe()
        {
            var state = NewSettlement();
            AddBuilding(state, "shop-0", Recipe(
                "r1", 1,
                null,
                Amounts(("food", 1)),
                requiresWorkers: true));
            // RequiredWorkers stays 0: an invalid worker recipe.

            Tick(state);
            Assert.AreEqual(0f, state.GetResource("food"), 0.0001f);
        }

        [Test]
        public void OneResident_CannotFillTwoWorkplaces()
        {
            var state = NewSettlement();
            var a = AddBuilding(state, "shop-0");
            var b = AddBuilding(state, "shop-1");
            a.RequiredWorkers = b.RequiredWorkers = 1;
            state.Residents.Add(new EconomyResidentState(
                age: 30, hp: 100f, comfort: 50f, houseCollapsed: false));

            var (available, assigned) =
                new EconomyWorkerAllocationService().Allocate(state, null);

            Assert.AreEqual(1, available);
            Assert.AreEqual(1, assigned);
            Assert.AreEqual(
                1,
                a.AssignedWorkers + b.AssignedWorkers,
                "One adult staffs exactly one workplace per turn");
        }

        [Test]
        public void EnlistedAndQueuedResidents_AreNotWorkforce()
        {
            var state = NewSettlement();
            var building = AddBuilding(state, "shop-0");
            building.RequiredWorkers = 1;
            state.Residents.Add(new EconomyResidentState(
                age: 30, hp: 100f, comfort: 50f, houseCollapsed: false,
                militaryUnitId: "u-1"));
            state.Residents.Add(new EconomyResidentState(
                age: 30, hp: 100f, comfort: 50f, houseCollapsed: false,
                recruitmentQueueId: 7));

            var (available, assigned) =
                new EconomyWorkerAllocationService().Allocate(state, null);

            Assert.AreEqual(0, available);
            Assert.AreEqual(0, assigned);
            Assert.AreEqual(0, building.AssignedWorkers);
        }

        [Test]
        public void ChildrenAndElders_AreNotWorkforce()
        {
            var state = NewSettlement();
            var building = AddBuilding(state, "shop-0");
            building.RequiredWorkers = 2;
            state.Residents.Add(new EconomyResidentState(
                age: 12, hp: 100f, comfort: 50f, houseCollapsed: false));
            state.Residents.Add(new EconomyResidentState(
                age: 70, hp: 100f, comfort: 50f, houseCollapsed: false));

            var (available, _) =
                new EconomyWorkerAllocationService().Allocate(state, null);

            Assert.AreEqual(0, available);
            Assert.AreEqual(0, building.AssignedWorkers);
        }
    }
}
