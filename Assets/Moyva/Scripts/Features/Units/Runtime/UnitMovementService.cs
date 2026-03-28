using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Animations.API;
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
        private readonly SignalBus _signalBus;
        private readonly IUnitClassConfig _unitClassConfig;

        private readonly Dictionary<string, CancellationTokenSource> _activeMovements = new();

        public UnitMovementService(
            IUnitService unitService,
            IPathfinder pathfinder,
            IMovementAnimationService animationService,
            ITileSettingsService tileSettings,
            IGridService gridService,
            SignalBus signalBus,
            IUnitClassConfig unitClassConfig) // Додаємо залежність від конфігів юнітів, якщо потрібно
        {
            _unitService = unitService;
            _pathfinder = pathfinder;
            _animationService = animationService;
            _tileSettings = tileSettings;
            _gridService = gridService;
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
            if (string.IsNullOrEmpty(unitId)) return;

            // Скасування старого руху того самого юніта
            if (_activeMovements.TryGetValue(unitId, out var oldCts))
            {
                oldCts.Cancel();
                oldCts.Dispose();
            }

            if (!_unitService.TryGetUnitPosition(unitId, out var startPosition)) return;

            var path = _pathfinder.FindPath(startPosition, targetPosition);
            if (path == null || path.Count <= 1) return;

            var unitObj = _unitService.GetUnitObject(unitId);
            if (unitObj == null) return;

            var internalCts = new CancellationTokenSource();
            _activeMovements[unitId] = internalCts;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, internalCts.Token);

            try
            {
                var settings = _unitClassConfig.GetConfig(unitId)?.AnimationSettings ?? PathAnimationSettings.Default;

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
            float currentStamina = _unitService.GetStamina(unitId);

            if (_gridService.TryGetTileData(stepPos, out var tileData))
            {
                // Розраховуємо вартість (тут можна додати коефіцієнт діагоналі, якщо треба)
                float cost = _tileSettings.GetTileWeight(tileData.TileTypeId);
                Debug.Log($"[UnitMovement] Перевірка кроку для {unitId} на {stepPos}: поточна стаміна = {currentStamina}, вартість кроку = {cost}");    
                return currentStamina >= cost;
            }

            return false;
        }

        private void OnStepCompleted(string unitId, Vector2Int stepPos)
        {
            if (!_gridService.TryGetTileData(stepPos, out var tileData)) return;

            float stepCost = _tileSettings.GetTileWeight(tileData.TileTypeId);

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