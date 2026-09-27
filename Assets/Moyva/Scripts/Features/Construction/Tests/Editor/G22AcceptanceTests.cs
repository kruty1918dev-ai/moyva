using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    /// <summary>
    /// G22 acceptance invariants for the build-grid overlay: every collected
    /// tile carries the canonical query verdict verbatim (Valid/Invalid/
    /// Unaffordable/General), Missing cells are skipped, only grid-covered
    /// tiles emit entries, and the mode controller never leaves a hover or
    /// selection behind on mode/selection changes.
    /// </summary>
    public sealed class G22AcceptanceTests
    {
        private sealed class FakeGridService : IGridService
        {
            private readonly Dictionary<Vector2Int, string> _tiles = new();
            public string GetTileData(Vector2Int position)
                => _tiles.TryGetValue(position, out var id) ? id : null;
            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
                => _tiles.TryGetValue(position, out tileTypeId);
            public void SetTileData(Vector2Int position, string tileTypeId)
                => _tiles[position] = tileTypeId;
            public int GridWidth => 100;
            public int GridHeight => 100;
        }

        private sealed class FakeGridGeometry : IConstructionGridGeometryService
        {
            public Vector2 CellSize = new Vector2(2f, 2f);
            public bool TryGetCellCenter(Vector2Int tile, out Vector3 center)
            {
                center = new Vector3(tile.x * CellSize.x, 0f, tile.y * CellSize.y);
                return true;
            }
            public bool TryGetCellSize(out Vector2 size)
            {
                size = CellSize;
                return true;
            }
            public bool TryGetCellAtWorld(Vector3 worldPosition, out Vector2Int tile)
            {
                tile = default;
                return false;
            }
            public bool TryGetGridPlaneY(out float y)
            {
                y = 0f;
                return false;
            }
        }

        private sealed class FakeTerrainQuery : IGeneratedTerrainLevelQuery
        {
            public bool HasExplicitTerrainSurfaceMap => false;
            public bool TryGetTerrainLevel(Vector2Int position, out int level)
            {
                level = 0;
                return true;
            }
            public bool TryGetTerrainSurfaceY(Vector2Int position, out float surfaceY)
            {
                surfaceY = 0f;
                return true;
            }
        }

        private FakeGridService _grid;
        private ConstructionBuildGridTileCollector _collector;

        [SetUp]
        public void SetUp()
        {
            _grid = new FakeGridService();
            var geometry = new FakeGridGeometry();
            var alignment = new ConstructionTerrainAlignmentService(
                _grid,
                gridProjection: null,
                gridGeometry: geometry,
                generatedTerrainLevelQuery: new FakeTerrainQuery(),
                terrainPassages: null);
            _collector = new ConstructionBuildGridTileCollector(
                _grid, alignment, geometry);
        }

        [Test]
        public void Collect_PreservesVisualState_Verbatim()
        {
            _grid.SetTileData(new Vector2Int(0, 0), "grass");
            _grid.SetTileData(new Vector2Int(1, 0), "grass");
            _grid.SetTileData(new Vector2Int(2, 0), "grass");
            _grid.SetTileData(new Vector2Int(3, 0), "grass");

            var verdicts = new Dictionary<Vector2Int, ConstructionBuildGridTileVisualState>
            {
                [new Vector2Int(0, 0)] = ConstructionBuildGridTileVisualState.Valid,
                [new Vector2Int(1, 0)] = ConstructionBuildGridTileVisualState.Invalid,
                [new Vector2Int(2, 0)] = ConstructionBuildGridTileVisualState.Unaffordable,
                [new Vector2Int(3, 0)] = ConstructionBuildGridTileVisualState.General,
            };
            var entries = new List<ConstructionBuildGridOverlayEntry>();
            _collector.Collect(entries, p => verdicts[p]);

            Assert.AreEqual(4, entries.Count);
            foreach (var e in entries)
                Assert.AreEqual(verdicts[e.Position], e.VisualState,
                    $"tile {e.Position} must carry the query verdict unchanged");
        }

        [Test]
        public void Collect_SkipsMissing_AndOutOfGridCells()
        {
            _grid.SetTileData(new Vector2Int(0, 0), "grass");
            _grid.SetTileData(new Vector2Int(1, 0), "grass");

            var entries = new List<ConstructionBuildGridOverlayEntry>();
            _collector.Collect(entries, p =>
                p == Vector2Int.zero
                    ? ConstructionBuildGridTileVisualState.Missing
                    : ConstructionBuildGridTileVisualState.Valid);

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual(new Vector2Int(1, 0), entries[0].Position);
            Assert.AreEqual(ConstructionBuildGridTileVisualState.Valid, entries[0].VisualState);
        }

        [Test]
        public void Collect_NullResolver_DefaultsToGeneral()
        {
            _grid.SetTileData(new Vector2Int(0, 0), "grass");
            var entries = new List<ConstructionBuildGridOverlayEntry>();
            _collector.Collect(entries, null);
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual(ConstructionBuildGridTileVisualState.General, entries[0].VisualState);
        }

        [Test]
        public void Controller_ModeGatesSelection_AndClearsHover()
        {
            var controller = new BuildModeGridStateController();
            // Selection impossible while hidden.
            Assert.IsFalse(controller.SetSelection("farm", false));
            Assert.IsNull(controller.SelectedBuildingId);

            Assert.IsTrue(controller.SetConstructionModeActive(true));
            Assert.AreEqual(BuildModeGridState.General, controller.State);

            Assert.IsTrue(controller.SetSelection("farm", false));
            Assert.AreEqual(BuildModeGridState.BuildingSelected, controller.State);
            Assert.AreEqual("farm", controller.SelectedBuildingId);

            Assert.IsTrue(controller.SetHover(
                new Vector2Int(3, 3), ConstructionBuildGridTileVisualState.Valid));
            Assert.AreEqual(new Vector2Int(3, 3), controller.HoverPosition);

            // Switching selection or leaving mode drops any stale hover —
            // the overlay must never show a verdict for the wrong target.
            controller.SetSelection("barn", false);
            Assert.IsFalse(controller.HoverPosition.HasValue,
                "selection change must clear the previous hover");

            Assert.IsTrue(controller.SetConstructionModeActive(false));
            Assert.AreEqual(BuildModeGridState.Hidden, controller.State);
            Assert.IsNull(controller.SelectedBuildingId);
            Assert.IsFalse(controller.HoverPosition.HasValue);
            // Hover while hidden is rejected.
            Assert.IsFalse(controller.SetHover(
                new Vector2Int(0, 0), ConstructionBuildGridTileVisualState.Valid));
        }

        [Test]
        public void Controller_MissingHover_Clears_AndDemolishClearsSelection()
        {
            var controller = new BuildModeGridStateController();
            controller.SetConstructionModeActive(true);
            controller.SetSelection("farm", false);
            controller.SetHover(new Vector2Int(1, 1),
                ConstructionBuildGridTileVisualState.Valid);

            // A resolver returning Missing (e.g. pointer over hidden cell)
            // clears the hover rather than freezing the last verdict.
            Assert.IsTrue(controller.SetHover(new Vector2Int(2, 2),
                ConstructionBuildGridTileVisualState.Missing));
            Assert.IsFalse(controller.HoverPosition.HasValue);

            // Demolish mode carries no selected building.
            controller.SetHover(new Vector2Int(1, 1),
                ConstructionBuildGridTileVisualState.Valid);
            Assert.IsTrue(controller.SetSelection("farm", isDemolishMode: true));
            Assert.AreEqual(BuildModeGridState.General, controller.State);
            Assert.IsNull(controller.SelectedBuildingId);
            Assert.IsFalse(controller.HoverPosition.HasValue);
        }
    }
}
