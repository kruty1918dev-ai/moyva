using Kruty1918.Moyva.Interactions.API;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Localization;
using Kruty1918.Notifications.API;
using UnityEngine;
using Zenject;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class TileInteractionService : ITileInteractionService, IInitializable, IDisposable
    {
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
        private readonly IUnitMovementQuery _unitMovementQuery;
        private readonly IUnitOwnershipQuery _unitOwnershipQuery;
        private readonly IUnitCombatService _unitCombatService;
        private readonly IUnitGroupService _unitGroupService;
        private readonly IUnitGroupRemoteCommandRequester _remoteGroups;
        private readonly ICombatCommandService _combatCommandService;
        private readonly ICombatRemoteCommandRequester _remoteCombat;
        private readonly ILocalGameplayRoleResolver _roleResolver;
        private readonly IFogStateReader _fogState;
        private readonly IConstructionSessionCommands _constructionService;
        private readonly IConstructionLifecycle _constructionLifecycle;
        private readonly IGameplayNotificationService _notifications;
        private readonly ILocalizationService _loca;
        private readonly SignalBus _signalBus;
        private readonly ITurnService _turns;
        private GameModeType _currentMode = GameModeType.Normal;

        private string _selectedUnitId;
        private bool _groupMergeArmed;
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
            [InjectOptional] IUnitMovementQuery unitMovementQuery,
            [InjectOptional] IUnitOwnershipQuery unitOwnershipQuery,
            [InjectOptional] IUnitCombatService unitCombatService,
            [InjectOptional] IUnitGroupService unitGroupService,
            [InjectOptional] IUnitGroupRemoteCommandRequester remoteGroups,
            [InjectOptional] ICombatCommandService combatCommandService,
            [InjectOptional] ICombatRemoteCommandRequester remoteCombat,
            [InjectOptional] ILocalGameplayRoleResolver roleResolver,
            [InjectOptional] IFogStateReader fogState,
            [InjectOptional] IConstructionSessionCommands constructionService,
            [InjectOptional] ITurnService turns,
            [InjectOptional] IConstructionLifecycle constructionLifecycle,
            [InjectOptional] IGameplayNotificationService notifications,
            SignalBus signalBus,
            [InjectOptional] ILocalizationService localization = null)
        {
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _buildingRegistry = buildingRegistry;
            _mapObjectRegistryService = mapObjectRegistryService;
            _mapObjectEconomyService = mapObjectEconomyService;
            _unitMovementService = unitMovementService;
            _unitMovementQuery = unitMovementQuery;
            _unitOwnershipQuery = unitOwnershipQuery;
            _unitCombatService = unitCombatService;
            _unitGroupService = unitGroupService;
            _remoteGroups = remoteGroups;
            _combatCommandService = combatCommandService;
            _remoteCombat = remoteCombat;
            _roleResolver = roleResolver;
            _fogState = fogState;
            _constructionService = constructionService;
            _turns = turns;
            _constructionLifecycle = constructionLifecycle;
            _notifications = notifications;
            _signalBus = signalBus;
            _loca = localization;
        }

        private string T(string key)
            => _loca?.T(key) ?? key;

        public void Initialize()
        {
            _signalBus.Subscribe<TileClickedSignal>(OnTileClicked);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<UnitMoveRejectedSignal>(OnUnitMoveRejected);
            _signalBus.Subscribe<UnitGroupCommandRejectedSignal>(OnUnitGroupCommandRejected);
            _signalBus.Subscribe<UnitRecruitmentCommandRejectedSignal>(OnUnitRecruitmentCommandRejected);
            _signalBus.Subscribe<ConstructionPlacementRejectedSignal>(OnConstructionPlacementRejected);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<TileClickedSignal>(OnTileClicked);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<UnitMoveRejectedSignal>(OnUnitMoveRejected);
            _signalBus.TryUnsubscribe<UnitGroupCommandRejectedSignal>(OnUnitGroupCommandRejected);
            _signalBus.TryUnsubscribe<UnitRecruitmentCommandRejectedSignal>(OnUnitRecruitmentCommandRejected);
            _signalBus.TryUnsubscribe<ConstructionPlacementRejectedSignal>(OnConstructionPlacementRejected);
            CancelMovement(MovementCancelReason.Dispose);
        }

        private void OnUnitMoveRejected(UnitMoveRejectedSignal signal)
        {
            // Only surface rejections for units the local player could command.
            if (!CanCommandUnit(signal.UnitId))
                return;

            _notifications?.Show(
                string.IsNullOrWhiteSpace(signal.Reason)
                    ? T("Movement is not possible.")
                    : T(signal.Reason),
                GameplayNotificationKind.Warning,
                dedupKey: "unit-move-rejected");
        }

        private void OnUnitGroupCommandRejected(UnitGroupCommandRejectedSignal signal)
        {
            _notifications?.Show(
                string.IsNullOrWhiteSpace(signal.Reason)
                    ? T("Unit group command rejected.")
                    : T(signal.Reason),
                GameplayNotificationKind.Warning,
                dedupKey: "unit-group-rejected");
        }

        private void OnUnitRecruitmentCommandRejected(UnitRecruitmentCommandRejectedSignal signal)
        {
            _notifications?.Show(
                string.IsNullOrWhiteSpace(signal.Reason)
                    ? T("Unit recruitment command rejected.")
                    : T(signal.Reason),
                GameplayNotificationKind.Warning,
                dedupKey: "unit-recruitment-rejected");
        }

        private void OnConstructionPlacementRejected(ConstructionPlacementRejectedSignal signal)
        {
            _notifications?.Show(
                string.IsNullOrWhiteSpace(signal.Reason)
                    ? T("The placement was rejected by host.")
                    : T(signal.Reason),
                GameplayNotificationKind.Warning,
                dedupKey: $"construction-place-rejected:{signal.Position}");
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _currentMode = signal.NewMode;
            if (!CanSelectUnit())
                ClearSelectedUnit();
            else
                _groupMergeArmed = false;

            if (signal.NewMode != GameModeType.Normal)
            {
                if (_moveCts != null && !string.IsNullOrEmpty(_activeMoveUnitId))
                {
                    _queuedResumeMove = (_activeMoveUnitId, _activeMoveTarget);
                }

                CancelMovement(MovementCancelReason.ModeSwitch);
                return;
            }

            if (_queuedResumeMove.HasValue)
            {
                var move = _queuedResumeMove.Value;
                _queuedResumeMove = null;

                StartMove(move.unitId, move.target);
            }
        }

        private void OnTileClicked(TileClickedSignal signal)
        {
            if (signal.Button == TilePointerButton.Secondary)
            {
                HandleSecondaryTileClick(signal.Position);
            }
            else
            {
                HandleTileClick(signal.Position);
            }
        }
        public void HandleTileClick(Vector2Int position)
        {
            bool canInspectWorld = CanInspectWorld();
            bool canSelectUnit = CanSelectUnit();
            if (!canInspectWorld && !canSelectUnit)
                return;

            if (!_gridService.TryGetTileData(position, out _))
            {
                return;
            }

            // Fog gate: occupants on non-visible tiles are not inspectable or
            // selectable — the click falls through to movement instead of
            // leaking building/unit presence to the local player.
            bool tileVisible = _fogState == null || _fogState.IsVisible(position);
            _objectsMapService.TryGetOccupant(position, out var occupantId);
            if (!tileVisible)
                occupantId = null;

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

                if (TryHandleGroupMergeClick(occupantId))
                    return;

                if (TryHandleAttackClick(occupantId))
                    return;

                // Повторний клік на вже вибраного юніта — зняти вибір (toggle)
                if (string.Equals(occupantId, _selectedUnitId, StringComparison.Ordinal))
                {
                    ClearSelectedUnit(position);
                    _signalBus.Fire(new WorldInfoPanelClosedSignal());
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
                return;
            }

            if (!string.IsNullOrWhiteSpace(_selectedUnitId)
                && CanCommandUnit(_selectedUnitId))
            {
                TryIssueMoveCommand(position);
                return;
            }
        }

        public bool IsGroupMergeArmed => _groupMergeArmed;

        public bool ToggleGroupMergeArm()
        {
            if (!CanCommandUnit(_selectedUnitId))
            {
                _groupMergeArmed = false;
                _notifications?.Show(
                    "Спочатку виберіть власного юніта",
                    GameplayNotificationKind.Warning,
                    dedupKey: "unit-group-merge-no-unit");
                return false;
            }

            _groupMergeArmed = !_groupMergeArmed;
            if (_groupMergeArmed)
            {
                _notifications?.Show(
                    "Клікніть на іншого свого юніта, щоб об'єднати їх у групу",
                    GameplayNotificationKind.Info,
                    dedupKey: "unit-group-merge-armed");
            }
            return true;
        }

        public bool TryDisbandSelectedGroup()
        {
            if (!CanCommandUnit(_selectedUnitId) || _unitGroupService == null)
                return false;

            string groupId = _unitGroupService.GetGroupIdOfUnit(_selectedUnitId);
            if (string.IsNullOrEmpty(groupId))
            {
                _notifications?.Show(
                    "Вибраний юніт не належить до групи",
                    GameplayNotificationKind.Warning,
                    dedupKey: "unit-group-disband-none");
                return false;
            }

            string ownerId = GetLocalOwnerId();
            string reason = null;
            bool requested;
            if (IsLocalClient())
            {
                requested = _remoteGroups != null
                    && _remoteGroups.TryRequestDisbandGroup(ownerId, groupId, out reason);
            }
            else
            {
                requested = _unitGroupService.TryDisbandGroup(ownerId, groupId, out reason);
            }
            if (!requested)
                FailGroupCommand(reason);
            return requested;
        }

        private bool TryHandleGroupMergeClick(string targetUnitId)
        {
            if (!_groupMergeArmed || _unitGroupService == null
                || string.IsNullOrWhiteSpace(_selectedUnitId)
                || string.IsNullOrWhiteSpace(targetUnitId)
                || string.Equals(_selectedUnitId, targetUnitId, StringComparison.Ordinal)
                || !CanCommandUnit(_selectedUnitId))
            {
                return false;
            }

            // Merge only targets own units; clicking an enemy stays an attack.
            if (!CanCommandUnit(targetUnitId))
                return false;

            _groupMergeArmed = false;
            string ownerId = GetLocalOwnerId();
            string targetGroupId = _unitGroupService?.GetGroupIdOfUnit(targetUnitId);
            string selectedGroupId = _unitGroupService?.GetGroupIdOfUnit(_selectedUnitId);
            bool requested;

            if (!string.IsNullOrEmpty(selectedGroupId)
                && string.Equals(selectedGroupId, targetGroupId, StringComparison.Ordinal))
            {
                _notifications?.Show(
                    "Юніти вже в одній групі",
                    GameplayNotificationKind.Warning,
                    dedupKey: "unit-group-merge-same");
                return true;
            }

            string reason = null;
            if (!string.IsNullOrEmpty(targetGroupId))
            {
                requested = IsLocalClient()
                    ? _remoteGroups != null
                      && _remoteGroups.TryRequestAddUnit(
                          ownerId, targetGroupId, _selectedUnitId, out reason)
                    : _unitGroupService.TryAddUnit(
                          ownerId, targetGroupId, _selectedUnitId, out reason);
            }
            else if (!string.IsNullOrEmpty(selectedGroupId))
            {
                requested = IsLocalClient()
                    ? _remoteGroups != null
                      && _remoteGroups.TryRequestAddUnit(
                          ownerId, selectedGroupId, targetUnitId, out reason)
                    : _unitGroupService.TryAddUnit(
                          ownerId, selectedGroupId, targetUnitId, out reason);
            }
            else
            {
                var members = new[] { _selectedUnitId, targetUnitId };
                requested = IsLocalClient()
                    ? _remoteGroups != null
                      && _remoteGroups.TryRequestCreateGroup(
                          ownerId, members, out reason)
                    : _unitGroupService.TryCreateGroup(
                          ownerId, members, out _, out reason);
            }
            if (!requested)
                FailGroupCommand(reason);

            if (requested)
            {
                _notifications?.Show(
                    "Групу оновлено",
                    GameplayNotificationKind.Info,
                    dedupKey: "unit-group-merged");
            }
            return true;
        }

        private bool IsLocalClient()
            => _roleResolver?.Resolve().Role == LocalGameplayRole.Client;

        private bool FailGroupCommand(string reason)
        {
            _notifications?.Show(
                string.IsNullOrWhiteSpace(reason)
                    ? "Команду групи відхилено"
                    : reason,
                GameplayNotificationKind.Warning,
                dedupKey: "unit-group-rejected");
            return false;
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
                    _notifications?.Show(T("Target is out of attack range."), GameplayNotificationKind.Warning, dedupKey: "unit-attack-out-of-range");
                else if (rejectReason == UnitAttackRejectReason.TargetNotVisible)
                    _notifications?.Show(T("Target is outside the visible area."), GameplayNotificationKind.Warning, dedupKey: "unit-attack-not-visible");
                else if (rejectReason == UnitAttackRejectReason.AttackUnavailable)
                    _notifications?.Show(T("This unit cannot attack right now."), GameplayNotificationKind.Warning, dedupKey: "unit-attack-unavailable");
                return true;
            }

            if (_roleResolver?.Resolve().Role == LocalGameplayRole.Client)
            {
                string remoteReason = null;
                if (_remoteCombat != null
                    && _remoteCombat.TryRequestAttack(
                        attackerOwner,
                        _selectedUnitId,
                        targetUnitId,
                        out remoteReason))
                {
                    return true;
                }

                _notifications?.Show(
                    string.IsNullOrWhiteSpace(remoteReason)
                        ? "Атака недоступна"
                        : remoteReason,
                    GameplayNotificationKind.Warning,
                    dedupKey: "unit-attack-remote");
                return true;
            }

            if (_combatCommandService != null)
            {
                _ = _combatCommandService.ExecuteAsync(
                    attackerOwner,
                    _selectedUnitId,
                    targetUnitId);
            }
            else
            {
                _unitCombatService.TryAttack(_selectedUnitId, targetUnitId, out _);
            }
            return true;
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (!string.Equals(signal.UnitId, _selectedUnitId, StringComparison.Ordinal)) return;
            ClearSelectedUnit();
            _signalBus.Fire(new WorldInfoPanelClosedSignal());
        }

        private void HandleSecondaryTileClick(Vector2Int position)
        {
            TryIssueMoveCommand(position);
        }
        private bool TryIssueMoveCommand(Vector2Int position)
        {
            if (!CanCommandUnit(_selectedUnitId)
                || !_gridService.TryGetTileData(position, out _))
            {
                return false;
            }

            IUnitMovementQuery movementQuery =
                _unitMovementQuery
                ?? _unitMovementService as IUnitMovementQuery;

            if (movementQuery != null)
            {
                IReadOnlyList<UnitMovementTileSnapshot> tiles =
                    movementQuery.GetMovementTiles(_selectedUnitId);

                bool reachable = false;
                for (int i = 0; i < tiles.Count; i++)
                {
                    if (tiles[i].Position == position
                        && tiles[i].IsReachable)
                    {
                        reachable = true;
                        break;
                    }
                }

                if (!reachable)
                {
                    Debug.LogWarning(
                        $"[MOYVA_MOVE][INPUT] Move target is not reachable. unit='{_selectedUnitId}' target={position} reachableTiles={tiles.Count}.");
                    _notifications?.Show(
                        "Ця клітинка недоступна для руху",
                        GameplayNotificationKind.Warning,
                        dedupKey: "unit-move-unreachable");
                    return true;
                }
            }

            StartMove(_selectedUnitId, position);
            return true;
        }

        private void StartMove(string unitId, Vector2Int target)
        {
            bool canCommand = CanCommandUnit(unitId);

            if (string.IsNullOrEmpty(unitId) || !canCommand)
            {
                return;
            }

            CancelMovement(MovementCancelReason.NewCommand);
            _moveCts = new CancellationTokenSource();
            _activeMoveUnitId = unitId;
            _activeMoveTarget = target;
            _cancelReason = MovementCancelReason.None;

            string groupId = _unitGroupService?.GetGroupIdOfUnit(unitId);
            if (!string.IsNullOrEmpty(groupId)
                && _unitGroupService.TryGetGroup(groupId, out var group)
                && group.Count > 1)
            {
                _signalBus.Fire(new MoveGroupRequestSignal
                {
                    GroupId = groupId,
                    TargetPosition = target,
                    RequesterOwnerId = GetLocalOwnerId(),
                });
                return;
            }

            var request = new MoveUnitRequestSignal
            {
                UnitId = unitId,
                TargetPosition = target,
                RequesterOwnerId = GetLocalOwnerId(),
            };

            _signalBus.Fire(request);
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

            if (string.Equals(
                    _selectedUnitId,
                    normalizedUnitId,
                    StringComparison.Ordinal))
            {
                return;
            }

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
            _groupMergeArmed = false;
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

            string interruptedUnitId = _activeMoveUnitId;
            _activeMoveUnitId = null;
            _activeMoveTarget = default;
            if (reason == MovementCancelReason.Dispose)
                _queuedResumeMove = null;

            if (_moveCts != null)
            {
                _moveCts.Cancel();
                _moveCts.Dispose();
                _moveCts = null;
            }

            if (reason == MovementCancelReason.Dispose
                || string.IsNullOrEmpty(interruptedUnitId))
            {
                return;
            }

            _signalBus.Fire(new InterruptMovementSignal { UnitId = interruptedUnitId });
        }
    }
}
