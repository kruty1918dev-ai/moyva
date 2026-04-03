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
    public class FogOfWarService : IFogOfWarService, IInitializable, IDisposable
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

        // unitId -> pending registration data received before Initialize(width,height)
        private readonly Dictionary<string, (Vector2Int Position, int VisionRange)> _pendingUnits
            = new Dictionary<string, (Vector2Int Position, int VisionRange)>();

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
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Unsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Unsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        // ─── IFogOfWarService ─────────────────────────────────────────────────

        public void Initialize(int width, int height)
        {
            _width  = width;
            _height = height;

            _visibilityCounters = new int[width, height];
            _exploredTiles      = new bool[width, height];

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
                    RegisterUnit(kvp.Key, kvp.Value.Position, kvp.Value.VisionRange);

                _pendingUnits.Clear();
            }

            // Ensure texture reflects current state after all pending units processed
            _textureUpdater?.RebuildFullTexture(this);
        }

        public void RegisterUnit(string unitId, Vector2Int position, int visionRange)
        {
            if (!_initialized)
            {
                _pendingUnits[unitId] = (position, visionRange);
                return;
            }

            visionRange = ClampVisionRange(visionRange);
            _unitVisionRange[unitId] = visionRange;

            var tiles = _resolver.ComputeVisibleTiles(position, visionRange, _width, _height);
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

                _pendingUnits[unitId] = (newPosition, pendingRange);
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
            var newTiles = _resolver.ComputeVisibleTiles(newPosition, range, _width, _height);
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
        }

        public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles()
            => _lastDirtyTiles;

        // ─── Signal handlers ──────────────────────────────────────────────────

        private void OnUnitCreated(UnitCreatedSignal signal)
            => RegisterUnit(signal.UnitId, signal.Position, ClampVisionRange(_defaultVisionRange));

        private void OnUnitMoved(UnitMovedSignal signal)
            => UpdateUnitPosition(signal.UnitId, signal.NewPosition);

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
            => UnregisterUnit(signal.UnitId);

        // ─── Helpers ──────────────────────────────────────────────────────────

        private int ClampVisionRange(int range)
        {
            int min = _settings != null ? _settings.MinVisionRange : 1;
            int max = _settings != null ? _settings.MaxVisionRange : 12;
            return Mathf.Clamp(range, min, max);
        }

        private bool IsInBounds(Vector2Int pos)
            => pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;

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
    }
}
