using Kruty1918.Moyva.Interactions.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Units.API; // Новий API
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;
using System;
using System.Threading;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class TileInteractionService : ITileInteractionService, IInitializable, IDisposable
    {
        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IUnitMovementService _unitMovementService; // Сервіс руху
        private readonly SignalBus _signalBus;
        
        private string _selectedUnitId;
        private CancellationTokenSource _moveCts;

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
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<TileClickedSignal>(OnTileClicked);
            CancelMovement();
        }

        private void OnTileClicked(TileClickedSignal signal)
        {
            HandleTileClick(signal.Position);
        }

        public async void HandleTileClick(Vector2Int position)
        {
            if (!_gridService.TryGetTileData(position, out _)) return;

            // КРОК 1: Вибір юніта (якщо ніхто не вибраний)
            if (string.IsNullOrEmpty(_selectedUnitId))
            {
                if (_objectsMapService.TryGetOccupant(position, out var occupantId))
                {
                    _selectedUnitId = occupantId;
                    Debug.Log($"[Interaction] Вибрано юніта: {_selectedUnitId}");
                    // Тут можна кинути сигнал UnitSelectedSignal для підсвічування в UI
                }
                return;
            }

            // КРОК 2: Наказ на рух (якщо юніт вже вибраний)
            string unitToMove = _selectedUnitId;
            _selectedUnitId = null; // Скидаємо виділення перед початком руху

            Debug.Log($"[Interaction] Наказ для {unitToMove}: рух до {position}");

            // Скасовуємо попередній рух, якщо він ще тривав
            CancelMovement();
            _moveCts = new CancellationTokenSource();

            try
            {
                // Викликаємо асинхронний рух
                await _unitMovementService.MoveUnitAsync(unitToMove, position, _moveCts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[Interaction] Рух юніта {unitToMove} перервано.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Interaction] Помилка під час руху: {e.Message}");
            }
        }

        private void CancelMovement()
        {
            if (_moveCts != null)
            {
                _moveCts.Cancel();
                _moveCts.Dispose();
                _moveCts = null;
            }
        }
    }
}