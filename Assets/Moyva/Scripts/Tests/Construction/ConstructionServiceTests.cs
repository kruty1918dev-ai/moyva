using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
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
        private IConstructionService _service;
        private IInitializable _serviceInitializable;
        private System.IDisposable _serviceDisposable;
        private SignalBus _signalBus;
        private IObjectsMapService _objectsMap;

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

            var constructionServiceType = typeof(IConstructionService).Assembly
                .GetType("Kruty1918.Moyva.Construction.Runtime.ConstructionService");
            Assert.NotNull(constructionServiceType, "Не знайдено тип ConstructionService у збірці Construction.");

            Container.Bind(typeof(IConstructionService), typeof(IInitializable), typeof(System.IDisposable))
                .To(constructionServiceType)
                .AsSingle()
                .NonLazy();

            _signalBus   = Container.Resolve<SignalBus>();
            _objectsMap  = Container.Resolve<IObjectsMapService>();
            _service     = Container.Resolve<IConstructionService>();
            _serviceInitializable = _service as IInitializable;
            _serviceDisposable = _service as System.IDisposable;
            Assert.NotNull(_serviceInitializable, "IConstructionService має реалізовувати IInitializable.");
            Assert.NotNull(_serviceDisposable, "IConstructionService має реалізовувати IDisposable.");

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
    }
}
