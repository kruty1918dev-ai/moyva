using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>
    /// Canonical reachable-tile query for unit movement.
    /// Owns preview-range search/cache; movement execution stays in UnitMovementService.
    /// </summary>
    internal sealed class UnitMovementRangeQuery : IUnitMovementQuery, IInitializable, IDisposable
    {
        private readonly IUnitService _units;
        private readonly IPathfinder _pathfinder;
        private readonly SignalBus _signals;
        private readonly ITurnService _turns;
        private readonly IGameplayProgressClock _progressClock;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IUnitTraversalPolicy _traversal;
        private readonly Dictionary<string, MovementRangeCacheEntry> _cache = new();
        private int _worldVersion;

        [Inject]
        public UnitMovementRangeQuery(
            IUnitService units,
            IPathfinder pathfinder,
            SignalBus signals,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] IGameplayProgressClock progressClock = null,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] IUnitTraversalPolicy traversal = null)
        {
            _units = units;
            _pathfinder = pathfinder;
            _signals = signals;
            _turns = turns;
            _progressClock = progressClock;
            _ownership = ownership;
            _traversal = traversal;
        }

        public void Initialize()
        {
            _signals.Subscribe<OnObjectsMapChangedSignal>(OnObjectsMapChanged);
            _signals.Subscribe<GridTileChangedSignal>(OnGridTileChanged);
            _signals.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signals.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signals.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        public void Dispose()
        {
            _signals.TryUnsubscribe<OnObjectsMapChangedSignal>(OnObjectsMapChanged);
            _signals.TryUnsubscribe<GridTileChangedSignal>(OnGridTileChanged);
            _signals.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signals.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signals.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        public IReadOnlyList<UnitMovementTileSnapshot> GetMovementTiles(string unitId)
        {
            if (string.IsNullOrWhiteSpace(unitId)
                || !_units.TryGetUnitPosition(unitId, out Vector2Int startPosition))
                return Array.Empty<UnitMovementTileSnapshot>();

            string ownerId = _ownership?.GetUnitOwnerId(unitId);
            if (_turns != null
                && _progressClock?.IsRealtime != true
                && !_turns.CanOwnerAct(ownerId, out _))
                return Array.Empty<UnitMovementTileSnapshot>();

            float movement = Mathf.Max(0f, _units.GetStamina(unitId));
            if (_cache.TryGetValue(unitId, out MovementRangeCacheEntry cached)
                && cached.Position == startPosition
                && Mathf.Abs(cached.Movement - movement) <= 0.0001f
                && cached.WorldVersion == _worldVersion)
                return cached.Tiles;

            if (_traversal == null)
            {
                Debug.LogError($"[MOYVA_MOVE][RANGE] traversal policy is not bound for '{unitId}'.");
                return Array.Empty<UnitMovementTileSnapshot>();
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var costs = new Dictionary<Vector2Int, float>(128);
            var frontier = new List<MovementFrontierNode>(128);
            costs[startPosition] = 0f;
            Push(frontier, new MovementFrontierNode(startPosition, 0f));
            int expanded = 0;
            int edges = 0;

            while (frontier.Count > 0)
            {
                MovementFrontierNode current = Pop(frontier);
                if (!costs.TryGetValue(current.Position, out float best)
                    || current.Cost > best + 0.0001f
                    || current.Cost > movement + 0.0001f)
                    continue;

                expanded++;
                foreach (Vector2Int neighbor in _pathfinder.GetNeighbors(current.Position))
                {
                    edges++;
                    if (neighbor == startPosition)
                        continue;

                    float remaining = Mathf.Max(0f, movement - current.Cost);
                    if (!_traversal.TryEvaluateStep(
                            unitId, current.Position, neighbor, remaining,
                            UnitTraversalMode.Preview, out float stepCost, out _))
                        continue;

                    float next = current.Cost + stepCost;
                    if (next > movement + 0.0001f)
                        continue;
                    if (costs.TryGetValue(neighbor, out float previous)
                        && next >= previous - 0.0001f)
                        continue;

                    costs[neighbor] = next;
                    Push(frontier, new MovementFrontierNode(neighbor, next));
                }
            }

            var ordered = new List<UnitMovementTileSnapshot>(costs.Count);
            foreach (KeyValuePair<Vector2Int, float> entry in costs)
                ordered.Add(new UnitMovementTileSnapshot(entry.Key, isReachable: true, cost: entry.Value));

            ordered.Sort((left, right) =>
            {
                int c = left.Cost.CompareTo(right.Cost);
                if (c != 0) return c;
                int y = left.Position.y.CompareTo(right.Position.y);
                return y != 0 ? y : left.Position.x.CompareTo(right.Position.x);
            });

            stopwatch.Stop();
            _cache[unitId] = new MovementRangeCacheEntry(startPosition, movement, _worldVersion, ordered);
            string tag = stopwatch.Elapsed.TotalMilliseconds >= 20d ? "SLOW" : "OK";
            return ordered;
        }

        private void OnObjectsMapChanged(OnObjectsMapChangedSignal _) => Invalidate();
        private void OnGridTileChanged(GridTileChangedSignal _)
        {
            _traversal?.InvalidateStaticCache();
            Invalidate();
        }
        private void OnUnitMoved(UnitMovedSignal _) => Invalidate();
        private void OnUnitCreated(UnitCreatedSignal _) => Invalidate();
        private void OnUnitDestroyed(UnitDestroyedSignal _) => Invalidate();

        private void Invalidate()
        {
            unchecked { _worldVersion++; }
            _cache.Clear();
        }

        private readonly struct MovementRangeCacheEntry
        {
            public MovementRangeCacheEntry(Vector2Int position, float movement, int worldVersion, IReadOnlyList<UnitMovementTileSnapshot> tiles)
            { Position=position; Movement=movement; WorldVersion=worldVersion; Tiles=tiles; }
            public Vector2Int Position { get; }
            public float Movement { get; }
            public int WorldVersion { get; }
            public IReadOnlyList<UnitMovementTileSnapshot> Tiles { get; }
        }

        private readonly struct MovementFrontierNode
        {
            public MovementFrontierNode(Vector2Int position, float cost) { Position=position; Cost=cost; }
            public Vector2Int Position { get; }
            public float Cost { get; }
        }

        private static void Push(List<MovementFrontierNode> heap, MovementFrontierNode node)
        {
            heap.Add(node);
            int index=heap.Count-1;
            while(index>0)
            {
                int parent=(index-1)/2;
                if(heap[parent].Cost<=heap[index].Cost) break;
                (heap[parent],heap[index])=(heap[index],heap[parent]);
                index=parent;
            }
        }

        private static MovementFrontierNode Pop(List<MovementFrontierNode> heap)
        {
            MovementFrontierNode result=heap[0];
            int lastIndex=heap.Count-1;
            MovementFrontierNode last=heap[lastIndex];
            heap.RemoveAt(lastIndex);
            if(heap.Count==0) return result;
            heap[0]=last;
            int index=0;
            while(true)
            {
                int left=index*2+1;
                if(left>=heap.Count) break;
                int right=left+1;
                int best=right<heap.Count && heap[right].Cost<heap[left].Cost ? right : left;
                if(heap[index].Cost<=heap[best].Cost) break;
                (heap[index],heap[best])=(heap[best],heap[index]);
                index=best;
            }
            return result;
        }
    }
}
