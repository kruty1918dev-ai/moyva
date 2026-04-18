using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.ObjectsMap
{
    // ====================================================================
    // ObjectsMapServiceExtendedTests — 15 tests
    // ====================================================================
    [TestFixture]
    public sealed class ObjectsMapServiceExtendedTests : ZenjectUnitTestFixture
    {
        private IObjectsMapService _service;
        private SignalBus _signalBus;
        private int _changedCount;

        public override void Setup()
        {
            base.Setup();
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<OnMapObjectSpawnedSignal>();
            Container.DeclareSignal<OnObjectsMapChangedSignal>();

            Container.BindInterfacesAndSelfTo<Kruty1918.Moyva.ObjectsMap.Runtime.ObjectsMapService>()
                .AsSingle().NonLazy();
            Container.ResolveRoots();

            _signalBus = Container.Resolve<SignalBus>();
            _service = Container.Resolve<IObjectsMapService>();
            Container.Resolve<Kruty1918.Moyva.ObjectsMap.Runtime.ObjectsMapService>().Initialize();
            _changedCount = 0;
            _signalBus.Subscribe<OnObjectsMapChangedSignal>(_ => _changedCount++);
        }

        public override void Teardown()
        {
            Container.Resolve<Kruty1918.Moyva.ObjectsMap.Runtime.ObjectsMapService>().Dispose();
            base.Teardown();
        }

        [Test]
        public void Register_FiresChangedSignal()
        {
            _service.Register(Vector2Int.zero, "u1");
            Assert.AreEqual(1, _changedCount);
        }

        [Test]
        public void Move_FiresTwoChangedSignals()
        {
            _service.Register(Vector2Int.zero, "u1");
            int before = _changedCount;
            _service.Move(Vector2Int.zero, Vector2Int.one);
            Assert.AreEqual(before + 2, _changedCount);
        }

        [Test]
        public void Unregister_FiresChangedSignal()
        {
            _service.Register(Vector2Int.zero, "u1");
            int before = _changedCount;
            _service.Unregister(Vector2Int.zero);
            Assert.AreEqual(before + 1, _changedCount);
        }

        [Test]
        public void Register_OccupiedPosition_Throws()
        {
            _service.Register(Vector2Int.zero, "u1");
            Assert.Throws<System.InvalidOperationException>(
                () => _service.Register(Vector2Int.zero, "u2"));
        }

        [Test]
        public void Move_EmptySource_Throws()
        {
            Assert.Throws<System.InvalidOperationException>(
                () => _service.Move(Vector2Int.zero, Vector2Int.one));
        }

        [Test]
        public void Move_OccupiedDest_Throws()
        {
            _service.Register(Vector2Int.zero, "u1");
            _service.Register(Vector2Int.one, "u2");
            Assert.Throws<System.InvalidOperationException>(
                () => _service.Move(Vector2Int.zero, Vector2Int.one));
        }

        [Test]
        public void Unregister_EmptyPosition_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.Unregister(Vector2Int.zero));
        }

        [Test]
        public void TryGetPosition_AfterRegister_ReturnsTrue()
        {
            _service.Register(new Vector2Int(5, 5), "elem");
            Assert.IsTrue(_service.TryGetPosition("elem", out var pos));
            Assert.AreEqual(new Vector2Int(5, 5), pos);
        }

        [Test]
        public void TryGetPosition_AfterUnregister_ReturnsFalse()
        {
            _service.Register(Vector2Int.zero, "elem");
            _service.Unregister(Vector2Int.zero);
            Assert.IsFalse(_service.TryGetPosition("elem", out _));
        }

        [Test]
        public void IsOccupied_AfterMove_SourceEmpty_DestOccupied()
        {
            _service.Register(Vector2Int.zero, "u1");
            _service.Move(Vector2Int.zero, Vector2Int.one);
            Assert.IsFalse(_service.IsOccupied(Vector2Int.zero));
            Assert.IsTrue(_service.IsOccupied(Vector2Int.one));
        }

        [Test]
        public void TryGetOccupant_ReturnsCorrectId()
        {
            _service.Register(new Vector2Int(3, 3), "building_01");
            Assert.IsTrue(_service.TryGetOccupant(new Vector2Int(3, 3), out var id));
            Assert.AreEqual("building_01", id);
        }

        [Test]
        public void TryGetOccupant_EmptyPos_ReturnsFalse()
        {
            Assert.IsFalse(_service.TryGetOccupant(new Vector2Int(7, 7), out _));
        }

        // Signal-based operations
        [Test]
        public void UnitCreatedSignal_RegistersOccupant()
        {
            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "u1",
                Position = new Vector2Int(2, 2)
            });
            Assert.IsTrue(_service.IsOccupied(new Vector2Int(2, 2)));
        }

        [Test]
        public void UnitDestroyedSignal_UnregistersOccupant()
        {
            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "u1",
                Position = new Vector2Int(2, 2)
            });
            _signalBus.Fire(new UnitDestroyedSignal { UnitId = "u1" });
            Assert.IsFalse(_service.IsOccupied(new Vector2Int(2, 2)));
        }

        [Test]
        public void UnitMovedSignal_UpdatesPosition()
        {
            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "u1",
                Position = new Vector2Int(2, 2)
            });
            _signalBus.Fire(new UnitMovedSignal
            {
                UnitId = "u1",
                NewPosition = new Vector2Int(4, 4),
                Cost = 1f
            });
            Assert.IsFalse(_service.IsOccupied(new Vector2Int(2, 2)));
            Assert.IsTrue(_service.IsOccupied(new Vector2Int(4, 4)));
        }
    }
}
