using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;

namespace Kruty1918.Moyva.Turns.Runtime
{
    /// <summary>
    /// Replays the deterministic turn lifecycle a multiplayer client missed.
    /// The authoritative peer already committed the state the client just
    /// restored; this path only reproduces the same participant callbacks so
    /// client-side construction progress, recruitment, stamina and the
    /// calendar-driven economy tick stay consistent. All contexts are marked
    /// <see cref="TurnContext.IsReplicated"/> so single-sourced observers
    /// (telemetry) can opt out.
    /// </summary>
    internal sealed partial class TurnService
    {
        private const int MaxReplayRounds = 64;
        private const int MaxReplaySteps = 256;

        private void ReplayAuthoritativeDelta(in TurnTransitionDelta delta)
        {
            try
            {
                List<ITurnParticipant> participants = ResolveParticipants();
                if (FlowMode == TurnFlowMode.Simultaneous)
                    ReplaySimultaneousDelta(participants, in delta);
                else
                    ReplaySequentialDelta(participants, in delta);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Turns] Replicated turn-effects replay failed: {exception}");
            }
        }

        // Simultaneous host flow per completed round (see CompleteSimultaneousRound):
        // OnTurnEnding per alive faction -> RoundResolution (OnRoundCompleted +
        // calendar advance) -> GlobalTurn += completedOwners -> Round++ ->
        // OnTurnStarted per alive faction with the new round state.
        private void ReplaySimultaneousDelta(
            List<ITurnParticipant> participants,
            in TurnTransitionDelta delta)
        {
            int completedRounds = Math.Min(Round - delta.PreviousRound, MaxReplayRounds);
            if (completedRounds <= 0 || _factions.Count == 0)
                return;

            int aliveCount = Math.Max(1, CountAliveFactions());
            // Host increments GlobalTurn once per completed owner during each
            // resolution, so round r started at applied value minus that step
            // per round still to go. Intermediate values are approximations on
            // multi-round jumps; the final round lands exactly on GlobalTurn.
            long GlobalTurnAtRoundStart(int round)
                => GlobalTurn - aliveCount * (long)(Round - round);

            for (int step = 0; step < completedRounds; step++)
            {
                int completedRound = delta.PreviousRound + step;
                long endingGlobal = GlobalTurnAtRoundStart(completedRound);

                for (int factionIndex = _factions.Count - 1; factionIndex >= 0; factionIndex--)
                {
                    TurnFaction faction = _factions[factionIndex];
                    if (_eliminatedOwners.Contains(faction.OwnerId))
                        continue;

                    var ending = new TurnContext(
                        completedRound, endingGlobal, factionIndex, faction,
                        FlowMode, IsLocalOwnerId(faction.OwnerId), isReplicated: true);
                    for (int index = participants.Count - 1; index >= 0; index--)
                        participants[index]?.OnTurnEnding(ending);
                }

                if (!_roundResolution.TryResolve(
                        completedRound, participants, null, out string reason))
                {
                    Debug.LogWarning(
                        $"[Turns] Replicated round {completedRound} resolution failed: {reason}");
                    return;
                }

                int nextRound = completedRound + 1;
                long startGlobal = GlobalTurnAtRoundStart(nextRound);
                for (int factionIndex = 0; factionIndex < _factions.Count; factionIndex++)
                {
                    TurnFaction faction = _factions[factionIndex];
                    if (_eliminatedOwners.Contains(faction.OwnerId))
                        continue;

                    var starting = new TurnContext(
                        nextRound, startGlobal, factionIndex, faction,
                        FlowMode, IsLocalOwnerId(faction.OwnerId), isReplicated: true);
                    for (int index = 0; index < participants.Count; index++)
                        participants[index]?.OnTurnStarted(starting);
                }
            }
        }

        // Sequential host flow per end turn (see EndCurrentTurn): OnTurnEnding
        // for the finishing owner -> eligible-faction walk (a wrap resolves the
        // round) -> GlobalTurn++ -> OnTurnStarted for the next owner.
        private void ReplaySequentialDelta(
            List<ITurnParticipant> participants,
            in TurnTransitionDelta delta)
        {
            long steps = Math.Min(GlobalTurn - delta.PreviousGlobalTurn, MaxReplaySteps);
            if (steps <= 0 || _factions.Count == 0)
                return;

            int cursor = delta.PreviousActiveFactionIndex;
            if (cursor < 0 || cursor >= _factions.Count)
                return;

            int round = delta.PreviousRound;
            long globalTurn = delta.PreviousGlobalTurn;

            for (long step = 0; step < steps; step++)
            {
                TurnFaction endingFaction = _factions[cursor];
                var ending = new TurnContext(
                    round, globalTurn, cursor, endingFaction,
                    FlowMode, IsLocalOwnerId(endingFaction.OwnerId), isReplicated: true);
                for (int index = participants.Count - 1; index >= 0; index--)
                    participants[index]?.OnTurnEnding(ending);

                if (!TryFindNextEligibleFrom(cursor, out int nextIndex, out bool wrapped))
                    break;

                if (wrapped)
                {
                    if (!_roundResolution.TryResolve(
                            round, participants, null, out string reason))
                    {
                        Debug.LogWarning(
                            $"[Turns] Replicated round {round} resolution failed: {reason}");
                        return;
                    }

                    round++;
                }

                cursor = nextIndex;
                globalTurn++;

                TurnFaction startingFaction = _factions[cursor];
                var starting = new TurnContext(
                    round, globalTurn, cursor, startingFaction,
                    FlowMode, IsLocalOwnerId(startingFaction.OwnerId), isReplicated: true);
                for (int index = 0; index < participants.Count; index++)
                    participants[index]?.OnTurnStarted(starting);
            }
        }

        private int CountAliveFactions()
        {
            int alive = 0;
            for (int index = 0; index < _factions.Count; index++)
            {
                if (!_eliminatedOwners.Contains(_factions[index].OwnerId))
                    alive++;
            }

            return alive;
        }
    }
}
