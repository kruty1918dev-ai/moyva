using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallPlacementService : IWallPlacementService, IInitializable, IDisposable
    {
        private readonly IConstructionService _constructionService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IScreenToGridConverter _screenToGridConverter;
        private readonly SignalBus _signalBus;

        private Vector2Int? _dragStartPosition;

        [Inject]
        public WallPlacementService(
            IConstructionService constructionService,
            IBuildingRegistry buildingRegistry,
            IObjectsMapService objectsMapService,
            IScreenToGridConverter screenToGridConverter,
            SignalBus signalBus)
        {
            _constructionService = constructionService;
            _buildingRegistry = buildingRegistry;
            _objectsMapService = objectsMapService;
            _screenToGridConverter = screenToGridConverter;
            _signalBus = signalBus;
        }

        public void Initialize() { }

        public void Dispose() { }

        public void ShowWallHandles(Vector2Int wallPosition)
        {
            _signalBus.Fire(new ShowWallHandlesSignal { Center = wallPosition, Hide = false });
        }

        public void DragWall(Vector2Int startPosition, Vector2 touchWorldPosition)
        {
            _dragStartPosition = startPosition;
            Vector2Int endGrid = _screenToGridConverter.WorldToGrid(touchWorldPosition);

            var path = BuildPath(startPosition, endGrid);
            for (int i = 0; i < path.Count; i++)
                _constructionService.TryPreviewAt(path[i]);
        }

        public IReadOnlyList<Vector2Int> BuildPath(Vector2Int startPosition, Vector2Int endPosition)
        {
            var result = new List<Vector2Int>();
            foreach (Vector2Int tile in BresenhamLine(startPosition, endPosition))
                result.Add(tile);
            return result;
        }

        public bool IsWallOrGate(string buildingId)
        {
            return _buildingRegistry.GetWallCollectionByBuildingId(buildingId) != null;
        }

        public bool IsWall(string buildingId)
        {
            var collection = _buildingRegistry.GetWallCollectionByBuildingId(buildingId);
            return collection != null && collection.IsWall(buildingId);
        }

        public bool IsGate(string buildingId)
        {
            var collection = _buildingRegistry.GetWallCollectionByBuildingId(buildingId);
            return collection != null && collection.IsGate(buildingId);
        }

        public bool CanReplaceWallWithGate(Vector2Int position, string gateBuildingId, out string replacedWallId)
        {
            replacedWallId = null;

            var collection = _buildingRegistry.GetWallCollectionByBuildingId(gateBuildingId);
            if (collection == null || !collection.IsGate(gateBuildingId))
                return false;

            if (!_objectsMapService.TryGetOccupant(position, out var occupantId))
                return false;

            if (!collection.IsWall(occupantId))
                return false;

            replacedWallId = occupantId;
            return true;
        }

        public bool TryResolvePlacedVisual(Vector2Int position, string occupantId, out GameObject prefab, out Quaternion rotation)
        {
            prefab = null;
            rotation = Quaternion.identity;

            var collection = _buildingRegistry.GetWallCollectionByBuildingId(occupantId);
            if (collection == null)
                return false;

            if (collection.IsGate(occupantId))
            {
                prefab = collection.GatePrefab;
                if (prefab == null)
                    prefab = _buildingRegistry.GetById(occupantId)?.Prefab;
                return prefab != null;
            }

            if (!collection.IsWall(occupantId))
                return false;

            bool n = IsConnected(position + Vector2Int.up, collection);
            bool e = IsConnected(position + Vector2Int.right, collection);
            bool s = IsConnected(position + Vector2Int.down, collection);
            bool w = IsConnected(position + Vector2Int.left, collection);
            int connections = (n ? 1 : 0) + (e ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0);

            switch (connections)
            {
                case 0:
                    // Одиночний сегмент без сусідів — fallback на горизонтальну
                    prefab = collection.HorizontalPrefab;
                    break;
                case 1:
                    prefab = (n || s) ? collection.VerticalPrefab : collection.HorizontalPrefab;
                    break;
                case 2:
                    if (n && s)
                    {
                        prefab = collection.VerticalPrefab;
                    }
                    else if (e && w)
                    {
                        prefab = collection.HorizontalPrefab;
                    }
                    else if (n && e)
                    {
                        prefab = collection.CornerNorthEastPrefab;
                    }
                    else if (n && w)
                    {
                        prefab = collection.CornerNorthWestPrefab;
                    }
                    else if (s && e)
                    {
                        prefab = collection.CornerSouthEastPrefab;
                    }
                    else
                    {
                        prefab = collection.CornerSouthWestPrefab;
                    }
                    break;
                case 3:
                    // T-подібне з'єднання (3 сусіди) — окремого варіанта немає.
                    // Якщо є обидва кінці прямої осі — показуємо прямий сегмент.
                    // n+s+e (без w) або n+s+w (без e) → вертикальна
                    // e+w+n (без s) або e+w+s (без n) → горизонтальна
                    if (n && s)
                        prefab = collection.VerticalPrefab;
                    else if (e && w)
                        prefab = collection.HorizontalPrefab;
                    else if (n && e)
                        prefab = collection.CornerNorthEastPrefab;
                    else if (n && w)
                        prefab = collection.CornerNorthWestPrefab;
                    else if (s && e)
                        prefab = collection.CornerSouthEastPrefab;
                    else
                        prefab = collection.CornerSouthWestPrefab;
                    break;
                default:
                    // Хрестоподібне з'єднання (4 сусіди) — окремого варіанта немає, fallback горизонтальна.
                    prefab = collection.HorizontalPrefab;
                    break;
            }

            if (prefab == null)
                prefab = _buildingRegistry.GetById(occupantId)?.Prefab;

            return prefab != null;
        }

        public void EndDrag()
        {
            if (_dragStartPosition.HasValue)
            {
                _signalBus.Fire(new ShowWallHandlesSignal { Center = _dragStartPosition.Value, Hide = true });
                _dragStartPosition = null;
            }
        }

        private bool IsConnected(Vector2Int position, WallCollectionDefinition collection)
        {
            return _objectsMapService.TryGetOccupant(position, out var neighborId)
                && collection.ContainsBuilding(neighborId);
        }

        private static IEnumerable<Vector2Int> BresenhamLine(Vector2Int start, Vector2Int end)
        {
            int x0 = start.x;
            int y0 = start.y;
            int x1 = end.x;
            int y1 = end.y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                yield return new Vector2Int(x0, y0);
                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}
