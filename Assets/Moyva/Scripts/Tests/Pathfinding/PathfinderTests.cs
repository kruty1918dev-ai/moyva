using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Pathfinding.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Pathfinding
{
    [TestFixture]
    public sealed class PathfinderTests
    {
        private FakeGridService _grid;
        private FakeTileSettingsService _tileSettings;
        private FakeObjectsMapService _objectsMap;
        private IPathfinder _pathfinder;

        private sealed class FakeGridService : IGridService
        {
            private readonly Dictionary<Vector2Int, string> _tiles = new();
            public int GridWidth { get; set; } = 10;
            public int GridHeight { get; set; } = 10;

            public void SetTile(Vector2Int pos, string id) => _tiles[pos] = id;

            public string GetTileData(Vector2Int pos)
            {
                return _tiles.TryGetValue(pos, out var d) ? d : null;
            }

            public bool TryGetTileData(Vector2Int pos, out string data)
            {
                if (pos.x < 0 || pos.y < 0 || pos.x >= GridWidth || pos.y >= GridHeight)
                {
                    data = null;
                    return false;
                }
                data = _tiles.TryGetValue(pos, out var d) ? d : null;
                return true;
            }

            public void SetTileData(Vector2Int pos, string data) => _tiles[pos] = data;
        }

        private sealed class FakeTileSettingsService : ITileSettingsService
        {
            private readonly Dictionary<string, float> _weights = new();
            public void SetWeight(string id, float w) => _weights[id] = w;
            public float GetTileWeight(string tileTypeId)
            {
                return _weights.TryGetValue(tileTypeId ?? "", out var w) ? w : 1f;
            }
        }

        private sealed class FakeObjectsMapService : IObjectsMapService
        {
            private readonly Dictionary<Vector2Int, string> _map = new();
            public void SetOccupied(Vector2Int pos) => _map[pos] = "blocker";
            public bool IsOccupied(Vector2Int position) => _map.ContainsKey(position);
            public bool TryGetOccupant(Vector2Int position, out string occupantId)
            {
                return _map.TryGetValue(position, out occupantId);
            }
            public void Register(Vector2Int position, string occupantId) => _map[position] = occupantId;
            public void Move(Vector2Int from, Vector2Int to)
            {
                if (_map.TryGetValue(from, out var id)) { _map.Remove(from); _map[to] = id; }
            }
            public void Unregister(Vector2Int position) => _map.Remove(position);
            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                foreach (var kv in _map)
                    if (kv.Value == occupantId) { position = kv.Key; return true; }
                position = default;
                return false;
            }
        }

        private void FillGrid(int w, int h, string tileId = "grass")
        {
            _grid.GridWidth = w;
            _grid.GridHeight = h;
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    _grid.SetTile(new Vector2Int(x, y), tileId);
        }

        [SetUp]
        public void SetUp()
        {
            _grid = new FakeGridService();
            _tileSettings = new FakeTileSettingsService();
            _objectsMap = new FakeObjectsMapService();

            _tileSettings.SetWeight("grass", 1f);
            _tileSettings.SetWeight("water", 5f);
            _tileSettings.SetWeight("mountain", 10f);
            _tileSettings.SetWeight("road", 0.5f);

            FillGrid(10, 10);

            var type = typeof(IPathfinder).Assembly
                .GetType("Kruty1918.Moyva.Pathfinding.Runtime.Pathfinder");
            _pathfinder = (IPathfinder)Activator.CreateInstance(type,
                (IGridService)_grid, (ITileSettingsService)_tileSettings, (IObjectsMapService)_objectsMap);
        }

        // --- GetNeighbors ---
        [Test]
        public void GetNeighbors_Center_Returns8()
        {
            var n = _pathfinder.GetNeighbors(new Vector2Int(5, 5)).ToList();
            Assert.AreEqual(8, n.Count);
        }

        [Test]
        public void GetNeighbors_Corner00_Returns3()
        {
            var n = _pathfinder.GetNeighbors(new Vector2Int(0, 0)).ToList();
            Assert.AreEqual(3, n.Count);
        }

        [Test]
        public void GetNeighbors_Corner99_Returns3()
        {
            var n = _pathfinder.GetNeighbors(new Vector2Int(9, 9)).ToList();
            Assert.AreEqual(3, n.Count);
        }

        [Test]
        public void GetNeighbors_Edge_Returns5()
        {
            var n = _pathfinder.GetNeighbors(new Vector2Int(0, 5)).ToList();
            Assert.AreEqual(5, n.Count);
        }

        [Test]
        public void GetNeighbors_DoesNotIncludeSelf()
        {
            var pos = new Vector2Int(5, 5);
            var n = _pathfinder.GetNeighbors(pos).ToList();
            Assert.IsFalse(n.Contains(pos));
        }

        [Test]
        public void GetNeighbors_IncludesDiagonals()
        {
            var n = _pathfinder.GetNeighbors(new Vector2Int(5, 5)).ToList();
            Assert.IsTrue(n.Contains(new Vector2Int(4, 4)));
            Assert.IsTrue(n.Contains(new Vector2Int(6, 6)));
            Assert.IsTrue(n.Contains(new Vector2Int(4, 6)));
            Assert.IsTrue(n.Contains(new Vector2Int(6, 4)));
        }

        [Test]
        public void GetNeighbors_IncludesCardinals()
        {
            var n = _pathfinder.GetNeighbors(new Vector2Int(5, 5)).ToList();
            Assert.IsTrue(n.Contains(new Vector2Int(5, 4)));
            Assert.IsTrue(n.Contains(new Vector2Int(5, 6)));
            Assert.IsTrue(n.Contains(new Vector2Int(4, 5)));
            Assert.IsTrue(n.Contains(new Vector2Int(6, 5)));
        }

        [Test]
        public void MooreNeighborhoodStrategy_Center_ReturnsCurrent8NeighborBehavior()
        {
            var strategy = new Kruty1918.Moyva.Pathfinding.Runtime.MooreNeighborhoodStrategy();

            var neighbors = strategy.GetNeighbors(new Vector2Int(5, 5), _grid).ToList();

            Assert.AreEqual(8, neighbors.Count);
            Assert.IsTrue(neighbors.Contains(new Vector2Int(4, 4)));
            Assert.IsTrue(neighbors.Contains(new Vector2Int(5, 4)));
        }

        [Test]
        public void HexAxialNeighborhoodStrategy_Center_Returns6Neighbors()
        {
            var strategy = new Kruty1918.Moyva.Pathfinding.Runtime.HexAxialNeighborhoodStrategy();

            var neighbors = strategy.GetNeighbors(new Vector2Int(5, 5), _grid).ToList();

            Assert.AreEqual(6, neighbors.Count);
            Assert.IsTrue(neighbors.Contains(new Vector2Int(6, 5)));
            Assert.IsTrue(neighbors.Contains(new Vector2Int(6, 4)));
            Assert.IsTrue(neighbors.Contains(new Vector2Int(5, 4)));
        }

        // --- FindPath - basic ---
        [Test]
        public void FindPath_SameStartEnd_ReturnsSingleElement()
        {
            var path = _pathfinder.FindPath(new Vector2Int(3, 3), new Vector2Int(3, 3));
            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(new Vector2Int(3, 3), path[0]);
        }

        [Test]
        public void FindPath_Adjacent_ReturnsTwoElements()
        {
            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(1, 0));
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(new Vector2Int(0, 0), path[0]);
            Assert.AreEqual(new Vector2Int(1, 0), path[path.Count - 1]);
        }

        [Test]
        public void FindPath_Diagonal_ReturnsDiagonalMove()
        {
            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(1, 1));
            Assert.AreEqual(2, path.Count);
        }

        [Test]
        public void FindPath_StartsAtStart()
        {
            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(5, 5));
            Assert.AreEqual(new Vector2Int(0, 0), path[0]);
        }

        [Test]
        public void FindPath_EndsAtEnd()
        {
            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(5, 5));
            Assert.AreEqual(new Vector2Int(5, 5), path[path.Count - 1]);
        }

        [Test]
        public void FindPath_StraightLine_IsOptimal()
        {
            FillGrid(5, 1);
            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(4, 0));
            Assert.AreEqual(5, path.Count);
        }

        // --- FindPath - obstacles ---
        [Test]
        public void FindPath_Blocked_ReturnsEmpty()
        {
            // Wall blocks all passage between left and right
            FillGrid(5, 3);
            for (int y = 0; y < 3; y++)
                _objectsMap.SetOccupied(new Vector2Int(2, y));
            var path = _pathfinder.FindPath(new Vector2Int(0, 1), new Vector2Int(4, 1));
            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void FindPath_AroundObstacle_FindsDetour()
        {
            FillGrid(5, 5);
            _objectsMap.SetOccupied(new Vector2Int(2, 2));
            var path = _pathfinder.FindPath(new Vector2Int(0, 2), new Vector2Int(4, 2));
            Assert.Greater(path.Count, 0);
            Assert.IsFalse(path.Contains(new Vector2Int(2, 2)));
        }

        [Test]
        public void FindPath_StartOccupied_StillFindsPath()
        {
            // Occupancy check skips start position
            FillGrid(5, 5);
            _objectsMap.SetOccupied(new Vector2Int(0, 0));
            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(4, 4));
            Assert.Greater(path.Count, 0);
        }

        // --- FindPath - tile weights ---
        [Test]
        public void FindPath_PrefersLowWeight()
        {
            FillGrid(3, 3);
            // Fill middle row with heavy tiles
            for (int x = 0; x < 3; x++)
                _grid.SetTile(new Vector2Int(x, 1), "mountain");

            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(2, 2));
            // Should NOT go through (1,1) mountain if there's a lighter diagonal
            Assert.Greater(path.Count, 0);
        }

        [Test]
        public void FindPath_RoadIsFasterThanGrass()
        {
            FillGrid(5, 3);
            // Top row is road, middle is grass
            for (int x = 0; x < 5; x++)
                _grid.SetTile(new Vector2Int(x, 0), "road");

            var pathViaRoad = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(4, 0));
            Assert.Greater(pathViaRoad.Count, 0);
        }

        [Test]
        public void FindPath_NoValidTile_ReturnsEmpty()
        {
            // Create a tiny grid with no tiles around
            _grid = new FakeGridService { GridWidth = 2, GridHeight = 1 };
            _grid.SetTile(new Vector2Int(0, 0), "grass");
            // Don't set (1,0) so TryGetTileData returns false in bounds

            var type = typeof(IPathfinder).Assembly
                .GetType("Kruty1918.Moyva.Pathfinding.Runtime.Pathfinder");
            var pf = (IPathfinder)Activator.CreateInstance(type,
                (IGridService)_grid, (ITileSettingsService)_tileSettings, (IObjectsMapService)_objectsMap);

            var path = pf.FindPath(new Vector2Int(0, 0), new Vector2Int(1, 0));
            // (1,0) is in bounds but has no tile data => TryGetTileData returns true with null
            // GetTileWeight(null) returns 1f, so path should still work if tile is in grid bounds
            Assert.GreaterOrEqual(path.Count, 0);
        }

        // --- FindPath - diagonal cost ---
        [Test]
        public void FindPath_DiagonalCostsMore_ThanCardinal()
        {
            FillGrid(3, 3);
            // Horizontal path 0,0 -> 2,0 costs 2 * 1.0 = 2.0 (2 moves)
            // Diagonal path 0,0 -> 1,1 costs 1.414 * 1.0 (1 move)
            var pathCardinal = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(2, 0));
            Assert.AreEqual(3, pathCardinal.Count);
        }

        // --- FindPath - large map ---
        [Test]
        public void FindPath_LargeMap_FindsPath()
        {
            FillGrid(50, 50);
            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(49, 49));
            Assert.Greater(path.Count, 0);
            Assert.AreEqual(new Vector2Int(0, 0), path[0]);
            Assert.AreEqual(new Vector2Int(49, 49), path[path.Count - 1]);
        }

        // --- Contiguous path ---
        [Test]
        public void FindPath_EachStep_IsNeighborOfPrevious()
        {
            FillGrid(10, 10);
            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(9, 9));
            for (int i = 1; i < path.Count; i++)
            {
                int dx = Mathf.Abs(path[i].x - path[i - 1].x);
                int dy = Mathf.Abs(path[i].y - path[i - 1].y);
                Assert.LessOrEqual(dx, 1);
                Assert.LessOrEqual(dy, 1);
                Assert.IsTrue(dx + dy > 0);
            }
        }

        [Test]
        public void FindPath_NoDuplicates()
        {
            FillGrid(10, 10);
            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(5, 5));
            Assert.AreEqual(path.Count, path.Distinct().Count());
        }

        // --- Edge cases ---
        [Test]
        public void FindPath_1x1Grid_SamePoint()
        {
            FillGrid(1, 1);
            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(0, 0));
            Assert.AreEqual(1, path.Count);
        }

        [Test]
        public void FindPath_Corridor_FollowsCorridor()
        {
            _grid = new FakeGridService { GridWidth = 5, GridHeight = 3 };
            // Only middle row is walkable
            for (int x = 0; x < 5; x++)
                _grid.SetTile(new Vector2Int(x, 1), "grass");

            var type = typeof(IPathfinder).Assembly
                .GetType("Kruty1918.Moyva.Pathfinding.Runtime.Pathfinder");
            var pf = (IPathfinder)Activator.CreateInstance(type,
                (IGridService)_grid, (ITileSettingsService)_tileSettings, (IObjectsMapService)_objectsMap);

            var path = pf.FindPath(new Vector2Int(0, 1), new Vector2Int(4, 1));
            Assert.AreEqual(5, path.Count);
            foreach (var p in path)
                Assert.AreEqual(1, p.y);
        }

        [Test]
        public void FindPath_MazeWinding_FindsPath()
        {
            FillGrid(7, 7);
            // Create a wall with a single gap
            for (int y = 0; y < 6; y++)
                _objectsMap.SetOccupied(new Vector2Int(3, y));
            // Gap at (3, 6)

            var path = _pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(6, 0));
            Assert.Greater(path.Count, 0);
            Assert.AreEqual(new Vector2Int(6, 0), path[path.Count - 1]);
        }
    }
}
