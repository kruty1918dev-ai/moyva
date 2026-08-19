using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Grid.Runtime
{
    public sealed class WorldPointerGridResolver : IWorldPointerGridResolver
    {
        private const float SurfaceHeightQuantization = 1000f;
        private const float SurfaceHeightMatchEpsilon = 0.002f;
        private const string LogTag = "[MoyvaPointerGrid]";

        private readonly Camera _camera;
        private readonly IGridProjection _gridProjection;
        private readonly IGridWorldGeometryQuery _gridGeometry;
        private readonly IGridService _gridService;
        private readonly IGridTerrainSurfaceQuery _terrainSurfaceQuery;
        private readonly List<float> _surfaceHeightCandidates = new();
        private readonly HashSet<int> _surfaceHeightKeys = new();

        private int _cachedGridWidth = -1;
        private int _cachedGridHeight = -1;
        private int _cachedTerrainSurfaceVersion = int.MinValue;
        private bool _surfaceHeightCacheInitialized;
        private bool _loggedSurfaceFallback;

        public WorldPointerGridResolver(
            Camera camera,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IGridWorldGeometryQuery gridGeometry = null,
            [InjectOptional] IGridService gridService = null,
            [InjectOptional] IGridTerrainSurfaceQuery terrainSurfaceQuery = null)
        {
            _camera = camera;
            _gridProjection = gridProjection
                ?? (gridGeometry != null
                    ? new Orthographic3DGridProjection()
                    : new OrthogonalGridProjection());
            _gridGeometry = gridGeometry;
            _gridService = gridService;
            _terrainSurfaceQuery = terrainSurfaceQuery;
        }

        public bool TryScreenToGrid(Vector2 screenPosition, out Vector2Int tile)
        {
            tile = default;
            Camera camera = ResolveCamera();
            if (camera == null)
                return false;

            Ray ray = camera.ScreenPointToRay(screenPosition);
            if (TryResolveTerrainSurfaceTile(ray, out tile))
                return true;

            Vector3 worldPos = ScreenToWorldOnGridPlane(screenPosition, camera);
            return TryWorldToGrid(worldPos, out tile);
        }

        public Vector2Int ScreenToGrid(Vector2 screenPosition)
            => TryScreenToGrid(screenPosition, out Vector2Int tile)
                ? tile
                : Vector2Int.zero;

        public bool TryWorldToGrid(Vector3 worldPosition, out Vector2Int tile)
        {
            if (TryUseGeneratedGrid(worldPosition, out tile))
                return true;

            if (_gridProjection == null)
            {
                tile = default;
                return false;
            }

            tile = _gridProjection.WorldToGrid(worldPosition);
            return true;
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
            => TryWorldToGrid(worldPosition, out Vector2Int tile)
                ? tile
                : Vector2Int.zero;

        public bool TryResolveTerrainSurfaceTile(Ray ray, out Vector2Int tile)
        {
            tile = default;
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                || !RefreshSurfaceHeightCandidates())
            {
                return false;
            }

            bool found = false;
            float nearestDistance = float.PositiveInfinity;
            for (int index = 0; index < _surfaceHeightCandidates.Count; index++)
            {
                float candidateY = _surfaceHeightCandidates[index];
                var plane = new Plane(Vector3.up, new Vector3(0f, candidateY, 0f));
                if (!plane.Raycast(ray, out float distance)
                    || distance < 0f
                    || distance >= nearestDistance)
                {
                    continue;
                }

                Vector3 worldPoint = ray.GetPoint(distance);
                if (!TryUseGeneratedGrid(worldPoint, out Vector2Int candidateTile)
                    || _gridService == null
                    || !_gridService.TryGetTileData(candidateTile, out _)
                    || _terrainSurfaceQuery == null
                    || !_terrainSurfaceQuery.TryGetTerrainSurfaceY(candidateTile, out float actualSurfaceY)
                    || !IsFinite(actualSurfaceY)
                    || Mathf.Abs(actualSurfaceY - candidateY) > SurfaceHeightMatchEpsilon)
                {
                    continue;
                }

                tile = candidateTile;
                nearestDistance = distance;
                found = true;
            }

            if (found)
            {
                _loggedSurfaceFallback = false;
                return true;
            }

            if (!_loggedSurfaceFallback)
            {
                _loggedSurfaceFallback = true;
                Debug.LogWarning(
                    $"{LogTag} Surface-aware pointer mapping found no generated terrain surface; falling back to the grid plane.");
            }

            return false;
        }

        private Vector3 ScreenToWorldOnGridPlane(Vector2 screenPosition, Camera camera)
        {
            if (camera == null)
                return Vector3.zero;

            Ray ray = camera.ScreenPointToRay(screenPosition);
            Plane plane = _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? new Plane(Vector3.up, new Vector3(0f, ResolveGridPlaneY(), 0f))
                : new Plane(Vector3.forward, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);

            return camera.ScreenToWorldPoint(new Vector3(
                screenPosition.x,
                screenPosition.y,
                -camera.transform.position.z));
        }

        private bool RefreshSurfaceHeightCandidates()
        {
            if (_gridGeometry == null
                || _gridService == null
                || _terrainSurfaceQuery == null
                || !_terrainSurfaceQuery.HasExplicitTerrainSurfaceMap
                || _gridService.GridWidth <= 0
                || _gridService.GridHeight <= 0)
            {
                return false;
            }

            int terrainVersion = _terrainSurfaceQuery.TerrainSurfaceVersion;
            bool dimensionsChanged =
                _cachedGridWidth != _gridService.GridWidth
                || _cachedGridHeight != _gridService.GridHeight;
            bool terrainChanged = _cachedTerrainSurfaceVersion != terrainVersion;
            if (_surfaceHeightCacheInitialized
                && !dimensionsChanged
                && !terrainChanged)
            {
                return _surfaceHeightCandidates.Count > 0;
            }

            _cachedGridWidth = _gridService.GridWidth;
            _cachedGridHeight = _gridService.GridHeight;
            _cachedTerrainSurfaceVersion = terrainVersion;
            _surfaceHeightCacheInitialized = true;
            _surfaceHeightCandidates.Clear();
            _surfaceHeightKeys.Clear();

            for (int y = 0; y < _cachedGridHeight; y++)
            {
                for (int x = 0; x < _cachedGridWidth; x++)
                {
                    var position = new Vector2Int(x, y);
                    if (!_gridService.TryGetTileData(position, out _)
                        || !_terrainSurfaceQuery.TryGetTerrainSurfaceY(position, out float surfaceY)
                        || !IsFinite(surfaceY))
                    {
                        continue;
                    }

                    int key = Mathf.RoundToInt(surfaceY * SurfaceHeightQuantization);
                    if (_surfaceHeightKeys.Add(key))
                        _surfaceHeightCandidates.Add(surfaceY);
                }
            }

            _surfaceHeightCandidates.Sort((left, right) => right.CompareTo(left));
            return _surfaceHeightCandidates.Count > 0;
        }

        private bool TryUseGeneratedGrid(Vector3 worldPosition, out Vector2Int tile)
        {
            tile = default;
            return _gridGeometry != null
                   && _gridGeometry.TryGetCellAtWorld(worldPosition, out tile);
        }

        private float ResolveGridPlaneY()
        {
            return _gridGeometry != null && _gridGeometry.TryGetGridPlaneY(out float y)
                ? y
                : 0f;
        }

        private Camera ResolveCamera()
            => _camera != null && _camera.isActiveAndEnabled
                ? _camera
                : Camera.main;

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
