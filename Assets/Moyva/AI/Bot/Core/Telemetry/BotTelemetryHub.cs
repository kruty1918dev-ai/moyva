using System;
using System.Collections.Generic;
using System.Linq;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class BotDecisionTrace
    {
        public string SessionId, Player, Reason, ContractHash, ObservationHash;
        public long Turn, Sequence;
        public int CandidateCount, RealCandidateCount, Slot;
        public BotIntentType Intent;
        public BotCapabilityId Capability;
        public BotPolicyMode Mode;
        public BotExecutionStatus Result;
        public BotDecisionFailure Failure;
        public float Latency;
        public BotDecisionTrace(BotTurnSession session, BotDecisionFrame frame, int slot, BotPolicyMode mode,
            BotExecutionResult result, BotDecisionFailure failure, float latency)
        {
            SessionId = session.Id; Player = session.Owner; Turn = session.Turn; Sequence = frame?.Sequence ?? 0;
            CandidateCount = frame?.Candidates.Count ?? 0; RealCandidateCount = frame?.RealCandidateCount ?? 0;
            Slot = slot; Mode = mode; Result = result.Status;
            Intent = result.Candidate?.Intent ?? BotIntentType.None;
            Capability = result.Candidate?.Capability ?? BotCapabilityId.Turn;
            Reason = result.Reason; Failure = failure; Latency = latency; ContractHash = BotDecisionContract.Hash;
            unchecked
            {
                uint hash = 2166136261;
                if (frame != null) foreach (float value in frame.Observations)
                    foreach (byte b in BitConverter.GetBytes(value)) hash = (hash ^ b) * 16777619;
                ObservationHash = hash.ToString("x8");
            }
        }
    }
    public sealed class BotTelemetryHub
    {
        private readonly Queue<BotDecisionTrace> _traces = new Queue<BotDecisionTrace>();
        private readonly int _capacity;
        private readonly bool _enabled;
        public int EnvironmentId = -1;
        public BotPolicyMode PolicyMode;
        public string ProfileName, FallbackReason, LastError;
        public float? ExplorationRate;
        public int NonFiniteValues, NoLegalActionFallbackCount;
        public BotRunMetrics Metrics { get; } = new BotRunMetrics();
        public IEnumerable<BotDecisionTrace> Traces => _traces.ToArray();
        public BotDecisionTrace Last { get; private set; }
        /// <summary>Fired after each recorded trace (not fired when telemetry is disabled).</summary>
        public event Action<BotDecisionTrace> TraceRecorded;

        public BotTelemetryHub(int capacity = 128, bool enabled = true) { _capacity = Math.Clamp(capacity, 1, 4096); _enabled = enabled; }
        public void Record(BotDecisionTrace trace)
        {
            if (!_enabled) return;
            Last = trace; Metrics.Record(trace);
            if (_traces.Count == _capacity) _traces.Dequeue();
            _traces.Enqueue(trace);
            TraceRecorded?.Invoke(trace);
        }
    }
    public sealed class BotRunMetrics
    {
        private readonly Queue<BotEpisodeMetrics> _episodes = new Queue<BotEpisodeMetrics>();
        /// <summary>Fired after each recorded episode metrics row.</summary>
        public event Action<BotEpisodeMetrics> EpisodeRecorded;
        public long Decisions, Invalid, Stale, CandidateTotal, EndTurns;
        public long[] IntentCounts { get; } = new long[12];
        public IReadOnlyList<BotEpisodeMetrics> Episodes => _episodes.ToArray();
        public void Record(BotDecisionTrace trace)
        {
            Decisions++; CandidateTotal += trace.RealCandidateCount;
            if (trace.Failure == BotDecisionFailure.ModelInvalid) Invalid++;
            if (trace.Failure == BotDecisionFailure.StaleState) Stale++;
            IntentCounts[(int)trace.Intent]++;
            if (trace.Intent == BotIntentType.EndTurn && trace.Result == BotExecutionStatus.Completed) EndTurns++;
        }
        public void RecordEpisode(BotEpisodeMetrics episode)
        {
            if (_episodes.Count == 50) _episodes.Dequeue();
            _episodes.Enqueue(episode);
            EpisodeRecorded?.Invoke(episode);
        }
    }
    public readonly struct BotEpisodeMetrics
    {
        public readonly long Episode;
        public readonly float Reward, Shaping;
        public readonly bool Won, Lost, Draw, Timeout;
        public readonly int Decisions, Turns;
        public BotEpisodeMetrics(long episode, float reward, float shaping, bool won, bool lost, bool draw, bool timeout, int decisions, int turns)
        { Episode = episode; Reward = reward; Shaping = shaping; Won = won; Lost = lost; Draw = draw; Timeout = timeout; Decisions = decisions; Turns = turns; }
    }
    public enum BotLearningHealth { NoData, Warmup, Improving, Plateau, Unstable, ActionCollapse, InvalidHeavy, Stalling, RewardExploitSuspected }
    public static class BotLearningHealthEvaluator
    {
        public static BotLearningHealth Evaluate(BotRunMetrics metrics)
        {
            var episodes = metrics.Episodes;
            if (episodes.Count == 0) return BotLearningHealth.NoData;
            if (episodes.Count < 10) return BotLearningHealth.Warmup;
            if (metrics.Invalid / (double)Math.Max(1, metrics.Decisions) > 0.2) return BotLearningHealth.InvalidHeavy;
            if (episodes.Count(x => x.Timeout) > episodes.Count * 0.7) return BotLearningHealth.Stalling;
            if (metrics.IntentCounts.Max() > metrics.Decisions * 0.95) return BotLearningHealth.ActionCollapse;
            int half = episodes.Count / 2;
            float previous = episodes.Take(half).Average(x => x.Reward);
            float recent = episodes.Skip(half).Average(x => x.Reward);
            double winChange = episodes.Skip(half).Average(x => x.Won ? 1 : 0) - episodes.Take(half).Average(x => x.Won ? 1 : 0);
            if (recent > previous + 0.05f && winChange <= 0 && episodes.Skip(half).Average(x => x.Shaping) > 0.1f)
                return BotLearningHealth.RewardExploitSuspected;
            if (episodes.Max(x => x.Reward) - episodes.Min(x => x.Reward) > 2f) return BotLearningHealth.Unstable;
            return recent > previous + 0.05f || winChange > 0.05 ? BotLearningHealth.Improving : BotLearningHealth.Plateau;
        }
    }
}
