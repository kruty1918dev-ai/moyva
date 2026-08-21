using System;
using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    internal sealed class BotDiagnosticsRecoveryObserver :
        IInitializable,
        IDisposable,
        ITickable
    {
        private readonly DiContainer _container;
        private readonly SignalBus _signalBus;
        private readonly ITurnService _turns;
        private readonly IBotDiagnosticsLogger _log;

        private bool _spawnTopologyOk;
        private bool _bindingsOk;
        private bool _botTurnObserved;
        private bool _botActionObserved;
        private bool _handoffObserved;
        private string _activeBotOwner = string.Empty;
        private long _activeBotTurn;
        private int _lastActions;
        private string _lastSummary = string.Empty;
        private double _nextSummary;

        public BotDiagnosticsRecoveryObserver(
            DiContainer container,
            SignalBus signalBus,
            IBotDiagnosticsLogger log,
            [InjectOptional] ITurnService turns = null)
        {
            _container = container;
            _signalBus = signalBus;
            _log = log;
            _turns = turns;
        }

        public void Initialize()
        {
            _signalBus?.Subscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);

            _bindingsOk = BotRuntimeBindings.IsCoreReady(_container);
            if (_bindingsOk)
            {
                _log.Info(
                    BotDiagnosticCategory.Health,
                    "RECOVERY.BINDINGS_OK",
                    "Canonical BotAI runtime bindings are available from Gameplay bootstrap.",
                    details:
                        "executor=true; snapshot=true; turnPlanner=true; actionExecutor=true; strategy=true; goals=true; decisionTrace=true");
            }
            else
            {
                _log.Error(
                    BotDiagnosticCategory.Health,
                    "RECOVERY.BINDINGS_FAILED",
                    "Canonical BotAI bindings are still incomplete after recovery installation.",
                    reason:
                        "BotRuntimeBindings.Install should have run from BootstrapInstaller before root resolution.");
            }

            if (GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest)
            {
                if (GameLaunchContext.MaxPlayers >= 2)
                {
                    _log.Info(
                        BotDiagnosticCategory.Session,
                        "RECOVERY.DIRECT_SESSION_OK",
                        "Direct Gameplay is configured for Human + Bot.",
                        details: $"maxPlayers={GameLaunchContext.MaxPlayers}");
                }
                else
                {
                    _log.Error(
                        BotDiagnosticCategory.Session,
                        "RECOVERY.DIRECT_SESSION_FAILED",
                        "Direct Gameplay still has fewer than two launch slots.",
                        details: $"maxPlayers={GameLaunchContext.MaxPlayers}");
                }
            }
        }

        public void Dispose()
            => _signalBus?.TryUnsubscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);

        public void Tick()
        {
            if (_turns == null)
                return;

            if (_turns.IsActiveFactionBot &&
                _turns.Phase == TurnPhase.AwaitingInput)
            {
                string owner = _turns.ActiveOwnerId ?? string.Empty;

                if (!_botTurnObserved ||
                    _activeBotTurn != _turns.GlobalTurn ||
                    !string.Equals(_activeBotOwner, owner, StringComparison.Ordinal))
                {
                    _botTurnObserved = true;
                    _activeBotOwner = owner;
                    _activeBotTurn = _turns.GlobalTurn;
                    _lastActions = _turns.ActionsThisTurn;

                    _log.Info(
                        BotDiagnosticCategory.Turn,
                        "RECOVERY.BOT_TURN_ENTERED",
                        "Authoritative turn handoff reached the bot.",
                        details:
                            $"owner='{owner}'; round={_turns.Round}; globalTurn={_turns.GlobalTurn}; actions={_turns.ActionsThisTurn}",
                        ownerId: owner);
                }

                if (_turns.ActionsThisTurn > _lastActions)
                {
                    _botActionObserved = true;
                    _lastActions = _turns.ActionsThisTurn;

                    _log.Info(
                        BotDiagnosticCategory.Executor,
                        "RECOVERY.BOT_ACTION_RECORDED",
                        "The active bot recorded an authoritative gameplay action.",
                        details:
                            $"owner='{owner}'; globalTurn={_turns.GlobalTurn}; actionsThisTurn={_turns.ActionsThisTurn}",
                        ownerId: owner);
                }
            }
            else if (_botTurnObserved &&
                     !_handoffObserved &&
                     _turns.Phase == TurnPhase.AwaitingInput &&
                     !string.IsNullOrWhiteSpace(_turns.ActiveOwnerId))
            {
                _handoffObserved = true;

                _log.Info(
                    BotDiagnosticCategory.Handoff,
                    "RECOVERY.HANDOFF_OK",
                    "Bot turn completed and control returned to a non-bot faction.",
                    details:
                        $"previousBot='{_activeBotOwner}'; currentOwner='{_turns.ActiveOwnerId}'; " +
                        $"round={_turns.Round}; globalTurn={_turns.GlobalTurn}; botActionObserved={_botActionObserved}",
                    ownerId: _turns.ActiveOwnerId);
            }

            double now = Time.realtimeSinceStartupAsDouble;
            if (now < _nextSummary)
                return;

            _nextSummary = now + 1.0d;
            string summary =
                $"spawn={_spawnTopologyOk};bindings={_bindingsOk};" +
                $"botTurn={_botTurnObserved};botAction={_botActionObserved};handoff={_handoffObserved};" +
                $"factions={_turns.Factions?.Count ?? 0};active='{_turns.ActiveOwnerId}';bot={_turns.IsActiveFactionBot}";

            if (string.Equals(summary, _lastSummary, StringComparison.Ordinal))
                return;

            _lastSummary = summary;

            _log.Trace(
                BotDiagnosticCategory.Health,
                "RECOVERY.STATE",
                "Player-vs-Bot recovery state changed.",
                details: summary);
        }

        private void OnSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            int count = signal.Assignments?.Length ?? 0;
            int humanCount = 0;
            int botCount = 0;
            string botOwner = string.Empty;

            if (signal.Assignments != null)
            {
                for (int i = 0; i < signal.Assignments.Length; i++)
                {
                    SpawnPositionAssignment assignment = signal.Assignments[i];
                    if (assignment.IsBot)
                    {
                        botCount++;
                        if (string.IsNullOrWhiteSpace(botOwner))
                            botOwner = assignment.ParticipantId;
                    }
                    else
                    {
                        humanCount++;
                    }
                }
            }

            _spawnTopologyOk =
                count >= 2 &&
                humanCount >= 1 &&
                botCount >= 1;

            if (_spawnTopologyOk)
            {
                _log.Info(
                    BotDiagnosticCategory.Spawn,
                    "RECOVERY.SPAWN_TOPOLOGY_OK",
                    "Direct Gameplay published Human + Bot spawn assignments.",
                    details:
                        $"count={count}; humans={humanCount}; bots={botCount}; firstBot='{botOwner}'; source={signal.Source}");
            }
            else
            {
                _log.Error(
                    BotDiagnosticCategory.Spawn,
                    "RECOVERY.SPAWN_TOPOLOGY_FAILED",
                    "Spawn topology still does not contain both a human and a bot.",
                    reason:
                        "StartingPositionPolicy and StartingPositionAssignmentFactory must both honor DirectGameplayTest MaxPlayers.",
                    details:
                        $"count={count}; humans={humanCount}; bots={botCount}; source={signal.Source}");
            }
        }
    }
}
