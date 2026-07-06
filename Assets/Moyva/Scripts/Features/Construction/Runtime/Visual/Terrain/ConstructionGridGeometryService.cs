using System;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionGridGeometryService : IConstructionGridGeometryService, IInitializable, IDisposable
    {
        private const float MinCellSize = 0.0001f;
        private const string LogTag = "[ConstructionGridGeometry]";

        private readonly SignalBus _signalBus;
        private readonly IGridProjection _gridProjection;
        private readonly IWorldGenerationSignalState _worldState;

        private bool _hasGeneratedBounds;
        private Bounds _mapBounds;
        private float _cellSize = 1f;

        [Inject]
        public ConstructionGridGeometryService(
            [InjectOptional] SignalBus signalBus = null,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IWorldGenerationSignalState worldState = null)
        {
            _signalBus = signalBus;
            _gridProjection = gridProjection;
            _worldState = worldState;
        }

        public void Initialize()
        {
            _signalBus?.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            if (_worldState != null && _worldState.TryGetWorldGeneratedData(out var signal))
                ApplyWorldSignal(signal, "cached");
        }

        public void Dispose() => _signalBus?.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);

        public bool TryGetCellCenter(Vector2Int tile, out Vector3 center)
        {
            if (_hasGeneratedBounds)
            {
                Vector3 min = _mapBounds.min;
                float half = _cellSize * 0.5f;
                center = new Vector3(min.x + half + tile.x * _cellSize, _mapBounds.center.y, min.z + half + tile.y * _cellSize);
                return true;
            }

            center = _gridProjection != null ? _gridProjection.GridToWorld(tile, 0f, 0f) : default;
            return _gridProjection != null;
        }

        public bool TryGetCellSize(out Vector2 size)
        {
            if (_hasGeneratedBounds)
            {
                size = new Vector2(_cellSize, _cellSize);
                return true;
            }

            return TryGetProjectedCellSize(out size);
        }

        public bool TryGetCellAtWorld(Vector3 worldPosition, out Vector2Int tile)
        {
            if (_hasGeneratedBounds)
            {
                Vector3 min = _mapBounds.min;
                tile = new Vector2Int(
                    Mathf.FloorToInt((worldPosition.x - min.x) / _cellSize),
                    Mathf.FloorToInt((worldPosition.z - min.z) / _cellSize));
                return true;
            }

            tile = _gridProjection != null ? _gridProjection.WorldToGrid(worldPosition) : default;
            return _gridProjection != null;
        }

        public bool TryGetGridPlaneY(out float y)
        {
            y = _hasGeneratedBounds ? _mapBounds.center.y : 0f;
            return _hasGeneratedBounds;
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal signal) => ApplyWorldSignal(signal, "signal");

        private void ApplyWorldSignal(WorldGeneratedDataSignal signal, string source)
        {
            if (!TryResolveBounds(signal, out Bounds bounds))
            {
                _hasGeneratedBounds = false;
                Debug.LogWarning($"{LogTag} Missing generated map bounds from {source}; falling back to grid projection.");
                return;
            }

            _mapBounds = bounds;
            _cellSize = ResolveCellSize(signal, bounds);
            _hasGeneratedBounds = true;
            Debug.Log($"{LogTag} Ready from {source}: boundsCenter={bounds.center}, boundsSize={bounds.size}, cellSize={_cellSize:0.###}.");
        }

        private static float ResolveCellSize(WorldGeneratedDataSignal signal, Bounds bounds)
        {
            if (signal.CellSize > MinCellSize)
                return signal.CellSize;

            float widthCell = bounds.size.x / Mathf.Max(1, signal.Width);
            float heightCell = bounds.size.z / Mathf.Max(1, signal.Height);
            return Mathf.Max(MinCellSize, Mathf.Min(widthCell, heightCell));
        }

        private static bool TryResolveBounds(WorldGeneratedDataSignal signal, out Bounds bounds)
        {
            bounds = default;
            if (!signal.HasMapWorldBounds || !IsFinite(signal.MapWorldBoundsCenter) || !IsFinite(signal.MapWorldBoundsSize))
                return false;

            Vector3 size = new(
                Mathf.Abs(signal.MapWorldBoundsSize.x),
                Mathf.Abs(signal.MapWorldBoundsSize.y),
                Mathf.Abs(signal.MapWorldBoundsSize.z));
            if (size.x <= MinCellSize || size.z <= MinCellSize)
                return false;

            bounds = new Bounds(signal.MapWorldBoundsCenter, size);
            return true;
        }

        private bool TryGetProjectedCellSize(out Vector2 size)
        {
            size = default;
            if (_gridProjection == null)
                return false;

            Vector3 origin = _gridProjection.GridToWorld(Vector2Int.zero, 0f, 0f);
            Vector3 right = _gridProjection.GridToWorld(Vector2Int.right, 0f, 0f);
            Vector3 up = _gridProjection.GridToWorld(Vector2Int.up, 0f, 0f);
            float width = Mathf.Abs(right.x - origin.x);
            float depth = Mathf.Abs(up.z - origin.z);
            size = new Vector2(Mathf.Max(MinCellSize, width), Mathf.Max(MinCellSize, depth));
            return true;
        }

        private static bool IsFinite(Vector3 value) => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
