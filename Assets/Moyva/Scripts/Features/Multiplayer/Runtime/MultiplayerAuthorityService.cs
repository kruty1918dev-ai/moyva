using System;
using System.Collections.Generic;
using System.Threading;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
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
    internal sealed partial class MultiplayerAuthorityService :
        IInitializable,
        IDisposable,
        IConstructionConfirmRequestExecutor,
        IConstructionAuthorityEndpointRegistry,
        IUnitCommandAuthorityEndpointRegistry,
        ICaravanRemoteCommandRequester,
        ICombatRemoteCommandRequester,
        ISettlementCaptureRemoteCommandRequester
    {
        private readonly IGameCommandSyncService _syncService;
        private readonly ISessionManager         _sessionManager;
        private readonly ILocalGameplayRoleResolver _roleResolver;
        private readonly SignalBus               _signalBus;
        private IConstructionService             _constructionService;
        private IUnitMovementService _unitMovementService;
        private readonly IUnitService _unitService;
        private IUnitOwnershipQuery _unitOwnershipQuery;
        private readonly IUnitFactory            _unitFactory;
        private readonly ICaravanService _caravanService;
        private readonly ICombatCommandService _combatCommandService;
        private readonly IHealthRegistry _healthRegistry;
        private readonly ISettlementCaptureService _settlementCaptureService;
        private readonly IConstructionBuildingCombatTargetQuery _buildingTargetQuery;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IFogOwnerStateReader _ownerFog;
        private readonly Dictionary<string, HashSet<string>> _knownUnitsByPeer =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, Vector2Int> _replicatedUnitPositions =
            new(StringComparer.Ordinal);

        // Guard: не ретранслюємо події, що прийшли з мережі (уникаємо нескінченного циклу).
        private bool _applyingNetworkEvent;
        private bool _disposed;
        private readonly CancellationTokenSource _lifetime = new();

        public MultiplayerAuthorityService(
            IGameCommandSyncService syncService,
            ISessionManager         sessionManager,
            SignalBus               signalBus,
            ILocalGameplayRoleResolver roleResolver,
            [InjectOptional] IUnitMovementService unitMovementService = null,
            [InjectOptional] IUnitService unitService = null,
            [InjectOptional] IUnitOwnershipQuery unitOwnershipQuery = null,
            [InjectOptional] IUnitFactory unitFactory = null,
            [InjectOptional] ICaravanService caravanService = null,
            [InjectOptional] ICombatCommandService combatCommandService = null,
            [InjectOptional] IHealthRegistry healthRegistry = null,
            [InjectOptional] ISettlementCaptureService settlementCaptureService = null,
            [InjectOptional] IConstructionBuildingCombatTargetQuery buildingTargetQuery = null,
            [InjectOptional] IBuildingRegistry buildingRegistry = null,
            [InjectOptional] IFogOwnerStateReader ownerFog = null,
            [InjectOptional] IConstructionService constructionService = null)
        {
            _syncService         = syncService;
            _sessionManager      = sessionManager;
            _roleResolver        = roleResolver;
            _signalBus           = signalBus;
            _unitMovementService = unitMovementService;
            _unitService = unitService;
            _unitOwnershipQuery = unitOwnershipQuery;
            _unitFactory         = unitFactory;
            _caravanService = caravanService;
            _combatCommandService = combatCommandService;
            _healthRegistry = healthRegistry;
            _settlementCaptureService = settlementCaptureService;
            _buildingTargetQuery = buildingTargetQuery;
            _buildingRegistry = buildingRegistry;
            _ownerFog = ownerFog;
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
            if (_unitService == null || _unitFactory == null || _caravanService == null
                || _combatCommandService == null || _healthRegistry == null
                || _settlementCaptureService == null || _buildingTargetQuery == null
                || _ownerFog == null || _constructionService == null)
                throw new InvalidOperationException("Multiplayer gameplay authority must be installed in the Gameplay scene with its gameplay services.");
            // Локальні дії гравця: перехоплення перед виконанням
            _signalBus.Subscribe<MoveUnitRequestSignal>(OnLocalMoveUnitRequest);

            // Хост: слухає локальні результати і транслює іншим клієнтам
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlacedLocally);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolishedLocally);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMovedLocally);
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreatedLocally);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyedLocally);
            if (_caravanService != null)
                _caravanService.RouteTransferCommitted += OnRouteTransferCommittedLocally;

            // Мережеві обробники (вхідні повідомлення)
            _syncService.RegisterHandler(GameCommandType.BuildingPlace,    OnNetworkBuildingPlace);
            _syncService.RegisterHandler(GameCommandType.BuildingDemolish, OnNetworkBuildingDemolish);
            _syncService.RegisterHandler(GameCommandType.UnitMove,         OnNetworkUnitMove);
            _syncService.RegisterHandler(GameCommandType.UnitSpawn,        OnNetworkUnitSpawn);
            _syncService.RegisterHandler(GameCommandType.CaravanCommand,   OnNetworkCaravanCommand);
            _syncService.RegisterHandler(GameCommandType.CombatCommand,    OnNetworkCombatCommand);
            _syncService.RegisterHandler(GameCommandType.SettlementCaptureCommand, OnNetworkSettlementCaptureCommand);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _applyingNetworkEvent = true;
            _lifetime.Cancel();
            _syncService.RegisterHandler(GameCommandType.BuildingPlace, null);
            _syncService.RegisterHandler(GameCommandType.BuildingDemolish, null);
            _syncService.RegisterHandler(GameCommandType.UnitMove, null);
            _syncService.RegisterHandler(GameCommandType.UnitSpawn, null);
            _syncService.RegisterHandler(GameCommandType.CaravanCommand, null);
            _syncService.RegisterHandler(GameCommandType.CombatCommand, null);
            _syncService.RegisterHandler(GameCommandType.SettlementCaptureCommand, null);
            _signalBus.TryUnsubscribe<MoveUnitRequestSignal>(OnLocalMoveUnitRequest);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlacedLocally);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolishedLocally);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMovedLocally);
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreatedLocally);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyedLocally);
            if (_caravanService != null)
                _caravanService.RouteTransferCommitted -= OnRouteTransferCommittedLocally;
            _lifetime.Dispose();
        }

        private void SendConfirmedCommandToVisiblePeers(
            GameCommandType type,
            byte[] payload,
            string ownerId,
            Vector2Int position)
        {
            IReadOnlyList<Participant> participants =
                _sessionManager?.Participants;
            if (participants == null || participants.Count == 0)
            {
                _syncService.SendCommand(type, payload);
                return;
            }

            string normalizedOwnerId =
                NormalizeOwnerId(ownerId);
            string localPlayerId =
                NormalizeOwnerId(_sessionManager.LocalPlayerId);
            bool sent = false;

            for (int index = 0;
                 index < participants.Count;
                 index++)
            {
                string participantId =
                    NormalizeOwnerId(
                        participants[index]?.Identity?.PlayerId);
                if (string.IsNullOrWhiteSpace(participantId)
                    || string.Equals(
                        participantId,
                        localPlayerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (!CanPeerObserveWorldEvent(
                        participantId,
                        normalizedOwnerId,
                        position))
                {
                    continue;
                }

                _syncService.SendCommandToPeer(
                    participantId,
                    type,
                    payload);
                sent = true;
            }

            if (!sent && participants.Count == 1)
                _syncService.SendCommand(type, payload);
        }

        private void SendRequestToHost(
            GameCommandType type,
            byte[] payload)
        {
            string hostPeerId = ResolveHostPeerId();
            if (!string.IsNullOrWhiteSpace(hostPeerId)
                && !string.Equals(
                    hostPeerId,
                    NormalizeOwnerId(_sessionManager.LocalPlayerId),
                    StringComparison.Ordinal))
            {
                _syncService.SendCommandToPeer(
                    hostPeerId,
                    type,
                    payload);
                return;
            }

            _syncService.SendCommand(type, payload);
        }

        private void SendConfirmedCommandToOwnerPeers(
            GameCommandType type,
            byte[] payload,
            string ownerId)
        {
            string normalizedOwnerId =
                NormalizeOwnerId(ownerId);
            if (string.IsNullOrWhiteSpace(normalizedOwnerId))
            {
                _syncService.SendCommand(type, payload);
                return;
            }

            IReadOnlyList<Participant> participants =
                _sessionManager?.Participants;
            if (participants == null || participants.Count == 0)
            {
                _syncService.SendCommand(type, payload);
                return;
            }

            string localPlayerId =
                NormalizeOwnerId(_sessionManager.LocalPlayerId);
            bool sent = false;
            for (int index = 0;
                 index < participants.Count;
                 index++)
            {
                string participantId =
                    NormalizeOwnerId(
                        participants[index]?.Identity?.PlayerId);
                if (!string.Equals(
                        participantId,
                        normalizedOwnerId,
                        StringComparison.Ordinal)
                    || string.Equals(
                        participantId,
                        localPlayerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                _syncService.SendCommandToPeer(
                    participantId,
                    type,
                    payload);
                sent = true;
            }

            if (!sent && participants.Count == 1)
                _syncService.SendCommand(type, payload);
        }

        private string ResolveHostPeerId()
        {
            IReadOnlyList<Participant> participants =
                _sessionManager?.Participants;
            if (participants == null || participants.Count == 0)
                return string.Empty;

            for (int index = 0;
                 index < participants.Count;
                 index++)
            {
                Participant participant = participants[index];
                if (participant?.IsHost == true)
                    return NormalizeOwnerId(
                        participant.Identity?.PlayerId);
            }

            return string.Empty;
        }

        private bool CanPeerObserveWorldEvent(
            string peerOwnerId,
            string eventOwnerId,
            Vector2Int position)
        {
            string normalizedPeerOwnerId =
                NormalizeOwnerId(peerOwnerId);
            if (string.IsNullOrWhiteSpace(normalizedPeerOwnerId))
                return false;

            if (!string.IsNullOrWhiteSpace(eventOwnerId)
                && string.Equals(
                    normalizedPeerOwnerId,
                    eventOwnerId,
                    StringComparison.Ordinal))
            {
                return true;
            }

            return _ownerFog == null
                   || _ownerFog.IsVisible(
                       normalizedPeerOwnerId,
                       position);
        }

        private static string NormalizeOwnerId(
            string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId)
                ? string.Empty
                : ownerId.Trim();
        }

    }
    internal sealed class MultiplayerConstructionPlacementAuthorityPolicy :
        IConstructionPlacementAuthorityPolicy
    {
        private readonly ILocalGameplayRoleResolver _roleResolver;

        public MultiplayerConstructionPlacementAuthorityPolicy(
            ILocalGameplayRoleResolver roleResolver)
        {
            _roleResolver = roleResolver;
        }

        public bool CanCommit(
            string ownerId,
            ConstructionPlacementAttemptSource attemptSource,
            out string reason)
        {
            if (_roleResolver == null
                || _roleResolver.Resolve().IsAuthoritative)
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
