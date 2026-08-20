using Kruty1918.Moyva.Interactions.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Notifications.API;
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
        private readonly IUnitOwnershipQuery _unitOwnershipQuery;
        private readonly IUnitCombatService _unitCombatService;
        private readonly IConstructionService _constructionService;
        private readonly IConstructionLifecycle _constructionLifecycle;
        private readonly IGameplayNotificationService _notifications;
        private readonly SignalBus _signalBus;
        private readonly ITurnService _turns;
        private GameModeType _currentMode = GameModeType.Normal;
        
        private string _selectedUnitId;
        private CancellationTokenSource _moveCts;
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
            [InjectOptional] IMapObjectRegistryService mapObjectRegistryService,
            [InjectOptional] IMapObjectEconomyService mapObjectEconomyService,
            [InjectOptional] IUnitMovementService unitMovementService,
            [InjectOptional] IUnitOwnershipQuery unitOwnershipQuery,
            [InjectOptional] IUnitCombatService unitCombatService,
            [InjectOptional] IConstructionService constructionService,
            [InjectOptional] ITurnService turns,
            [InjectOptional] IConstructionLifecycle constructionLifecycle,
            [InjectOptional] IGameplayNotificationService notifications,
            SignalBus signalBus)
        {
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _buildingRegistry = buildingRegistry;
            _mapObjectRegistryService = mapObjectRegistryService;
            _mapObjectEconomyService = mapObjectEconomyService;
            _unitMovementService = unitMovementService;
            _unitOwnershipQuery = unitOwnershipQuery;
            _unitCombatService = unitCombatService;
            _constructionService = constructionService;
            _turns = turns;
            _constructionLifecycle = constructionLifecycle;
            _notifications = notifications;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<TileClickedSignal>(OnTileClicked);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<TileClickedSignal>(OnTileClicked);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            CancelMovement(MovementCancelReason.Dispose);
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _currentMode = signal.NewMode;
            if (!CanSelectUnit())
                ClearSelectedUnit();

            if (signal.NewMode != GameModeType.Normal)
            {
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
            if (signal.Button == TilePointerButton.Secondary)
                HandleSecondaryTileClick(signal.Position);
            else
                HandleTileClick(signal.Position);
        }

        public void HandleTileClick(Vector2Int position)
        {
            bool canInspectWorld = CanInspectWorld();
            bool canSelectUnit = CanSelectUnit();
            if (!canInspectWorld && !canSelectUnit)
                return;

            if (!_gridService.TryGetTileData(position, out _))
            {
                if (VerboseLogs)
                    Debug.Log($"[Interaction] HandleTileClick ignored: позиція {position} поза межами grid.");
                return;
            }

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
                if (!canInspectWorld)
                    return;

                if (!IsBuildingOperationalForFunctionalUi(position))
                {
                    ClearSelectedUnit();
                    _notifications?.Show(
                        "Будівля ще будується",
                        GameplayNotificationKind.Warning,
                        dedupKey: "building-under-construction");
                    if (VerboseLogs)
                        Debug.Log($"[Interaction] Building '{occupantId}' at {position} is still under construction. Functional UI request ignored.");
                    return;
                }

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
                ClearSelectedUnit();
                return;
            }

            // --- Клік на інтерактивний об'єкт карти ---
            if (isMapObject)
            {
                if (!canInspectWorld)
                    return;

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

                ClearSelectedUnit();
                return;
            }

            bool isUnit = !string.IsNullOrEmpty(occupantId);

            // --- Клік на юніта ---
            if (isUnit)
            {
                if (!canSelectUnit)
                    return;

                if (TryHandleAttackClick(occupantId))
                    return;

                // Повторний клік на вже вибраного юніта — зняти вибір (toggle)
                if (string.Equals(occupantId, _selectedUnitId, StringComparison.Ordinal))
                {
                    ClearSelectedUnit(position);
                    _signalBus.Fire(new WorldInfoPanelClosedSignal());
                    Debug.Log($"[Interaction] Вибір юніта скасовано (toggle): {occupantId}");
                    return;
                }

                // Вибрати нового юніта (замість попереднього)
                if (CanCommandUnit(occupantId))
                    SelectLocalUnit(occupantId, position);
                else
                    ClearSelectedUnit();

                _signalBus.Fire(new UnitInfoPanelRequestedSignal
                {
                    UnitId = occupantId,
                    Position = position,
                });
                Debug.Log(_selectedUnitId != null
                    ? $"[Interaction] Вибрано власного юніта: {_selectedUnitId} на позиції {position}"
                    : $"[Interaction] Іноземний юніт '{occupantId}' відкритий лише для огляду.");
                return;
            }
        }

        private bool TryHandleAttackClick(string targetUnitId)
        {
            if (_unitCombatService == null || string.IsNullOrWhiteSpace(_selectedUnitId)
                || string.IsNullOrWhiteSpace(targetUnitId)
                || string.Equals(_selectedUnitId, targetUnitId, StringComparison.Ordinal)
                || !CanCommandUnit(_selectedUnitId) || _unitOwnershipQuery == null)
                return false;

            string attackerOwner = _unitOwnershipQuery.GetUnitOwnerId(_selectedUnitId)?.Trim();
            string targetOwner = _unitOwnershipQuery.GetUnitOwnerId(targetUnitId)?.Trim();
            if (string.IsNullOrWhiteSpace(attackerOwner) || string.IsNullOrWhiteSpace(targetOwner)
                || string.Equals(attackerOwner, targetOwner, StringComparison.Ordinal))
                return false;

            UnitAttackRejectReason rejectReason;
            if (!_unitCombatService.CanAttack(_selectedUnitId, targetUnitId, out rejectReason))
            {
                if (rejectReason == UnitAttackRejectReason.TargetOutOfRange)
                    _notifications?.Show("Ціль поза дальністю атаки", GameplayNotificationKind.Warning, dedupKey: "unit-attack-out-of-range");
                else if (rejectReason == UnitAttackRejectReason.AttackUnavailable)
                    _notifications?.Show("Цей юніт зараз не може атакувати", GameplayNotificationKind.Warning, dedupKey: "unit-attack-unavailable");
                return true;
            }

            UnitAttackResult result;
            _unitCombatService.TryAttack(_selectedUnitId, targetUnitId, out result);
            if (VerboseLogs && result.Succeeded)
                Debug.Log($"[Combat] {_selectedUnitId} -> {targetUnitId}: damage={result.DamageApplied}, hp={result.TargetHpBefore}->{result.TargetHpAfter}, died={result.TargetDied}");
            return true;
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (!string.Equals(signal.UnitId, _selectedUnitId, StringComparison.Ordinal)) return;
            ClearSelectedUnit();
            _signalBus.Fire(new WorldInfoPanelClosedSignal());
        }

        private bool IsBuildingOperationalForFunctionalUi(Vector2Int position)
            => _constructionLifecycle == null
                || _constructionLifecycle.IsOperational(position);

        private void HandleSecondaryTileClick(Vector2Int position)
        {
            if (!CanCommandUnit(_selectedUnitId)
                || !_gridService.TryGetTileData(position, out _))
            {
                return;
            }

            _objectsMapService.TryGetOccupant(position, out string occupantId);
            if (!string.IsNullOrWhiteSpace(occupantId))
                return;

            StartMove(_selectedUnitId, position);
        }

        private void StartMove(string unitId, Vector2Int target)
        {
            if (string.IsNullOrEmpty(unitId) || !CanCommandUnit(unitId))
                return;

            // Скасовуємо попередній рух (InterruptMovementSignal надіслано в CancelMovement).
            CancelMovement(MovementCancelReason.NewCommand);
            _moveCts = new CancellationTokenSource(); // sentinel: рух активний (для queue-логіки)
            _activeMoveUnitId = unitId;
            _activeMoveTarget = target;
            _cancelReason = MovementCancelReason.None;

            // Маршрутизуємо через MultiplayerAuthorityService:
            // офлайн/хост → виконає MoveUnitAsync локально;
            // клієнт → надішле Request до хоста.
            _signalBus.Fire(new MoveUnitRequestSignal
            {
                UnitId = unitId,
                TargetPosition = target,
                RequesterOwnerId = GetLocalOwnerId(),
            });

            if (VerboseLogs)
                Debug.Log($"[Interaction] Move requested for {unitId} to {target}");
        }

        private bool CanCommandUnit(string unitId)
        {
            if (_currentMode != GameModeType.Normal)
                return false;

            if (_unitOwnershipQuery == null || string.IsNullOrWhiteSpace(unitId))
                return false;

            return string.Equals(
                _unitOwnershipQuery.GetUnitOwnerId(unitId)?.Trim(),
                GetLocalOwnerId(),
                StringComparison.Ordinal);
        }

        private string GetLocalOwnerId()
            => _turns?.LocalOwnerId?.Trim()
               ?? _constructionService?.GetActiveOwner()?.Trim()
               ?? string.Empty;

        private bool CanInspectWorld()
        {
            if (_currentMode == GameModeType.Normal)
                return true;

            if (_currentMode != GameModeType.Construction)
                return false;

            return _constructionService == null
                   || (_constructionService.State == BuildingPlacementState.Idle
                       && !_constructionService.IsDemolishMode);
        }

        private bool CanSelectUnit()
            => _currentMode == GameModeType.Normal;

        private void SelectLocalUnit(string unitId, Vector2Int position)
        {
            string normalizedUnitId = string.IsNullOrWhiteSpace(unitId)
                ? null
                : unitId.Trim();
            if (string.IsNullOrWhiteSpace(normalizedUnitId))
            {
                ClearSelectedUnit(position);
                return;
            }

            if (string.Equals(_selectedUnitId, normalizedUnitId, StringComparison.Ordinal))
                return;

            ClearSelectedUnit();
            _selectedUnitId = normalizedUnitId;
            _signalBus.Fire(new LocalUnitSelectionChangedSignal
            {
                UnitId = normalizedUnitId,
                Position = position,
                IsSelected = true,
            });
        }

        private void ClearSelectedUnit(Vector2Int? knownPosition = null)
        {
            if (string.IsNullOrWhiteSpace(_selectedUnitId))
            {
                _selectedUnitId = null;
                return;
            }

            string previousUnitId = _selectedUnitId;
            Vector2Int position = knownPosition
                                  ?? (_objectsMapService.TryGetPosition(previousUnitId, out Vector2Int resolved)
                                      ? resolved
                                      : default);
            _selectedUnitId = null;
            _signalBus.Fire(new LocalUnitSelectionChangedSignal
            {
                UnitId = previousUnitId,
                Position = position,
                IsSelected = false,
            });
        }

        private void OnWorldInfoSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            _inspectedKind     = signal.Kind;
            _inspectedObjectId = signal.ObjectId;

            // Якщо панель закрита ззовні (наприклад кнопкою X) — скидаємо вибір юніта
            if (signal.Kind == WorldInfoSelectionKind.None
                || signal.Kind != WorldInfoSelectionKind.Unit
                || !string.Equals(
                    signal.ObjectId,
                    _selectedUnitId,
                    StringComparison.Ordinal))
            {
                ClearSelectedUnit(signal.Position);
            }
        }

        private void CancelMovement(MovementCancelReason reason)
        {
            _cancelReason = reason;

            if (_moveCts != null)
            {
                _moveCts.Cancel();
                _moveCts.Dispose();
                _moveCts = null;
            }

            if (!string.IsNullOrEmpty(_activeMoveUnitId))
                _signalBus.Fire(new InterruptMovementSignal { UnitId = _activeMoveUnitId });
        }
    }
}
