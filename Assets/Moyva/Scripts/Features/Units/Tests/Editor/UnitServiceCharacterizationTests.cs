using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;
using Kruty1918.EntityHealth;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// Characterization tests locking the externally observable behaviour of
    /// <see cref="UnitService"/> before its parallel dictionaries are migrated
    /// to a single UnitState aggregate. Lifecycle is driven through the real
    /// SignalBus, exactly like production wiring.
    /// </summary>
    [TestFixture]
    public sealed class UnitServiceCharacterizationTests
    {
        private SignalBus _bus;
        private FakeGrid _grid;
        private FakeTileSettings _tileSettings;
        private FakeUnitClassConfig _configs;
        private FakeObjectsMap _objectsMap;
        private FakeHealthRegistry _health;
        private UnitService _service;
        private readonly List<UnitService> _extraServices = new();
        private readonly List<UnitGarrisonStateChangedSignal> _garrisonSignals = new();
        private readonly List<InterruptMovementSignal> _interrupts = new();
        private readonly List<GameObject> _objectsToCleanup = new();

        [SetUp]
        public void SetUp()
        {
            _bus = CreateSignalBus();
            _garrisonSignals.Clear();
            _interrupts.Clear();
            _bus.Subscribe<UnitGarrisonStateChangedSignal>(
                signal => _garrisonSignals.Add(signal));
            _bus.Subscribe<InterruptMovementSignal>(
                signal => _interrupts.Add(signal));

            _grid = new FakeGrid(32, 32);
            _tileSettings = new FakeTileSettings();
            _configs = new FakeUnitClassConfig();
            _objectsMap = new FakeObjectsMap();
            _health = new FakeHealthRegistry();
            _configs.Set("warrior", new UnitClassConfig
            {
                TypeId = "warrior",
                MovementPointsPerTurn = 5f,
                HitPoints = 30,
            });
            _configs.Set("archer", new UnitClassConfig
            {
                TypeId = "archer",
                MovementPointsPerTurn = 4f,
                HitPoints = 20,
            });

            _service = CreateService(_objectsMap);
            _service.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();
            foreach (UnitService service in _extraServices)
                service.Dispose();
            _extraServices.Clear();
            foreach (GameObject go in _objectsToCleanup)
            {
                if (go != null)
                    UnityEngine.Object.DestroyImmediate(go);
            }
            _objectsToCleanup.Clear();
        }

        // ── Create ───────────────────────────────────────────────────

        [Test]
        public void Create_RegistersAllAttributes()
        {
            FireCreated("u1", "warrior", new Vector2Int(3, 4), "p1", vision: 3);

            Assert.IsTrue(
                _service.TryGetUnitPosition("u1", out Vector2Int pos));
            Assert.AreEqual(new Vector2Int(3, 4), pos);
            Assert.AreEqual("warrior", _service.GetUnitTypeId("u1"));
            Assert.AreEqual("p1", _service.GetUnitOwnerId("u1"));
            Assert.AreEqual(5f, _service.GetStamina("u1"), 0.0001f);
            CollectionAssert.Contains(_service.GetAllUnitIds(), "u1");
        }

        [Test]
        public void Create_NullOwner_DefaultsToPlayer0()
        {
            FireCreated("u1", "warrior", Vector2Int.zero, ownerId: null);
            Assert.AreEqual("player_0", _service.GetUnitOwnerId("u1"));
        }

        [Test]
        public void Create_WhitespacePaddedOwner_IsTrimmed()
        {
            FireCreated("u1", "warrior", Vector2Int.zero, ownerId: "  p1  ");
            Assert.AreEqual("p1", _service.GetUnitOwnerId("u1"));
        }

        [Test]
        public void Create_UnknownType_UnitNotRegistered()
        {
            FireCreated("u1", "ghost-type", Vector2Int.zero, "p1");

            Assert.IsFalse(_service.TryGetUnitPosition("u1", out _));
            Assert.IsNull(_service.GetUnitTypeId("u1"));
            Assert.AreEqual(0f, _service.GetStamina("u1"));
            Assert.IsEmpty(_service.GetAllUnitIds());
        }

        [Test]
        public void Create_RegistersHealthWithConfigHitPoints()
        {
            FireCreated("u1", "warrior", Vector2Int.zero, "p1");

            Assert.IsTrue(_health.TryGet("u1", out IHealth health));
            Assert.AreEqual(30, health.MaxHp);
            Assert.AreEqual(30, health.CurrentHp);
        }

        [Test]
        public void Create_NonPositiveHitPoints_ClampsToOne()
        {
            _configs.Set("fragile", new UnitClassConfig
            {
                TypeId = "fragile",
                MovementPointsPerTurn = 1f,
                HitPoints = 0,
            });

            FireCreated("u1", "fragile", Vector2Int.zero, "p1");

            Assert.IsTrue(_health.TryGet("u1", out IHealth health));
            Assert.AreEqual(1, health.MaxHp);
        }

        [Test]
        public void Create_WithoutHealthRegistry_StillRegistersUnit()
        {
            var service = new UnitService(
                _bus, _grid, _tileSettings, _configs, _objectsMap);
            try
            {
                service.Initialize();
                FireCreated("u1", "warrior", Vector2Int.zero, "p1");
                Assert.IsTrue(service.TryGetUnitPosition("u1", out _));
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void Create_DuplicateId_ReplacesStoredState()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            FireCreated("u1", "archer", new Vector2Int(5, 5), "p2");

            Assert.IsTrue(
                _service.TryGetUnitPosition("u1", out Vector2Int pos));
            Assert.AreEqual(new Vector2Int(5, 5), pos);
            Assert.AreEqual("archer", _service.GetUnitTypeId("u1"));
            Assert.AreEqual("p2", _service.GetUnitOwnerId("u1"));
            Assert.AreEqual(4f, _service.GetStamina("u1"), 0.0001f);
        }

        // ── Move ─────────────────────────────────────────────────────

        [Test]
        public void Move_SpendsStaminaAndUpdatesPosition()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");

            _bus.Fire(new UnitMovedSignal
            {
                UnitId = "u1",
                NewPosition = new Vector2Int(2, 1),
                Cost = 2f,
            });

            Assert.IsTrue(
                _service.TryGetUnitPosition("u1", out Vector2Int pos));
            Assert.AreEqual(new Vector2Int(2, 1), pos);
            Assert.AreEqual(3f, _service.GetStamina("u1"), 0.0001f);
            Assert.IsEmpty(_interrupts);
        }

        [Test]
        public void Move_InsufficientStamina_InterruptsAndKeepsState()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");

            _bus.Fire(new UnitMovedSignal
            {
                UnitId = "u1",
                NewPosition = new Vector2Int(9, 9),
                Cost = 99f,
            });

            Assert.AreEqual(1, _interrupts.Count);
            Assert.AreEqual("u1", _interrupts[0].UnitId);
            Assert.IsTrue(
                _service.TryGetUnitPosition("u1", out Vector2Int pos));
            Assert.AreEqual(new Vector2Int(1, 1), pos);
            Assert.AreEqual(5f, _service.GetStamina("u1"), 0.0001f);
        }

        [Test]
        public void Move_NegativeCost_InterruptsAndKeepsState()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");

            _bus.Fire(new UnitMovedSignal
            {
                UnitId = "u1",
                NewPosition = new Vector2Int(2, 1),
                Cost = -1f,
            });

            Assert.AreEqual(1, _interrupts.Count);
            Assert.AreEqual(5f, _service.GetStamina("u1"), 0.0001f);
            Assert.IsTrue(
                _service.TryGetUnitPosition("u1", out Vector2Int pos));
            Assert.AreEqual(new Vector2Int(1, 1), pos);
        }

        [Test]
        public void Move_UnknownUnit_IgnoredWithoutInterrupt()
        {
            _bus.Fire(new UnitMovedSignal
            {
                UnitId = "ghost",
                NewPosition = new Vector2Int(2, 2),
                Cost = 1f,
            });

            Assert.IsEmpty(_interrupts);
            Assert.IsFalse(_service.TryGetUnitPosition("ghost", out _));
        }

        [Test]
        public void Move_GarrisonedUnit_InterruptsAndStaysGarrisoned()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(1, 2));

            _bus.Fire(new UnitMovedSignal
            {
                UnitId = "u1",
                NewPosition = new Vector2Int(2, 1),
                Cost = 1f,
            });

            Assert.AreEqual(1, _interrupts.Count);
            Assert.IsTrue(_service.IsGarrisoned("u1"));
            Assert.IsTrue(
                _service.TryGetUnitPosition("u1", out Vector2Int pos));
            Assert.AreEqual(new Vector2Int(1, 1), pos);
        }

        // ── Stamina ──────────────────────────────────────────────────

        [Test]
        public void SetStamina_ClampsToMovementPointsPerTurn()
        {
            FireCreated("u1", "warrior", Vector2Int.zero, "p1");

            _service.SetStamina("u1", 99f);
            Assert.AreEqual(5f, _service.GetStamina("u1"), 0.0001f);
        }

        [Test]
        public void SetStamina_Negative_ClampsToZero()
        {
            FireCreated("u1", "warrior", Vector2Int.zero, "p1");

            _service.SetStamina("u1", -3f);
            Assert.AreEqual(0f, _service.GetStamina("u1"), 0.0001f);
        }

        [Test]
        public void SetStamina_UnknownOrNullId_NoOp()
        {
            _service.SetStamina("ghost", 3f);
            _service.SetStamina(null, 3f);

            Assert.AreEqual(0f, _service.GetStamina("ghost"));
            Assert.IsEmpty(_service.GetAllUnitIds());
        }

        [Test]
        public void SetStamina_ConfigRemoved_HasNoUpperCap()
        {
            FireCreated("u1", "warrior", Vector2Int.zero, "p1");
            _configs.Remove("warrior");

            _service.SetStamina("u1", 99f);
            Assert.AreEqual(99f, _service.GetStamina("u1"), 0.0001f);
        }

        // ── Vision ───────────────────────────────────────────────────

        [Test]
        public void Vision_StoredFromCreateSignal_AndReportedInGarrisonSignal()
        {
            FireCreated("u1", "warrior", Vector2Int.zero, "p1", vision: 7);

            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(0, 1));

            Assert.AreEqual(1, _garrisonSignals.Count);
            Assert.AreEqual(7, _garrisonSignals[0].VisionRange);
        }

        [Test]
        public void Vision_NegativeRange_ClampedToZero()
        {
            FireCreated("u1", "warrior", Vector2Int.zero, "p1", vision: -5);

            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(0, 1));

            Assert.AreEqual(0, _garrisonSignals[0].VisionRange);
        }

        // ── Garrison ─────────────────────────────────────────────────

        [Test]
        public void EnterGarrison_UnregisteredUnit_Fails()
        {
            Assert.IsFalse(_service.TryEnterGarrison(
                "ghost", new Vector2Int(1, 1), out string reason));
            Assert.IsNotEmpty(reason);
            Assert.IsFalse(_service.IsGarrisoned("ghost"));
        }

        [Test]
        public void EnterGarrison_BuildingNotOccupied_Fails()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");

            Assert.IsFalse(_service.TryEnterGarrison(
                "u1", new Vector2Int(1, 2), out string reason));
            Assert.IsNotEmpty(reason);
            Assert.IsFalse(_service.IsGarrisoned("u1"));
        }

        [Test]
        public void EnterGarrison_TooFarFromBuilding_Fails()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            _objectsMap.SetOccupant(new Vector2Int(5, 5), "bld-1");

            Assert.IsFalse(_service.TryEnterGarrison(
                "u1", new Vector2Int(5, 5), out string reason));
            Assert.IsNotEmpty(reason);
            Assert.IsFalse(_service.IsGarrisoned("u1"));
        }

        [Test]
        public void EnterGarrison_Success_FreesCellDeactivatesAndSignals()
        {
            var unitObject = TrackObject(new GameObject("u1"));
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1", obj: unitObject);
            _objectsMap.SetOccupant(new Vector2Int(1, 1), "u1");
            _objectsMap.SetOccupant(new Vector2Int(1, 2), "bld-1");

            Assert.IsTrue(_service.TryEnterGarrison(
                "u1", new Vector2Int(1, 2), out string reason), reason);

            Assert.IsTrue(_service.IsGarrisoned("u1"));
            Assert.IsFalse(_objectsMap.IsOccupied(new Vector2Int(1, 1)));
            Assert.IsFalse(unitObject.activeSelf);
            Assert.AreEqual(1, _garrisonSignals.Count);
            UnitGarrisonStateChangedSignal signal = _garrisonSignals[0];
            Assert.IsTrue(signal.IsGarrisoned);
            Assert.AreEqual("u1", signal.UnitId);
            Assert.AreEqual(new Vector2Int(1, 2), signal.BuildingPosition);
            Assert.AreEqual(new Vector2Int(1, 1), signal.UnitPosition);
            Assert.AreEqual("p1", signal.OwnerId);
            Assert.IsTrue(_service.TryGetUnitPosition("u1", out Vector2Int pos));
            Assert.AreEqual(new Vector2Int(1, 1), pos);
        }

        [Test]
        public void EnterGarrison_WithSharedOccupancyMap_UsesSharedUnregister()
        {
            var sharedMap = new FakeSharedObjectsMap();
            UnitService service = CreateService(sharedMap);
            _extraServices.Add(service);
            service.Initialize();
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            sharedMap.SetOccupant(new Vector2Int(1, 1), "u1");
            sharedMap.SetOccupant(new Vector2Int(1, 2), "bld-1");

            Assert.IsTrue(service.TryEnterGarrison(
                "u1", new Vector2Int(1, 2), out _));

            CollectionAssert.Contains(sharedMap.UnregisterOccupantCalls, "u1");
            Assert.IsFalse(sharedMap.IsOccupied(new Vector2Int(1, 1)));
        }

        [Test]
        public void EnterGarrison_AlreadyGarrisoned_Fails()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(1, 2));

            Assert.IsFalse(_service.TryEnterGarrison(
                "u1", new Vector2Int(2, 2), out string reason));
            Assert.IsNotEmpty(reason);
        }

        [Test]
        public void RestoreGarrison_SameBuilding_IsIdempotent()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(1, 2));

            Assert.IsTrue(_service.TryRestoreGarrison(
                "u1", new Vector2Int(1, 2), out _));
            Assert.IsTrue(_service.IsGarrisoned("u1"));
            // Only the first enter produced a signal.
            Assert.AreEqual(1, _garrisonSignals.Count);
        }

        [Test]
        public void RestoreGarrison_OtherBuilding_Fails()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(1, 2));
            _objectsMap.SetOccupant(new Vector2Int(3, 3), "bld-2");

            Assert.IsFalse(_service.TryRestoreGarrison(
                "u1", new Vector2Int(3, 3), out string reason));
            Assert.IsNotEmpty(reason);
            Assert.IsTrue(_service.IsGarrisoned("u1"));
        }

        [Test]
        public void RestoreGarrison_WithSharedOccupancyMap_UnregistersSharedFirst()
        {
            var sharedMap = new FakeSharedObjectsMap();
            UnitService service = CreateService(sharedMap);
            _extraServices.Add(service);
            service.Initialize();
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            // The unit is a shared occupant on a gate cell: the primary
            // occupant is the building, not the unit.
            sharedMap.SetOccupant(new Vector2Int(1, 1), "gate-1");
            sharedMap.MarkShared("u1");
            sharedMap.SetOccupant(new Vector2Int(1, 2), "bld-1");

            Assert.IsTrue(service.TryRestoreGarrison(
                "u1", new Vector2Int(1, 2), out _));

            CollectionAssert.Contains(sharedMap.UnregisterOccupantCalls, "u1");
            Assert.IsFalse(sharedMap.IsSharedOccupant("u1"));
            Assert.IsTrue(service.IsGarrisoned("u1"));
        }

        [Test]
        public void ExitGarrison_NotGarrisoned_Fails()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");

            Assert.IsFalse(_service.TryExitGarrison(
                "u1", new Vector2Int(2, 2), out string reason));
            Assert.IsNotEmpty(reason);
        }

        [Test]
        public void ExitGarrison_OccupiedTarget_FailsAndStaysGarrisoned()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(1, 2));
            _objectsMap.SetOccupant(new Vector2Int(2, 2), "other-unit");

            Assert.IsFalse(_service.TryExitGarrison(
                "u1", new Vector2Int(2, 2), out string reason));
            Assert.IsNotEmpty(reason);
            Assert.IsTrue(_service.IsGarrisoned("u1"));
        }

        [Test]
        public void ExitGarrison_MissingTile_Fails()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(1, 2));

            Assert.IsFalse(_service.TryExitGarrison(
                "u1", new Vector2Int(99, 99), out string reason));
            Assert.IsNotEmpty(reason);
            Assert.IsTrue(_service.IsGarrisoned("u1"));
        }

        [Test]
        public void ExitGarrison_Success_RegistersOccupantActivatesAndSignals()
        {
            var unitObject = TrackObject(new GameObject("u1"));
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1", obj: unitObject);
            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(1, 2));
            _garrisonSignals.Clear();

            Assert.IsTrue(_service.TryExitGarrison(
                "u1", new Vector2Int(2, 2), out string reason), reason);

            Assert.IsFalse(_service.IsGarrisoned("u1"));
            Assert.IsTrue(_objectsMap.TryGetOccupant(
                new Vector2Int(2, 2), out string occupantId));
            Assert.AreEqual("u1", occupantId);
            Assert.IsTrue(unitObject.activeSelf);
            Assert.IsTrue(_service.TryGetUnitPosition("u1", out Vector2Int pos));
            Assert.AreEqual(new Vector2Int(2, 2), pos);
            Assert.AreEqual(1, _garrisonSignals.Count);
            UnitGarrisonStateChangedSignal signal = _garrisonSignals[0];
            Assert.IsFalse(signal.IsGarrisoned);
            Assert.AreEqual(new Vector2Int(2, 2), signal.UnitPosition);
            Assert.AreEqual("p1", signal.OwnerId);
        }

        [Test]
        public void ExitGarrisonNear_PicksDeterministicFreeCell()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(1, 2));
            var origin = new Vector2Int(4, 4);
            _objectsMap.SetOccupant(origin, "blocker");

            Assert.IsTrue(_service.TryExitGarrisonNear(
                "u1", origin, 2, out Vector2Int target, out string reason), reason);

            // Clockwise perimeter starts at the top edge of radius 1.
            Assert.AreEqual(new Vector2Int(3, 5), target);
            Assert.IsTrue(_objectsMap.TryGetOccupant(target, out string occ));
            Assert.AreEqual("u1", occ);
        }

        [Test]
        public void ExitGarrisonNear_NoFreeCell_Fails()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(1, 2));
            var origin = new Vector2Int(4, 4);
            _objectsMap.SetOccupant(origin, "blocker");

            Assert.IsFalse(_service.TryExitGarrisonNear(
                "u1", origin, 0, out Vector2Int target, out string reason));
            Assert.IsNotEmpty(reason);
            Assert.AreEqual(origin, target);
            Assert.IsTrue(_service.IsGarrisoned("u1"));
        }

        // ── Destroy ──────────────────────────────────────────────────

        [Test]
        public void Destroy_RemovesAllUnitState()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");

            _bus.Fire(new UnitDestroyedSignal { UnitId = "u1" });

            Assert.IsFalse(_service.TryGetUnitPosition("u1", out _));
            Assert.IsNull(_service.GetUnitTypeId("u1"));
            Assert.AreEqual(0f, _service.GetStamina("u1"));
            Assert.IsNull(_service.GetUnitObject("u1"));
            Assert.IsEmpty(_service.GetAllUnitIds());
            Assert.AreEqual("player_0", _service.GetUnitOwnerId("u1"));
            Assert.IsFalse(_service.IsGarrisoned("u1"));
        }

        [Test]
        public void Destroy_FreesOccupancyAndUnregistersHealth()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            _objectsMap.SetOccupant(new Vector2Int(1, 1), "u1");

            _bus.Fire(new UnitDestroyedSignal { UnitId = "u1" });

            Assert.IsFalse(_objectsMap.IsOccupied(new Vector2Int(1, 1)));
            Assert.IsFalse(_health.TryGet("u1", out _));
        }

        [Test]
        public void Destroy_WithSharedOccupancyMap_UnregistersOccupantById()
        {
            var sharedMap = new FakeSharedObjectsMap();
            UnitService service = CreateService(sharedMap);
            _extraServices.Add(service);
            service.Initialize();
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            sharedMap.SetOccupant(new Vector2Int(1, 1), "u1");

            _bus.Fire(new UnitDestroyedSignal { UnitId = "u1" });

            CollectionAssert.Contains(sharedMap.UnregisterOccupantCalls, "u1");
            Assert.IsFalse(sharedMap.IsOccupied(new Vector2Int(1, 1)));
        }

        [Test]
        public void Destroy_UnknownUnit_IsNoOp()
        {
            Assert.DoesNotThrow(
                () => _bus.Fire(new UnitDestroyedSignal { UnitId = "ghost" }));
            Assert.IsEmpty(_service.GetAllUnitIds());
        }

        [Test]
        public void Destroy_GarrisonedUnit_RemovesEverythingWithoutGarrisonSignal()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            EnterGarrison(_service, _objectsMap, "u1", new Vector2Int(1, 2));
            _garrisonSignals.Clear();

            _bus.Fire(new UnitDestroyedSignal { UnitId = "u1" });

            Assert.IsFalse(_service.IsGarrisoned("u1"));
            Assert.IsFalse(_service.TryGetUnitPosition("u1", out _));
            Assert.IsFalse(_health.TryGet("u1", out _));
            Assert.IsEmpty(_garrisonSignals);
        }

        [Test]
        public void Destroy_ViaHealthKill_RemovesUnitState()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            Assert.IsTrue(_health.TryGet("u1", out IHealth health));

            health.TakeDamage(30);

            Assert.IsFalse(_service.TryGetUnitPosition("u1", out _));
            Assert.IsFalse(_health.TryGet("u1", out _));
        }

        // ── Replication boundary ─────────────────────────────────────
        // WorldStateReplicationService restores units through
        // IUnitFactory.CreateUnitWithId (UnitCreatedSignal) followed by
        // SetStamina and a health damage offset. These tests lock that
        // contract against the service.

        [Test]
        public void Replication_ForcedIdThenStatRestore_RebuildsSnapshot()
        {
            FireCreated("net-42", "archer", new Vector2Int(4, 4), "p2", vision: 2);
            _service.SetStamina("net-42", 2.5f);
            Assert.IsTrue(_health.TryGet("net-42", out IHealth health));
            health.TakeDamage(Math.Max(0, health.CurrentHp - 12));

            Assert.IsTrue(
                _service.TryGetUnitPosition("net-42", out Vector2Int pos));
            Assert.AreEqual(new Vector2Int(4, 4), pos);
            Assert.AreEqual("archer", _service.GetUnitTypeId("net-42"));
            Assert.AreEqual("p2", _service.GetUnitOwnerId("net-42"));
            Assert.AreEqual(2.5f, _service.GetStamina("net-42"), 0.0001f);
            Assert.AreEqual(12, health.CurrentHp);
        }

        [Test]
        public void Replication_SnapshotFlow_PreservesAllReadableAttributes()
        {
            // Simulates the host->client record: id, type, owner, position,
            // stamina, hp must all be readable from the canonical services.
            FireCreated("net-7", "warrior", new Vector2Int(2, 3), "p9", vision: 4);
            _service.SetStamina("net-7", 1.5f);

            Assert.AreEqual("net-7",
                _service.GetAllUnitIds().Single());
            Assert.AreEqual("p9", _service.GetUnitOwnerId("net-7"));
            Assert.AreEqual("warrior", _service.GetUnitTypeId("net-7"));
            Assert.IsTrue(
                _service.TryGetUnitPosition("net-7", out Vector2Int pos));
            Assert.AreEqual(new Vector2Int(2, 3), pos);
            Assert.AreEqual(1.5f, _service.GetStamina("net-7"), 0.0001f);
        }

        // ── Helpers ──────────────────────────────────────────────────

        private UnitService CreateService(IObjectsMapService objectsMap)
        {
            return new UnitService(
                _bus,
                _grid,
                _tileSettings,
                _configs,
                objectsMap,
                healthRegistry: _health,
                buildingRegistry: null,
                gateStateService: null);
        }

        private void FireCreated(
            string unitId,
            string typeId,
            Vector2Int position,
            string ownerId,
            int vision = 3,
            GameObject obj = null)
        {
            _bus.Fire(new UnitCreatedSignal
            {
                UnitId = unitId,
                UnitTypeId = typeId,
                Position = position,
                VisionRange = vision,
                UnitObject = obj,
                OwnerId = ownerId,
            });
        }

        private static void EnterGarrison(
            UnitService service,
            FakeObjectsMap objectsMap,
            string unitId,
            Vector2Int buildingPosition)
        {
            objectsMap.SetOccupant(buildingPosition, "bld-1");
            Assert.IsTrue(
                service.TryEnterGarrison(unitId, buildingPosition, out string reason),
                reason);
        }

        private GameObject TrackObject(GameObject go)
        {
            _objectsToCleanup.Add(go);
            return go;
        }

        private static SignalBus CreateSignalBus()
        {
            var container = new DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<UnitCreatedSignal>();
            container.DeclareSignal<UnitMovedSignal>();
            container.DeclareSignal<UnitDestroyedSignal>();
            container.DeclareSignal<UnitGarrisonStateChangedSignal>()
                .OptionalSubscriber();
            container.DeclareSignal<InterruptMovementSignal>()
                .OptionalSubscriber();
            return container.Resolve<SignalBus>();
        }

        // ── Fakes ────────────────────────────────────────────────────

        private sealed class FakeGrid : IGridService
        {
            public FakeGrid(int width, int height)
            {
                GridWidth = width;
                GridHeight = height;
            }

            public int GridWidth { get; }
            public int GridHeight { get; }
            public string GetTileData(Vector2Int position) => "grass";

            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                if (position.x < 0 || position.y < 0
                    || position.x >= GridWidth || position.y >= GridHeight)
                {
                    tileTypeId = null;
                    return false;
                }
                tileTypeId = "grass";
                return true;
            }

            public void SetTileData(Vector2Int position, string tileTypeId) { }
        }

        private sealed class FakeTileSettings : ITileSettingsService
        {
            public float GetTileWeight(string tileId) => 1f;
            public bool IsBuildBlocked(string tileId) => false;
            public float GetSurfaceOffset(string tileId) => 0f;
        }

        private sealed class FakeUnitClassConfig : IUnitClassConfig
        {
            private readonly Dictionary<string, UnitClassConfig> _configs = new();
            public void Set(string typeId, UnitClassConfig config)
                => _configs[typeId] = config;
            public void Remove(string typeId) => _configs.Remove(typeId);
            public UnitClassConfig GetConfig(string typeId)
                => _configs.TryGetValue(typeId, out UnitClassConfig config)
                    ? config
                    : null;
        }

        private class FakeObjectsMap : IObjectsMapService
        {
            protected readonly Dictionary<Vector2Int, string> Occupants = new();

            public void SetOccupant(Vector2Int position, string occupantId)
                => Occupants[position] = occupantId;

            public bool IsOccupied(Vector2Int position)
                => Occupants.ContainsKey(position);

            public bool TryGetOccupant(Vector2Int position, out string occupantId)
                => Occupants.TryGetValue(position, out occupantId);

            public virtual void Register(Vector2Int position, string occupantId)
                => Occupants[position] = occupantId;

            public void Move(Vector2Int from, Vector2Int to)
            {
                if (Occupants.TryGetValue(from, out string occupantId))
                {
                    Occupants.Remove(from);
                    Occupants[to] = occupantId;
                }
            }

            public virtual void Unregister(Vector2Int position)
                => Occupants.Remove(position);

            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                foreach (var pair in Occupants)
                {
                    if (pair.Value == occupantId)
                    {
                        position = pair.Key;
                        return true;
                    }
                }
                position = default;
                return false;
            }
        }

        private sealed class FakeSharedObjectsMap
            : FakeObjectsMap, IObjectsMapSharedOccupancy
        {
            private readonly HashSet<string> _shared = new(StringComparer.Ordinal);
            public readonly List<string> UnregisterOccupantCalls = new();

            public void MarkShared(string occupantId) => _shared.Add(occupantId);

            public bool TryUnregisterOccupant(string occupantId)
            {
                UnregisterOccupantCalls.Add(occupantId);
                bool removed = _shared.Remove(occupantId);
                Vector2Int? primary = null;
                foreach (var pair in Occupants)
                {
                    if (pair.Value == occupantId)
                    {
                        primary = pair.Key;
                        break;
                    }
                }
                if (primary.HasValue)
                {
                    Occupants.Remove(primary.Value);
                    removed = true;
                }
                return removed;
            }

            public bool IsSharedOccupant(string occupantId)
                => _shared.Contains(occupantId);
        }

        private sealed class FakeHealthRegistry : IHealthRegistry
        {
            private readonly Dictionary<string, IHealth> _entries =
                new(StringComparer.Ordinal);

            public void Register(IHealth health)
            {
                if (health != null)
                    _entries[health.EntityId] = health;
            }

            public void Unregister(string entityId) => _entries.Remove(entityId);

            public IHealth Get(string entityId)
                => TryGet(entityId, out IHealth health) ? health : null;

            public bool TryGet(string entityId, out IHealth health)
            {
                if (_entries.TryGetValue(entityId, out IHealth found)
                    && !found.IsDestroyed)
                {
                    health = found;
                    return true;
                }
                health = null;
                return false;
            }

            public IReadOnlyCollection<IHealth> GetAll()
                => _entries.Values
                    .Where(h => !h.IsDestroyed)
                    .ToArray();

            public IReadOnlyCollection<IHealth> GetMany(
                IEnumerable<string> entityIds)
                => entityIds?
                        .Select(id => Get(id))
                        .Where(h => h != null)
                        .ToArray()
                    ?? (IReadOnlyCollection<IHealth>)Array.Empty<IHealth>();

            public int Count => _entries.Count;
        }
    }
}
