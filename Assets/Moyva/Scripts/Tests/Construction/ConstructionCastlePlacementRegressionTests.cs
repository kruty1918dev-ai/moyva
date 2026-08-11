#if MOYVA_LEGACY_SCRIPTABLEOBJECT_TESTS
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.ObjectsMap.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public sealed class ConstructionCastlePlacementRegressionTests :
        ZenjectUnitTestFixture
    {
        private sealed class VisibleFogService : IFogOfWarService
        {
            public void Initialize(int width, int height) { }
            public void RegisterUnit(
                string unitId,
                Vector2Int position,
                int visionRange) { }
            public void UpdateUnitVisionRange(
                string unitId,
                int visionRange) { }
            public void RegisterFixedVisionArea(
                string areaId,
                Vector2Int position,
                int visionRange,
                FogRevealShape shape) { }
            public void RevealArea(
                Vector2Int center,
                int radius,
                FogRevealShape shape,
                bool keepVisible,
                string visibleAreaId = null) { }
            public void UpdateUnitPosition(
                string unitId,
                Vector2Int newPosition) { }
            public void UnregisterUnit(string unitId) { }
            public FogStateType GetFogState(Vector2Int position)
                => FogStateType.Visible;
            public bool IsVisible(Vector2Int position) => true;
            public bool IsExplored(Vector2Int position) => true;
            public bool[,] GetExploredSnapshot() => new bool[0, 0];
            public void LoadFromSnapshot(bool[,] explored) { }
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles()
                => System.Array.Empty<Vector2Int>();
        }

        private IConstructionService _service;
        private IConstructionPlacementQuery _placementQuery;
        private IObjectsMapService _objectsMap;
        private BuildingRegistrySO _registry;
        private IInitializable _initializable;
        private System.IDisposable _disposable;
        private SignalBus _signalBus;

        public override void Setup()
        {
            base.Setup();
            Zenject.SignalBusInstaller.Install(Container);

            Container.DeclareSignal<GameModeChangedSignal>();
            Container.DeclareSignal<BuildingPlacedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingCancelledSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingPreviewChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingPreviewMovedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingSelectionChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitCreatedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitMovedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitDestroyedSignal>().OptionalSubscriber();
            Container.DeclareSignal<OnMapObjectSpawnedSignal>().OptionalSubscriber();
            Container.DeclareSignal<OnObjectsMapChangedSignal>().OptionalSubscriber();

            Container.BindInterfacesAndSelfTo<ObjectsMapService>()
                .AsSingle()
                .NonLazy();
            Container.Bind<IFogOfWarService>()
                .To<VisibleFogService>()
                .AsSingle();

            _registry = MoyvaJsonObjectFactory.Create<BuildingRegistrySO>();
            _registry.Buildings = new[]
            {
                CreateCastleDefinition(),
                CreateLimitedDefinition(
                    "move-pending-2",
                    2,
                    BuildingLimitScope.PerOwner,
                    BuildingLimitOverflowPolicy.MovePending),
                CreateLimitedDefinition(
                    "relocate-owner-2",
                    2,
                    BuildingLimitScope.PerOwner,
                    BuildingLimitOverflowPolicy.RelocateExisting),
                CreateLimitedDefinition(
                    "relocate-global-2",
                    2,
                    BuildingLimitScope.Global,
                    BuildingLimitOverflowPolicy.RelocateExisting),
            };
            Container.Bind<IBuildingRegistry>()
                .FromInstance(_registry)
                .AsSingle();
            Container.BindInstance(0).WithId("minSpacing");
            Container.BindInstance(0).WithId("townHallBuildRadius");

            System.Type serviceType = typeof(IConstructionService).Assembly
                .GetType(
                    "Kruty1918.Moyva.Construction.Runtime.ConstructionService");
            Assert.NotNull(serviceType);

            Container.Bind(
                    typeof(IConstructionService),
                    typeof(IConstructionPlacementQuery),
                    typeof(IInitializable),
                    typeof(System.IDisposable))
                .To(serviceType)
                .AsSingle()
                .NonLazy();

            _objectsMap = Container.Resolve<IObjectsMapService>();
            _service = Container.Resolve<IConstructionService>();
            _placementQuery = Container.Resolve<IConstructionPlacementQuery>();
            _initializable = _service as IInitializable;
            _disposable = _service as System.IDisposable;
            _signalBus = Container.Resolve<SignalBus>();

            Container.Resolve<ObjectsMapService>().Initialize();
            _initializable.Initialize();
            _signalBus.Fire(
                new GameModeChangedSignal
                {
                    NewMode = GameModeType.Construction,
                });
        }

        public override void Teardown()
        {
            _disposable?.Dispose();
            Container.Resolve<ObjectsMapService>().Dispose();
            if (_registry != null)
                MoyvaJsonObjectFactory.DestroyImmediate(_registry);
            base.Teardown();
        }

        [Test]
        public void SecondCastleClick_MovesTheOnlyPendingPreview()
        {
            Vector2Int first = new Vector2Int(4, 4);
            Vector2Int second = new Vector2Int(8, 4);
            _service.SetActiveOwner("player-a");
            _service.SelectBuilding("castle-01");

            Assert.IsTrue(_service.TryPreviewAt(first));
            Assert.IsTrue(_service.TryPreviewAt(second));

            IReadOnlyDictionary<Vector2Int, string> pending =
                _service.GetPendingPlacements();
            Assert.AreEqual(1, pending.Count);
            Assert.IsFalse(pending.ContainsKey(first));
            Assert.AreEqual("castle-01", pending[second]);
        }

        [Test]
        public void GridQuery_AutoIgnoresMovableCastlePreview_ButStrictQueryDoesNot()
        {
            Vector2Int first = new Vector2Int(10, 4);
            Vector2Int second = new Vector2Int(12, 4);
            _service.SetActiveOwner("player-a");
            _service.SelectBuilding("castle-01");
            Assert.IsTrue(_service.TryPreviewAt(first));

            ConstructionPlacementQueryResult strict =
                _placementQuery.EvaluatePlacement(
                    new ConstructionPlacementQueryRequest(
                        "castle-01",
                        second,
                        includeDetails: true,
                        ownerId: "player-a",
                        allowUniquePreviewRelocation: false));
            ConstructionPlacementQueryResult movable =
                _placementQuery.EvaluatePlacement(
                    new ConstructionPlacementQueryRequest(
                        "castle-01",
                        second,
                        includeDetails: true,
                        ownerId: "player-a",
                        attemptSource:
                            ConstructionPlacementAttemptSource.GridTileFilter,
                        allowUniquePreviewRelocation: true));

            Assert.IsFalse(strict.IsValid);
            Assert.AreEqual("per-player-limit", strict.Diagnostic.ReasonCode);
            Assert.IsTrue(movable.IsValid);
            Assert.AreEqual(first, movable.Diagnostic.IgnoredPendingPosition);
            Assert.AreEqual(0, movable.Diagnostic.PendingOwnedCount);
        }

        [Test]
        public void ConfirmedCastle_CannotStartSecondPreviewForSameOwner()
        {
            Vector2Int factionAStart =
                new Vector2Int(20, 4);
            Vector2Int factionBStart =
                new Vector2Int(30, 4);
            Vector2Int secondA =
                new Vector2Int(24, 4);

            Assert.IsTrue(
                _service.TryDirectPlace(
                    "castle-01",
                    factionAStart,
                    "faction-a"));
            Assert.IsTrue(
                _service.TryDirectPlace(
                    "castle-01",
                    factionBStart,
                    "faction-b"));

            _service.SetActiveOwner("faction-a");
            _service.SelectBuilding("castle-01");

            Assert.IsFalse(_service.TryPreviewAt(secondA));
            Assert.IsTrue(_objectsMap.IsOccupied(factionAStart));
            Assert.IsTrue(_objectsMap.IsOccupied(factionBStart));
            Assert.IsFalse(_objectsMap.IsOccupied(secondA));
        }

        [Test]
        public void SaveSnapshot_PreservesDifferentCastleOwners()
        {
            Vector2Int factionA =
                new Vector2Int(20, 4);
            Vector2Int factionB =
                new Vector2Int(30, 4);

            Assert.IsTrue(
                _service.TryDirectPlace(
                    "castle-01",
                    factionA,
                    "faction-a"));
            Assert.IsTrue(
                _service.TryDirectPlace(
                    "castle-01",
                    factionB,
                    "faction-b"));

            var snapshotSource =
                _service as IConstructionSaveSnapshotSource;
            Assert.NotNull(snapshotSource);

            IReadOnlyList<ConstructionSavedPlacement> placements =
                snapshotSource.GetSavedPlacements();

            bool foundA = false;
            bool foundB = false;
            for (int index = 0;
                 index < placements.Count;
                 index++)
            {
                ConstructionSavedPlacement placement =
                    placements[index];
                if (placement.Position == factionA
                    && placement.OwnerId == "faction-a")
                {
                    foundA = true;
                }

                if (placement.Position == factionB
                    && placement.OwnerId == "faction-b")
                {
                    foundB = true;
                }
            }

            Assert.IsTrue(foundA);
            Assert.IsTrue(foundB);
        }

        [Test]
        public void DirectPlacement_RemainsStrictAndRejectsSecondCastleForOwner()
        {
            Assert.IsTrue(
                _service.TryDirectPlace(
                    "castle-01",
                    new Vector2Int(40, 4),
                    "faction-a"));
            Assert.IsFalse(
                _service.TryDirectPlace(
                    "castle-01",
                    new Vector2Int(42, 4),
                    "faction-a"));
        }

        [Test]
        public void AuthoritativeCastleRelocation_IsRejectedAfterCastleExists()
        {
            Vector2Int source =
                new Vector2Int(50, 4);
            Vector2Int target =
                new Vector2Int(54, 4);

            Assert.IsTrue(
                _service.TryDirectPlace(
                    "castle-01",
                    source,
                    "faction-a"));

            var executor =
                _service as IAuthoritativeConstructionPlacementExecutor;
            Assert.NotNull(executor);

            Assert.IsFalse(
                executor.TryPlaceAuthoritatively(
                    "castle-01",
                    target,
                    "faction-a",
                    new ConstructionPlacementCommitIntent(source)));

            Assert.IsTrue(_objectsMap.IsOccupied(source));
            Assert.IsFalse(_objectsMap.IsOccupied(target));
        }

        [Test]
        public void MovePending_WithLimitAboveOne_MovesPreviewOnlyAtCapacity()
        {
            Vector2Int first = new Vector2Int(70, 4);
            Vector2Int second = new Vector2Int(72, 4);
            Vector2Int third = new Vector2Int(74, 4);
            _service.SetActiveOwner("faction-a");
            _service.SelectBuilding("move-pending-2");

            Assert.IsTrue(_service.TryPreviewAt(first));
            Assert.IsTrue(_service.TryPreviewAt(second));
            Assert.AreEqual(2, _service.GetPendingPlacements().Count);

            Assert.IsTrue(_service.TryPreviewAt(third));
            IReadOnlyDictionary<Vector2Int, string> pending =
                _service.GetPendingPlacements();
            Assert.AreEqual(2, pending.Count);
            Assert.IsFalse(pending.ContainsKey(first));
            Assert.IsTrue(pending.ContainsKey(second));
            Assert.AreEqual("move-pending-2", pending[third]);
        }

        [Test]
        public void RelocateExisting_WithLimitAboveOne_ReplacesOneOwnedInstance()
        {
            Vector2Int first = new Vector2Int(80, 4);
            Vector2Int second = new Vector2Int(82, 4);
            Vector2Int target = new Vector2Int(84, 4);
            Assert.IsTrue(
                _service.TryDirectPlace(
                    "relocate-owner-2",
                    first,
                    "faction-a"));
            Assert.IsTrue(
                _service.TryDirectPlace(
                    "relocate-owner-2",
                    second,
                    "faction-a"));

            _service.SetActiveOwner("faction-a");
            _service.SelectBuilding("relocate-owner-2");
            Assert.IsTrue(_service.TryPreviewAt(target));
            var intentSource =
                _service as IConstructionPendingPlacementIntentSource;
            Assert.NotNull(intentSource);
            Assert.IsTrue(
                intentSource.TryGetPendingPlacementIntent(
                    target,
                    out ConstructionPlacementCommitIntent intent));
            Assert.IsTrue(intent.RelocationSourcePosition.HasValue);
            CollectionAssert.Contains(
                new[] { first, second },
                intent.RelocationSourcePosition.Value);

            _service.Confirm();

            Assert.IsTrue(_objectsMap.IsOccupied(target));
            Assert.AreEqual(
                1,
                (_objectsMap.IsOccupied(first) ? 1 : 0)
                + (_objectsMap.IsOccupied(second) ? 1 : 0));
        }

        [Test]
        public void GlobalRelocateExisting_DoesNotRelocateForeignInstance()
        {
            Vector2Int foreignFirst = new Vector2Int(90, 4);
            Vector2Int foreignSecond = new Vector2Int(92, 4);
            Vector2Int target = new Vector2Int(94, 4);
            Assert.IsTrue(
                _service.TryDirectPlace(
                    "relocate-global-2",
                    foreignFirst,
                    "faction-b"));
            Assert.IsTrue(
                _service.TryDirectPlace(
                    "relocate-global-2",
                    foreignSecond,
                    "faction-c"));

            _service.SetActiveOwner("faction-a");
            _service.SelectBuilding("relocate-global-2");

            Assert.IsFalse(_service.TryPreviewAt(target));
            Assert.IsTrue(_objectsMap.IsOccupied(foreignFirst));
            Assert.IsTrue(_objectsMap.IsOccupied(foreignSecond));
            Assert.IsFalse(_objectsMap.IsOccupied(target));
        }

        [Test]
        public void GlobalRelocateExisting_RelocatesOnlyActiveOwnersInstance()
        {
            Vector2Int ownedSource = new Vector2Int(100, 4);
            Vector2Int foreignSource = new Vector2Int(102, 4);
            Vector2Int target = new Vector2Int(104, 4);
            Assert.IsTrue(
                _service.TryDirectPlace(
                    "relocate-global-2",
                    ownedSource,
                    "faction-a"));
            Assert.IsTrue(
                _service.TryDirectPlace(
                    "relocate-global-2",
                    foreignSource,
                    "faction-b"));

            _service.SetActiveOwner("faction-a");
            _service.SelectBuilding("relocate-global-2");
            Assert.IsTrue(_service.TryPreviewAt(target));
            _service.Confirm();

            Assert.IsFalse(_objectsMap.IsOccupied(ownedSource));
            Assert.IsTrue(_objectsMap.IsOccupied(foreignSource));
            Assert.IsTrue(_objectsMap.IsOccupied(target));
        }

        private static BuildingDefinition CreateCastleDefinition()
        {
            return new BuildingDefinition
            {
                Id = "castle-01",
                DisplayName = "Castle",
                Category = BuildingCategory.Military,
                UseCustomTownHallRules = true,
                RequireTownHallInRange = false,
                BlockIfTownHallAlreadyInRange = false,
                TownHallProximityRadiusOverride = 0,
                Footprint = new BuildingFootprint
                {
                    Size = Vector2Int.one,
                    OccupiedCells = new[]
                    {
                        Vector2Int.zero,
                    },
                    RequiresFlatGround = true,
                },
                Modules = new List<BuildingModuleDefinition>
                {
                    new CastleBuildingModule
                    {
                        IsEnabled = true,
                        SingletonScope = BuildingModuleScope.PerBuilding,
                        IsCapital = true,
                        ExclusionRadius = 0,
                    },
                    new SettlementCenterBuildingModule
                    {
                        IsEnabled = true,
                        SingletonScope =
                            BuildingModuleScope.PerBuilding,
                        InfluenceRadius = 5,
                    },
                    new BuildingPerPlayerLimitModule
                    {
                        IsEnabled = true,
                        SingletonScope = BuildingModuleScope.PerBuilding,
                        MaxBuildingsPerPlayer = 1,
                        LimitScope = BuildingLimitScope.PerOwner,
                        OverflowPolicy =
                            BuildingLimitOverflowPolicy
                                .RelocateExisting,
                    },
                },
            };
        }

        private static BuildingDefinition CreateLimitedDefinition(
            string id,
            int limit,
            BuildingLimitScope scope,
            BuildingLimitOverflowPolicy overflowPolicy)
        {
            return new BuildingDefinition
            {
                Id = id,
                DisplayName = id,
                Category = BuildingCategory.Civilian,
                UseCustomTownHallRules = true,
                RequireTownHallInRange = false,
                BlockIfTownHallAlreadyInRange = false,
                Footprint = new BuildingFootprint
                {
                    Size = Vector2Int.one,
                    OccupiedCells = new[]
                    {
                        Vector2Int.zero,
                    },
                    RequiresFlatGround = true,
                },
                Modules = new List<BuildingModuleDefinition>
                {
                    new BuildingPerPlayerLimitModule
                    {
                        IsEnabled = true,
                        SingletonScope =
                            BuildingModuleScope.PerBuilding,
                        MaxBuildingsPerPlayer = limit,
                        LimitScope = scope,
                        OverflowPolicy = overflowPolicy,
                    },
                },
            };
        }
    }
}

#endif
