using Kruty1918.Moyva.Interactions.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Economy.API;
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
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IMapObjectRegistryService _mapObjectRegistryService;
        private readonly IMapObjectEconomyService _mapObjectEconomyService;
        private readonly IUnitMovementService _unitMovementService;
        private readonly SignalBus _signalBus;
        private GameModeType _currentMode = GameModeType.Normal;
        
        private string _selectedUnitId;
        private CancellationTokenSource _moveCts;
        private bool _isActive = true;
        // Tracks what the WorldInfoPanel is currently showing (synced via signal).
        private WorldInfoSelectionKind _inspectedKind;
        private string _inspectedObjectId;
        private string _activeMoveUnitId;
        private Vector2Int _activeMoveTarget;
        private (string unitId, Vector2Int target)? _queuedResumeMove;
        private MovementCancelReason _cancelReason = MovementCancelReason.None;

        public TileInteractionService(
            IGridService gridService,
            IObjectsMapService objectsMapService,
            IBuildingRegistry buildingRegistry,
            IMapObjectRegistryService mapObjectRegistryService,
            IMapObjectEconomyService mapObjectEconomyService,
            IUnitMovementService unitMovementService, 
            SignalBus signalBus)
        {
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _buildingRegistry = buildingRegistry;
            _mapObjectRegistryService = mapObjectRegistryService;
            _mapObjectEconomyService = mapObjectEconomyService;
            _unitMovementService = unitMovementService;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<TileClickedSignal>(OnTileClicked);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<TileClickedSignal>(OnTileClicked);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
            CancelMovement(MovementCancelReason.Dispose);
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _currentMode = signal.NewMode;
            _isActive = signal.NewMode == GameModeType.Normal;
            if (!_isActive)
            {
                _selectedUnitId = null;

                // Закриваємо інфо панель при вході в режим будування
                if (_inspectedKind != WorldInfoSelectionKind.None)
                    _signalBus.Fire(new WorldInfoPanelClosedSignal());

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

        public void HandleTileClick(Vector2Int position)
        {
            if (!_gridService.TryGetTileData(position, out _))
            {
                if (VerboseLogs)
                    Debug.Log($"[Interaction] HandleTileClick ignored: позиція {position} поза межами grid.");
                return;
            }

            // Інфо панель НЕ працює в режимі будування
            if (!_isActive)
                return;

            _objectsMapService.TryGetOccupant(position, out var occupantId);

            bool isBuilding = !string.IsNullOrEmpty(occupantId)
                && _buildingRegistry != null
                && _buildingRegistry.GetById(occupantId) != null;

            bool isMapObject = !string.IsNullOrEmpty(occupantId)
                && _mapObjectRegistryService != null
                && _mapObjectRegistryService.TryGetDefinition(occupantId, out _);

            bool isMapObjectInteractable = isMapObject
                && _mapObjectEconomyService != null
                && _mapObjectEconomyService.IsInteractable(occupantId);

            // --- Клік на будівлю ---
            if (isBuilding)
            {
                // Повторний клік на вже відкриту будівлю — закрити панель (toggle)
                if (_inspectedKind == WorldInfoSelectionKind.Building
                    && string.Equals(_inspectedObjectId, occupantId, StringComparison.Ordinal))
                {
                    _signalBus.Fire(new WorldInfoPanelClosedSignal());
                }
                else
                {
                    _signalBus.Fire(new BuildingInfoPanelRequestedSignal
                    {
                        BuildingId = occupantId,
                        Position = position,
                    });

                    if (VerboseLogs)
                        Debug.Log($"[Interaction] Building info requested for '{occupantId}' at {position}. mode={_currentMode}");
                }

                // Знімаємо вибір юніта при кліку на будівлю
                _selectedUnitId = null;
                return;
            }

            // --- Клік на інтерактивний об'єкт карти ---
            if (isMapObject)
            {
                if (!isMapObjectInteractable)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Interaction] Map object '{occupantId}' at {position} is not interactable. Ignored.");
                    return;
                }

                if (_inspectedKind == WorldInfoSelectionKind.MapObject
                    && string.Equals(_inspectedObjectId, occupantId, StringComparison.Ordinal))
                {
                    _signalBus.Fire(new WorldInfoPanelClosedSignal());
                }
                else
                {
                    _signalBus.Fire(new MapObjectInfoPanelRequestedSignal
                    {
                        MapObjectId = occupantId,
                        Position = position,
                    });

                    if (VerboseLogs)
                        Debug.Log($"[Interaction] MapObject info requested for '{occupantId}' at {position}. mode={_currentMode}");
                }

                _selectedUnitId = null;
                return;
            }

            bool isUnit = !string.IsNullOrEmpty(occupantId);

            // --- Клік на юніта ---
            if (isUnit)
            {
                // Повторний клік на вже вибраного юніта — зняти вибір (toggle)
                if (string.Equals(occupantId, _selectedUnitId, StringComparison.Ordinal))
                {
                    _selectedUnitId = null;
                    _signalBus.Fire(new WorldInfoPanelClosedSignal());
                    Debug.Log($"[Interaction] Вибір юніта скасовано (toggle): {occupantId}");
                    return;
                }

                // Вибрати нового юніта (замість попереднього)
                _selectedUnitId = occupantId;
                _signalBus.Fire(new UnitInfoPanelRequestedSignal
                {
                    UnitId = occupantId,
                    Position = position,
                });
                Debug.Log($"[Interaction] Вибрано юніта: {_selectedUnitId} на позиції {position}");
                return;
            }

            // --- Клік на порожній тайл ---
            if (!string.IsNullOrEmpty(_selectedUnitId))
            {
                string unitToMove = _selectedUnitId;
                _selectedUnitId = null;

                Debug.Log($"[Interaction] Наказ для {unitToMove}: рух до {position}");

                StartMove(unitToMove, position);
            }
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

                // Після успішного завершення руху — повертаємо юніта до поточного стану вибраного.
                // Це дозволяє гравцю відразу дати наступну команду без повторного кліку для вибору.
                if (_cancelReason == MovementCancelReason.None
                    && !string.IsNullOrEmpty(unitId)
                    && _inspectedKind == WorldInfoSelectionKind.Unit
                    && string.Equals(_inspectedObjectId, unitId, StringComparison.Ordinal))
                {
                    _selectedUnitId = unitId;
                }

                _cancelReason = MovementCancelReason.None;
            }
        }

        private void OnWorldInfoSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            _inspectedKind     = signal.Kind;
            _inspectedObjectId = signal.ObjectId;

            // Якщо панель закрита ззовні (наприклад кнопкою X) — скидаємо вибір юніта
            if (signal.Kind == WorldInfoSelectionKind.None)
                _selectedUnitId = null;
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