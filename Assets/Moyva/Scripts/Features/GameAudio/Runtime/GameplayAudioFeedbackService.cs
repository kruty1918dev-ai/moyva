using System;
using System.Collections.Generic;
using Kruty1918.Audio;
using Kruty1918.Moyva.GameAudio.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Доменні події → звукові ключі з audio-feedback пресету.
    /// Не містить рішень про гру — тільки мапа подія→звук, редагується у JSON.
    /// Правила з context ("unit:archer", "tile:water") мають пріоритет над базовим eventName.
    /// </summary>
    public sealed class GameplayAudioFeedbackService : IInitializable, IDisposable
    {
        private readonly IAudioService _audio;
        private readonly AudioFeedbackConfig _config;
        private readonly IGridProjection _projection;
        private readonly IGridService _grid;
        private readonly SignalBus _signalBus;
        private readonly IUnitCombatService _combat;
        private readonly IUnitService _units;
        private readonly ITurnService _turns;

        private readonly Dictionary<string, AudioFeedbackEventRule> _rules =
            new Dictionary<string, AudioFeedbackEventRule>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Створює сервіс аудіо-фідбека із конфігурацією та джерелами подій.</summary>
        public GameplayAudioFeedbackService(
            [InjectOptional] IAudioService audio,
            [InjectOptional] AudioFeedbackConfig config,
            [InjectOptional] IGridProjection projection,
            [InjectOptional] IGridService grid,
            [InjectOptional] SignalBus signalBus,
            [InjectOptional] IUnitCombatService combat,
            [InjectOptional] IUnitService units,
            [InjectOptional] ITurnService turns)
        {
            _audio = audio;
            _config = config;
            _projection = projection;
            _grid = grid;
            _signalBus = signalBus;
            _combat = combat;
            _units = units;
            _turns = turns;
        }

        /// <summary>Підписує сервіс на gameplay-події.</summary>
        public void Initialize()
        {
            if (_config?.eventSounds != null)
            {
                foreach (var rule in _config.eventSounds)
                {
                    if (rule == null || string.IsNullOrWhiteSpace(rule.eventName)
                        || string.IsNullOrWhiteSpace(rule.soundKey))
                        continue;
                    _rules[RuleKey(rule)] = rule;
                }
            }

            if (_signalBus != null)
            {
                _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
                _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
                _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
                _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
                _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
                _signalBus.Subscribe<SettlementCreatedSignal>(OnSettlementCreated);
                _signalBus.Subscribe<SettlementDeactivatedSignal>(OnSettlementDeactivated);
                _signalBus.Subscribe<ResourceDeficitSignal>(OnResourceDeficit);
                _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
                _signalBus.Subscribe<GameEndedSignal>(OnGameEnded);
                _signalBus.Subscribe<GamePausedSignal>(OnGamePaused);
                _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);

                _signalBus.Subscribe<MoveUnitRequestSignal>(OnMoveUnitRequested);
                _signalBus.Subscribe<MoveGroupRequestSignal>(OnMoveGroupRequested);
                _signalBus.Subscribe<UnitMoveRejectedSignal>(OnUnitMoveRejected);
                _signalBus.Subscribe<UnitRecruitmentReadySignal>(OnRecruitmentReady);
                _signalBus.Subscribe<UnitRecruitmentCommandRejectedSignal>(OnCommandRejected);
                _signalBus.Subscribe<BuildingOperationalSignal>(OnBuildingOperational);
                _signalBus.Subscribe<BuildingCancelledSignal>(OnBuildingCancelled);
                _signalBus.Subscribe<SettlementCapturedSignal>(OnSettlementCaptured);
                _signalBus.Subscribe<FactionEliminatedSignal>(OnFactionEliminated);
                _signalBus.Subscribe<UnitGarrisonStateChangedSignal>(OnGarrisonStateChanged);
                _signalBus.Subscribe<CaravanDeliveryCompletedSignal>(OnCaravanDelivered);
                _signalBus.Subscribe<ConstructionSupplyReadySignal>(OnSupplyReady);
                _signalBus.Subscribe<SaveCompletedSignal>(OnSaveCompleted);
                _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
                _signalBus.Subscribe<WorldInfoPanelClosedSignal>(OnPanelClosed);
                _signalBus.Subscribe<BuildingInfoPanelClosedSignal>(OnPanelClosed);
            }

            if (_combat != null)
            {
                _combat.AttackStarted += OnAttackStarted;
                _combat.AttackResolved += OnAttackResolved;
            }

            if (_turns != null)
                _turns.StateChanged += OnTurnStateChanged;
        }

        /// <summary>Відписує сервіс.</summary>
        public void Dispose()
        {
            if (_signalBus != null)
            {
                _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
                _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
                _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
                _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
                _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
                _signalBus.TryUnsubscribe<SettlementCreatedSignal>(OnSettlementCreated);
                _signalBus.TryUnsubscribe<SettlementDeactivatedSignal>(OnSettlementDeactivated);
                _signalBus.TryUnsubscribe<ResourceDeficitSignal>(OnResourceDeficit);
                _signalBus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
                _signalBus.TryUnsubscribe<GameEndedSignal>(OnGameEnded);
                _signalBus.TryUnsubscribe<GamePausedSignal>(OnGamePaused);
                _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);

                _signalBus.TryUnsubscribe<MoveUnitRequestSignal>(OnMoveUnitRequested);
                _signalBus.TryUnsubscribe<MoveGroupRequestSignal>(OnMoveGroupRequested);
                _signalBus.TryUnsubscribe<UnitMoveRejectedSignal>(OnUnitMoveRejected);
                _signalBus.TryUnsubscribe<UnitRecruitmentReadySignal>(OnRecruitmentReady);
                _signalBus.TryUnsubscribe<UnitRecruitmentCommandRejectedSignal>(OnCommandRejected);
                _signalBus.TryUnsubscribe<BuildingOperationalSignal>(OnBuildingOperational);
                _signalBus.TryUnsubscribe<BuildingCancelledSignal>(OnBuildingCancelled);
                _signalBus.TryUnsubscribe<SettlementCapturedSignal>(OnSettlementCaptured);
                _signalBus.TryUnsubscribe<FactionEliminatedSignal>(OnFactionEliminated);
                _signalBus.TryUnsubscribe<UnitGarrisonStateChangedSignal>(OnGarrisonStateChanged);
                _signalBus.TryUnsubscribe<CaravanDeliveryCompletedSignal>(OnCaravanDelivered);
                _signalBus.TryUnsubscribe<ConstructionSupplyReadySignal>(OnSupplyReady);
                _signalBus.TryUnsubscribe<SaveCompletedSignal>(OnSaveCompleted);
                _signalBus.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
                _signalBus.TryUnsubscribe<WorldInfoPanelClosedSignal>(OnPanelClosed);
                _signalBus.TryUnsubscribe<BuildingInfoPanelClosedSignal>(OnPanelClosed);
            }

            if (_combat != null)
            {
                _combat.AttackStarted -= OnAttackStarted;
                _combat.AttackResolved -= OnAttackResolved;
            }

            if (_turns != null)
                _turns.StateChanged -= OnTurnStateChanged;
        }

        private void OnUnitMoved(UnitMovedSignal evt)
            => Play("unit-moved", ToWorld(evt.NewPosition),
                UnitContext(_units?.GetUnitTypeId(evt.UnitId)),
                TileContext(TileTypeAt(evt.NewPosition)));

        private void OnUnitCreated(UnitCreatedSignal evt)
            => Play("unit-created", evt.Position);

        private void OnUnitDestroyed(UnitDestroyedSignal evt)
            => Play("unit-destroyed", UnitPositionOrNull(evt.UnitId));

        private void OnBuildingPlaced(BuildingPlacedSignal evt)
            => Play("building-placed", evt.Position);

        private void OnBuildingDemolished(BuildingDemolishedSignal evt)
            => Play("building-demolished", evt.Position);

        private void OnSettlementCreated(SettlementCreatedSignal evt)
            => Play("settlement-created", evt.TownHallPosition);

        private void OnSettlementDeactivated(SettlementDeactivatedSignal evt)
            => Play("settlement-deactivated", (Vector3?)null);

        private void OnResourceDeficit(ResourceDeficitSignal evt)
            => Play("resource-deficit", (Vector3?)null);

        private void OnGameStarted(GameStartedSignal evt)
            => Play("game-started", (Vector3?)null);

        private void OnGameEnded(GameEndedSignal evt)
            => Play("game-ended", (Vector3?)null);

        private void OnGamePaused(GamePausedSignal evt)
            => Play(evt.IsPaused ? "game-paused" : "game-resumed", (Vector3?)null);

        private void OnGameModeChanged(GameModeChangedSignal evt)
        {
            if (evt.NewMode == GameModeType.Construction)
                Play("mode-construction", (Vector3?)null);
        }

        private void OnMoveUnitRequested(MoveUnitRequestSignal signal)
            => Play("unit-command", UnitPositionOrNull(signal.UnitId));

        private void OnMoveGroupRequested(MoveGroupRequestSignal signal)
            => Play("unit-command", ToWorld(signal.TargetPosition));

        private void OnUnitMoveRejected(UnitMoveRejectedSignal signal)
            => Play("unit-move-rejected", ToWorld(signal.TargetPosition));

        private void OnRecruitmentReady(UnitRecruitmentReadySignal signal)
            => Play("recruitment-ready", ToWorld(signal.BuildingPosition));

        private void OnCommandRejected(UnitRecruitmentCommandRejectedSignal signal)
            => Play("command-rejected", (Vector3?)null);

        private void OnBuildingOperational(BuildingOperationalSignal signal)
            => Play("building-operational", ToWorld(signal.Position));

        private void OnBuildingCancelled(BuildingCancelledSignal signal)
            => Play("construction-cancelled", (Vector3?)null);

        private void OnSettlementCaptured(SettlementCapturedSignal signal)
            => Play("settlement-captured", ToWorld(signal.CenterPosition));

        private void OnFactionEliminated(FactionEliminatedSignal signal)
            => Play("faction-eliminated", (Vector3?)null);

        private void OnGarrisonStateChanged(UnitGarrisonStateChangedSignal signal)
            => Play(signal.IsGarrisoned ? "unit-garrison-enter" : "unit-garrison-exit",
                ToWorld(signal.BuildingPosition));

        private void OnCaravanDelivered(CaravanDeliveryCompletedSignal signal)
            => Play("caravan-delivered", ToWorld(signal.WarehousePosition));

        private void OnSupplyReady(ConstructionSupplyReadySignal signal)
            => Play("supply-ready", ToWorld(signal.Position));

        private void OnSaveCompleted(SaveCompletedSignal signal)
        {
            if (signal.Success)
                Play("game-saved", (Vector3?)null);
        }

        private void OnWorldInfoSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            if (signal.Kind != WorldInfoSelectionKind.None)
                Play("selection-changed", (Vector3?)null);
        }

        private void OnPanelClosed(WorldInfoPanelClosedSignal signal)
            => Play("panel-closed", (Vector3?)null);

        private void OnPanelClosed(BuildingInfoPanelClosedSignal signal)
            => Play("panel-closed", (Vector3?)null);

        private void OnAttackStarted(string attackerId, string defenderId)
            => Play("combat-attack", UnitPositionOrNull(attackerId),
                UnitContext(_units?.GetUnitTypeId(attackerId)));

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

        private string TileTypeAt(Vector2Int gridPos)
            => _grid != null && _grid.TryGetTileData(gridPos, out string tileTypeId)
                ? tileTypeId
                : null;

        private static string UnitContext(string unitTypeId)
            => string.IsNullOrWhiteSpace(unitTypeId) ? null : "unit:" + unitTypeId;

        private static string TileContext(string tileTypeId)
            => string.IsNullOrWhiteSpace(tileTypeId) ? null : "tile:" + tileTypeId;

        private Vector3? ToWorld(Vector2Int gridPos)
            => _projection != null ? _projection.GridToWorld(gridPos) : (Vector3?)null;

        private void Play(string eventName, Vector2Int? gridPos)
            => Play(eventName, gridPos.HasValue ? ToWorld(gridPos.Value) : null);

        private void Play(string eventName, Vector3? position, params string[] contexts)
        {
            if (_audio == null)
                return;

            AudioFeedbackEventRule rule = FindRule(eventName, contexts);
            if (rule == null || string.IsNullOrWhiteSpace(rule.soundKey))
                return;

            if (rule.atPosition && position.HasValue)
            {
                _audio.PlayAt(rule.soundKey, position.Value, rule.volumeScale);
                return;
            }

            _audio.Play(rule.soundKey,
                new AudioPlayOptions(volumeScale: rule.volumeScale));
        }

        private AudioFeedbackEventRule FindRule(string eventName, string[] contexts)
        {
            if (contexts != null)
            {
                foreach (string context in contexts)
                {
                    if (string.IsNullOrEmpty(context))
                        continue;
                    if (_rules.TryGetValue(eventName + "|" + context, out var rule))
                        return rule;
                }
            }

            return _rules.TryGetValue(eventName, out var fallback) ? fallback : null;
        }

        private static string RuleKey(AudioFeedbackEventRule rule)
            => string.IsNullOrWhiteSpace(rule.context)
                ? rule.eventName.Trim()
                : rule.eventName.Trim() + "|" + rule.context.Trim();
    }
}
