using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.Tilemaps;
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
        private const string BuildingVisionAreaPrefix = "building:";

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

        internal int Version { get; private set; }
        internal bool IsReady => _initialized;

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
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
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
            BumpVersion();
        }

        public void RegisterUnit(string unitId, Vector2Int position, int visionRange)
            => RegisterVisionArea(unitId, position, visionRange, null);

        public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape)
            => RegisterVisionArea(areaId, position, visionRange, shape);

        private void RegisterVisionArea(string unitId, Vector2Int position, int visionRange, FogRevealShape? shape)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            if (!_initialized)
            {
                _pendingUnits[unitId] = (position, visionRange, shape);
                return;
            }

            RemoveVisibleTiles(unitId);

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
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            if (!_initialized)
            {
                _pendingUnits.Remove(unitId);
                _unitVisionRange.Remove(unitId);
                _unitPositions.Remove(unitId);
                _fixedVisionShapes.Remove(unitId);
                return;
            }

            if (!RemoveVisibleTiles(unitId))
                return;

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
            BumpVersion();
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

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            int requestedRange = _defaultVisionRange;
            RegisterFixedVisionArea(GetBuildingVisionAreaId(signal.Position), signal.Position, requestedRange, FogRevealShape.PixelCircle);
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
            => UnregisterUnit(GetBuildingVisionAreaId(signal.Position));

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

        private bool RemoveVisibleTiles(string unitId)
        {
            if (!_unitVisibleTiles.TryGetValue(unitId, out var tiles))
                return false;

            foreach (var tile in tiles)
            {
                _visibilityCounters[tile.x, tile.y] = Mathf.Max(0, _visibilityCounters[tile.x, tile.y] - 1);
                _lastDirtyTiles.Add(tile);
            }

            _unitVisibleTiles.Remove(unitId);
            return true;
        }

        private static string GetBuildingVisionAreaId(Vector2Int position)
            => $"{BuildingVisionAreaPrefix}{position.x}:{position.y}";

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
            int dirtyCount = _lastDirtyTiles.Count;
            if (_textureUpdater != null)
                _textureUpdater.UpdateDirtyTiles(this, _lastDirtyTiles);

            if (dirtyCount > 0)
                BumpVersion();

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
            BumpVersion();
            _lastDirtyTiles.Clear();
        }

        private void BumpVersion()
        {
            unchecked
            {
                Version++;
            }
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

    internal static class FogRendererCullingEvaluator
    {
        private const float BoundsEdgeEpsilon = 0.001f;

        public static bool ShouldRender(Bounds worldBounds, IFogOfWarService fogService, IGridService gridService, float boundsPaddingCells)
        {
            if (fogService == null || gridService == null)
                return true;

            if (!TryGetCoveredTileRange(worldBounds, gridService, boundsPaddingCells, out var min, out var max))
                return true;

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    if (fogService.GetFogState(new Vector2Int(x, y)) != FogStateType.Unexplored)
                        return true;
                }
            }

            return false;
        }

        internal static bool TryGetCoveredTileRange(
            Bounds worldBounds,
            IGridService gridService,
            float boundsPaddingCells,
            out Vector2Int min,
            out Vector2Int max)
        {
            min = default;
            max = default;

            if (gridService == null || gridService.GridWidth <= 0 || gridService.GridHeight <= 0)
                return false;

            float padding = Mathf.Max(0f, boundsPaddingCells);
            int rawMinX = Mathf.FloorToInt(worldBounds.min.x + 0.5f - padding);
            int rawMinY = Mathf.FloorToInt(worldBounds.min.y + 0.5f - padding);
            int rawMaxX = Mathf.FloorToInt(worldBounds.max.x + 0.5f - BoundsEdgeEpsilon + padding);
            int rawMaxY = Mathf.FloorToInt(worldBounds.max.y + 0.5f - BoundsEdgeEpsilon + padding);

            if (rawMaxX < 0 || rawMaxY < 0 || rawMinX >= gridService.GridWidth || rawMinY >= gridService.GridHeight)
                return false;

            min = new Vector2Int(
                Mathf.Clamp(rawMinX, 0, gridService.GridWidth - 1),
                Mathf.Clamp(rawMinY, 0, gridService.GridHeight - 1));

            max = new Vector2Int(
                Mathf.Clamp(rawMaxX, 0, gridService.GridWidth - 1),
                Mathf.Clamp(rawMaxY, 0, gridService.GridHeight - 1));

            return min.x <= max.x && min.y <= max.y;
        }
    }

    internal sealed class FogRendererCullingService : IInitializable, ITickable, IDisposable
    {
        private const int DefaultMaxRenderersPerFrame = 384;
        private const float DefaultDiscoveryInterval = 0.75f;
        private const float DefaultBoundsPaddingCells = 0f;

        private static readonly string[] WorldRootNames =
        {
            "TilesRoot",
            "ObjectsRoot",
            "BuildingsRoot",
            "PlayerBuildingsRoot",
            "Clouds",
            "CloudsRoot",
        };

        private readonly FogOfWarService _fogService;
        private readonly IGridService _gridService;
        private readonly SignalBus _signalBus;
        private readonly FogOfWarSettings _settings;

        private readonly List<CullableRenderer> _renderers = new List<CullableRenderer>();
        private readonly Dictionary<Renderer, CullableRenderer> _tracked = new Dictionary<Renderer, CullableRenderer>();
        private readonly Dictionary<string, GameObject> _unitObjects = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Transform> _worldRoots = new Dictionary<string, Transform>();
        private readonly HashSet<Renderer> _discoveredRenderers = new HashSet<Renderer>();
        private readonly List<Renderer> _rendererDiscoveryBuffer = new List<Renderer>(512);

        private bool _discoveryRequested = true;
        private bool _evaluationPending;
        private int _cursor;
        private int _lastFogVersion = -1;
        private float _nextDiscoveryAt;

        public FogRendererCullingService(
            FogOfWarService fogService,
            IGridService gridService,
            SignalBus signalBus,
            [InjectOptional] FogOfWarSettings settings)
        {
            _fogService = fogService;
            _gridService = gridService;
            _signalBus = signalBus;
            _settings = settings;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            RequestDiscovery();
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);

            RestoreAllRenderers();
            _unitObjects.Clear();
        }

        public void Tick()
        {
            if (!IsCullingEnabled())
            {
                RestoreAllRenderers();
                return;
            }

            if (_fogService == null || !_fogService.IsReady)
                return;

            float now = Time.unscaledTime;
            if (now >= _nextDiscoveryAt)
                RequestDiscovery();

            if (_discoveryRequested)
            {
                RebuildTrackedRenderers();
                _discoveryRequested = false;
                _nextDiscoveryAt = now + ResolveDiscoveryInterval();
                RequestEvaluation(resetCursor: true);
            }

            if (_lastFogVersion != _fogService.Version)
            {
                _lastFogVersion = _fogService.Version;
                RequestEvaluation(resetCursor: true);
            }

            if (!_evaluationPending)
                return;

            EvaluateBatch(ResolveMaxRenderersPerFrame());
        }

        private void OnWorldBuilt(WorldBuiltSignal _)
            => RequestDiscovery();

        private void OnWorldGeneratedData(WorldGeneratedDataSignal _)
            => RequestDiscovery();

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            if (!string.IsNullOrWhiteSpace(signal.UnitId) && signal.UnitObject != null)
                _unitObjects[signal.UnitId] = signal.UnitObject;

            RequestDiscovery();
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (!string.IsNullOrWhiteSpace(signal.UnitId))
                _unitObjects.Remove(signal.UnitId);

            RequestDiscovery();
        }

        private void OnBuildingPlaced(BuildingPlacedSignal _)
            => RequestDiscovery();

        private void OnBuildingDemolished(BuildingDemolishedSignal _)
            => RequestDiscovery();

        private void RequestDiscovery()
        {
            _discoveryRequested = true;
        }

        private void RequestEvaluation(bool resetCursor)
        {
            if (resetCursor)
                _cursor = 0;

            _evaluationPending = true;
        }

        private void RebuildTrackedRenderers()
        {
            _discoveredRenderers.Clear();

            for (int i = 0; i < WorldRootNames.Length; i++)
            {
                var root = ResolveWorldRoot(WorldRootNames[i]);
                if (root != null)
                    AddRenderersFrom(root, _discoveredRenderers);
            }

            foreach (var unitObject in _unitObjects.Values)
            {
                if (unitObject != null)
                    AddRenderersFrom(unitObject.transform, _discoveredRenderers);
            }

            for (int i = _renderers.Count - 1; i >= 0; i--)
            {
                var entry = _renderers[i];
                if (entry.Renderer != null && _discoveredRenderers.Contains(entry.Renderer))
                    continue;

                entry.Restore();
                if (entry.Renderer != null)
                    _tracked.Remove(entry.Renderer);

                _renderers.RemoveAt(i);
            }

            _discoveredRenderers.Clear();
        }

        private Transform ResolveWorldRoot(string rootName)
        {
            if (_worldRoots.TryGetValue(rootName, out var cachedRoot) && cachedRoot != null)
                return cachedRoot;

            var rootObject = GameObject.Find(rootName);
            var root = rootObject != null ? rootObject.transform : null;
            _worldRoots[rootName] = root;
            return root;
        }

        private void AddRenderersFrom(Transform root, HashSet<Renderer> discovered)
        {
            if (root == null || !root.gameObject.activeInHierarchy)
                return;

            _rendererDiscoveryBuffer.Clear();
            root.GetComponentsInChildren(true, _rendererDiscoveryBuffer);
            for (int i = 0; i < _rendererDiscoveryBuffer.Count; i++)
            {
                var renderer = _rendererDiscoveryBuffer[i];
                if (!IsSupportedRenderer(renderer))
                    continue;

                discovered.Add(renderer);

                if (_tracked.ContainsKey(renderer))
                    continue;

                var entry = new CullableRenderer(renderer);
                _tracked.Add(renderer, entry);
                _renderers.Add(entry);
            }

            _rendererDiscoveryBuffer.Clear();
        }

        private void EvaluateBatch(int maxRenderers)
        {
            if (_renderers.Count == 0)
            {
                _cursor = 0;
                _evaluationPending = false;
                return;
            }

            int processed = 0;
            float paddingCells = ResolveBoundsPaddingCells();

            while (processed < maxRenderers && _cursor < _renderers.Count)
            {
                var entry = _renderers[_cursor++];
                var renderer = entry.Renderer;
                if (renderer == null || !renderer.gameObject.activeInHierarchy)
                {
                    processed++;
                    continue;
                }

                bool shouldRender = FogRendererCullingEvaluator.ShouldRender(renderer.bounds, _fogService, _gridService, paddingCells);
                entry.SetHiddenByFog(!shouldRender);
                processed++;
            }

            if (_cursor < _renderers.Count)
                return;

            _cursor = 0;
            _evaluationPending = false;
        }

        private void RestoreAllRenderers()
        {
            for (int i = 0; i < _renderers.Count; i++)
                _renderers[i].Restore();

            _tracked.Clear();
            _renderers.Clear();
            _cursor = 0;
            _evaluationPending = false;
            _lastFogVersion = -1;
        }

        private bool IsCullingEnabled()
        {
            if (_settings == null)
                return true;

            if (!_settings.EnableRendererCulling)
                return false;

            if (_settings.RequireOpaqueUnexploredForCulling && _settings.UnexploredAlpha < 0.99f)
                return false;

            return true;
        }

        private int ResolveMaxRenderersPerFrame()
            => _settings != null
                ? Mathf.Max(1, _settings.RendererCullingMaxRenderersPerFrame)
                : DefaultMaxRenderersPerFrame;

        private float ResolveDiscoveryInterval()
            => _settings != null
                ? Mathf.Max(0.05f, _settings.RendererCullingDiscoveryInterval)
                : DefaultDiscoveryInterval;

        private float ResolveBoundsPaddingCells()
            => _settings != null
                ? Mathf.Max(0f, _settings.RendererCullingBoundsPaddingCells)
                : DefaultBoundsPaddingCells;

        private bool IsSupportedRenderer(Renderer renderer)
        {
            if (renderer == null || !renderer.gameObject.activeInHierarchy)
                return false;

            if (!(renderer is SpriteRenderer) && !(renderer is MeshRenderer) && !(renderer is TilemapRenderer))
                return false;

            if (_settings != null)
            {
                int bit = 1 << renderer.gameObject.layer;
                if ((_settings.RendererCullingLayerMask.value & bit) == 0)
                    return false;
            }

            return renderer.GetComponentInParent<FogQuadController>() == null;
        }

        private sealed class CullableRenderer
        {
            public readonly Renderer Renderer;
            private bool _hiddenByFog;
            private bool _enabledBeforeFog;

            public CullableRenderer(Renderer renderer)
            {
                Renderer = renderer;
                _enabledBeforeFog = renderer != null && renderer.enabled;
            }

            public void SetHiddenByFog(bool hidden)
            {
                if (Renderer == null)
                    return;

                if (hidden)
                {
                    if (!_hiddenByFog)
                    {
                        _enabledBeforeFog = Renderer.enabled;
                        _hiddenByFog = true;
                    }

                    Renderer.enabled = false;
                    return;
                }

                Restore();
            }

            public void Restore()
            {
                if (!_hiddenByFog || Renderer == null)
                    return;

                Renderer.enabled = _enabledBeforeFog;
                _hiddenByFog = false;
            }
        }
    }
}
