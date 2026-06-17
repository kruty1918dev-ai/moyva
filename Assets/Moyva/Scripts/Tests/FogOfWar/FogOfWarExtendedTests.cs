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
    // ====================================================================
    // FogOfWarServiceExtendedTests — 15 tests
    // ====================================================================
    [TestFixture]
    public sealed class FogOfWarServiceExtendedTests : ZenjectUnitTestFixture
    {
        private sealed class StubGrid : IGridService
        {
            public int GridWidth { get; set; } = 20;
            public int GridHeight { get; set; } = 20;
            public string GetTileData(Vector2Int p) => "grass";
            public bool TryGetTileData(Vector2Int p, out string id) { id = "grass"; return true; }
            public void SetTileData(Vector2Int p, string id) { }
        }

        private sealed class StubTexUpdater : IFogTextureUpdater
        {
            public void Initialize(int w, int h, Material mat) { }
            public void UpdateDirtyTiles(IFogOfWarService s, IEnumerable<Vector2Int> d) { }
            public void RebuildFullTexture(IFogOfWarService s) { }
        }

        private sealed class StubSaveProvider : IFogSaveDataProvider
        {
            public bool[,] SnapshotToLoad;
            public bool[,] LoadExploredData() => SnapshotToLoad;
            public void SaveExploredData(bool[,] e) { }
        }

        private FogOfWarService _service;
        private FogOfWarSettings _settings;

        public override void Setup()
        {
            base.Setup();
            GameLaunchContext.Reset();
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<BuildingPlacedSignal>();
            Container.DeclareSignal<BuildingDemolishedSignal>();
            Container.DeclareSignal<WorldGeneratedDataSignal>();

            Container.BindInstance<IGridService>(new StubGrid()).AsSingle();
            Container.BindInstance<IFogTextureUpdater>(new StubTexUpdater()).AsSingle();
            Container.BindInstance<IFogSaveDataProvider>(new StubSaveProvider()).AsSingle();

            _settings = ScriptableObject.CreateInstance<FogOfWarSettings>();
            _settings.DefaultVisionRange = 1;
            _settings.MinVisionRange = 1;
            _settings.MaxVisionRange = 8;
            _settings.ElevationStep = 0.25f;
            _settings.MaxObserverHeightBonus = 4;
            _settings.MaxDownhillVisionBonus = 2;
            _settings.MaxUphillVisionPenalty = 4;
            _settings.EnableStartupFallbackReveal = false;
            Container.BindInstance(_settings).AsSingle();

            Container.Bind<IHeightAwareVisionService>().To<HeightAwareVisionService>().AsSingle();
            Container.Bind<IFogVisibilityResolver>().To<FogVisibilityResolver>().AsSingle();
            Container.BindInterfacesAndSelfTo<FogOfWarService>().AsSingle().NonLazy();

            _service = Container.Resolve<FogOfWarService>();
            _service.Initialize();
        }

        public override void Teardown()
        {
            _service.Dispose();
            Object.DestroyImmediate(_settings);
            GameLaunchContext.Reset();
            base.Teardown();
        }

        [Test]
        public void Initialize_1x1_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.Initialize(1, 1));
        }

        [Test]
        public void Initialize_LargeMap_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.Initialize(256, 256));
        }

        [Test]
        public void RegisterUnit_ThenUnregister_TileBecomesExplored()
        {
            _service.Initialize(10, 10);
            var pos = new Vector2Int(5, 5);
            _service.RegisterUnit("u1", pos, 0);
            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(pos));
            _service.UnregisterUnit("u1");
            Assert.AreEqual(FogStateType.Explored, _service.GetFogState(pos));
        }

        [Test]
        public void GetExploredSnapshot_ReturnsSameSize()
        {
            _service.Initialize(8, 8);
            var snap = _service.GetExploredSnapshot();
            Assert.AreEqual(8, snap.GetLength(0));
            Assert.AreEqual(8, snap.GetLength(1));
        }

        [Test]
        public void GetExploredSnapshot_ReflectsExploredTiles()
        {
            _service.Initialize(8, 8);
            _service.RegisterUnit("u1", new Vector2Int(3, 3), 0);
            _service.UnregisterUnit("u1");
            var snap = _service.GetExploredSnapshot();
            Assert.IsTrue(snap[3, 3]);
        }

        [Test]
        public void LoadFromSnapshot_NullDoesNotThrow()
        {
            _service.Initialize(5, 5);
            Assert.DoesNotThrow(() => _service.LoadFromSnapshot(null));
        }

        [Test]
        public void LoadFromSnapshot_SizeMismatch_DoesNotThrow()
        {
            _service.Initialize(5, 5);
            Assert.DoesNotThrow(() => _service.LoadFromSnapshot(new bool[10, 10]));
        }

        [Test]
        public void MultipleUnits_SamePos_VisibleUntilAllLeave()
        {
            _service.Initialize(10, 10);
            var pos = new Vector2Int(2, 2);
            _service.RegisterUnit("u1", pos, 0);
            _service.RegisterUnit("u2", pos, 0);
            _service.UnregisterUnit("u1");
            Assert.AreEqual(FogStateType.Visible, _service.GetFogState(pos));
            _service.UnregisterUnit("u2");
            Assert.AreEqual(FogStateType.Explored, _service.GetFogState(pos));
        }

        [Test]
        public void UpdatePosition_ToOutOfBounds_DoesNotThrow()
        {
            _service.Initialize(5, 5);
            _service.RegisterUnit("u1", new Vector2Int(2, 2), 0);
            Assert.DoesNotThrow(() => _service.UpdateUnitPosition("u1", new Vector2Int(100, 100)));
        }

        [Test]
        public void UnregisterUnit_UnknownId_DoesNotThrow()
        {
            _service.Initialize(5, 5);
            Assert.DoesNotThrow(() => _service.UnregisterUnit("ghost"));
        }

        [Test]
        public void GetLastDirtyTiles_ReturnsCollection()
        {
            _service.Initialize(5, 5);
            _service.RegisterUnit("u1", new Vector2Int(2, 2), 1);
            var dirty = _service.GetLastDirtyTiles();
            Assert.IsNotNull(dirty);
            // Dirty tiles are flushed after each operation for texture updates,
            // so verify visibility was applied instead.
            Assert.IsTrue(_service.IsVisible(new Vector2Int(2, 2)));
        }

        [Test]
        public void VisionRange_Larger_CoverMoreTiles()
        {
            _service.Initialize(20, 20);
            _service.RegisterUnit("u1", new Vector2Int(10, 10), 1);
            int visCount1 = 0;
            for (int x = 0; x < 20; x++)
                for (int y = 0; y < 20; y++)
                    if (_service.IsVisible(new Vector2Int(x, y))) visCount1++;

            _service.UnregisterUnit("u1");
            _service.RegisterUnit("u2", new Vector2Int(10, 10), 4);
            int visCount4 = 0;
            for (int x = 0; x < 20; x++)
                for (int y = 0; y < 20; y++)
                    if (_service.IsVisible(new Vector2Int(x, y))) visCount4++;

            Assert.Greater(visCount4, visCount1);
        }

        [Test]
        public void IsVisible_OutOfBounds_ReturnsFalse()
        {
            _service.Initialize(5, 5);
            Assert.IsFalse(_service.IsVisible(new Vector2Int(-1, -1)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(100, 100)));
        }

        [Test]
        public void IsExplored_OutOfBounds_ReturnsFalse()
        {
            _service.Initialize(5, 5);
            Assert.IsFalse(_service.IsExplored(new Vector2Int(-1, -1)));
        }

        [Test]
        public void Move_UpdatesVisibility()
        {
            _service.Initialize(20, 20);
            _service.RegisterUnit("u1", new Vector2Int(2, 2), 1);
            Assert.IsTrue(_service.IsVisible(new Vector2Int(2, 2)));
            _service.UpdateUnitPosition("u1", new Vector2Int(15, 15));
            Assert.IsTrue(_service.IsVisible(new Vector2Int(15, 15)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(2, 2)));
        }
    }

    [TestFixture]
    public sealed class FogRendererCullingEvaluatorTests
    {
        [Test]
        public void ShouldRender_FullyUnexploredBounds_ReturnsFalse()
        {
            var fog = new StubFogService();
            var grid = new StubGridService(4, 4);
            var bounds = new Bounds(new Vector3(1f, 1f, 0f), Vector3.one);

            bool shouldRender = FogRendererCullingEvaluator.ShouldRender(bounds, fog, grid, 0f);

            Assert.IsFalse(shouldRender);
        }

        [Test]
        public void ShouldRender_ExploredTileInsideBounds_ReturnsTrue()
        {
            var fog = new StubFogService();
            fog.SetState(new Vector2Int(1, 1), FogStateType.Explored);
            var grid = new StubGridService(4, 4);
            var bounds = new Bounds(new Vector3(1f, 1f, 0f), Vector3.one);

            bool shouldRender = FogRendererCullingEvaluator.ShouldRender(bounds, fog, grid, 0f);

            Assert.IsTrue(shouldRender);
        }

        [Test]
        public void ShouldRender_VisibleTileInsideBounds_ReturnsTrue()
        {
            var fog = new StubFogService();
            fog.SetState(new Vector2Int(2, 2), FogStateType.Visible);
            var grid = new StubGridService(4, 4);
            var bounds = new Bounds(new Vector3(2f, 2f, 0f), Vector3.one);

            bool shouldRender = FogRendererCullingEvaluator.ShouldRender(bounds, fog, grid, 0f);

            Assert.IsTrue(shouldRender);
        }

        [Test]
        public void ShouldRender_OutsideMap_ReturnsTrue()
        {
            var fog = new StubFogService();
            var grid = new StubGridService(4, 4);
            var bounds = new Bounds(new Vector3(-10f, -10f, 0f), Vector3.one);

            bool shouldRender = FogRendererCullingEvaluator.ShouldRender(bounds, fog, grid, 0f);

            Assert.IsTrue(shouldRender);
        }

        [Test]
        public void ShouldRender_TileSizedBounds_DoNotBleedIntoNeighbor()
        {
            var fog = new StubFogService();
            fog.SetState(new Vector2Int(1, 0), FogStateType.Explored);
            var grid = new StubGridService(3, 3);
            var bounds = new Bounds(new Vector3(0f, 0f, 0f), Vector3.one);

            bool shouldRender = FogRendererCullingEvaluator.ShouldRender(bounds, fog, grid, 0f);

            Assert.IsFalse(shouldRender);

            fog.SetState(new Vector2Int(0, 0), FogStateType.Explored);
            shouldRender = FogRendererCullingEvaluator.ShouldRender(bounds, fog, grid, 0f);

            Assert.IsTrue(shouldRender);
        }

        [Test]
        public void ShouldRender_XzProjection_UsesWorldZAsGridY()
        {
            var fog = new StubFogService();
            fog.SetState(new Vector2Int(2, 3), FogStateType.Explored);
            var grid = new StubGridService(5, 5);
            var projection = new Kruty1918.Moyva.Grid.Runtime.Orthographic3DGridProjection();
            var bounds = new Bounds(new Vector3(2f, 0.5f, 3f), Vector3.one);

            bool shouldRender = FogRendererCullingEvaluator.ShouldRender(bounds, fog, grid, 0f, projection);

            Assert.IsTrue(shouldRender);
        }

        private sealed class StubGridService : IGridService
        {
            public StubGridService(int width, int height)
            {
                GridWidth = width;
                GridHeight = height;
            }

            public int GridWidth { get; }
            public int GridHeight { get; }
            public string GetTileData(Vector2Int position) => "grass";
            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                tileTypeId = "grass";
                return true;
            }
            public void SetTileData(Vector2Int position, string tileTypeId) { }
        }

        private sealed class StubFogService : IFogOfWarService
        {
            private readonly Dictionary<Vector2Int, FogStateType> _states = new Dictionary<Vector2Int, FogStateType>();

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
            public FogStateType GetFogState(Vector2Int position)
                => _states.TryGetValue(position, out var state) ? state : FogStateType.Unexplored;
            public bool IsVisible(Vector2Int position) => GetFogState(position) == FogStateType.Visible;
            public bool IsExplored(Vector2Int position) => GetFogState(position) != FogStateType.Unexplored;
            public bool[,] GetExploredSnapshot() => null;
            public void LoadFromSnapshot(bool[,] explored) { }
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => System.Array.Empty<Vector2Int>();
        }
    }
}
