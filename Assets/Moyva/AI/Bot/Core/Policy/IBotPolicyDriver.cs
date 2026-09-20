using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.AI.Bot
{
    public enum BotPolicyMode { Heuristic, MLAgentsTraining, MLAgentsInference, External, Disabled }
    public readonly struct BotPolicyDecision
    {
        public readonly int Slot;
        public readonly long Sequence;
        public readonly BotPolicyMode Mode;
        public BotPolicyDecision(int slot, long sequence, BotPolicyMode mode) { Slot = slot; Sequence = sequence; Mode = mode; }
    }
    public interface IBotPolicyDriver
    {
        BotPolicyMode Mode { get; }
        Task<BotPolicyDecision> Decide(BotDecisionFrame frame, CancellationToken token);
    }
    public interface IBotPolicyDriverFactory { IBotPolicyDriver Create(BotRuntimeConfig config, BotTelemetryHub telemetry); }
    public sealed class HeuristicBotPolicyDriver : IBotPolicyDriver
    {
        public BotPolicyMode Mode => BotPolicyMode.Heuristic;
        public Task<BotPolicyDecision> Decide(BotDecisionFrame frame, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            // Only the shared, player-visible frame is available to this policy.
            foreach (var intent in new[] { BotIntentType.Attack, BotIntentType.Capture, BotIntentType.Recruit,
                BotIntentType.Build, BotIntentType.Move, BotIntentType.EndTurn })
                for (int i = 0; i < frame.Candidates.Count; i++)
                    if (frame.Candidates.IsLegal(i) && frame.Candidates[i].Intent == intent)
                        return Task.FromResult(new BotPolicyDecision(i, frame.Sequence, Mode));
            return Task.FromResult(new BotPolicyDecision(0, frame.Sequence, Mode));
        }
    }
    /// <summary>
    /// Difficulty wrapper: with probability <paramref name="explorationRate"/> the
    /// decision is replaced by a uniformly random legal candidate. This weakens
    /// any inner policy (learned or heuristic) without touching observations,
    /// resources, fog or rules — a legitimate strength dial, not a cheat.
    /// </summary>
    public sealed class EpsilonBlendPolicyDriver : IBotPolicyDriver
    {
        private readonly IBotPolicyDriver _inner;
        private readonly float _explorationRate;
        private readonly System.Random _random;
        public IBotPolicyDriver Inner => _inner;
        public float ExplorationRate => _explorationRate;
        public BotPolicyMode Mode => _inner.Mode;
        public EpsilonBlendPolicyDriver(IBotPolicyDriver inner, float explorationRate, int seed = 1337)
        {
            _inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
            _explorationRate = System.Math.Clamp(explorationRate, 0f, 1f);
            _random = new System.Random(seed);
        }
        public async Task<BotPolicyDecision> Decide(BotDecisionFrame frame, CancellationToken token)
        {
            var decision = await _inner.Decide(frame, token);
            if (_explorationRate <= 0f || _random.NextDouble() >= _explorationRate) return decision;
            int legal = 0;
            for (int i = 0; i < frame.Candidates.Count; i++)
                if (frame.Candidates.IsLegal(i)) legal++;
            if (legal <= 1) return decision;
            int pick = _random.Next(legal);
            for (int i = 0; i < frame.Candidates.Count; i++)
                if (frame.Candidates.IsLegal(i) && pick-- == 0)
                    return new BotPolicyDecision(i, frame.Sequence, Mode);
            return decision;
        }
    }

    public sealed class ManualBotPolicyDriver : IBotPolicyDriver
    {
        private TaskCompletionSource<BotPolicyDecision> _pending;
        private long _sequence;
        private CancellationTokenRegistration _registration;
        public BotPolicyMode Mode => BotPolicyMode.MLAgentsTraining;
        public Task<BotPolicyDecision> Decide(BotDecisionFrame frame, CancellationToken token)
        {
            if (_pending != null && !_pending.Task.IsCompleted) throw new System.InvalidOperationException("Decision already pending.");
            _registration.Dispose();
            _pending = new TaskCompletionSource<BotPolicyDecision>();
            _sequence = frame.Sequence;
            var pending = _pending;
            _registration = token.Register(() => pending.TrySetCanceled());
            return _pending.Task;
        }
        public void Submit(int slot) => _pending?.TrySetResult(new BotPolicyDecision(slot, _sequence, Mode));
    }
    public static class BotPolicyContractValidator
    {
        public static bool Validate(BotModelProfile profile, bool hasModel, out string reason)
        {
            reason = profile == null || !profile.enabled ? "Model disabled." : !hasModel ? "No model assigned."
                : profile.contractVersion != BotDecisionContract.ContractVersion || profile.contractHash != BotDecisionContract.Hash
                    ? "Model contract mismatch." : null;
            return reason == null;
        }
    }
}

