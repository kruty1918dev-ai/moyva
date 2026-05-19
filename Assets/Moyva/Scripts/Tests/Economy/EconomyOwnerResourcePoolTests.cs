using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    [TestFixture]
    public sealed class EconomyOwnerResourcePoolTests
    {
        [Test]
        public void StarterPackWithoutSettlement_IsVisibleInOwnerTotals()
        {
            var manager = CreateManager();

            GrantStarterPack(manager, " player_0 ", string.Empty);

            var totals = manager.GetOwnerResourceTotals("player_0");
            Assert.AreEqual(50f, totals["steak-food-resources"], 0.01f);
            Assert.AreEqual(30f, totals["copper-ore-materials-resources"], 0.01f);
        }

        [Test]
        public void RuntimeApiCategoryTotals_IncludeOwnerPoolBeforeSettlementExists()
        {
            var manager = CreateManager();
            var api = new EconomyRuntimeApi(manager, CreateDatabase(), null);

            GrantStarterPack(manager, "player_0", string.Empty);

            var totals = api.GetOwnerCategoryTotals("player_0");
            Assert.AreEqual(50f, totals.FoodTotal, 0.01f);
            Assert.AreEqual(30f, totals.MaterialsTotal, 0.01f);
        }

        [Test]
        public void OwnerPoolTransfersIntoFirstSettlementWithoutDoubleCounting()
        {
            var manager = CreateManager();
            GrantStarterPack(manager, "player_0", string.Empty);

            var settlement = new EconomySettlementState
            {
                SettlementId = "settlement-1",
                OwnerId = "player_0",
                IsActive = true,
            };

            var settlements = GetPrivateField<Dictionary<string, EconomySettlementState>>(manager, "_settlements");
            settlements[settlement.SettlementId] = settlement;

            // Use the public restore pipeline, which transfers owner pools into existing settlements.
            var snapshotBeforeTransfer = manager.GetOwnerResourcePoolsSnapshot();
            manager.RestoreOwnerResourcePools(snapshotBeforeTransfer);

            Assert.AreEqual(50f, settlement.GetResource("steak-food-resources"), 0.01f);
            Assert.AreEqual(30f, settlement.GetResource("copper-ore-materials-resources"), 0.01f);

            var totals = manager.GetOwnerResourceTotals("player_0");
            Assert.AreEqual(50f, totals["steak-food-resources"], 0.01f);
            Assert.AreEqual(30f, totals["copper-ore-materials-resources"], 0.01f);
        }

        [Test]
        public void OwnerTotalsSnapshot_IncludesSettlementResourcesAfterTransfer()
        {
            var manager = CreateManager();
            var settlement = AddSettlement(manager, "settlement-1", "player_0");
            settlement.AddResource("steak-food-resources", 50f);
            settlement.AddResource("copper-ore-materials-resources", 30f);

            var snapshot = manager.GetOwnerResourceTotalsSnapshot();

            Assert.AreEqual(50f, snapshot["player_0"]["steak-food-resources"], 0.01f);
            Assert.AreEqual(30f, snapshot["player_0"]["copper-ore-materials-resources"], 0.01f);
        }

        [Test]
        public void RestoreOwnerResourcePools_TransfersIntoExistingSettlementWithoutDoubleCounting()
        {
            var manager = CreateManager();
            var settlement = AddSettlement(manager, "settlement-1", "player_0");

            manager.RestoreOwnerResourcePools(new Dictionary<string, Dictionary<string, float>>
            {
                ["player_0"] = new Dictionary<string, float>
                {
                    ["steak-food-resources"] = 50f,
                    ["copper-ore-materials-resources"] = 30f,
                },
            });

            Assert.AreEqual(50f, settlement.GetResource("steak-food-resources"), 0.01f);
            Assert.AreEqual(30f, settlement.GetResource("copper-ore-materials-resources"), 0.01f);

            var totals = manager.GetOwnerResourceTotals("player_0");
            Assert.AreEqual(50f, totals["steak-food-resources"], 0.01f);
            Assert.AreEqual(30f, totals["copper-ore-materials-resources"], 0.01f);
        }

        private static EconomyManager CreateManager()
        {
            return new EconomyManager(null, null, null, new FakeBuildingRegistry());
        }

        private static EconomySettlementState AddSettlement(EconomyManager manager, string settlementId, string ownerId)
        {
            var settlement = new EconomySettlementState
            {
                SettlementId = settlementId,
                OwnerId = ownerId,
                IsActive = true,
            };

            var settlements = GetPrivateField<Dictionary<string, EconomySettlementState>>(manager, "_settlements");
            settlements[settlement.SettlementId] = settlement;
            return settlement;
        }

        private static void GrantStarterPack(EconomyManager manager, string ownerId, string settlementId)
        {
            var signal = new GrantStarterPackResourcesSignal
            {
                SettlementId = settlementId,
                OwnerId = ownerId,
                Entries = new[]
                {
                    new StarterPackResourceEntrySignal { ResourceId = "steak-food-resources", Amount = 50f },
                    new StarterPackResourceEntrySignal { ResourceId = "copper-ore-materials-resources", Amount = 30f },
                },
            };

            InvokePrivate(manager, "OnGrantStarterPackResources", signal);
        }

        private static EconomyDatabaseSO CreateDatabase()
        {
            var database = ScriptableObject.CreateInstance<EconomyDatabaseSO>();
            SetPrivateField(database, "_resources", new List<EconomyResourceDefinition>
            {
                CreateResource("steak-food-resources", EconomyResourceCategory.Food),
                CreateResource("copper-ore-materials-resources", EconomyResourceCategory.Materials),
            });
            return database;
        }

        private static EconomyResourceDefinition CreateResource(string resourceId, EconomyResourceCategory category)
        {
            var resource = ScriptableObject.CreateInstance<EconomyResourceDefinition>();
            SetPrivateField(resource, "_id", resourceId);
            SetPrivateField(resource, "_category", category);
            return resource;
        }

        private static void InvokePrivate(object target, string methodName, params object[] arguments)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method, $"Missing private method '{methodName}'.");
            method.Invoke(target, arguments);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Missing private field '{fieldName}'.");
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Missing private field '{fieldName}'.");
            field.SetValue(target, value);
        }

        private sealed class FakeBuildingRegistry : IBuildingRegistry
        {
            public BuildingDefinition[] GetAll() => System.Array.Empty<BuildingDefinition>();
            public BuildingDefinition GetById(string id) => null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category) => System.Array.Empty<BuildingDefinition>();
            public WallCollectionDefinition[] GetWallCollections() => System.Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId) => null;
        }
    }
}