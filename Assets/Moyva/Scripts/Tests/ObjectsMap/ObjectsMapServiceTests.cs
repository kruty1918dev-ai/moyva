using System.Collections.Generic;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.ObjectsMap.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.ObjectsMap
{
    /// <summary>
    /// Юніт-тести для ObjectsMapService.
    /// Використовує ZenjectUnitTestFixture з реальним SignalBus для сигнало-орієнтованих тестів.
    /// </summary>
    [TestFixture]
    public class ObjectsMapServiceTests : ZenjectUnitTestFixture
    {
        private IObjectsMapService _service;
        private ObjectsMapService _serviceImpl;
        private SignalBus _signalBus;

        public override void Setup()
        {
            base.Setup();

            // Встановлюємо SignalBus з потрібними сигналами
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<OnMapObjectSpawnedSignal>();
            Container.DeclareSignal<OnObjectsMapChangedSignal>();

            // Реєструємо ObjectsMapService
            Container.BindInterfacesAndSelfTo<ObjectsMapService>().AsSingle().NonLazy();

            _signalBus = Container.Resolve<SignalBus>();
            _service = Container.Resolve<IObjectsMapService>();
            _serviceImpl = Container.Resolve<ObjectsMapService>();
            _serviceImpl.Initialize();
        }

        public override void Teardown()
        {
            _serviceImpl.Dispose();
            base.Teardown();
        }

        // ─── Register ────────────────────────────────────────────────────────

        [Test]
        public void Register_ShouldOccupyPosition()
        {
            var pos = new Vector2Int(2, 3);
            _service.Register(pos, "unit_01");

            Assert.IsTrue(_service.IsOccupied(pos));
            Assert.IsTrue(_service.TryGetOccupant(pos, out var id));
            Assert.AreEqual("unit_01", id);
        }

        [Test]
        public void Register_ShouldThrow_WhenPositionAlreadyOccupied()
        {
            var pos = new Vector2Int(1, 1);
            _service.Register(pos, "unit_01");

            Assert.Throws<System.InvalidOperationException>(() => _service.Register(pos, "unit_02"));
        }

        [Test]
        public void Register_ShouldFireOnObjectsMapChangedSignal()
        {
            var fired = new List<OnObjectsMapChangedSignal>();
            _signalBus.Subscribe<OnObjectsMapChangedSignal>(s => fired.Add(s));

            var pos = new Vector2Int(0, 0);
            _service.Register(pos, "unit_01");

            Assert.AreEqual(1, fired.Count);
            Assert.AreEqual(pos, fired[0].Position);
            Assert.AreEqual("unit_01", fired[0].OccupantId);
        }

        // ─── Unregister ───────────────────────────────────────────────────────

        [Test]
        public void Unregister_ShouldFreePosition()
        {
            var pos = new Vector2Int(3, 3);
            _service.Register(pos, "unit_01");
            _service.Unregister(pos);

            Assert.IsFalse(_service.IsOccupied(pos));
        }

        [Test]
        public void Unregister_ShouldDoNothing_WhenPositionAlreadyEmpty()
        {
            var pos = new Vector2Int(5, 5);
            Assert.DoesNotThrow(() => _service.Unregister(pos));
        }

        [Test]
        public void Unregister_ShouldFireOnObjectsMapChangedSignal_WithNullOccupant()
        {
            var pos = new Vector2Int(1, 2);
            _service.Register(pos, "unit_01");

            var fired = new List<OnObjectsMapChangedSignal>();
            _signalBus.Subscribe<OnObjectsMapChangedSignal>(s => fired.Add(s));

            _service.Unregister(pos);

            Assert.AreEqual(1, fired.Count);
            Assert.IsNull(fired[0].OccupantId);
        }

        // ─── Move ─────────────────────────────────────────────────────────────

        [Test]
        public void Move_ShouldUpdateBothPositions()
        {
            var from = new Vector2Int(0, 0);
            var to = new Vector2Int(1, 0);
            _service.Register(from, "unit_01");

            _service.Move(from, to);

            Assert.IsFalse(_service.IsOccupied(from));
            Assert.IsTrue(_service.IsOccupied(to));
            Assert.IsTrue(_service.TryGetOccupant(to, out var id));
            Assert.AreEqual("unit_01", id);
        }

        [Test]
        public void Move_ShouldThrow_WhenSourceEmpty()
        {
            Assert.Throws<System.InvalidOperationException>(() =>
                _service.Move(new Vector2Int(0, 0), new Vector2Int(1, 0)));
        }

        [Test]
        public void Move_ShouldThrow_WhenDestinationOccupied()
        {
            var from = new Vector2Int(0, 0);
            var to = new Vector2Int(1, 0);
            _service.Register(from, "unit_01");
            _service.Register(to, "unit_02");

            Assert.Throws<System.InvalidOperationException>(() => _service.Move(from, to));
        }

        [Test]
        public void Move_ShouldFireTwoOnObjectsMapChangedSignals()
        {
            var from = new Vector2Int(0, 0);
            var to = new Vector2Int(2, 0);
            _service.Register(from, "unit_01");

            var fired = new List<OnObjectsMapChangedSignal>();
            _signalBus.Subscribe<OnObjectsMapChangedSignal>(s => fired.Add(s));

            _service.Move(from, to);

            Assert.AreEqual(2, fired.Count);
        }

        // ─── TryGetOccupant ───────────────────────────────────────────────────

        [Test]
        public void TryGetOccupant_ShouldReturnFalse_WhenEmpty()
        {
            Assert.IsFalse(_service.TryGetOccupant(new Vector2Int(9, 9), out _));
        }

        // ─── TryGetPosition ───────────────────────────────────────────────────

        [Test]
        public void TryGetPosition_ShouldReturnCorrectPosition()
        {
            var pos = new Vector2Int(4, 4);
            _service.Register(pos, "unit_99");

            Assert.IsTrue(_service.TryGetPosition("unit_99", out var result));
            Assert.AreEqual(pos, result);
        }

        [Test]
        public void TryGetPosition_ShouldReturnFalse_WhenNotRegistered()
        {
            Assert.IsFalse(_service.TryGetPosition("ghost_unit", out _));
        }

        // ─── Signal-driven: OnUnitCreated ─────────────────────────────────────

        [Test]
        public void OnUnitCreated_ShouldRegisterUnit()
        {
            var pos = new Vector2Int(1, 1);
            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "warrior_01",
                UnitTypeId = "warrior",
                Position = pos,
                UnitObject = null
            });

            Assert.IsTrue(_service.IsOccupied(pos));
            Assert.IsTrue(_service.TryGetOccupant(pos, out var id));
            Assert.AreEqual("warrior_01", id);
        }

        [Test]
        public void OnUnitCreated_ShouldSkip_WhenPositionAlreadyOccupied()
        {
            var pos = new Vector2Int(1, 1);
            _service.Register(pos, "unit_01");

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "warrior_02",
                UnitTypeId = "warrior",
                Position = pos,
                UnitObject = null
            });

            Assert.IsTrue(_service.TryGetOccupant(pos, out var id));
            Assert.AreEqual("unit_01", id);
            Assert.IsFalse(_service.TryGetPosition("warrior_02", out _));
        }

        // ─── Signal-driven: OnUnitMoved ───────────────────────────────────────

        [Test]
        public void OnUnitMoved_ShouldMoveUnit()
        {
            var startPos = new Vector2Int(1, 1);
            var endPos = new Vector2Int(2, 1);

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "warrior_01",
                UnitTypeId = "warrior",
                Position = startPos,
                UnitObject = null
            });

            _signalBus.Fire(new UnitMovedSignal
            {
                UnitId = "warrior_01",
                NewPosition = endPos,
                Cost = 1f
            });

            Assert.IsFalse(_service.IsOccupied(startPos));
            Assert.IsTrue(_service.IsOccupied(endPos));
        }

        [Test]
        public void OnUnitMoved_ShouldSkip_WhenDestinationOccupied()
        {
            var startPos = new Vector2Int(1, 1);
            var endPos = new Vector2Int(2, 1);

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "warrior_01",
                UnitTypeId = "warrior",
                Position = startPos,
                UnitObject = null
            });

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "warrior_02",
                UnitTypeId = "warrior",
                Position = endPos,
                UnitObject = null
            });

            _signalBus.Fire(new UnitMovedSignal
            {
                UnitId = "warrior_01",
                NewPosition = endPos,
                Cost = 1f
            });

            Assert.IsTrue(_service.TryGetOccupant(startPos, out var startOccupant));
            Assert.AreEqual("warrior_01", startOccupant);
            Assert.IsTrue(_service.TryGetOccupant(endPos, out var endOccupant));
            Assert.AreEqual("warrior_02", endOccupant);
        }

        // ─── Signal-driven: OnUnitDestroyed ──────────────────────────────────

        [Test]
        public void OnUnitDestroyed_ShouldUnregisterUnit()
        {
            var pos = new Vector2Int(3, 3);
            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "warrior_01",
                UnitTypeId = "warrior",
                Position = pos,
                UnitObject = null
            });

            _signalBus.Fire(new UnitDestroyedSignal { UnitId = "warrior_01" });

            Assert.IsFalse(_service.IsOccupied(pos));
        }

        // ─── Signal-driven: OnMapObjectSpawned ───────────────────────────────

        [Test]
        public void OnMapObjectSpawned_ShouldRegisterStaticObject()
        {
            var pos = new Vector2Int(5, 5);
            _signalBus.Fire(new OnMapObjectSpawnedSignal { ObjectId = "river", Position = pos });

            Assert.IsTrue(_service.IsOccupied(pos));
            Assert.IsTrue(_service.TryGetOccupant(pos, out var id));
            Assert.AreEqual("river", id);
        }

        [Test]
        public void OnMapObjectSpawned_ShouldSkip_WhenPositionAlreadyOccupied()
        {
            var pos = new Vector2Int(5, 5);
            _service.Register(pos, "unit_01");

            // Не кидає виняток, але й не перезаписує
            _signalBus.Fire(new OnMapObjectSpawnedSignal { ObjectId = "river", Position = pos });

            Assert.IsTrue(_service.TryGetOccupant(pos, out var id));
            Assert.AreEqual("unit_01", id);
        }
    }

}
