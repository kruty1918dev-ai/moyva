using System;
using System.Threading;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Авторитативна хост-модель для мультиплеєра.
    ///
    /// Потік:
    ///   Клієнт → PlaceBuildingConfirmRequestSignal / MoveUnitRequestSignal
    ///          → серіалізує запит → SendCommand (broadcast "*")
    ///          → Хост отримує Request → валідує → виконує локально
    ///          → BuildingPlacedSignal / UnitMovedSignal / UnitCreatedSignal
    ///          → Хост broadcast Confirmed → всі клієнти отримують
    ///          → клієнти застосовують стан локально
    ///
    /// Офлайн / хост: дії виконуються безпосередньо, без мережевого round-trip.
    /// </summary>
    internal sealed class MultiplayerAuthorityService :
        IInitializable,
        IDisposable,
        IConstructionConfirmRequestExecutor,
        IConstructionAuthorityEndpointRegistry,
        IUnitCommandAuthorityEndpointRegistry
    {
        private readonly IGameCommandSyncService _syncService;
        private readonly ISessionManager         _sessionManager;
        private readonly SignalBus               _signalBus;
        private IConstructionService             _constructionService;
        private IUnitMovementService _unitMovementService;
        private IUnitOwnershipQuery _unitOwnershipQuery;
        private readonly IUnitFactory            _unitFactory;

        // Guard: не ретранслюємо події, що прийшли з мережі (уникаємо нескінченного циклу).
        private bool _applyingNetworkEvent;

        public MultiplayerAuthorityService(
            IGameCommandSyncService syncService,
            ISessionManager         sessionManager,
            SignalBus               signalBus,
            [InjectOptional] IUnitMovementService unitMovementService = null,
            [InjectOptional] IUnitOwnershipQuery unitOwnershipQuery = null,
            [InjectOptional] IUnitFactory unitFactory = null,
            [InjectOptional] IConstructionService constructionService = null)
        {
            _syncService         = syncService;
            _sessionManager      = sessionManager;
            _signalBus           = signalBus;
            _unitMovementService = unitMovementService;
            _unitOwnershipQuery = unitOwnershipQuery;
            _unitFactory         = unitFactory;
            _constructionService = constructionService;
        }

        // ─── Lifecycle ───────────────────────────────────────────────────────────

        public int Priority => 100;

        public void Attach(IConstructionService constructionService)
        {
            if (constructionService == null)
                throw new ArgumentNullException(nameof(constructionService));

            if (_constructionService != null
                && !ReferenceEquals(
                    _constructionService,
                    constructionService))
            {
                Debug.LogWarning(
                    "[MultiplayerAuthority] Replacing a stale construction scene endpoint.");
            }

            _constructionService = constructionService;
        }

        public void Detach(IConstructionService constructionService)
        {
            if (ReferenceEquals(
                    _constructionService,
                    constructionService))
            {
                _constructionService = null;
            }
        }

                public void AttachUnitCommandServices(
            IUnitMovementService movementService,
            IUnitOwnershipQuery ownershipQuery)
        {
            if (movementService == null)
                throw new ArgumentNullException(nameof(movementService));
            if (ownershipQuery == null)
                throw new ArgumentNullException(nameof(ownershipQuery));

            _unitMovementService = movementService;
            _unitOwnershipQuery = ownershipQuery;

            Debug.Log(
                "[MOYVA_MOVE][AUTHORITY_BRIDGE] authority endpoint attached.");
        }

        public void DetachUnitCommandServices(
            IUnitMovementService movementService,
            IUnitOwnershipQuery ownershipQuery)
        {
            if (ReferenceEquals(_unitMovementService, movementService))
                _unitMovementService = null;
            if (ReferenceEquals(_unitOwnershipQuery, ownershipQuery))
                _unitOwnershipQuery = null;
        }

public void Initialize()
        {
            // Локальні дії гравця: перехоплення перед виконанням
            _signalBus.Subscribe<MoveUnitRequestSignal>(OnLocalMoveUnitRequest);

            // Хост: слухає локальні результати і транслює іншим клієнтам
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlacedLocally);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolishedLocally);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMovedLocally);
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreatedLocally);

            // Мережеві обробники (вхідні повідомлення)
            _syncService.RegisterHandler(GameCommandType.BuildingPlace,    OnNetworkBuildingPlace);
            _syncService.RegisterHandler(GameCommandType.BuildingDemolish, OnNetworkBuildingDemolish);
            _syncService.RegisterHandler(GameCommandType.UnitMove,         OnNetworkUnitMove);
            _syncService.RegisterHandler(GameCommandType.UnitSpawn,        OnNetworkUnitSpawn);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<MoveUnitRequestSignal>(OnLocalMoveUnitRequest);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlacedLocally);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolishedLocally);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMovedLocally);
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreatedLocally);
        }

        // ─── Локальні дії гравця (перехоплення) ─────────────────────────────────

        public bool TryHandleConfirmRequest()
        {
            if (_constructionService == null)
            {
                Debug.LogWarning("[MultiplayerAuthority] PlaceBuildingConfirmRequestSignal received, but IConstructionService is not bound in this scene.");
                return false;
            }

            if (IsOfflineOrHost())
            {
                // Хост / офлайн: виконуємо одразу; BuildingPlacedSignal транслює результат.
                _constructionService.Confirm();
                return true;
            }

            // Клієнт: зібрати pending-розміщення, скасувати локально, надіслати запити до хоста.
            var pending = _constructionService.GetPendingPlacements();
            if (pending == null || pending.Count == 0)
            {
                _constructionService.Cancel();
                return true;
            }

            string ownerId = _constructionService.GetActiveOwner();
            var placementQuery =
                _constructionService as IConstructionPlacementQuery;
            var intentSource =
                _constructionService
                    as IConstructionPendingPlacementIntentSource;
            foreach (var kv in pending)
            {
                ConstructionPlacementCommitIntent intent =
                    intentSource != null
                    && intentSource.TryGetPendingPlacementIntent(
                        kv.Key,
                        out ConstructionPlacementCommitIntent
                            pendingIntent)
                        ? pendingIntent
                        : ConstructionPlacementCommitIntent.None;
                if (placementQuery != null)
                {
                    ConstructionPlacementQueryResult placement =
                        placementQuery.EvaluatePlacement(
                            CreateClientPlacementPreflightRequest(
                                kv.Value,
                                kv.Key,
                                ownerId,
                                intent.Rotation));
                    if (!placement.CanPreview
                        || !placement.ResourcesValid)
                    {
                        continue;
                    }
                }

                var payload = new BuildingPlacePayload(
                    GameActionMessageKind.Request,
                    kv.Value,
                    kv.Key,
                    ownerId,
                    ownerId,
                    intent.HasRelocationSource,
                    intent.RelocationSourcePosition
                        .GetValueOrDefault(),
                    intent.SatisfiedReplacementBuildingId,
                    intent.Rotation);

                _syncService.SendCommand(GameCommandType.BuildingPlace, payload.ToBytes());
            }

            // Pending previews remain until a host confirmation is received.
            // This preserves unaffordable previews and their exact deficit, and
            // also prevents a lost/rejected request from silently deleting the
            // player's placement intent.
            return true;
        }

        internal static ConstructionPlacementQueryRequest
            CreateClientPlacementPreflightRequest(
                string buildingId,
                Vector2Int position,
                string ownerId,
                ConstructionRotation rotation =
                    ConstructionRotation.Degrees0)
            => new ConstructionPlacementQueryRequest(
                buildingId,
                position,
                ignoredPendingPosition: position,
                includeResources: true,
                includeDetails: true,
                ownerId: ownerId,
                includePendingPlacements: false,
                attemptSource:
                    ConstructionPlacementAttemptSource
                        .NetworkRequest,
                allowUniquePreviewRelocation: false,
                rotation: rotation);

private void OnLocalMoveUnitRequest(MoveUnitRequestSignal signal)
{
    long trace = UnitMovementDiagnostics.TraceForUnit(signal.UnitId);
    double started = UnitMovementDiagnostics.NowMs();

    UnitMovementDiagnostics.Log(
        trace,
        "AUTHORITY_REQUEST_RECEIVED",
        $"unit={UnitMovementDiagnostics.Safe(signal.UnitId)}; " +
        $"target={signal.TargetPosition}; " +
        $"requesterInSignal={UnitMovementDiagnostics.Safe(signal.RequesterOwnerId)}; " +
        $"movementServiceBound={_unitMovementService != null}; " +
        $"ownershipBound={_unitOwnershipQuery != null}");

    if (_unitMovementService == null || _unitOwnershipQuery == null)
    {
        UnitMovementDiagnostics.Error(
            trace,
            "AUTHORITY_REJECT_SERVICES",
            "unit movement or ownership service is not bound");

        Debug.LogWarning(
            "[MultiplayerAuthority] Move request rejected because unit command services are not bound in this scene.");
        return;
    }

    string requesterOwnerId =
        string.IsNullOrWhiteSpace(signal.RequesterOwnerId)
            ? _sessionManager?.LocalPlayerId
            : signal.RequesterOwnerId;

    string unitOwnerId =
        _unitOwnershipQuery.GetUnitOwnerId(signal.UnitId);

    bool authorized =
        IsUnitCommandAuthorized(
            unitOwnerId,
            requesterOwnerId);

    if (!authorized)
    {
        UnitMovementDiagnostics.Warn(
            trace,
            "AUTHORITY_REJECT_OWNERSHIP",
            $"unit={signal.UnitId}; " +
            $"requester={UnitMovementDiagnostics.Safe(requesterOwnerId)}; " +
            $"unitOwner={UnitMovementDiagnostics.Safe(unitOwnerId)}");

        Debug.LogWarning(
            $"[Authority] Rejected local UnitMove for '{signal.UnitId}': " +
            $"requester '{requesterOwnerId}' does not own unit '{unitOwnerId}'.");
        return;
    }

    bool offlineOrHost = IsOfflineOrHost();

    if (offlineOrHost)
    {
        UnitMovementDiagnostics.Log(
            trace,
            "AUTHORITY_ROUTE_LOCAL",
            $"unit={signal.UnitId}; target={signal.TargetPosition}; " +
            $"requester={UnitMovementDiagnostics.Safe(requesterOwnerId)}; " +
            $"unitOwner={UnitMovementDiagnostics.Safe(unitOwnerId)}; " +
            $"dispatchMs={UnitMovementDiagnostics.Ms(UnitMovementDiagnostics.NowMs() - started)}");

        _ = _unitMovementService.MoveUnitAsync(
            signal.UnitId,
            signal.TargetPosition,
            CancellationToken.None);
        return;
    }

    UnitMovementDiagnostics.Log(
        trace,
        "AUTHORITY_ROUTE_NETWORK",
        $"unit={signal.UnitId}; target={signal.TargetPosition}; " +
        $"requester={UnitMovementDiagnostics.Safe(requesterOwnerId)}; " +
        $"unitOwner={UnitMovementDiagnostics.Safe(unitOwnerId)}");

    var payload = new UnitMovePayload(
        GameActionMessageKind.Request,
        signal.UnitId,
        signal.TargetPosition);

    _syncService.SendCommand(
        GameCommandType.UnitMove,
        payload.ToBytes());

    UnitMovementDiagnostics.Log(
        trace,
        "AUTHORITY_NETWORK_SENT",
        $"unit={signal.UnitId}; target={signal.TargetPosition}; " +
        $"elapsedMs={UnitMovementDiagnostics.Ms(UnitMovementDiagnostics.NowMs() - started)}");
}
        // ─── Хост: трансляція після локального виконання ─────────────────────────

        private void OnBuildingPlacedLocally(BuildingPlacedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;

            var payload = new BuildingPlacePayload(
                GameActionMessageKind.Confirmed,
                signal.BuildingId,
                signal.Position,
                signal.OwnerId,
                signal.SourceFactionId,
                signal.HasRelocationSource,
                signal.RelocationSourcePosition,
                rotation: ConstructionRotationUtility.Normalize(
                    signal.RotationQuarterTurns));
            _syncService.SendCommand(GameCommandType.BuildingPlace, payload.ToBytes());
        }

        private void OnBuildingDemolishedLocally(BuildingDemolishedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;

            var payload = new BuildingDemolishPayload(
                GameActionMessageKind.Confirmed,
                signal.Position,
                signal.OwnerId);
            _syncService.SendCommand(GameCommandType.BuildingDemolish, payload.ToBytes());
        }

        private void OnUnitMovedLocally(UnitMovedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;

            // Транслюємо кожен крок руху; клієнти синхронно запускають власний MoveUnitAsync.
            var payload = new UnitMovePayload(
                GameActionMessageKind.Confirmed,
                signal.UnitId,
                signal.NewPosition);
            _syncService.SendCommand(GameCommandType.UnitMove, payload.ToBytes());
        }

        private void OnUnitCreatedLocally(UnitCreatedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;

            var payload = new UnitSpawnPayload(
                GameActionMessageKind.Confirmed,
                signal.UnitId,
                signal.UnitTypeId,
                signal.Position,
                signal.OwnerId);
            _syncService.SendCommand(GameCommandType.UnitSpawn, payload.ToBytes());
        }

        // ─── Мережеві обробники (вхідні повідомлення) ────────────────────────────

        private void OnNetworkBuildingPlace(string senderId, byte[] body)
        {
            var data = BuildingPlacePayload.FromBytes(body);

            if (_constructionService == null)
            {
                Debug.LogWarning("[MultiplayerAuthority] BuildingPlace command received, but IConstructionService is not bound in this scene.");
                return;
            }

            if (data.Kind == GameActionMessageKind.Request)
            {
                // Лише хост обробляє запити.
                if (!IsOfflineOrHost()) return;
                if (!TryResolveAuthorizedRequestOwner(
                        senderId,
                        data.OwnerId,
                        data.SourceFactionId,
                        out string authorizedOwnerId,
                        out string authorizationReason))
                {
                    Debug.LogWarning(
                        $"[Authority] Rejected BuildingPlace from '{senderId}': {authorizationReason}");
                    return;
                }

                _applyingNetworkEvent = true;
                try
                {
                    ConstructionPlacementCommitIntent intent =
                        data.ToCommitIntent();
                    bool placed;
                    if (_constructionService
                        is IAuthoritativeConstructionPlacementExecutor
                            authoritativeExecutor)
                    {
                        placed =
                            authoritativeExecutor
                                .TryPlaceAuthoritatively(
                                    data.BuildingId,
                                    data.Position,
                                    authorizedOwnerId,
                                    intent);
                    }
                    else if (!intent.HasRelocationSource
                             && string.IsNullOrWhiteSpace(
                                 intent
                                     .SatisfiedReplacementBuildingId))
                    {
                        placed = _constructionService.TryDirectPlace(
                            data.BuildingId,
                            data.Position,
                            authorizedOwnerId);
                    }
                    else
                    {
                        Debug.LogError(
                            "[MultiplayerAuthority] Construction service cannot execute the requested placement intent authoritatively.");
                        placed = false;
                    }

                    if (placed)
                    {
                        // Хост вручну транслює підтвердження (BuildingPlacedSignal вже заблоковано флагом).
                        var confirmed = new BuildingPlacePayload(
                            GameActionMessageKind.Confirmed,
                            data.BuildingId,
                            data.Position,
                            authorizedOwnerId,
                            authorizedOwnerId,
                            intent.HasRelocationSource,
                            intent.RelocationSourcePosition
                                .GetValueOrDefault(),
                            intent.SatisfiedReplacementBuildingId,
                            intent.Rotation);
                        _syncService.SendCommand(GameCommandType.BuildingPlace, confirmed.ToBytes());
                    }
                    else
                    {
                        Debug.LogWarning($"[Authority] Хост відхилив BuildingPlace від {senderId}: " +
                                         $"buildingId={data.BuildingId} pos={data.Position}");
                    }
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
            }
            else if (data.Kind == GameActionMessageKind.Confirmed)
            {
                // Клієнти застосовують підтверджене розміщення.
                if (IsOfflineOrHost()) return;
                if (!IsAuthorizedHostSender(senderId))
                {
                    Debug.LogWarning(
                        $"[Authority] Ignored BuildingPlace confirmation from non-host '{senderId}'.");
                    return;
                }

                _applyingNetworkEvent = true;
                try
                {
                    string confirmedOwnerId =
                        string.IsNullOrWhiteSpace(data.SourceFactionId)
                            ? data.OwnerId
                            : data.SourceFactionId;
                    ConstructionPlacementCommitIntent intent =
                        data.ToCommitIntent();
                    bool applied;
                    if (_constructionService
                        is IConfirmedConstructionPlacementIntentApplier
                            intentApplier)
                    {
                        applied =
                            intentApplier
                                .TryApplyConfirmedPlacement(
                                    data.BuildingId,
                                    data.Position,
                                    confirmedOwnerId,
                                    intent);
                    }
                    else if (!intent.HasRelocationSource
                             && string.IsNullOrWhiteSpace(
                                 intent
                                     .SatisfiedReplacementBuildingId)
                             && _constructionService
                                 is IConfirmedConstructionPlacementApplier
                                     legacyApplier)
                    {
                        applied =
                            legacyApplier.TryApplyConfirmedPlacement(
                                data.BuildingId,
                                data.Position,
                                confirmedOwnerId);
                    }
                    else
                    {
                        Debug.LogError(
                            "[MultiplayerAuthority] Construction service cannot apply a host-confirmed placement intent without charging client resources.");
                        return;
                    }
                    if (applied
                        && _constructionService
                            .TryGetPendingBuildingIdAt(
                                data.Position,
                                out string pendingBuildingId)
                        && string.Equals(
                            pendingBuildingId,
                            data.BuildingId,
                            System.StringComparison.Ordinal))
                    {
                        _constructionService.RemovePendingAt(
                            data.Position);
                    }
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
            }
        }

        private void OnNetworkBuildingDemolish(string senderId, byte[] body)
        {
            var data = BuildingDemolishPayload.FromBytes(body);

            if (_constructionService == null)
            {
                Debug.LogWarning("[MultiplayerAuthority] BuildingDemolish command received, but IConstructionService is not bound in this scene.");
                return;
            }

            if (data.Kind == GameActionMessageKind.Request)
            {
                if (!IsOfflineOrHost()) return;
                if (!TryResolveAuthorizedRequestOwner(
                        senderId,
                        data.OwnerId,
                        requestedSourceOwnerId: null,
                        out string authorizedOwnerId,
                        out string authorizationReason))
                {
                    Debug.LogWarning(
                        $"[Authority] Rejected BuildingDemolish from '{senderId}': {authorizationReason}");
                    return;
                }

                _applyingNetworkEvent = true;
                try
                {
                    bool demolished =
                        _constructionService.TryDemolishByFaction(
                            data.Position,
                            authorizedOwnerId);
                    if (demolished)
                    {
                        var confirmed = new BuildingDemolishPayload(
                            GameActionMessageKind.Confirmed,
                            data.Position,
                            authorizedOwnerId);
                        _syncService.SendCommand(GameCommandType.BuildingDemolish, confirmed.ToBytes());
                    }
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
            }
            else if (data.Kind == GameActionMessageKind.Confirmed)
            {
                if (IsOfflineOrHost()) return;
                if (!IsAuthorizedHostSender(senderId))
                {
                    Debug.LogWarning(
                        $"[Authority] Ignored BuildingDemolish confirmation from non-host '{senderId}'.");
                    return;
                }

                _applyingNetworkEvent = true;
                try
                {
                    if (_constructionService
                        is not IConfirmedConstructionDemolitionApplier applier)
                    {
                        Debug.LogError(
                            "[MultiplayerAuthority] Construction service cannot apply a host-confirmed demolition without local turn authority.");
                        return;
                    }

                    if (!applier.TryApplyConfirmedDemolition(
                            data.Position,
                            data.OwnerId))
                    {
                        Debug.LogWarning(
                            $"[Authority] Host-confirmed demolition could not be applied at {data.Position} for owner '{data.OwnerId}'.");
                    }
                }
                finally { _applyingNetworkEvent = false; }
            }
        }

        private void OnNetworkUnitMove(string senderId, byte[] body)
        {
            var data = UnitMovePayload.FromBytes(body);

            if (_unitMovementService == null)
            {
                Debug.LogWarning("[MultiplayerAuthority] UnitMove command received, but IUnitMovementService is not bound in this scene.");
                return;
            }

            if (data.Kind == GameActionMessageKind.Request)
            {
                // Лише хост обробляє запити на рух.
                if (!IsOfflineOrHost()) return;
                if (_unitOwnershipQuery == null)
                    return;

                string unitOwnerId = _unitOwnershipQuery.GetUnitOwnerId(data.UnitId);
                if (!TryResolveAuthorizedRequestOwner(
                        senderId,
                        unitOwnerId,
                        unitOwnerId,
                        out _,
                        out string authorizationReason))
                {
                    Debug.LogWarning(
                        $"[Authority] Rejected UnitMove from '{senderId}' for '{data.UnitId}': {authorizationReason}");
                    return;
                }
                // Хост виконує рух; UnitMovedSignal транслює кожен крок через OnUnitMovedLocally.
                _ = _unitMovementService.MoveUnitAsync(data.UnitId, data.TargetPosition, CancellationToken.None);
            }
            else // Confirmed
            {
                // Клієнт запускає власний рух до тієї ж позиції (детерміноване pathfinding).
                if (IsOfflineOrHost()) return;
                if (!IsAuthorizedHostSender(senderId))
                {
                    Debug.LogWarning(
                        $"[Authority] Ignored UnitMove confirmation from non-host '{senderId}'.");
                    return;
                }

                _ = _unitMovementService.MoveUnitAsync(data.UnitId, data.TargetPosition, CancellationToken.None);
            }
        }

        private void OnNetworkUnitSpawn(string senderId, byte[] body)
        {
            var data = UnitSpawnPayload.FromBytes(body);

            if (_unitFactory == null)
            {
                Debug.LogWarning("[MultiplayerAuthority] UnitSpawn command received, but IUnitFactory is not bound in this scene.");
                return;
            }

            if (data.Kind == GameActionMessageKind.Request)
            {
                // Резерв для майбутнього: клієнт запитує спавн юніта.
                if (!IsOfflineOrHost()) return;

                // Хост створює юніта; UnitCreatedSignal надішле Confirmed із призначеним ID.
                _unitFactory.CreateUnit(data.UnitTypeId, data.Position, data.OwnerId);
            }
            else // Confirmed
            {
                // Клієнт створює юніта з тим самим ID, що і на хості.
                if (IsOfflineOrHost()) return;

                _applyingNetworkEvent = true;
                try
                {
                    _unitFactory.CreateUnitWithId(
                        data.AssignedUnitId, data.UnitTypeId, data.Position, data.OwnerId);
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
            }
        }

        // ─── Допоміжне ───────────────────────────────────────────────────────────

        private bool IsOfflineOrHost()
        {
            if (_sessionManager.Participants == null || _sessionManager.Participants.Count == 0)
                return true;
            return _sessionManager.IsLocalPlayerHost;
        }

        private bool TryResolveAuthorizedRequestOwner(
            string senderId,
            string requestedOwnerId,
            string requestedSourceOwnerId,
            out string authorizedOwnerId,
            out string reason)
            => TryResolveAuthorizedRequestOwner(
                _sessionManager?.Participants,
                senderId,
                requestedOwnerId,
                requestedSourceOwnerId,
                out authorizedOwnerId,
                out reason);

        internal static bool TryResolveAuthorizedRequestOwner(
            System.Collections.Generic.IReadOnlyList<Participant>
                participants,
            string senderId,
            string requestedOwnerId,
            string requestedSourceOwnerId,
            out string authorizedOwnerId,
            out string reason)
        {
            authorizedOwnerId = null;
            reason = null;
            string normalizedSender = senderId?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedSender))
            {
                reason = "Transport sender identity is empty.";
                return false;
            }

            Participant authorizedParticipant = null;
            if (participants != null)
            {
                for (int index = 0;
                     index < participants.Count;
                     index++)
                {
                    Participant candidate = participants[index];
                    if (candidate?.Identity == null
                        || candidate.IsBot
                        || !string.Equals(
                            candidate.Identity.PlayerId,
                            normalizedSender,
                            System.StringComparison.Ordinal))
                    {
                        continue;
                    }

                    authorizedParticipant = candidate;
                    break;
                }
            }

            if (authorizedParticipant == null)
            {
                reason =
                    $"Sender '{normalizedSender}' is not an active human participant.";
                return false;
            }

            if (!MatchesRequestedOwner(
                    requestedOwnerId,
                    normalizedSender)
                || !MatchesRequestedOwner(
                    requestedSourceOwnerId,
                    normalizedSender))
            {
                reason =
                    $"Requested owner does not match sender '{normalizedSender}'.";
                return false;
            }

            authorizedOwnerId = normalizedSender;
            return true;
        }

        private bool IsAuthorizedHostSender(string senderId)
            => IsAuthorizedHostSender(
                _sessionManager?.Participants,
                senderId);

        internal static bool IsAuthorizedHostSender(
            System.Collections.Generic.IReadOnlyList<Participant>
                participants,
            string senderId)
        {
            string normalizedSender = senderId?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedSender))
                return false;

            if (participants == null)
                return false;

            for (int index = 0; index < participants.Count; index++)
            {
                Participant participant = participants[index];
                if (participant?.Identity != null
                    && participant.IsHost
                    && string.Equals(
                        participant.Identity.PlayerId,
                        normalizedSender,
                        System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesRequestedOwner(
            string requestedOwnerId,
            string senderId)
            => string.IsNullOrWhiteSpace(requestedOwnerId)
                || string.Equals(
                    requestedOwnerId.Trim(),
                    senderId,
                    System.StringComparison.Ordinal);

        internal static bool IsUnitCommandAuthorized(
            string unitOwnerId,
            string requesterOwnerId)
            => !string.IsNullOrWhiteSpace(unitOwnerId)
               && !string.IsNullOrWhiteSpace(requesterOwnerId)
               && string.Equals(
                   unitOwnerId.Trim(),
                   requesterOwnerId.Trim(),
                   StringComparison.Ordinal);
    }

    internal sealed class MultiplayerConstructionPlacementAuthorityPolicy :
        IConstructionPlacementAuthorityPolicy
    {
        private readonly ISessionManager _sessionManager;

        public MultiplayerConstructionPlacementAuthorityPolicy(
            ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public bool CanCommit(
            string ownerId,
            ConstructionPlacementAttemptSource attemptSource,
            out string reason)
        {
            var participants = _sessionManager?.Participants;
            if (participants == null
                || participants.Count == 0
                || _sessionManager.IsLocalPlayerHost)
            {
                reason = null;
                return true;
            }

            reason =
                "Placement is awaiting authoritative host confirmation.";
            return false;
        }
    }

    internal sealed class MultiplayerGamePauseModePolicy :
        IGamePauseModePolicy
    {
        private readonly ISessionManager _sessionManager;

        public MultiplayerGamePauseModePolicy(
            ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public bool IsMultiplayerSessionActive
            => _sessionManager?.Participants != null
               && _sessionManager.Participants.Count > 0;
    }
}
