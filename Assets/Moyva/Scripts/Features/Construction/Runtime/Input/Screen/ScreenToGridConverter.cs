using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ScreenToGridConverter : IScreenToGridConverter
    {
        private const float SurfaceHeightQuantization = 1000f;
        private const float SurfaceHeightMatchEpsilon = 0.002f;
        private const float SurfaceHeightCacheLifetimeSeconds = 1f;
        private const string LogTag = "[MoyvaBuildGridDiag]";

        private readonly Camera _camera;
        private readonly IGridProjection _gridProjection;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly IGridService _gridService;
        private readonly IGeneratedTerrainLevelQuery _terrainLevelQuery;
        private readonly List<float> _surfaceHeightCandidates = new();
        private readonly HashSet<int> _surfaceHeightKeys = new();

        private int _cachedGridWidth = -1;
        private int _cachedGridHeight = -1;
        private float _nextSurfaceHeightRefreshAt;
        private bool _loggedSurfaceFallback;

        public ScreenToGridConverter(Camera camera)
            : this(camera, null, null, null, null)
        {
        }

        [Inject]
        public ScreenToGridConverter(
            Camera camera,
            [InjectOptional] IGridProjection gridProjection,
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null,
            [InjectOptional] IGridService gridService = null,
            [InjectOptional] IGeneratedTerrainLevelQuery terrainLevelQuery = null)
        {
            _camera = camera;
            _gridProjection = gridProjection ?? new OrthogonalGridProjection();
            _gridGeometry = gridGeometry;
            _gridService = gridService;
            _terrainLevelQuery = terrainLevelQuery;
        }

        public Vector2Int ScreenToGrid(Vector2 screenPosition)
        {
            if (_camera != null)
            {
                Ray ray = _camera.ScreenPointToRay(screenPosition);
                if (TryResolveTerrainSurfaceTile(ray, out Vector2Int surfaceTile))
                    return surfaceTile;
            }

            Vector3 worldPos = ScreenToWorldOnGridPlane(screenPosition);
            return TryUseGeneratedGrid(worldPos, out Vector2Int tile)
                ? tile
                : _gridProjection.WorldToGrid(worldPos);
        }

        public Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            Vector3 projectedWorldPosition = _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? new Vector3(worldPosition.x, 0f, worldPosition.y)
                : new Vector3(worldPosition.x, worldPosition.y, 0f);
            return TryUseGeneratedGrid(projectedWorldPosition, out Vector2Int tile)
                ? tile
                : _gridProjection.WorldToGrid(projectedWorldPosition);
        }

        internal bool TryResolveTerrainSurfaceTile(Ray ray, out Vector2Int tile)
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
                    || _terrainLevelQuery == null
                    || !_terrainLevelQuery.TryGetTerrainSurfaceY(candidateTile, out float actualSurfaceY)
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
                    $"{LogTag} Surface-aware pointer mapping found no generated terrain surface; " +
                    "falling back to the legacy single grid plane.");
            }

            return false;
        }

        private Vector3 ScreenToWorldOnGridPlane(Vector2 screenPosition)
        {
            if (_camera == null)
                return Vector3.zero;

            Ray ray = _camera.ScreenPointToRay(screenPosition);
            Plane plane = _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? new Plane(Vector3.up, new Vector3(0f, ResolveGridPlaneY(), 0f))
                : new Plane(Vector3.forward, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);

            return _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_camera.transform.position.z));
        }

        private bool RefreshSurfaceHeightCandidates()
        {
            if (_gridGeometry == null
                || _gridService == null
                || _terrainLevelQuery == null
                || !_terrainLevelQuery.HasExplicitTerrainSurfaceMap
                || _gridService.GridWidth <= 0
                || _gridService.GridHeight <= 0)
            {
                return false;
            }

            float now = Time.realtimeSinceStartup;
            bool dimensionsChanged = _cachedGridWidth != _gridService.GridWidth
                || _cachedGridHeight != _gridService.GridHeight;
            if (!dimensionsChanged
                && _surfaceHeightCandidates.Count > 0
                && now < _nextSurfaceHeightRefreshAt)
            {
                return true;
            }

            _cachedGridWidth = _gridService.GridWidth;
            _cachedGridHeight = _gridService.GridHeight;
            _nextSurfaceHeightRefreshAt = now + SurfaceHeightCacheLifetimeSeconds;
            _surfaceHeightCandidates.Clear();
            _surfaceHeightKeys.Clear();

            for (int y = 0; y < _cachedGridHeight; y++)
            {
                for (int x = 0; x < _cachedGridWidth; x++)
                {
                    var position = new Vector2Int(x, y);
                    if (!_gridService.TryGetTileData(position, out _)
                        || !_terrainLevelQuery.TryGetTerrainSurfaceY(position, out float surfaceY)
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
            return _gridGeometry != null && _gridGeometry.TryGetCellAtWorld(worldPosition, out tile);
        }

        private float ResolveGridPlaneY()
        {
            return _gridGeometry != null && _gridGeometry.TryGetGridPlaneY(out float y)
                ? y
                : 0f;
        }

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
