using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.SaveSystem;
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
            public int RebuildCallCount { get; private set; }
            public void Initialize(int w, int h, Material mat) { }
            public void UpdateDirtyTiles(IFogOfWarService svc, IEnumerable<Vector2Int> dirty) => UpdateCallCount++;
            public void RebuildFullTexture(IFogOfWarService svc) => RebuildCallCount++;
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
        private FogOfWarSettings    _settings;

        public override void Setup()
        {
            base.Setup();
            GameLaunchContext.Reset();

            // SignalBus
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<BuildingPlacedSignal>();
            Container.DeclareSignal<BuildingDemolishedSignal>();
            Container.DeclareSignal<WorldGeneratedDataSignal>();

            _gridService     = new TestGridService();
            _textureUpdater  = new TestTextureUpdater();
            _saveProvider    = new TestSaveDataProvider();
            _settings        = ScriptableObject.CreateInstance<FogOfWarSettings>();
            _settings.DefaultVisionRange = 1;
            _settings.MinVisionRange = 1;
            _settings.MaxVisionRange = 8;
            _settings.ElevationStep = 0.25f;
            _settings.MaxObserverHeightBonus = 4;
            _settings.MaxDownhillVisionBonus = 2;
            _settings.MaxUphillVisionPenalty = 4;
            _settings.EnableStartupFallbackReveal = false;

            Container.BindInstance<IGridService>(_gridService).AsSingle();
            Container.BindInstance<IFogTextureUpdater>(_textureUpdater).AsSingle();
            Container.BindInstance<IFogSaveDataProvider>(_saveProvider).AsSingle();
            Container.BindInstance(_settings).AsSingle();

            Container.Bind<IHeightAwareVisionService>().To<HeightAwareVisionService>().AsSingle();
            Container.Bind<IFogVisibilityResolver>().To<FogVisibilityResolver>().AsSingle();
            Container.BindInterfacesAndSelfTo<FogOfWarService>().AsSingle().NonLazy();

            _signalBus = Container.Resolve<SignalBus>();
            _service   = Container.Resolve<FogOfWarService>();
            _service.Initialize();
        }

        public override void Teardown()
        {
            _service.Dispose();
            Object.DestroyImmediate(_settings);
            GameLaunchContext.Reset();
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

        [Test]
        public void RegisterUnit_InitialReveal_IsDeterministicPixelCircle()
        {
            InitMap();
            _service.RegisterUnit("u1", new Vector2Int(5, 5), 2);

            int visibleCount = 0;
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    if (_service.IsVisible(new Vector2Int(x, y)))
                        visibleCount++;

            Assert.AreEqual(21, visibleCount);
            Assert.IsTrue(_service.IsVisible(new Vector2Int(7, 5)));
            Assert.IsTrue(_service.IsVisible(new Vector2Int(6, 6)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(7, 7)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(8, 5)));
        }

        [Test]
        public void RegisterUnit_InitialReveal_IgnoresHeightOcclusionHoles()
        {
            InitMap();
            var heightMap = new float[10, 10];
            heightMap[5, 5] = 0.05f;
            heightMap[6, 5] = 1f;

            _signalBus.Fire(new WorldGeneratedDataSignal
            {
                Width = 10,
                Height = 10,
                HeightMap = heightMap,
                TileMap = null,
                ObjectMap = null,
            });

            _service.RegisterUnit("u1", new Vector2Int(5, 5), 1);

            Assert.IsTrue(_service.IsVisible(new Vector2Int(6, 5)));
            Assert.IsTrue(_service.IsVisible(new Vector2Int(6, 6)));
        }

        [Test]
        public void RegisterFixedVisionArea_Diamond_KeepsShapeAfterWorldRecalculate()
        {
            InitMap();
            _service.RegisterFixedVisionArea("start", new Vector2Int(5, 5), 2, FogRevealShape.Diamond);

            _signalBus.Fire(new WorldGeneratedDataSignal
            {
                Width = 10,
                Height = 10,
                HeightMap = new float[10, 10],
                TileMap = null,
                ObjectMap = null,
            });

            Assert.IsTrue(_service.IsVisible(new Vector2Int(5, 7)));
            Assert.IsTrue(_service.IsVisible(new Vector2Int(6, 6)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(7, 7)));
        }

        [Test]
        public void FixedVisionAreasSnapshot_RestoresStarterAnchorVisibility()
        {
            InitMap();
            var center = new Vector2Int(5, 5);
            _service.RegisterFixedVisionArea("bootstrap-start-vision-anchor", center, 2, FogRevealShape.Diamond);

            var exploredSnapshot = _service.GetExploredSnapshot();
            var fixedAreas = _service.GetFixedVisionAreasSnapshot();

            _service.UnregisterUnit("bootstrap-start-vision-anchor");
            Assert.IsFalse(_service.IsVisible(center));

            _service.LoadFromSnapshot(exploredSnapshot);
            _service.LoadFixedVisionAreasSnapshot(fixedAreas);

            Assert.IsTrue(_service.IsVisible(center));
            Assert.IsTrue(_service.IsVisible(new Vector2Int(6, 6)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(7, 7)));
        }

        [Test]
        public void LoadFromSnapshot_ThenRegisterFixedVisionArea_MakesStartupCenterVisible()
        {
            InitMap();
            var center = new Vector2Int(5, 5);
            var snapshot = new bool[10, 10];
            snapshot[center.x, center.y] = true;

            _service.LoadFromSnapshot(snapshot);

            Assert.AreEqual(FogStateType.Explored, _service.GetFogState(center));
            Assert.IsFalse(_service.IsVisible(center));

            _service.RegisterFixedVisionArea("bootstrap-start-vision-anchor-initial", center, 0, FogRevealShape.PixelCircle);

            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(center));
            Assert.IsTrue(_service.IsVisible(center));
        }

        [Test]
        public void RevealArea_KeepVisible_MarksStartupRadiusVisible()
        {
            InitMap();
            var center = new Vector2Int(5, 5);

            _service.RevealArea(center, 2, FogRevealShape.Diamond, keepVisible: true, visibleAreaId: "start");

            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(center));
            Assert.IsTrue(_service.IsVisible(new Vector2Int(7, 5)));
            Assert.IsTrue(_service.IsVisible(new Vector2Int(6, 6)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(7, 7)));
        }

        [Test]
        public void RevealArea_AddsExploredTilesWithoutReplacingExistingSnapshot()
        {
            InitMap();
            var snapshot = new bool[10, 10];
            snapshot[1, 1] = true;
            _service.LoadFromSnapshot(snapshot);

            _service.RevealArea(new Vector2Int(5, 5), 1, FogRevealShape.Square, keepVisible: false);

            Assert.AreEqual(FogStateType.Explored, _service.GetFogState(new Vector2Int(1, 1)));
            Assert.AreEqual(FogStateType.Explored, _service.GetFogState(new Vector2Int(5, 5)));
            Assert.AreEqual(FogStateType.Unexplored, _service.GetFogState(new Vector2Int(9, 9)));
        }

        [Test]
        public void RevealArea_NearMapEdge_ClampsToMapBounds()
        {
            InitMap(4, 4);

            _service.RevealArea(new Vector2Int(-2, -2), 4, FogRevealShape.Square, keepVisible: true, visibleAreaId: "edge-start");

            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(new Vector2Int(0, 0)));
            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(new Vector2Int(2, 2)));
            Assert.AreEqual(FogStateType.Unexplored, _service.GetFogState(new Vector2Int(-1, -1)));
        }

        [Test]
        public void RevealArea_BeforeMapInitialize_AppliesAfterInitialize()
        {
            var center = new Vector2Int(3, 3);

            _service.RevealArea(center, 1, FogRevealShape.Square, keepVisible: true, visibleAreaId: "pending-start");
            InitMap(8, 8);

            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(center));
            Assert.IsTrue(_service.IsVisible(new Vector2Int(4, 4)));
        }

        [Test]
        public void RevealArea_WhenCurrentMapIsStale_QueuesUntilNextInitialize()
        {
            InitMap(5, 5);
            var center = new Vector2Int(8, 8);

            _service.RevealArea(center, 1, FogRevealShape.Square, keepVisible: true, visibleAreaId: "stale-start");

            Assert.AreEqual(FogStateType.Unexplored, _service.GetFogState(center));

            InitMap(10, 10);

            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(center));
            Assert.IsTrue(_service.IsVisible(new Vector2Int(9, 9)));
        }

        [Test]
        public void Initialize_FreshNewWorldWithoutBootstrapReveal_AppliesStartupFallbackReveal()
        {
            GameLaunchContext.ConfigureMenuNewGame();
            _settings.EnableStartupFallbackReveal = true;
            _settings.StartupFallbackRevealRadius = 2;
            _settings.StartupFallbackMinMarginFromBorder = 2;
            _settings.StartupFallbackRelativeMarginFactor = 0f;
            _settings.StartupFallbackRevealShape = FogRevealShape.Square;

            InitMap(10, 10);

            int visibleCount = 0;
            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                if (_service.IsVisible(new Vector2Int(x, y)))
                    visibleCount++;

            Assert.Greater(visibleCount, 0);
        }

        [Test]
        public void BuildingPlacedSignal_RegistersFixedVisionArea()
        {
            InitMap();

            _signalBus.Fire(new BuildingPlacedSignal
            {
                BuildingId = "house",
                Position = new Vector2Int(5, 5),
                OwnerId = "player",
                SourceFactionId = "player",
            });

            Assert.IsTrue(_service.IsVisible(new Vector2Int(5, 5)));
            Assert.IsTrue(_service.IsVisible(new Vector2Int(6, 5)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(7, 5)));
        }

        [Test]
        public void BuildingDemolishedSignal_RemovesFixedVisionAreaButKeepsExplored()
        {
            InitMap();
            var position = new Vector2Int(5, 5);

            _signalBus.Fire(new BuildingPlacedSignal
            {
                BuildingId = "house",
                Position = position,
                OwnerId = "player",
                SourceFactionId = "player",
            });

            _signalBus.Fire(new BuildingDemolishedSignal
            {
                BuildingId = "house",
                Position = position,
                OwnerId = "player",
                SourceFactionId = "player",
            });

            Assert.IsFalse(_service.IsVisible(position));
            Assert.AreEqual(FogStateType.Explored, _service.GetFogState(position));
        }

        [Test]
        public void LoadFromSnapshot_RebuildsTextureImmediately()
        {
            InitMap(5, 5);
            int rebuildsBefore = _textureUpdater.RebuildCallCount;
            var snap = new bool[5, 5];
            snap[2, 2] = true;

            _service.LoadFromSnapshot(snap);

            Assert.Greater(_textureUpdater.RebuildCallCount, rebuildsBefore);
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
            _service.RegisterUnit("u1", origin, 0);
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

        // ─── 10. MinVisionRange_IsClampedToOne ───────────────────────────────

        [Test]
        public void MinVisionRange_IsClampedToOne()
        {
            InitMap();

            _service.RegisterUnit("u1", new Vector2Int(2, 2), 0);

            Assert.IsTrue(_service.IsVisible(new Vector2Int(3, 3)));
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
                VisionRange = 1,
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
                UnitId = "u1", UnitTypeId = "warrior", Position = start, VisionRange = 1, UnitObject = null
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
                UnitId = "u1", UnitTypeId = "warrior", Position = pos, VisionRange = 1, UnitObject = null
            });

            _signalBus.Fire(new UnitDestroyedSignal { UnitId = "u1" });

            Assert.IsFalse(_service.IsVisible(pos));
        }

        [Test]
        public void WorldGeneratedDataSignal_RebuildsVisionWithHeightMap()
        {
            InitMap();
            var unitPos = new Vector2Int(5, 5);
            var farTile = new Vector2Int(7, 5);

            _service.RegisterUnit("u1", unitPos, 1);
            Assert.IsFalse(_service.IsVisible(farTile));

            var heightMap = new float[10, 10];
            heightMap[5, 5] = 0.75f;

            _signalBus.Fire(new WorldGeneratedDataSignal
            {
                Width = 10,
                Height = 10,
                HeightMap = heightMap,
                TileMap = null,
                ObjectMap = null,
            });

            Assert.IsTrue(_service.IsVisible(farTile));
            Assert.GreaterOrEqual(_textureUpdater.RebuildCallCount, 1);
        }
    }
}
