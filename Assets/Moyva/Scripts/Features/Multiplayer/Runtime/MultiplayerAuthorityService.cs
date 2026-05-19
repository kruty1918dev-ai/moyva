using System;
using System.Threading;
using Kruty1918.Moyva.Construction.API;
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
    internal sealed class MultiplayerAuthorityService : IInitializable, IDisposable, IConstructionConfirmRequestExecutor
    {
        private readonly IGameCommandSyncService _syncService;
        private readonly ISessionManager         _sessionManager;
        private readonly SignalBus               _signalBus;
        private readonly IConstructionService    _constructionService;
        private readonly IUnitMovementService    _unitMovementService;
        private readonly IUnitFactory            _unitFactory;

        // Guard: не ретранслюємо події, що прийшли з мережі (уникаємо нескінченного циклу).
        private bool _applyingNetworkEvent;

        public MultiplayerAuthorityService(
            IGameCommandSyncService syncService,
            ISessionManager         sessionManager,
            SignalBus               signalBus,
            [InjectOptional] IUnitMovementService unitMovementService = null,
            [InjectOptional] IUnitFactory unitFactory = null,
            [InjectOptional] IConstructionService constructionService = null)
        {
            _syncService         = syncService;
            _sessionManager      = sessionManager;
            _signalBus           = signalBus;
            _unitMovementService = unitMovementService;
            _unitFactory         = unitFactory;
            _constructionService = constructionService;
        }

        // ─── Lifecycle ───────────────────────────────────────────────────────────

        public int Priority => 100;

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
            foreach (var kv in pending)
            {
                var payload = new BuildingPlacePayload(
                    GameActionMessageKind.Request,
                    kv.Value,
                    kv.Key,
                    ownerId,
                    ownerId);

                _syncService.SendCommand(GameCommandType.BuildingPlace, payload.ToBytes());
            }

            _constructionService.Cancel();
            return true;
        }

        private void OnLocalMoveUnitRequest(MoveUnitRequestSignal signal)
        {
            if (_unitMovementService == null)
            {
                Debug.LogWarning("[MultiplayerAuthority] MoveUnitRequestSignal received, but IUnitMovementService is not bound in this scene.");
                return;
            }

            if (IsOfflineOrHost())
            {
                // Хост / офлайн: виконуємо рух одразу; UnitMovedSignal транслює кожен крок.
                _ = _unitMovementService.MoveUnitAsync(signal.UnitId, signal.TargetPosition, CancellationToken.None);
                return;
            }

            // Клієнт: надсилаємо запит до хоста.
            var payload = new UnitMovePayload(
                GameActionMessageKind.Request,
                signal.UnitId,
                signal.TargetPosition);
            _syncService.SendCommand(GameCommandType.UnitMove, payload.ToBytes());
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
                signal.SourceFactionId);
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

                _applyingNetworkEvent = true;
                try
                {
                    bool placed = _constructionService.TryDirectPlace(
                        data.BuildingId, data.Position, data.SourceFactionId);

                    if (placed)
                    {
                        // Хост вручну транслює підтвердження (BuildingPlacedSignal вже заблоковано флагом).
                        var confirmed = new BuildingPlacePayload(
                            GameActionMessageKind.Confirmed,
                            data.BuildingId,
                            data.Position,
                            data.OwnerId,
                            data.SourceFactionId);
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
            else // Confirmed
            {
                // Клієнти застосовують підтверджене розміщення.
                if (IsOfflineOrHost()) return;

                _applyingNetworkEvent = true;
                try
                {
                    _constructionService.TryDirectPlace(
                        data.BuildingId, data.Position, data.SourceFactionId);
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

                _applyingNetworkEvent = true;
                try
                {
                    bool demolished = _constructionService.TryDemolishAt(data.Position);
                    if (demolished)
                    {
                        var confirmed = new BuildingDemolishPayload(
                            GameActionMessageKind.Confirmed,
                            data.Position,
                            data.OwnerId);
                        _syncService.SendCommand(GameCommandType.BuildingDemolish, confirmed.ToBytes());
                    }
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
            }
            else // Confirmed
            {
                if (IsOfflineOrHost()) return;

                _applyingNetworkEvent = true;
                try { _constructionService.TryDemolishAt(data.Position); }
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
                // Хост виконує рух; UnitMovedSignal транслює кожен крок через OnUnitMovedLocally.
                _ = _unitMovementService.MoveUnitAsync(data.UnitId, data.TargetPosition, CancellationToken.None);
            }
            else // Confirmed
            {
                // Клієнт запускає власний рух до тієї ж позиції (детерміноване pathfinding).
                if (IsOfflineOrHost()) return;

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
    }
}
