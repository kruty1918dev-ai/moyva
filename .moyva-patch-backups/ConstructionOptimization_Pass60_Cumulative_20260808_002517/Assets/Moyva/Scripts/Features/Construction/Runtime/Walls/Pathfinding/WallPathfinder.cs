using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallPathfinder : IWallPathfinder
    {
        private static readonly Vector2Int[] NeighborDirections =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        private readonly LazyInject<IConstructionService> _constructionService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IConstructionWallSettingsProvider _wallSettingsProvider;
        private readonly List<Vector2Int> _openSet = new();
        private readonly HashSet<Vector2Int> _openMembership = new();
        private readonly Dictionary<Vector2Int, Vector2Int> _cameFrom = new();
        private readonly Dictionary<Vector2Int, int> _gScore = new();
        private readonly Dictionary<Vector2Int, int> _fScore = new();
        private readonly HashSet<Vector2Int> _closed = new();
        private readonly List<Vector2Int> _pathBuffer = new();

        [Inject]
        public WallPathfinder(
            LazyInject<IConstructionService> constructionService,
            IBuildingRegistry buildingRegistry,
            IGridService gridService,
            IObjectsMapService objectsMapService,
            IWallTopologyService wallTopologyService,
            [InjectOptional] IConstructionWallSettingsProvider wallSettingsProvider = null)
        {
            _constructionService = constructionService;
            _buildingRegistry = buildingRegistry;
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _wallTopologyService = wallTopologyService;
            _wallSettingsProvider = wallSettingsProvider;
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

            ResetWorkspace(startPosition, endPosition);

            while (_openSet.Count > 0)
            {
                int currentIndex = 0;
                var current = _openSet[0];
                int currentF = _fScore.TryGetValue(current, out var score) ? score : int.MaxValue;

                for (int i = 1; i < _openSet.Count; i++)
                {
                    var candidate = _openSet[i];
                    int candidateF = _fScore.TryGetValue(candidate, out var candidateScore) ? candidateScore : int.MaxValue;
                    if (candidateF < currentF)
                    {
                        current = candidate;
                        currentF = candidateF;
                        currentIndex = i;
                    }
                }

                if (current == endPosition)
                    return BuildResultPath(current);

                _openSet.RemoveAt(currentIndex);
                _openMembership.Remove(current);
                _closed.Add(current);

                for (int i = 0; i < NeighborDirections.Length; i++)
                {
                    var neighbor = current + NeighborDirections[i];
                    if (!_gridService.TryGetTileData(neighbor, out _))
                        continue;

                    if (_closed.Contains(neighbor))
                        continue;

                    if (!IsTilePassableForWallPath(neighbor, startPosition, endPosition, selectedCollection))
                        continue;

                    int tentativeG = _gScore[current] + GetTraversalCost(neighbor, selectedCollection);
                    if (!_gScore.TryGetValue(neighbor, out var knownG) || tentativeG < knownG)
                    {
                        _cameFrom[neighbor] = current;
                        _gScore[neighbor] = tentativeG;
                        _fScore[neighbor] = tentativeG + Heuristic(neighbor, endPosition);

                        if (_openMembership.Add(neighbor))
                            _openSet.Add(neighbor);
                    }
                }
            }

            return new[] { startPosition };
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
            {
                if (!_constructionService.Value.TryGetPendingBuildingIdAt(position, out var pendingId))
                    return false;

                bool isPendingGate = _wallTopologyService.IsGate(pendingId);
                if (isPendingGate)
                    return _wallSettingsProvider?.AllowWallPathThroughGates ?? false;

                bool isPendingWall = _wallTopologyService.IsWall(pendingId);
                return isPendingWall && (_wallSettingsProvider?.AllowWallPathThroughPendingWalls ?? true);
            }

            if (!_objectsMapService.TryGetOccupant(position, out var occupantId))
                return true;

            if (selectedCollection != null && selectedCollection.ContainsBuilding(occupantId))
            {
                bool isGate = selectedCollection.IsGate(occupantId);
                if (isGate)
                    return _wallSettingsProvider?.AllowWallPathThroughGates ?? false;

                return _wallSettingsProvider?.AllowWallPathThroughExistingWalls ?? true;
            }

            return false;
        }

        private int GetTraversalCost(Vector2Int position, WallCollectionDefinition selectedCollection)
        {
            if ((_wallSettingsProvider?.WallPathMode ?? ConstructionWallPathMode.OrthogonalOnly) != ConstructionWallPathMode.ExistingWallsPreferred)
                return 1;

            if (_constructionService.Value.HasPendingPlacementAt(position)
                && _constructionService.Value.TryGetPendingBuildingIdAt(position, out var pendingId)
                && _wallTopologyService.IsWallOrGate(pendingId))
            {
                return 0;
            }

            if (selectedCollection != null
                && _objectsMapService.TryGetOccupant(position, out var occupantId)
                && selectedCollection.ContainsBuilding(occupantId))
            {
                return 0;
            }

            return 1;
        }

        private void ResetWorkspace(Vector2Int startPosition, Vector2Int endPosition)
        {
            _openSet.Clear();
            _openMembership.Clear();
            _cameFrom.Clear();
            _gScore.Clear();
            _fScore.Clear();
            _closed.Clear();
            _pathBuffer.Clear();

            _openSet.Add(startPosition);
            _openMembership.Add(startPosition);
            _gScore[startPosition] = 0;
            _fScore[startPosition] = Heuristic(startPosition, endPosition);
        }

        private static int Heuristic(Vector2Int from, Vector2Int to)
        {
            return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
        }

        private IReadOnlyList<Vector2Int> BuildResultPath(Vector2Int current)
        {
            _pathBuffer.Clear();
            _pathBuffer.Add(current);
            while (_cameFrom.TryGetValue(current, out var previous))
            {
                current = previous;
                _pathBuffer.Add(current);
            }

            _pathBuffer.Reverse();
            return new List<Vector2Int>(_pathBuffer);
        }
    }
}
