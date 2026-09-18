using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    [TestFixture]
    public sealed class FogIntelStoreTests
    {
        private const string Observer = "owner-a";
        private const string Enemy = "owner-b";

        private DiContainer _container;
        private SignalBus _signals;
        private FakeOwnerStateReader _fog;
        private FakeOwnerSnapshotStore _snapshots;
        private FakeVisibilityFeed _feed;
        private FogIntelStore _store;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<UnitCreatedSignal>();
            _container.DeclareSignal<UnitMovedSignal>();
            _container.DeclareSignal<UnitDestroyedSignal>();
            _container.DeclareSignal<UnitGarrisonStateChangedSignal>();
            _container.DeclareSignal<BuildingPlacedSignal>();
            _container.DeclareSignal<BuildingDemolishedSignal>();
            _container.DeclareSignal<BuildingOwnershipTransferredSignal>();

            _signals = _container.Resolve<SignalBus>();
            _fog = new FakeOwnerStateReader();
            _snapshots = new FakeOwnerSnapshotStore();
            _feed = new FakeVisibilityFeed();
            _snapshots.KnownOwners.Add(Observer);
            _snapshots.KnownOwners.Add(Enemy);

            _store = new FogIntelStore(_signals, _fog, _snapshots, _feed);
            _store.Initialize();
        }

        [TearDown]
        public void TearDown() => _store.Dispose();

        // ── Unit observation ────────────────────────────────────────────────

        [Test]
        public void UnitCreated_InObserverVision_RecordsIntel()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));

            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));

            Assert.IsTrue(_store.TryGetRememberedUnit(Observer, "u1", out var record));
            Assert.AreEqual(new Vector2Int(4, 4), record.LastKnownPosition);
            Assert.AreEqual(Enemy, record.OwnerId);
            Assert.AreEqual("warrior", record.TypeId);
        }

        [Test]
        public void UnitCreated_OutsideVision_NoIntel()
        {
            FireUnitCreated("u1", Enemy, new Vector2Int(9, 9));

            Assert.IsFalse(_store.TryGetRememberedUnit(Observer, "u1", out _));
            Assert.AreEqual(0, _store.GetRememberedUnits(Observer).Count);
        }

        [Test]
        public void OwnUnit_NeverRecorded()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));

            FireUnitCreated("mine", Observer, new Vector2Int(4, 4));

            Assert.IsFalse(_store.TryGetRememberedUnit(Observer, "mine", out _));
        }

        // ── Movement ────────────────────────────────────────────────────────

        [Test]
        public void UnitMoved_WhileVisible_UpdatesRecord()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            _fog.SetVisible(Observer, new Vector2Int(5, 4));
            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));

            FireUnitMoved("u1", new Vector2Int(5, 4));

            Assert.IsTrue(_store.TryGetRememberedUnit(Observer, "u1", out var record));
            Assert.AreEqual(new Vector2Int(5, 4), record.LastKnownPosition);
        }

        [Test]
        public void UnitMoved_IntoHidden_StalePositionKept()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));

            FireUnitMoved("u1", new Vector2Int(20, 20));

            Assert.IsTrue(_store.TryGetRememberedUnit(Observer, "u1", out var record));
            Assert.AreEqual(new Vector2Int(4, 4), record.LastKnownPosition);
        }

        // ── Death ───────────────────────────────────────────────────────────

        [Test]
        public void UnitDestroyed_DeathVisible_RecordRemoved()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));

            _signals.Fire(new UnitDestroyedSignal { UnitId = "u1" });

            Assert.IsFalse(_store.TryGetRememberedUnit(Observer, "u1", out _));
        }

        [Test]
        public void UnitDestroyed_DeathHidden_StaleRecordKept()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));
            FireUnitMoved("u1", new Vector2Int(20, 20));

            _signals.Fire(new UnitDestroyedSignal { UnitId = "u1" });

            Assert.IsTrue(_store.TryGetRememberedUnit(Observer, "u1", out var record));
            Assert.AreEqual(new Vector2Int(4, 4), record.LastKnownPosition);
        }

        // ── Re-observation reconciliation ────────────────────────────────────

        [Test]
        public void Reobserve_UnitStillThere_RecordRefreshed()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));
            _fog.Clear();
            long before = _store.GetRememberedUnits(Observer).Single().LastSeenSequence;

            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            _feed.Emit(Observer, new[] { new Vector2Int(4, 4) });

            Assert.IsTrue(_store.TryGetRememberedUnit(Observer, "u1", out var record));
            Assert.Greater(record.LastSeenSequence, before);
        }

        [Test]
        public void Reobserve_UnitMovedAway_RecordRemoved()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));
            FireUnitMoved("u1", new Vector2Int(20, 20));
            _fog.Clear();

            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            _feed.Emit(Observer, new[] { new Vector2Int(4, 4) });

            Assert.IsFalse(_store.TryGetRememberedUnit(Observer, "u1", out _));
        }

        [Test]
        public void Reobserve_MultipleUnitsSameCell_OnlyGoneRemoved()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));
            FireUnitCreated("u2", Enemy, new Vector2Int(4, 4));
            FireUnitMoved("u1", new Vector2Int(20, 20));
            _fog.Clear();

            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            _feed.Emit(Observer, new[] { new Vector2Int(4, 4) });

            Assert.IsFalse(_store.TryGetRememberedUnit(Observer, "u1", out _));
            Assert.IsTrue(_store.TryGetRememberedUnit(Observer, "u2", out _));
        }

        // ── Buildings ───────────────────────────────────────────────────────

        [Test]
        public void BuildingPlaced_InVision_Recorded()
        {
            _fog.SetVisible(Observer, new Vector2Int(7, 7));

            FireBuildingPlaced("b1", Enemy, new Vector2Int(7, 7), rotation: 2);

            Assert.IsTrue(_store.TryGetRememberedBuilding(Observer, new Vector2Int(7, 7), out var record));
            Assert.AreEqual("b1", record.BuildingId);
            Assert.AreEqual(2, record.RotationQuarterTurns);
        }

        [Test]
        public void BuildingDemolished_Hidden_StaleRecordKept_ThenReconcileRemoves()
        {
            _fog.SetVisible(Observer, new Vector2Int(7, 7));
            FireBuildingPlaced("b1", Enemy, new Vector2Int(7, 7));
            _fog.Clear();

            _signals.Fire(new BuildingDemolishedSignal
            {
                BuildingId = "b1",
                Position = new Vector2Int(7, 7),
                OwnerId = Enemy,
            });

            Assert.IsTrue(_store.TryGetRememberedBuilding(Observer, new Vector2Int(7, 7), out _));

            _fog.SetVisible(Observer, new Vector2Int(7, 7));
            _feed.Emit(Observer, new[] { new Vector2Int(7, 7) });

            Assert.IsFalse(_store.TryGetRememberedBuilding(Observer, new Vector2Int(7, 7), out _));
        }

        [Test]
        public void BuildingDemolished_Visible_RecordRemovedImmediately()
        {
            _fog.SetVisible(Observer, new Vector2Int(7, 7));
            FireBuildingPlaced("b1", Enemy, new Vector2Int(7, 7));

            _signals.Fire(new BuildingDemolishedSignal
            {
                BuildingId = "b1",
                Position = new Vector2Int(7, 7),
                OwnerId = Enemy,
            });

            Assert.IsFalse(_store.TryGetRememberedBuilding(Observer, new Vector2Int(7, 7), out _));
        }

        // ── Ownership transfer ──────────────────────────────────────────────

        [Test]
        public void BuildingTransfer_Visible_OwnerUpdated_Hidden_OwnerStale()
        {
            _fog.SetVisible(Observer, new Vector2Int(7, 7));
            _fog.SetVisible(Observer, new Vector2Int(8, 8));
            _snapshots.KnownOwners.Add("owner-c");
            FireBuildingPlaced("b1", Enemy, new Vector2Int(7, 7));
            FireBuildingPlaced("b2", Enemy, new Vector2Int(8, 8));
            _fog.Clear();
            _fog.SetVisible(Observer, new Vector2Int(7, 7));

            _signals.Fire(new BuildingOwnershipTransferredSignal
            {
                BuildingId = "b1",
                Position = new Vector2Int(7, 7),
                PreviousOwnerId = Enemy,
                NewOwnerId = "owner-c",
            });
            _signals.Fire(new BuildingOwnershipTransferredSignal
            {
                BuildingId = "b2",
                Position = new Vector2Int(8, 8),
                PreviousOwnerId = Enemy,
                NewOwnerId = "owner-c",
            });

            Assert.AreEqual("owner-c",
                _store.GetRememberedBuildings(Observer).Single(r => r.Position == new Vector2Int(7, 7)).OwnerId);
            Assert.AreEqual(Enemy,
                _store.GetRememberedBuildings(Observer).Single(r => r.Position == new Vector2Int(8, 8)).OwnerId);
        }

        // ── Garrison ────────────────────────────────────────────────────────

        [Test]
        public void UnitGarrisoned_SeenByObserver_RecordRemoved()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            _fog.SetVisible(Observer, new Vector2Int(5, 5));
            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));

            _signals.Fire(new UnitGarrisonStateChangedSignal
            {
                UnitId = "u1",
                IsGarrisoned = true,
                BuildingPosition = new Vector2Int(5, 5),
                UnitPosition = new Vector2Int(4, 4),
                OwnerId = Enemy,
            });

            Assert.IsFalse(_store.TryGetRememberedUnit(Observer, "u1", out _));
        }

        // ── Snapshots & record isolation ────────────────────────────────────

        [Test]
        public void Snapshot_RoundTrip_PreservesRecords()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            _fog.SetVisible(Observer, new Vector2Int(7, 7));
            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));
            FireBuildingPlaced("b1", Enemy, new Vector2Int(7, 7));

            FogIntelSnapshot snapshot = _store.CaptureSnapshot(Observer);

            var restored = new FogIntelStore(_signals, _fog, _snapshots, _feed);
            restored.LoadSnapshot(Observer, snapshot);

            Assert.IsTrue(restored.TryGetRememberedUnit(Observer, "u1", out var unit));
            Assert.AreEqual(new Vector2Int(4, 4), unit.LastKnownPosition);
            Assert.IsTrue(restored.TryGetRememberedBuilding(Observer, new Vector2Int(7, 7), out _));
        }

        [Test]
        public void ReturnedRecords_AreClones_MutationDoesNotCorruptStore()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));

            FogIntelUnitRecord leaked = _store.GetRememberedUnits(Observer).Single();
            leaked.LastKnownPosition = new Vector2Int(99, 99);

            Assert.AreEqual(new Vector2Int(4, 4),
                _store.GetRememberedUnits(Observer).Single().LastKnownPosition);
        }

        [Test]
        public void IntelChanged_FiresOnNewObservation()
        {
            _fog.SetVisible(Observer, new Vector2Int(4, 4));
            string notified = null;
            _store.IntelChanged += owner => notified = owner;

            FireUnitCreated("u1", Enemy, new Vector2Int(4, 4));

            Assert.AreEqual(Observer, notified);
        }

        // ── Helpers & fakes ─────────────────────────────────────────────────

        private void FireUnitCreated(string unitId, string ownerId, Vector2Int position)
            => _signals.Fire(new UnitCreatedSignal
            {
                UnitId = unitId,
                UnitTypeId = "warrior",
                Position = position,
                OwnerId = ownerId,
            });

        private void FireUnitMoved(string unitId, Vector2Int position)
            => _signals.Fire(new UnitMovedSignal
            {
                UnitId = unitId,
                NewPosition = position,
            });

        private void FireBuildingPlaced(string buildingId, string ownerId, Vector2Int position, int rotation = 0)
            => _signals.Fire(new BuildingPlacedSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = ownerId,
                RotationQuarterTurns = rotation,
            });

        private sealed class FakeOwnerStateReader : IFogOwnerStateReader
        {
            private readonly HashSet<(string owner, Vector2Int cell)> _visible =
                new HashSet<(string, Vector2Int)>();

            public void SetVisible(string ownerId, Vector2Int cell) => _visible.Add((ownerId, cell));
            public void Clear() => _visible.Clear();

            public FogStateType GetFogState(string ownerId, Vector2Int position)
                => IsVisible(ownerId, position) ? FogStateType.Visible : FogStateType.Unexplored;
            public bool IsVisible(string ownerId, Vector2Int position)
                => _visible.Contains((ownerId, position));
            public bool IsExplored(string ownerId, Vector2Int position)
                => IsVisible(ownerId, position);
        }

        private sealed class FakeOwnerSnapshotStore : IFogOwnerExplorationSnapshotStore
        {
            public readonly List<string> KnownOwners = new List<string>();
            public IReadOnlyCollection<string> GetKnownFogOwnerIds() => KnownOwners;
            public bool[,] GetExploredSnapshot(string ownerId) => null;
            public void LoadFromSnapshot(string ownerId, bool[,] explored) { }
        }

        private sealed class FakeVisibilityFeed : IFogOwnerVisibilityFeed
        {
            public event Action<string, IReadOnlyCollection<Vector2Int>> CellsBecameVisible;
            public void Emit(string ownerId, IReadOnlyCollection<Vector2Int> cells)
                => CellsBecameVisible?.Invoke(ownerId, cells);
        }
    }
}
