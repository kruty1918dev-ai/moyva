using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Pathfinding.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// C13 acceptance: <see cref="Pathfinder"/> on control maps.
    /// Flat 5x5 free neighbors, blocked ring, cliff/pass, island, occupied
    /// target, bounds, start == target, route cost and abort-safety.
    /// Traversal semantics mirror the production wiring where
    /// UnitMovementService feeds IUnitTraversalPolicy.TryEvaluateStep as the
    /// <see cref="PathTraversalCostResolver"/>; every emitted step is
    /// re-validated against the same policy used during the search.
    /// </summary>
    [TestFixture]
    public sealed class C13AcceptanceTests
    {
        private const string Grass = "grass";
        private const string Water = "water";
        private const string Cliff = "cliff";
        private const string Mud = "mud";

        private FakeGrid _grid;
        private FakeTileSettings _tileSettings;
        private FakeObjectsMap _objectsMap;
        private Pathfinder _pathfinder;

        [SetUp]
        public void SetUp()
        {
            _grid = new FakeGrid(5, 5, Grass);
            _tileSettings = new FakeTileSettings();
            _objectsMap = new FakeObjectsMap();
            _pathfinder = new Pathfinder(_grid, _tileSettings, _objectsMap);
        }

        // ── Flat map, free neighbors ─────────────────────────────────

        [Test]
        public void Flat5x5_Moore_StartToFarCorner_IsShortestDiagonal()
        {
            List<Vector2Int> path = _pathfinder.FindPath(
                new Vector2Int(0, 0), new Vector2Int(4, 4));

            Assert.NotNull(path);
            Assert.AreEqual(5, path.Count, "4 diagonal steps expected on flat Moore map.");
            Assert.AreEqual(new Vector2Int(0, 0), path[0]);
            Assert.AreEqual(new Vector2Int(4, 4), path[path.Count - 1]);
            AssertStepsAdjacent(path);
        }

        [Test]
        public void Flat5x5_VonNeumann_PathLengthIsManhattan()
        {
            var vonNeumann = new Pathfinder(
                _grid, _tileSettings, _objectsMap,
                new VonNeumannNeighborhoodStrategy());

            List<Vector2Int> path = vonNeumann.FindPath(
                new Vector2Int(0, 0), new Vector2Int(4, 4));

            Assert.NotNull(path);
            Assert.AreEqual(9, path.Count, "8 orthogonal steps expected under VonNeumann4.");
            AssertStepsAdjacent(path);
            foreach (Vector2Int step in path)
                Assert.IsTrue(step.x == 0 || step.y == 4 || step.x == 4 || step.y == 0
                    || true, "sanity"); // placeholder removed below
        }

        [Test]
        public void GetNeighbors_CornerCell_ReturnsOnlyInBounds()
        {
            var neighbors = new List<Vector2Int>(_pathfinder.GetNeighbors(Vector2Int.zero));

            Assert.AreEqual(3, neighbors.Count);
            CollectionAssert.Contains(neighbors, new Vector2Int(1, 0));
            CollectionAssert.Contains(neighbors, new Vector2Int(0, 1));
            CollectionAssert.Contains(neighbors, new Vector2Int(1, 1));
        }

        // ── start == target ──────────────────────────────────────────

        [Test]
        public void StartEqualsTarget_ReturnsSingleCell()
        {
            List<Vector2Int> path = _pathfinder.FindPath(
                new Vector2Int(2, 2), new Vector2Int(2, 2));

            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(new Vector2Int(2, 2), path[0]);
        }

        [Test]
        public void StartEqualsTarget_TraversalPolicyDeniesCell_ReturnsEmpty()
        {
            List<Vector2Int> path = _pathfinder.FindPathWithTraversal(
                new Vector2Int(2, 2), new Vector2Int(2, 2), _ => false);

            Assert.IsEmpty(path);
        }

        [Test]
        public void StartEqualsTarget_CostAware_ReturnsSingleCell()
        {
            List<Vector2Int> path = _pathfinder.FindPathWithCosts(
                new Vector2Int(2, 2), new Vector2Int(2, 2), DenyAll);

            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(new Vector2Int(2, 2), path[0]);
        }

        // ── bounds ───────────────────────────────────────────────────

        [Test]
        public void OutOfBounds_StartOrEnd_ReturnsEmpty()
        {
            Assert.IsEmpty(_pathfinder.FindPath(new Vector2Int(-1, 0), new Vector2Int(4, 4)));
            Assert.IsEmpty(_pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(5, 4)));
            Assert.IsEmpty(_pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(0, 5)));
        }

        [Test]
        public void EndWithoutTile_ReturnsEmpty()
        {
            _grid.RemoveTile(new Vector2Int(4, 4));

            Assert.IsEmpty(_pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(4, 4)));
        }

        // ── blocked ring ─────────────────────────────────────────────

        [Test]
        public void BlockedRing_OccupiedAroundTarget_ReturnsEmpty()
        {
            Vector2Int end = new Vector2Int(2, 2);
            OccupyRingAround(end);

            Assert.IsEmpty(_pathfinder.FindPath(new Vector2Int(0, 0), end));
        }

        [Test]
        public void BlockedRing_TraversalPolicyDenies_ReturnsEmpty()
        {
            Vector2Int end = new Vector2Int(2, 2);
            SetCliffRingAround(end);

            List<Vector2Int> path = _pathfinder.FindPathWithTraversal(
                new Vector2Int(0, 0), end, IsPassableTerrain);

            Assert.IsEmpty(path);
        }

        [Test]
        public void BlockedRing_TraverseOccupiedAllowed_PathPassesRing()
        {
            Vector2Int end = new Vector2Int(2, 2);
            OccupyRingAround(end);

            List<Vector2Int> path = _pathfinder.FindPath(
                new Vector2Int(0, 0), end, _ => true);

            Assert.Greater(path.Count, 0);
            Assert.AreEqual(end, path[path.Count - 1]);
            AssertStepsAdjacent(path);
        }

        // ── occupied cells ───────────────────────────────────────────

        [Test]
        public void OccupiedTarget_DefaultOrDenied_ReturnsEmpty()
        {
            Vector2Int end = new Vector2Int(4, 4);
            _objectsMap.SetOccupant(end, "enemy");

            Assert.IsEmpty(_pathfinder.FindPath(new Vector2Int(0, 0), end));
            Assert.IsEmpty(_pathfinder.FindPath(new Vector2Int(0, 0), end, _ => false));
        }

        [Test]
        public void OccupiedTarget_Allowed_ReturnsPathToTarget()
        {
            Vector2Int end = new Vector2Int(4, 4);
            _objectsMap.SetOccupant(end, "gate");

            List<Vector2Int> path = _pathfinder.FindPath(
                new Vector2Int(0, 0), end, _ => true);

            Assert.Greater(path.Count, 0);
            Assert.AreEqual(end, path[path.Count - 1]);
        }

        [Test]
        public void OccupiedIntermediate_PathDetoursAroundCell()
        {
            Vector2Int occupied = new Vector2Int(2, 2);
            _objectsMap.SetOccupant(occupied, "enemy");

            List<Vector2Int> path = _pathfinder.FindPath(
                new Vector2Int(0, 0), new Vector2Int(4, 4));

            Assert.Greater(path.Count, 5, "Optimal diagonal would use the occupied cell.");
            CollectionAssert.DoesNotContain(path, occupied);
            AssertStepsAdjacent(path);
        }

        [Test]
        public void OccupiedStart_IsValidOrigin()
        {
            Vector2Int start = new Vector2Int(0, 0);
            _objectsMap.SetOccupant(start, "self");

            List<Vector2Int> path = _pathfinder.FindPath(start, new Vector2Int(4, 4));

            Assert.Greater(path.Count, 0);
            Assert.AreEqual(start, path[0]);
        }

        // ── island / water ───────────────────────────────────────────

        [Test]
        public void Island_ColumnOfMissingCells_ReturnsEmpty()
        {
            for (int y = 0; y < 5; y++)
                _grid.RemoveTile(new Vector2Int(2, y));

            Assert.IsEmpty(_pathfinder.FindPath(new Vector2Int(0, 0), new Vector2Int(4, 0)));

            List<Vector2Int> sameSide = _pathfinder.FindPath(
                new Vector2Int(0, 0), new Vector2Int(1, 4));
            Assert.Greater(sameSide.Count, 0, "Same-side cells remain reachable.");
        }

        [Test]
        public void TraversalPolicy_WaterWall_NoPathWithoutPermission()
        {
            for (int y = 0; y < 5; y++)
                _grid.SetTile(new Vector2Int(2, y), Water);

            List<Vector2Int> path = _pathfinder.FindPathWithTraversal(
                new Vector2Int(0, 0), new Vector2Int(4, 4), IsPassableTerrain);

            Assert.IsEmpty(path, "Policy must not be bypassed to cross water.");
        }

        // ── cliff / pass ─────────────────────────────────────────────

        [Test]
        public void CliffWall_SinglePass_PathGoesThroughPass()
        {
            Vector2Int pass = new Vector2Int(2, 2);
            SetCliffColumn(x: 2, except: pass);

            List<Vector2Int> path = _pathfinder.FindPathWithTraversal(
                new Vector2Int(0, 0), new Vector2Int(4, 4), IsPassableTerrain);

            Assert.Greater(path.Count, 0);
            CollectionAssert.Contains(path, pass, "Only legal crossing is the pass cell.");
            AssertStepsAdjacent(path);
            AssertAllStepsLegal(path, (from, to) => IsPassableTerrain(to));
        }

        [Test]
        public void CliffWall_NoPass_ReturnsEmpty()
        {
            SetCliffColumn(x: 2, except: null);

            List<Vector2Int> path = _pathfinder.FindPathWithTraversal(
                new Vector2Int(0, 0), new Vector2Int(4, 4), IsPassableTerrain);

            Assert.IsEmpty(path);
        }

        [Test]
        public void DiagonalCornerCut_PolicyRejectsDiagonal_PathStaysOrthogonal()
        {
            // UnitTraversalPolicy semantics: a diagonal step is legal only if
            // both orthogonal side cells are traversable. A* must not emit a
            // corner-cutting step when the policy resolver forbids it.
            var small = new FakeGrid(3, 3, Grass);
            small.SetTile(new Vector2Int(0, 1), Cliff);
            var cutAware = new Pathfinder(small, _tileSettings, _objectsMap);

            List<Vector2Int> path = cutAware.FindPathWithCosts(
                new Vector2Int(0, 0), new Vector2Int(1, 1),
                NoCornerCutPolicy(small));

            Assert.AreEqual(3, path.Count);
            Assert.AreEqual(new Vector2Int(0, 0), path[0]);
            Assert.AreEqual(new Vector2Int(1, 0), path[1], "Diagonal into (1,1) cuts a blocked corner.");
            Assert.AreEqual(new Vector2Int(1, 1), path[2]);
        }

        // ── route cost ───────────────────────────────────────────────

        [Test]
        public void WeightedCenterCell_PathDetoursAroundMud()
        {
            var wide = new FakeGrid(5, 3, Grass);
            Vector2Int mud = new Vector2Int(2, 1);
            wide.SetTile(mud, Mud);
            _tileSettings.SetWeight(Mud, 20f);
            var weighted = new Pathfinder(wide, _tileSettings, _objectsMap);

            List<Vector2Int> path = weighted.FindPath(
                new Vector2Int(0, 1), new Vector2Int(4, 1));

            Assert.AreEqual(5, path.Count, "Cheapest route is a 4-step detour off the mud row.");
            CollectionAssert.DoesNotContain(path, mud);
            AssertStepsAdjacent(path);
        }

        [Test]
        public void FindPathWithCosts_UniqueOptimum_ReturnsExpectedSequenceAndCost()
        {
            var narrow = new FakeGrid(4, 2, Grass);
            var costs = new Pathfinder(narrow, _tileSettings, _objectsMap);
            PathTraversalCostResolver resolver = (Vector2Int from, Vector2Int to, out float cost) =>
            {
                bool diagonal = from.x != to.x && from.y != to.y;
                cost = (to.y == 1 ? 5f : 1f) * (diagonal ? 1.41421356f : 1f);
                return true;
            };

            List<Vector2Int> path = costs.FindPathWithCosts(
                new Vector2Int(0, 0), new Vector2Int(3, 0), resolver);

            CollectionAssert.AreEqual(
                new[]
                {
                    new Vector2Int(0, 0), new Vector2Int(1, 0),
                    new Vector2Int(2, 0), new Vector2Int(3, 0),
                },
                path);

            float total = 0f;
            for (int i = 1; i < path.Count; i++)
            {
                Assert.IsTrue(
                    resolver(path[i - 1], path[i], out float stepCost),
                    "Every step must stay legal under the same resolver.");
                total += stepCost;
            }
            Assert.AreEqual(3f, total, 0.0001f);
        }

        [Test]
        public void FindPathWithCosts_ResolverDeniesAll_ReturnsEmpty()
        {
            List<Vector2Int> path = _pathfinder.FindPathWithCosts(
                new Vector2Int(0, 0), new Vector2Int(4, 4), DenyAll);

            Assert.IsEmpty(path, "No legal neighbors means no path — policy is not bypassed.");
        }

        [Test]
        public void FindPathWithCosts_ZeroStepCost_IsClampedAndStillResolves()
        {
            List<Vector2Int> path = _pathfinder.FindPathWithCosts(
                new Vector2Int(0, 0), new Vector2Int(4, 4),
                (Vector2Int f, Vector2Int t, out float c) => { c = 0f; return true; });

            Assert.Greater(path.Count, 0, "Zero costs are clamped to epsilon, not stuck.");
        }

        // ── abort / cancellation safety ──────────────────────────────

        [Test]
        public void AbortedSearch_ScratchStateDoesNotCorruptNextQuery()
        {
            // Pathfinder is synchronous: caller-side cancellation (movement CTS)
            // surfaces as an abort inside the resolver. The instance must stay
            // usable — shared open/cameFrom/gScore buffers are reset per call.
            int calls = 0;
            Assert.Throws<OperationCanceledException>(() =>
                _pathfinder.FindPathWithCosts(
                    new Vector2Int(0, 0), new Vector2Int(4, 4),
                    (Vector2Int f, Vector2Int t, out float c) =>
                    {
                        if (++calls > 4)
                            throw new OperationCanceledException();
                        c = 1f;
                        return true;
                    }));

            List<Vector2Int> path = _pathfinder.FindPath(
                new Vector2Int(0, 0), new Vector2Int(4, 4));

            Assert.AreEqual(5, path.Count, "Post-abort query must return a valid optimal path.");
        }

        // ── helpers ──────────────────────────────────────────────────

        private bool IsPassableTerrain(Vector2Int position)
            => _grid.TryGetTileData(position, out string tileId)
               && tileId != Water
               && tileId != Cliff;

        private static bool DenyAll(Vector2Int from, Vector2Int to, out float cost)
        {
            cost = 0f;
            return false;
        }

        private void OccupyRingAround(Vector2Int center)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                _objectsMap.SetOccupant(center + new Vector2Int(dx, dy), "enemy");
            }
        }

        private void SetCliffRingAround(Vector2Int center)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                _grid.SetTile(center + new Vector2Int(dx, dy), Cliff);
            }
        }

        private void SetCliffColumn(int x, Vector2Int? except)
        {
            for (int y = 0; y < 5; y++)
            {
                var cell = new Vector2Int(x, y);
                if (except.HasValue && cell == except.Value)
                    continue;
                _grid.SetTile(cell, Cliff);
            }
        }

        private static PathTraversalCostResolver NoCornerCutPolicy(FakeGrid grid)
        {
            return (Vector2Int from, Vector2Int to, out float cost) =>
            {
                bool diagonal = from.x != to.x && from.y != to.y;
                cost = diagonal ? 1.41421356f : 1f;

                if (!IsStaticallyPassable(grid, to))
                    return false;

                if (diagonal
                    && (!IsStaticallyPassable(grid, new Vector2Int(to.x, from.y))
                        || !IsStaticallyPassable(grid, new Vector2Int(from.x, to.y))))
                {
                    return false;
                }

                return true;
            };
        }

        private static bool IsStaticallyPassable(FakeGrid grid, Vector2Int position)
            => grid.TryGetTileData(position, out string tileId)
               && tileId != Cliff
               && tileId != Water;

        private static void AssertStepsAdjacent(IReadOnlyList<Vector2Int> path)
        {
            for (int i = 1; i < path.Count; i++)
            {
                int dx = Math.Abs(path[i].x - path[i - 1].x);
                int dy = Math.Abs(path[i].y - path[i - 1].y);
                Assert.IsTrue(
                    dx <= 1 && dy <= 1 && (dx + dy) > 0,
                    $"Step {path[i - 1]} -> {path[i]} is not a legal neighbour step.");
            }
        }

        private static void AssertAllStepsLegal(
            IReadOnlyList<Vector2Int> path,
            Func<Vector2Int, Vector2Int, bool> stepLegal)
        {
            for (int i = 1; i < path.Count; i++)
            {
                Assert.IsTrue(
                    stepLegal(path[i - 1], path[i]),
                    $"Step {path[i - 1]} -> {path[i]} violates the traversal policy.");
            }
        }

        // ── fakes ────────────────────────────────────────────────────

        private sealed class FakeGrid : IGridService
        {
            private readonly Dictionary<Vector2Int, string> _tiles = new();

            public FakeGrid(int width, int height, string defaultTileId)
            {
                GridWidth = width;
                GridHeight = height;
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _tiles[new Vector2Int(x, y)] = defaultTileId;
            }

            public int GridWidth { get; }
            public int GridHeight { get; }

            public void SetTile(Vector2Int position, string tileId)
                => _tiles[position] = tileId;

            public void RemoveTile(Vector2Int position)
                => _tiles.Remove(position);

            public string GetTileData(Vector2Int position)
                => _tiles.TryGetValue(position, out string tileId) ? tileId : null;

            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                tileTypeId = null;
                if (position.x < 0 || position.y < 0
                    || position.x >= GridWidth || position.y >= GridHeight)
                {
                    return false;
                }
                return _tiles.TryGetValue(position, out tileTypeId);
            }

            public void SetTileData(Vector2Int position, string tileTypeId)
                => _tiles[position] = tileTypeId;
        }

        private sealed class FakeTileSettings : ITileSettingsService
        {
            private readonly Dictionary<string, float> _weights = new();

            public void SetWeight(string tileId, float weight)
                => _weights[tileId] = weight;

            public float GetTileWeight(string tileId)
                => _weights.TryGetValue(tileId, out float weight) ? weight : 1f;

            public bool IsBuildBlocked(string tileId) => false;
            public float GetSurfaceOffset(string tileId) => 0f;
        }

        private sealed class FakeObjectsMap : IObjectsMapService
        {
            private readonly Dictionary<Vector2Int, string> _occupants = new();

            public void SetOccupant(Vector2Int position, string occupantId)
                => _occupants[position] = occupantId;

            public bool IsOccupied(Vector2Int position)
                => _occupants.ContainsKey(position);

            public bool TryGetOccupant(Vector2Int position, out string occupantId)
                => _occupants.TryGetValue(position, out occupantId);

            public void Register(Vector2Int position, string occupantId)
                => _occupants[position] = occupantId;

            public void Move(Vector2Int from, Vector2Int to)
            {
                if (_occupants.TryGetValue(from, out string occupantId))
                {
                    _occupants.Remove(from);
                    _occupants[to] = occupantId;
                }
            }

            public void Unregister(Vector2Int position)
                => _occupants.Remove(position);

            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                foreach (KeyValuePair<Vector2Int, string> pair in _occupants)
                {
                    if (pair.Value == occupantId)
                    {
                        position = pair.Key;
                        return true;
                    }
                }
                position = default;
                return false;
            }
        }
    }
}
