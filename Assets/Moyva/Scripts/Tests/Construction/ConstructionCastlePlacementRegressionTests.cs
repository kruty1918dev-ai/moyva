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

            _registry = ScriptableObject.CreateInstance<BuildingRegistrySO>();
            _registry.Buildings = new[]
            {
                CreateCastleDefinition(),
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
                Object.DestroyImmediate(_registry);
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
        public void PerOwnerCastleRelocation_DoesNotMoveAnotherFactionsCastle()
        {
            Vector2Int factionAStart = new Vector2Int(20, 4);
            Vector2Int factionBStart = new Vector2Int(30, 4);
            Vector2Int factionADestination = new Vector2Int(24, 4);

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
            Assert.IsTrue(_service.TryPreviewAt(factionADestination));
            _service.Confirm();

            Assert.IsFalse(_objectsMap.IsOccupied(factionAStart));
            Assert.IsTrue(_objectsMap.IsOccupied(factionADestination));
            Assert.IsTrue(_objectsMap.IsOccupied(factionBStart));
            Assert.IsTrue(
                _service.HasPlacedBuilding("castle-01", "faction-a"));
            Assert.IsTrue(
                _service.HasPlacedBuilding("castle-01", "faction-b"));
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
                    new BuildingPerPlayerLimitModule
                    {
                        IsEnabled = true,
                        SingletonScope = BuildingModuleScope.PerBuilding,
                        MaxBuildingsPerPlayer = 1,
                    },
                },
            };
        }
    }
}
