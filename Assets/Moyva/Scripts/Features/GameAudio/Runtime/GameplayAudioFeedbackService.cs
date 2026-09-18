using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.GameAudio.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals.DomainEvents;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Доменні події → звукові ключі з audio-feedback пресету.
    /// Не містить рішень про гру — тільки мапа подія→звук, редагується у JSON.
    /// </summary>
    public sealed class GameplayAudioFeedbackService : IInitializable, IDisposable
    {
        private readonly IAudioService _audio;
        private readonly AudioFeedbackConfig _config;
        private readonly IGridProjection _projection;
        private readonly SignalBus _signalBus;
        private readonly IUnitCombatService _combat;
        private readonly IUnitService _units;
        private readonly ITurnService _turns;

        private readonly Dictionary<string, AudioFeedbackEventRule> _rules =
            new Dictionary<string, AudioFeedbackEventRule>(StringComparer.OrdinalIgnoreCase);

        public GameplayAudioFeedbackService(
            [InjectOptional] IAudioService audio,
            [InjectOptional] AudioFeedbackConfig config,
            [InjectOptional] IGridProjection projection,
            [InjectOptional] SignalBus signalBus,
            [InjectOptional] IUnitCombatService combat,
            [InjectOptional] IUnitService units,
            [InjectOptional] ITurnService turns)
        {
            _audio = audio;
            _config = config;
            _projection = projection;
            _signalBus = signalBus;
            _combat = combat;
            _units = units;
            _turns = turns;
        }

        public void Initialize()
        {
            if (_config?.eventSounds != null)
            {
                foreach (var rule in _config.eventSounds)
                {
                    if (rule == null || string.IsNullOrWhiteSpace(rule.eventName)
                        || string.IsNullOrWhiteSpace(rule.soundKey))
                        continue;
                    _rules[rule.eventName.Trim()] = rule;
                }
            }

            if (_signalBus != null)
            {
                _signalBus.Subscribe<UnitMovedDomainEvent>(OnUnitMoved);
                _signalBus.Subscribe<UnitCreatedDomainEvent>(OnUnitCreated);
                _signalBus.Subscribe<UnitDestroyedDomainEvent>(OnUnitDestroyed);
                _signalBus.Subscribe<BuildingPlacedDomainEvent>(OnBuildingPlaced);
                _signalBus.Subscribe<BuildingDemolishedDomainEvent>(OnBuildingDemolished);
                _signalBus.Subscribe<SettlementCreatedDomainEvent>(OnSettlementCreated);
                _signalBus.Subscribe<ResourceDeficitDomainEvent>(OnResourceDeficit);
                _signalBus.Subscribe<GameStartedDomainEvent>(OnGameStarted);
                _signalBus.Subscribe<GameEndedDomainEvent>(OnGameEnded);
                _signalBus.Subscribe<GamePausedDomainEvent>(OnGamePaused);
            }

            if (_combat != null)
            {
                _combat.AttackStarted += OnAttackStarted;
                _combat.AttackResolved += OnAttackResolved;
            }

            if (_turns != null)
                _turns.StateChanged += OnTurnStateChanged;
        }

        public void Dispose()
        {
            if (_signalBus != null)
            {
                _signalBus.TryUnsubscribe<UnitMovedDomainEvent>(OnUnitMoved);
                _signalBus.TryUnsubscribe<UnitCreatedDomainEvent>(OnUnitCreated);
                _signalBus.TryUnsubscribe<UnitDestroyedDomainEvent>(OnUnitDestroyed);
                _signalBus.TryUnsubscribe<BuildingPlacedDomainEvent>(OnBuildingPlaced);
                _signalBus.TryUnsubscribe<BuildingDemolishedDomainEvent>(OnBuildingDemolished);
                _signalBus.TryUnsubscribe<SettlementCreatedDomainEvent>(OnSettlementCreated);
                _signalBus.TryUnsubscribe<ResourceDeficitDomainEvent>(OnResourceDeficit);
                _signalBus.TryUnsubscribe<GameStartedDomainEvent>(OnGameStarted);
                _signalBus.TryUnsubscribe<GameEndedDomainEvent>(OnGameEnded);
                _signalBus.TryUnsubscribe<GamePausedDomainEvent>(OnGamePaused);
            }

            if (_combat != null)
            {
                _combat.AttackStarted -= OnAttackStarted;
                _combat.AttackResolved -= OnAttackResolved;
            }

            if (_turns != null)
                _turns.StateChanged -= OnTurnStateChanged;
        }

        private void OnUnitMoved(UnitMovedDomainEvent evt)
            => Play("unit-moved", evt.NewPosition);

        private void OnUnitCreated(UnitCreatedDomainEvent evt)
            => Play("unit-created", evt.Position);

        private void OnUnitDestroyed(UnitDestroyedDomainEvent evt)
            => Play("unit-destroyed", UnitPositionOrNull(evt.UnitId));

        private void OnBuildingPlaced(BuildingPlacedDomainEvent evt)
            => Play("building-placed", evt.Position);

        private void OnBuildingDemolished(BuildingDemolishedDomainEvent evt)
            => Play("building-demolished", evt.Position);

        private void OnSettlementCreated(SettlementCreatedDomainEvent evt)
            => Play("settlement-created", evt.TownHallPosition);

        private void OnResourceDeficit(ResourceDeficitDomainEvent evt)
            => Play("resource-deficit", (Vector3?)null);

        private void OnGameStarted(GameStartedDomainEvent evt)
            => Play("game-started", (Vector3?)null);

        private void OnGameEnded(GameEndedDomainEvent evt)
            => Play("game-ended", (Vector3?)null);

        private void OnGamePaused(GamePausedDomainEvent evt)
            => Play(evt.IsPaused ? "game-paused" : "game-resumed", (Vector3?)null);

        private void OnAttackStarted(string attackerId, string defenderId)
            => Play("combat-attack", UnitPositionOrNull(attackerId));

        private void OnAttackResolved(UnitAttackResult result)
        {
            Vector3? pos = UnitPositionOrNull(result.TargetUnitId);
            if (!result.Succeeded)
            {
                Play("combat-miss", pos);
                return;
            }

            Play("combat-hit", pos);
            if (result.TargetDied)
                Play("combat-kill", pos);
        }

        private void OnTurnStateChanged()
            => Play("turn-changed", (Vector3?)null);

        private Vector3? UnitPositionOrNull(string unitId)
        {
            if (_units != null && !string.IsNullOrEmpty(unitId)
                && _units.TryGetUnitPosition(unitId, out Vector2Int pos))
                return ToWorld(pos);

            return null;
        }

        private Vector3? ToWorld(Vector2Int gridPos)
            => _projection != null ? _projection.GridToWorld(gridPos) : (Vector3?)null;

        private void Play(string eventName, Vector2Int? gridPos)
            => Play(eventName, gridPos.HasValue ? ToWorld(gridPos.Value) : null);

        private void Play(string eventName, Vector3? position)
        {
            if (_audio == null
                || !_rules.TryGetValue(eventName, out var rule)
                || string.IsNullOrWhiteSpace(rule.soundKey))
                return;

            if (rule.atPosition && position.HasValue)
            {
                _audio.PlayAt(rule.soundKey, position.Value, rule.volumeScale);
                return;
            }

            _audio.Play(rule.soundKey,
                new AudioPlayOptions(volumeScale: rule.volumeScale));
        }
    }
}
