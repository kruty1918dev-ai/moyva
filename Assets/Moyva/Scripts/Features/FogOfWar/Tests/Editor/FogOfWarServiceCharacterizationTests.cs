using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.SaveSystem;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    /// <summary>
    /// Characterization tests for <see cref="FogOfWarService"/> before the
    /// single-source-of-truth refactor. They pin the observable contract for
    /// every consumer group: owners, local perspective, reveals, unit vision,
    /// construction, combat reads, save/load, multiplayer replication reads
    /// and the simulation (training/bot) binding path.
    ///
    /// The "Divergence" region intentionally pins today's dual-grid answers
    /// where the local grid and the owner grid disagree. Those asserts are
    /// flipped to the unified contract by the refactor commit.
    /// </summary>
    [TestFixture]
    public sealed class FogOfWarServiceCharacterizationTests
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
            DeclareGameplaySignals(_container);

            _signals = _container.Resolve<SignalBus>();
            _service = CreateService(_signals);
            _service.Initialize();
            _service.Initialize(Width, Height);
        }

        [TearDown]
        public void TearDown() => _service.Dispose();

        // ── Owners ─────────────────────────────────────────────────────────

        [Test]
        public void OwnerGrids_IndependentPerOwner()
        {
            var cellA = new Vector2Int(4, 4);
            var cellB = new Vector2Int(18, 18);

            _service.RegisterUnit(OwnerA, "ua", cellA, 2);
            _service.RegisterUnit(OwnerB, "ub", cellB, 2);

            Assert.IsTrue(_service.IsVisible(OwnerA, cellA));
            Assert.IsFalse(_service.IsVisible(OwnerA, cellB));
            Assert.IsTrue(_service.IsVisible(OwnerB, cellB));
            Assert.IsFalse(_service.IsVisible(OwnerB, cellA));
            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(OwnerA, cellA));
            Assert.AreEqual(FogStateType.Unexplored, _service.GetFogState(OwnerA, cellB));
        }

        [Test]
        public void OwnerUnregister_RemovesOnlyThatOwnersVision()
        {
            var cell = new Vector2Int(8, 8);
            _service.RegisterUnit(OwnerA, "ua", cell, 2);
            _service.RegisterUnit(OwnerB, "ub", cell, 2);

            _service.UnregisterUnit(OwnerA, "ua");

            Assert.IsFalse(_service.IsVisible(OwnerA, cell));
            Assert.IsTrue(_service.IsExplored(OwnerA, cell));
            Assert.IsTrue(_service.IsVisible(OwnerB, cell), "other owner vision untouched");
        }

        // ── Local player alias ─────────────────────────────────────────────

        [Test]
        public void LocalApi_ReflectsLocalOwnerSources()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(5, 5);
            FireUnitCreated("ua", OwnerA, cell, visionRange: 2);

            Assert.IsTrue(_service.IsVisible(cell));
            Assert.AreEqual(
                _service.IsVisible(OwnerA, cell),
                _service.IsVisible(cell),
                "local read must agree with the local owner's grid");
        }

        [Test]
        public void LocalApi_IncludesUnownedSources_WhileOwnerScopeDoesNot()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(10, 10);

            _service.RegisterUnit("neutral-scout", cell, 2);

            Assert.IsTrue(_service.IsVisible(cell), "unowned sources always contribute locally");
            Assert.IsFalse(_service.IsVisible(OwnerA, cell),
                "unowned sources do not belong to any owner grid");
        }

        [Test]
        public void LocalApi_UnresolvedPerspective_SeesAllOwners()
        {
            var cell = new Vector2Int(18, 18);
            FireUnitCreated("ub", OwnerB, cell, visionRange: 2);

            Assert.IsTrue(_service.IsVisible(cell));
        }

        [Test]
        public void PerspectiveSwitch_KeepsLocalExploredMemory()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cellA = new Vector2Int(4, 4);
            FireUnitCreated("ua", OwnerA, cellA, visionRange: 2);
            Assert.IsTrue(_service.IsExplored(cellA));

            _service.SetLocalPerspectiveOwnerId(OwnerB);

            Assert.IsTrue(_service.IsExplored(cellA),
                "local explored memory survives a perspective switch");
            Assert.IsFalse(_service.IsVisible(cellA));
        }

        // ── Reveal / hide ──────────────────────────────────────────────────

        [Test]
        public void RevealArea_KeepVisible_StaysVisible_UntilAreaRemoved()
        {
            var center = new Vector2Int(8, 8);

            _service.RevealArea(center, 2, FogRevealShape.Square, keepVisible: true, "reveal-x");

            Assert.IsTrue(_service.IsVisible(center));
            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(center));

            _service.UnregisterUnit("reveal-x");

            Assert.IsFalse(_service.IsVisible(center));
            Assert.IsTrue(_service.IsExplored(center), "seen cells stay explored");
        }

        [Test]
        public void RevealArea_ExploredOnly_MarksExploredWithoutVision()
        {
            var center = new Vector2Int(8, 8);

            _service.RevealArea(center, 2, FogRevealShape.Square, keepVisible: false);

            Assert.IsFalse(_service.IsVisible(center));
            Assert.IsTrue(_service.IsExplored(center));
            Assert.AreEqual(FogStateType.Explored, _service.GetFogState(center));
        }

        [Test]
        public void RevealArea_BeforeInitialize_IsAppliedOnInit()
        {
            var service = CreateService(_signals);
            service.Initialize();
            var center = new Vector2Int(6, 6);

            service.RevealArea(center, 2, FogRevealShape.Square, keepVisible: true, "pre-init");
            Assert.IsFalse(service.IsVisible(center), "no state before map init");

            service.Initialize(Width, Height);

            Assert.IsTrue(service.IsVisible(center));
            service.Dispose();
        }

        // ── Unit vision ────────────────────────────────────────────────────

        [Test]
        public void UnitCreatedSignal_FeedsLocalAndOwnerGrids()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(4, 4);

            FireUnitCreated("ua", OwnerA, cell, visionRange: 2);

            Assert.IsTrue(_service.IsVisible(cell), "local scope");
            Assert.IsTrue(_service.IsVisible(OwnerA, cell), "owner scope");
            Assert.IsFalse(_service.IsVisible(OwnerB, cell), "other owner");
        }

        [Test]
        public void UnitMovedSignal_MovesVision_OldCellStaysExplored()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var from = new Vector2Int(4, 4);
            var to = new Vector2Int(4, 10);
            FireUnitCreated("ua", OwnerA, from, visionRange: 1);

            _signals.Fire(new UnitMovedSignal
            {
                UnitId = "ua",
                NewPosition = to,
                SourceFactionId = OwnerA,
            });

            Assert.IsTrue(_service.IsVisible(to));
            Assert.IsFalse(_service.IsVisible(from));
            Assert.IsTrue(_service.IsExplored(from));
            Assert.IsTrue(_service.IsVisible(OwnerA, to));
            Assert.IsFalse(_service.IsVisible(OwnerA, from));
        }

        [Test]
        public void UnitDestroyedSignal_RemovesVision_KeepsExplored()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(4, 4);
            FireUnitCreated("ua", OwnerA, cell, visionRange: 1);

            _signals.Fire(new UnitDestroyedSignal { UnitId = "ua" });

            Assert.IsFalse(_service.IsVisible(cell));
            Assert.IsTrue(_service.IsExplored(cell));
            Assert.IsFalse(_service.IsVisible(OwnerA, cell));
            Assert.IsTrue(_service.IsExplored(OwnerA, cell));
        }

        [Test]
        public void GarrisonToggle_HidesThenRestoresUnitVision()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var building = new Vector2Int(6, 6);
            var unitCell = new Vector2Int(6, 7);
            FireUnitCreated("ua", OwnerA, unitCell, visionRange: 1);

            _signals.Fire(new UnitGarrisonStateChangedSignal
            {
                UnitId = "ua",
                IsGarrisoned = true,
                BuildingPosition = building,
                OwnerId = OwnerA,
            });

            Assert.IsFalse(_service.IsVisible(unitCell), "garrisoned unit stops seeing");
            Assert.IsFalse(_service.IsVisible(OwnerA, unitCell));

            _signals.Fire(new UnitGarrisonStateChangedSignal
            {
                UnitId = "ua",
                IsGarrisoned = false,
                UnitPosition = unitCell,
                VisionRange = 1,
                OwnerId = OwnerA,
            });

            Assert.IsTrue(_service.IsVisible(unitCell));
            Assert.IsTrue(_service.IsVisible(OwnerA, unitCell));
        }

        [Test]
        public void UpdateUnitVisionRange_ChangesOwnerScopeCoverage()
        {
            var origin = new Vector2Int(6, 6);
            var far = new Vector2Int(6, 10);
            _service.RegisterUnit(OwnerA, "ua", origin, 1);
            Assert.IsFalse(_service.IsVisible(OwnerA, far));

            _service.UpdateUnitVisionRange(OwnerA, "ua", 5);

            Assert.IsTrue(_service.IsVisible(OwnerA, far));
        }

        // ── Construction visibility ────────────────────────────────────────

        [Test]
        public void OwnedFixedVisionArea_ContributesWhileOwnerIsLocal()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(12, 12);

            _service.RegisterOwnedFixedVisionArea(
                OwnerA, "building:12:12", cell, 2, FogRevealShape.Square);

            Assert.IsTrue(_service.IsVisible(cell), "local owner building seen locally");
            Assert.IsTrue(_service.IsVisible(OwnerA, cell));
            Assert.IsFalse(_service.IsVisible(OwnerB, cell));
        }

        [Test]
        public void BuildingDemolishedSignal_RemovesVisionEverywhere()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(12, 12);
            _service.RegisterOwnedFixedVisionArea(
                OwnerA, "building:12:12", cell, 2, FogRevealShape.Square);
            Assert.IsTrue(_service.IsVisible(cell));

            _signals.Fire(new BuildingDemolishedSignal
            {
                BuildingId = "house",
                Position = cell,
                OwnerId = OwnerA,
            });

            Assert.IsFalse(_service.IsVisible(cell));
            Assert.IsFalse(_service.IsVisible(OwnerA, cell));
            Assert.IsTrue(_service.IsExplored(cell));
            Assert.IsTrue(_service.IsExplored(OwnerA, cell));
        }

        [Test]
        public void BuildingOwnershipTransferredSignal_MovesVisionBetweenOwners()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(12, 12);
            _service.RegisterOwnedFixedVisionArea(
                OwnerA, "building:12:12", cell, 2, FogRevealShape.Square);

            _signals.Fire(new BuildingOwnershipTransferredSignal
            {
                BuildingId = "house",
                Position = cell,
                PreviousOwnerId = OwnerA,
                NewOwnerId = OwnerB,
            });

            Assert.IsFalse(_service.IsVisible(cell), "local loses transferred building");
            Assert.IsFalse(_service.IsVisible(OwnerA, cell));
            Assert.IsTrue(_service.IsVisible(OwnerB, cell));
            Assert.IsTrue(_service.IsExplored(cell));
        }

        // ── Combat visibility (owner-scoped reads) ─────────────────────────

        [Test]
        public void CombatStyleQuery_HiddenFromAttackerUntilTheirVisionReaches()
        {
            // Mirrors UnitCombatService: _ownerFog.IsVisible(attackerOwner, targetPosition)
            var targetCell = new Vector2Int(18, 18);
            FireUnitCreated("defender", OwnerB, targetCell, visionRange: 1);

            Assert.IsFalse(_service.IsVisible(OwnerA, targetCell),
                "attacker without vision cannot see the target cell");

            _service.RegisterUnit(OwnerA, "scout", new Vector2Int(18, 16), 3);

            Assert.IsTrue(_service.IsVisible(OwnerA, targetCell));
        }

        // ── Save / load ────────────────────────────────────────────────────

        [Test]
        public void LocalExploredSnapshot_RoundTrips()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(4, 4);
            FireUnitCreated("ua", OwnerA, cell, visionRange: 1);
            bool[,] snapshot = _service.GetExploredSnapshot();
            Assert.IsTrue(snapshot[cell.x, cell.y]);

            var restored = CreateService(_signals);
            restored.Initialize();
            restored.Initialize(Width, Height);
            restored.SetLocalPerspectiveOwnerId(OwnerA);
            restored.LoadFromSnapshot(snapshot);

            Assert.IsTrue(restored.IsExplored(cell));
            Assert.IsFalse(restored.IsVisible(cell), "visibility is not restored from snapshots");
            restored.Dispose();
        }

        [Test]
        public void OwnerExploredSnapshot_RoundTrips()
        {
            var cell = new Vector2Int(4, 4);
            _service.RegisterUnit(OwnerA, "ua", cell, 1);
            bool[,] snapshot = _service.GetExploredSnapshot(OwnerA);
            Assert.IsTrue(snapshot[cell.x, cell.y]);

            var restored = CreateService(_signals);
            restored.Initialize();
            restored.Initialize(Width, Height);
            restored.LoadFromSnapshot(OwnerA, snapshot);

            Assert.IsTrue(restored.IsExplored(OwnerA, cell));
            Assert.IsFalse(restored.IsVisible(OwnerA, cell));
            restored.Dispose();
        }

        [Test]
        public void SaveModule_RoundTrip_PreservesLocalOwnerAndFixedAreas()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var localCell = new Vector2Int(4, 4);
            var enemyCell = new Vector2Int(18, 18);
            var buildingCell = new Vector2Int(12, 12);
            FireUnitCreated("ua", OwnerA, localCell, visionRange: 1);
            _service.RegisterUnit(OwnerB, "ub", enemyCell, 1);
            _service.RegisterOwnedFixedVisionArea(
                OwnerA, "building:12:12", buildingCell, 2, FogRevealShape.Square);

            var module = new FogOfWarSaveModule(_service);
            var stream = new MemoryStream();
            module.OnSave(new BinarySaveContext(stream));

            stream.Position = 0;
            var restored = CreateService(_signals);
            restored.Initialize();
            restored.Initialize(Width, Height);
            restored.SetLocalPerspectiveOwnerId(OwnerA);
            new FogOfWarSaveModule(restored).OnLoad(new BinarySaveContext(stream));

            Assert.IsTrue(restored.IsExplored(localCell), "local explored restored");
            Assert.IsTrue(restored.IsExplored(OwnerA, localCell), "owner explored restored");
            Assert.IsTrue(restored.IsExplored(OwnerB, enemyCell), "other owner explored restored");
            Assert.IsTrue(restored.IsExplored(buildingCell),
                "restored fixed vision area re-marks explored cells");
            restored.Dispose();
        }

        [Test]
        public void SaveModule_FormatVersion_IsStable()
        {
            var module = new FogOfWarSaveModule(_service);
            var stream = new MemoryStream();
            module.OnSave(new BinarySaveContext(stream));

            stream.Position = 0;
            int version = new BinaryReader(stream).ReadInt32();

            Assert.AreEqual(-4, version, "fog save format version must not change");
        }

        // ── Multiplayer replication reads ──────────────────────────────────

        [Test]
        public void PeerObservation_OnlyWhenPeerOwnerSeesCell()
        {
            // Mirrors MultiplayerAuthorityService.CanPeerObserveWorldEvent:
            // a peer observes a world event only when its owner grid sees the cell.
            var eventCell = new Vector2Int(15, 15);
            FireUnitCreated("ua", OwnerA, eventCell, visionRange: 1);

            Assert.IsTrue(_service.IsVisible(OwnerA, eventCell), "event owner observes");
            Assert.IsFalse(_service.IsVisible(OwnerB, eventCell),
                "peer without vision must not observe (fail closed)");
        }

        [Test]
        public void CellsBecameVisible_BatchesPerOwner()
        {
            var events = new List<(string owner, List<Vector2Int> cells)>();
            _service.CellsBecameVisible += (owner, cells)
                => events.Add((owner, cells.ToList()));

            _service.RegisterUnit(OwnerA, "ua", new Vector2Int(3, 3), 1);
            _service.RegisterUnit(OwnerA, "ua2", new Vector2Int(4, 4), 1);

            Assert.IsTrue(events.Count >= 2);
            Assert.IsTrue(events.All(e => e.owner == OwnerA));
            Assert.IsTrue(events.SelectMany(e => e.cells).Contains(new Vector2Int(3, 3)));
        }

        // ── Training / bot environment (simulation bindings) ───────────────

        [Test]
        public void SimulationBindings_ExposeWorkingOwnerScopedFog()
        {
            // Mirrors GameplayTrainingEpisode: InstallSimulationBindings wires the
            // fog stack without any visual updater or MonoBehaviour installer.
            var container = new DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            DeclareGameplaySignals(container);
            container.Bind<IGridService>().To<FakeGridService>().AsSingle();
            container.Bind<IWorldGenerationSignalState>()
                .To<FakeWorldGenerationSignalState>().AsSingle();

            FogOfWarInstaller.InstallSimulationBindings(container);

            var service = container.Resolve<FogOfWarService>();
            var ownerFog = container.Resolve<IFogOwnerStateReader>();
            var ownerVision = container.Resolve<IFogOwnerVisionSourceRegistry>();
            var localFog = container.Resolve<IFogStateReader>();

            service.Initialize();
            service.Initialize(Width, Height);

            var cell = new Vector2Int(7, 7);
            ownerVision.RegisterUnit(OwnerA, "ua", cell, 2);

            Assert.IsTrue(ownerFog.IsVisible(OwnerA, cell));
            Assert.IsFalse(ownerFog.IsVisible(OwnerB, cell));
            Assert.IsTrue(localFog.IsVisible(cell),
                "unresolved local perspective sees every owner");
        }

        // ── Dual-source divergence pins (flipped by the refactor commit) ───
        // Today _stateGrid and _ownerStates are written through separate
        // code paths, so owner-scoped mutations bypass the local grid even
        // when the owner IS the local perspective owner. These tests pin the
        // current (divergent) answers; the unification commit flips them to
        // the single-source contract.

        [Test]
        public void Divergence_OwnerScopedKeepVisibleReveal_TodaySkipsLocalGrid()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var center = new Vector2Int(9, 9);

            _service.RevealArea(OwnerA, center, 2, FogRevealShape.Square, keepVisible: true, "owner-reveal");

            Assert.IsTrue(_service.IsVisible(OwnerA, center));
            Assert.IsFalse(_service.IsVisible(center),
                "pre-unification: owner reveal never reaches the local grid");
        }

        [Test]
        public void Divergence_OwnerScopedRegistration_TodaySkipsLocalGrid()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(9, 9);

            _service.RegisterUnit(OwnerA, "ua", cell, 2);

            Assert.IsTrue(_service.IsVisible(OwnerA, cell));
            Assert.IsFalse(_service.IsVisible(cell),
                "pre-unification: owner-only registration bypasses the local grid");
        }

        [Test]
        public void Divergence_OwnerScopedMove_TodayLeavesLocalVisionBehind()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var from = new Vector2Int(4, 4);
            var to = new Vector2Int(4, 10);
            FireUnitCreated("ua", OwnerA, from, visionRange: 1);
            Assert.IsTrue(_service.IsVisible(from));

            _service.UpdateUnitPosition(OwnerA, "ua", to);

            Assert.IsTrue(_service.IsVisible(OwnerA, to));
            Assert.IsTrue(_service.IsVisible(from),
                "pre-unification: the local grid keeps the stale vision patch");
            Assert.IsFalse(_service.IsVisible(to),
                "pre-unification: the local grid never receives the move");
        }

        [Test]
        public void Divergence_OwnerScopedUnregister_TodayLeavesLocalVision()
        {
            _service.SetLocalPerspectiveOwnerId(OwnerA);
            var cell = new Vector2Int(4, 4);
            FireUnitCreated("ua", OwnerA, cell, visionRange: 1);
            Assert.IsTrue(_service.IsVisible(cell));

            _service.UnregisterUnit(OwnerA, "ua");

            Assert.IsFalse(_service.IsVisible(OwnerA, cell));
            Assert.IsTrue(_service.IsVisible(cell),
                "pre-unification: owner unregister leaves the local vision patch");
        }

        [Test]
        public void Divergence_OwnerSnapshotLoad_TodaySkipsLocalExplored()
        {
            var cell = new Vector2Int(4, 4);
            var snapshot = new bool[Width, Height];
            snapshot[cell.x, cell.y] = true;
            _service.SetLocalPerspectiveOwnerId(OwnerA);

            _service.LoadFromSnapshot(OwnerA, snapshot);

            Assert.IsTrue(_service.IsExplored(OwnerA, cell));
            Assert.IsFalse(_service.IsExplored(cell),
                "pre-unification: owner snapshot load never marks local explored");
        }

        // ── Boundary fog margin ───────────────────────────────────────────

        [Test]
        public void BoundaryMargin_RevealArea_KeepsEdgeCellsUnexplored()
        {
            var service = CreateService(
                _signals,
                new FogOfWarSettings { BoundaryFogMarginCells = 2 });
            try
            {
                service.Initialize();
                service.Initialize(Width, Height);

                service.RevealArea(
                    new Vector2Int(3, 3),
                    8,
                    FogRevealShape.PixelCircle,
                    keepVisible: true);

                Assert.AreEqual(
                    FogStateType.Unexplored,
                    service.GetFogState(new Vector2Int(0, 3)),
                    "outermost ring stays unexplored");
                Assert.AreEqual(
                    FogStateType.Unexplored,
                    service.GetFogState(new Vector2Int(1, 5)),
                    "margin ring stays unexplored");
                Assert.IsTrue(
                    service.IsVisible(new Vector2Int(2, 3)),
                    "first revealable cell is visible");
                Assert.IsTrue(
                    service.IsVisible(new Vector2Int(4, 4)),
                    "inner cells reveal normally");
                Assert.AreEqual(
                    FogStateType.Unexplored,
                    service.GetFogState(new Vector2Int(Width - 1, 6)),
                    "far edge stays unexplored");
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void BoundaryMargin_UnitVision_KeepsEdgeCellsUnexplored()
        {
            var service = CreateService(
                _signals,
                new FogOfWarSettings { BoundaryFogMarginCells = 2 });
            try
            {
                service.Initialize();
                service.Initialize(Width, Height);

                service.RegisterUnit("u", new Vector2Int(3, 3), 6);

                Assert.IsFalse(service.IsVisible(new Vector2Int(0, 3)));
                Assert.IsFalse(service.IsVisible(new Vector2Int(1, 3)));
                Assert.IsFalse(
                    service.IsExplored(new Vector2Int(0, 3)),
                    "edge cells never become explored");
                Assert.IsTrue(service.IsVisible(new Vector2Int(2, 3)));
                Assert.IsTrue(service.IsVisible(new Vector2Int(3, 3)));
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void BoundaryMargin_SnapshotLoad_ClearsEdgeCells()
        {
            var service = CreateService(
                _signals,
                new FogOfWarSettings { BoundaryFogMarginCells = 1 });
            try
            {
                service.Initialize();
                service.Initialize(Width, Height);

                var snapshot = new bool[Width, Height];
                for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    snapshot[x, y] = true;

                service.LoadFromSnapshot(snapshot);

                Assert.IsFalse(
                    service.IsExplored(new Vector2Int(0, 5)),
                    "loaded edge cell is cleared back to unexplored");
                Assert.IsFalse(
                    service.IsExplored(new Vector2Int(Width - 1, Height - 1)));
                Assert.IsTrue(
                    service.IsExplored(new Vector2Int(5, 5)),
                    "inner cells keep loaded explored state");
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void BoundaryMargin_PreviewReveal_KeepsEdgeCellsUnexplored()
        {
            var updater = new FogScreenSpaceTextureUpdater(
                new FogOfWarSettings { BoundaryFogMarginCells = 2 },
                null);
            try
            {
                updater.Initialize(8, 8, default);
                updater.PreviewRevealArea(
                    new Vector2Int(0, 0),
                    6,
                    FogRevealShape.Square,
                    keepVisible: true);

                var texture =
                    Shader.GetGlobalTexture("_MoyvaFogStateTexture")
                        as Texture2D;
                Assert.NotNull(texture);

                Color edge = texture.GetPixel(0, 0);
                Assert.Greater(
                    edge.g,
                    0.5f,
                    "preview leaves margin cell unexplored");

                Color inner = texture.GetPixel(4, 4);
                Assert.Less(
                    inner.g,
                    0.5f,
                    "preview reveals inner cells normally");
            }
            finally
            {
                updater.Dispose();
            }
        }

        // ── Helpers & fakes ────────────────────────────────────────────────

        private static void DeclareGameplaySignals(DiContainer container)
        {
            container.DeclareSignal<UnitCreatedSignal>();
            container.DeclareSignal<UnitMovedSignal>();
            container.DeclareSignal<UnitDestroyedSignal>();
            container.DeclareSignal<UnitGarrisonStateChangedSignal>();
            container.DeclareSignal<BuildingDemolishedSignal>();
            container.DeclareSignal<BuildingOwnershipTransferredSignal>();
            container.DeclareSignal<WorldGeneratedDataSignal>();
        }

        private static FogOfWarService CreateService(SignalBus signals)
            => CreateService(signals, null);

        private static FogOfWarService CreateService(
            SignalBus signals,
            FogOfWarSettings settings)
            => new FogOfWarService(
                new FakeVisibilityResolver(),
                null,
                null,
                null,
                signals,
                settings,
                null);

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

        private sealed class FakeGridService : IGridService
        {
            public string GetTileData(Vector2Int position) => "grass";

            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                tileTypeId = "grass";
                return true;
            }

            public void SetTileData(Vector2Int position, string tileTypeId) { }

            public int GridWidth => Width;
            public int GridHeight => Height;
        }

        private sealed class FakeWorldGenerationSignalState : IWorldGenerationSignalState
        {
            public long BeginWorldSnapshotCycle(string sessionId) => 1;
            public void Clear() { }
            public bool TryGetCurrentWorldIdentity(out long startupSequence, out string sessionId)
            {
                startupSequence = 1;
                sessionId = "test";
                return true;
            }

            public WorldGeneratedDataSignal StoreWorldGeneratedData(WorldGeneratedDataSignal signal)
                => signal;

            public bool TryGetWorldGeneratedData(out WorldGeneratedDataSignal signal)
            {
                signal = default;
                return false;
            }

            public bool TryStoreWorldSpawnPositions(
                WorldSpawnPositionsSignal signal, out WorldSpawnPositionsSignal storedSignal)
            {
                storedSignal = signal;
                return true;
            }

            public bool TryGetWorldSpawnPositions(out WorldSpawnPositionsSignal signal)
            {
                signal = default;
                return false;
            }
        }

        private sealed class BinarySaveContext : ISaveContext
        {
            public BinarySaveContext(MemoryStream stream)
            {
                Writer = new BinaryWriter(stream);
                Reader = new BinaryReader(stream);
            }

            public BinaryWriter Writer { get; }
            public BinaryReader Reader { get; }
        }
    }
}
