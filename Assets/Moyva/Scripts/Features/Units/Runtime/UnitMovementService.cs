using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Presentation.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
	internal sealed class UnitMovementService : IUnitMovementService, ITurnBlocker, IInitializable, IDisposable
	{
		private readonly IUnitService _unitService;
		private readonly IPathfinder _pathfinder;
		private readonly IMovementAnimationService _animationService;
		private readonly ITileSettingsService _tileSettings;
		private readonly IGridService _gridService;
		private readonly IObjectsMapService _objectsMapService;
		private readonly SignalBus _signalBus;
		private readonly IUnitClassConfig _unitClassConfig;
		private readonly IUnitGameplayProfileService _unitGameplayProfileService;
		private readonly IGeneratedTerrainLevelQuery _terrainLevelQuery;
		private readonly WorldCreationDefaultsSO _worldDefaults;
		private readonly ITurnService _turns;
		private readonly IGameplayProgressClock _progressClock;
		private readonly IUnitOwnershipQuery _ownership;
		private readonly IUnitPlacementValidator _placementValidator;
		private readonly IUnitWorldPositionResolver _worldPositionResolver;
		private readonly IUnitTraversalPolicy _traversalPolicy;
		private readonly ITraversalCostResolver _traversalCosts;

		private readonly Dictionary<string, CancellationTokenSource> _activeMovements = new();

		public UnitMovementService(
			IUnitService unitService,
			IPathfinder pathfinder,
			IMovementAnimationService animationService,
			ITileSettingsService tileSettings,
			IGridService gridService,
			IObjectsMapService objectsMapService,
			SignalBus signalBus,
			IUnitClassConfig unitClassConfig,
			IUnitGameplayProfileService unitGameplayProfileService,
			[InjectOptional] IGeneratedTerrainLevelQuery terrainLevelQuery = null,
			[InjectOptional] WorldCreationDefaultsSO worldDefaults = null,
			[InjectOptional] ITurnService turns = null,
			[InjectOptional] IGameplayProgressClock progressClock = null,
			[InjectOptional] IUnitOwnershipQuery ownership = null,
			[InjectOptional] IUnitPlacementValidator placementValidator = null,
			[InjectOptional] IUnitWorldPositionResolver worldPositionResolver = null,
			[InjectOptional] IUnitTraversalPolicy traversalPolicy = null,
			[InjectOptional] ITraversalCostResolver traversalCosts = null)
		{
			_unitService = unitService;
			_pathfinder = pathfinder;
			_animationService = animationService;
			_tileSettings = tileSettings;
			_gridService = gridService;
			_objectsMapService = objectsMapService;
			_signalBus = signalBus;
			_unitClassConfig = unitClassConfig;
			_unitGameplayProfileService = unitGameplayProfileService;
			_terrainLevelQuery = terrainLevelQuery;
			_worldDefaults = worldDefaults;
			_turns = turns;
			_progressClock = progressClock;
			_ownership = ownership;
			_placementValidator = placementValidator;
			_worldPositionResolver = worldPositionResolver;
			_traversalPolicy = traversalPolicy;
			_traversalCosts = traversalCosts;
		}

		public void Initialize()
		{
			_signalBus.Subscribe<InterruptMovementSignal>(OnInterruptRequested);
			_signalBus.Subscribe<UnitGarrisonStateChangedSignal>(
				OnUnitGarrisonStateChanged);
		}

		public void Dispose()
		{
			_signalBus.TryUnsubscribe<InterruptMovementSignal>(OnInterruptRequested);
			_signalBus.TryUnsubscribe<UnitGarrisonStateChangedSignal>(
				OnUnitGarrisonStateChanged);

			foreach (var cts in _activeMovements.Values)
			{
				cts.Cancel();
				cts.Dispose();
			}
			_activeMovements.Clear();
		}

		private void OnUnitGarrisonStateChanged(
			UnitGarrisonStateChangedSignal signal)
		{
			if (signal.IsGarrisoned
				|| string.IsNullOrWhiteSpace(signal.UnitId))
			{
				return;
			}

			GameObject unitObject =
				_unitService.GetUnitObject(signal.UnitId);
			if (unitObject == null)
				return;

			unitObject.transform.position =
				ResolveCanonicalMovementWorldPosition(
					signal.UnitPosition,
					0.02f);
			_worldPositionResolver?.AlignBottomToSurface(
				unitObject,
				signal.UnitPosition);
			ApplyUnitPresentationPosition(
				unitObject,
				signal.UnitId);
		}

		private void OnInterruptRequested(InterruptMovementSignal signal)
		{
			if (_activeMovements.TryGetValue(signal.UnitId, out var cts))
				cts.Cancel();
		}

		public async Task MoveUnitAsync(string unitId, Vector2Int targetPosition, CancellationToken externalToken = default)
		{
			if (string.IsNullOrEmpty(unitId))
			{
				LogMoveRejected(unitId, targetPosition, "Unit id is empty.");
				return;
			}

			string ownerId = _ownership?.GetUnitOwnerId(unitId);
			if (_turns != null
			    && _progressClock?.IsRealtime != true
			    && !_turns.CanOwnerAct(ownerId, out string turnReason))
			{
				LogMoveRejected(unitId, targetPosition, turnReason ?? "Owner cannot act.");
				return;
			}

			if (_activeMovements.TryGetValue(unitId, out var oldCts))
			{
				oldCts.Cancel();
				oldCts.Dispose();
			}

			if (!_unitService.TryGetUnitPosition(unitId, out var startPosition))
			{
				LogMoveRejected(unitId, targetPosition, "Unit position is not registered.");
				return;
			}

			if (startPosition == targetPosition)
			{
				return;
			}

			if (_objectsMapService.IsOccupied(targetPosition)
				&& _objectsMapService.TryGetOccupant(targetPosition, out var targetOccupantId)
				&& targetOccupantId != unitId
				&& !CanPathTraverseOccupiedConstructionCell(
					unitId,
					targetPosition))
			{
				LogMoveRejected(
					unitId,
					targetPosition,
					$"Target cell is occupied by '{targetOccupantId}'.");
				return;
			}

			List<Vector2Int> path;
			if (_traversalPolicy != null
				&& _pathfinder is ICostAwarePathfinder costAwarePathfinder)
			{
				path = costAwarePathfinder.FindPathWithCosts(
					startPosition,
					targetPosition,
					(Vector2Int from, Vector2Int to, out float stepCost) =>
						_traversalPolicy.TryEvaluateStep(
							unitId,
							from,
							to,
							float.PositiveInfinity,
							UnitTraversalMode.Pathfinding,
							out stepCost,
							out _));
			}
			else if (_pathfinder is IOccupiedCellPathfinder occupiedPathfinder)
			{
				path = occupiedPathfinder.FindPath(
					startPosition,
					targetPosition,
					position =>
						CanPathTraverseOccupiedConstructionCell(
							unitId,
							position));
			}
			else
			{
				path = _pathfinder.FindPath(
					startPosition,
					targetPosition);
			}

			if (path == null || path.Count <= 1)
			{
				LogMoveRejected(unitId, targetPosition, "Pathfinder returned no path.");
				return;
			}

			if (_traversalPolicy != null)
			{
				float requiredMovement = 0f;
				for (int pathIndex = 1;
					 pathIndex < path.Count;
					 pathIndex++)
				{
					if (!_traversalPolicy.TryEvaluateStep(
							unitId,
							path[pathIndex - 1],
							path[pathIndex],
							float.PositiveInfinity,
							UnitTraversalMode.Pathfinding,
							out float pathStepCost,
							out string pathStepReason))
					{
						LogMoveRejected(
							unitId,
							targetPosition,
							$"Traversal rejected path step {path[pathIndex - 1]} -> {path[pathIndex]}: {pathStepReason ?? "Unknown"}.");
						return;
					}

					requiredMovement += pathStepCost;
				}

				float availableMovement = _unitService.GetStamina(unitId);
				if (requiredMovement > availableMovement + 0.0001f)
				{
					LogMoveRejected(
						unitId,
						targetPosition,
						$"Required movement {requiredMovement:0.###} exceeds stamina {availableMovement:0.###}.");
					return;
				}
			}

			var unitObj = _unitService.GetUnitObject(unitId);
			if (unitObj == null)
			{
				LogMoveRejected(unitId, targetPosition, "Unit GameObject is missing.");
				return;
			}

			var internalCts = new CancellationTokenSource();
			_activeMovements[unitId] = internalCts;

			using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, internalCts.Token);

			int completedSteps = 0;
			try
			{
				var unitTypeId = _unitService.GetUnitTypeId(unitId);
				var config = string.IsNullOrEmpty(unitTypeId) ? null : _unitClassConfig.GetConfig(unitTypeId);

				var settings = _unitGameplayProfileService.ResolveMovementAnimationSettings(unitTypeId);
				settings.CanPerformStep = stepPos => CanMakeStep(unitId, stepPos);
					settings.OnStepCompleted = stepPos =>
					{
						OnStepCompleted(unitId, stepPos);
						completedSteps++;
					};
				float unitSurfacePivotOffsetY = ResolveUnitSurfacePivotOffsetY(unitObj, startPosition);
				EntityPresentationConfig presentation = config?.ResolvePresentation();
				settings.ResolveWorldPosition = stepPos => ResolveMovementWorldPosition(
					stepPos,
					unitSurfacePivotOffsetY,
					presentation,
					unitObj.transform.rotation);

				await _animationService.MoveAlongPathAsync(unitObj.transform, path, settings, linkedCts.Token);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception e)
			{
				Debug.LogError($"[UnitMovement] Помилка руху: {e.Message}");
			}
			finally
			{
				if (_activeMovements.TryGetValue(unitId, out var currentCts) && currentCts == internalCts)
					_activeMovements.Remove(unitId);

					internalCts.Dispose();
				}

			if (completedSteps > 0 && _progressClock?.IsRealtime != true)
				_turns?.TryRecordAction(ownerId, "unit-move");
		}

		public bool IsTurnBlocked(out string reason)
		{
			if (_activeMovements.Count > 0)
			{
				reason = "Дочекайтеся завершення руху юніта.";
				return true;
			}

			reason = null;
			return false;
		}

private bool CanMakeStep(string unitId, Vector2Int stepPos)
{
	float currentMovement = _unitService.GetStamina(unitId);

	if (_traversalPolicy != null
		&& _unitService.TryGetUnitPosition(
			unitId,
			out Vector2Int from))
	{
		bool allowed = _traversalPolicy.TryEvaluateStep(
			unitId,
			from,
			stepPos,
			currentMovement,
			UnitTraversalMode.Execute,
			out float exactCost,
			out string exactReason);

		return allowed;
	}

	bool canStep = TryEvaluateMovementStep(
		unitId,
		stepPos,
		currentMovement,
		openConstructionGateIfNeeded: true,
		out float cost,
		out string reason);

	return canStep;
}

		private bool TryEvaluateMovementStep(
			string unitId,
			Vector2Int stepPos,
			float availableStamina,
			bool openConstructionGateIfNeeded,
			out float cost,
			out string reason)
		{
			cost = 0f;
			reason = null;

			if (_objectsMapService.IsOccupied(stepPos)
				&& _objectsMapService.TryGetOccupant(stepPos, out string occupantId)
				&& occupantId != unitId
				&& !(openConstructionGateIfNeeded
					? CanTraverseOccupiedConstructionCell(unitId, stepPos, occupantId, out reason)
					: CanPathTraverseOccupiedConstructionCell(unitId, stepPos)))
			{
				reason ??= $"Клітинка зайнята '{occupantId}'.";
				return false;
			}

			if (!_gridService.TryGetTileData(stepPos, out string tileTypeId))
			{
				reason = "Тайл не знайдено в grid-сервісі.";
				return false;
			}

			if (string.IsNullOrEmpty(tileTypeId))
			{
				reason = "Тайл має порожній tileTypeId.";
				return false;
			}

			bool terrainBlocked;
			string blockReason;
			if (_placementValidator != null)
			{
				terrainBlocked = !_placementValidator.IsTerrainAllowed(
					stepPos,
					out blockReason);
			}
			else
			{
				terrainBlocked = IsBlockedByUnitPlacementRules(
					stepPos,
					tileTypeId,
					out blockReason);
			}

			if (terrainBlocked)
			{
				reason = blockReason;
				return false;
			}

			if (!TryResolveUnitTileCost(unitId, tileTypeId, out cost, out reason))
				return false;
			if (availableStamina + 0.0001f < cost)
			{
				reason = "Недостатньо витривалості.";
				return false;
			}

			return true;
		}

		private bool CanPathTraverseOccupiedConstructionCell(
			string unitId,
			Vector2Int position)
		{
			return _unitService is IConstructionUnitTraversalQuery traversal
				&& traversal.CanTraverseOccupiedConstructionCell(
					unitId,
					position,
					openGateIfNeeded: false,
					out _);
		}

		private bool CanTraverseOccupiedConstructionCell(
			string unitId,
			Vector2Int position,
			string occupantId,
			out string reason)
		{
			if (_unitService is IConstructionUnitTraversalQuery traversal)
			{
				return traversal.CanTraverseOccupiedConstructionCell(
					unitId,
					position,
					openGateIfNeeded: true,
					out reason);
			}

			reason = "Construction traversal query не підключений.";
			return false;
		}

		private void OnStepCompleted(string unitId, Vector2Int stepPos)
		{
			if (!_gridService.TryGetTileData(stepPos, out var tileTypeId))
				return;
			if (string.IsNullOrEmpty(tileTypeId))
				return;

			float stepCost = TryResolveUnitTileCost(
				unitId,
				tileTypeId,
				out float resolvedCost,
				out _)
				? resolvedCost
				: _tileSettings.GetTileWeight(tileTypeId);
			if (_traversalPolicy != null
				&& _unitService.TryGetUnitPosition(unitId, out Vector2Int previousPosition)
				&& _traversalPolicy.TryEvaluateStep(
					unitId,
					previousPosition,
					stepPos,
					float.PositiveInfinity,
					UnitTraversalMode.Pathfinding,
					out float exactStepCost,
					out _))
			{
				stepCost = exactStepCost;
			}

			bool sharedOccupancy =
				_objectsMapService.TryGetOccupant(
					stepPos,
					out string primaryOccupant)
				&& primaryOccupant != unitId
				&& CanTraverseOccupiedConstructionCell(
					unitId,
					stepPos,
					primaryOccupant,
					out _);

			_signalBus.Fire(new UnitMovedSignal
			{
				UnitId = unitId,
				NewPosition = stepPos,
				Cost = stepCost,
				AllowSharedOccupancy = sharedOccupancy,
			});
		}

		private bool TryResolveUnitTileCost(
			string unitId,
			string tileTypeId,
			out float cost,
			out string reason)
		{
			if (_traversalCosts != null)
			{
				string unitTypeId = _unitService.GetUnitTypeId(unitId);
				UnitClassConfig config = string.IsNullOrWhiteSpace(unitTypeId)
					? null
					: _unitClassConfig.GetConfig(unitTypeId);
				string movementProfileId = config?.MovementProfile?.JsonId;
				if (string.IsNullOrWhiteSpace(movementProfileId))
					movementProfileId = MovementProfileIds.GroundDefault;

				return _traversalCosts.TryResolve(
					movementProfileId,
					tileTypeId,
					out cost,
					out reason);
			}

			cost = _tileSettings.GetTileWeight(tileTypeId);
			if (cost <= 0f)
			{
				reason = "Тайл непрохідний.";
				return false;
			}

			reason = null;
			return true;
		}

		private Vector3 ResolveMovementWorldPosition(
			Vector2Int gridPosition,
			float unitSurfacePivotOffsetY,
			EntityPresentationConfig presentation,
			Quaternion baseRotation)
		{
			Vector3 canonicalPosition =
				ResolveCanonicalMovementWorldPosition(
					gridPosition,
					unitSurfacePivotOffsetY);
			return EntityPresentationApplier.ResolvePosition(
				canonicalPosition,
				baseRotation,
				presentation);
		}

		private Vector3 ResolveCanonicalMovementWorldPosition(Vector2Int gridPosition, float unitSurfacePivotOffsetY)
		{
			if (_worldPositionResolver != null)
				return _worldPositionResolver.ResolveWorldPosition(gridPosition, unitSurfacePivotOffsetY);

			return new Vector3(gridPosition.x, gridPosition.y, 0f);
		}

		private float ResolveUnitSurfacePivotOffsetY(GameObject unitObject, Vector2Int gridPosition)
		{
			if (_worldPositionResolver != null)
			{
				return _worldPositionResolver.ResolveSurfacePivotOffsetY(
					unitObject,
					gridPosition,
					0.02f);
			}

			return 0.05f;
		}

		private void ApplyUnitPresentationPosition(
			GameObject unitObject,
			string unitId)
		{
			if (unitObject == null)
				return;

			string unitTypeId = _unitService.GetUnitTypeId(unitId);
			UnitClassConfig config =
				string.IsNullOrEmpty(unitTypeId)
					? null
					: _unitClassConfig.GetConfig(unitTypeId);
			Vector3 alignedPosition = unitObject.transform.position;
			EntityPresentationApplier.ApplyPosition(
				unitObject,
				config?.ResolvePresentation(),
				alignedPosition,
				unitObject.transform.rotation);
			EntityPresentationApplier.ApplyStyleAndShadows(
				unitObject,
				config?.ResolvePresentation());
		}

		private bool IsBlockedByUnitPlacementRules(Vector2Int position, string tileTypeId, out string reason)
		{
			reason = null;

			if (IsBlockedUnitTile(tileTypeId))
			{
				reason = $"blocked tile '{tileTypeId}'";
				return true;
			}

			if (_terrainLevelQuery != null
				&& _terrainLevelQuery.TryGetTerrainLevel(position, out int terrainLevel)
				&& terrainLevel > 0
				&& IsTerrainLevelBlocked(_worldDefaults?.BlockedUnitHillLevelRanges, terrainLevel))
			{
				reason = $"blocked hill level {terrainLevel}";
				return true;
			}

			return false;
		}

		private bool IsBlockedUnitTile(string tileTypeId)
		{
			if (string.IsNullOrWhiteSpace(tileTypeId))
				return false;

			var blockedTileIds = _worldDefaults?.BlockedUnitTileIds;
			if (blockedTileIds == null || blockedTileIds.Count == 0)
				return false;

			for (int i = 0; i < blockedTileIds.Count; i++)
			{
				string blockedId = blockedTileIds[i];
				if (string.IsNullOrWhiteSpace(blockedId))
					continue;

				if (string.Equals(blockedId.Trim(), tileTypeId, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		private static bool IsTerrainLevelBlocked(IReadOnlyList<TerrainLevelRestrictionRange> ranges, int terrainLevel)
		{
			if (ranges == null || ranges.Count == 0)
				return false;

			for (int i = 0; i < ranges.Count; i++)
			{
				var range = ranges[i];
				if (range == null)
					continue;

				int min = Mathf.Max(1, range.MinLevel);
				int max = Mathf.Max(1, range.MaxLevel);
				if (max < min)
				{
					int swap = min;
					min = max;
					max = swap;
				}

				if (terrainLevel >= min && terrainLevel <= max)
					return true;
			}

			return false;
		}

		private static void LogMoveRejected(
			string unitId,
			Vector2Int targetPosition,
			string reason)
		{
			Debug.LogWarning(
				$"[MOYVA_MOVE][EXECUTE] Move rejected. unit='{unitId}' target={targetPosition}. reason={reason ?? "Unknown"}.");
		}

		private static Vector2Int PopLowestCost(
			List<Vector2Int> open,
			IReadOnlyDictionary<Vector2Int, float> costByPosition)
		{
			int bestIndex = 0;
			float bestCost = costByPosition.TryGetValue(open[0], out float cost)
				? cost
				: float.PositiveInfinity;

			for (int index = 1; index < open.Count; index++)
			{
				float candidateCost = costByPosition.TryGetValue(open[index], out cost)
					? cost
					: float.PositiveInfinity;
				if (candidateCost >= bestCost)
					continue;

				bestIndex = index;
				bestCost = candidateCost;
			}

			Vector2Int result = open[bestIndex];
			open.RemoveAt(bestIndex);
			return result;
		}
	}
}
