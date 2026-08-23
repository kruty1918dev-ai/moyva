using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotStallTracker :
        IBotStallTracker
    {
        private sealed class OwnerState
        {
            public long ActiveTurn;
            public int ActiveMutations;
            public string LastCandidateId = string.Empty;
            public string LastFailureReason = string.Empty;
            public long LastCompletedTurn;
            public int ConsecutiveZeroMutationTurns;
        }

        private readonly Dictionary<string, OwnerState> _owners =
            new(StringComparer.Ordinal);

        private readonly BotPlanningProfile _profile;

        [Inject]
        public BotStallTracker(
            [InjectOptional] BotPlanningProfile profile = null)
        {
            _profile =
                profile ??
                BotPlanningProfile.Normal();
        }

        public void BeginTurn(
            string ownerId,
            long globalTurn)
        {
            string owner = Normalize(ownerId);
            if (owner == null ||
                globalTurn < 1)
            {
                return;
            }

            OwnerState state = GetOrCreate(owner);

            if (state.ActiveTurn == globalTurn)
                return;

            state.ActiveTurn = globalTurn;
            state.ActiveMutations = 0;

            // Preserve the previous completed turn's last candidate/reason until
            // this turn actually evaluates an action. Anti-stall policy needs
            // that history to penalize blind repetition across turns.
        }

        public void RecordActionResult(
            string ownerId,
            long globalTurn,
            string candidateId,
            bool succeeded,
            bool mutated,
            string failureReason)
        {
            string owner = Normalize(ownerId);
            if (owner == null)
                return;

            OwnerState state = GetOrCreate(owner);

            if (state.ActiveTurn != globalTurn)
                BeginTurn(owner, globalTurn);

            state.LastCandidateId =
                Normalize(candidateId) ??
                string.Empty;

            if (mutated)
            {
                state.ActiveMutations++;
                state.LastFailureReason =
                    string.Empty;
                return;
            }

            if (!succeeded ||
                !string.IsNullOrWhiteSpace(failureReason))
            {
                state.LastFailureReason =
                    Normalize(failureReason) ??
                    (succeeded
                        ? "action produced no mutation"
                        : "action rejected");
            }
        }

        public BotStallStatus CompleteTurn(
            string ownerId,
            long globalTurn)
        {
            string owner = Normalize(ownerId);

            if (owner == null)
                return default;

            OwnerState state = GetOrCreate(owner);

            // Idempotent completion protects against lifecycle re-entry.
            if (state.LastCompletedTurn == globalTurn)
                return BuildStatus(owner, state);

            if (state.ActiveTurn != globalTurn)
                BeginTurn(owner, globalTurn);

            if (state.ActiveMutations <= 0)
                state.ConsecutiveZeroMutationTurns++;
            else
                state.ConsecutiveZeroMutationTurns = 0;

            state.LastCompletedTurn = globalTurn;

            return BuildStatus(owner, state);
        }

        public BotStallStatus GetStatus(
            string ownerId)
        {
            string owner = Normalize(ownerId);

            if (owner == null ||
                !_owners.TryGetValue(
                    owner,
                    out OwnerState state))
            {
                return new BotStallStatus(
                    owner ?? string.Empty,
                    0,
                    0,
                    ResolveThreshold(),
                    string.Empty,
                    string.Empty);
            }

            return BuildStatus(owner, state);
        }

        private BotStallStatus BuildStatus(
            string owner,
            OwnerState state)
            => new(
                owner,
                state.LastCompletedTurn,
                state.ConsecutiveZeroMutationTurns,
                ResolveThreshold(),
                state.LastCandidateId,
                state.LastFailureReason);

        private int ResolveThreshold()
            => Math.Max(
                1,
                _profile.StallTurnThreshold);

        private OwnerState GetOrCreate(
            string owner)
        {
            if (!_owners.TryGetValue(
                    owner,
                    out OwnerState state))
            {
                state = new OwnerState();
                _owners.Add(owner, state);
            }

            return state;
        }

        private static string Normalize(
            string value)
            => string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
    }
}
