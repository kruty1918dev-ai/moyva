using System;
using System.Collections.Generic;
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
        private bool _hasPendingWorldGeneratedSignal;
        private bool _bootstrapApplied;
        private bool _starterPackGrantEnabled;
        private readonly HashSet<string> _ownersWithGrantedStarterPack = new HashSet<string>(StringComparer.Ordinal);

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
            BootstrapStartingPositionState startingPositionState)
        {
            _constructionService   = constructionService;
            _signalBus             = signalBus;
            _settings              = settings;
            _saveService           = saveService;
            _startingPositionState = startingPositionState;
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

            // Якщо є сучасне збереження економіки — не робимо bootstrap.
            // Старі сейви не мають EconomySaveModule, тому їм потрібна одноразова міграція стартових ресурсів.
            int slot = GameLaunchContext.SaveSlot;
            if (GameLaunchContext.IsAutoLoadEnabled() && _saveService.HasSave(slot))
            {
                bool hasEconomySave = _saveInspectorService != null
                    && _saveInspectorService.HasBlock(slot, EconomySaveModuleFullName);

                if (hasEconomySave)
                {
                    _starterPackGrantEnabled = false;
                    _bootstrapApplied = true;
                    return;
                }

                _starterPackGrantEnabled = true;
                Debug.LogWarning($"[Bootstrap] Save slot {slot} не має economy-блоку. Виконується міграційна видача стартових ресурсів.");
            }
            else
            {
                _starterPackGrantEnabled = ShouldGrantStarterPackForCurrentLaunch();
            }

            string activeOwnerId = ResolveBootstrapOwnerId();
            _constructionService.SetActiveOwner(activeOwnerId);

            if (_starterPackGrantEnabled && !_ownersWithGrantedStarterPack.Contains(activeOwnerId))
            {
                TryGrantStarterPack(string.Empty, activeOwnerId);

                // Після видачі — робимо автосейв і перевіряємо, чи з'явився economy-блок у файлі.
                try
                {
                    _saveService?.Save(slot);
                    Debug.Log($"[Bootstrap] Автосейв після видачі стартових ресурсів у слот {slot}.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Bootstrap] Не вдалося зберегти після видачі стартових ресурсів: {ex}");
                }

                bool hasEconomySave = _saveInspectorService != null && _saveInspectorService.HasBlock(slot, EconomySaveModuleFullName);
                if (hasEconomySave)
                {
                    _ownersWithGrantedStarterPack.Add(activeOwnerId);
                }
                else
                {
                    Debug.LogWarning($"[Bootstrap] Після автосейву не знайдено economy-блоку у слоті {slot}. Відкладено маркування owner '{activeOwnerId}' як granted; повторна спроба відбудеться при створенні поселення.");
                }
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
            if (_ownersWithGrantedStarterPack.Contains(ownerId))
                return;

            if (!TryGrantStarterPack(signal.SettlementId, ownerId))
                return;

            int slot = GameLaunchContext.SaveSlot;
            try
            {
                _saveService?.Save(slot);
                Debug.Log($"[Bootstrap] Автосейв після видачі стартових ресурсів (поселення) у слот {slot}.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Bootstrap] Не вдалося зберегти після видачі стартових ресурсів (поселення): {ex}");
            }

            bool hasEconomySave = _saveInspectorService != null && _saveInspectorService.HasBlock(slot, EconomySaveModuleFullName);
            if (hasEconomySave)
            {
                _ownersWithGrantedStarterPack.Add(ownerId);
            }
            else
            {
                Debug.LogWarning($"[Bootstrap] Після автосейву не знайдено economy-блоку у слоті {slot}. Owner '{ownerId}' не буде марковано як granted.");
            }
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
}
