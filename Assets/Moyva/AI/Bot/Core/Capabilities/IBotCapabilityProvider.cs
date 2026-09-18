using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.AI.Bot
{
    public interface IBotTurnGateway
    {
        BotGameStamp Read(string player);
        bool CanEndTurn(string player, out string reason);
        bool EndTurn(string player, out string reason);
    }
    public interface IBotCapabilityProvider
    {
        BotCapabilityId Id { get; }
        string UnavailableReason(string player);
        IEnumerable<BotCandidateAction> Enumerate(string player);
        bool Validate(string player, BotCandidateAction candidate, out string reason);
        Task<BotExecutionResult> Execute(string player, BotCandidateAction candidate, CancellationToken token);
    }

    public sealed class BotCapabilityRegistry
    {
        private readonly SortedDictionary<BotCapabilityId, IBotCapabilityProvider> _providers = new SortedDictionary<BotCapabilityId, IBotCapabilityProvider>();
        public IEnumerable<IBotCapabilityProvider> Providers => _providers.Values;
        public void Register(IBotCapabilityProvider provider)
        {
            if (_providers.ContainsKey(provider.Id)) throw new System.ArgumentException("Duplicate capability: " + provider.Id);
            _providers.Add(provider.Id, provider);
        }
        public IBotCapabilityProvider Get(BotCapabilityId id) => _providers.TryGetValue(id, out var value) ? value : null;
    }
}

