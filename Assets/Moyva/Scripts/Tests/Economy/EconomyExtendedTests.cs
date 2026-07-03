using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Economy
{
    // ====================================================================
    // EconomyNeedSnapshotTests — 5 tests
    // ====================================================================
    [TestFixture]
    public sealed class EconomyNeedSnapshotTests
    {
        [Test]
        public void Constructor_SetsAllFields()
        {
            var snap = new EconomyNeedSnapshot(1f, 2f, 3f, 4f);
            Assert.AreEqual(1f, snap.FoodSeverity);
            Assert.AreEqual(2f, snap.ColdSeverity);
            Assert.AreEqual(3f, snap.DiseaseSeverity);
            Assert.AreEqual(4f, snap.WarSeverity);
        }

        [Test]
        public void Default_AllZeros()
        {
            var snap = default(EconomyNeedSnapshot);
            Assert.AreEqual(0f, snap.FoodSeverity);
            Assert.AreEqual(0f, snap.ColdSeverity);
        }

        [Test]
        public void NegativeValues_Stored()
        {
            var snap = new EconomyNeedSnapshot(-1f, -2f, 0f, 0f);
            Assert.AreEqual(-1f, snap.FoodSeverity);
            Assert.AreEqual(-2f, snap.ColdSeverity);
        }

        [Test]
        public void LargeValues_Stored()
        {
            var snap = new EconomyNeedSnapshot(float.MaxValue, 0f, 0f, 0f);
            Assert.AreEqual(float.MaxValue, snap.FoodSeverity);
        }

        [Test]
        public void Equality_SameValues()
        {
            var a = new EconomyNeedSnapshot(1f, 2f, 3f, 4f);
            var b = new EconomyNeedSnapshot(1f, 2f, 3f, 4f);
            Assert.AreEqual(a, b);
        }
    }

    // ====================================================================
    // EconomyResidentStateTests — 5 tests
    // ====================================================================
    [TestFixture]
    public sealed class EconomyResidentStateTests
    {
        [Test]
        public void Constructor_SetsAllFields()
        {
            var r = new EconomyResidentState(25, 100f, 0.9f, false);
            Assert.AreEqual(25, r.Age);
            Assert.AreEqual(100f, r.Hp);
            Assert.AreEqual(0.9f, r.Comfort, 0.001f);
            Assert.IsFalse(r.HouseCollapsed);
        }

        [Test]
        public void HouseCollapsed_True()
        {
            var r = new EconomyResidentState(30, 80f, 0.5f, true);
            Assert.IsTrue(r.HouseCollapsed);
        }

        [Test]
        public void Default_AllZeros()
        {
            var r = default(EconomyResidentState);
            Assert.AreEqual(0, r.Age);
            Assert.AreEqual(0f, r.Hp);
        }

        [Test]
        public void ZeroAge_IsValid()
        {
            var r = new EconomyResidentState(0, 100f, 1f, false);
            Assert.AreEqual(0, r.Age);
        }

        [Test]
        public void LargeAge_Stored()
        {
            var r = new EconomyResidentState(999, 100f, 1f, false);
            Assert.AreEqual(999, r.Age);
        }
    }

    // ====================================================================
    // EconomyTickResultTests — 6 tests
    // ====================================================================
    [TestFixture]
    public sealed class EconomyTickResultTests
    {
        [Test]
        public void Default_AllZeros()
        {
            var tr = new EconomyTickResult();
            Assert.AreEqual(0, tr.Turn);
            Assert.AreEqual(0, tr.Arrivals);
            Assert.AreEqual(0, tr.Deaths);
            Assert.AreEqual(0, tr.TotalPopulation);
        }

        [Test]
        public void SetFields_ReadsBack()
        {
            var tr = new EconomyTickResult
            {
                Turn = 5,
                Arrivals = 3,
                Deaths = 1,
                TotalPopulation = 20,
                AvailableWorkers = 12,
                AssignedWorkers = 8,
                ProductionCyclesCompleted = 4,
                TotalFoodConsumed = 15.5f,
                TotalWaterConsumed = 10.2f
            };
            Assert.AreEqual(5, tr.Turn);
            Assert.AreEqual(3, tr.Arrivals);
            Assert.AreEqual(1, tr.Deaths);
            Assert.AreEqual(20, tr.TotalPopulation);
            Assert.AreEqual(12, tr.AvailableWorkers);
            Assert.AreEqual(8, tr.AssignedWorkers);
            Assert.AreEqual(4, tr.ProductionCyclesCompleted);
            Assert.AreEqual(15.5f, tr.TotalFoodConsumed, 0.01f);
            Assert.AreEqual(10.2f, tr.TotalWaterConsumed, 0.01f);
        }

        [Test]
        public void NegativeDeaths_Stored()
        {
            var tr = new EconomyTickResult { Deaths = -1 };
            Assert.AreEqual(-1, tr.Deaths);
        }

        [Test]
        public void LargePopulation_Stored()
        {
            var tr = new EconomyTickResult { TotalPopulation = 1000000 };
            Assert.AreEqual(1000000, tr.TotalPopulation);
        }

        [Test]
        public void ZeroConsumption_Valid()
        {
            var tr = new EconomyTickResult { TotalFoodConsumed = 0f, TotalWaterConsumed = 0f };
            Assert.AreEqual(0f, tr.TotalFoodConsumed);
        }

        [Test]
        public void AvailableWorkers_GreaterThanAssigned()
        {
            var tr = new EconomyTickResult { AvailableWorkers = 10, AssignedWorkers = 5 };
            Assert.Greater(tr.AvailableWorkers, tr.AssignedWorkers);
        }
    }

    // ====================================================================
    // EconomySettlementStateTests — 18 tests
    // ====================================================================
    [TestFixture]
    public sealed class EconomySettlementStateTests
    {
        [Test]
        public void Default_IsActive()
        {
            var s = new EconomySettlementState();
            Assert.IsTrue(s.IsActive);
        }

        [Test]
        public void Default_EmptyResources()
        {
            var s = new EconomySettlementState();
            Assert.AreEqual(0, s.ResourcePool.Count);
        }

        [Test]
        public void Default_EmptyResidents()
        {
            var s = new EconomySettlementState();
            Assert.AreEqual(0, s.Residents.Count);
        }

        [Test]
        public void GetResource_Unknown_ReturnsZero()
        {
            var s = new EconomySettlementState();
            Assert.AreEqual(0f, s.GetResource("gold"));
        }

        [Test]
        public void AddResource_IncrementsPool()
        {
            var s = new EconomySettlementState();
            s.AddResource("food", 100f);
            Assert.AreEqual(100f, s.GetResource("food"), 0.01f);
        }

        [Test]
        public void AddResource_TwiceAccumulates()
        {
            var s = new EconomySettlementState();
            s.AddResource("food", 50f);
            s.AddResource("food", 30f);
            Assert.AreEqual(80f, s.GetResource("food"), 0.01f);
        }

        [Test]
        public void ConsumeResource_Sufficient_ReturnsTrue()
        {
            var s = new EconomySettlementState();
            s.AddResource("wood", 100f);
            Assert.IsTrue(s.ConsumeResource("wood", 50f));
            Assert.AreEqual(50f, s.GetResource("wood"), 0.01f);
        }

        [Test]
        public void ConsumeResource_Insufficient_ReturnsFalse()
        {
            var s = new EconomySettlementState();
            s.AddResource("wood", 10f);
            Assert.IsFalse(s.ConsumeResource("wood", 20f));
            Assert.AreEqual(10f, s.GetResource("wood"), 0.01f);
        }

        [Test]
        public void ConsumeResource_UnknownResource_ReturnsFalse()
        {
            var s = new EconomySettlementState();
            Assert.IsFalse(s.ConsumeResource("gold", 1f));
        }

        [Test]
        public void ConsumeResource_ExactAmount_ReturnsTrue()
        {
            var s = new EconomySettlementState();
            s.AddResource("iron", 50f);
            Assert.IsTrue(s.ConsumeResource("iron", 50f));
            Assert.AreEqual(0f, s.GetResource("iron"), 0.01f);
        }

        [Test]
        public void EnsureWarehousePool_CreatesIfNotExists()
        {
            var s = new EconomySettlementState();
            s.EnsureWarehousePool("wh1");
            Assert.IsTrue(s.WarehouseResourcePools.ContainsKey("wh1"));
        }

        [Test]
        public void EnsureWarehousePool_NullKey_NothingCreated()
        {
            var s = new EconomySettlementState();
            s.EnsureWarehousePool(null);
            Assert.AreEqual(0, s.WarehouseResourcePools.Count);
        }

        [Test]
        public void RemoveWarehousePool_RemovesEntry()
        {
            var s = new EconomySettlementState();
            s.EnsureWarehousePool("wh1");
            s.RemoveWarehousePool("wh1");
            Assert.IsFalse(s.WarehouseResourcePools.ContainsKey("wh1"));
        }

        [Test]
        public void RemoveWarehousePool_NullKey_DoesNotThrow()
        {
            var s = new EconomySettlementState();
            Assert.DoesNotThrow(() => s.RemoveWarehousePool(null));
        }

        [Test]
        public void GetWarehouseSnapshot_ReturnsNewDictionary()
        {
            var s = new EconomySettlementState();
            s.EnsureWarehousePool("wh1");
            var snap = s.GetWarehouseSnapshot("wh1");
            Assert.IsNotNull(snap);
            Assert.AreEqual(0, snap.Count);
        }

        [Test]
        public void GetWarehouseSnapshot_UnknownKey_ReturnsEmpty()
        {
            var s = new EconomySettlementState();
            var snap = s.GetWarehouseSnapshot("unknown");
            Assert.IsNotNull(snap);
            Assert.AreEqual(0, snap.Count);
        }

        [Test]
        public void GetAllWarehousesTotalSnapshot_AggregatesAcrossWarehouses()
        {
            var s = new EconomySettlementState();
            s.EnsureWarehousePool("wh1");
            s.EnsureWarehousePool("wh2");
            s.WarehouseResourcePools["wh1"]["food"] = 10f;
            s.WarehouseResourcePools["wh2"]["food"] = 20f;
            s.ResourcePool["food"] = 30f;
            var snap = s.GetAllWarehousesTotalSnapshot();
            Assert.AreEqual(30f, snap["food"], 0.01f);
        }

        [Test]
        public void WorkerAssignments_AddAndRead()
        {
            var s = new EconomySettlementState();
            s.WorkerAssignments["farm"] = 5;
            Assert.AreEqual(5, s.WorkerAssignments["farm"]);
        }
    }

    // ====================================================================
    // EconomyBuildingStateTests — 6 tests
    // ====================================================================
    [TestFixture]
    public sealed class EconomyBuildingStateTests
    {
        [Test]
        public void Default_IsActive()
        {
            var b = new EconomyBuildingState();
            Assert.IsTrue(b.IsActive);
        }

        [Test]
        public void IsFullyStaffed_WhenAssignedEqualsRequired()
        {
            var b = new EconomyBuildingState { RequiredWorkers = 3, AssignedWorkers = 3 };
            Assert.IsTrue(b.IsFullyStaffed);
        }

        [Test]
        public void IsFullyStaffed_WhenAssignedGreater()
        {
            var b = new EconomyBuildingState { RequiredWorkers = 2, AssignedWorkers = 5 };
            Assert.IsTrue(b.IsFullyStaffed);
        }

        [Test]
        public void IsFullyStaffed_WhenUnderstaffed_ReturnsFalse()
        {
            var b = new EconomyBuildingState { RequiredWorkers = 5, AssignedWorkers = 2 };
            Assert.IsFalse(b.IsFullyStaffed);
        }

        [Test]
        public void ProductionProgress_Default_IsZero()
        {
            var b = new EconomyBuildingState();
            Assert.AreEqual(0f, b.ProductionProgress);
        }

        [Test]
        public void EconomyPriority_CanBeSet()
        {
            var b = new EconomyBuildingState { EconomyPriority = 10 };
            Assert.AreEqual(10, b.EconomyPriority);
        }
    }

    // ====================================================================
    // EconomySchemaTests — 2 tests
    // ====================================================================
    [TestFixture]
    public sealed class EconomySchemaTests
    {
        [Test]
        public void InitialVersion_Is1()
        {
            Assert.AreEqual(1, EconomySchema.InitialVersion);
        }

        [Test]
        public void CurrentVersion_GreaterOrEqual_Initial()
        {
            Assert.GreaterOrEqual(EconomySchema.CurrentVersion, EconomySchema.InitialVersion);
        }
    }

    // ====================================================================
    // EconomyEnumTests — 8 tests
    // ====================================================================
    [TestFixture]
    public sealed class EconomyEnumTests
    {
        [Test]
        public void ResourceCategory_None_IsZero()
            => Assert.AreEqual(0, (int)EconomyResourceCategory.None);

        [Test]
        public void ResourceCategory_Food_IsOne()
            => Assert.AreEqual(1, (int)EconomyResourceCategory.Food);

        [Test]
        public void ResourceCategory_Materials_IsTwo()
            => Assert.AreEqual(2, (int)EconomyResourceCategory.Materials);

        [Test]
        public void ResourceCategory_Money_IsThree()
            => Assert.AreEqual(3, (int)EconomyResourceCategory.Money);

        [Test]
        public void SettlementType_Village_IsZero()
            => Assert.AreEqual(0, (int)EconomySettlementType.Village);

        [Test]
        public void SettlementType_Castle_IsOne()
            => Assert.AreEqual(1, (int)EconomySettlementType.Castle);

        [Test]
        public void WarehouseType_Food_IsZero()
            => Assert.AreEqual(0, (int)EconomyWarehouseType.FoodWarehouse);

        [Test]
        public void WarehouseType_Materials_IsOne()
            => Assert.AreEqual(1, (int)EconomyWarehouseType.MaterialsWarehouse);

        [Test]
        public void AllEnums_HaveUniqueCounts()
        {
            Assert.AreEqual(4, System.Enum.GetValues(typeof(EconomyResourceCategory)).Length);
            Assert.AreEqual(2, System.Enum.GetValues(typeof(EconomySettlementType)).Length);
            Assert.AreEqual(2, System.Enum.GetValues(typeof(EconomyWarehouseType)).Length);
        }
    }
}
