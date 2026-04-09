using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallPlacementService : IWallPlacementService, IInitializable, IDisposable
    {
        private readonly LazyInject<IConstructionService> _constructionService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IAutoTileVariantResolver _autoTileVariantResolver;
        private readonly IObjectTypePicker _objectTypePicker;
        private readonly IScreenToGridConverter _screenToGridConverter;
        private readonly SignalBus _signalBus;

        private Vector2Int? _dragStartPosition;

        [Inject]
        public WallPlacementService(
            LazyInject<IConstructionService> constructionService,
            IBuildingRegistry buildingRegistry,
            IGridService gridService,
            IObjectsMapService objectsMapService,
            IAutoTileVariantResolver autoTileVariantResolver,
            IObjectTypePicker objectTypePicker,
            IScreenToGridConverter screenToGridConverter,
            SignalBus signalBus)
        {
            _constructionService = constructionService;
            _buildingRegistry = buildingRegistry;
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _autoTileVariantResolver = autoTileVariantResolver;
            _objectTypePicker = objectTypePicker;
            _screenToGridConverter = screenToGridConverter;
            _signalBus = signalBus;
        }

        private const string SlotHorizontal = "horizontal";
        private const string SlotVertical = "vertical";
        private const string SlotCornerNorthEast = "corner_ne";
        private const string SlotCornerNorthWest = "corner_nw";
        private const string SlotCornerSouthEast = "corner_se";
        private const string SlotCornerSouthWest = "corner_sw";

        private static readonly IReadOnlyDictionary<TopologyCaseType, string> WallCaseToSlot =
            new Dictionary<TopologyCaseType, string>
            {
                [TopologyCaseType.Isolated] = SlotHorizontal,
                [TopologyCaseType.CrossIntersection] = SlotHorizontal,

                [TopologyCaseType.TJunctionOpenNorth] = SlotHorizontal,
                [TopologyCaseType.TJunctionOpenSouth] = SlotHorizontal,
                [TopologyCaseType.TJunctionOpenEast] = SlotVertical,
                [TopologyCaseType.TJunctionOpenWest] = SlotVertical,

                [TopologyCaseType.Vertical] = SlotVertical,
                [TopologyCaseType.VerticalLeft] = SlotVertical,
                [TopologyCaseType.VerticalRight] = SlotVertical,

                [TopologyCaseType.Horizontal] = SlotHorizontal,
                [TopologyCaseType.HorizontalTop] = SlotHorizontal,
                [TopologyCaseType.HorizontalBottom] = SlotHorizontal,

                [TopologyCaseType.EndNorth] = SlotVertical,
                [TopologyCaseType.EndSouth] = SlotVertical,
                [TopologyCaseType.EndEast] = SlotHorizontal,
                [TopologyCaseType.EndWest] = SlotHorizontal,

                // Atlas-position semantics: case name maps directly to atlas slot name.
                [TopologyCaseType.CornerNorthEast] = SlotCornerNorthEast,
                [TopologyCaseType.CornerNorthWest] = SlotCornerNorthWest,
                [TopologyCaseType.CornerSouthEast] = SlotCornerSouthEast,
                [TopologyCaseType.CornerSouthWest] = SlotCornerSouthWest,
            };

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
                _constructionService.Value.TryPreviewAt(path[i]);
        }

        public IReadOnlyList<Vector2Int> BuildPath(Vector2Int startPosition, Vector2Int endPosition)
        {
            if (!_gridService.TryGetTileData(startPosition, out _))
                return Array.Empty<Vector2Int>();

            if (!_gridService.TryGetTileData(endPosition, out _))
                return Array.Empty<Vector2Int>();

            if (startPosition == endPosition)
                return new[] { startPosition };

            var selectedBuildingId = _constructionService.Value.GetSelectedBuildingId();
            var selectedCollection = _buildingRegistry.GetWallCollectionByBuildingId(selectedBuildingId);

            var openSet = new List<Vector2Int> { startPosition };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, int> { [startPosition] = 0 };
            var fScore = new Dictionary<Vector2Int, int> { [startPosition] = Heuristic(startPosition, endPosition) };
            var closed = new HashSet<Vector2Int>();

            while (openSet.Count > 0)
            {
                int currentIndex = 0;
                var current = openSet[0];
                int currentF = fScore.TryGetValue(current, out var score) ? score : int.MaxValue;

                for (int i = 1; i < openSet.Count; i++)
                {
                    var candidate = openSet[i];
                    int candidateF = fScore.TryGetValue(candidate, out var candidateScore) ? candidateScore : int.MaxValue;
                    if (candidateF < currentF)
                    {
                        current = candidate;
                        currentF = candidateF;
                        currentIndex = i;
                    }
                }

                if (current == endPosition)
                    return ReconstructPath(cameFrom, current);

                openSet.RemoveAt(currentIndex);
                closed.Add(current);

                foreach (var neighbor in GetNeighbors4(current))
                {
                    if (!_gridService.TryGetTileData(neighbor, out _))
                        continue;

                    if (closed.Contains(neighbor))
                        continue;

                    if (!IsTilePassableForWallPath(neighbor, startPosition, endPosition, selectedCollection))
                        continue;

                    int tentativeG = gScore[current] + 1;
                    if (!gScore.TryGetValue(neighbor, out var knownG) || tentativeG < knownG)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Heuristic(neighbor, endPosition);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            // Якщо шлях до end не знайдено — повертаємо старт, щоб не виникали "дірки" через частковий skip.
            return new[] { startPosition };
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

            string wallAtPosition = null;

            // 1) placed-стіна
            if (_objectsMapService.TryGetOccupant(position, out var occupantId) && collection.IsWall(occupantId))
                wallAtPosition = occupantId;

            // 2) pending-стіна (preview), якщо placed немає
            if (string.IsNullOrWhiteSpace(wallAtPosition)
                && _constructionService.Value.TryGetPendingBuildingIdAt(position, out var pendingId)
                && collection.IsWall(pendingId))
            {
                wallAtPosition = pendingId;
            }

            if (string.IsNullOrWhiteSpace(wallAtPosition))
                return false;

            // Для воріт дозволяємо заміну лише горизонтального сегмента,
            // але з урахуванням pending-сусідів, щоб preview-плейс працював коректно.
            if (!IsHorizontalWallSegment(position, collection, includePendingNeighbors: true))
                return false;

            replacedWallId = wallAtPosition;
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

            var mask = new TopologyNeighborMask(
                north: n,
                northEast: false,
                east: e,
                southEast: false,
                south: s,
                southWest: false,
                west: w,
                northWest: false);

            if (_objectTypePicker.TryPickId(occupantId, mask, out var resolvedId))
                prefab = _buildingRegistry.GetById(resolvedId)?.Prefab;

            if (prefab == null)
                prefab = ResolveWallPrefab(collection, mask, _autoTileVariantResolver);

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

        public bool TryResolvePreviewVisual(Vector2Int position, string buildingId, out GameObject prefab)
        {
            prefab = null;

            var collection = _buildingRegistry.GetWallCollectionByBuildingId(buildingId);
            if (collection == null)
                return false;

            if (collection.IsGate(buildingId))
            {
                prefab = collection.GatePrefab;
                if (prefab == null)
                    prefab = _buildingRegistry.GetById(buildingId)?.Prefab;
                return prefab != null;
            }

            if (!collection.IsWall(buildingId))
                return false;

            bool n = IsConnectedForPreview(position + Vector2Int.up, collection);
            bool e = IsConnectedForPreview(position + Vector2Int.right, collection);
            bool s = IsConnectedForPreview(position + Vector2Int.down, collection);
            bool w = IsConnectedForPreview(position + Vector2Int.left, collection);

            var mask = new TopologyNeighborMask(
                north: n,
                northEast: false,
                east: e,
                southEast: false,
                south: s,
                southWest: false,
                west: w,
                northWest: false);

            if (_objectTypePicker.TryPickId(buildingId, mask, out var resolvedId))
                prefab = _buildingRegistry.GetById(resolvedId)?.Prefab;

            if (prefab == null)
                prefab = ResolveWallPrefab(collection, mask, _autoTileVariantResolver);

            if (prefab == null)
                prefab = _buildingRegistry.GetById(buildingId)?.Prefab;

            return prefab != null;
        }

        private bool IsConnected(Vector2Int position, WallCollectionDefinition collection)
        {
            return _objectsMapService.TryGetOccupant(position, out var neighborId)
                && collection.ContainsBuilding(neighborId);
        }

        /// <summary>Перевіряє з'єднання як для placed, так і для pending стін.</summary>
        private bool IsConnectedForPreview(Vector2Int position, WallCollectionDefinition collection)
        {
            // Перевірка placed
            if (_objectsMapService.TryGetOccupant(position, out var neighborId)
                && collection.ContainsBuilding(neighborId))
                return true;

            // Перевірка pending
            if (_constructionService.Value.TryGetPendingBuildingIdAt(position, out var pendingBuildingId)
                && collection.ContainsBuilding(pendingBuildingId))
            {
                return true;
            }

            return false;
        }

        private bool IsHorizontalWallSegment(Vector2Int position, WallCollectionDefinition collection, bool includePendingNeighbors)
        {
            bool n = includePendingNeighbors
                ? IsConnectedForPreview(position + Vector2Int.up, collection)
                : IsConnected(position + Vector2Int.up, collection);
            bool e = includePendingNeighbors
                ? IsConnectedForPreview(position + Vector2Int.right, collection)
                : IsConnected(position + Vector2Int.right, collection);
            bool s = includePendingNeighbors
                ? IsConnectedForPreview(position + Vector2Int.down, collection)
                : IsConnected(position + Vector2Int.down, collection);
            bool w = includePendingNeighbors
                ? IsConnectedForPreview(position + Vector2Int.left, collection)
                : IsConnected(position + Vector2Int.left, collection);

            bool hasHorizontalConnection = e || w;
            bool hasVerticalConnection = n || s;
            return hasHorizontalConnection && !hasVerticalConnection;
        }

        /// <summary>
        /// Єдина точка входу підбору wall prefab по топологічному кейсу.
        /// Якщо кейс відсутній у конфігурації, застосовується fallback.
        /// </summary>
        private static GameObject ResolveWallPrefab(
            WallCollectionDefinition collection,
            TopologyNeighborMask mask,
            IAutoTileVariantResolver resolver)
        {
            GameObject preferred = null;

            if (resolver != null && resolver.TryResolveId(mask, WallCaseToSlot, out var slotId, out _))
                preferred = GetPrefabBySlot(collection, slotId);

            if (preferred != null)
                return preferred;

            var fallback = FirstNotNull(
                collection.HorizontalPrefab,
                collection.VerticalPrefab,
                collection.CornerNorthEastPrefab,
                collection.CornerNorthWestPrefab,
                collection.CornerSouthEastPrefab,
                collection.CornerSouthWestPrefab);

            Debug.LogWarning(
                $"[WallPlacement] Відсутній prefab для кейсу (N={mask.North},E={mask.East},S={mask.South},W={mask.West}). " +
                $"Використано fallback '{(fallback != null ? fallback.name : "NULL")}'.");

            return fallback;
        }

        private static GameObject GetPrefabBySlot(WallCollectionDefinition collection, string slotId)
        {
            switch (slotId)
            {
                case SlotHorizontal:
                    return collection.HorizontalPrefab;
                case SlotVertical:
                    return collection.VerticalPrefab;
                case SlotCornerNorthEast:
                    return collection.CornerNorthEastPrefab;
                case SlotCornerNorthWest:
                    return collection.CornerNorthWestPrefab;
                case SlotCornerSouthEast:
                    return collection.CornerSouthEastPrefab;
                case SlotCornerSouthWest:
                    return collection.CornerSouthWestPrefab;
                default:
                    return null;
            }
        }

        private static GameObject FirstNotNull(params GameObject[] prefabs)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i] != null)
                    return prefabs[i];
            }

            return null;
        }

        private bool IsTilePassableForWallPath(
            Vector2Int position,
            Vector2Int startPosition,
            Vector2Int endPosition,
            WallCollectionDefinition selectedCollection)
        {
            if (position == startPosition || position == endPosition)
                return true;

            if (_constructionService.Value.HasPendingPlacementAt(position))
                return true;

            if (!_objectsMapService.TryGetOccupant(position, out var occupantId))
                return true;

            if (selectedCollection != null && selectedCollection.ContainsBuilding(occupantId))
                return true;

            return false;
        }

        private static int Heuristic(Vector2Int from, Vector2Int to)
        {
            return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
        }

        private static IReadOnlyList<Vector2Int> ReconstructPath(
            IReadOnlyDictionary<Vector2Int, Vector2Int> cameFrom,
            Vector2Int current)
        {
            var result = new List<Vector2Int> { current };
            while (cameFrom.TryGetValue(current, out var previous))
            {
                current = previous;
                result.Add(current);
            }

            result.Reverse();
            return result;
        }

        private static IEnumerable<Vector2Int> GetNeighbors4(Vector2Int position)
        {
            yield return position + Vector2Int.up;
            yield return position + Vector2Int.right;
            yield return position + Vector2Int.down;
            yield return position + Vector2Int.left;
        }
    }
}
