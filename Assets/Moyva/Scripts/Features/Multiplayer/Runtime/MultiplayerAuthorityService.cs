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
    internal sealed partial class MultiplayerAuthorityService :
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
