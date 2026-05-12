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
    /// На старті нової гри розміщує дефолтну будівлю на стартовій позиції
    /// та видає стартові ресурси гравцю.
    ///
    /// При завантаженні збереження — не втручається.
    /// </summary>
    internal sealed class BootstrapGameInitializer : IInitializable, IDisposable
    {
        private const int MaxSearchRadius = 20;

        private readonly IConstructionService _constructionService;
        private readonly SignalBus _signalBus;
        private readonly BootstrapGameSettings _settings;
        private readonly ISaveService _saveService;
        private readonly BootstrapStartingPositionState _startingPositionState;
        private WorldGeneratedDataSignal _pendingWorldGeneratedSignal;
        private bool _hasPendingWorldGeneratedSignal;
        private bool _bootstrapApplied;
        private bool _starterPackGrantEnabled;
        private readonly HashSet<string> _ownersWithGrantedStarterPack = new HashSet<string>(StringComparer.Ordinal);

    #pragma warning disable CS0649
        [InjectOptional] private ISessionManager _sessionManager;
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
            _pendingWorldGeneratedSignal = signal;
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

            // Якщо є збереження — не робимо bootstrap
            int slot = GameLaunchContext.SaveSlot;
            if (GameLaunchContext.IsAutoLoadEnabled() && _saveService.HasSave(slot))
            {
                _starterPackGrantEnabled = false;
                _bootstrapApplied = true;
                return;
            }

            _starterPackGrantEnabled = ShouldGrantStarterPackForCurrentLaunch();

            if (!CanRunBootstrapLogic())
                return;

            if (!string.IsNullOrEmpty(_settings.DefaultBuildingId))
            {
                // Центр ядра розкриття туману, встановлений StartingPositionInitializer (order 101).
                // Ми маємо order 102, тому значення вже є.
                var fallback = _startingPositionState.IsSet
                    ? _startingPositionState.StartPosition
                    : new Vector2Int(_pendingWorldGeneratedSignal.Width / 2, _pendingWorldGeneratedSignal.Height / 2);
                var targets = ResolveActiveSpawnAssignments(fallback);
                int placedCount = 0;

                for (int index = 0; index < targets.Count; index++)
                {
                    var target = targets[index];
                    string ownerId = ResolveSpawnOwnerId(target, index);
                    bool placed = TryPlaceWithSpiralSearch(_settings.DefaultBuildingId, target.Position, _pendingWorldGeneratedSignal.Width, _pendingWorldGeneratedSignal.Height, ownerId, out Vector2Int placedPosition);

                    if (placed)
                    {
                        placedCount++;
                        Debug.Log($"[Bootstrap] '{_settings.DefaultBuildingId}' розміщено для slot {target.SlotIndex} ({ownerId}) на {placedPosition} (центр {target.Position})");
                    }
                    else
                    {
                        Debug.LogWarning($"[Bootstrap] Не вдалось розмістити '{_settings.DefaultBuildingId}' для slot {target.SlotIndex} в радіусі {MaxSearchRadius} від {target.Position}");
                    }
                }

                if (targets.Count > 1)
                    Debug.Log($"[Bootstrap] Стартові будівлі розміщено: {placedCount}/{targets.Count}.");
            }
            else
            {
                Debug.LogWarning("[Bootstrap] DefaultBuildingId не установлено");
            }

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

            _ownersWithGrantedStarterPack.Add(ownerId);
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
                Debug.Log($"[Bootstrap] Видано стартовий пакет owner='{ownerId}' для settlement='{settlementId}'.");
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
            return GameLaunchContext.Mode == GameLaunchMode.MenuNewGame
                || GameLaunchContext.Mode == GameLaunchMode.MenuMultiplayerGame;
        }

        // ─── Спіральний пошук вільного тайлу ─────────────────────────────────

        private bool TryPlaceWithSpiralSearch(string buildingId, Vector2Int center, int mapWidth, int mapHeight, string ownerId, out Vector2Int placedPosition)
        {
            for (int radius = 0; radius <= MaxSearchRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        // Тільки оболонка поточного радіусу
                        if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                            continue;

                        var pos = new Vector2Int(center.x + dx, center.y + dy);
                        if (pos.x < 0 || pos.x >= mapWidth || pos.y < 0 || pos.y >= mapHeight)
                            continue;

                        if (_constructionService.TryDirectPlace(buildingId, pos, ownerId))
                        {
                            placedPosition = pos;
                            return true;
                        }
                    }
                }
            }
            placedPosition = default;
            return false;
        }

        private IReadOnlyList<SpawnPositionAssignment> ResolveActiveSpawnAssignments(Vector2Int fallback)
        {
            var assignments = _startingPositionState.SpawnAssignments;

            var result = new List<SpawnPositionAssignment>();
            if (assignments == null || assignments.Count == 0)
            {
                result.Add(new SpawnPositionAssignment { SlotIndex = 0, Position = fallback });
                return result;
            }

            string localPlayerId = _sessionManager?.LocalPlayerId;
            bool isHost = _sessionManager == null || _sessionManager.IsLocalPlayerHost;

            for (int index = 0; index < assignments.Count; index++)
            {
                var assignment = assignments[index];

                if (!string.IsNullOrEmpty(localPlayerId) && string.Equals(assignment.ParticipantId, localPlayerId, StringComparison.Ordinal))
                {
                    result.Add(assignment);
                    continue;
                }

                if (isHost && assignment.IsBot)
                {
                    result.Add(assignment);
                }
            }

            if (result.Count == 0)
            {
                for (int index = 0; index < assignments.Count; index++)
                {
                    if (!assignments[index].IsBot)
                    {
                        result.Add(assignments[index]);
                        break;
                    }
                }

                if (result.Count == 0)
                    result.Add(assignments[0]);
            }

            return result;
        }

        private static string ResolveSpawnOwnerId(SpawnPositionAssignment assignment, int fallbackIndex)
        {
            if (!string.IsNullOrWhiteSpace(assignment.ParticipantId))
                return assignment.ParticipantId;

            if (assignment.IsBot)
                return $"bot-{assignment.SlotIndex:00}";

            return fallbackIndex == 0 ? "bootstrap" : $"spawn-slot-{assignment.SlotIndex:00}";
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
