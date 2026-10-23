using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;

namespace Kruty1918.Moyva.Turns.Runtime
{
    /// <summary>
    /// Simultaneous (Civilization-style) round flow. Every living faction acts
    /// during the same <see cref="TurnPhase.AwaitingInput"/> window; the round
    /// resolves on the authoritative peer once all required owners finished or
    /// the configured round timer expires. The timer is paused while a
    /// registered <see cref="ITurnTimerHold"/> applies (initial castle gate).
    /// </summary>
    internal sealed partial class TurnService : ITurnRoundState
    {
        private readonly HashSet<string> _endedTurnOwners = new(StringComparer.Ordinal);
        private float _roundEndsAtUnscaled = float.PositiveInfinity;

        public TurnFlowMode FlowMode => _rules.Mode;
        public int RoundTimerSeconds => _rules.RoundTimerSeconds;
        public IReadOnlyCollection<string> EndedTurnOwnerIds => _endedTurnOwners;

        public bool RoundTimerHeld => EvaluateRoundTimerHold(out _);

        public float RoundSecondsRemaining
            => FlowMode == TurnFlowMode.Simultaneous
               && RoundTimerSeconds > 0
               && Phase == TurnPhase.AwaitingInput
                   ? Mathf.Max(0f, _roundEndsAtUnscaled - Time.unscaledTime)
                   : 0f;

        public bool HasOwnerEndedTurn(string ownerId)
            => !string.IsNullOrWhiteSpace(ownerId)
               && _endedTurnOwners.Contains(ownerId.Trim());

        /// <summary>
        /// Drives the authoritative round timer. Called every frame by
        /// <see cref="TurnRoundTimerService"/>; no-ops for sequential mode,
        /// non-authoritative peers, inactive phases and disabled timers.
        /// </summary>
        internal void TickRoundTimer()
        {
            if (FlowMode != TurnFlowMode.Simultaneous
                || RoundTimerSeconds <= 0
                || Phase != TurnPhase.AwaitingInput
                || _matchEnded
                || !_worldReady
                || (_authority != null && !_authority.IsAuthoritative))
            {
                return;
            }

            float now = Time.unscaledTime;
            if (EvaluateRoundTimerHold(out _))
            {
                _roundEndsAtUnscaled = now + RoundTimerSeconds;
                return;
            }

            if (now >= _roundEndsAtUnscaled)
                CompleteSimultaneousRound();
        }

        private bool EvaluateRoundTimerHold(out string reason)
        {
            reason = null;
            if (FlowMode != TurnFlowMode.Simultaneous || RoundTimerSeconds <= 0)
                return false;

            List<ITurnTimerHold> holds = ResolveTimerHolds();
            for (int index = 0; index < holds.Count; index++)
            {
                if (holds[index] != null && holds[index].ShouldHoldRoundTimer(out reason))
                    return true;
            }

            return false;
        }

        private bool IsPendingOwner(string ownerId)
            => !_eliminatedOwners.Contains(ownerId) && !_endedTurnOwners.Contains(ownerId);

        private bool AllAliveOwnersEnded()
        {
            if (_factions.Count == 0)
                return false;

            for (int index = 0; index < _factions.Count; index++)
            {
                if (IsPendingOwner(_factions[index].OwnerId))
                    return false;
            }

            return true;
        }

        private void RefreshSimultaneousActiveCursor()
        {
            if (_factions.Count == 0)
                return;
            if (IsPendingOwner(_factions[_activeFactionIndex].OwnerId))
                return;

            for (int index = 0; index < _factions.Count; index++)
            {
                if (IsPendingOwner(_factions[index].OwnerId))
                {
                    _activeFactionIndex = index;
                    return;
                }
            }
        }

        private bool EndOwnerTurnSimultaneous(string requesterOwnerId)
        {
            _endedTurnOwners.Add(requesterOwnerId.Trim());
            _historyCache = null;
            RefreshSimultaneousActiveCursor();
            StateChanged?.Invoke();

            if (!AllAliveOwnersEnded())
                return true;

            return CompleteSimultaneousRound();
        }

        private bool CompleteSimultaneousRound()
        {
            List<ITurnParticipant> participants = ResolveParticipants();
            TurnStateSnapshot transition = CaptureTurnState();
            Phase = TurnPhase.Ending;
            transition = transition.WithPhaseAndActions(TurnPhase.Ending, ActionsThisTurn);
            StateChanged?.Invoke();
            if (!MatchesLifecycleState(transition, TurnPhase.Ending))
                return false;

            int completedRound = Round;
            var completedOwners = new List<string>(_factions.Count);
            for (int factionIndex = _factions.Count - 1; factionIndex >= 0; factionIndex--)
            {
                TurnFaction faction = _factions[factionIndex];
                if (_eliminatedOwners.Contains(faction.OwnerId))
                    continue;

                var context = new TurnContext(
                    completedRound, GlobalTurn, factionIndex, faction,
                    FlowMode, IsLocalOwnerId(faction.OwnerId));
                for (int index = participants.Count - 1; index >= 0; index--)
                {
                    participants[index]?.OnTurnEnding(context);
                    if (!MatchesLifecycleState(transition, TurnPhase.Ending))
                        return false;
                }

                completedOwners.Add(faction.OwnerId);
            }

            Phase = TurnPhase.Resolving;
            TurnStateSnapshot resolving = transition.WithPhaseAndActions(TurnPhase.Resolving, ActionsThisTurn);
            StateChanged?.Invoke();
            if (!MatchesLifecycleState(resolving, TurnPhase.Resolving))
                return false;

            if (!_roundResolution.TryResolve(
                    completedRound,
                    participants,
                    () => MatchesLifecycleState(resolving, TurnPhase.Resolving),
                    out string resolutionReason))
            {
                Debug.LogError(
                    $"[Turns] Round {completedRound} resolution failed; " +
                    $"next round will not start. Reason: {resolutionReason}");
                return false;
            }

            for (int index = 0; index < completedOwners.Count; index++)
            {
                RecordCompletedTurn(completedOwners[index]);
                GlobalTurn++;
            }

            Round++;
            return StartSimultaneousRound();
        }

        private bool StartSimultaneousRound()
        {
            List<ITurnParticipant> participants = ResolveParticipants();
            _endedTurnOwners.Clear();
            TrySelectFirstEligibleFaction();
            ArmRoundTimer();

            TurnStateSnapshot transition = CaptureTurnState();
            Phase = TurnPhase.Starting;
            ActionsThisTurn = 0;
            transition = transition.WithPhaseAndActions(TurnPhase.Starting, 0);
            StateChanged?.Invoke();
            if (!MatchesLifecycleState(transition, TurnPhase.Starting))
                return false;

            for (int factionIndex = 0; factionIndex < _factions.Count; factionIndex++)
            {
                TurnFaction faction = _factions[factionIndex];
                if (_eliminatedOwners.Contains(faction.OwnerId))
                    continue;

                var context = new TurnContext(
                    Round, GlobalTurn, factionIndex, faction,
                    FlowMode, IsLocalOwnerId(faction.OwnerId));
                for (int index = 0; index < participants.Count; index++)
                {
                    participants[index]?.OnTurnStarted(context);
                    if (!MatchesLifecycleState(transition, TurnPhase.Starting))
                        return false;
                }
            }

            if (AllAliveOwnersEnded() || !AnyAliveFaction())
            {
                Phase = TurnPhase.Resolving;
                StateChanged?.Invoke();
                return true;
            }

            Phase = TurnPhase.AwaitingInput;
            StateChanged?.Invoke();
            return true;
        }

        private bool AnyAliveFaction()
        {
            for (int index = 0; index < _factions.Count; index++)
            {
                if (!_eliminatedOwners.Contains(_factions[index].OwnerId))
                    return true;
            }

            return false;
        }

        private void ArmRoundTimer()
            => _roundEndsAtUnscaled = RoundTimerSeconds > 0
                ? Time.unscaledTime + RoundTimerSeconds
                : float.PositiveInfinity;

        private bool IsLocalOwnerId(string ownerId)
            => !string.IsNullOrWhiteSpace(ownerId)
               && string.Equals(ownerId.Trim(), LocalOwnerId, StringComparison.Ordinal);
    }
}
