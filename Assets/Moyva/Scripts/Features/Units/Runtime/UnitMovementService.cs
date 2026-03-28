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

        private readonly Dictionary<string, CancellationTokenSource> _activeMovements = new();

        public UnitMovementService(
            IUnitService unitService,
            IPathfinder pathfinder,
            IMovementAnimationService animationService,
            ITileSettingsService tileSettings,
            IGridService gridService,
            SignalBus signalBus)
        {
            _unitService = unitService;
            _pathfinder = pathfinder;
            _animationService = animationService;
            _tileSettings = tileSettings;
            _gridService = gridService;
            _signalBus = signalBus;
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

            if (_activeMovements.TryGetValue(unitId, out var oldCts))
            {
                oldCts.Cancel();
                oldCts.Dispose();
            }

            if (!_unitService.TryGetUnitPosition(unitId, out var startPosition)) return;

            var path = _pathfinder.FindPath(startPosition, targetPosition);
            if (path == null || path.Count <= 1) return;

            // --- ПЕРЕВІРКА СТАМІНИ ПЕРЕД ПОЧАТКОМ ---
            // Перевіряємо вартість першого кроку (path[0] - це поточна позиція, path[1] - перший крок)
            if (!CanMakeFirstStep(unitId, path[1]))
            {
                Debug.Log($"[UnitMovement] Юніт {unitId} занадто втомлений, щоб почати рух.");
                return; // Навіть не створюємо CTS і не запускаємо анімацію
            }
            // ----------------------------------------

            var unitObj = _unitService.GetUnitObject(unitId);
            if (unitObj == null) return;

            var internalCts = new CancellationTokenSource();
            _activeMovements[unitId] = internalCts;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, internalCts.Token);

            try
            {
                var settings = PathAnimationSettings.Default;
                settings.OnStepCompleted = (stepPos) => OnStepCompleted(unitId, stepPos);

                await _animationService.MoveAlongPathAsync(unitObj.transform, path, settings, linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[UnitMovement] Рух юніта {unitId} було перервано.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnitMovement] Помилка: {e.Message}");
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
        /// Перевіряє, чи вистачить стаміни на перший крок маршруту.
        /// </summary>
        private bool CanMakeFirstStep(string unitId, Vector2Int firstStepPos)
        {
            float currentStamina = _unitService.GetStamina(unitId);
            
            if (_gridService.TryGetTileData(firstStepPos, out var tileData))
            {
                float cost = _tileSettings.GetTileWeight(tileData.TileTypeId);
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