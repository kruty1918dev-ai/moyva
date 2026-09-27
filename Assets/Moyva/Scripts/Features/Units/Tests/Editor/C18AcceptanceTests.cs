using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// C18 — a ready queue entry becomes exactly one canonical unit instance.
    /// Deployment validates owner turn authority, building context, spawn
    /// radius and tile legality; occupied/blocked tiles reject without
    /// consuming the ready entry, so no re-payment is needed.
    /// </summary>
    public sealed class C18AcceptanceTests
    {
        private const string Owner = "p1";
        private static readonly Vector2Int BuildingPos = new Vector2Int(5, 5);

        private UnitRecruitmentQueueStateMachine _queue;
        private C15Fakes.Turns _turns;
        private C15Fakes.Clock _clock;
        private C14Fakes.Units _units;
        private C15Fakes.Ownership _ownership;
        private C18Fakes.Factory _factory;
        private C18Fakes.Placement _placement;
        private C18Fakes.Snapshots _snapshots;
        private C18Fakes.Lifecycle _lifecycle;
        private UnitRecruitmentDeploymentService _service;

        [SetUp]
        public void SetUp()
        {
            _queue = new UnitRecruitmentQueueStateMachine();
            _turns = new C15Fakes.Turns();
            _clock = new C15Fakes.Clock();
            _units = new C14Fakes.Units();
            _ownership = new C15Fakes.Ownership();
            _factory = new C18Fakes.Factory(_units, _ownership);
            _placement = new C18Fakes.Placement();
            _snapshots = new C18Fakes.Snapshots();
            _lifecycle = new C18Fakes.Lifecycle { Operational = true };

            _snapshots.Placements.Add(
                new ConstructionSavedPlacement(BuildingPos, "barracks", Owner));

            var resolver = new UnitRecruitmentBuildingContextResolver(
                new C18Fakes.UnitConfigs(),
                new C18Fakes.BuildingRegistry(),
                _snapshots,
                _lifecycle);

            _service = new UnitRecruitmentDeploymentService(
                _queue, resolver, _turns, _clock, signalBus: null,
                _factory, _units, _ownership, _placement);
        }

        private long EnqueueReady(int trainingTurns = 1)
        {
            var item = _queue.EnqueueValidated(
                Owner, BuildingPos, "barracks", "worker", trainingTurns, 1);
            for (long turn = 2; turn <= 1 + trainingTurns; turn++)
                _queue.AdvanceOwnerTurn(Owner, turn);
            Assert.IsTrue(_queue.TryGetReady(Owner, BuildingPos, item.QueueId, out _),
                "Fixture: entry must be ready.");
            return item.QueueId;
        }

        [Test]
        public void Ready_Deploys_CanonicalUnit()
        {
            long id = EnqueueReady();
            var target = new Vector2Int(6, 5);

            Assert.IsTrue(_service.TryDeployReady(
                Owner, BuildingPos, id, target, out string unitId, out string reason), reason);

            Assert.IsFalse(string.IsNullOrWhiteSpace(unitId));
            Assert.IsTrue(_units.TryGetUnitPosition(unitId, out var pos));
            Assert.AreEqual(target, pos);
            Assert.AreEqual(Owner, _ownership.GetUnitOwnerId(unitId),
                "Deployed unit carries the recruiting owner's id.");
            Assert.AreEqual("worker", _units.GetUnitTypeId(unitId));
        }

        [Test]
        public void EntryConsumed_ExactlyOnce()
        {
            long id = EnqueueReady();
            var target = new Vector2Int(6, 5);
            Assert.IsTrue(_service.TryDeployReady(Owner, BuildingPos, id, target, out _, out _));

            Assert.IsFalse(_service.TryDeployReady(Owner, BuildingPos, id, target, out _, out _),
                "The consumed ready entry cannot deploy a second unit.");
        }

        [Test]
        public void Unready_Rejected()
        {
            var pending = _queue.EnqueueValidated(Owner, BuildingPos, "barracks", "worker", 5, 1);
            Assert.IsFalse(_service.TryDeployReady(
                Owner, BuildingPos, pending.QueueId, new Vector2Int(6, 5), out _, out var reason));
            Assert.IsNotEmpty(reason);
        }

        [Test]
        public void OccupiedTile_Rejected_ReadyKept()
        {
            long id = EnqueueReady();
            var target = new Vector2Int(6, 5);
            _placement.Blocked.Add(target);

            Assert.IsFalse(_service.TryDeployReady(
                Owner, BuildingPos, id, target, out _, out var reason));
            Assert.IsNotEmpty(reason);
            Assert.IsTrue(_queue.TryGetReady(Owner, BuildingPos, id, out _),
                "Rejected deploy keeps the ready entry — no second payment, no loss.");
        }

        [Test]
        public void OutsideRadius_Rejected()
        {
            long id = EnqueueReady();
            var far = new Vector2Int(BuildingPos.x + 10, BuildingPos.y);
            Assert.IsFalse(_service.TryDeployReady(
                Owner, BuildingPos, id, far, out _, out var reason));
            StringAssert.Contains("radius", reason);
            Assert.IsTrue(_queue.TryGetReady(Owner, BuildingPos, id, out _));
        }

        [Test]
        public void BuildingCenter_Rejected()
        {
            long id = EnqueueReady();
            Assert.IsFalse(_service.TryDeployReady(
                Owner, BuildingPos, id, BuildingPos, out _, out _),
                "Ring distance 0 is not a valid spawn tile.");
        }

        [Test]
        public void WrongOwner_CannotDeploy()
        {
            long id = EnqueueReady();
            _turns.ActiveOwnerId = "p2";
            Assert.IsFalse(_service.TryDeployReady(
                "p2", BuildingPos, id, new Vector2Int(6, 5), out _, out _));
        }

        [Test]
        public void NotActiveTurn_CannotDeploy()
        {
            long id = EnqueueReady();
            _turns.Phase = Kruty1918.Moyva.Turns.API.TurnPhase.Resolving;
            Assert.IsFalse(_service.TryDeployReady(
                Owner, BuildingPos, id, new Vector2Int(6, 5), out _, out _));
        }

        [Test]
        public void Realtime_BypassesTurnGate()
        {
            _clock.IsRealtime = true;
            _turns.ActiveOwnerId = "p2"; // not p1's turn — irrelevant in realtime
            long id = EnqueueReady();
            Assert.IsTrue(_service.TryDeployReady(
                Owner, BuildingPos, id, new Vector2Int(6, 5), out _, out var reason), reason);
        }

        [Test]
        public void LostSourceBuilding_Rejected_ReadyKept()
        {
            long id = EnqueueReady();
            _snapshots.Placements.Clear(); // building demolished
            Assert.IsFalse(_service.TryDeployReady(
                Owner, BuildingPos, id, new Vector2Int(6, 5), out _, out var reason));
            StringAssert.Contains("no longer exists", reason);
            Assert.IsTrue(_queue.TryGetReady(Owner, BuildingPos, id, out _));
        }

        [Test]
        public void NotOperationalBuilding_Rejected()
        {
            long id = EnqueueReady();
            _lifecycle.Operational = false;
            Assert.IsFalse(_service.TryDeployReady(
                Owner, BuildingPos, id, new Vector2Int(6, 5), out _, out var reason));
            StringAssert.Contains("operational", reason);
        }

        [Test]
        public void DeploymentTiles_ExposeValidityPerCandidate()
        {
            long id = EnqueueReady();
            _placement.Blocked.Add(new Vector2Int(6, 5));
            var tiles = _service.GetDeploymentTiles(Owner, BuildingPos, id);
            Assert.IsNotEmpty(tiles);
            int invalid = 0;
            foreach (var tile in tiles)
            {
                if (!tile.IsValid)
                {
                    invalid++;
                    Assert.IsNotEmpty(tile.Reason);
                }
            }
            Assert.Greater(invalid, 0, "The blocked tile reports invalid with a reason.");
        }

        [Test]
        public void DeployedId_IsStableForQueueEntry()
        {
            long id = EnqueueReady();
            var expected = UnitRecruitmentDeploymentService.BuildRecruitmentUnitId(id, "worker");
            Assert.IsTrue(_service.TryDeployReady(
                Owner, BuildingPos, id, new Vector2Int(6, 5), out string unitId, out _));
            Assert.AreEqual(expected, unitId,
                "Unit id derives from the queue entry — reissue/redeploy reconciles instead of duplicating.");
        }
    }

    internal static class C18Fakes
    {
        internal sealed class UnitConfigs : IUnitClassConfig
        {
            public UnitClassConfig GetConfig(string typeId)
                => typeId == "worker"
                    ? new UnitClassConfig { TypeId = typeId }
                    : null;
        }

        internal sealed class BuildingRegistry : IBuildingRegistry
        {
            private readonly BuildingDefinition _barracks = new BuildingDefinition
            {
                Id = "barracks",
                Modules =
                {
                    new UnitRecruitmentBuildingModule { SpawnRadius = 2, QueueCapacity = 3 },
                },
            };

            public BuildingDefinition[] GetAll() => new[] { _barracks };
            public BuildingDefinition GetById(string id)
                => id == "barracks" ? _barracks : null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => Array.Empty<BuildingDefinition>();
            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
                => null;
        }

        internal sealed class Snapshots : IConstructionSaveSnapshotSource
        {
            public readonly List<ConstructionSavedPlacement> Placements = new();
            public IReadOnlyList<ConstructionSavedPlacement> GetSavedPlacements() => Placements;
        }

        internal sealed class Lifecycle : IConstructionLifecycle
        {
            public bool Operational = true;
            public bool IsOperational(Vector2Int position) => Operational;
            public bool TryGetProgress(Vector2Int position, out int completedTurns, out int requiredTurns)
            {
                completedTurns = requiredTurns = 1;
                return true;
            }
        }

        internal sealed class Placement : IUnitPlacementValidator
        {
            public readonly HashSet<Vector2Int> Blocked = new();
            public bool IsTerrainAllowed(Vector2Int position, out string reason)
            {
                reason = null;
                return true;
            }
            public bool CanDeployUnit(string unitTypeId, Vector2Int position, out string reason)
            {
                if (Blocked.Contains(position))
                {
                    reason = "occupied";
                    return false;
                }
                reason = null;
                return true;
            }
        }

        internal sealed class Factory : IUnitFactory
        {
            private readonly C14Fakes.Units _units;
            private readonly C15Fakes.Ownership _ownership;
            public Factory(C14Fakes.Units units, C15Fakes.Ownership ownership)
            {
                _units = units;
                _ownership = ownership;
            }
            public string CreateUnit(string typeId, Vector2Int gridPosition)
                => CreateUnitWithId($"auto_{Guid.NewGuid():N}", typeId, gridPosition, null);
            public string CreateUnit(string typeId, Vector2Int gridPosition, string ownerId)
                => CreateUnitWithId($"auto_{Guid.NewGuid():N}", typeId, gridPosition, ownerId);
            public string CreateUnitWithId(string forcedUnitId, string typeId,
                Vector2Int gridPosition, string ownerId)
            {
                _units.Spawn(forcedUnitId, gridPosition, 1f);
                _ownership.Owners[forcedUnitId] = ownerId;
                return forcedUnitId;
            }
        }
    }
}
