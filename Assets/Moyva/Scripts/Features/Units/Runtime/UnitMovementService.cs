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

        private readonly Dictionary<string, CancellationTokenSource> _activeMovements = new();

        public UnitMovementService(
            IUnitService unitService,
            IPathfinder pathfinder,
            IMovementAnimationService animationService,
            ITileSettingsService tileSettings,
            IGridService gridService,
            IObjectsMapService objectsMapService,
            SignalBus signalBus,
            IUnitClassConfig unitClassConfig) // Додаємо залежність від конфігів юнітів, якщо потрібно
        {
            _unitService = unitService;
            _pathfinder = pathfinder;
            _animationService = animationService;
            _tileSettings = tileSettings;
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _signalBus = signalBus;
            _unitClassConfig = unitClassConfig;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<InterruptMovementSignal>(OnInterruptRequested);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<InterruptMovementSignal>(OnInterruptRequested);

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
                var settings = config?.AnimationSettings ?? PathAnimationSettings.Default;

                // --- НОВА ЛОГІКА ПЕРЕВІРКИ КОЖНОГО КРОКУ ---
                settings.CanPerformStep = (stepPos) =>
                {
                    // Використовуємо твою готову логіку перевірки стаміни
                    return CanMakeStep(unitId, stepPos);
                };
                // -------------------------------------------

                settings.OnStepCompleted = (stepPos) => OnStepCompleted(unitId, stepPos);

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
    }
}