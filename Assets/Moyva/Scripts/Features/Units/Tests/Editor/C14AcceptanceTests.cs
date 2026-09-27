using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// C14 — the reachable-tile preview equals what movement may actually
    /// execute: stamina budget, occupancy and diagonal corner rules are
    /// shared, cache invalidates on map changes, and the query never treats
    /// the unit's current tile as a blocker.
    /// </summary>
    public sealed class C14AcceptanceTests
    {
        private const string UnitId = "u-1";
        private static readonly Vector2Int Start = new Vector2Int(5, 5);

        private DiContainer _container;
        private SignalBus _signals;
        private C14Fakes.Units _units;
        private C14Fakes.ObjectsMap _objects;
        private UnitTraversalPolicy _traversal;
        private UnitMovementRangeQuery _query;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<OnObjectsMapChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<GridTileChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<UnitMovedSignal>().OptionalSubscriber();
            _container.DeclareSignal<UnitCreatedSignal>().OptionalSubscriber();
            _container.DeclareSignal<UnitDestroyedSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();

            _units = new C14Fakes.Units();
            _units.Spawn(UnitId, Start, stamina: 3f);
            _objects = new C14Fakes.ObjectsMap();
            _objects.Register(Start, UnitId);

            var grid = new C14Fakes.Grid();
            _traversal = new UnitTraversalPolicy(
                grid,
                new C14Fakes.TraversalCosts(),
                _objects,
                _units,
                new C14Fakes.UnitConfigs(),
                new C14Fakes.PlacementValidator(),
                _signals);
            _traversal.Initialize();

            _query = new UnitMovementRangeQuery(
                _units,
                new C14Fakes.Pathfinder(grid),
                _signals,
                traversal: _traversal);
            _query.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _query?.Dispose();
            _traversal?.Dispose();
            _container?.UnbindAll();
        }

        private HashSet<Vector2Int> Reachable()
        {
            var result = new HashSet<Vector2Int>();
            foreach (var tile in _query.GetMovementTiles(UnitId))
                if (tile.IsReachable)
                    result.Add(tile.Position);
            return result;
        }

        [Test]
        public void EveryPreviewTile_PassesTheSameStepBudget()
        {
            foreach (var tile in _query.GetMovementTiles(UnitId))
            {
                Assert.LessOrEqual(
                    tile.Cost, 3.0001f,
                    $"Tile {tile.Position} exceeds the 3-stamina budget");
            }
        }

        [Test]
        public void StaminaBudget_CutsOffExpensiveSteps()
        {
            _units.SetStamina(UnitId, 1f);
            var tiles = Reachable();

            Assert.IsTrue(tiles.Contains(Start + Vector2Int.right));
            Assert.IsFalse(
                tiles.Contains(Start + new Vector2Int(1, 1)),
                "Diagonal costs 1.414 — beyond a 1.0 budget");
            Assert.IsFalse(
                tiles.Contains(Start + new Vector2Int(2, 0)),
                "Two steps cost 2.0 — beyond a 1.0 budget");
        }

        [Test]
        public void OccupiedTarget_IsNotReachableInPreview()
        {
            Vector2Int occupied = Start + Vector2Int.right;
            _objects.Register(occupied, "enemy-unit");

            Assert.IsFalse(Reachable().Contains(occupied));
        }

        [Test]
        public void OwnTile_IsReachable_AndNeverBlocks()
        {
            var tiles = Reachable();
            Assert.IsTrue(
                tiles.Contains(Start),
                "The current tile belongs to the reachable set (cost 0)");
            Assert.IsTrue(
                tiles.Contains(Start + Vector2Int.up),
                "Own occupancy must not poison neighbor steps");
        }

        [Test]
        public void DiagonalStep_Blocked_WhenEitherSideBlocked()
        {
            Vector2Int diagonal = Start + new Vector2Int(1, 1);
            Vector2Int side = Start + Vector2Int.right;
            _objects.Register(side, "blocker");

            // The diagonal STEP is rejected when one flank is occupied —
            // the cell may still be reached around the other side, but never
            // by cutting the corner.
            Assert.IsFalse(
                _traversal.TryEvaluateStep(
                    UnitId, Start, diagonal, float.PositiveInfinity,
                    UnitTraversalMode.Preview, out _, out _),
                "Corner cutting through a blocked side must not be allowed");
            Assert.IsTrue(
                _traversal.TryEvaluateStep(
                    UnitId, Start, Start + Vector2Int.up, float.PositiveInfinity,
                    UnitTraversalMode.Preview, out _, out _),
                "The free side stays enterable");
        }

        [Test]
        public void OccupancyChange_InvalidatesPreview()
        {
            Vector2Int cell = Start + Vector2Int.right;
            Assert.IsTrue(Reachable().Contains(cell));

            _objects.Register(cell, "late-arrival");
            _signals.Fire(new OnObjectsMapChangedSignal());

            Assert.IsFalse(
                Reachable().Contains(cell),
                "A post-query occupancy change must refresh the preview");
        }

        [Test]
        public void UnregisteredUnit_YieldsEmptyPreview()
        {
            Assert.AreEqual(
                0, _query.GetMovementTiles("ghost").Count);
        }
    }

    internal static class C14Fakes
    {
        internal sealed class Units : IUnitService
        {
            private readonly Dictionary<string, (Vector2Int pos, float stamina, string type)>
                _units = new();

            public void Spawn(string id, Vector2Int pos, float stamina)
                => _units[id] = (pos, stamina, "worker");

            public float GetStamina(string unitId)
                => _units.TryGetValue(unitId, out var u) ? u.stamina : 0f;
            public void SetStamina(string unitId, float stamina)
            {
                if (_units.TryGetValue(unitId, out var u))
                    _units[unitId] = (u.pos, stamina, u.type);
            }
            public bool TryGetUnitPosition(string unitId, out Vector2Int position)
            {
                if (_units.TryGetValue(unitId, out var u))
                {
                    position = u.pos;
                    return true;
                }
                position = default;
                return false;
            }
            public GameObject GetUnitObject(string unitId) => null;
            public IReadOnlyCollection<string> GetAllUnitIds() => _units.Keys;
            public string GetUnitTypeId(string unitId)
                => _units.TryGetValue(unitId, out var u) ? u.type : null;
        }

        internal sealed class ObjectsMap : IObjectsMapService
        {
            private readonly Dictionary<Vector2Int, string> _occupants = new();
            public bool IsOccupied(Vector2Int position)
                => _occupants.ContainsKey(position);
            public bool TryGetOccupant(Vector2Int position, out string occupantId)
                => _occupants.TryGetValue(position, out occupantId);
            public void Register(Vector2Int position, string occupantId)
                => _occupants[position] = occupantId;
            public void Move(Vector2Int from, Vector2Int to)
            {
                string id = _occupants[from];
                _occupants.Remove(from);
                _occupants[to] = id;
            }
            public void Unregister(Vector2Int position)
                => _occupants.Remove(position);
            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                foreach (var pair in _occupants)
                {
                    if (pair.Value != occupantId) continue;
                    position = pair.Key;
                    return true;
                }
                position = default;
                return false;
            }
        }

        internal sealed class Grid : IGridService
        {
            public string GetTileData(Vector2Int position) => "grass";
            public void SetTileData(Vector2Int position, string tileTypeId) { }
            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                tileTypeId = position.x is >= 0 and < 100
                             && position.y is >= 0 and < 100
                    ? "grass"
                    : null;
                return tileTypeId != null;
            }
            public int GridWidth => 100;
            public int GridHeight => 100;
        }

        internal sealed class Pathfinder : IPathfinder
        {
            private readonly IGridService _grid;
            public Pathfinder(IGridService grid) => _grid = grid;

            public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
                => new List<Vector2Int> { start, end };

            public IEnumerable<Vector2Int> GetNeighbors(Vector2Int position)
            {
                for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var n = new Vector2Int(position.x + dx, position.y + dy);
                    if (n.x >= 0 && n.y >= 0
                        && n.x < _grid.GridWidth && n.y < _grid.GridHeight)
                        yield return n;
                }
            }
        }

        internal sealed class TraversalCosts : ITraversalCostResolver
        {
            public bool TryResolve(
                string movementProfileId,
                string tileTypeId,
                out float staminaCost,
                out string reason)
            {
                if (string.IsNullOrWhiteSpace(tileTypeId))
                {
                    staminaCost = 0f;
                    reason = "no tile";
                    return false;
                }
                staminaCost = 1f;
                reason = null;
                return true;
            }
        }

        internal sealed class UnitConfigs : IUnitClassConfig
        {
            public UnitClassConfig GetConfig(string typeId) => null;
        }

        internal sealed class PlacementValidator : IUnitPlacementValidator
        {
            public bool IsTerrainAllowed(Vector2Int position, out string reason)
            {
                reason = null;
                return true;
            }
            public bool CanDeployUnit(
                string unitTypeId, Vector2Int position, out string reason)
            {
                reason = null;
                return true;
            }
        }
    }
}
