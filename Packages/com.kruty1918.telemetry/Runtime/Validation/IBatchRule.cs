using System.Collections.Generic;
using Kruty1918.Telemetry.Serialization;

namespace Kruty1918.Telemetry.Validation
{
    /// <summary>
    /// Optional semantic/sequence/statistical rule evaluated over the parsed events of
    /// one sealed batch. Rules are registered by id and referenced from EventContract.RuleIds,
    /// keeping the contract the single source of which checks apply.
    /// </summary>
    public interface IBatchRule
    {
        string RuleId { get; }
        /// <summary>Inspect parsed events (each = one JSONL line as JsonValue) and report violations.</summary>
        void Evaluate(IReadOnlyList<JsonValue> events, DataQualityReport report);
    }

    /// <summary>
    /// Sequence rules: match.end requires match.start in the same gameplay session;
    /// terminal events must not repeat; event sequences must be non-decreasing.
    /// Event types are matched by suffix convention: "*.started"/"*.ended" pairs.
    /// </summary>
    public sealed class LifecycleSequenceRule : IBatchRule
    {
        public string RuleId => "seq.lifecycle";
        private readonly string _startSuffix;
        private readonly string _endSuffix;

        public LifecycleSequenceRule(string startSuffix = ".started", string endSuffix = ".ended")
        {
            _startSuffix = startSuffix;
            _endSuffix = endSuffix;
        }

        public void Evaluate(IReadOnlyList<JsonValue> events, DataQualityReport report)
        {
            var openScopes = new HashSet<string>(System.StringComparer.Ordinal);
            var closedScopes = new HashSet<string>(System.StringComparer.Ordinal);
            long lastSeq = -1;
            foreach (var e in events)
            {
                string type = e.GetString("t");
                long seq = e.GetInt("seq", -1);
                if (seq >= 0 && seq < lastSeq)
                    report.Add(new ValidationViolation(ValidationViolationKind.Sequence, type, "out-of-order", seq));
                if (seq >= 0) lastSeq = seq;
                if (type == null) continue;
                if (type.EndsWith(_startSuffix, System.StringComparison.Ordinal))
                {
                    string scope = type.Substring(0, type.Length - _startSuffix.Length);
                    if (closedScopes.Remove(scope))
                        report.Add(new ValidationViolation(ValidationViolationKind.Sequence, type, "reopened-after-end", seq));
                    openScopes.Add(scope);
                }
                else if (type.EndsWith(_endSuffix, System.StringComparison.Ordinal))
                {
                    string scope = type.Substring(0, type.Length - _endSuffix.Length);
                    if (!openScopes.Remove(scope) && !closedScopes.Contains(scope))
                        report.Add(new ValidationViolation(ValidationViolationKind.Sequence, type, "end-without-start", seq));
                    if (!closedScopes.Add(scope))
                        report.Add(new ValidationViolation(ValidationViolationKind.Sequence, type, "duplicate-terminal", seq));
                }
            }
        }
    }

    /// <summary>
    /// Statistical rules over a batch: suspicious constant numeric fields and
    /// impossible event rates. Advisory — anomalies flag quality, not rejection.
    /// </summary>
    public sealed class StatisticalBatchRule : IBatchRule
    {
        public string RuleId => "stat.batch";
        private readonly int _minEventsForConstantCheck;
        private readonly int _maxEventsPerSecond;

        public StatisticalBatchRule(int minEventsForConstantCheck = 64, int maxEventsPerSecond = 2000)
        {
            _minEventsForConstantCheck = minEventsForConstantCheck;
            _maxEventsPerSecond = maxEventsPerSecond;
        }

        public void Evaluate(IReadOnlyList<JsonValue> events, DataQualityReport report)
        {
            if (events.Count == 0) return;
            // Event rate: compare first/last timestamps (ms) with count.
            long t0 = events[0].GetInt("ts", 0);
            long t1 = events[events.Count - 1].GetInt("ts", 0);
            double seconds = (t1 - t0) / 1000.0;
            if (seconds > 0 && events.Count / seconds > _maxEventsPerSecond)
                report.Anomalies.Add("rate-exceeded:" + (int)(events.Count / seconds));

            // Constant-field heuristic per numeric field within a contract.
            var firstValues = new Dictionary<string, double>(System.StringComparer.Ordinal);
            var varying = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var e in events)
            {
                if (!e.TryGet("d", out var d) || !d.IsObject) continue;
                foreach (var kv in d.Obj)
                {
                    if (!(kv.Value.Raw is long || kv.Value.Raw is double)) continue;
                    if (varying.Contains(kv.Key)) continue;
                    double num = kv.Value.Num;
                    if (!firstValues.TryGetValue(kv.Key, out double f)) firstValues[kv.Key] = num;
                    else if (f != num) varying.Add(kv.Key);
                }
            }
            if (events.Count >= _minEventsForConstantCheck)
                foreach (var kv in firstValues)
                    if (!varying.Contains(kv.Key))
                        report.Anomalies.Add("constant-field:" + kv.Key);
        }
    }
}
