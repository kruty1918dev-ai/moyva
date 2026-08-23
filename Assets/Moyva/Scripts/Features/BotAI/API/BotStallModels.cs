using System;

namespace Kruty1918.Moyva.BotAI.API
{
    public readonly struct BotStallStatus
    {
        public BotStallStatus(
            string ownerId,
            long lastCompletedTurn,
            int consecutiveZeroMutationTurns,
            int threshold,
            string lastCandidateId,
            string lastFailureReason)
        {
            OwnerId =
                string.IsNullOrWhiteSpace(ownerId)
                    ? string.Empty
                    : ownerId.Trim();

            LastCompletedTurn = lastCompletedTurn;
            ConsecutiveZeroMutationTurns =
                Math.Max(0, consecutiveZeroMutationTurns);

            Threshold = Math.Max(1, threshold);

            LastCandidateId =
                string.IsNullOrWhiteSpace(lastCandidateId)
                    ? string.Empty
                    : lastCandidateId.Trim();

            LastFailureReason =
                string.IsNullOrWhiteSpace(lastFailureReason)
                    ? string.Empty
                    : lastFailureReason.Trim();
        }

        public string OwnerId { get; }
        public long LastCompletedTurn { get; }
        public int ConsecutiveZeroMutationTurns { get; }
        public int Threshold { get; }
        public string LastCandidateId { get; }
        public string LastFailureReason { get; }

        public bool IsStalled =>
            Threshold > 0 &&
            ConsecutiveZeroMutationTurns >= Threshold;

        public string StallReason =>
            IsStalled
                ? $"Anti-stall invariant triggered: " +
                  $"{ConsecutiveZeroMutationTurns} consecutive completed " +
                  $"bot turns produced 0 authoritative mutations. " +
                  $"lastCandidate='{LastCandidateId}', " +
                  $"lastFailure='{LastFailureReason}'."
                : string.Empty;
    }

    public interface IBotStallTracker
    {
        void BeginTurn(
            string ownerId,
            long globalTurn);

        void RecordActionResult(
            string ownerId,
            long globalTurn,
            string candidateId,
            bool succeeded,
            bool mutated,
            string failureReason);

        BotStallStatus CompleteTurn(
            string ownerId,
            long globalTurn);

        BotStallStatus GetStatus(
            string ownerId);
    }
}
