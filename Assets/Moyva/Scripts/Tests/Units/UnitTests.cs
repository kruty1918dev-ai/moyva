using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Units
{
    [TestFixture]
    public sealed class UnitClassConfigTests
    {
        [Test]
        public void Default_Role_IsWorker()
        {
            var cfg = new UnitClassConfig();
            Assert.AreEqual(UnitRole.Worker, cfg.Role);
        }

        [Test]
        public void Default_VisionRange_IsOne()
        {
            var cfg = new UnitClassConfig();
            Assert.AreEqual(1, cfg.VisionRange);
        }

        [Test]
        public void TypeId_CanBeSet()
        {
            var cfg = new UnitClassConfig { TypeId = "warrior-01" };
            Assert.AreEqual("warrior-01", cfg.TypeId);
        }

        [Test]
        public void Role_Military_CanBeSet()
        {
            var cfg = new UnitClassConfig { Role = UnitRole.Military };
            Assert.AreEqual(UnitRole.Military, cfg.Role);
        }

        [Test]
        public void BaseStamina_CanBeSet()
        {
            var cfg = new UnitClassConfig { BaseStamina = 100f };
            Assert.AreEqual(100f, cfg.BaseStamina);
        }

        [Test]
        public void StaminaRandomRange_Default()
        {
            var cfg = new UnitClassConfig();
            Assert.AreEqual(new Vector2(-5, 5), cfg.StaminaRandomRange);
        }
    }

    [TestFixture]
    public sealed class UnitRoleEnumTests
    {
        [Test]
        public void Worker_IsZero()
        {
            Assert.AreEqual(0, (int)UnitRole.Worker);
        }

        [Test]
        public void Military_IsOne()
        {
            Assert.AreEqual(1, (int)UnitRole.Military);
        }
    }

    [TestFixture]
    public sealed class UnitServiceTests : ZenjectUnitTestFixture
    {
        private sealed class FakeGridService : Grid.API.IGridService
        {
            private readonly Dictionary<Vector2Int, string> _tiles = new();
            public int GridWidth { get; set; } = 30;
            public int GridHeight { get; set; } = 30;
            public void SetTile(Vector2Int pos, string id) => _tiles[pos] = id;
            public string GetTileData(Vector2Int pos)
                => _tiles.TryGetValue(pos, out var d) ? d : "grass";
            public bool TryGetTileData(Vector2Int pos, out string data)
            {
                data = GetTileData(pos);
                return true;
            }
            public void SetTileData(Vector2Int pos, string data) => _tiles[pos] = data;
        }

        private sealed class FakeTileSettingsService : Grid.API.ITileSettingsService
        {
            public float GetTileWeight(string tileTypeId) => 1f;
        }

        private sealed class FakeUnitClassConfig : IUnitClassConfig
        {
            private readonly Dictionary<string, UnitClassConfig> _configs = new();
            public void Add(string typeId, UnitClassConfig cfg) => _configs[typeId] = cfg;
            public UnitClassConfig GetConfig(string typeId)
                => _configs.TryGetValue(typeId, out var c) ? c : null;
        }

        private sealed class FakeObjectsMapService : ObjectsMap.API.IObjectsMapService
        {
            public bool IsOccupied(Vector2Int position) => false;
            public bool TryGetOccupant(Vector2Int position, out string occupantId) { occupantId = null; return false; }
            public void Register(Vector2Int position, string occupantId) { }
            public void Move(Vector2Int from, Vector2Int to) { }
            public void Unregister(Vector2Int position) { }
            public bool TryGetPosition(string occupantId, out Vector2Int position) { position = default; return false; }
        }

        private IUnitService _service;
        private SignalBus _signalBus;
        private FakeUnitClassConfig _unitClassConfig;

        [SetUp]
        public void SetUp()
        {
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<InterruptMovementSignal>();

            _unitClassConfig = new FakeUnitClassConfig();
            _unitClassConfig.Add("warrior", new UnitClassConfig
            {
                TypeId = "warrior",
                BaseStamina = 50f,
                StaminaRandomRange = Vector2.zero,
                VisionRange = 3
            });
            _unitClassConfig.Add("scout", new UnitClassConfig
            {
                TypeId = "scout",
                BaseStamina = 80f,
                StaminaRandomRange = Vector2.zero,
                VisionRange = 5
            });

            Container.Bind<Grid.API.IGridService>().To<FakeGridService>().AsSingle();
            Container.Bind<Grid.API.ITileSettingsService>().To<FakeTileSettingsService>().AsSingle();
            Container.Bind<IUnitClassConfig>().FromInstance(_unitClassConfig).AsSingle();
            Container.Bind<ObjectsMap.API.IObjectsMapService>().To<FakeObjectsMapService>().AsSingle();

            var type = typeof(IUnitService).Assembly
                .GetType("Kruty1918.Moyva.Units.Runtime.UnitService");
            Container.BindInterfacesAndSelfTo(type).AsSingle().NonLazy();
            Container.ResolveRoots();

            _signalBus = Container.Resolve<SignalBus>();
            _service = Container.Resolve<IUnitService>();
            (_service as IInitializable)?.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            (_service as System.IDisposable)?.Dispose();
        }

        private void FireCreateUnit(string unitId, string typeId, Vector2Int pos)
        {
            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = unitId,
                UnitTypeId = typeId,
                Position = pos,
                VisionRange = 3,
                OwnerId = null
            });
        }

        // --- GetStamina ---
        [Test]
        public void GetStamina_AfterCreate_ReturnsBaseStamina()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            Assert.AreEqual(50f, _service.GetStamina("u1"));
        }

        [Test]
        public void GetStamina_Unknown_ReturnsZero()
        {
            Assert.AreEqual(0f, _service.GetStamina("unknown"));
        }

        [Test]
        public void GetStamina_AfterMove_DeductesCost()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            _signalBus.Fire(new UnitMovedSignal { UnitId = "u1", NewPosition = Vector2Int.one, Cost = 1f });
            Assert.AreEqual(49f, _service.GetStamina("u1"));
        }

        // --- SetStamina ---
        [Test]
        public void SetStamina_SetsValue()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            _service.SetStamina("u1", 25f);
            Assert.AreEqual(25f, _service.GetStamina("u1"));
        }

        [Test]
        public void SetStamina_NegativeValue_ClampsToZero()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            _service.SetStamina("u1", -10f);
            Assert.AreEqual(0f, _service.GetStamina("u1"));
        }

        [Test]
        public void SetStamina_UnknownUnit_DoesNothing()
        {
            Assert.DoesNotThrow(() => _service.SetStamina("unknown", 50f));
        }

        [Test]
        public void SetStamina_NullId_DoesNothing()
        {
            Assert.DoesNotThrow(() => _service.SetStamina(null, 50f));
        }

        [Test]
        public void SetStamina_EmptyId_DoesNothing()
        {
            Assert.DoesNotThrow(() => _service.SetStamina("", 50f));
        }

        // --- TryGetUnitPosition ---
        [Test]
        public void TryGetUnitPosition_AfterCreate_ReturnsTrue()
        {
            FireCreateUnit("u1", "warrior", new Vector2Int(5, 5));
            Assert.IsTrue(_service.TryGetUnitPosition("u1", out var pos));
            Assert.AreEqual(new Vector2Int(5, 5), pos);
        }

        [Test]
        public void TryGetUnitPosition_Unknown_ReturnsFalse()
        {
            Assert.IsFalse(_service.TryGetUnitPosition("unknown", out _));
        }

        [Test]
        public void TryGetUnitPosition_AfterMove_ReturnsNewPosition()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            _signalBus.Fire(new UnitMovedSignal { UnitId = "u1", NewPosition = new Vector2Int(3, 3), Cost = 1f });
            Assert.IsTrue(_service.TryGetUnitPosition("u1", out var pos));
            Assert.AreEqual(new Vector2Int(3, 3), pos);
        }

        // --- GetAllUnitIds ---
        [Test]
        public void GetAllUnitIds_Empty_Initially()
        {
            Assert.AreEqual(0, _service.GetAllUnitIds().Count);
        }

        [Test]
        public void GetAllUnitIds_ReturnsAllCreatedUnits()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            FireCreateUnit("u2", "scout", Vector2Int.one);
            Assert.AreEqual(2, _service.GetAllUnitIds().Count);
        }

        [Test]
        public void GetAllUnitIds_AfterDestroy_ExcludesDestroyed()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            FireCreateUnit("u2", "scout", Vector2Int.one);
            _signalBus.Fire(new UnitDestroyedSignal { UnitId = "u1" });
            Assert.AreEqual(1, _service.GetAllUnitIds().Count);
        }

        // --- GetUnitTypeId ---
        [Test]
        public void GetUnitTypeId_ReturnsTypeId()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            Assert.AreEqual("warrior", _service.GetUnitTypeId("u1"));
        }

        [Test]
        public void GetUnitTypeId_Unknown_ReturnsNull()
        {
            Assert.IsNull(_service.GetUnitTypeId("unknown"));
        }

        // --- Destroy ---
        [Test]
        public void Destroy_RemovesAllData()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            _signalBus.Fire(new UnitDestroyedSignal { UnitId = "u1" });

            Assert.AreEqual(0f, _service.GetStamina("u1"));
            Assert.IsFalse(_service.TryGetUnitPosition("u1", out _));
            Assert.IsNull(_service.GetUnitTypeId("u1"));
            Assert.IsNull(_service.GetUnitObject("u1"));
        }

        [Test]
        public void Destroy_UnknownUnit_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                _signalBus.Fire(new UnitDestroyedSignal { UnitId = "unknown" }));
        }

        // --- Unknown type ---
        [Test]
        public void Create_UnknownType_DoesNotRegister()
        {
            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "u1",
                UnitTypeId = "nonexistent",
                Position = Vector2Int.zero
            });
            Assert.AreEqual(0, _service.GetAllUnitIds().Count);
        }

        // --- InterruptMovement on insufficient stamina ---
        [Test]
        public void Move_InsufficientStamina_FiresInterrupt()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            _service.SetStamina("u1", 0.5f);

            int interruptCount = 0;
            _signalBus.Subscribe<InterruptMovementSignal>(_ => interruptCount++);

            _signalBus.Fire(new UnitMovedSignal { UnitId = "u1", NewPosition = Vector2Int.one, Cost = 1f });
            Assert.AreEqual(1, interruptCount);
            // Stamina should NOT change since move was blocked
            Assert.AreEqual(0.5f, _service.GetStamina("u1"));
        }

        // --- Move unregistered ---
        [Test]
        public void Move_UnregisteredUnit_Ignores()
        {
            Assert.DoesNotThrow(() =>
                _signalBus.Fire(new UnitMovedSignal { UnitId = "unknown", NewPosition = Vector2Int.one, Cost = 1f }));
        }

        // --- Multiple units ---
        [Test]
        public void MultipleUnits_IndependentStamina()
        {
            FireCreateUnit("u1", "warrior", Vector2Int.zero);
            FireCreateUnit("u2", "scout", Vector2Int.one);

            Assert.AreEqual(50f, _service.GetStamina("u1"));
            Assert.AreEqual(80f, _service.GetStamina("u2"));

            _signalBus.Fire(new UnitMovedSignal { UnitId = "u1", NewPosition = new Vector2Int(1, 0), Cost = 1f });
            Assert.AreEqual(49f, _service.GetStamina("u1"));
            Assert.AreEqual(80f, _service.GetStamina("u2"));
        }

        [Test]
        public void MultipleUnits_IndependentPositions()
        {
            FireCreateUnit("u1", "warrior", new Vector2Int(0, 0));
            FireCreateUnit("u2", "scout", new Vector2Int(5, 5));

            _service.TryGetUnitPosition("u1", out var p1);
            _service.TryGetUnitPosition("u2", out var p2);

            Assert.AreEqual(new Vector2Int(0, 0), p1);
            Assert.AreEqual(new Vector2Int(5, 5), p2);
        }
    }

    [TestFixture]
    public sealed class GameModeServiceTests : ZenjectUnitTestFixture
    {
        private Kruty1918.Moyva.GameMode.API.IGameModeService _service;
        private SignalBus _signalBus;
        private int _modeChangedCount;
        private GameModeChangedSignal _lastModeSignal;

        [SetUp]
        public void SetUp()
        {
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<GameModeChangedSignal>();

            var type = typeof(Kruty1918.Moyva.GameMode.API.IGameModeService).Assembly
                .GetType("Kruty1918.Moyva.GameMode.Runtime.GameModeService");
            Container.BindInterfacesAndSelfTo(type).AsSingle();
            Container.ResolveRoots();

            _signalBus = Container.Resolve<SignalBus>();
            _service = Container.Resolve<Kruty1918.Moyva.GameMode.API.IGameModeService>();

            _modeChangedCount = 0;
            _signalBus.Subscribe<GameModeChangedSignal>(s =>
            {
                _modeChangedCount++;
                _lastModeSignal = s;
            });
        }

        [Test]
        public void InitialMode_IsNormal()
        {
            Assert.AreEqual(GameModeType.Normal, _service.CurrentMode);
        }

        [Test]
        public void SetMode_Construction_ChangesMode()
        {
            _service.SetMode(GameModeType.Construction);
            Assert.AreEqual(GameModeType.Construction, _service.CurrentMode);
        }

        [Test]
        public void SetMode_FiresSignal()
        {
            _service.SetMode(GameModeType.Construction);
            Assert.AreEqual(1, _modeChangedCount);
            Assert.AreEqual(GameModeType.Construction, _lastModeSignal.NewMode);
        }

        [Test]
        public void SetMode_SameMode_DoesNotFireSignal()
        {
            _service.SetMode(GameModeType.Normal);
            Assert.AreEqual(0, _modeChangedCount);
        }

        [Test]
        public void SetMode_Toggle_FiresTwice()
        {
            _service.SetMode(GameModeType.Construction);
            _service.SetMode(GameModeType.Normal);
            Assert.AreEqual(2, _modeChangedCount);
            Assert.AreEqual(GameModeType.Normal, _service.CurrentMode);
        }

        [Test]
        public void SetMode_Lobby_Works()
        {
            _service.SetMode(GameModeType.Lobby);
            Assert.AreEqual(GameModeType.Lobby, _service.CurrentMode);
        }

        [Test]
        public void SetMode_ConstructionToLobby_Works()
        {
            _service.SetMode(GameModeType.Construction);
            _service.SetMode(GameModeType.Lobby);
            Assert.AreEqual(GameModeType.Lobby, _service.CurrentMode);
            Assert.AreEqual(2, _modeChangedCount);
        }
    }
}
