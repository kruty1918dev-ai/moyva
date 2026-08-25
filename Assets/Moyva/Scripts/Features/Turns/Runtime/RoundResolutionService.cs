using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;

namespace Kruty1918.Moyva.Turns.Runtime
{
    /// <summary>
    /// Owns the deterministic end-of-round resolution transaction.
    ///
    /// Order is intentionally explicit:
    /// 1) turn participants resolve their round work in TurnOrder order;
    /// 2) calendar advances exactly once;
    /// 3) calendar-driven systems (currently EconomyManager) process the economy tick;
    /// 4) TurnService may advance Round and start the next faction turn only after success.
    /// </summary>
    internal sealed class RoundResolutionService
    {
        private readonly ICalendarService _calendar;

        public RoundResolutionService(ICalendarService calendar)
        {
            _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        }

        public bool TryResolve(
            int completedRound,
            IReadOnlyList<ITurnParticipant> participants,
            Func<bool> lifecycleGuard,
            out string reason)
        {
            reason = null;
            if (completedRound < 1)
            {
                reason = $"Completed round must be positive, got {completedRound}.";
                return false;
            }

            participants ??= Array.Empty<ITurnParticipant>();

            try
            {
                for (int index = 0; index < participants.Count; index++)
                {
                    ITurnParticipant participant = participants[index];
                    if (participant == null)
                        continue;

                    participant.OnRoundCompleted(completedRound);

                    if (lifecycleGuard != null && !lifecycleGuard())
                    {
                        reason = $"Turn lifecycle changed while resolving participant {participant.GetType().Name}.";
                        return false;
                    }
                }
                _calendar.AdvanceTurn();

                if (lifecycleGuard != null && !lifecycleGuard())
                {
                    reason = "Turn lifecycle changed while advancing the round calendar.";
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                reason = $"Round {completedRound} resolution failed: {exception.Message}";
                Debug.LogError($"[RoundResolution] failed round={completedRound}: {exception}");
                return false;
            }
        }
    }
}
