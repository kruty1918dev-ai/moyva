using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    /// <summary>
    /// Unit tests for FogOfWarService.
    /// Uses ZenjectUnitTestFixture with real SignalBus.
    /// IGridService and IFogTextureUpdater are stubbed inline.
    /// </summary>
    [TestFixture]
    public class FogOfWarServiceTests : ZenjectUnitTestFixture
    {
        // ─── Inline stubs ─────────────────────────────────────────────────────

        private class TestGridService : IGridService
        {
            public int GridWidth  { get; set; } = 20;
            public int GridHeight { get; set; } = 20;
            public string GetTileData(Vector2Int p) => "grass";
            public bool TryGetTileData(Vector2Int p, out string id) { id = "grass"; return true; }
            public void SetTileData(Vector2Int p, string id) { }
        }

        private class TestTextureUpdater : IFogTextureUpdater
        {
            public int UpdateCallCount { get; private set; }
            public void Initialize(int w, int h, Material mat) { }
            public void UpdateDirtyTiles(IFogOfWarService svc, IEnumerable<Vector2Int> dirty) => UpdateCallCount++;
            public void RebuildFullTexture(IFogOfWarService svc) { }
        }

        private class TestSaveDataProvider : IFogSaveDataProvider
        {
            public bool[,] SnapshotToLoad;
            public bool[,] LoadExploredData() => SnapshotToLoad;
            public void SaveExploredData(bool[,] e) { }
        }

        // ─── Fields ───────────────────────────────────────────────────────────

        private FogOfWarService     _service;
        private SignalBus           _signalBus;
        private TestTextureUpdater  _textureUpdater;
        private TestSaveDataProvider _saveProvider;
        private TestGridService     _gridService;

        public override void Setup()
        {
            base.Setup();

            // SignalBus
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();

            _gridService     = new TestGridService();
            _textureUpdater  = new TestTextureUpdater();
            _saveProvider    = new TestSaveDataProvider();

            Container.BindInstance<IGridService>(_gridService).AsSingle();
            Container.BindInstance<IFogTextureUpdater>(_textureUpdater).AsSingle();
            Container.BindInstance<IFogSaveDataProvider>(_saveProvider).AsSingle();

            Container.Bind<IFogVisibilityResolver>().To<FogVisibilityResolver>().AsSingle();
            Container.BindInterfacesAndSelfTo<FogOfWarService>().AsSingle().NonLazy();

            _signalBus = Container.Resolve<SignalBus>();
            _service   = Container.Resolve<FogOfWarService>();
            _service.Initialize();
        }

        public override void Teardown()
        {
            _service.Dispose();
            base.Teardown();
        }

        // Helper — initialize with a 10×10 map
        private void InitMap(int w = 10, int h = 10)
        {
            _service.Initialize(w, h);
        }

        // ─── 1. Initialize_SetsAllTilesToUnexplored ───────────────────────────

        [Test]
        public void Initialize_SetsAllTilesToUnexplored()
        {
            InitMap();
            Assert.AreEqual(FogStateType.Unexplored, _service.GetFogState(new Vector2Int(0, 0)));
            Assert.AreEqual(FogStateType.Unexplored, _service.GetFogState(new Vector2Int(5, 5)));
            Assert.AreEqual(FogStateType.Unexplored, _service.GetFogState(new Vector2Int(9, 9)));
        }

        // ─── 2. RegisterUnit_AddsVisibilityToTilesInRange ────────────────────

        [Test]
        public void RegisterUnit_AddsVisibilityToTilesInRange()
        {
            InitMap();
            _service.RegisterUnit("u1", new Vector2Int(5, 5), 2);
            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(new Vector2Int(5, 5)));
        }

        // ─── 3. RegisterUnit_MarksVisibleTilesAsExplored ─────────────────────

        [Test]
        public void RegisterUnit_MarksVisibleTilesAsExplored()
        {
            InitMap();
            _service.RegisterUnit("u1", new Vector2Int(5, 5), 2);
            Assert.IsTrue(_service.IsExplored(new Vector2Int(5, 5)));
        }

        // ─── 4. UpdateUnitPosition_RemovesVisibilityFromOldTiles ─────────────

        [Test]
        public void UpdateUnitPosition_RemovesVisibilityFromOldTiles()
        {
            InitMap();
            _service.RegisterUnit("u1", new Vector2Int(1, 1), 1);
            _service.UpdateUnitPosition("u1", new Vector2Int(8, 8));

            // Tile (1,1) should no longer be Visible (but is now Explored)
            Assert.AreNotEqual(FogStateType.Visible, _service.GetFogState(new Vector2Int(1, 1)));
            Assert.IsTrue(_service.IsExplored(new Vector2Int(1, 1)));
        }

        // ─── 5. TwoUnits_SameTile_CounterIsTwo_TileRemainsVisibleWhenOneLeaves

        [Test]
        public void TwoUnits_SameTile_CounterIsTwo_TileRemainsVisibleWhenOneLeaves()
        {
            InitMap();
            var origin = new Vector2Int(5, 5);
            _service.RegisterUnit("u1", origin, 0); // visionRange=0 → only origin
            _service.RegisterUnit("u2", origin, 0);

            // Move u1 away
            _service.UpdateUnitPosition("u1", new Vector2Int(0, 0));

            // origin should still be visible because u2 is there
            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(origin));
        }

        // ─── 6. LastUnit_Leaves_TileBecomesExplored_NotUnexplored ────────────

        [Test]
        public void LastUnit_Leaves_TileBecomesExplored_NotUnexplored()
        {
            InitMap();
            var origin = new Vector2Int(5, 5);
            _service.RegisterUnit("u1", origin, 0);
            _service.UpdateUnitPosition("u1", new Vector2Int(0, 0));

            Assert.AreEqual(FogStateType.Explored, _service.GetFogState(origin));
        }

        // ─── 7. UnregisterUnit_RemovesAllVision ──────────────────────────────

        [Test]
        public void UnregisterUnit_RemovesAllVision()
        {
            InitMap();
            var origin = new Vector2Int(5, 5);
            _service.RegisterUnit("u1", origin, 0);
            _service.UnregisterUnit("u1");

            Assert.AreEqual(FogStateType.Explored, _service.GetFogState(origin));
            Assert.IsFalse(_service.IsVisible(origin));
        }

        // ─── 8. LoadFromSnapshot_RestoresExploredState ───────────────────────

        [Test]
        public void LoadFromSnapshot_RestoresExploredState()
        {
            InitMap(5, 5);
            var snap = new bool[5, 5];
            snap[2, 2] = true;

            _service.LoadFromSnapshot(snap);

            Assert.IsTrue(_service.IsExplored(new Vector2Int(2, 2)));
            Assert.IsFalse(_service.IsExplored(new Vector2Int(0, 0)));
        }

        // ─── 9. GetFogState_ReturnsCorrectEnum_ForAllStates ──────────────────

        [Test]
        public void GetFogState_ReturnsCorrectEnum_ForAllStates()
        {
            InitMap();
            var pos = new Vector2Int(3, 3);

            // Initially unexplored
            Assert.AreEqual(FogStateType.Unexplored, _service.GetFogState(pos));

            // Mark as explored via snapshot
            var snap = new bool[10, 10];
            snap[3, 3] = true;
            _service.LoadFromSnapshot(snap);
            Assert.AreEqual(FogStateType.Explored, _service.GetFogState(pos));

            // Register unit → visible
            _service.RegisterUnit("u1", pos, 0);
            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(pos));
        }

        // ─── 10. NullSettings_DoesNotThrow_UsesDefaults ──────────────────────

        [Test]
        public void NullSettings_DoesNotThrow_UsesDefaults()
        {
            // FogOfWarService is created without settings by the container above
            // (no FogOfWarSettings bound → null injected)
            // The service should still initialise and operate without throwing.
            Assert.DoesNotThrow(() =>
            {
                _service.Initialize(5, 5);
                _service.RegisterUnit("u1", new Vector2Int(2, 2), 2);
            });
        }

        // ─── 11. UpdateUnitPosition_UnknownUnit_DoesNotThrow ─────────────────

        [Test]
        public void UpdateUnitPosition_UnknownUnit_DoesNotThrow()
        {
            InitMap();
            Assert.DoesNotThrow(() =>
                _service.UpdateUnitPosition("ghost", new Vector2Int(1, 1)));
        }

        // ─── 12. SignalBus_UnitCreatedSignal_RegistersUnit ───────────────────

        [Test]
        public void SignalBus_UnitCreatedSignal_RegistersUnit()
        {
            InitMap();
            var pos = new Vector2Int(4, 4);

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId     = "warrior-01_1",
                UnitTypeId = "warrior",
                Position   = pos,
                UnitObject = null
            });

            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(pos));
        }

        // ─── 13. SignalBus_UnitMovedSignal_UpdatesPosition ───────────────────

        [Test]
        public void SignalBus_UnitMovedSignal_UpdatesPosition()
        {
            InitMap();
            var start = new Vector2Int(1, 1);
            var end   = new Vector2Int(8, 8);

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "u1", UnitTypeId = "warrior", Position = start, UnitObject = null
            });

            _signalBus.Fire(new UnitMovedSignal { UnitId = "u1", NewPosition = end, Cost = 1f });

            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(end));
            Assert.AreNotEqual(FogStateType.Visible, _service.GetFogState(start));
        }

        // ─── 14. SignalBus_UnitDestroyedSignal_UnregistersUnit ───────────────

        [Test]
        public void SignalBus_UnitDestroyedSignal_UnregistersUnit()
        {
            InitMap();
            var pos = new Vector2Int(5, 5);

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = "u1", UnitTypeId = "warrior", Position = pos, UnitObject = null
            });

            _signalBus.Fire(new UnitDestroyedSignal { UnitId = "u1" });

            Assert.IsFalse(_service.IsVisible(pos));
        }
    }
}
