using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
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
            public void UpdateUnitPosition(string unitId, Vector2Int newPosition) { }
            public void UnregisterUnit(string unitId) { }
            public FogStateType GetFogState(Vector2Int position) => _states.TryGetValue(position, out var state) ? state : FogStateType.Visible;
            public bool IsVisible(Vector2Int position) => GetFogState(position) == FogStateType.Visible;
            public bool IsExplored(Vector2Int position) => GetFogState(position) != FogStateType.Unexplored;
            public bool[,] GetExploredSnapshot() => new bool[0, 0];
            public void LoadFromSnapshot(bool[,] explored) { }
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => System.Array.Empty<Vector2Int>();
        }

        private sealed class FakeWallPlacementService : IWallPlacementService
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

        public override void Setup()
        {
            base.Setup();

            Zenject.SignalBusInstaller.Install(Container);

            // Сигнали будівництва
            Container.DeclareSignal<GameModeChangedSignal>();
            Container.DeclareSignal<BuildingPlacedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingCancelledSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingPreviewChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();

            // Сигнали ObjectsMap (потрібні для ObjectsMapService)
            Container.DeclareSignal<UnitCreatedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitMovedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitDestroyedSignal>().OptionalSubscriber();
            Container.DeclareSignal<OnMapObjectSpawnedSignal>().OptionalSubscriber();
            Container.DeclareSignal<OnObjectsMapChangedSignal>().OptionalSubscriber();

            Container.BindInterfacesAndSelfTo<ObjectsMapService>().AsSingle().NonLazy();
            Container.Bind<IFogOfWarService>().To<FakeFogOfWarService>().AsSingle();
            Container.Bind<IWallPlacementService>().To<FakeWallPlacementService>().AsSingle();

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
            Assert.IsFalse(_service.TryDemolishAt(pos)); // повторний виклик — false
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
    }
}
