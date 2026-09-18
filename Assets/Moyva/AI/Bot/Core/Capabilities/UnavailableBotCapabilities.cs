using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.AI.Bot
{
    public interface IBotCombatGateway : IBotCapabilityProvider { }
    public interface IBotRecruitmentGateway : IBotCapabilityProvider { }
    public interface IBotConstructionGateway : IBotCapabilityProvider { }
    public interface IBotCaptureGateway : IBotCapabilityProvider { }

    public sealed class UnavailableBotCapability : IBotCapabilityProvider
    {
        public BotCapabilityId Id { get; }
        private readonly string _reason;
        public UnavailableBotCapability(BotCapabilityId id, string reason) { Id = id; _reason = reason; }
        public string UnavailableReason(string player) => _reason;
        public IEnumerable<BotCandidateAction> Enumerate(string player) { yield break; }
        public bool Validate(string player, BotCandidateAction candidate, out string reason) { reason = _reason; return false; }
        public Task<BotExecutionResult> Execute(string player, BotCandidateAction candidate, CancellationToken token)
            => Task.FromResult(new BotExecutionResult(BotExecutionStatus.Rejected, candidate, _reason));
    }
}

