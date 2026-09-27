using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// C27: group merge/disband keeps members and their ownership — the
    /// canonical group command path validates members (existence, owner,
    /// single membership), moves via the movement service (never bypassing
    /// turn authority), and restores membership without duplication.
    /// </summary>
    public sealed class C27AcceptanceTests
    {
        private const string Owner = "p1";

        private SignalBus _bus;
        private C14Fakes.Units _units;
        private C15Fakes.Ownership _ownership;
        private C15Fakes.Turns _turns;
        private C15Fakes.Clock _clock;
        private C27Fakes.Movement _movement;
        private C27Fakes.MovementQuery _movementQuery;
        private UnitGroupService _service;

        [SetUp]
        public void SetUp()
        {
            var container = new DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<UnitGroupChangedSignal>().OptionalSubscriber();
            container.DeclareSignal<UnitDestroyedSignal>().OptionalSubscriber();
            container.DeclareSignal<UnitGarrisonStateChangedSignal>().OptionalSubscriber();
            _bus = container.Resolve<SignalBus>();

            _units = new C14Fakes.Units();
            _ownership = new C15Fakes.Ownership();
            _turns = new C15Fakes.Turns();
            _clock = new C15Fakes.Clock();
            _movement = new C27Fakes.Movement();
            _movementQuery = new C27Fakes.MovementQuery(
                new[] { new Vector2Int(5, 5), new Vector2Int(9, 9) });
            _service = new UnitGroupService(
                _bus, _units, _ownership, _movement,
                movementQuery: _movementQuery, objectsMap: null, grid: null,
                turns: _turns, progressClock: _clock);
            _service.Initialize();
        }

        [TearDown]
        public void TearDown() => _service?.Dispose();

        private void Spawn(string id, string owner = Owner, float x = 1f)
        {
            _units.Spawn(id, new Vector2Int((int)x, 1), 3f);
            _ownership.Owners[id] = owner;
        }

        // ── Membership ───────────────────────────────────────────────

        [Test]
        public void Create_PreservesMembersAndOwner()
        {
            Spawn("u1"); Spawn("u2");
            Assert.IsTrue(_service.TryCreateGroup(Owner, new[] { "u1", "u2" },
                out string groupId, out string reason), reason);
            Assert.IsTrue(_service.TryGetGroup(groupId, out var snapshot));
            Assert.AreEqual(Owner, snapshot.OwnerId);
            CollectionAssert.AreEquivalent(new[] { "u1", "u2" }, snapshot.UnitIds);
        }

        [Test]
        public void Create_DuplicateMember_Collapses()
        {
            Spawn("u1");
            Assert.IsTrue(_service.TryCreateGroup(Owner, new[] { "u1", "u1" },
                out string groupId, out _));
            Assert.IsTrue(_service.TryGetGroup(groupId, out var snapshot));
            Assert.AreEqual(1, snapshot.UnitIds.Count,
                "A duplicated member id is one membership, not two.");
        }

        [Test]
        public void Create_ForeignUnit_Rejected()
        {
            Spawn("u1"); Spawn("enemy", owner: "p2");
            Assert.IsFalse(_service.TryCreateGroup(Owner, new[] { "u1", "enemy" },
                out _, out var reason));
            Assert.IsNotEmpty(reason);
        }

        [Test]
        public void Create_UnknownUnit_Rejected()
        {
            Spawn("u1");
            Assert.IsFalse(_service.TryCreateGroup(Owner, new[] { "u1", "ghost" },
                out _, out _));
        }

        [Test]
        public void Create_UnitAlreadyGrouped_Rejected()
        {
            Spawn("u1"); Spawn("u2");
            _service.TryCreateGroup(Owner, new[] { "u1" }, out _, out _);
            Assert.IsFalse(_service.TryCreateGroup(Owner, new[] { "u1", "u2" },
                out _, out var reason));
            StringAssert.Contains("already belongs", reason);
        }

        [Test]
        public void Remove_LastMember_Disbands()
        {
            Spawn("u1");
            _service.TryCreateGroup(Owner, new[] { "u1" }, out string groupId, out _);
            Assert.IsTrue(_service.TryRemoveUnit(Owner, groupId, "u1", out _));
            Assert.IsFalse(_service.TryGetGroup(groupId, out _),
                "An empty group disbands instead of lingering.");
        }

        [Test]
        public void DestroyedMember_LeavesGroup()
        {
            Spawn("u1"); Spawn("u2");
            _service.TryCreateGroup(Owner, new[] { "u1", "u2" }, out string groupId, out _);
            _bus.Fire(new UnitDestroyedSignal { UnitId = "u1" });
            Assert.IsTrue(_service.TryGetGroup(groupId, out var snapshot));
            CollectionAssert.AreEquivalent(new[] { "u2" }, snapshot.UnitIds);
            Assert.AreEqual(string.Empty, _service.GetGroupIdOfUnit("u1"));
        }

        [Test]
        public void Destroyed_LastMember_Disbands()
        {
            Spawn("u1");
            _service.TryCreateGroup(Owner, new[] { "u1" }, out string groupId, out _);
            _bus.Fire(new UnitDestroyedSignal { UnitId = "u1" });
            Assert.IsFalse(_service.TryGetGroup(groupId, out _));
        }

        [Test]
        public void Disband_OnlyByOwner()
        {
            Spawn("u1");
            _service.TryCreateGroup(Owner, new[] { "u1" }, out string groupId, out _);
            Assert.IsFalse(_service.TryDisbandGroup("p2", groupId, out _));
            Assert.IsTrue(_service.TryGetGroup(groupId, out _),
                "Foreign disband attempt must not dissolve the group.");
            Assert.IsTrue(_service.TryDisbandGroup(Owner, groupId, out _));
        }

        // ── Movement ────────────────────────────────────────────────

        [Test]
        public void MoveGroup_DelegatesThroughMovementService()
        {
            Spawn("u1"); Spawn("u2");
            _service.TryCreateGroup(Owner, new[] { "u1", "u2" }, out string groupId, out _);


            Assert.IsTrue(_service.TryMoveGroup(Owner, groupId,
                new Vector2Int(9, 9), out var reason), reason);
            Assert.AreEqual(2, _movement.Calls,
                "Each member moves via the movement service — authority is not bypassed.");
        }

        [Test]
        public void MoveGroup_NotYourTurn_Rejected()
        {
            Spawn("u1");
            _service.TryCreateGroup(Owner, new[] { "u1" }, out string groupId, out _);
            _turns.ActiveOwnerId = "p2";

            Assert.IsFalse(_service.TryMoveGroup(Owner, groupId,
                new Vector2Int(5, 5), out _));
            Assert.AreEqual(0, _movement.Calls,
                "A group order never bypasses turn authority.");
        }

        [Test]
        public void MoveGroup_Realtime_BypassesTurnGate()
        {
            Spawn("u1");
            _service.TryCreateGroup(Owner, new[] { "u1" }, out string groupId, out _);
            _clock.IsRealtime = true;
            _turns.ActiveOwnerId = "p2";


            Assert.IsTrue(_service.TryMoveGroup(Owner, groupId,
                new Vector2Int(5, 5), out var reason), reason);
            Assert.AreEqual(1, _movement.Calls);
        }

        // ── Persistence ─────────────────────────────────────────────

        [Test]
        public void Restore_DropsDeadMembers_KeepOwnership()
        {
            Spawn("u1"); Spawn("u2");
            _service.TryCreateGroup(Owner, new[] { "u1", "u2" }, out string groupId, out _);
            var saved = _service.CaptureState();

            // Restart: only u1 exists in the new world.
            var unitsB = new C14Fakes.Units();
            unitsB.Spawn("u1", new Vector2Int(1, 1), 3f);
            var serviceB = new UnitGroupService(
                _bus, unitsB, _ownership, _movement,
                null, null, null, _turns, _clock);
            serviceB.RestoreState(saved);

            Assert.IsTrue(serviceB.TryGetGroup(groupId, out var snapshot));
            CollectionAssert.AreEquivalent(new[] { "u1" }, snapshot.UnitIds,
                "Missing units are pruned on restore.");
            Assert.AreEqual(Owner, snapshot.OwnerId);
            serviceB.Dispose();
        }

        [Test]
        public void Restore_NewGroups_KeepIdsUnique()
        {
            Spawn("u1"); Spawn("u2");
            _service.TryCreateGroup(Owner, new[] { "u1" }, out string groupId, out _);
            var saved = _service.CaptureState();
            _service.RestoreState(saved);
            Assert.IsTrue(_service.TryCreateGroup(Owner, new[] { "u2" },
                out string newId, out _));
            Assert.AreNotEqual(groupId, newId,
                "Restored ordinals advance — ids never recycle.");
        }

        [Test]
        public void RestoreReplicated_TrustsSnapshotVerbatim()
        {
            var replicated = new List<UnitGroupSnapshot>
            {
                new UnitGroupSnapshot("grp-9", Owner, new[] { "u1", "ghost" }),
            };
            _service.RestoreReplicated(replicated);
            Assert.IsTrue(_service.TryGetGroup("grp-9", out var snapshot));
            CollectionAssert.AreEquivalent(new[] { "u1", "ghost" }, snapshot.UnitIds,
                "Replication keeps members the host reports, even lagging local state.");
        }
    }

    internal static class C27Fakes
    {
        internal sealed class Movement : IUnitMovementService
        {
            public int Calls;
            public IUnitMovementQuery Query;

            public Task MoveUnitAsync(string unitId, Vector2Int targetPosition,
                CancellationToken token = default)
            {
                Calls++;
                return Task.CompletedTask;
            }
        }

        internal sealed class MovementQuery : IUnitMovementQuery
        {
            private readonly Vector2Int[] _tiles;
            public MovementQuery(IEnumerable<Vector2Int> tiles)
            {
                var list = new List<Vector2Int>(tiles);
                _tiles = list.ToArray();
            }
            public IReadOnlyList<UnitMovementTileSnapshot> GetMovementTiles(string unitId)
            {
                var result = new List<UnitMovementTileSnapshot>();
                foreach (var tile in _tiles)
                    result.Add(new UnitMovementTileSnapshot(tile, isReachable: true, cost: 1f));
                return result;
            }
        }
    }
}
