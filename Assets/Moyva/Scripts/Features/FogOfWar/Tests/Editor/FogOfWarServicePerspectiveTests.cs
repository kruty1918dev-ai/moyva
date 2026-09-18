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
    public sealed class FogOfWarServicePerspectiveTests
    {
        private const string OwnerA = "owner-a";
        private const string OwnerB = "owner-b";
        private const int Width = 24;
        private const int Height = 24;

        private DiContainer _container;
        private SignalBus _signals;
        private FogOfWarService _service;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<UnitCreatedSignal>();
            _container.DeclareSignal<UnitMovedSignal>();
            _container.DeclareSignal<UnitDestroyedSignal>();
            _container.DeclareSignal<UnitGarrisonStateChangedSignal>();
            _container.DeclareSignal<BuildingDemolishedSignal>();
            _container.DeclareSignal<BuildingOwnershipTransferredSignal>();
            _container.DeclareSignal<WorldGeneratedDataSignal>();

            _signals = _container.Resolve<SignalBus>();
            _service = new FogOfWarService(
                new FakeVisibilityResolver(),
                null,
                null,
                null,
                _signals,
                null,
                null);
            _service.Initialize();
            _service.Initialize(Width, Height);
        }

        [TearDown]
        public void TearDown() => _service.Dispose();

        // ── Local perspective gating ────────────────────────────────────────

        [Test]
        public void LocalOwnerUnit_ContributesToLocalGrid()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);

            FireUnitCreated("ua", OwnerA, new Vector2Int(4, 4), visionRange: 2);

            Assert.IsTrue(_service.IsVisible(new Vector2Int(4, 4)));
            Assert.IsTrue(_service.IsVisible(OwnerA, new Vector2Int(4, 4)));
        }

        [Test]
        public void OtherOwnerUnit_DoesNotContributeToLocalGrid_ButHasOwnerVision()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(18, 18);

            FireUnitCreated("ub", OwnerB, cell, visionRange: 2);

            Assert.IsFalse(_service.IsVisible(cell), "enemy source leaked into local grid");
            Assert.AreEqual(FogStateType.Unexplored, _service.GetFogState(cell));
            Assert.IsTrue(_service.IsVisible(OwnerB, cell), "owner grid must still see own unit");
        }

        [Test]
        public void UnresolvedLocalPerspective_AllSourcesContribute()
        {
            var cell = new Vector2Int(18, 18);

            FireUnitCreated("ub", OwnerB, cell, visionRange: 2);

            Assert.IsTrue(_service.IsVisible(cell));
        }

        [Test]
        public void UnownedSource_AlwaysContributesToLocalGrid()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(10, 10);

            _service.RegisterUnit("neutral-scout", cell, 2);

            Assert.IsTrue(_service.IsVisible(cell));
        }

        [Test]
        public void SwitchPerspective_RebuildsLocalGrid_KeepsExploredMemory()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cellA = new Vector2Int(4, 4);
            var cellB = new Vector2Int(18, 18);
            FireUnitCreated("ua", OwnerA, cellA, visionRange: 2);
            FireUnitCreated("ub", OwnerB, cellB, visionRange: 2);

            _service.SetLocalPerspectiveOwnerId(OwnerB);

            Assert.IsTrue(_service.IsVisible(cellB), "new owner's source must appear locally");
            Assert.IsFalse(_service.IsVisible(cellA), "old owner's source must leave local vision");
            Assert.IsTrue(_service.IsExplored(cellA), "explored memory survives perspective switch");
            Assert.IsTrue(_service.IsVisible(OwnerA, cellA), "owner grid is unaffected");
        }

        // ── Per-owner grid transitions ──────────────────────────────────────

        [Test]
        public void CellsBecameVisible_FiresForOwningPlayer()
        {
            var events = new List<(string owner, List<Vector2Int> cells)>();
            _service.CellsBecameVisible += (owner, cells)
                => events.Add((owner, cells.ToList()));

            _service.RegisterUnit(OwnerA, "ua", new Vector2Int(3, 3), 1);
            _service.RegisterUnit(OwnerB, "ub", new Vector2Int(20, 20), 1);

            Assert.AreEqual(2, events.Count);
            Assert.IsTrue(events.Any(e => e.owner == OwnerA && e.cells.Contains(new Vector2Int(3, 3))));
            Assert.IsTrue(events.Any(e => e.owner == OwnerB && e.cells.Contains(new Vector2Int(20, 20))));
        }

        [Test]
        public void OwnerUnitMoved_VisionFollows_OldCellBecomesExplored()
        {
            var from = new Vector2Int(4, 4);
            var to = new Vector2Int(4, 10);
            _service.RegisterUnit(OwnerA, "ua", from, 1);

            _service.UpdateUnitPosition(OwnerA, "ua", to);

            Assert.IsTrue(_service.IsVisible(OwnerA, to));
            Assert.IsFalse(_service.IsVisible(OwnerA, from));
            Assert.IsTrue(_service.IsExplored(OwnerA, from));
        }

        [Test]
        public void OwnerUnitUnregistered_VisionRemoved_ExploredKept()
        {
            var cell = new Vector2Int(6, 6);
            _service.RegisterUnit(OwnerA, "ua", cell, 1);

            _service.UnregisterUnit(OwnerA, "ua");

            Assert.IsFalse(_service.IsVisible(OwnerA, cell));
            Assert.IsTrue(_service.IsExplored(OwnerA, cell));
        }

        // ── Ownership transfer of fixed vision areas (buildings) ────────────

        [Test]
        public void VisionAreaTransfer_MovesVisionBetweenOwnerGrids()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(12, 12);
            const string areaId = "building:12:12";
            _service.RegisterOwnedFixedVisionArea(OwnerA, areaId, cell, 2, FogRevealShape.Square);
            Assert.IsTrue(_service.IsVisible(cell), "precondition: local owner sees own building");

            _service.TransferFixedVisionAreaOwner(areaId, OwnerA, OwnerB);

            Assert.IsTrue(_service.IsVisible(OwnerB, cell), "new owner gains vision");
            Assert.IsFalse(_service.IsVisible(OwnerA, cell), "old owner loses vision");
            Assert.IsTrue(_service.IsExplored(OwnerA, cell), "old owner keeps explored memory");
            Assert.IsFalse(_service.IsVisible(cell),
                "local grid drops the area after it transfers away");
        }

        [Test]
        public void OwnedArea_PerspectiveSwitch_DoesNotLeakVision()
        {
            // Simulates host migration: a building registered while the local
            // owner was A must stop contributing after the perspective moves
            // to B, and must contribute again if it ever switches back.
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(12, 12);
            const string areaId = "building:12:12";
            _service.RegisterOwnedFixedVisionArea(OwnerA, areaId, cell, 2, FogRevealShape.Square);
            Assert.IsTrue(_service.IsVisible(cell));

            _service.SetLocalPerspectiveOwnerId(OwnerB);

            Assert.IsFalse(_service.IsVisible(cell),
                "A-owned building vision must not leak into B's local grid");
            Assert.IsTrue(_service.IsExplored(cell), "explored memory persists");

            _service.SetLocalPerspectiveOwnerId(OwnerA);

            Assert.IsTrue(_service.IsVisible(cell),
                "switching back restores the owned building's vision");
        }

        [Test]
        public void OwnedArea_NonLocalOwner_NeverContributesLocally()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(12, 12);

            _service.RegisterOwnedFixedVisionArea(OwnerB, "building:12:12", cell, 2, FogRevealShape.Square);

            Assert.IsFalse(_service.IsVisible(cell));
            Assert.IsTrue(_service.IsVisible(OwnerB, cell));
        }

        // ── Helpers & fakes ─────────────────────────────────────────────────

        private void FireUnitCreated(string unitId, string ownerId, Vector2Int position, int visionRange)
            => _signals.Fire(new UnitCreatedSignal
            {
                UnitId = unitId,
                UnitTypeId = "warrior",
                Position = position,
                VisionRange = visionRange,
                OwnerId = ownerId,
            });

        private sealed class FakeVisibilityResolver : IFogVisibilityResolver
        {
            public void SetHeightMap(float[,] heightMap) { }

            public IReadOnlyList<Vector2Int> ComputeVisibleTiles(
                Vector2Int origin, int visionRange, int mapWidth, int mapHeight,
                FogVisionModifiers observerModifiers = default)
                => FogRevealShapeTileCalculator.ComputePixelCircleTiles(
                    origin, visionRange, mapWidth, mapHeight);

            public IReadOnlyList<FogTileVisibility> ComputeVisibility(
                Vector2Int origin, int visionRange, int mapWidth, int mapHeight,
                FogVisionModifiers observerModifiers = default)
                => ComputeVisibleTiles(origin, visionRange, mapWidth, mapHeight, observerModifiers)
                    .Select(t => new FogTileVisibility(t, 1f))
                    .ToList();
        }
    }
}
