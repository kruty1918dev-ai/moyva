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
    /// <summary>
    /// Юніт-тести для ConstructionService.
    /// Перевіряє: знесення будівель, режим знесення, відстеження гравецьких будівель.
    /// </summary>
    [TestFixture]
    public class ConstructionServiceTests : ZenjectUnitTestFixture
    {
        private sealed class FakeFogOfWarService : IFogOfWarService
        {
            private readonly Dictionary<Vector2Int, FogStateType> _states = new();

            public void SetState(Vector2Int position, FogStateType state)
            {
                _states[position] = state;
            }

            public void Initialize(int width, int height) { }
            public void RegisterUnit(string unitId, Vector2Int position, int visionRange) { }
            public void UpdateUnitVisionRange(string unitId, int visionRange) { }
            public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape) { }
            public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null) { }
            public void UpdateUnitPosition(string unitId, Vector2Int newPosition) { }
            public void UnregisterUnit(string unitId) { }
            public FogStateType GetFogState(Vector2Int position) => _states.TryGetValue(position, out var state) ? state : FogStateType.Visible;
            public bool IsVisible(Vector2Int position) => GetFogState(position) == FogStateType.Visible;
            public bool IsExplored(Vector2Int position) => GetFogState(position) != FogStateType.Unexplored;
            public bool[,] GetExploredSnapshot() => new bool[0, 0];
            public void LoadFromSnapshot(bool[,] explored) { }
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => System.Array.Empty<Vector2Int>();
        }

        private sealed class FakeWallPlacementService :
            IWallPlacementService,
            IWallTopologyService,
            IWallGateReplacementValidator
        {
            private readonly IObjectsMapService _objectsMapService;

            public FakeWallPlacementService(IObjectsMapService objectsMapService)
            {
                _objectsMapService = objectsMapService;
            }

            public void ShowWallHandles(Vector2Int wallPosition) { }
            public void DragWall(Vector2Int startPosition, Vector2 touchWorldPosition) { }
            public IReadOnlyList<Vector2Int> BuildPath(Vector2Int startPosition, Vector2Int endPosition) => new[] { startPosition, endPosition };
            public bool IsWallOrGate(string buildingId) => IsWall(buildingId) || IsGate(buildingId);
            public bool IsWall(string buildingId) => buildingId == "wall";
            public bool IsGate(string buildingId) => buildingId == "gate";

            public bool TryBuildPlacedMask(
                Vector2Int position,
                string buildingId,
                out WallCollectionDefinition collection,
                out TopologyNeighborMask mask)
            {
                collection = null;
                mask = default;
                return false;
            }

            public bool TryBuildPreviewMask(
                Vector2Int position,
                string buildingId,
                out WallCollectionDefinition collection,
                out TopologyNeighborMask mask)
            {
                collection = null;
                mask = default;
                return false;
            }

            public bool IsHorizontalWallSegment(
                Vector2Int position,
                WallCollectionDefinition collection,
                bool includePendingNeighbors)
                => false;

            public bool CanReplaceWallWithGate(Vector2Int position, string gateBuildingId, out string replacedWallId)
            {
                replacedWallId = null;
                if (!IsGate(gateBuildingId))
                    return false;

                if (!_objectsMapService.TryGetOccupant(position, out var occupantId) || occupantId != "wall")
                    return false;

                replacedWallId = occupantId;
                return true;
            }

            public bool TryResolvePlacedVisual(Vector2Int position, string occupantId, out GameObject prefab, out Quaternion rotation)
            {
                prefab = null;
                rotation = Quaternion.identity;
                return false;
            }

            public bool TryResolvePreviewVisual(Vector2Int position, string buildingId, out GameObject prefab)
            {
                prefab = null;
                return false;
            }

            public void EndDrag() { }
        }

        private IConstructionService _service;
        private IInitializable _serviceInitializable;
        private System.IDisposable _serviceDisposable;
        private SignalBus _signalBus;
        private IObjectsMapService _objectsMap;
        private FakeFogOfWarService _fogOfWarService;
        private BuildingRegistrySO _buildingRegistry;

        public override void Setup()
        {
            base.Setup();

            Zenject.SignalBusInstaller.Install(Container);

            // Сигнали будівництва
            Container.DeclareSignal<GameModeChangedSignal>();
            Container.DeclareSignal<BuildingPlacedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingCancelledSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingPreviewChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingPreviewMovedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingSelectionChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();

            // Сигнали ObjectsMap (потрібні для ObjectsMapService)
            Container.DeclareSignal<UnitCreatedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitMovedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitDestroyedSignal>().OptionalSubscriber();
            Container.DeclareSignal<OnMapObjectSpawnedSignal>().OptionalSubscriber();
            Container.DeclareSignal<OnObjectsMapChangedSignal>().OptionalSubscriber();

            Container.BindInterfacesAndSelfTo<ObjectsMapService>().AsSingle().NonLazy();
            Container.Bind<IFogOfWarService>().To<FakeFogOfWarService>().AsSingle();
            Container.BindInterfacesTo<FakeWallPlacementService>().AsSingle();

            _buildingRegistry = ScriptableObject.CreateInstance<BuildingRegistrySO>();
            _buildingRegistry.Buildings = CreateUnrestrictedDefinitions(
                "house",
                "barracks",
                "tower",
                "market",
                "wall",
                "gate",
                "castle");
            Container.Bind<IBuildingRegistry>().FromInstance(_buildingRegistry).AsSingle();
            Container.BindInstance(0).WithId("minSpacing");
            Container.BindInstance(2).WithId("townHallBuildRadius");

            var constructionServiceType = typeof(IConstructionService).Assembly
                .GetType("Kruty1918.Moyva.Construction.Runtime.ConstructionService");
            Assert.NotNull(constructionServiceType, "Не знайдено тип ConstructionService у збірці Construction.");

            Container.Bind(typeof(IConstructionService), typeof(IInitializable), typeof(System.IDisposable))
                .To(constructionServiceType)
                .AsSingle()
                .NonLazy();

            _signalBus   = Container.Resolve<SignalBus>();
            _objectsMap  = Container.Resolve<IObjectsMapService>();
            _fogOfWarService = Container.Resolve<IFogOfWarService>() as FakeFogOfWarService;
            _service     = Container.Resolve<IConstructionService>();
            _serviceInitializable = _service as IInitializable;
            _serviceDisposable = _service as System.IDisposable;
            Assert.NotNull(_serviceInitializable, "IConstructionService має реалізовувати IInitializable.");
            Assert.NotNull(_serviceDisposable, "IConstructionService має реалізовувати IDisposable.");
            Assert.NotNull(_fogOfWarService, "FakeFogOfWarService має бути зарезолвлений для construction tests.");

            Container.Resolve<ObjectsMapService>().Initialize();
            _serviceInitializable.Initialize();

            // Переходимо в режим будівництва, щоб сервіс став активним
            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Construction });
        }

        public override void Teardown()
        {
            _serviceDisposable.Dispose();
            Container.Resolve<ObjectsMapService>().Dispose();
            if (_buildingRegistry != null)
                Object.DestroyImmediate(_buildingRegistry);
            base.Teardown();
        }

        // ─── IsDemolishMode ───────────────────────────────────────────────────

        [Test]
        public void IsDemolishMode_ShouldBeFalseByDefault()
        {
            Assert.IsFalse(_service.IsDemolishMode);
        }

        [Test]
        public void ToggleDemolishMode_ShouldEnableDemolishMode()
        {
            _service.ToggleDemolishMode();
            Assert.IsTrue(_service.IsDemolishMode);
        }

        [Test]
        public void ToggleDemolishMode_ShouldDisableDemolishMode_WhenCalledTwice()
        {
            _service.ToggleDemolishMode();
            _service.ToggleDemolishMode();
            Assert.IsFalse(_service.IsDemolishMode);
        }

        [Test]
        public void ToggleDemolishMode_ShouldResetToFalse_WhenModeChangedToNormal()
        {
            _service.ToggleDemolishMode();
            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Normal });
            Assert.IsFalse(_service.IsDemolishMode);
        }

        [Test]
        public void SelectBuilding_ShouldDisableDemolishMode_AndAllowPreview()
        {
            var pos = new Vector2Int(9, 9);

            _service.ToggleDemolishMode();
            Assert.IsTrue(_service.IsDemolishMode);

            _service.SelectBuilding("barracks");

            Assert.IsFalse(_service.IsDemolishMode);
            Assert.AreEqual(BuildingPlacementState.Placing, _service.State);
            Assert.IsTrue(_service.TryPreviewAt(pos));
        }

        [Test]
        public void ModeChangeToNormal_ShouldCancelPendingPlacements()
        {
            var cancelledSignals = new List<BuildingCancelledSignal>();
            var previewSignals = new List<BuildingPreviewChangedSignal>();
            var pos = new Vector2Int(7, 7);

            _signalBus.Subscribe<BuildingCancelledSignal>(s => cancelledSignals.Add(s));
            _signalBus.Subscribe<BuildingPreviewChangedSignal>(s => previewSignals.Add(s));

            _service.SelectBuilding("barracks");
            _service.TryPreviewAt(pos);

            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Normal });

            Assert.AreEqual(BuildingPlacementState.Idle, _service.State);
            Assert.AreEqual(1, cancelledSignals.Count);
            Assert.IsTrue(previewSignals.Exists(s => s.Position == pos && s.PreviewState == BuildingPreviewState.None));
            Assert.IsFalse(_service.TryPreviewAt(new Vector2Int(8, 8)));
        }

        [Test]
        public void RedoLast_ShouldRestoreAllUndoneBuildings_OneByOne()
        {
            var positions = new[]
            {
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(3, 1),
                new Vector2Int(4, 1),
            };

            foreach (var position in positions)
                PreviewBuilding("barracks", position);

            for (int i = 0; i < positions.Length; i++)
                _service.UndoLast();

            foreach (var position in positions)
                Assert.IsFalse(_service.HasPendingPlacementAt(position));

            foreach (var position in positions)
            {
                _service.RedoLast();
                Assert.IsTrue(_service.HasPendingPlacementAt(position));
            }
        }

        [Test]
        public void Cancel_ShouldKeepRedoHistory_ForCurrentConstructionSession()
        {
            var positions = new[]
            {
                new Vector2Int(10, 1),
                new Vector2Int(11, 1),
                new Vector2Int(12, 1),
                new Vector2Int(13, 1),
            };

            foreach (var position in positions)
                PreviewBuilding("tower", position);

            _service.Cancel();

            foreach (var position in positions)
                Assert.IsFalse(_service.HasPendingPlacementAt(position));

            foreach (var position in positions)
            {
                _service.RedoLast();
                Assert.IsTrue(_service.HasPendingPlacementAt(position));
            }
        }

        [Test]
        public void ModeChangeToNormal_ShouldClearRedoHistory()
        {
            var position = new Vector2Int(14, 2);

            PreviewBuilding("market", position);
            _service.Cancel();
            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Normal });
            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Construction });

            _service.RedoLast();

            Assert.IsFalse(_service.HasPendingPlacementAt(position));
        }

        [Test]
        public void TryPreviewAt_ShouldAllowOnlyVisibleFogTiles()
        {
            var exploredPosition = new Vector2Int(15, 2);
            var visiblePosition = new Vector2Int(16, 2);

            _fogOfWarService.SetState(exploredPosition, FogStateType.Explored);
            _fogOfWarService.SetState(visiblePosition, FogStateType.Visible);
            _service.SelectBuilding("barracks");

            Assert.IsFalse(_service.TryPreviewAt(exploredPosition));
            Assert.IsTrue(_service.TryPreviewAt(visiblePosition));
        }

        [Test]
        public void TryMovePendingPlacement_ShouldMoveUnconfirmedBuilding()
        {
            var start = new Vector2Int(17, 3);
            var end = new Vector2Int(18, 3);

            PreviewBuilding("wall", start);

            Assert.IsTrue(_service.TryMovePendingPlacement(start, end));
            Assert.IsFalse(_service.HasPendingPlacementAt(start));
            Assert.IsTrue(_service.HasPendingPlacementAt(end));
        }

        [Test]
        public void GatePreview_ShouldBeBlocked_WhenTargetIsNotWall()
        {
            var freePosition = new Vector2Int(20, 3);

            _service.SelectBuilding("gate");

            Assert.IsFalse(_service.TryPreviewAt(freePosition));
        }

        [Test]
        public void GateConfirm_ShouldReplaceExistingWall()
        {
            var pos = new Vector2Int(21, 3);
            PlaceAndConfirmBuilding("wall", pos);

            _service.SelectBuilding("gate");
            Assert.IsTrue(_service.TryPreviewAt(pos));
            _service.Confirm();

            Assert.IsTrue(_objectsMap.TryGetOccupant(pos, out var occupantId));
            Assert.AreEqual("gate", occupantId);
        }

        [Test]
        public void PerPlayerLimit_BlocksSecondCopyAndAllowsAnotherFaction()
        {
            ConfigurePerPlayerLimitRegistry(1);

            Assert.IsTrue(_service.TryDirectPlace("limited", new Vector2Int(50, 1), "faction-a"));
            Assert.IsFalse(_service.TryDirectPlace("limited", new Vector2Int(51, 1), "faction-a"));
            Assert.IsTrue(_service.TryDirectPlace("limited", new Vector2Int(51, 1), "faction-b"));
        }

        [Test]
        public void PerPlayerLimit_BlocksSecondPendingCopyBeforeConfirm()
        {
            ConfigurePerPlayerLimitRegistry(1);
            _service.SetActiveOwner("solo-player");
            _service.SelectBuilding("limited");

            Assert.IsTrue(_service.TryPreviewAt(new Vector2Int(52, 1)));
            Assert.IsFalse(_service.TryPreviewAt(new Vector2Int(53, 1)));
        }

        [Test]
        public void BuildGridQuery_IgnoresPendingPreview_WhileAuthoritativeQueryStillCountsIt()
        {
            ConfigurePerPlayerLimitRegistry(1);
            _service.SetActiveOwner("solo-player");
            _service.SelectBuilding("limited");
            var previewPosition = new Vector2Int(52, 2);
            var otherPosition = new Vector2Int(53, 2);

            Assert.IsTrue(_service.TryPreviewAt(previewPosition));

            var placementQuery = (IConstructionPlacementQuery)_service;
            ConstructionPlacementQueryResult authoritative = placementQuery.EvaluatePlacement(
                new ConstructionPlacementQueryRequest("limited", otherPosition));
            ConstructionPlacementQueryResult buildGrid = placementQuery.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    "limited",
                    previewPosition,
                    includePendingPlacements: false));

            Assert.IsFalse(authoritative.IsValid, "Final placement must count pending previews.");
            Assert.IsTrue(buildGrid.IsValid, "Base grid must not be changed by a temporary preview.");
        }

        [Test]
        public void PerPlayerLimit_IsReleasedAfterDemolition()
        {
            ConfigurePerPlayerLimitRegistry(1);
            var position = new Vector2Int(54, 1);

            Assert.IsTrue(_service.TryDirectPlace("limited", position, "faction-a"));
            Assert.IsTrue(_service.TryDemolishByFaction(position, "faction-a"));
            Assert.IsTrue(_service.TryDirectPlace("limited", new Vector2Int(55, 1), "faction-a"));
        }

        [Test]
        public void PerPlayerLimit_DoesNotBlockRelocatingTheSameBuilding()
        {
            ConfigurePerPlayerLimitRegistry(1, singletonScope: BuildingModuleScope.Global);
            var source = new Vector2Int(56, 1);
            var destination = new Vector2Int(57, 1);

            Assert.IsTrue(_service.TryDirectPlace("limited", source, "faction-a"));
            _service.SetActiveOwner("faction-a");
            _service.SelectBuilding("limited");

            Assert.IsTrue(_service.TryPreviewAt(destination));
            _service.Confirm();
            Assert.IsFalse(_service.HasPlacedBuilding("limited", "missing-owner"));
            Assert.IsTrue(_service.HasPlacedBuilding("limited", "faction-a"));
            Assert.IsFalse(_objectsMap.IsOccupied(source));
            Assert.IsTrue(_objectsMap.IsOccupied(destination));
        }

        [Test]
        public void PerPlayerLimit_ZeroDoesNotBlockPlacement()
        {
            ConfigurePerPlayerLimitRegistry(0);
            Assert.IsTrue(_service.TryDirectPlace("limited", new Vector2Int(58, 1), "faction-a"));
            Assert.IsTrue(_service.TryDirectPlace("limited", new Vector2Int(59, 1), "faction-a"));
        }

        [Test]
        public void PerPlayerLimit_DisabledModuleDoesNotBlockPlacement()
        {
            ConfigurePerPlayerLimitRegistry(1, isEnabled: false);
            Assert.IsTrue(_service.TryDirectPlace("limited", new Vector2Int(60, 1), "faction-a"));
            Assert.IsTrue(_service.TryDirectPlace("limited", new Vector2Int(61, 1), "faction-a"));
        }

        [Test]
        public void TryPreviewAt_ShouldBlockRegularBuildingOutsideTownHallOrCastleRadius()
        {
            ConfigureInfluenceRegistry();

            _service.SelectBuilding("house");

            Assert.IsFalse(_service.TryPreviewAt(new Vector2Int(10, 10)));
        }

        [Test]
        public void TryPreviewAt_ShouldAllowRegularBuildingInsideTownHallRadius()
        {
            ConfigureInfluenceRegistry();
            var townHallPosition = new Vector2Int(30, 30);

            PlaceAndConfirmBuilding("townhall", townHallPosition);
            _service.SelectBuilding("house");

            Assert.IsTrue(_service.TryPreviewAt(new Vector2Int(32, 31)));
            Assert.IsFalse(_service.TryPreviewAt(new Vector2Int(33, 30)));
        }

        [Test]
        public void TryPreviewAt_ShouldAllowRegularBuildingInsideCastleRadius()
        {
            ConfigureInfluenceRegistry();
            var castlePosition = new Vector2Int(40, 40);

            PlaceAndConfirmBuilding("castle", castlePosition);
            _service.SelectBuilding("house");

            Assert.IsTrue(_service.TryPreviewAt(new Vector2Int(43, 40)));
            Assert.IsFalse(_service.TryPreviewAt(new Vector2Int(44, 40)));
        }

        [Test]
        public void TryPreviewAt_ShouldBlockSecondCenterInsideInfluenceRadius()
        {
            ConfigureInfluenceRegistry();

            PlaceAndConfirmBuilding("townhall", new Vector2Int(50, 50));
            _service.SelectBuilding("castle");

            Assert.IsFalse(_service.TryPreviewAt(new Vector2Int(51, 51)));
        }

        [Test]
        public void TryPreviewAt_ShouldBlockSecondCenterWhenInfluenceRadiiTouch()
        {
            ConfigureInfluenceRegistry();

            PlaceAndConfirmBuilding("townhall", new Vector2Int(80, 80));
            _service.SelectBuilding("castle");

            Assert.IsFalse(_service.TryPreviewAt(new Vector2Int(85, 80)));
        }

        [Test]
        public void TryPreviewAt_ShouldAllowSecondCenterWhenInfluenceRadiiDoNotOverlap()
        {
            ConfigureInfluenceRegistry();

            PlaceAndConfirmBuilding("townhall", new Vector2Int(90, 90));
            _service.SelectBuilding("castle");

            Assert.IsTrue(_service.TryPreviewAt(new Vector2Int(96, 90)));
        }

        [Test]
        public void Confirm_ShouldSkipPendingBuilding_WhenInfluenceCenterPendingWasRemoved()
        {
            ConfigureInfluenceRegistry();
            var townHallPosition = new Vector2Int(60, 60);
            var housePosition = new Vector2Int(61, 61);

            PreviewBuilding("townhall", townHallPosition);
            PreviewBuilding("house", housePosition);
            Assert.IsTrue(_service.RemovePendingAt(townHallPosition));

            _service.Confirm();

            Assert.IsFalse(_objectsMap.IsOccupied(housePosition));
        }

        [Test]
        public void TryDirectPlace_ShouldRespectTownHallOrCastleRadius()
        {
            ConfigureInfluenceRegistry();
            var castlePosition = new Vector2Int(70, 70);

            Assert.IsFalse(_service.TryDirectPlace("house", new Vector2Int(80, 80), "bot"));
            Assert.IsTrue(_service.TryDirectPlace("castle", castlePosition, "bot"));
            Assert.IsTrue(_service.TryDirectPlace("house", new Vector2Int(73, 70), "bot"));
            Assert.IsFalse(_service.TryDirectPlace("house", new Vector2Int(74, 70), "bot"));
        }

        // ─── TryDemolishAt ───────────────────────────────────────────────────

        [Test]
        public void TryDemolishAt_ShouldReturnFalse_WhenNotInDemolishMode()
        {
            var pos = new Vector2Int(1, 1);
            PlaceAndConfirmBuilding("barracks", pos);

            Assert.IsFalse(_service.TryDemolishAt(pos));
        }

        [Test]
        public void TryDemolishAt_ShouldReturnTrue_WhenPlayerPlacedBuilding()
        {
            var pos = new Vector2Int(2, 2);
            PlaceAndConfirmBuilding("barracks", pos);

            _service.ToggleDemolishMode();
            Assert.IsTrue(_service.TryDemolishAt(pos));
        }

        [Test]
        public void TryDemolishAt_ShouldFreePosition_AfterDemolish()
        {
            var pos = new Vector2Int(3, 3);
            PlaceAndConfirmBuilding("tower", pos);

            _service.ToggleDemolishMode();
            _service.TryDemolishAt(pos);
            _service.Confirm();

            Assert.IsFalse(_objectsMap.IsOccupied(pos));
        }

        [Test]
        public void TryDemolishAt_ShouldReturnFalse_WhenNotPlayerPlaced()
        {
            // Реєструємо будівлю безпосередньо в ObjectsMap (не через гравця)
            var pos = new Vector2Int(5, 5);
            _objectsMap.Register(pos, "pre-existing-building");

            _service.ToggleDemolishMode();
            Assert.IsFalse(_service.TryDemolishAt(pos));

            // Будівля залишається на місці
            Assert.IsTrue(_objectsMap.IsOccupied(pos));
        }

        [Test]
        public void TryDemolishAt_ShouldFireBuildingDemolishedSignal()
        {
            var pos = new Vector2Int(4, 4);
            PlaceAndConfirmBuilding("market", pos);

            var fired = new List<BuildingDemolishedSignal>();
            _signalBus.Subscribe<BuildingDemolishedSignal>(s => fired.Add(s));

            _service.ToggleDemolishMode();
            _service.TryDemolishAt(pos);
            _service.Confirm();

            Assert.AreEqual(1, fired.Count);
            Assert.AreEqual("market", fired[0].BuildingId);
            Assert.AreEqual(pos, fired[0].Position);
        }

        [Test]
        public void TryDemolishAt_ShouldNotAllowSamePositionTwice()
        {
            var pos = new Vector2Int(6, 6);
            PlaceAndConfirmBuilding("wall", pos);

            _service.ToggleDemolishMode();
            Assert.IsTrue(_service.TryDemolishAt(pos));
            Assert.IsTrue(_service.TryDemolishAt(pos)); // повторний виклик скасовує відмітку
        }

        [Test]
        public void ConfirmedFootprint_RegistersAllCells_AndDemolishFromSecondaryCellClearsAll()
        {
            var origin = new Vector2Int(24, 24);
            var secondary = origin + Vector2Int.right;
            _buildingRegistry.Buildings = new[]
            {
                new BuildingDefinition
                {
                    Id = "wide-house",
                    Footprint = new BuildingFootprint
                    {
                        Size = new Vector2Int(2, 1),
                        Anchor = BuildingFootprintAnchor.SouthWest,
                    },
                    UseCustomTownHallRules = true,
                    RequireTownHallInRange = false,
                },
            };

            _service.SelectBuilding("wide-house");
            Assert.IsTrue(_service.TryPreviewAt(origin));
            _service.Confirm();

            Assert.IsTrue(_objectsMap.IsOccupied(origin));
            Assert.IsTrue(_objectsMap.IsOccupied(secondary));

            _service.ToggleDemolishMode();
            Assert.IsTrue(_service.TryDemolishAt(secondary));
            _service.Confirm();

            Assert.IsFalse(_objectsMap.IsOccupied(origin));
            Assert.IsFalse(_objectsMap.IsOccupied(secondary));
        }

        [Test]
        public void Confirm_UpdatesOccupancyBeforePlacedSignal_ThenClearsSelection()
        {
            var position = new Vector2Int(26, 26);
            _service.SelectBuilding("house");
            Assert.IsTrue(_service.TryPreviewAt(position));

            var events = new List<string>();
            _signalBus.Subscribe<OnObjectsMapChangedSignal>(_ => events.Add("map"));
            _signalBus.Subscribe<BuildingPlacedSignal>(_ => events.Add("placed"));
            _signalBus.Subscribe<BuildingSelectionChangedSignal>(signal =>
            {
                if (signal.BuildingId == null)
                    events.Add("selection-cleared");
            });

            _service.Confirm();

            CollectionAssert.AreEqual(new[] { "map", "placed", "selection-cleared" }, events);
            Assert.AreEqual(BuildingPlacementState.Idle, _service.State);
            Assert.IsNull(_service.GetSelectedBuildingId());
        }

        // ─── Допоміжні методи ────────────────────────────────────────────────

        private void PlaceAndConfirmBuilding(string buildingId, Vector2Int pos)
        {
            _service.SelectBuilding(buildingId);
            _service.TryPreviewAt(pos);
            _service.Confirm();
        }

        private void PreviewBuilding(string buildingId, Vector2Int pos)
        {
            _service.SelectBuilding(buildingId);
            Assert.IsTrue(_service.TryPreviewAt(pos));
        }

        private void ConfigureInfluenceRegistry()
        {
            _buildingRegistry.Buildings = new[]
            {
                CreateBuilding("townhall", new TownHallBuildingModule { BuildRadius = 2 }),
                CreateBuilding("castle", new CastleBuildingModule { ExclusionRadius = 3 }),
                CreateBuilding("house"),
            };
        }

        private void ConfigurePerPlayerLimitRegistry(
            int limit,
            bool isEnabled = true,
            BuildingModuleScope singletonScope = BuildingModuleScope.PerBuilding)
        {
            var definition = CreateBuilding(
                "limited",
                new BuildingPerPlayerLimitModule
                {
                        MaxBuildingsPerPlayer = limit,
                        IsEnabled = isEnabled,
                        SingletonScope = singletonScope,
                });
            definition.UseCustomTownHallRules = true;
            definition.RequireTownHallInRange = false;
            _buildingRegistry.Buildings = new[]
            {
                definition,
            };
        }

        private static BuildingDefinition CreateBuilding(string id, params BuildingModuleDefinition[] modules)
        {
            return new BuildingDefinition
            {
                Id = id,
                Modules = modules != null
                    ? new List<BuildingModuleDefinition>(modules)
                    : new List<BuildingModuleDefinition>(),
            };
        }

        private static BuildingDefinition[] CreateUnrestrictedDefinitions(params string[] ids)
        {
            var definitions = new BuildingDefinition[ids.Length];
            for (int index = 0; index < ids.Length; index++)
            {
                definitions[index] = new BuildingDefinition
                {
                    Id = ids[index],
                    UseCustomTownHallRules = true,
                    RequireTownHallInRange = false,
                    BlockIfTownHallAlreadyInRange = false,
                };
            }

            return definitions;
        }
    }
}
