using System;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.BotAI.Diagnostics;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Thin authoritative bridge between ITurnService and the bot executor.
    /// It begins at most one executor pass per owner/global-turn epoch, then retries
    /// TryEndTurn on later ticks until normal ITurnBlockers allow the turn to finish.
    /// </summary>
    internal sealed class TurnBotDriver : ITickable
    {
        private readonly ITurnService _turns;
        private readonly IBotTurnExecutor _executor;
        private readonly IBotDiagnosticsLogger _diagnostics;
        private long _begunGlobalTurn = long.MinValue;
        private string _lastEndTurnReason = string.Empty;
        private double _nextEndTurnDiagnosticTime;
        private string _begunOwnerId = string.Empty;

        [Inject]
        public TurnBotDriver(
            ITurnService turns,
            DiContainer container,
            [InjectOptional] IBotTurnExecutor executor = null,
            [InjectOptional] IBotDiagnosticsLogger diagnostics = null)
        {
            _turns = turns ?? throw new ArgumentNullException(nameof(turns));
            _diagnostics = diagnostics;

            bool injectedExecutor = executor != null;
            _executor = executor;

            _diagnostics?.Info(
                BotDiagnosticCategory.Executor,
                "DRIVER.CONSTRUCTED",
                "TurnBotDriver constructed.",
                reason: injectedExecutor
                    ? "IBotTurnExecutor was injected from the container."
                    : "No executor binding was injected. Canonical BotRuntimeBindings must provide IBotTurnExecutor.",
                details:
                    $"injectedExecutor={injectedExecutor}; executorAvailable={_executor != null}; " +
                    "fallbackConstruction=false");
        }

        public void Tick()
        {
            if (_turns.Phase != TurnPhase.AwaitingInput
                || !_turns.IsActiveFactionBot)
            {
                return;
            }

            string ownerId = NormalizeId(_turns.ActiveOwnerId);
            if (ownerId == null)
                return;

            long globalTurn = _turns.GlobalTurn;
            bool alreadyBegun =
                _begunGlobalTurn == globalTurn
                && string.Equals(_begunOwnerId, ownerId, StringComparison.Ordinal);

            if (!alreadyBegun)
            {
                _diagnostics?.Info(
                    BotDiagnosticCategory.Executor,
                    "DRIVER.BOT_EPOCH_DETECTED",
                    "TurnBotDriver detected a new authoritative bot turn epoch.",
                    details:
                        $"owner='{ownerId}'; globalTurn={globalTurn}; round={_turns.Round}; " +
                        $"phase={_turns.Phase}; actionsThisTurn={_turns.ActionsThisTurn}",
                    ownerId: ownerId);

                // Claim before invoking the executor so synchronous signals cannot
                // re-enter this driver and start the same bot turn twice.
                _begunGlobalTurn = globalTurn;
                _begunOwnerId = ownerId;

                if (_executor == null)
                {
                    _diagnostics?.Critical(
                        BotDiagnosticCategory.Executor,
                        "DRIVER.EXECUTOR_MISSING",
                        "No IBotTurnExecutor is available for the active bot.",
                        reason:
                            "The driver cannot execute any AI action. The turn will become a no-op and may end immediately.",
                        details:
                            $"owner='{ownerId}'; globalTurn={globalTurn}; " +
                            $"this strongly suggests BotInstaller/runtime bindings are missing or failed.",
                        ownerId: ownerId);

                    Debug.LogWarning(
                        $"[TurnBotDriver] No IBotTurnExecutor is available for owner '{ownerId}'. " +
                        "The turn will fail closed to a no-op and proceed when blockers allow.");
                }
                else if (!_executor.TryBeginTurn(ownerId, globalTurn, out string reason))
                {
                    _diagnostics?.Error(
                        BotDiagnosticCategory.Executor,
                        "DRIVER.TRY_BEGIN_REJECTED",
                        "IBotTurnExecutor rejected the bot turn.",
                        reason: reason,
                        details:
                            $"owner='{ownerId}'; globalTurn={globalTurn}; phase={_turns.Phase}; " +
                            $"isActiveFactionBot={_turns.IsActiveFactionBot}; activeOwner='{_turns.ActiveOwnerId}'",
                        ownerId: ownerId);

                    Debug.LogWarning(
                        $"[TurnBotDriver] Bot execution did not start for owner '{ownerId}' " +
                        $"at globalTurn={globalTurn}: {reason}");
                }
                else
                {
                    _diagnostics?.Info(
                        BotDiagnosticCategory.Executor,
                        "DRIVER.TRY_BEGIN_ACCEPTED",
                        "IBotTurnExecutor accepted the bot turn.",
                        details:
                            $"owner='{ownerId}'; globalTurn={globalTurn}; executorType='{_executor.GetType().FullName}'",
                        ownerId: ownerId);
                }

                // Do not attempt End Turn in the same tick. Asynchronous gameplay
                // commands get one scheduler turn to register their ITurnBlocker state.
                return;
            }

            bool ended = _turns.TryEndTurn(ownerId, out string endReason);
            double now = Time.realtimeSinceStartupAsDouble;

            if (ended)
            {
                _diagnostics?.Info(
                    BotDiagnosticCategory.Handoff,
                    "DRIVER.END_TURN_SUCCEEDED",
                    "Authoritative bot turn ended successfully.",
                    reason:
                        "ITurnService accepted TryEndTurn after the bot executor epoch had begun.",
                    details:
                        $"owner='{ownerId}'; completedGlobalTurn={globalTurn}; " +
                        $"newActiveOwner='{_turns.ActiveOwnerId}'; newGlobalTurn={_turns.GlobalTurn}; newRound={_turns.Round}",
                    ownerId: ownerId);

                _lastEndTurnReason = string.Empty;
                _nextEndTurnDiagnosticTime = 0d;
                return;
            }

            string normalizedReason = endReason ?? string.Empty;
            bool reasonChanged =
                !string.Equals(
                    normalizedReason,
                    _lastEndTurnReason,
                    StringComparison.Ordinal);

            if (reasonChanged || now >= _nextEndTurnDiagnosticTime)
            {
                _diagnostics?.Trace(
                    BotDiagnosticCategory.Handoff,
                    "DRIVER.END_TURN_BLOCKED",
                    "Bot turn cannot end yet.",
                    reason:
                        string.IsNullOrWhiteSpace(normalizedReason)
                            ? "ITurnService rejected TryEndTurn without a textual reason."
                            : normalizedReason,
                    details:
                        $"owner='{ownerId}'; globalTurn={globalTurn}; phase={_turns.Phase}; " +
                        $"actionsThisTurn={_turns.ActionsThisTurn}; retrying on later ticks.",
                    ownerId: ownerId);

                _lastEndTurnReason = normalizedReason;
                _nextEndTurnDiagnosticTime = now + 1.0d;
            }
        }


        private static string NormalizeId(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
