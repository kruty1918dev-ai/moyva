using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Pathfinding.API;
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
		private readonly IGridProjection _gridProjection;
		private readonly TileRegistrySO _tileRegistry;
		private readonly ITurnService _turns;
		private readonly IUnitOwnershipQuery _ownership;

		private readonly Dictionary<string, CancellationTokenSource> _activeMovements = new();
		private readonly Dictionary<string, float> _tileSurfaceOffsetYById = new();

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
			[InjectOptional] IGridProjection gridProjection = null,
			[InjectOptional] TileRegistrySO tileRegistry = null,
			[InjectOptional] ITurnService turns = null,
			[InjectOptional] IUnitOwnershipQuery ownership = null)
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
			_gridProjection = gridProjection;
			_tileRegistry = tileRegistry;
			_turns = turns;
			_ownership = ownership;
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
				ResolveMovementWorldPosition(
					signal.UnitPosition,
					GridSurfacePlacementUtility.DefaultSurfaceClearance);
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
			List<Vector2Int> path =
				_pathfinder is IOccupiedCellPathfinder occupiedPathfinder
					? occupiedPathfinder.FindPath(
						startPosition,
						targetPosition,
						position =>
							CanPathTraverseOccupiedConstructionCell(
								unitId,
								position))
					: _pathfinder.FindPath(
						startPosition,
						targetPosition);
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
				settings.ResolveWorldPosition = stepPos => ResolveMovementWorldPosition(stepPos, unitSurfacePivotOffsetY);

				await _animationService.MoveAlongPathAsync(unitObj.transform, path, settings, linkedCts.Token);
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

		private bool CanMakeStep(string unitId, Vector2Int stepPos)
		{
			if (_objectsMapService.IsOccupied(stepPos)
				&& _objectsMapService.TryGetOccupant(stepPos, out var occupantId)
				&& occupantId != unitId
				&& !CanTraverseOccupiedConstructionCell(
					unitId,
					stepPos,
					occupantId,
					out _))
			{
				return false;
			}

			float currentStamina = _unitService.GetStamina(unitId);

			if (_gridService.TryGetTileData(stepPos, out var tileTypeId))
			{
				if (string.IsNullOrEmpty(tileTypeId))
				{
					Debug.LogWarning($"[UnitMovement] CanMakeStep: тайл {stepPos} має порожній tileTypeId.");
					return false;
				}

				if (IsBlockedByUnitPlacementRules(stepPos, tileTypeId, out string blockReason))
				{
					Debug.Log($"[UnitMovement] Перевірка кроку для {unitId} на {stepPos}: BLOCKED ({blockReason}).");
					return false;
				}

				float cost = _tileSettings.GetTileWeight(tileTypeId);
				bool canStep = currentStamina >= cost;
				LogMovementVerbose(
					$"[UnitMovement] step {unitId}@{stepPos}: " +
					$"stamina={currentStamina} cost={cost} ok={canStep}");
				return canStep;
			}

			Debug.LogWarning($"[UnitMovement] CanMakeStep: тайл {stepPos} не знайдено в грід-сервісі.");
			return false;
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

		private Vector3 ResolveMovementWorldPosition(Vector2Int gridPosition, float unitSurfacePivotOffsetY)
		{
			if (_gridProjection == null)
				return new Vector3(gridPosition.x, gridPosition.y, 0f);

			float elevation = _terrainLevelQuery != null && _terrainLevelQuery.TryGetTerrainLevel(gridPosition, out int level)
				? level
				: 0f;
			Vector3 basePosition = _gridProjection.GridToWorld(gridPosition, elevation, 0.05f);
			if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
				return basePosition;

			basePosition.y = ResolveTerrainSurfaceY(gridPosition, elevation) + unitSurfacePivotOffsetY;
			return basePosition;
		}

		private float ResolveUnitSurfacePivotOffsetY(GameObject unitObject, Vector2Int gridPosition)
		{
			if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection) || unitObject == null)
				return 0.05f;

			float elevation = _terrainLevelQuery != null && _terrainLevelQuery.TryGetTerrainLevel(gridPosition, out int level)
				? level
				: 0f;
			float surfaceY = ResolveTerrainSurfaceY(gridPosition, elevation);
			return Mathf.Max(GridSurfacePlacementUtility.DefaultSurfaceClearance, unitObject.transform.position.y - surfaceY);
		}

		private float ResolveTerrainSurfaceY(Vector2Int gridPosition, float elevation)
		{
			float baseY = _gridProjection.GridToWorld(gridPosition, elevation, 0f).y;
			if (_gridService.TryGetTileData(gridPosition, out string tileId) && TryResolveTileSurfaceOffsetY(tileId, out float offsetY))
				return baseY + offsetY;

			return baseY;
		}

		private bool TryResolveTileSurfaceOffsetY(string tileId, out float offsetY)
		{
			offsetY = 0f;
			if (string.IsNullOrWhiteSpace(tileId) || _tileRegistry?.Definitions == null)
				return false;

			if (_tileSurfaceOffsetYById.TryGetValue(tileId, out offsetY))
				return true;

			for (int i = 0; i < _tileRegistry.Definitions.Length; i++)
			{
				var definition = _tileRegistry.Definitions[i];
				var surfacePrefab = definition?.SurfaceReferencePrefab;
				if (definition == null || definition.Id != tileId || surfacePrefab == null)
					continue;

				if (!GridSurfacePlacementUtility.TryResolveTopOffsetY(surfacePrefab, out offsetY))
					offsetY = 0f;

				_tileSurfaceOffsetYById[tileId] = offsetY;
				return true;
			}

			return false;
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
	}
}
