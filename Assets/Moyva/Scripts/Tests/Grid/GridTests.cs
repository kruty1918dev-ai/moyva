using System;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Grid
{
    [TestFixture]
    public sealed class GridServiceTests
    {
        private IGridService _grid;

        private IGridService CreateGrid(int w = 10, int h = 10)
        {
            var type = typeof(IGridService).Assembly.GetType("Kruty1918.Moyva.Grid.Runtime.GridService");
            Assert.IsNotNull(type, "GridService type not found");
            return (IGridService)Activator.CreateInstance(type, w, h);
        }

        [SetUp]
        public void SetUp()
        {
            _grid = CreateGrid();
        }

        // --- Dimensions ---
        [Test]
        public void GridWidth_ReturnsConstructorValue()
        {
            Assert.AreEqual(10, _grid.GridWidth);
        }

        [Test]
        public void GridHeight_ReturnsConstructorValue()
        {
            Assert.AreEqual(10, _grid.GridHeight);
        }

        [Test]
        public void CustomDimensions_AreRespected()
        {
            var g = CreateGrid(32, 64);
            Assert.AreEqual(32, g.GridWidth);
            Assert.AreEqual(64, g.GridHeight);
        }

        [Test]
        public void Resize_ChangesDimensions_AndPreservesOverlappingTiles()
        {
            _grid.SetTileData(new Vector2Int(2, 3), "grass");

            Assert.IsInstanceOf<IGridResizeService>(_grid);
            ((IGridResizeService)_grid).Resize(16, 12);

            Assert.AreEqual(16, _grid.GridWidth);
            Assert.AreEqual(12, _grid.GridHeight);
            Assert.AreEqual("grass", _grid.GetTileData(new Vector2Int(2, 3)));
            Assert.DoesNotThrow(() => _grid.SetTileData(new Vector2Int(15, 11), "water"));
        }

        // --- SetTileData / GetTileData ---
        [Test]
        public void SetTileData_GetTileData_ReturnsSameValue()
        {
            _grid.SetTileData(new Vector2Int(0, 0), "grass");
            Assert.AreEqual("grass", _grid.GetTileData(new Vector2Int(0, 0)));
        }

        [Test]
        public void GetTileData_Unset_ReturnsNull()
        {
            Assert.IsNull(_grid.GetTileData(new Vector2Int(0, 0)));
        }

        [Test]
        public void SetTileData_Override_ReturnsLatest()
        {
            _grid.SetTileData(new Vector2Int(3, 3), "grass");
            _grid.SetTileData(new Vector2Int(3, 3), "water");
            Assert.AreEqual("water", _grid.GetTileData(new Vector2Int(3, 3)));
        }

        [Test]
        public void SetTileData_MultiplePositions_Independent()
        {
            _grid.SetTileData(new Vector2Int(0, 0), "grass");
            _grid.SetTileData(new Vector2Int(1, 1), "water");
            Assert.AreEqual("grass", _grid.GetTileData(new Vector2Int(0, 0)));
            Assert.AreEqual("water", _grid.GetTileData(new Vector2Int(1, 1)));
        }

        [Test]
        public void SetTileData_Null_StoresNull()
        {
            _grid.SetTileData(new Vector2Int(0, 0), "grass");
            _grid.SetTileData(new Vector2Int(0, 0), null);
            Assert.IsNull(_grid.GetTileData(new Vector2Int(0, 0)));
        }

        [Test]
        public void SetTileData_AllCorners_WorkCorrectly()
        {
            _grid.SetTileData(new Vector2Int(0, 0), "a");
            _grid.SetTileData(new Vector2Int(9, 0), "b");
            _grid.SetTileData(new Vector2Int(0, 9), "c");
            _grid.SetTileData(new Vector2Int(9, 9), "d");

            Assert.AreEqual("a", _grid.GetTileData(new Vector2Int(0, 0)));
            Assert.AreEqual("b", _grid.GetTileData(new Vector2Int(9, 0)));
            Assert.AreEqual("c", _grid.GetTileData(new Vector2Int(0, 9)));
            Assert.AreEqual("d", _grid.GetTileData(new Vector2Int(9, 9)));
        }

        // --- Bounds checks ---
        [Test]
        public void GetTileData_NegativeX_ThrowsOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _grid.GetTileData(new Vector2Int(-1, 0)));
        }

        [Test]
        public void GetTileData_NegativeY_ThrowsOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _grid.GetTileData(new Vector2Int(0, -1)));
        }

        [Test]
        public void GetTileData_BeyondWidth_ThrowsOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _grid.GetTileData(new Vector2Int(10, 0)));
        }

        [Test]
        public void GetTileData_BeyondHeight_ThrowsOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _grid.GetTileData(new Vector2Int(0, 10)));
        }

        [Test]
        public void SetTileData_NegativePosition_ThrowsOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _grid.SetTileData(new Vector2Int(-1, -1), "x"));
        }

        [Test]
        public void SetTileData_BeyondBounds_ThrowsOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _grid.SetTileData(new Vector2Int(10, 10), "x"));
        }

        // --- TryGetTileData ---
        [Test]
        public void TryGetTileData_ValidPosition_ReturnsTrue()
        {
            _grid.SetTileData(new Vector2Int(5, 5), "stone");
            Assert.IsTrue(_grid.TryGetTileData(new Vector2Int(5, 5), out var data));
            Assert.AreEqual("stone", data);
        }

        [Test]
        public void TryGetTileData_UnsetPosition_ReturnsTrueWithNull()
        {
            Assert.IsTrue(_grid.TryGetTileData(new Vector2Int(0, 0), out var data));
            Assert.IsNull(data);
        }

        [Test]
        public void TryGetTileData_OutOfBounds_ReturnsFalse()
        {
            Assert.IsFalse(_grid.TryGetTileData(new Vector2Int(-1, 0), out _));
        }

        [Test]
        public void TryGetTileData_BeyondWidth_ReturnsFalse()
        {
            Assert.IsFalse(_grid.TryGetTileData(new Vector2Int(10, 5), out _));
        }

        [Test]
        public void TryGetTileData_BeyondHeight_ReturnsFalse()
        {
            Assert.IsFalse(_grid.TryGetTileData(new Vector2Int(5, 10), out _));
        }

        // --- Edge cases ---
        [Test]
        public void SmallGrid_1x1_Works()
        {
            var g = CreateGrid(1, 1);
            g.SetTileData(new Vector2Int(0, 0), "x");
            Assert.AreEqual("x", g.GetTileData(new Vector2Int(0, 0)));
        }

        [Test]
        public void LargeGrid_256x256_DoesNotThrow()
        {
            var g = CreateGrid(256, 256);
            g.SetTileData(new Vector2Int(255, 255), "edge");
            Assert.AreEqual("edge", g.GetTileData(new Vector2Int(255, 255)));
        }

        [Test]
        public void FillEntireGrid_AllTilesAccessible()
        {
            var g = CreateGrid(5, 5);
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    g.SetTileData(new Vector2Int(x, y), $"{x}_{y}");

            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    Assert.AreEqual($"{x}_{y}", g.GetTileData(new Vector2Int(x, y)));
        }
    }

    [TestFixture]
    public sealed class GridProjectionTests
    {
        [Test]
        public void OrthogonalProjection_GridToWorld_MatchesLegacyPositioning()
        {
            var projection = new Kruty1918.Moyva.Grid.Runtime.OrthogonalGridProjection();

            Assert.AreEqual(new Vector3(2f, 3f, 0f), projection.GridToWorld(new Vector2Int(2, 3)));
        }

        [Test]
        public void OrthogonalProjection_WorldToGrid_MatchesLegacyRounding()
        {
            var projection = new Kruty1918.Moyva.Grid.Runtime.OrthogonalGridProjection();

            Assert.AreEqual(new Vector2Int(2, 3), projection.WorldToGrid(new Vector3(2.2f, 3.4f, 0f)));
        }

        [Test]
        public void IsometricProjection_RoundTripsGridCoordinates()
        {
            var projection = new Kruty1918.Moyva.Grid.Runtime.IsometricGridProjection();
            var grid = new Vector2Int(7, 4);

            Assert.AreEqual(grid, projection.WorldToGrid(projection.GridToWorld(grid)));
        }

        [Test]
        public void Isometric3DPreviewProjection_PreservesConfiguredMode()
        {
            var settings = ScriptableObject.CreateInstance<MoyvaProjectSettingsSO>();
            settings.DefaultProjectionMode = GridProjectionMode.Isometric3DPreview;
            settings.OrthogonalCellWidth = 2f;
            settings.OrthogonalCellDepth = 3f;
            settings.HeightScale = 0.5f;
            settings.Normalize();

            var projection = new Kruty1918.Moyva.Grid.Runtime.IsometricGridProjection(settings);
            var grid = new Vector2Int(2, 3);
            Vector3 world = projection.GridToWorld(grid, elevation: 3f);

            Assert.AreEqual(GridProjectionMode.Isometric3DPreview, projection.ProjectionMode);
            Assert.AreEqual(GridWorldPlane.XZ, projection.WorldPlane);
            Assert.AreEqual(GridTopology.Layered, projection.Topology);
            Assert.AreEqual(new Vector3(4f, 1.5f, 9f), world);
            Assert.AreEqual(grid, projection.WorldToGrid(world));
            Assert.AreEqual(GridRenderMode.Mesh3DPreview, ResolveRenderMode(projection.ProjectionMode));
        }

        [Test]
        public void HexProjection_RoundTripsAxialCoordinates()
        {
            var projection = new Kruty1918.Moyva.Grid.Runtime.HexAxialGridProjection();
            var grid = new Vector2Int(3, -2);

            Assert.AreEqual(grid, projection.WorldToGrid(projection.GridToWorld(grid)));
        }

        [Test]
        public void Orthographic3DProjection_UsesXZPlaneAndYHeight()
        {
            var settings = ScriptableObject.CreateInstance<MoyvaProjectSettingsSO>();
            settings.OrthogonalCellWidth = 2f;
            settings.OrthogonalCellDepth = 3f;
            settings.HeightScale = 0.5f;
            settings.Normalize();
            var projection = new Kruty1918.Moyva.Grid.Runtime.Orthographic3DGridProjection(settings);

            Assert.AreEqual(GridWorldPlane.XZ, projection.WorldPlane);
            Assert.AreEqual(new Vector3(4f, 1.5f, 9f), projection.GridToWorld(new Vector2Int(2, 3), elevation: 3f));
        }

        [Test]
        public void Orthographic3DProjection_RoundTripsGridCoordinates()
        {
            var projection = new Kruty1918.Moyva.Grid.Runtime.Orthographic3DGridProjection();
            var grid = new Vector2Int(5, 8);

            Assert.AreEqual(grid, projection.WorldToGrid(projection.GridToWorld(grid)));
        }

        private static GridRenderMode ResolveRenderMode(GridProjectionMode projectionMode)
        {
            return projectionMode switch
            {
                GridProjectionMode.Orthographic3D => GridRenderMode.Mesh3D,
                GridProjectionMode.Isometric2D => GridRenderMode.Isometric2D,
                GridProjectionMode.Isometric3DPreview => GridRenderMode.Mesh3DPreview,
                _ => GridRenderMode.Sprite2D,
            };
        }
    }

    [TestFixture]
    public sealed class TileSettingsServiceTests
    {
        private ITileSettingsService CreateService(params (string id, float cost)[] tiles)
        {
            var registry = ScriptableObject.CreateInstance<TileRegistrySO>();

            // Use SerializedObject to set private _definitions field
            var so = new UnityEditor.SerializedObject(registry);
            var prop = so.FindProperty("_definitions");
            prop.arraySize = tiles.Length;
            for (int i = 0; i < tiles.Length; i++)
            {
                var elem = prop.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("_id").stringValue = tiles[i].id;
                elem.FindPropertyRelative("_movementCost").floatValue = tiles[i].cost;
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            var type = typeof(ITileSettingsService).Assembly
                .GetType("Kruty1918.Moyva.Grid.Runtime.TileSettingsService");
            return (ITileSettingsService)Activator.CreateInstance(type, new object[] { registry });
        }

        [Test]
        public void GetTileWeight_KnownTile_ReturnsCost()
        {
            var svc = CreateService(("grass", 1.0f), ("water", 3.0f));
            Assert.AreEqual(1.0f, svc.GetTileWeight("grass"));
            Assert.AreEqual(3.0f, svc.GetTileWeight("water"));
        }

        [Test]
        public void GetTileWeight_UnknownTile_ReturnsZero()
        {
            var svc = CreateService(("grass", 1.0f));
            Assert.AreEqual(0f, svc.GetTileWeight("unknown"));
        }

        [Test]
        public void GetTileWeight_NullId_ReturnsZero()
        {
            var svc = CreateService(("grass", 1.0f));
            Assert.AreEqual(0f, svc.GetTileWeight(null));
        }

        [Test]
        public void GetTileWeight_EmptyId_ReturnsZero()
        {
            var svc = CreateService(("grass", 1.0f));
            Assert.AreEqual(0f, svc.GetTileWeight(""));
        }

        [Test]
        public void GetTileWeight_MultipleTiles_EachReturnsOwnCost()
        {
            var svc = CreateService(("grass", 1f), ("sand", 1.5f), ("mountain", 5f), ("road", 0.5f));
            Assert.AreEqual(1f, svc.GetTileWeight("grass"));
            Assert.AreEqual(1.5f, svc.GetTileWeight("sand"));
            Assert.AreEqual(5f, svc.GetTileWeight("mountain"));
            Assert.AreEqual(0.5f, svc.GetTileWeight("road"));
        }
    }
}
