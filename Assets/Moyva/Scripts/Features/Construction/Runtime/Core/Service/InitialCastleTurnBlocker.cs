using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Round-1 gate: a faction cannot finish its first turn before placing the
    /// initial castle, and the simultaneous round timer is held while any
    /// living faction still owes one. Implements both blocker shapes so the
    /// check works for local input (<see cref="ITurnBlocker"/>) and for the
    /// host validating a remote player's end-turn (<see cref="ITurnOwnerBlocker"/>).
    /// </summary>
    internal sealed class InitialCastleTurnBlocker : ITurnBlocker, ITurnOwnerBlocker, ITurnTimerHold
    {
        private readonly IConstructionBootstrapQuery _bootstrap;
        private readonly ITurnService _turns;
        private readonly ITurnHistoryQuery _history;

        public InitialCastleTurnBlocker(
            IConstructionBootstrapQuery bootstrap,
            ITurnService turns,
            ITurnHistoryQuery history)
        {
            _bootstrap = bootstrap;
            _turns = turns;
            _history = history;
        }

        public bool IsTurnBlocked(out string reason)
            => IsTurnBlocked(_turns?.LocalOwnerId, out reason);

        public bool IsTurnBlocked(string ownerId, out string reason)
        {
            reason = null;
            if (_bootstrap == null || _turns == null || string.IsNullOrWhiteSpace(ownerId))
                return false;

            if (!_bootstrap.RequiresInitialCastle(ownerId.Trim(), out _))
                return false;

            reason = "Спочатку розмістіть стартовий замок.";
            return true;
        }

        public bool ShouldHoldRoundTimer(out string reason)
        {
            reason = null;
            if (_bootstrap == null || _turns == null)
                return false;

            for (int index = 0; index < _turns.Factions.Count; index++)
            {
                string ownerId = _turns.Factions[index].OwnerId;
                if (IsEliminated(ownerId))
                    continue;

                if (_bootstrap.RequiresInitialCastle(ownerId, out _))
                {
                    reason = $"Фракція '{ownerId}' ще не розмістила стартовий замок.";
                    return true;
                }
            }

            return false;
        }

        private bool IsEliminated(string ownerId)
        {
            var history = _history?.GetParticipantHistory();
            if (history == null)
                return false;

            for (int index = 0; index < history.Count; index++)
            {
                var item = history[index];
                if (item.IsEliminated
                    && string.Equals(item.OwnerId, ownerId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
