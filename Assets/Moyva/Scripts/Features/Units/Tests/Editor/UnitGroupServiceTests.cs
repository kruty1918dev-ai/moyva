using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Units
{
    [TestFixture]
    public sealed class UnitGroupServiceTests
    {
        private FakeUnitService _units;
        private FakeOwnership _ownership;
        private UnitGroupService _service;

        [SetUp]
        public void SetUp()
        {
            _units = new FakeUnitService();
            _ownership = new FakeOwnership();
            _service = new UnitGroupService(CreateSignalBus(), _units, _ownership);
        }

        [TearDown]
        public void TearDown() => _service.Dispose();

        // ── Creation ─────────────────────────────────────────────────

        [Test]
        public void CreateGroup_AssignsStableIdAndMembership()
        {
            _units.Add("u1", new Vector2Int(0, 0));
            _units.Add("u2", new Vector2Int(1, 0));
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p1");

            Assert.IsTrue(
                _service.TryCreateGroup("p1", new[] { "u1", "u2" }, out string groupId, out _));
            StringAssert.StartsWith("grp-", groupId);
            Assert.AreEqual(groupId, _service.GetGroupIdOfUnit("u1"));
            Assert.AreEqual(groupId, _service.GetGroupIdOfUnit("u2"));

            Assert.IsTrue(_service.TryGetGroup(groupId, out UnitGroupSnapshot snapshot));
            Assert.AreEqual("p1", snapshot.OwnerId);
            CollectionAssert.AreEqual(new[] { "u1", "u2" }, snapshot.UnitIds.ToArray());
        }

        [Test]
        public void CreateGroup_RejectsEmptyOwner()
        {
            _units.Add("u1", Vector2Int.zero);
            Assert.IsFalse(
                _service.TryCreateGroup(" ", new[] { "u1" }, out _, out string reason));
            Assert.IsNotEmpty(reason);
        }

        [Test]
        public void CreateGroup_RejectsEmptyMemberList()
        {
            Assert.IsFalse(
                _service.TryCreateGroup("p1", new string[0], out _, out string reason));
            Assert.IsNotEmpty(reason);
        }

        [Test]
        public void CreateGroup_RejectsMissingUnit()
        {
            Assert.IsFalse(
                _service.TryCreateGroup("p1", new[] { "ghost" }, out _, out string reason));
            StringAssert.Contains("not on the map", reason);
        }

        [Test]
        public void CreateGroup_RejectsForeignUnit()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", Vector2Int.one);
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p2");

            Assert.IsFalse(
                _service.TryCreateGroup("p1", new[] { "u1", "u2" }, out _, out string reason));
            StringAssert.Contains("another owner", reason);
        }

        [Test]
        public void CreateGroup_RejectsAlreadyGroupedUnit()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", Vector2Int.one);
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p1");
            Assert.IsTrue(_service.TryCreateGroup("p1", new[] { "u1" }, out _, out _));

            Assert.IsFalse(
                _service.TryCreateGroup("p1", new[] { "u2", "u1" }, out _, out string reason));
            StringAssert.Contains("already belongs", reason);
        }

        [Test]
        public void CreateGroup_DeduplicatesRepeatedIds()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", Vector2Int.one);
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p1");

            Assert.IsTrue(_service.TryCreateGroup(
                "p1", new[] { "u1", "u1", "u2" }, out string groupId, out _));
            _service.TryGetGroup(groupId, out UnitGroupSnapshot snapshot);
            Assert.AreEqual(2, snapshot.Count);
        }

        // ── Membership mutation ──────────────────────────────────────

        [Test]
        public void AddUnit_ThenRemove_EmptyGroupDisbands()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", Vector2Int.one);
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p1");
            _service.TryCreateGroup("p1", new[] { "u1" }, out string groupId, out _);

            Assert.IsTrue(_service.TryAddUnit("p1", groupId, "u2", out _));
            Assert.IsTrue(_service.TryRemoveUnit("p1", groupId, "u1", out _));
            Assert.IsTrue(_service.TryRemoveUnit("p1", groupId, "u2", out _));

            Assert.IsFalse(_service.TryGetGroup(groupId, out _));
        }

        [Test]
        public void AddUnit_RejectsForeignUnit()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", Vector2Int.one);
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p2");
            _service.TryCreateGroup("p1", new[] { "u1" }, out string groupId, out _);

            Assert.IsFalse(_service.TryAddUnit("p1", groupId, "u2", out string reason));
            StringAssert.Contains("another owner", reason);
        }

        [Test]
        public void Disband_ClearsMembership()
        {
            _units.Add("u1", Vector2Int.zero);
            _ownership.Set("u1", "p1");
            _service.TryCreateGroup("p1", new[] { "u1" }, out string groupId, out _);

            Assert.IsTrue(_service.TryDisbandGroup("p1", groupId, out _));
            Assert.AreEqual(string.Empty, _service.GetGroupIdOfUnit("u1"));
            Assert.IsFalse(_service.TryGetGroup(groupId, out _));
        }

        [Test]
        public void Disband_RejectsForeignOwner()
        {
            _units.Add("u1", Vector2Int.zero);
            _ownership.Set("u1", "p1");
            _service.TryCreateGroup("p1", new[] { "u1" }, out string groupId, out _);

            Assert.IsFalse(_service.TryDisbandGroup("p2", groupId, out string reason));
            StringAssert.Contains("another owner", reason);
        }

        // ── Movement ─────────────────────────────────────────────────

        [Test]
        public void MoveGroup_WithoutMovementService_Rejected()
        {
            _units.Add("u1", Vector2Int.zero);
            _ownership.Set("u1", "p1");
            _service.TryCreateGroup("p1", new[] { "u1" }, out string groupId, out _);

            Assert.IsFalse(_service.TryMoveGroup(
                "p1", groupId, new Vector2Int(3, 3), out string reason));
            StringAssert.Contains("unavailable", reason);
        }

        [Test]
        public void MoveGroup_RejectsForeignOwner()
        {
            _units.Add("u1", Vector2Int.zero);
            _ownership.Set("u1", "p1");
            _service.TryCreateGroup("p1", new[] { "u1" }, out string groupId, out _);

            Assert.IsFalse(_service.TryMoveGroup(
                "p2", groupId, new Vector2Int(3, 3), out _));
        }

        [Test]
        public void MoveGroup_DecomposesIntoPerUnitMoves()
        {
            // u1 at (4,4) is strictly closer to target than u2 at (0,0).
            // Both can reach (5,5) and (5,4); nearest member claims the
            // requested cell, the other takes the next best formation cell.
            _units.Add("u1", new Vector2Int(4, 4));
            _units.Add("u2", new Vector2Int(0, 0));
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p1");

            var target = new Vector2Int(5, 5);
            var near = new Vector2Int(5, 4);
            var movement = new FakeMovement();
            var movementQuery = new FakeMovementQuery();
            movementQuery.SetTiles("u1", Reachable(target, near));
            movementQuery.SetTiles("u2", Reachable(target, near));
            var objectsMap = new FakeObjectsMap();
            objectsMap.SetOccupant(new Vector2Int(4, 4), "u1");
            objectsMap.SetOccupant(new Vector2Int(0, 0), "u2");
            var grid = new FakeGrid(16, 16);

            var service = new UnitGroupService(
                CreateSignalBus(), _units, _ownership, movement, movementQuery, objectsMap, grid);
            try
            {
                service.TryCreateGroup("p1", new[] { "u1", "u2" }, out string groupId, out _);
                Assert.IsTrue(
                    service.TryMoveGroup("p1", groupId, target, out string reason),
                    reason);

                Assert.AreEqual(2, movement.Calls.Count);
                Assert.AreEqual(("u1", target), movement.Calls[0]);
                Assert.AreEqual(("u2", near), movement.Calls[1]);
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void MoveGroup_SucceedsWhenOnlyOneMemberCanMove()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", Vector2Int.one);
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p1");

            var target = new Vector2Int(5, 5);
            var movement = new FakeMovement();
            var movementQuery = new FakeMovementQuery();
            movementQuery.SetTiles("u1", Reachable(target));
            movementQuery.SetTiles("u2", Reachable()); // no reachable tiles
            var grid = new FakeGrid(16, 16);

            var service = new UnitGroupService(
                CreateSignalBus(), _units, _ownership, movement, movementQuery, null, grid);
            try
            {
                service.TryCreateGroup("p1", new[] { "u1", "u2" }, out string groupId, out _);
                Assert.IsTrue(
                    service.TryMoveGroup("p1", groupId, target, out string reason),
                    reason);
                Assert.AreEqual(1, movement.Calls.Count);
                Assert.AreEqual(("u1", target), movement.Calls[0]);
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void MoveGroup_RejectsWhenNoMemberCanMove()
        {
            _units.Add("u1", Vector2Int.zero);
            _ownership.Set("u1", "p1");

            var movement = new FakeMovement();
            var movementQuery = new FakeMovementQuery(); // no tiles at all
            var grid = new FakeGrid(16, 16);

            var service = new UnitGroupService(
                CreateSignalBus(), _units, _ownership, movement, movementQuery, null, grid);
            try
            {
                service.TryCreateGroup("p1", new[] { "u1" }, out string groupId, out _);
                Assert.IsFalse(service.TryMoveGroup(
                    "p1", groupId, new Vector2Int(5, 5), out string reason));
                StringAssert.Contains("reach", reason);
                Assert.AreEqual(0, movement.Calls.Count);
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void MoveGroup_ForeignOccupantBlocksFormationCell()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", new Vector2Int(1, 0));
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p1");

            var target = new Vector2Int(5, 5);
            var near = new Vector2Int(5, 4);
            var movement = new FakeMovement();
            var movementQuery = new FakeMovementQuery();
            movementQuery.SetTiles("u1", Reachable(target, near));
            movementQuery.SetTiles("u2", Reachable(near)); // only 'near' reachable
            var objectsMap = new FakeObjectsMap();
            objectsMap.SetOccupant(near, "enemy-unit"); // foreign blocker
            var grid = new FakeGrid(16, 16);

            var service = new UnitGroupService(
                CreateSignalBus(), _units, _ownership, movement, movementQuery, objectsMap, grid);
            try
            {
                service.TryCreateGroup("p1", new[] { "u1", "u2" }, out string groupId, out _);
                Assert.IsTrue(service.TryMoveGroup("p1", groupId, target, out _));

                // u2 had only the blocked cell reachable → skipped; u1 takes target.
                Assert.AreEqual(1, movement.Calls.Count);
                Assert.AreEqual(("u1", target), movement.Calls[0]);
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void MoveGroup_RejectsOutsideGrid()
        {
            _units.Add("u1", Vector2Int.zero);
            _ownership.Set("u1", "p1");
            var movement = new FakeMovement();
            var grid = new FakeGrid(8, 8);

            var service = new UnitGroupService(
                CreateSignalBus(), _units, _ownership, movement, null, null, grid);
            try
            {
                service.TryCreateGroup("p1", new[] { "u1" }, out string groupId, out _);
                Assert.IsFalse(service.TryMoveGroup(
                    "p1", groupId, new Vector2Int(99, 99), out string reason));
                StringAssert.Contains("outside the map", reason);
            }
            finally
            {
                service.Dispose();
            }
        }

        // ── Lifecycle signals ────────────────────────────────────────

        [Test]
        public void DestroyedMember_IsRemovedFromGroup()
        {
            var bus = CreateSignalBus();
            var service = new UnitGroupService(bus, _units, _ownership);
            try
            {
                _units.Add("u1", Vector2Int.zero);
                _units.Add("u2", Vector2Int.one);
                _ownership.Set("u1", "p1");
                _ownership.Set("u2", "p1");
                service.Initialize();
                service.TryCreateGroup("p1", new[] { "u1", "u2" }, out string groupId, out _);

                bus.Fire(new UnitDestroyedSignal { UnitId = "u1" });

                Assert.AreEqual(string.Empty, service.GetGroupIdOfUnit("u1"));
                Assert.AreEqual(groupId, service.GetGroupIdOfUnit("u2"));
                Assert.IsTrue(service.TryGetGroup(groupId, out UnitGroupSnapshot snapshot));
                Assert.AreEqual(1, snapshot.Count);
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void DestroyedLastMember_DisbandsGroup()
        {
            var bus = CreateSignalBus();
            var service = new UnitGroupService(bus, _units, _ownership);
            try
            {
                _units.Add("u1", Vector2Int.zero);
                _ownership.Set("u1", "p1");
                service.Initialize();
                service.TryCreateGroup("p1", new[] { "u1" }, out string groupId, out _);

                bus.Fire(new UnitDestroyedSignal { UnitId = "u1" });

                Assert.IsFalse(service.TryGetGroup(groupId, out _));
                Assert.AreEqual(string.Empty, service.GetGroupIdOfUnit("u1"));
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void GarrisonedMember_IsRemovedFromGroup()
        {
            var bus = CreateSignalBus();
            var service = new UnitGroupService(bus, _units, _ownership);
            try
            {
                _units.Add("u1", Vector2Int.zero);
                _units.Add("u2", Vector2Int.one);
                _ownership.Set("u1", "p1");
                _ownership.Set("u2", "p1");
                service.Initialize();
                service.TryCreateGroup("p1", new[] { "u1", "u2" }, out string groupId, out _);

                bus.Fire(new UnitGarrisonStateChangedSignal
                {
                    UnitId = "u1",
                    IsGarrisoned = true,
                    OwnerId = "p1",
                });

                Assert.AreEqual(string.Empty, service.GetGroupIdOfUnit("u1"));
                Assert.IsTrue(service.TryGetGroup(groupId, out UnitGroupSnapshot snapshot));
                Assert.AreEqual(1, snapshot.Count);
            }
            finally
            {
                service.Dispose();
            }
        }

        // ── Persistence ──────────────────────────────────────────────

        [Test]
        public void RestoreState_RebuildsMembershipWithoutAuthorityChecks()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", Vector2Int.one);
            // Ownership deliberately differs: restore must not re-check it.
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p2");

            _service.RestoreState(new[]
            {
                new UnitGroupSnapshot("grp-7", "p1", new[] { "u1", "u2" }),
            });

            Assert.AreEqual("grp-7", _service.GetGroupIdOfUnit("u1"));
            Assert.AreEqual("grp-7", _service.GetGroupIdOfUnit("u2"));
        }

        [Test]
        public void RestoreState_DropsMissingUnitsAndEmptyGroups()
        {
            _units.Add("u1", Vector2Int.zero);

            _service.RestoreState(new[]
            {
                new UnitGroupSnapshot("grp-1", "p1", new[] { "u1", "ghost" }),
                new UnitGroupSnapshot("grp-2", "p1", new[] { "ghost-only" }),
            });

            Assert.IsTrue(_service.TryGetGroup("grp-1", out UnitGroupSnapshot snapshot));
            CollectionAssert.AreEqual(new[] { "u1" }, snapshot.UnitIds.ToArray());
            Assert.IsFalse(_service.TryGetGroup("grp-2", out _));
            Assert.AreEqual(string.Empty, _service.GetGroupIdOfUnit("ghost"));
        }

        [Test]
        public void RestoreState_RejectsDuplicateUnitAcrossGroups()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", Vector2Int.one);

            _service.RestoreState(new[]
            {
                new UnitGroupSnapshot("grp-1", "p1", new[] { "u1" }),
                new UnitGroupSnapshot("grp-2", "p1", new[] { "u1", "u2" }),
            });

            // u1 stays with the first restored group; grp-2 keeps only u2.
            Assert.AreEqual("grp-1", _service.GetGroupIdOfUnit("u1"));
            Assert.AreEqual("grp-2", _service.GetGroupIdOfUnit("u2"));
            Assert.IsTrue(_service.TryGetGroup("grp-2", out UnitGroupSnapshot snapshot));
            CollectionAssert.AreEqual(new[] { "u2" }, snapshot.UnitIds.ToArray());
        }

        [Test]
        public void RestoreState_AdvancesOrdinalPastRestoredIds()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", Vector2Int.one);
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p1");

            _service.RestoreState(new[]
            {
                new UnitGroupSnapshot("grp-5", "p1", new[] { "u2" }),
            });

            Assert.IsTrue(_service.TryCreateGroup("p1", new[] { "u1" }, out string groupId, out _));
            Assert.AreNotEqual("grp-5", groupId);
            StringAssert.StartsWith("grp-", groupId);
        }

        [Test]
        public void RestoreReplicated_KeepsMembersNotYetReplicatedLocally()
        {
            // Client-side group sync may arrive before unit replication; the
            // authoritative host table must not be filtered by local state.
            _service.RestoreReplicated(new[]
            {
                new UnitGroupSnapshot("grp-4", "p1", new[] { "net-u1", "net-u2" }),
            });

            Assert.AreEqual("grp-4", _service.GetGroupIdOfUnit("net-u1"));
            Assert.AreEqual("grp-4", _service.GetGroupIdOfUnit("net-u2"));
            Assert.IsTrue(_service.TryGetGroup("grp-4", out UnitGroupSnapshot snapshot));
            Assert.AreEqual(2, snapshot.Count);
        }

        [Test]
        public void CaptureState_RoundTripsThroughRestore()
        {
            _units.Add("u1", Vector2Int.zero);
            _units.Add("u2", Vector2Int.one);
            _ownership.Set("u1", "p1");
            _ownership.Set("u2", "p1");
            _service.TryCreateGroup("p1", new[] { "u1", "u2" }, out string groupId, out _);

            var captured = ((IUnitGroupStateStore)_service).CaptureState();
            var restored = new UnitGroupService(CreateSignalBus(), _units, _ownership);
            try
            {
                ((IUnitGroupStateStore)restored).RestoreState(captured);
                Assert.AreEqual(groupId, restored.GetGroupIdOfUnit("u1"));
                Assert.AreEqual(groupId, restored.GetGroupIdOfUnit("u2"));
            }
            finally
            {
                restored.Dispose();
            }
        }

        // ── Fakes ────────────────────────────────────────────────────

        private static SignalBus CreateSignalBus()
        {
            var container = new DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<UnitGroupChangedSignal>().OptionalSubscriber();
            container.DeclareSignal<UnitDestroyedSignal>().OptionalSubscriber();
            container.DeclareSignal<UnitGarrisonStateChangedSignal>().OptionalSubscriber();
            return container.Resolve<SignalBus>();
        }

        private static UnitMovementTileSnapshot[] Reachable(params Vector2Int[] positions)
            => positions
                .Select(p => new UnitMovementTileSnapshot(p, isReachable: true, cost: 1f))
                .ToArray();

        private sealed class FakeUnitService : IUnitService
        {
            private readonly Dictionary<string, Vector2Int> _positions = new();
            private readonly Dictionary<string, string> _types = new();

            public void Add(string unitId, Vector2Int position, string typeId = "warrior")
            {
                _positions[unitId] = position;
                _types[unitId] = typeId;
            }

            public float GetStamina(string unitId) => 1f;
            public void SetStamina(string unitId, float stamina) { }
            public bool TryGetUnitPosition(string unitId, out Vector2Int position)
                => _positions.TryGetValue(unitId, out position);
            public GameObject GetUnitObject(string unitId) => null;
            public IReadOnlyCollection<string> GetAllUnitIds() => _positions.Keys.ToArray();
            public string GetUnitTypeId(string unitId)
                => _types.TryGetValue(unitId, out string typeId) ? typeId : null;
        }

        private sealed class FakeOwnership : IUnitOwnershipQuery
        {
            private readonly Dictionary<string, string> _owners = new();
            public void Set(string unitId, string ownerId) => _owners[unitId] = ownerId;
            public string GetUnitOwnerId(string unitId)
                => _owners.TryGetValue(unitId, out string ownerId) ? ownerId : null;
        }

        private sealed class FakeMovement : IUnitMovementService
        {
            public readonly List<(string unitId, Vector2Int target)> Calls = new();

            public Task MoveUnitAsync(
                string unitId, Vector2Int targetPosition, CancellationToken token = default)
            {
                Calls.Add((unitId, targetPosition));
                return Task.CompletedTask;
            }
        }

        private sealed class FakeMovementQuery : IUnitMovementQuery
        {
            private readonly Dictionary<string, UnitMovementTileSnapshot[]> _tiles = new();
            public void SetTiles(string unitId, UnitMovementTileSnapshot[] tiles)
                => _tiles[unitId] = tiles;
            public IReadOnlyList<UnitMovementTileSnapshot> GetMovementTiles(string unitId)
                => _tiles.TryGetValue(unitId, out UnitMovementTileSnapshot[] tiles)
                    ? tiles
                    : (IReadOnlyList<UnitMovementTileSnapshot>)System.Array.Empty<UnitMovementTileSnapshot>();
        }

        private sealed class FakeObjectsMap : IObjectsMapService
        {
            private readonly Dictionary<Vector2Int, string> _occupants = new();
            public void SetOccupant(Vector2Int position, string occupantId)
                => _occupants[position] = occupantId;
            public bool IsOccupied(Vector2Int position) => _occupants.ContainsKey(position);
            public bool TryGetOccupant(Vector2Int position, out string occupantId)
                => _occupants.TryGetValue(position, out occupantId);
            public void Register(Vector2Int position, string occupantId)
                => _occupants[position] = occupantId;
            public void Move(Vector2Int from, Vector2Int to)
            {
                if (_occupants.TryGetValue(from, out string occupantId))
                {
                    _occupants.Remove(from);
                    _occupants[to] = occupantId;
                }
            }
            public void Unregister(Vector2Int position) => _occupants.Remove(position);
            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                foreach (var pair in _occupants)
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
                tileTypeId = "grass";
                return true;
            }
            public void SetTileData(Vector2Int position, string tileTypeId) { }
        }
    }
}
