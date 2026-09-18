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

        private readonly LazyInject<IConstructionSessionCommands> _constructionService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IConstructionWallSettingsProvider _wallSettingsProvider;
        private readonly List<OpenNode> _openHeap = new();
        private readonly Dictionary<Vector2Int, Vector2Int> _cameFrom = new();
        private readonly Dictionary<Vector2Int, int> _gScore = new();
        private readonly HashSet<Vector2Int> _closed = new();
        private readonly List<Vector2Int> _pathBuffer = new();

        [Inject]
        public WallPathfinder(
            LazyInject<IConstructionSessionCommands> constructionService,
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

            while (_openHeap.Count > 0)
            {
                OpenNode currentNode = PopOpenNode();
                Vector2Int current = currentNode.Position;

                if (_closed.Contains(current))
                    continue;

                if (!_gScore.TryGetValue(
                        current,
                        out int currentG)
                    || currentNode.GScore != currentG)
                {
                    continue;
                }

                if (current == endPosition)
                    return BuildResultPath(current);

                _closed.Add(current);

                for (int i = 0;
                     i < NeighborDirections.Length;
                     i++)
                {
                    Vector2Int neighbor =
                        current + NeighborDirections[i];
                    if (!_gridService.TryGetTileData(
                            neighbor,
                            out _))
                    {
                        continue;
                    }

                    if (_closed.Contains(neighbor))
                        continue;

                    if (!IsTilePassableForWallPath(
                            neighbor,
                            startPosition,
                            endPosition,
                            selectedCollection))
                    {
                        continue;
                    }

                    int tentativeG =
                        currentG
                        + GetTraversalCost(
                            neighbor,
                            selectedCollection);
                    if (_gScore.TryGetValue(
                            neighbor,
                            out int knownG)
                        && tentativeG >= knownG)
                    {
                        continue;
                    }

                    _cameFrom[neighbor] = current;
                    _gScore[neighbor] = tentativeG;

                    int fScore =
                        tentativeG
                        + Heuristic(
                            neighbor,
                            endPosition);
                    PushOpenNode(
                        new OpenNode(
                            neighbor,
                            tentativeG,
                            fScore));
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

        private void ResetWorkspace(
            Vector2Int startPosition,
            Vector2Int endPosition)
        {
            _openHeap.Clear();
            _cameFrom.Clear();
            _gScore.Clear();
            _closed.Clear();
            _pathBuffer.Clear();

            _gScore[startPosition] = 0;
            PushOpenNode(
                new OpenNode(
                    startPosition,
                    0,
                    Heuristic(
                        startPosition,
                        endPosition)));
        }

        private void PushOpenNode(OpenNode node)
        {
            int index = _openHeap.Count;
            _openHeap.Add(node);

            while (index > 0)
            {
                int parentIndex = (index - 1) >> 1;
                if (!IsHigherPriority(
                        _openHeap[index],
                        _openHeap[parentIndex]))
                {
                    break;
                }

                OpenNode parent = _openHeap[parentIndex];
                _openHeap[parentIndex] = _openHeap[index];
                _openHeap[index] = parent;
                index = parentIndex;
            }
        }

        private OpenNode PopOpenNode()
        {
            OpenNode root = _openHeap[0];
            int lastIndex = _openHeap.Count - 1;
            OpenNode last = _openHeap[lastIndex];
            _openHeap.RemoveAt(lastIndex);

            if (_openHeap.Count == 0)
                return root;

            _openHeap[0] = last;
            int index = 0;
            while (true)
            {
                int left = (index << 1) + 1;
                if (left >= _openHeap.Count)
                    break;

                int right = left + 1;
                int best = right < _openHeap.Count
                    && IsHigherPriority(
                        _openHeap[right],
                        _openHeap[left])
                    ? right
                    : left;

                if (!IsHigherPriority(
                        _openHeap[best],
                        _openHeap[index]))
                {
                    break;
                }

                OpenNode current = _openHeap[index];
                _openHeap[index] = _openHeap[best];
                _openHeap[best] = current;
                index = best;
            }

            return root;
        }

        private static bool IsHigherPriority(
            OpenNode left,
            OpenNode right)
        {
            if (left.FScore != right.FScore)
                return left.FScore < right.FScore;

            return left.GScore > right.GScore;
        }

        private static int Heuristic(Vector2Int from, Vector2Int to)
        {
            return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
        }

        private readonly struct OpenNode
        {
            public OpenNode(
                Vector2Int position,
                int gScore,
                int fScore)
            {
                Position = position;
                GScore = gScore;
                FScore = fScore;
            }

            public Vector2Int Position { get; }
            public int GScore { get; }
            public int FScore { get; }
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
