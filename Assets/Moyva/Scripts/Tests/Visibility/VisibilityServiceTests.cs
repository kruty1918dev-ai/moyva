using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using Kruty1918.Moyva.Visibility.API;
using Kruty1918.Moyva.Visibility.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Visibility
{
    /// <summary>
    /// Юніт-тести для VisibilityService.
    /// Перевіряє логіку лічильникової сітки видимості.
    /// </summary>
    [TestFixture]
    public class VisibilityServiceTests : ZenjectUnitTestFixture
    {
        private IVisibilityService _service;
        private VisibilityService _serviceImpl;
        private SignalBus _signalBus;

        private const int GridWidth = 10;
        private const int GridHeight = 10;
        private const int DefaultVisionRadius = 2;

        public override void Setup()
        {
            base.Setup();

            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<OnVisibilityChangedSignal>();

            Container.BindInstance<IGridService>(new StubGridService(GridWidth, GridHeight)).AsSingle();
            Container.BindInstance<IUnitClassConfig>(new StubUnitClassConfig(DefaultVisionRadius)).AsSingle();

            Container.BindInterfacesAndSelfTo<VisibilityService>().AsSingle().NonLazy();

            _signalBus = Container.Resolve<SignalBus>();
            _service = Container.Resolve<IVisibilityService>();
            _serviceImpl = Container.Resolve<VisibilityService>();
            _serviceImpl.Initialize();
        }

        public override void Teardown()
        {
            _serviceImpl.Dispose();
            base.Teardown();
        }

        // ─── Початковий стан ─────────────────────────────────────────────────

        [Test]
        public void InitialState_AllTilesAreInvisible()
        {
            for (int x = 0; x < GridWidth; x++)
            for (int y = 0; y < GridHeight; y++)
            {
                Assert.IsFalse(_service.IsVisible(new Vector2Int(x, y)),
                    $"Тайл ({x},{y}) має бути в тумані при старті");
            }
        }

        [Test]
        public void InitialState_AllCountersAreZero()
        {
            for (int x = 0; x < GridWidth; x++)
            for (int y = 0; y < GridHeight; y++)
                Assert.AreEqual(0, _service.GetVisibilityCount(new Vector2Int(x, y)));
        }

        // ─── UnitCreated ─────────────────────────────────────────────────────

        [Test]
        public void UnitCreated_TilesInRadiusBecomeVisible()
        {
            var center = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", center);

            Assert.IsTrue(_service.IsVisible(center));
            // Всі тайли в квадратному радіусі DefaultVisionRadius мають бути видимі
            for (int dx = -DefaultVisionRadius; dx <= DefaultVisionRadius; dx++)
            for (int dy = -DefaultVisionRadius; dy <= DefaultVisionRadius; dy++)
                Assert.IsTrue(_service.IsVisible(new Vector2Int(center.x + dx, center.y + dy)),
                    $"Тайл ({center.x + dx},{center.y + dy}) має бути видимий");
        }

        [Test]
        public void UnitCreated_TilesOutsideRadiusRemainInvisible()
        {
            var center = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", center);

            // Тайл далі за радіус має бути в тумані
            var farTile = new Vector2Int(center.x + DefaultVisionRadius + 1, center.y);
            Assert.IsFalse(_service.IsVisible(farTile));
        }

        [Test]
        public void UnitCreated_CounterEqualsOne_ForTilesInRadius()
        {
            var center = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", center);

            Assert.AreEqual(1, _service.GetVisibilityCount(center));
        }

        // ─── UnitMoved ───────────────────────────────────────────────────────

        [Test]
        public void UnitMoved_OldTilesLoseVisibility_NewTilesGainVisibility()
        {
            var startPos = new Vector2Int(1, 1);
            var newPos = new Vector2Int(8, 8);
            FireUnitCreated("unit-01", "warrior", startPos);
            FireUnitMoved("unit-01", newPos);

            // Тайл у центрі старої позиції за радіусом startPos не перекривається з newPos
            Assert.IsFalse(_service.IsVisible(startPos), "Стара позиція має бути в тумані");
            Assert.IsTrue(_service.IsVisible(newPos), "Нова позиція має бути видима");
        }

        [Test]
        public void UnitMoved_PositionIsTrackedCorrectly()
        {
            var startPos = new Vector2Int(5, 5);
            var midPos = new Vector2Int(5, 6);
            var finalPos = new Vector2Int(5, 7);

            FireUnitCreated("unit-01", "warrior", startPos);
            FireUnitMoved("unit-01", midPos);
            FireUnitMoved("unit-01", finalPos);

            // finalPos центр має бути видимий
            Assert.IsTrue(_service.IsVisible(finalPos));
        }

        // ─── UnitDestroyed ───────────────────────────────────────────────────

        [Test]
        public void UnitDestroyed_TilesBecomeFog()
        {
            var pos = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", pos);
            Assert.IsTrue(_service.IsVisible(pos));

            FireUnitDestroyed("unit-01");
            Assert.IsFalse(_service.IsVisible(pos));
        }

        [Test]
        public void UnitDestroyed_CounterDropsToZero()
        {
            var pos = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", pos);
            FireUnitDestroyed("unit-01");

            Assert.AreEqual(0, _service.GetVisibilityCount(pos));
        }

        // ─── Два юніти — спільний тайл ───────────────────────────────────────

        [Test]
        public void TwoUnits_SharedTile_CounterIsTwo()
        {
            // Два юніти дивляться на той самий тайл
            var pos = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", pos);
            FireUnitCreated("unit-02", "warrior", pos);

            Assert.AreEqual(2, _service.GetVisibilityCount(pos));
        }

        [Test]
        public void TwoUnits_OneLeaves_TileStillVisible()
        {
            var pos = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", pos);
            FireUnitCreated("unit-02", "warrior", pos);

            // unit-01 пішов далеко
            FireUnitMoved("unit-01", new Vector2Int(0, 0));

            // Тайл все ще видимий завдяки unit-02
            Assert.IsTrue(_service.IsVisible(pos), "Тайл має бути видимий бо unit-02 все ще там");
            Assert.AreEqual(1, _service.GetVisibilityCount(pos));
        }

        [Test]
        public void TwoUnits_BothLeave_TileBecomesFog()
        {
            var pos = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", pos);
            FireUnitCreated("unit-02", "warrior", pos);

            FireUnitDestroyed("unit-01");
            FireUnitDestroyed("unit-02");

            Assert.IsFalse(_service.IsVisible(pos));
            Assert.AreEqual(0, _service.GetVisibilityCount(pos));
        }

        // ─── Сигнали видимості ───────────────────────────────────────────────

        [Test]
        public void OnVisibilityChangedSignal_FiredWhenTileBecomesVisible()
        {
            var fired = new List<OnVisibilityChangedSignal>();
            _signalBus.Subscribe<OnVisibilityChangedSignal>(s => fired.Add(s));

            var pos = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", pos);

            Assert.IsTrue(fired.Count > 0, "Сигнал OnVisibilityChangedSignal має бути надіслано");
            Assert.IsTrue(fired.Exists(s => s.Position == pos && s.IsVisible),
                "Має бути сигнал з IsVisible=true для центрального тайлу");
        }

        [Test]
        public void OnVisibilityChangedSignal_FiredWhenTileBecomesFog()
        {
            var pos = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", pos);

            var fired = new List<OnVisibilityChangedSignal>();
            _signalBus.Subscribe<OnVisibilityChangedSignal>(s => fired.Add(s));

            FireUnitDestroyed("unit-01");

            Assert.IsTrue(fired.Count > 0);
            Assert.IsTrue(fired.Exists(s => s.Position == pos && !s.IsVisible),
                "Має бути сигнал з IsVisible=false для центрального тайлу");
        }

        [Test]
        public void OnVisibilityChangedSignal_NotFiredWhenCounterGoesFromOneToTwo()
        {
            var pos = new Vector2Int(5, 5);
            FireUnitCreated("unit-01", "warrior", pos);

            // Підписуємось ПІСЛЯ першого юніта
            var fired = new List<OnVisibilityChangedSignal>();
            _signalBus.Subscribe<OnVisibilityChangedSignal>(s => fired.Add(s));

            // Другий юніт дивиться на той же центр — лічильник 1→2, сигнал НЕ має надсилатись
            FireUnitCreated("unit-02", "warrior", pos);

            Assert.IsFalse(fired.Exists(s => s.Position == pos && s.IsVisible),
                "Сигнал не має надсилатись при переході лічильника 1→2");
        }

        // ─── Save / Load ─────────────────────────────────────────────────────

        [Test]
        public void GetRawGrid_ReturnsCopyOfCurrentState()
        {
            var pos = new Vector2Int(3, 3);
            FireUnitCreated("unit-01", "warrior", pos);

            var grid = _service.GetRawGrid();

            Assert.AreEqual(GridWidth, grid.GetLength(0));
            Assert.AreEqual(GridHeight, grid.GetLength(1));
            Assert.Greater(grid[pos.x, pos.y], 0);
        }

        [Test]
        public void GetRawGrid_IsACopy_NotReference()
        {
            var pos = new Vector2Int(3, 3);
            FireUnitCreated("unit-01", "warrior", pos);

            var grid = _service.GetRawGrid();
            int original = grid[pos.x, pos.y];
            grid[pos.x, pos.y] = 999; // Змінюємо копію

            Assert.AreEqual(original, _service.GetVisibilityCount(pos),
                "Зміна копії не має впливати на внутрішній стан сервісу");
        }

        [Test]
        public void LoadFromGrid_RestoresVisibilityState()
        {
            var manualGrid = new int[GridWidth, GridHeight];
            manualGrid[2, 4] = 1;
            manualGrid[7, 7] = 3;

            _service.LoadFromGrid(manualGrid);

            Assert.IsTrue(_service.IsVisible(new Vector2Int(2, 4)));
            Assert.AreEqual(3, _service.GetVisibilityCount(new Vector2Int(7, 7)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(0, 0)));
        }

        // ─── Межі сітки ──────────────────────────────────────────────────────

        [Test]
        public void IsVisible_OutOfBoundsPosition_ReturnsFalse()
        {
            Assert.IsFalse(_service.IsVisible(new Vector2Int(-1, 0)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(0, -1)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(GridWidth, 0)));
            Assert.IsFalse(_service.IsVisible(new Vector2Int(0, GridHeight)));
        }

        [Test]
        public void GetVisibilityCount_OutOfBoundsPosition_ReturnsZero()
        {
            Assert.AreEqual(0, _service.GetVisibilityCount(new Vector2Int(-1, -1)));
            Assert.AreEqual(0, _service.GetVisibilityCount(new Vector2Int(GridWidth, GridHeight)));
        }

        [Test]
        public void UnitAtCorner_VisionRadiusClampedToGrid()
        {
            // Юніт на кутовому тайлі — частина радіусу виходить за межі сітки
            var cornerPos = new Vector2Int(0, 0);
            Assert.DoesNotThrow(() => FireUnitCreated("unit-01", "warrior", cornerPos));
            Assert.IsTrue(_service.IsVisible(cornerPos));
        }

        // ─── Допоміжні методи ────────────────────────────────────────────────

        private void FireUnitCreated(string unitId, string typeId, Vector2Int position)
        {
            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = unitId,
                UnitTypeId = typeId,
                Position = position,
                UnitObject = null
            });
        }

        private void FireUnitMoved(string unitId, Vector2Int newPosition)
        {
            _signalBus.Fire(new UnitMovedSignal
            {
                UnitId = unitId,
                NewPosition = newPosition,
                Cost = 1f
            });
        }

        private void FireUnitDestroyed(string unitId)
        {
            _signalBus.Fire(new UnitDestroyedSignal { UnitId = unitId });
        }

        // ─── Стаби для тестів ────────────────────────────────────────────────

        private sealed class StubGridService : IGridService
        {
            public int GridWidth { get; }
            public int GridHeight { get; }

            public StubGridService(int width, int height)
            {
                GridWidth = width;
                GridHeight = height;
            }

            public string GetTileData(Vector2Int position) => "grass";
            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                tileTypeId = "grass";
                return true;
            }
            public void SetTileData(Vector2Int position, string tileTypeId) { }
        }

        private sealed class StubUnitClassConfig : IUnitClassConfig
        {
            private readonly int _visionRadius;

            public StubUnitClassConfig(int visionRadius) => _visionRadius = visionRadius;

            public UnitClassConfig GetConfig(string typeId) =>
                new UnitClassConfig { TypeId = typeId, VisionRadius = _visionRadius };
        }
    }
}
