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
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        // ─────────────────────────────────────────────────────────────

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            // Якщо є збереження — не робимо bootstrap
            int slot = GameLaunchContext.SaveSlot;
            if (GameLaunchContext.IsAutoLoadEnabled() && _saveService.HasSave(slot))
                return;

            if (!CanRunBootstrapLogic())
                return;

            if (!string.IsNullOrEmpty(_settings.DefaultBuildingId))
            {
                // Центр ядра розкриття туману, встановлений StartingPositionInitializer (order 101).
                // Ми маємо order 102, тому значення вже є.
                var fallback = _startingPositionState.IsSet
                    ? _startingPositionState.StartPosition
                    : new Vector2Int(signal.Width / 2, signal.Height / 2);
                var targets = ResolveActiveSpawnAssignments(fallback);
                int placedCount = 0;

                for (int index = 0; index < targets.Count; index++)
                {
                    var target = targets[index];
                    string ownerId = ResolveSpawnOwnerId(target, index);
                    bool placed = TryPlaceWithSpiralSearch(_settings.DefaultBuildingId, target.Position, signal.Width, signal.Height, ownerId, out Vector2Int placedPosition);

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
            var result = new List<SpawnPositionAssignment>();
            var assignments = _startingPositionState.SpawnAssignments;
            bool hasAssignedParticipant = false;

            for (int index = 0; index < assignments.Count; index++)
            {
                var assignment = assignments[index];
                if (!string.IsNullOrEmpty(assignment.ParticipantId) || assignment.IsBot)
                {
                    result.Add(assignment);
                    hasAssignedParticipant = true;
                }
            }

            if (!hasAssignedParticipant)
            {
                if (assignments.Count > 0)
                    result.Add(assignments[0]);
                else
                    result.Add(new SpawnPositionAssignment { SlotIndex = 0, Position = fallback });
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

            return _sessionManager.IsLocalPlayerHost;
        }
    }
}
