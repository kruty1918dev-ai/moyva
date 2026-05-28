using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
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
        private const string StarterPackLogTag = "[Bootstrap][StarterPack]";

        private readonly IConstructionService _constructionService;
        private readonly SignalBus _signalBus;
        private readonly ISaveService _saveService;
        private readonly BootstrapStarterPackState _starterPackState;
        private readonly IBootstrapOwnerIdResolver _ownerIdResolver;
        private readonly IBootstrapStarterPackDecisionService _decisionService;
        private readonly IBootstrapStarterPackPersistenceService _persistenceService;
        private readonly IBootstrapStarterPackGrantService _grantService;
        private bool _hasPendingWorldGeneratedSignal;
        private bool _bootstrapApplied;
        private bool _starterPackGrantEnabled;

        [Inject]
        public BootstrapGameInitializer(
            IConstructionService constructionService,
            SignalBus signalBus,
            ISaveService saveService,
            BootstrapStarterPackState starterPackState,
            IBootstrapOwnerIdResolver ownerIdResolver,
            IBootstrapStarterPackDecisionService decisionService,
            IBootstrapStarterPackPersistenceService persistenceService,
            IBootstrapStarterPackGrantService grantService)
        {
            _constructionService   = constructionService;
            _signalBus             = signalBus;
            _saveService           = saveService;
            _starterPackState      = starterPackState;
            _ownerIdResolver       = ownerIdResolver;
            _decisionService       = decisionService;
            _persistenceService    = persistenceService;
            _grantService          = grantService;
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

            string activeOwnerId = _ownerIdResolver.ResolveActiveOwnerId();
            _constructionService.SetActiveOwner(activeOwnerId);

            if (_decisionService.IsMultiplayerClient())
            {
                _starterPackGrantEnabled = false;
                _bootstrapApplied = true;
                Debug.Log("[Bootstrap] Multiplayer client detected — skipping starter pack grant; awaiting host snapshot.");
                return;
            }

            int slot = GameLaunchContext.SaveSlot;
            bool autoLoadEnabled = GameLaunchContext.IsAutoLoadEnabled();
            bool hasSave = _saveService.HasSave(slot);
            LogStarterPackBootstrapEvaluation(activeOwnerId, slot, autoLoadEnabled, hasSave);

            if (autoLoadEnabled && hasSave)
            {
                if (_starterPackState.HasGranted(activeOwnerId))
                {
                    Debug.Log($"{StarterPackLogTag} Skip grant: owner '{activeOwnerId}' already has granted marker in slot {slot}.");
                    _starterPackGrantEnabled = false;
                    _bootstrapApplied = true;
                    return;
                }

                if (_persistenceService.HasPersistedEconomyBlock(slot))
                {
                    Debug.Log($"{StarterPackLogTag} Skip grant: save slot {slot} already contains economy block for owner '{activeOwnerId}'.");
                    _starterPackState.MarkGranted(activeOwnerId);
                    _starterPackGrantEnabled = false;
                    _bootstrapApplied = true;
                    return;
                }

                _starterPackGrantEnabled = true;
                Debug.LogWarning($"[Bootstrap] Save slot {slot} не містить economy-блоку. Виконується міграційна видача стартових ресурсів для owner '{activeOwnerId}'.");
                Debug.Log($"{StarterPackLogTag} Migration grant enabled for slot {slot}, owner '{activeOwnerId}', entries=[{_grantService.DescribeConfiguredEntries()}].");
            }
            else
            {
                _starterPackGrantEnabled = _decisionService.ShouldGrantForCurrentLaunch();
                Debug.Log($"{StarterPackLogTag} New-world decision: grantEnabled={_starterPackGrantEnabled}, mode={GameLaunchContext.Mode}, slot={slot}, owner='{activeOwnerId}', entries=[{_grantService.DescribeConfiguredEntries()}].");
            }

            if (_starterPackGrantEnabled && !_starterPackState.HasGranted(activeOwnerId))
            {
                _grantService.TryGrant(string.Empty, activeOwnerId);
                _persistenceService.TryPersistStarterGrant(slot, activeOwnerId, "після видачі стартових ресурсів", _grantService.HasStarterPackEntries());
            }

            if (!_ownerIdResolver.CanRunBootstrapLogic())
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

            if (!_grantService.TryGrant(signal.SettlementId, ownerId))
                return;

            int slot = GameLaunchContext.SaveSlot;
            _persistenceService.TryPersistStarterGrant(slot, ownerId, "після видачі стартових ресурсів (поселення)", _grantService.HasStarterPackEntries());
        }

        private void LogStarterPackBootstrapEvaluation(string ownerId, int slot, bool autoLoadEnabled, bool hasSave)
        {
            Debug.Log($"{StarterPackLogTag} Evaluate bootstrap: mode={GameLaunchContext.Mode}, slot={slot}, autoLoad={autoLoadEnabled}, autoSave={GameLaunchContext.IsAutoSaveEnabled()}, hasSave={hasSave}, owner='{ownerId}', alreadyGranted={_starterPackState.HasGranted(ownerId)}, entries=[{_grantService.DescribeConfiguredEntries()}].");
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId)
                ? "player_0"
                : ownerId.Trim();
        }

        // BootstrapGameInitializer intentionally stays as a signal coordinator.
        // Starter economy decisions, payload creation, and save validation live in
        // focused services so future economy/editor changes do not silently alter
        // the new-world starter resource contract.
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
