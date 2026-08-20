namespace Kruty1918.Moyva.BotAI.API
{
    public readonly struct BotActionExecutionResult
    {
        private BotActionExecutionResult(bool succeeded, bool mutated, string reason)
        {
            Succeeded = succeeded;
            Mutated = mutated;
            Reason = string.IsNullOrWhiteSpace(reason) ? string.Empty : reason.Trim();
        }

        public bool Succeeded { get; }
        public bool Mutated { get; }
        public string Reason { get; }

        public static BotActionExecutionResult Success(string reason = null)
            => new(true, true, reason);

        public static BotActionExecutionResult NoOp(string reason = null)
            => new(true, false, reason);

        public static BotActionExecutionResult Rejected(string reason)
            => new(false, false, reason);
    }
}
