using System;
using System.Text;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    internal sealed class BotDiagnosticsTurnObserver : IInitializable, ITickable
    {
        private readonly ITurnService _turns;
        private readonly IBotDiagnosticsLogger _log;
        private readonly BotDiagnosticsSettings _settings;

        private bool _initializedState;
        private TurnPhase _lastPhase;
        private string _lastOwner = string.Empty;
        private long _lastGlobalTurn = long.MinValue;
        private int _lastRound = int.MinValue;
        private int _lastActions = int.MinValue;
        private bool _lastIsBot;
        private string _lastFactionSignature = string.Empty;
        private double _nextHeartbeat;

        public BotDiagnosticsTurnObserver(
            IBotDiagnosticsLogger log,
            BotDiagnosticsSettings settings,
            [InjectOptional] ITurnService turns = null)
        {
            _log = log;
            _settings = settings;
            _turns = turns;
        }

        public void Initialize()
        {
            if (_turns == null)
            {
                _log.Critical(
                    BotDiagnosticCategory.Turn,
                    "TURN.AUTHORITY_MISSING",
                    "Turn diagnostics cannot start because ITurnService is missing.");
                return;
            }

            LogFactionRegistry(force: true);
            CaptureAndLogState(force: true);
        }

        public void Tick()
        {
            if (_turns == null)
                return;

            LogFactionRegistry(force: false);
            CaptureAndLogState(force: false);

            if (!_settings.LogBotHeartbeat ||
                !_turns.IsActiveFactionBot ||
                _turns.Phase != TurnPhase.AwaitingInput)
            {
                return;
            }

            double now = Time.realtimeSinceStartupAsDouble;
            if (now < _nextHeartbeat)
                return;

            _nextHeartbeat =
                now + Math.Max(0.25, _settings.HeartbeatSeconds);

            _log.Trace(
                BotDiagnosticCategory.Turn,
                "TURN.BOT_HEARTBEAT",
                "Bot turn is still active.",
                details:
                    $"round={_turns.Round}; globalTurn={_turns.GlobalTurn}; " +
                    $"owner='{_turns.ActiveOwnerId}'; phase={_turns.Phase}; " +
                    $"actionsThisTurn={_turns.ActionsThisTurn}; canBotFlag={_turns.IsActiveFactionBot}",
                ownerId: _turns.ActiveOwnerId);
        }

        private void CaptureAndLogState(bool force)
        {
            TurnPhase phase = _turns.Phase;
            string owner = _turns.ActiveOwnerId ?? string.Empty;
            long globalTurn = _turns.GlobalTurn;
            int round = _turns.Round;
            int actions = _turns.ActionsThisTurn;
            bool isBot = _turns.IsActiveFactionBot;

            bool changed =
                force ||
                !_initializedState ||
                phase != _lastPhase ||
                !string.Equals(owner, _lastOwner, StringComparison.Ordinal) ||
                globalTurn != _lastGlobalTurn ||
                round != _lastRound ||
                actions != _lastActions ||
                isBot != _lastIsBot;

            if (!changed)
                return;

            if (_initializedState &&
                globalTurn != _lastGlobalTurn)
            {
                _log.Info(
                    BotDiagnosticCategory.Handoff,
                    "TURN.GLOBAL_TURN_CHANGED",
                    "Authoritative global turn changed.",
                    details:
                        $"{_lastGlobalTurn} -> {globalTurn}; round {_lastRound} -> {round}; " +
                        $"owner '{_lastOwner}' -> '{owner}'; bot {_lastIsBot} -> {isBot}",
                    ownerId: owner);
            }

            if (_initializedState &&
                round != _lastRound)
            {
                _log.Warning(
                    BotDiagnosticCategory.Handoff,
                    "TURN.ROUND_CHANGED",
                    "Round counter changed.",
                    reason:
                        isBot || _lastIsBot
                            ? "Round transition occurred around a bot/human handoff."
                            : "If no bot faction appeared between player turns, inspect FACTION and SPAWN diagnostics.",
                    details:
                        $"round {_lastRound} -> {round}; globalTurn={globalTurn}; owner='{owner}'; isBot={isBot}",
                    ownerId: owner);
            }

            _log.Info(
                BotDiagnosticCategory.Turn,
                isBot
                    ? "TURN.ACTIVE_BOT"
                    : "TURN.ACTIVE_HUMAN",
                isBot
                    ? "Active turn belongs to a bot faction."
                    : "Active turn belongs to a non-bot faction.",
                details:
                    $"round={round}; globalTurn={globalTurn}; phase={phase}; owner='{owner}'; " +
                    $"actionsThisTurn={actions}; isBot={isBot}",
                ownerId: owner);

            _lastPhase = phase;
            _lastOwner = owner;
            _lastGlobalTurn = globalTurn;
            _lastRound = round;
            _lastActions = actions;
            _lastIsBot = isBot;
            _initializedState = true;
        }

        private void LogFactionRegistry(bool force)
        {
            var factions = _turns.Factions;
            var b = new StringBuilder();
            int botCount = 0;

            if (factions != null)
            {
                for (int i = 0; i < factions.Count; i++)
                {
                    TurnFaction faction = factions[i];
                    if (faction.IsBot)
                        botCount++;

                    if (i > 0)
                        b.Append(" | ");

                    b.Append('#').Append(i)
                        .Append(":owner='").Append(faction.OwnerId).Append('\'')
                        .Append(",bot=").Append(faction.IsBot)
                        .Append(",start=").Append(faction.StartPosition);
                }
            }

            string signature = b.ToString();
            if (!force &&
                string.Equals(signature, _lastFactionSignature, StringComparison.Ordinal))
            {
                return;
            }

            _lastFactionSignature = signature;
            int count = factions?.Count ?? 0;

            if (botCount == 0)
            {
                _log.Error(
                    BotDiagnosticCategory.Faction,
                    "FACTION.REGISTRY_NO_BOT",
                    "Faction registry changed but still contains no bot.",
                    reason:
                        "The authoritative turn loop therefore has no AI turn to hand off to.",
                    details:
                        $"count={count}; factions=[{signature}]");
            }
            else
            {
                _log.Info(
                    BotDiagnosticCategory.Faction,
                    "FACTION.REGISTRY",
                    "Faction registry contains bot participant(s).",
                    details:
                        $"count={count}; bots={botCount}; factions=[{signature}]");
            }
        }
    }
}
