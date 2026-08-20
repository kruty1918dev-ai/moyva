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
	internal sealed class UnitMovementService : IUnitMovementService, IUnitMovementQuery, ITurnBlocker, IInitializable, IDisposable
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
		private readonly IUnitOwnershipQuery _ownership;
		private readonly IUnitPlacementValidator _placementValidator;
		private readonly IUnitWorldPositionResolver _worldPositionResolver;

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
			[InjectOptional] IUnitOwnershipQuery ownership = null,
			[InjectOptional] IUnitPlacementValidator placementValidator = null,
			[InjectOptional] IUnitWorldPositionResolver worldPositionResolver = null)
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
			_ownership = ownership;
			_placementValidator = placementValidator;
			_worldPositionResolver = worldPositionResolver;
		}

		[System.Diagnostics.Conditional("MOYVA_VERBOSE_MOVEMENT")]
		private static void LogMovementVerbose(string message)
			=> Debug.Log(message);

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
			long __diagTrace = UnitMovementDiagnostics.TraceForUnit(unitId);
			double __diagMoveStarted = UnitMovementDiagnostics.NowMs();
			UnitMovementDiagnostics.Log(
				__diagTrace,
				"MOVE_CORE_BEGIN",
				$"unit={UnitMovementDiagnostics.Safe(unitId)}; target={targetPosition}; " +
				$"activeMovements={_activeMovements.Count}");

			if (string.IsNullOrEmpty(unitId))
			{
				Debug.LogWarning("[UnitMovement] MoveUnitAsync: unitId пустий або null. Рух скасовано.");
				return;
			}

			string ownerId = _ownership?.GetUnitOwnerId(unitId);
			if (_turns != null && !_turns.CanOwnerAct(ownerId, out string turnReason))
			{
				Debug.LogWarning($"[UnitMovement] Move rejected for '{unitId}': {turnReason}");
				return;
			}

			if (_activeMovements.TryGetValue(unitId, out var oldCts))
			{
				Debug.Log($"[UnitMovement] Скасування попереднього руху для {unitId}.");
				oldCts.Cancel();
				oldCts.Dispose();
			}

			if (!_unitService.TryGetUnitPosition(unitId, out var startPosition))
			{
				Debug.LogWarning($"[UnitMovement] MoveUnitAsync: позиція юніта '{unitId}' не знайдена в UnitService. Юніт не зареєстрований?");
				return;
			}

			if (startPosition == targetPosition)
			{
				Debug.Log($"[UnitMovement] MoveUnitAsync: '{unitId}' вже знаходиться на {targetPosition}. Рух не потрібен.");
				return;
			}

			if (_objectsMapService.IsOccupied(targetPosition)
				&& _objectsMapService.TryGetOccupant(targetPosition, out var targetOccupantId)
				&& targetOccupantId != unitId
				&& !CanPathTraverseOccupiedConstructionCell(
					unitId,
					targetPosition))
			{
				Debug.LogWarning(
					$"[UnitMovement] MoveUnitAsync: ціль {targetPosition} " +
					$"зайнята '{targetOccupantId}' і не є прохідною.");
				return;
			}

			LogMovementVerbose(
				$"[UnitMovement] path {unitId}: {startPosition} -> {targetPosition}");
			double __diagPathStarted = UnitMovementDiagnostics.NowMs();
			List<Vector2Int> path;
			if (_pathfinder is ITraversalPathfinder traversalPathfinder)
			{
				path = traversalPathfinder.FindPathWithTraversal(
					startPosition,
					targetPosition,
					position =>
						position == startPosition
						|| CanPathTraverseCell(
							unitId,
							position));
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
			UnitMovementDiagnostics.Log(
				__diagTrace,
				"ASTAR_RESULT",
				$"unit={unitId}; start={startPosition}; target={targetPosition}; " +
				$"pathCount={path?.Count ?? 0}; " +
				$"pathMs={UnitMovementDiagnostics.Ms(UnitMovementDiagnostics.NowMs() - __diagPathStarted)}; " +
				$"path={UnitMovementDiagnostics.FormatPath(path)}");

			if (path == null || path.Count <= 1)
			{
				Debug.LogWarning($"[UnitMovement] MoveUnitAsync: шлях не знайдено або занадто короткий для '{unitId}' ({startPosition} → {targetPosition}). path={path?.Count ?? 0} точок.");
				return;
			}

			var unitObj = _unitService.GetUnitObject(unitId);
			if (unitObj == null)
			{
				Debug.LogWarning($"[UnitMovement] MoveUnitAsync: GameObject для '{unitId}' не знайдено в UnitService. Юніт не зареєстрований або об'єкт знищено?");
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
				if (config == null)
					Debug.LogWarning($"[UnitMovement] Конфігурація для unitId='{unitId}' (typeId='{unitTypeId}') не знайдена. Використовую PathAnimationSettings.Default.");

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
				UnitMovementDiagnostics.Log(
					__diagTrace,
					"MOVE_ANIMATION_RETURNED",
					$"unit={unitId}; completedSteps={completedSteps}; " +
					$"elapsedMs={UnitMovementDiagnostics.Ms(UnitMovementDiagnostics.NowMs() - __diagMoveStarted)}");
			}
			catch (OperationCanceledException)
			{
				Debug.Log($"[UnitMovement] Рух юніта {unitId} перервано (стаміна або команда).");
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

			if (_unitService.TryGetUnitPosition(unitId, out Vector2Int __diagFinalPosition))
			{
				UnitMovementDiagnostics.Log(
					__diagTrace,
					"MOVE_CORE_END",
					$"unit={unitId}; final={__diagFinalPosition}; target={targetPosition}; " +
					$"completedSteps={completedSteps}; stamina={_unitService.GetStamina(unitId):F3}; " +
					$"totalMs={UnitMovementDiagnostics.Ms(UnitMovementDiagnostics.NowMs() - __diagMoveStarted)}");
			}
			else
			{
				UnitMovementDiagnostics.Warn(
					__diagTrace,
					"MOVE_CORE_END_NO_POSITION",
					$"unit={unitId}; target={targetPosition}; completedSteps={completedSteps}; " +
					$"totalMs={UnitMovementDiagnostics.Ms(UnitMovementDiagnostics.NowMs() - __diagMoveStarted)}");
			}

			if (completedSteps > 0)
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

		public IReadOnlyList<UnitMovementTileSnapshot> GetMovementTiles(string unitId)
		{
			long __diagTrace = UnitMovementDiagnostics.TraceForUnit(unitId);
			double __diagRangeStarted = UnitMovementDiagnostics.NowMs();
			if (string.IsNullOrWhiteSpace(unitId)
				|| !_unitService.TryGetUnitPosition(unitId, out Vector2Int startPosition))
			{
				return Array.Empty<UnitMovementTileSnapshot>();
			}

			float stamina = Mathf.Max(0f, _unitService.GetStamina(unitId));

			var costByPosition = new Dictionary<Vector2Int, float>(128);
			var frontier = new List<MovementFrontierNode>(128);

			costByPosition[startPosition] = 0f;
			PushMovementFrontier(
				frontier,
				new MovementFrontierNode(startPosition, 0f));

			while (frontier.Count > 0)
			{
				MovementFrontierNode current = PopMovementFrontier(frontier);

				if (!costByPosition.TryGetValue(
						current.Position,
						out float bestKnownCost)
					|| current.Cost > bestKnownCost + 0.0001f)
				{
					continue;
				}

				foreach (Vector2Int neighbor in _pathfinder.GetNeighbors(current.Position))
				{
					if (neighbor == startPosition)
						continue;

					if (!TryGetMovementPreviewStepCost(
							unitId,
							neighbor,
							out float stepCost))
					{
						continue;
					}

					float nextCost = current.Cost + stepCost;
					if (nextCost > stamina + 0.0001f)
						continue;

					if (costByPosition.TryGetValue(
							neighbor,
							out float previousCost)
						&& nextCost >= previousCost - 0.0001f)
					{
						continue;
					}

					costByPosition[neighbor] = nextCost;
					PushMovementFrontier(
						frontier,
						new MovementFrontierNode(neighbor, nextCost));
				}
			}

			var ordered =
				new List<UnitMovementTileSnapshot>(costByPosition.Count);

			foreach (KeyValuePair<Vector2Int, float> entry in costByPosition)
			{
				ordered.Add(
					new UnitMovementTileSnapshot(
						entry.Key,
						isReachable: true,
						cost: entry.Value));
			}

			ordered.Sort((left, right) =>
			{
				int cost = left.Cost.CompareTo(right.Cost);
				if (cost != 0)
					return cost;

				int y = left.Position.y.CompareTo(right.Position.y);
				return y != 0
					? y
					: left.Position.x.CompareTo(right.Position.x);
			});

			double __diagRangeMs =
				UnitMovementDiagnostics.NowMs() - __diagRangeStarted;
			string __diagRangeSummary =
				$"unit={unitId}; start={startPosition}; stamina={stamina:F3}; " +
				$"grid={_gridService.GridWidth}x{_gridService.GridHeight}; " +
				$"reachable={ordered.Count}; elapsedMs={UnitMovementDiagnostics.Ms(__diagRangeMs)}";
			if (__diagRangeMs >= 100.0)
				UnitMovementDiagnostics.Warn(__diagTrace, "RANGE_QUERY_SLOW", __diagRangeSummary);
			else
				UnitMovementDiagnostics.Log(__diagTrace, "RANGE_QUERY_DONE", __diagRangeSummary);

			return ordered;
		}

		private readonly struct MovementFrontierNode
		{
			public MovementFrontierNode(Vector2Int position, float cost)
			{
				Position = position;
				Cost = cost;
			}

			public Vector2Int Position { get; }
			public float Cost { get; }
		}

		private static void PushMovementFrontier(
			List<MovementFrontierNode> heap,
			MovementFrontierNode node)
		{
			heap.Add(node);
			int index = heap.Count - 1;

			while (index > 0)
			{
				int parent = (index - 1) / 2;
				if (heap[parent].Cost <= heap[index].Cost)
					break;

				(heap[parent], heap[index]) =
					(heap[index], heap[parent]);
				index = parent;
			}
		}

		private static MovementFrontierNode PopMovementFrontier(
			List<MovementFrontierNode> heap)
		{
			MovementFrontierNode result = heap[0];
			int lastIndex = heap.Count - 1;
			MovementFrontierNode last = heap[lastIndex];
			heap.RemoveAt(lastIndex);

			if (heap.Count == 0)
				return result;

			heap[0] = last;
			int index = 0;

			while (true)
			{
				int left = index * 2 + 1;
				if (left >= heap.Count)
					break;

				int right = left + 1;
				int best =
					right < heap.Count
					&& heap[right].Cost < heap[left].Cost
						? right
						: left;

				if (heap[index].Cost <= heap[best].Cost)
					break;

				(heap[index], heap[best]) =
					(heap[best], heap[index]);
				index = best;
			}

			return result;
		}

		private bool TryGetMovementPreviewStepCost(
			string unitId,
			Vector2Int position,
			out float cost)
		{
			cost = 0f;

			if (_objectsMapService.IsOccupied(position)
				&& _objectsMapService.TryGetOccupant(
					position,
					out string occupantId)
				&& occupantId != unitId
				&& !CanPathTraverseOccupiedConstructionCell(unitId, position))
			{
				return false;
			}

			if (!_gridService.TryGetTileData(position, out string tileTypeId)
				|| string.IsNullOrEmpty(tileTypeId))
			{
				return false;
			}

			if (_placementValidator != null)
			{
				if (!_placementValidator.IsTerrainAllowed(position, out _))
					return false;
			}
			else if (IsBlockedByUnitPlacementRules(position, tileTypeId, out _))
			{
				return false;
			}

			cost = Mathf.Max(
				0.0001f,
				_tileSettings.GetTileWeight(tileTypeId));
			return true;
		}

		private bool CanPathTraverseCell(
			string unitId,
			Vector2Int position)
		{
			return TryGetMovementPreviewStepCost(
				unitId,
				position,
				out _);
		}

		private bool CanMakeStep(string unitId, Vector2Int stepPos)
		{
			float currentStamina = _unitService.GetStamina(unitId);
			bool canStep = TryEvaluateMovementStep(
				unitId,
				stepPos,
				currentStamina,
				openConstructionGateIfNeeded: true,
				out float cost,
				out string reason);
			if (!canStep && !string.IsNullOrWhiteSpace(reason))
				Debug.Log($"[UnitMovement] Перевірка кроку для {unitId} на {stepPos}: BLOCKED ({reason}).");

			LogMovementVerbose(
				$"[UnitMovement] step {unitId}@{stepPos}: " +
				$"stamina={currentStamina} cost={cost} ok={canStep}");
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

			cost = Mathf.Max(0.0001f, _tileSettings.GetTileWeight(tileTypeId));
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

			float stepCost = _tileSettings.GetTileWeight(tileTypeId);

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
