using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Pathfinding.API;
using UnityEngine;

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
	public sealed class Pathfinder : IPathfinder
	{
		private readonly IGridService _gridService;
		private readonly ITileSettingsService _tileSettingsService;
		private readonly IObjectsMapService _objectsMapService;
		private readonly INeighborhoodStrategy _neighborhoodStrategy;

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
		{
			if (start == end)
				return new List<Vector2Int> { start };

			if (!IsWalkableTile(start) || !IsWalkableTile(end))
				return new List<Vector2Int>();

			// Occupied target is not a valid destination, but occupied start is allowed.
			if (_objectsMapService.IsOccupied(end))
				return new List<Vector2Int>();

			var openSet = new HashSet<Vector2Int> { start };
			var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
			var gScore = new Dictionary<Vector2Int, float> { [start] = 0f };
			var fScore = new Dictionary<Vector2Int, float>
			{
				[start] = _neighborhoodStrategy.EstimateDistance(start, end)
			};

			while (openSet.Count > 0)
			{
				Vector2Int current = GetLowestFScore(openSet, fScore);
				if (current == end)
					return ReconstructPath(cameFrom, current);

				openSet.Remove(current);

				foreach (Vector2Int neighbor in _neighborhoodStrategy.GetNeighbors(current, _gridService))
				{
					if (!IsTraversable(neighbor, start, end))
						continue;

					float currentG = GetScoreOrInfinity(gScore, current);
					float tentativeG = currentG +
									   _neighborhoodStrategy.GetStepCost(current, neighbor) *
									   ResolveTileWeight(neighbor);

					if (tentativeG >= GetScoreOrInfinity(gScore, neighbor))
						continue;

					cameFrom[neighbor] = current;
					gScore[neighbor] = tentativeG;
					fScore[neighbor] = tentativeG + _neighborhoodStrategy.EstimateDistance(neighbor, end);
					openSet.Add(neighbor);
				}
			}

			return new List<Vector2Int>();
		}

		public IEnumerable<Vector2Int> GetNeighbors(Vector2Int position)
			=> _neighborhoodStrategy.GetNeighbors(position, _gridService);

		private bool IsTraversable(Vector2Int position, Vector2Int start, Vector2Int end)
		{
			if (!IsWalkableTile(position))
				return false;

			if (position == start || position == end)
				return true;

			return !_objectsMapService.IsOccupied(position);
		}

		private bool IsWalkableTile(Vector2Int position)
		{
			if (!_gridService.TryGetTileData(position, out string tileId))
				return false;

			return !string.IsNullOrEmpty(tileId);
		}

		private float ResolveTileWeight(Vector2Int position)
		{
			_gridService.TryGetTileData(position, out string tileId);
			return Mathf.Max(0.0001f, _tileSettingsService.GetTileWeight(tileId));
		}

		private static float GetScoreOrInfinity(Dictionary<Vector2Int, float> scoreMap, Vector2Int key)
			=> scoreMap.TryGetValue(key, out float score) ? score : float.PositiveInfinity;

		private static Vector2Int GetLowestFScore(HashSet<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
		{
			Vector2Int best = default;
			float bestScore = float.PositiveInfinity;
			bool initialized = false;

			foreach (Vector2Int candidate in openSet)
			{
				float score = GetScoreOrInfinity(fScore, candidate);
				if (!initialized || score < bestScore)
				{
					initialized = true;
					best = candidate;
					bestScore = score;
				}
			}

			return best;
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
