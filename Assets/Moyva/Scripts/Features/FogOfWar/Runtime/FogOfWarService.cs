using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Core FogOfWar service.
    /// Maintains a counter grid (int[,]) and explored flags (bool[,]).
    /// Subscribes to unit signals via Zenject SignalBus.
    /// </summary>
    internal sealed class FogOfWarService : IFogOfWarService, IInitializable, IDisposable
    {
        private readonly IFogVisibilityResolver _resolver;
        private readonly IFogTextureUpdater     _textureUpdater;
        private readonly IFogSaveDataProvider   _saveProvider;
        private readonly SignalBus              _signalBus;
        private readonly FogOfWarSettings       _settings;

        private int     _defaultVisionRange = 5;
        private int     _width;
        private int     _height;
        private bool    _initialized;

        private int[,]  _visibilityCounters;
        private bool[,] _exploredTiles;
        private bool[,] _pendingExploredSnapshot;

        // unitId → list of visible tiles when unit was last registered/moved
        private readonly Dictionary<string, IReadOnlyList<Vector2Int>> _unitVisibleTiles
            = new Dictionary<string, IReadOnlyList<Vector2Int>>();

        // unitId → vision range (stored at registration)
        private readonly Dictionary<string, int> _unitVisionRange
            = new Dictionary<string, int>();

        // unitId → current position
        private readonly Dictionary<string, Vector2Int> _unitPositions
            = new Dictionary<string, Vector2Int>();

        private readonly Dictionary<string, FogRevealShape> _fixedVisionShapes
            = new Dictionary<string, FogRevealShape>();

        // unitId -> pending registration data received before Initialize(width,height)
        private readonly Dictionary<string, (Vector2Int Position, int VisionRange, FogRevealShape? Shape)> _pendingUnits
            = new Dictionary<string, (Vector2Int Position, int VisionRange, FogRevealShape? Shape)>();

        private HashSet<Vector2Int> _lastDirtyTiles = new HashSet<Vector2Int>();

        public FogOfWarService(
            IFogVisibilityResolver resolver,
            IFogTextureUpdater     textureUpdater,
            IFogSaveDataProvider   saveProvider,
            SignalBus              signalBus,
            [InjectOptional] FogOfWarSettings settings)
        {
            _resolver       = resolver;
            _textureUpdater = textureUpdater;
            _saveProvider   = saveProvider;
            _signalBus      = signalBus;
            _settings       = settings;

            if (_settings != null)
                _defaultVisionRange = _settings.DefaultVisionRange;
            else
                Debug.LogWarning("[FogOfWar] FogOfWarService: FogOfWarSettings is null. Using DefaultVisionRange=5.");
        }

        // ─── Zenject lifecycle ────────────────────────────────────────────────

        public void Initialize()
        {
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Unsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Unsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
        }

        // ─── IFogOfWarService ─────────────────────────────────────────────────

        public void Initialize(int width, int height)
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);

            _width  = width;
            _height = height;

            _visibilityCounters = new int[width, height];
            _exploredTiles      = new bool[width, height];
            _unitVisibleTiles.Clear();

            _initialized = true;

            // Restore explored state from pending snapshot first (if load happened before map init).
            var snapshot = _pendingExploredSnapshot ?? _saveProvider?.LoadExploredData();
            if (snapshot != null)
                LoadFromSnapshot(snapshot);
            _pendingExploredSnapshot = null;

            // Process units that were spawned/moved before map initialization
            if (_pendingUnits.Count > 0)
            {
                foreach (var kvp in _pendingUnits)
                    RegisterVisionArea(kvp.Key, kvp.Value.Position, kvp.Value.VisionRange, kvp.Value.Shape);

                _pendingUnits.Clear();
            }
            else
            {
                RecalculateAllVisibility();
            }

            // Ensure texture reflects current state after all pending units processed
            _textureUpdater?.RebuildFullTexture(this);
        }

        public void RegisterUnit(string unitId, Vector2Int position, int visionRange)
            => RegisterVisionArea(unitId, position, visionRange, null);

        public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape)
            => RegisterVisionArea(areaId, position, visionRange, shape);

        private void RegisterVisionArea(string unitId, Vector2Int position, int visionRange, FogRevealShape? shape)
        {
            if (!_initialized)
            {
                _pendingUnits[unitId] = (position, visionRange, shape);
                return;
            }

            visionRange = ClampVisionRange(visionRange);
            _unitVisionRange[unitId] = visionRange;
            _unitPositions[unitId] = position;

            if (shape.HasValue)
                _fixedVisionShapes[unitId] = shape.Value;
            else
                _fixedVisionShapes.Remove(unitId);

            var tiles = ComputeInitialVisibleTiles(unitId, position, visionRange);
            _unitVisibleTiles[unitId] = tiles;

            foreach (var t in tiles)
            {
                _visibilityCounters[t.x, t.y]++;
                _exploredTiles[t.x, t.y] = true;
                _lastDirtyTiles.Add(t);
            }

            FlushTexture();
        }

        public void UpdateUnitPosition(string unitId, Vector2Int newPosition)
        {
            if (!_initialized)
            {
                int pendingRange = _unitVisionRange.TryGetValue(unitId, out int storedRange)
                    ? storedRange
                    : _defaultVisionRange;

                FogRevealShape? shape = _fixedVisionShapes.TryGetValue(unitId, out var storedShape)
                    ? storedShape
                    : null;
                _pendingUnits[unitId] = (newPosition, pendingRange, shape);
                return;
            }

            if (!_unitVisibleTiles.TryGetValue(unitId, out var oldTiles))
            {
                int fallbackRange = _unitVisionRange.TryGetValue(unitId, out int storedRange)
                    ? storedRange
                    : _defaultVisionRange;

                RegisterUnit(unitId, newPosition, fallbackRange);
                return;
            }

            // Decrement old tiles
            foreach (var t in oldTiles)
            {
                _visibilityCounters[t.x, t.y] = Mathf.Max(0, _visibilityCounters[t.x, t.y] - 1);
                _lastDirtyTiles.Add(t);
            }

            // Compute new visible tiles
            int range = _unitVisionRange.TryGetValue(unitId, out int r) ? r : _defaultVisionRange;
            _unitPositions[unitId] = newPosition;
            var newTiles = ComputeVisibleTiles(unitId, newPosition, range);
            _unitVisibleTiles[unitId] = newTiles;

            // Increment new tiles
            foreach (var t in newTiles)
            {
                _visibilityCounters[t.x, t.y]++;
                _exploredTiles[t.x, t.y] = true;
                _lastDirtyTiles.Add(t);
            }

            FlushTexture();
        }

        public void UnregisterUnit(string unitId)
        {
            if (!_initialized) { Debug.LogWarning("[FogOfWar] UnregisterUnit called before Initialize(width,height)."); return; }

            if (!_unitVisibleTiles.TryGetValue(unitId, out var tiles))
                return;

            foreach (var t in tiles)
            {
                _visibilityCounters[t.x, t.y] = Mathf.Max(0, _visibilityCounters[t.x, t.y] - 1);
                _lastDirtyTiles.Add(t);
            }

            _unitVisibleTiles.Remove(unitId);
            _unitVisionRange.Remove(unitId);
            _unitPositions.Remove(unitId);
            _fixedVisionShapes.Remove(unitId);

            FlushTexture();
        }

        public FogStateType GetFogState(Vector2Int position)
        {
            if (!_initialized || !IsInBounds(position))
                return FogStateType.Unexplored;

            if (_visibilityCounters[position.x, position.y] >= 1)
                return FogStateType.Visible;

            if (_exploredTiles[position.x, position.y])
                return FogStateType.Explored;

            return FogStateType.Unexplored;
        }

        public bool IsVisible(Vector2Int position)
            => _initialized && IsInBounds(position) && _visibilityCounters[position.x, position.y] >= 1;

        public bool IsExplored(Vector2Int position)
            => _initialized && IsInBounds(position) && _exploredTiles[position.x, position.y];

        public bool[,] GetExploredSnapshot()
        {
            if (!_initialized) return null;

            var snap = new bool[_width, _height];
            System.Array.Copy(_exploredTiles, snap, _exploredTiles.Length);
            return snap;
        }

        public void LoadFromSnapshot(bool[,] explored)
        {
            if (explored == null) return;

            if (!_initialized)
            {
                _pendingExploredSnapshot = CloneSnapshot(explored);
                return;
            }

            int w = explored.GetLength(0);
            int h = explored.GetLength(1);
            int copyW = Mathf.Min(w, _width);
            int copyH = Mathf.Min(h, _height);

            for (int x = 0; x < copyW; x++)
                for (int y = 0; y < copyH; y++)
                    _exploredTiles[x, y] = explored[x, y];

            _textureUpdater?.RebuildFullTexture(this);
        }

        public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles()
            => _lastDirtyTiles;

        // ─── Signal handlers ──────────────────────────────────────────────────

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            int requestedRange = signal.VisionRange > 0 ? signal.VisionRange : _defaultVisionRange;
            RegisterUnit(signal.UnitId, signal.Position, ClampVisionRange(requestedRange));
        }

        private void OnUnitMoved(UnitMovedSignal signal)
            => UpdateUnitPosition(signal.UnitId, signal.NewPosition);

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
            => UnregisterUnit(signal.UnitId);

        private void OnWorldGeneratedData(WorldGeneratedDataSignal signal)
        {
            _resolver.SetHeightMap(signal.HeightMap);

            int signalWidth = Mathf.Max(1, signal.Width);
            int signalHeight = Mathf.Max(1, signal.Height);

            if (!_initialized)
            {
                Initialize(signalWidth, signalHeight);
                return;
            }

            if (_width != signalWidth || _height != signalHeight)
                ResizeToWorldDimensions(signalWidth, signalHeight);

            RecalculateAllVisibility();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private int ClampVisionRange(int range)
        {
            int min = _settings != null ? _settings.MinVisionRange : 1;
            int max = _settings != null ? _settings.MaxVisionRange : 12;
            return Mathf.Clamp(range, min, max);
        }

        private bool IsInBounds(Vector2Int pos)
            => pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;

        private IReadOnlyList<Vector2Int> ComputePixelCircleTiles(Vector2Int origin, int radius)
            => ComputeShapeTiles(origin, radius, FogRevealShape.PixelCircle);

        private IReadOnlyList<Vector2Int> ComputeShapeTiles(Vector2Int origin, int radius, FogRevealShape shape)
        {
            var result = new List<Vector2Int>();
            int safeRadius = Mathf.Max(0, radius);
            float radiusWithCellCoverage = safeRadius + 0.5f;
            float sqrRadius = radiusWithCellCoverage * radiusWithCellCoverage;

            for (int dx = -safeRadius; dx <= safeRadius; dx++)
            {
                for (int dy = -safeRadius; dy <= safeRadius; dy++)
                {
                    if (!IsInsideShape(dx, dy, safeRadius, sqrRadius, shape))
                        continue;

                    var tile = new Vector2Int(origin.x + dx, origin.y + dy);
                    if (IsInBounds(tile))
                        result.Add(tile);
                }
            }

            return result;
        }

        private static bool IsInsideShape(int dx, int dy, int radius, float sqrRadius, FogRevealShape shape)
        {
            return shape switch
            {
                FogRevealShape.Square => Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= radius,
                FogRevealShape.Diamond => Mathf.Abs(dx) + Mathf.Abs(dy) <= radius,
                _ => dx * dx + dy * dy <= sqrRadius,
            };
        }

        private IReadOnlyList<Vector2Int> ComputeInitialVisibleTiles(string unitId, Vector2Int position, int range)
        {
            return _fixedVisionShapes.TryGetValue(unitId, out var shape)
                ? ComputeShapeTiles(position, range, shape)
                : ComputePixelCircleTiles(position, range);
        }

        private IReadOnlyList<Vector2Int> ComputeVisibleTiles(string unitId, Vector2Int position, int range)
        {
            return _fixedVisionShapes.TryGetValue(unitId, out var shape)
                ? ComputeShapeTiles(position, range, shape)
                : _resolver.ComputeVisibleTiles(position, range, _width, _height);
        }

        private static bool[,] CloneSnapshot(bool[,] source)
        {
            int w = source.GetLength(0);
            int h = source.GetLength(1);
            var copy = new bool[w, h];

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    copy[x, y] = source[x, y];

            return copy;
        }

        private void FlushTexture()
        {
            if (_textureUpdater != null)
                _textureUpdater.UpdateDirtyTiles(this, _lastDirtyTiles);

            _lastDirtyTiles.Clear();
        }

        private void RecalculateAllVisibility()
        {
            if (!_initialized)
                return;

            Array.Clear(_visibilityCounters, 0, _visibilityCounters.Length);
            _unitVisibleTiles.Clear();

            foreach (var unitEntry in _unitPositions)
            {
                if (!_unitVisionRange.TryGetValue(unitEntry.Key, out int range))
                    range = _defaultVisionRange;

                var visibleTiles = ComputeVisibleTiles(unitEntry.Key, unitEntry.Value, range);
                _unitVisibleTiles[unitEntry.Key] = visibleTiles;

                foreach (var tile in visibleTiles)
                {
                    _visibilityCounters[tile.x, tile.y]++;
                    _exploredTiles[tile.x, tile.y] = true;
                }
            }

            _textureUpdater?.RebuildFullTexture(this);
            _lastDirtyTiles.Clear();
        }

        private void ResizeToWorldDimensions(int width, int height)
        {
            var exploredSnapshot = GetExploredSnapshot();

            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            _visibilityCounters = new int[_width, _height];
            _exploredTiles = new bool[_width, _height];
            _unitVisibleTiles.Clear();
            _lastDirtyTiles.Clear();

            if (exploredSnapshot != null)
                LoadFromSnapshot(exploredSnapshot);
        }
    }
}
