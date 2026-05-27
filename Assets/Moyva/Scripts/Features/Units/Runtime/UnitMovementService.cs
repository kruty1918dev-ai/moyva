using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitMovementService : IUnitMovementService, IInitializable, IDisposable
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
            [InjectOptional] TileRegistrySO tileRegistry = null)
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
        }

        public void Initialize()
        {
            _signalBus.Subscribe<InterruptMovementSignal>(OnInterruptRequested);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<InterruptMovementSignal>(OnInterruptRequested);

            foreach (var cts in _activeMovements.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _activeMovements.Clear();
        }

        private void OnInterruptRequested(InterruptMovementSignal signal)
        {
            if (_activeMovements.TryGetValue(signal.UnitId, out var cts))
            {
                cts.Cancel();
            }
        }

        public async Task MoveUnitAsync(string unitId, Vector2Int targetPosition, CancellationToken externalToken = default)
        {
            if (string.IsNullOrEmpty(unitId))
            {
                Debug.LogWarning("[UnitMovement] MoveUnitAsync: unitId пустий або null. Рух скасовано.");
                return;
            }

            // Скасування старого руху того самого юніта
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
                && targetOccupantId != unitId)
            {
                Debug.LogWarning($"[UnitMovement] MoveUnitAsync: цільовий тайл {targetPosition} вже зайнятий '{targetOccupantId}'. Рух для '{unitId}' скасовано.");
                return;
            }

            Debug.Log($"[UnitMovement] Пошук шляху для '{unitId}': {startPosition} → {targetPosition}");
            var path = _pathfinder.FindPath(startPosition, targetPosition);
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

            try
            {
                var unitTypeId = _unitService.GetUnitTypeId(unitId);
                var config = string.IsNullOrEmpty(unitTypeId) ? null : _unitClassConfig.GetConfig(unitTypeId);
                if (config == null)
                {
                    Debug.LogWarning($"[UnitMovement] Конфігурація для unitId='{unitId}' (typeId='{unitTypeId}') не знайдена. Використовую PathAnimationSettings.Default.");
                }
                var settings = _unitGameplayProfileService.ResolveMovementAnimationSettings(unitTypeId);

                // --- НОВА ЛОГІКА ПЕРЕВІРКИ КОЖНОГО КРОКУ ---
                settings.CanPerformStep = (stepPos) =>
                {
                    // Використовуємо твою готову логіку перевірки стаміни
                    return CanMakeStep(unitId, stepPos);
                };
                // -------------------------------------------

                settings.OnStepCompleted = (stepPos) => OnStepCompleted(unitId, stepPos);
                float unitSurfacePivotOffsetY = ResolveUnitSurfacePivotOffsetY(unitObj, startPosition);
                settings.ResolveWorldPosition = (stepPos) => ResolveMovementWorldPosition(stepPos, unitSurfacePivotOffsetY);

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
                {
                    _activeMovements.Remove(unitId);
                }
                internalCts.Dispose();
            }
        }

        /// <summary>
        /// Універсальна перевірка: чи може юніт наступити на цей тайл.
        /// </summary>
        private bool CanMakeStep(string unitId, Vector2Int stepPos)
        {
            if (_objectsMapService.IsOccupied(stepPos)
                && _objectsMapService.TryGetOccupant(stepPos, out var occupantId)
                && occupantId != unitId)
            {
                Debug.Log($"[UnitMovement] Перевірка кроку для {unitId} на {stepPos}: тайл зайнятий '{occupantId}'.");
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
                Debug.Log($"[UnitMovement] Перевірка кроку для {unitId} на {stepPos}: стаміна={currentStamina}, вартість={cost}, результат={canStep}");
                return canStep;
            }

            Debug.LogWarning($"[UnitMovement] CanMakeStep: тайл {stepPos} не знайдено в грід-сервісі.");
            return false;
        }

        private void OnStepCompleted(string unitId, Vector2Int stepPos)
        {
            if (!_gridService.TryGetTileData(stepPos, out var tileTypeId)) return;
            if (string.IsNullOrEmpty(tileTypeId)) return;

            float stepCost = _tileSettings.GetTileWeight(tileTypeId);

            // Тут ми просто стріляємо сигналом. 
            // UnitService отримає його, відніме стаміну, і якщо вона стане <= 0, 
            // він кине InterruptMovementSignal, який ми обробимо в OnInterruptRequested.
            _signalBus.Fire(new UnitMovedSignal
            {
                UnitId = unitId,
                NewPosition = stepPos,
                Cost = stepCost
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
                if (definition == null || definition.Id != tileId || definition.VisualPrefab == null)
                    continue;

                if (!GridSurfacePlacementUtility.TryResolveTopOffsetY(definition.VisualPrefab, out offsetY))
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