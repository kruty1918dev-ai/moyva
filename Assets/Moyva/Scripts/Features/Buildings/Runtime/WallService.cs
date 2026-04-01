using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Buildings.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// Сервіс будування стін.
    /// Показує 8 точок з'єднання навколо розміщеної стіни
    /// і підтримує прокладання шляху стіни між двома позиціями.
    /// </summary>
    internal sealed class WallService : IWallService, IInitializable, IDisposable
    {
        private readonly IBuildingPlacementService _placementService;
        private readonly IGridService _gridService;
        private readonly BuildingRegistrySO _buildingRegistry;
        private readonly SignalBus _signalBus;
        private readonly DiContainer _container;
        private readonly GameObject _connectionPointPrefab;

        private readonly List<GameObject> _activeConnectionPoints = new List<GameObject>();

        // 8 напрямків навколо тайлу (ортогональні + діагональні)
        private static readonly Vector2Int[] Directions =
        {
            new Vector2Int( 0,  1), // північ
            new Vector2Int( 1,  1), // північ-схід
            new Vector2Int( 1,  0), // схід
            new Vector2Int( 1, -1), // південь-схід
            new Vector2Int( 0, -1), // південь
            new Vector2Int(-1, -1), // південь-захід
            new Vector2Int(-1,  0), // захід
            new Vector2Int(-1,  1)  // північ-захід
        };

        public WallService(
            IBuildingPlacementService placementService,
            IGridService gridService,
            BuildingRegistrySO buildingRegistry,
            SignalBus signalBus,
            DiContainer container,
            [Inject(Id = "WallConnectionPointPrefab", Optional = true)] GameObject connectionPointPrefab)
        {
            _placementService = placementService;
            _gridService = gridService;
            _buildingRegistry = buildingRegistry;
            _signalBus = signalBus;
            _container = container;
            _connectionPointPrefab = connectionPointPrefab;

            if (_connectionPointPrefab == null)
                Debug.LogWarning("[WallService] WallConnectionPointPrefab не призначено в BuildingsInstaller. Точки з'єднання стін не відображатимуться.");
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BuildingPreviewPlacedSignal>(OnBuildingPreviewPlaced);
            _signalBus.Subscribe<BuildingConstructionConfirmedSignal>(OnSessionEnded);
            _signalBus.Subscribe<BuildingConstructionCanceledSignal>(OnSessionEnded);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<BuildingPreviewPlacedSignal>(OnBuildingPreviewPlaced);
            _signalBus.Unsubscribe<BuildingConstructionConfirmedSignal>(OnSessionEnded);
            _signalBus.Unsubscribe<BuildingConstructionCanceledSignal>(OnSessionEnded);
            HideConnectionPoints();
        }

        private void OnBuildingPreviewPlaced(BuildingPreviewPlacedSignal signal)
        {
            var config = _buildingRegistry.GetConfig(signal.TypeId);
            if (config != null && config.IsWall)
                ShowConnectionPoints(signal.Position);
        }

        private void OnSessionEnded(BuildingConstructionConfirmedSignal _) => HideConnectionPoints();
        private void OnSessionEnded(BuildingConstructionCanceledSignal _) => HideConnectionPoints();

        public void ShowConnectionPoints(Vector2Int wallPosition)
        {
            HideConnectionPoints();

            if (_connectionPointPrefab == null) return;

            foreach (var dir in Directions)
            {
                var targetPos = wallPosition + dir;
                if (!_gridService.TryGetTileData(targetPos, out _)) continue;

                var pointObj = _container.InstantiatePrefab(
                    _connectionPointPrefab,
                    new Vector3(targetPos.x, targetPos.y, -0.2f),
                    Quaternion.identity,
                    null);

                var connectionPoint = pointObj.GetComponent<WallConnectionPoint>();
                if (connectionPoint != null)
                    connectionPoint.Setup(wallPosition, targetPos);

                _activeConnectionPoints.Add(pointObj);
            }
        }

        public void HideConnectionPoints()
        {
            foreach (var point in _activeConnectionPoints)
            {
                if (point != null)
                    UnityEngine.Object.Destroy(point);
            }
            _activeConnectionPoints.Clear();
        }

        public void DrawWallPath(Vector2Int startPosition, Vector2Int endPosition)
        {
            var positions = GetBresenhamLinePath(startPosition, endPosition);
            foreach (var pos in positions)
            {
                if (_placementService.CanPlace(pos))
                    _placementService.TryPlace(pos);
            }
        }

        /// <summary>
        /// Алгоритм Брезенхема для отримання списку тайлів між двома точками
        /// </summary>
        private static List<Vector2Int> GetBresenhamLinePath(Vector2Int start, Vector2Int end)
        {
            var path = new List<Vector2Int>();
            int x0 = start.x, y0 = start.y;
            int x1 = end.x, y1 = end.y;
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                path.Add(new Vector2Int(x0, y0));
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }

            return path;
        }
    }
}
