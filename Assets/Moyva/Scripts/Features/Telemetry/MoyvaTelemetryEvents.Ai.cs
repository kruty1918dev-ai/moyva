using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Telemetry.Core;

namespace Kruty1918.Moyva.Telemetry
{
    /// <summary>One bot decision — the ML-valuable state→action→result record.</summary>
    public struct MoyvaBotDecisionEvent : ITelemetryEvent
    {
        public long Turn;
        public long Sequence;
        public string Intent;
        public string Capability;
        public string Result;
        public int Candidates;
        public int RealCandidates;
        public float LatencyMs;
        public string PolicyMode;
        public string ContractHash;
        public string ObservationHash;
        public string Failure;
        public string Reason;

        public string EventType => "moyva.ai.bot.decision";
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public static MoyvaBotDecisionEvent FromTrace(BotDecisionTrace t) => new MoyvaBotDecisionEvent
        {
            Turn = t.Turn,
            Sequence = t.Sequence,
            Intent = t.Intent.ToString(),
            Capability = t.Capability.ToString(),
            Result = t.Result.ToString(),
            Candidates = t.CandidateCount,
            RealCandidates = t.RealCandidateCount,
            LatencyMs = t.Latency,
            PolicyMode = t.Mode.ToString(),
            ContractHash = t.ContractHash,
            ObservationHash = t.ObservationHash,
            Failure = t.Failure.ToString(),
            Reason = Trunc(t.Reason),
        };

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("turn", Turn);
            w.Field("sequence", Sequence);
            w.Field("intent", Intent);
            w.Field("capability", Capability);
            w.Field("result", Result);
            w.Field("candidates", Candidates);
            w.Field("realCandidates", RealCandidates);
            w.Field("latencyMs", LatencyMs);
            w.Field("policyMode", PolicyMode);
            w.Field("contractHash", ContractHash);
            w.Field("observationHash", ObservationHash);
            if (Failure != null) w.Field("failure", Failure);
            if (Reason != null) w.Field("reason", Reason);
        }

        private static string Trunc(string s) => s != null && s.Length > 256 ? s.Substring(0, 256) : s;
    }

    /// <summary>Training episode rollup (reward/outcome/health signals).</summary>
    public struct MoyvaBotEpisodeEvent : ITelemetryEvent
    {
        public long Episode;
        public float Reward;
        public float Shaping;
        public bool Won;
        public bool Lost;
        public bool Draw;
        public bool Timeout;
        public int Decisions;
        public int Turns;

        public string EventType => "moyva.ai.bot.episode";
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public static MoyvaBotEpisodeEvent From(BotEpisodeMetrics m) => new MoyvaBotEpisodeEvent
        {
            Episode = m.Episode,
            Reward = m.Reward,
            Shaping = m.Shaping,
            Won = m.Won,
            Lost = m.Lost,
            Draw = m.Draw,
            Timeout = m.Timeout,
            Decisions = m.Decisions,
            Turns = m.Turns,
        };

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("episode", Episode);
            w.Field("reward", Reward);
            w.Field("shaping", Shaping);
            w.Field("won", Won);
            w.Field("lost", Lost);
            w.Field("draw", Draw);
            w.Field("timeout", Timeout);
            w.Field("decisions", Decisions);
            w.Field("turns", Turns);
        }
    }
}
