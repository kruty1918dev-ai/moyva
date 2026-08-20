using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Pathfinding.API;
using UnityEngine;

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
    public sealed class Pathfinder : IOccupiedCellPathfinder, ITraversalPathfinder, ICostAwarePathfinder
    {
        private readonly IGridService _gridService;
        private readonly ITileSettingsService _tileSettingsService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly INeighborhoodStrategy _neighborhoodStrategy;

        private readonly List<OpenNode> _openHeap = new();
        private readonly Dictionary<Vector2Int, Vector2Int> _cameFromScratch = new();
        private readonly Dictionary<Vector2Int, float> _gScoreScratch = new();
        private readonly List<Vector2Int> _neighborScratch = new List<Vector2Int>(8);

        private readonly struct OpenNode
        {
            public OpenNode(Vector2Int position, float g, float f)
            {
                Position = position;
                G = g;
                F = f;
            }

            public Vector2Int Position { get; }
            public float G { get; }
            public float F { get; }
        }

        public Pathfinder(
            IGridService gridService,
            ITileSettingsService tileSettingsService,
            IObjectsMapService objectsMapService)
            : this(gridService, tileSettingsService, objectsMapService, null)
        {
        }

        public Pathfinder(
            IGridService gridService,
            ITileSettingsService tileSettingsService,
            IObjectsMapService objectsMapService,
            INeighborhoodStrategy neighborhoodStrategy)
        {
            _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
            _tileSettingsService = tileSettingsService ?? throw new ArgumentNullException(nameof(tileSettingsService));
            _objectsMapService = objectsMapService ?? throw new ArgumentNullException(nameof(objectsMapService));
            _neighborhoodStrategy = neighborhoodStrategy ?? new MooreNeighborhoodStrategy();
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
            => FindPathInternal(start, end, null, null);

        public List<Vector2Int> FindPath(
            Vector2Int start,
            Vector2Int end,
            Func<Vector2Int, bool> canTraverseOccupied)
            => FindPathInternal(start, end, canTraverseOccupied, null);

        public List<Vector2Int> FindPathWithTraversal(
            Vector2Int start,
            Vector2Int end,
            Func<Vector2Int, bool> canTraverseCell)
        {
            if (canTraverseCell == null)
                throw new ArgumentNullException(nameof(canTraverseCell));

            return FindPathInternal(
                start,
                end,
                canTraverseCell,
                canTraverseCell);
        }

public List<Vector2Int> FindPathWithCosts(
	Vector2Int start,
	Vector2Int end,
	PathTraversalCostResolver traversalCostResolver)
{
	if (traversalCostResolver == null)
		throw new ArgumentNullException(nameof(traversalCostResolver));

	if (start == end)
		return new List<Vector2Int> { start };

	if (!IsWalkableTile(start) || !IsWalkableTile(end))
		return new List<Vector2Int>();

	_openHeap.Clear();
	_cameFromScratch.Clear();
	_gScoreScratch.Clear();

	_gScoreScratch[start] = 0f;
	PushOpenNode(
		new OpenNode(
			start,
			0f,
			EstimateCostAwareHeuristic(start, end)));

	while (_openHeap.Count > 0)
	{
		OpenNode currentNode = PopOpenNode();
		Vector2Int current = currentNode.Position;

		if (!_gScoreScratch.TryGetValue(
				current,
				out float bestKnownG)
			|| currentNode.G > bestKnownG + 0.0001f)
		{
			continue;
		}

		if (current == end)
			return ReconstructPath(_cameFromScratch, current);

		if (_neighborhoodStrategy
			is INonAllocNeighborhoodStrategy nonAlloc)
		{
			_neighborScratch.Clear();
			nonAlloc.CollectCandidateNeighbors(
				current,
				_neighborScratch);

			for (int index = 0;
				 index < _neighborScratch.Count;
				 index++)
			{
				RelaxCostAwareNeighbor(
					current,
					bestKnownG,
					_neighborScratch[index],
					end,
					traversalCostResolver);
			}
		}
		else
		{
			foreach (Vector2Int neighbor
				in _neighborhoodStrategy.GetNeighbors(
					current,
					_gridService))
			{
				RelaxCostAwareNeighbor(
					current,
					bestKnownG,
					neighbor,
					end,
					traversalCostResolver);
			}
		}
	}

	return new List<Vector2Int>();
}

private void RelaxCostAwareNeighbor(
	Vector2Int current,
	float bestKnownG,
	Vector2Int neighbor,
	Vector2Int end,
	PathTraversalCostResolver traversalCostResolver)
{
	if (!traversalCostResolver(
			current,
			neighbor,
			out float stepCost))
	{
		return;
	}

	stepCost = Mathf.Max(0.0001f, stepCost);
	float tentativeG = bestKnownG + stepCost;

	if (_gScoreScratch.TryGetValue(
			neighbor,
			out float previousG)
		&& tentativeG >= previousG - 0.0001f)
	{
		return;
	}

	_cameFromScratch[neighbor] = current;
	_gScoreScratch[neighbor] = tentativeG;

	float f =
		tentativeG
		+ EstimateCostAwareHeuristic(
			neighbor,
			end);

	PushOpenNode(
		new OpenNode(
			neighbor,
			tentativeG,
			f));
}

private float EstimateCostAwareHeuristic(
	Vector2Int from,
	Vector2Int to)
{
	const float minimumSupportedStepCost = 0.0001f;
	return _neighborhoodStrategy.EstimateDistance(from, to)
		* minimumSupportedStepCost;
}

        public IEnumerable<Vector2Int> GetNeighbors(Vector2Int position)
            => _neighborhoodStrategy.GetNeighbors(position, _gridService);

        private List<Vector2Int> FindPathInternal(
            Vector2Int start,
            Vector2Int end,
            Func<Vector2Int, bool> canTraverseOccupied,
            Func<Vector2Int, bool> canTraverseCell)
        {
            if (start == end)
            {
                if (canTraverseCell != null && !canTraverseCell(start))
                    return new List<Vector2Int>();
                return new List<Vector2Int> { start };
            }

            if (!IsWalkableTile(start) || !IsWalkableTile(end))
                return new List<Vector2Int>();

            if (canTraverseCell != null
                && (!canTraverseCell(start) || !canTraverseCell(end)))
            {
                return new List<Vector2Int>();
            }

            if (_objectsMapService.IsOccupied(end)
                && (canTraverseOccupied == null || !canTraverseOccupied(end)))
            {
                return new List<Vector2Int>();
            }

            _openHeap.Clear();
            _cameFromScratch.Clear();
            _gScoreScratch.Clear();

            _gScoreScratch[start] = 0f;
            PushOpenNode(new OpenNode(start, 0f, EstimateHeuristic(start, end)));

            while (_openHeap.Count > 0)
            {
                OpenNode currentNode = PopOpenNode();
                Vector2Int current = currentNode.Position;

                if (!_gScoreScratch.TryGetValue(current, out float bestKnownG)
                    || currentNode.G > bestKnownG + 0.0001f)
                {
                    continue;
                }

                if (current == end)
                    return ReconstructPath(_cameFromScratch, current);

                if (_neighborhoodStrategy is INonAllocNeighborhoodStrategy nonAlloc)
                {
                    _neighborScratch.Clear();
                    nonAlloc.CollectCandidateNeighbors(current, _neighborScratch);

                    for (int index = 0; index < _neighborScratch.Count; index++)
                    {
                        RelaxNeighbor(
                            current,
                            bestKnownG,
                            _neighborScratch[index],
                            start,
                            end,
                            canTraverseOccupied,
                            canTraverseCell);
                    }
                }
                else
                {
                    foreach (Vector2Int neighbor
                        in _neighborhoodStrategy.GetNeighbors(current, _gridService))
                    {
                        RelaxNeighbor(
                            current,
                            bestKnownG,
                            neighbor,
                            start,
                            end,
                            canTraverseOccupied,
                            canTraverseCell);
                    }
                }
            }

            return new List<Vector2Int>();
        }

        private bool IsWalkableTile(Vector2Int position)
        {
            if (!_gridService.TryGetTileData(position, out string tileId))
                return false;

            return !string.IsNullOrEmpty(tileId);
        }

        private void RelaxNeighbor(
            Vector2Int current,
            float bestKnownG,
            Vector2Int neighbor,
            Vector2Int start,
            Vector2Int end,
            Func<Vector2Int, bool> canTraverseOccupied,
            Func<Vector2Int, bool> canTraverseCell)
        {
            if (!TryResolveTraversalWeight(
                    neighbor,
                    start,
                    canTraverseOccupied,
                    canTraverseCell,
                    out float tileWeight))
            {
                return;
            }

            float tentativeG =
                bestKnownG
                + _neighborhoodStrategy.GetStepCost(current, neighbor)
                * tileWeight;

            if (_gScoreScratch.TryGetValue(neighbor, out float previousG)
                && tentativeG >= previousG - 0.0001f)
            {
                return;
            }

            _cameFromScratch[neighbor] = current;
            _gScoreScratch[neighbor] = tentativeG;

            float f = tentativeG + EstimateHeuristic(neighbor, end);
            PushOpenNode(new OpenNode(neighbor, tentativeG, f));
        }

        private bool TryResolveTraversalWeight(
            Vector2Int position,
            Vector2Int start,
            Func<Vector2Int, bool> canTraverseOccupied,
            Func<Vector2Int, bool> canTraverseCell,
            out float weight)
        {
            weight = 0f;

            if (canTraverseCell != null && !canTraverseCell(position))
                return false;

            if (!_gridService.TryGetTileData(position, out string tileId)
                || string.IsNullOrEmpty(tileId))
            {
                return false;
            }

            if (position != start
                && _objectsMapService.IsOccupied(position)
                && (canTraverseOccupied == null || !canTraverseOccupied(position)))
            {
                return false;
            }

            weight = Mathf.Max(
                0.0001f,
                _tileSettingsService.GetTileWeight(tileId));
            return true;
        }

        private float EstimateHeuristic(Vector2Int from, Vector2Int to)
        {
            // Admissible for any positive custom tile weight supported by the game.
            const float minimumSupportedWeight = 0.0001f;
            return _neighborhoodStrategy.EstimateDistance(from, to)
                   * minimumSupportedWeight;
        }

        private void PushOpenNode(OpenNode node)
        {
            _openHeap.Add(node);
            int index = _openHeap.Count - 1;

            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (!IsOpenNodeBetter(_openHeap[index], _openHeap[parent]))
                    break;

                (_openHeap[parent], _openHeap[index]) =
                    (_openHeap[index], _openHeap[parent]);
                index = parent;
            }
        }

        private OpenNode PopOpenNode()
        {
            OpenNode result = _openHeap[0];
            int lastIndex = _openHeap.Count - 1;
            OpenNode last = _openHeap[lastIndex];
            _openHeap.RemoveAt(lastIndex);

            if (_openHeap.Count == 0)
                return result;

            _openHeap[0] = last;
            int index = 0;

            while (true)
            {
                int left = index * 2 + 1;
                if (left >= _openHeap.Count)
                    break;

                int right = left + 1;
                int best = left;

                if (right < _openHeap.Count
                    && IsOpenNodeBetter(_openHeap[right], _openHeap[left]))
                {
                    best = right;
                }

                if (!IsOpenNodeBetter(_openHeap[best], _openHeap[index]))
                    break;

                (_openHeap[index], _openHeap[best]) =
                    (_openHeap[best], _openHeap[index]);
                index = best;
            }

            return result;
        }

        private static bool IsOpenNodeBetter(OpenNode left, OpenNode right)
        {
            if (left.F < right.F)
                return true;
            if (left.F > right.F)
                return false;

            return left.G > right.G;
        }

        private static List<Vector2Int> ReconstructPath(
            IReadOnlyDictionary<Vector2Int, Vector2Int> cameFrom,
            Vector2Int current)
        {
            var path = new List<Vector2Int> { current };

            while (cameFrom.TryGetValue(current, out Vector2Int previous))
            {
                current = previous;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}
