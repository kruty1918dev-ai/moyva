using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class EndTurnBotCapability : IBotCapabilityProvider
    {
        private readonly IBotTurnGateway _turns;
        public EndTurnBotCapability(IBotTurnGateway turns) { _turns = turns; }
        public BotCapabilityId Id => BotCapabilityId.Turn;
        public string UnavailableReason(string player) => _turns.CanEndTurn(player, out var reason) ? null : reason ?? "EndTurn unavailable.";
        public IEnumerable<BotCandidateAction> Enumerate(string player)
        {
            if (_turns.CanEndTurn(player, out _))
                yield return new BotCandidateAction("end", Id, BotIntentType.EndTurn, critical: true);
        }
        public bool Validate(string player, BotCandidateAction candidate, out string reason)
            => _turns.CanEndTurn(player, out reason);
        public Task<BotExecutionResult> Execute(string player, BotCandidateAction candidate, CancellationToken token)
        {
            if (token.IsCancellationRequested) return Task.FromResult(BotExecutionResult.Cancelled(candidate));
            bool success = _turns.EndTurn(player, out string reason);
            return Task.FromResult(new BotExecutionResult(success ? BotExecutionStatus.Completed : BotExecutionStatus.Rejected, candidate, reason));
        }
    }
}

