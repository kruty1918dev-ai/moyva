using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// На старті нового світу готує owner-контекст і видає стартові ресурси гравцю.
    ///
    /// При завантаженні сучасного збереження не втручається; старі сейви без economy-блоку мігрує.
    /// </summary>
    internal sealed class BootstrapGameInitializer : IInitializable, IDisposable
    {
        private const string EconomySaveModuleFullName = "Kruty1918.Moyva.Economy.Runtime.EconomySaveModule";

        private readonly IConstructionService _constructionService;
        private readonly SignalBus _signalBus;
        private readonly BootstrapGameSettings _settings;
        private readonly ISaveService _saveService;
        private readonly BootstrapStartingPositionState _startingPositionState;
        private readonly BootstrapStarterPackState _starterPackState;
        private bool _hasPendingWorldGeneratedSignal;
        private bool _bootstrapApplied;
        private bool _starterPackGrantEnabled;

    #pragma warning disable CS0649
        [InjectOptional] private ISessionManager _sessionManager;
        [InjectOptional] private ISaveInspectorService _saveInspectorService;
    #pragma warning restore CS0649

        [Inject]
        public BootstrapGameInitializer(
            IConstructionService constructionService,
            SignalBus signalBus,
            BootstrapGameSettings settings,
            ISaveService saveService,
            BootstrapStartingPositionState startingPositionState,
            BootstrapStarterPackState starterPackState)
        {
            _constructionService   = constructionService;
            _signalBus             = signalBus;
            _settings              = settings;
            _saveService           = saveService;
            _startingPositionState = startingPositionState;
            _starterPackState      = starterPackState;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            _signalBus.Subscribe<SettlementCreatedSignal>(OnSettlementCreated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            _signalBus.TryUnsubscribe<SettlementCreatedSignal>(OnSettlementCreated);
        }

        // ─────────────────────────────────────────────────────────────

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            _hasPendingWorldGeneratedSignal = true;

            TryApplyBootstrap();
        }

        private void OnWorldSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            if (signal.Assignments == null || signal.Assignments.Length == 0)
                return;

            TryApplyBootstrap();
        }

        private void TryApplyBootstrap()
        {
            if (_bootstrapApplied || !_hasPendingWorldGeneratedSignal)
                return;

            string activeOwnerId = ResolveBootstrapOwnerId();
            _constructionService.SetActiveOwner(activeOwnerId);

            // Multiplayer client guard: only the host grants the starter pack.
            // Joining clients receive the host's economy state via the world snapshot
            // (see EconomyMultiplayerBridge) and must not duplicate starter resources.
            if (IsMultiplayerClient())
            {
                _starterPackGrantEnabled = false;
                _bootstrapApplied = true;
                Debug.Log("[Bootstrap] Multiplayer client detected — skipping starter pack grant; awaiting host snapshot.");
                return;
            }

            // Якщо є сучасне збереження економіки — не робимо bootstrap.
            // Старі сейви не мають EconomySaveModule, тому їм потрібна одноразова міграція стартових ресурсів.
            int slot = GameLaunchContext.SaveSlot;
            if (GameLaunchContext.IsAutoLoadEnabled() && _saveService.HasSave(slot))
            {
                if (_starterPackState.HasGranted(activeOwnerId))
                {
                    _starterPackGrantEnabled = false;
                    _bootstrapApplied = true;
                    return;
                }

                if (HasAnyPersistedOwnerResources(slot, activeOwnerId))
                {
                    _starterPackState.MarkGranted(activeOwnerId);
                    _starterPackGrantEnabled = false;
                    _bootstrapApplied = true;
                    return;
                }

                _starterPackGrantEnabled = true;
                Debug.LogWarning($"[Bootstrap] Save slot {slot} не має підтверджених economy-ресурсів для owner '{activeOwnerId}'. Виконується міграційна видача стартових ресурсів.");
            }
            else
            {
                _starterPackGrantEnabled = ShouldGrantStarterPackForCurrentLaunch();
            }

            if (_starterPackGrantEnabled && !_starterPackState.HasGranted(activeOwnerId))
            {
                TryGrantStarterPack(string.Empty, activeOwnerId);
                TryPersistStarterGrant(slot, activeOwnerId, "після видачі стартових ресурсів");
            }

            if (!CanRunBootstrapLogic())
                return;

            _bootstrapApplied = true;
        }

        private void OnSettlementCreated(SettlementCreatedSignal signal)
        {
            if (!_starterPackGrantEnabled)
                return;

            if (string.IsNullOrWhiteSpace(signal.SettlementId))
                return;

            string ownerId = NormalizeOwnerId(signal.OwnerId);
            if (_starterPackState.HasGranted(ownerId))
                return;

            if (!TryGrantStarterPack(signal.SettlementId, ownerId))
                return;

            int slot = GameLaunchContext.SaveSlot;
            TryPersistStarterGrant(slot, ownerId, "після видачі стартових ресурсів (поселення)");
        }

        private bool TryPersistStarterGrant(int slot, string ownerId, string contextLabel)
        {
            bool hasStarterEntries = HasStarterPackEntries();
            if (!GameLaunchContext.IsAutoSaveEnabled())
            {
                _starterPackState.MarkGranted(ownerId);
                return true;
            }

            try
            {
                _saveService?.Save(slot);
                Debug.Log($"[Bootstrap] Автосейв {contextLabel} у слот {slot}.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Bootstrap] Не вдалося зберегти {contextLabel}: {ex}");
                return false;
            }

            if (hasStarterEntries && !HasPersistedStarterResources(slot, ownerId))
            {
                Debug.LogWarning($"[Bootstrap] Після автосейву economy-блок у слоті {slot} не містить очікуваних стартових ресурсів owner '{ownerId}'. Маркер granted не записано.");
                return false;
            }

            _starterPackState.MarkGranted(ownerId);

            try
            {
                _saveService?.Save(slot);
                Debug.Log($"[Bootstrap] Маркер стартового пакета збережено для owner '{ownerId}' у слот {slot}.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Bootstrap] Не вдалося зберегти маркер стартового пакета для owner '{ownerId}': {ex}");
                return false;
            }
        }

        private bool HasAnyPersistedOwnerResources(int slot, string ownerId)
        {
            if (!TryReadEconomyOwnerResources(slot, ownerId, out var resources))
                return false;

            foreach (var resource in resources)
            {
                if (resource.Value > 0.0001f)
                    return true;
            }

            return false;
        }

        private bool HasPersistedStarterResources(int slot, string ownerId)
        {
            if (!HasStarterPackEntries())
                return true;

            if (!TryReadEconomyOwnerResources(slot, ownerId, out var resources))
                return false;

            bool hasExpectedEntry = false;
            var entries = _settings.InitialResources;
            for (int index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                    continue;

                hasExpectedEntry = true;
                string resourceId = entry.ResourceId.Trim();
                if (!resources.TryGetValue(resourceId, out float amount) || amount + 0.0001f < entry.Amount)
                    return false;
            }

            return hasExpectedEntry;
        }

        private bool TryReadEconomyOwnerResources(int slot, string ownerId, out Dictionary<string, float> resources)
        {
            resources = new Dictionary<string, float>(StringComparer.Ordinal);
            if (_saveInspectorService == null || !_saveInspectorService.TryGetBlockPayload(slot, EconomySaveModuleFullName, out byte[] payload))
                return false;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            try
            {
                using var stream = new MemoryStream(payload);
                using var reader = new BinaryReader(stream);

                int schemaVersion = reader.ReadInt32();
                if (schemaVersion != 1)
                    return false;

                int ownerCount = reader.ReadInt32();
                for (int ownerIndex = 0; ownerIndex < ownerCount; ownerIndex++)
                {
                    string savedOwnerId = NormalizeOwnerId(reader.ReadString());
                    int resourceCount = reader.ReadInt32();
                    bool isTargetOwner = string.Equals(savedOwnerId, normalizedOwnerId, StringComparison.Ordinal);

                    for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                    {
                        string resourceId = reader.ReadString();
                        float amount = reader.ReadSingle();
                        if (!isTargetOwner || string.IsNullOrWhiteSpace(resourceId) || amount <= 0f)
                            continue;

                        string normalizedResourceId = resourceId.Trim();
                        if (resources.TryGetValue(normalizedResourceId, out float current))
                            resources[normalizedResourceId] = current + amount;
                        else
                            resources[normalizedResourceId] = amount;
                    }
                }

                return resources.Count > 0;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Bootstrap] Не вдалося прочитати economy-блок слота {slot}: {ex.Message}");
                resources.Clear();
                return false;
            }
        }

        private bool HasStarterPackEntries()
        {
            var entries = _settings.InitialResources;
            if (entries == null || entries.Count == 0)
                return false;

            for (int index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry != null && !string.IsNullOrWhiteSpace(entry.ResourceId) && entry.Amount > 0f)
                    return true;
            }

            return false;
        }

        private bool TryGrantStarterPack(string settlementId, string ownerId)
        {
            var entries = _settings.InitialResources;
            if (entries == null || entries.Count == 0)
                return true;

            var payload = new List<StarterPackResourceEntrySignal>();
            bool grantedAny = false;
            for (int index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                    continue;

                payload.Add(new StarterPackResourceEntrySignal
                {
                    ResourceId = entry.ResourceId.Trim(),
                    Amount = entry.Amount,
                });
                grantedAny = true;
            }

            if (grantedAny)
            {
                _signalBus.Fire(new GrantStarterPackResourcesSignal
                {
                    SettlementId = settlementId,
                    OwnerId = ownerId,
                    Entries = payload.ToArray(),
                });

                string target = string.IsNullOrWhiteSpace(settlementId)
                    ? "owner pool"
                    : $"settlement='{settlementId}'";
                Debug.Log($"[Bootstrap] Видано стартовий пакет owner='{ownerId}' для {target}.");
            }

            return true;
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId)
                ? "player_0"
                : ownerId.Trim();
        }

        /// <summary>
        /// True when the local participant is connected to a multiplayer session
        /// (more than one participant in the lobby) and is not the host.
        /// </summary>
        private bool IsMultiplayerClient()
        {
            if (_sessionManager == null)
                return false;
            if (_sessionManager.Participants == null || _sessionManager.Participants.Count <= 1)
                return false;
            return !_sessionManager.IsLocalPlayerHost;
        }

        private static bool ShouldGrantStarterPackForCurrentLaunch()
        {
            switch (GameLaunchContext.Mode)
            {
                case GameLaunchMode.MenuLoadGame:
                case GameLaunchMode.MenuJoinGame:
                    return false;
                case GameLaunchMode.DirectGameplayTest:
                case GameLaunchMode.MenuNewGame:
                case GameLaunchMode.MenuMultiplayerGame:
                case GameLaunchMode.Unknown:
                default:
                    return true;
            }
        }

        private string ResolveLocalActiveOwnerId(IReadOnlyList<SpawnPositionAssignment> targets)
        {
            string localPlayerId = _sessionManager?.LocalPlayerId;

            if (targets != null)
            {
                for (int index = 0; index < targets.Count; index++)
                {
                    var target = targets[index];
                    if (target.IsBot)
                        continue;

                    if (!string.IsNullOrWhiteSpace(localPlayerId)
                        && string.Equals(target.ParticipantId, localPlayerId, StringComparison.Ordinal))
                    {
                        return ResolveSpawnOwnerId(target, index);
                    }
                }

                for (int index = 0; index < targets.Count; index++)
                {
                    var target = targets[index];
                    if (!target.IsBot)
                        return ResolveSpawnOwnerId(target, index);
                }

                if (targets.Count > 0)
                    return ResolveSpawnOwnerId(targets[0], 0);
            }

            return "player_0";
        }

        private static string ResolveSpawnOwnerId(SpawnPositionAssignment assignment, int fallbackIndex)
        {
            if (!string.IsNullOrWhiteSpace(assignment.ParticipantId))
                return assignment.ParticipantId;

            if (assignment.IsBot)
                return $"bot-{assignment.SlotIndex:00}";

            return fallbackIndex == 0 ? "player_0" : $"spawn-slot-{assignment.SlotIndex:00}";
        }

        private string ResolveBootstrapOwnerId()
        {
            var assignments = _startingPositionState.SpawnAssignments;
            if (assignments != null && assignments.Count > 0)
                return NormalizeOwnerId(ResolveLocalActiveOwnerId(assignments));

            if (!string.IsNullOrWhiteSpace(_sessionManager?.LocalPlayerId))
                return NormalizeOwnerId(_sessionManager.LocalPlayerId);

            return NormalizeOwnerId(_constructionService.GetActiveOwner());
        }

        private bool CanRunBootstrapLogic()
        {
            var participants = _sessionManager?.Participants;
            if (participants == null || participants.Count == 0)
                return true;

            return _startingPositionState.IsSet;
        }
    }

    internal sealed class BootstrapStarterPackState
    {
        private readonly HashSet<string> _ownersWithGrantedStarterPack = new HashSet<string>(StringComparer.Ordinal);

        public bool HasGranted(string ownerId)
            => _ownersWithGrantedStarterPack.Contains(NormalizeOwnerId(ownerId));

        public void MarkGranted(string ownerId)
            => _ownersWithGrantedStarterPack.Add(NormalizeOwnerId(ownerId));

        public string[] GetGrantedOwnersSnapshot()
        {
            var owners = new string[_ownersWithGrantedStarterPack.Count];
            _ownersWithGrantedStarterPack.CopyTo(owners);
            return owners;
        }

        public void RestoreGrantedOwners(IEnumerable<string> owners)
        {
            _ownersWithGrantedStarterPack.Clear();
            if (owners == null)
                return;

            foreach (string ownerId in owners)
            {
                if (string.IsNullOrWhiteSpace(ownerId))
                    continue;

                _ownersWithGrantedStarterPack.Add(NormalizeOwnerId(ownerId));
            }
        }

        private static string NormalizeOwnerId(string ownerId)
            => string.IsNullOrWhiteSpace(ownerId) ? "player_0" : ownerId.Trim();
    }

    internal sealed class BootstrapStarterPackSaveModule : ISaveModule
    {
        private const int SchemaVersion = 1;

        private readonly BootstrapStarterPackState _state;

        public BootstrapStarterPackSaveModule(BootstrapStarterPackState state)
        {
            _state = state;
        }

        public void OnSave(ISaveContext context)
        {
            var owners = _state.GetGrantedOwnersSnapshot();
            context.Writer.Write(SchemaVersion);
            context.Writer.Write(owners.Length);
            for (int index = 0; index < owners.Length; index++)
                context.Writer.Write(owners[index] ?? string.Empty);
        }

        public void OnLoad(ISaveContext context)
        {
            int version = context.Reader.ReadInt32();
            if (version != SchemaVersion)
            {
                Debug.LogWarning($"[Bootstrap] Непідтримувана версія starter-pack блоку: {version}.");
                return;
            }

            int ownerCount = context.Reader.ReadInt32();
            var owners = new List<string>(ownerCount);
            for (int index = 0; index < ownerCount; index++)
            {
                string ownerId = context.Reader.ReadString();
                if (!string.IsNullOrWhiteSpace(ownerId))
                    owners.Add(ownerId);
            }

            _state.RestoreGrantedOwners(owners);
        }
    }
}
