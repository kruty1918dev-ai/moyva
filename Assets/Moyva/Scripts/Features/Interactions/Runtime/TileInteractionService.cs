using Kruty1918.Moyva.Interactions.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;
using System;
using System.Threading;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class TileInteractionService : ITileInteractionService, IInitializable, IDisposable
    {
        private const bool VerboseLogs = true;

        private enum MovementCancelReason
        {
            None,
            ModeSwitch,
            NewCommand,
            Dispose
        }

        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IUnitMovementService _unitMovementService;
        private readonly SignalBus _signalBus;
        
        private string _selectedUnitId;
        private CancellationTokenSource _moveCts;
        private bool _isActive = true;
        private string _activeMoveUnitId;
        private Vector2Int _activeMoveTarget;
        private (string unitId, Vector2Int target)? _queuedResumeMove;
        private MovementCancelReason _cancelReason = MovementCancelReason.None;

        public TileInteractionService(
            IGridService gridService,
            IObjectsMapService objectsMapService,
            IUnitMovementService unitMovementService, 
            SignalBus signalBus)
        {
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _unitMovementService = unitMovementService;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<TileClickedSignal>(OnTileClicked);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<TileClickedSignal>(OnTileClicked);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            CancelMovement(MovementCancelReason.Dispose);
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _isActive = signal.NewMode == GameModeType.Normal;
            if (!_isActive)
            {
                _selectedUnitId = null;

                if (_moveCts != null && !string.IsNullOrEmpty(_activeMoveUnitId))
                {
                    _queuedResumeMove = (_activeMoveUnitId, _activeMoveTarget);

                    if (VerboseLogs)
                        Debug.Log($"[Interaction] Entered Construction mode. Movement paused for {_activeMoveUnitId}, queued target={_activeMoveTarget}");
                }

                CancelMovement(MovementCancelReason.ModeSwitch);
                return;
            }

            if (_queuedResumeMove.HasValue)
            {
                var move = _queuedResumeMove.Value;
                _queuedResumeMove = null;

                if (VerboseLogs)
                    Debug.Log($"[Interaction] Returned to Normal mode. Resuming movement for {move.unitId} to {move.target} with path recalculation.");

                StartMove(move.unitId, move.target);
            }
        }

        private void OnTileClicked(TileClickedSignal signal)
        {
            HandleTileClick(signal.Position);
        }

        public async void HandleTileClick(Vector2Int position)
        {
            if (!_isActive)
                return;

            if (!_gridService.TryGetTileData(position, out _))
            {
                Debug.LogWarning($"[Interaction] HandleTileClick: тайл на позиції {position} не існує в грід-сервісі.");
                return;
            }

            // КРОК 1: Вибір юніта (якщо ніхто не вибраний)
            if (string.IsNullOrEmpty(_selectedUnitId))
            {
                if (_objectsMapService.TryGetOccupant(position, out var occupantId))
                {
                    _selectedUnitId = occupantId;
                    Debug.Log($"[Interaction] Вибрано юніта: {_selectedUnitId} на позиції {position}");
                }
                else
                {
                    Debug.Log($"[Interaction] Тайл {position} натиснуто, але окупант не знайдений в ObjectsMapService. IsOccupied={_objectsMapService.IsOccupied(position)}");
                }
                return;
            }

            // КРОК 2: Наказ на рух (якщо юніт вже вибраний)
            string unitToMove = _selectedUnitId;
            _selectedUnitId = null; // Скидаємо виділення перед початком руху

            Debug.Log($"[Interaction] Наказ для {unitToMove}: рух до {position}");

            StartMove(unitToMove, position);
        }

        private async void StartMove(string unitId, Vector2Int target)
        {
            if (string.IsNullOrEmpty(unitId))
                return;

            // Скасовуємо попередній рух, якщо він ще тривав
            CancelMovement(MovementCancelReason.NewCommand);
            _moveCts = new CancellationTokenSource();
            _activeMoveUnitId = unitId;
            _activeMoveTarget = target;
            _cancelReason = MovementCancelReason.None;

            try
            {
                // Викликаємо асинхронний рух
                await _unitMovementService.MoveUnitAsync(unitId, target, _moveCts.Token);

                if (VerboseLogs)
                    Debug.Log($"[Interaction] Movement completed for {unitId} to {target}");
            }
            catch (OperationCanceledException)
            {
                if (VerboseLogs)
                    Debug.Log($"[Interaction] Movement canceled for {unitId}. reason={_cancelReason}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Interaction] Помилка під час руху: {e.Message}");
            }
            finally
            {
                if (_moveCts != null)
                {
                    _moveCts.Dispose();
                    _moveCts = null;
                }

                _activeMoveUnitId = null;
                _cancelReason = MovementCancelReason.None;
            }
        }

        private void CancelMovement(MovementCancelReason reason)
        {
            _cancelReason = reason;

            if (_moveCts != null)
            {
                _moveCts.Cancel();
            }
        }
    }
}