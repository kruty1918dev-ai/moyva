using System;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
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
        private long _begunGlobalTurn = long.MinValue;
        private string _begunOwnerId = string.Empty;

        [Inject]
        public TurnBotDriver(
            ITurnService turns,
            DiContainer container,
            [InjectOptional] IBotTurnExecutor executor = null)
        {
            _turns = turns ?? throw new ArgumentNullException(nameof(turns));
            _executor = executor ?? TryCreateFallbackExecutor(container);
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
                // Claim before invoking the executor so synchronous signals cannot
                // re-enter this driver and start the same bot turn twice.
                _begunGlobalTurn = globalTurn;
                _begunOwnerId = ownerId;

                if (_executor == null)
                {
                    Debug.LogWarning(
                        $"[TurnBotDriver] No IBotTurnExecutor is available for owner '{ownerId}'. " +
                        "The turn will fail closed to a no-op and proceed when blockers allow.");
                }
                else if (!_executor.TryBeginTurn(ownerId, globalTurn, out string reason))
                {
                    Debug.LogWarning(
                        $"[TurnBotDriver] Bot execution did not start for owner '{ownerId}' " +
                        $"at globalTurn={globalTurn}: {reason}");
                }

                // Do not attempt End Turn in the same tick. Asynchronous gameplay
                // commands get one scheduler turn to register their ITurnBlocker state.
                return;
            }

            _turns.TryEndTurn(ownerId, out _);
        }

        private static IBotTurnExecutor TryCreateFallbackExecutor(DiContainer container)
        {
            if (container == null)
                return null;

            try
            {
                // Most scenes resolve the singleton bound by BotInstaller. This fallback
                // protects direct/test scenes that contain TurnBotDriver but omit BotInstaller.
                return container.Instantiate<BotTurnExecutor>();
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[TurnBotDriver] Could not construct fallback BotTurnExecutor: {exception.Message}");
                return null;
            }
        }

        private static string NormalizeId(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
