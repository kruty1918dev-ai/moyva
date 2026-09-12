namespace Kruty1918.Moyva.AI.Bot
{
    public enum BotExecutionStatus { Accepted, Completed, Rejected, Failed, Pending, Cancelled }
    public enum BotDecisionFailure { None, ModelInvalid, StaleState, CommandRejected, PolicyTimeout, ExecutionTimeout, Cancelled }
    public sealed class BotExecutionResult
    {
        public BotExecutionStatus Status { get; }
        public BotCandidateAction Candidate { get; }
        public string Reason { get; }
        public string CorrelationId { get; }
        public BotExecutionResult(BotExecutionStatus status, BotCandidateAction candidate, string reason = null, string correlationId = null)
        { Status = status; Candidate = candidate; Reason = reason; CorrelationId = correlationId; }
        public static BotExecutionResult Cancelled(BotCandidateAction candidate) => new BotExecutionResult(BotExecutionStatus.Cancelled, candidate);
    }
}

