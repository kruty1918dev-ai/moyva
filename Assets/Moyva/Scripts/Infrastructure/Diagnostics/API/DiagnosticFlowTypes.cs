using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Diagnostics.API
{
    public readonly struct DiagnosticFlowId
    {
        public DiagnosticFlowId(string value)
        {
            Value = string.IsNullOrWhiteSpace(value) ? "FLOW-00000" : value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value ?? "FLOW-00000";
        }
    }

    public sealed class DiagnosticContext
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        public static DiagnosticContext Empty => new DiagnosticContext();

        public DiagnosticContext Add(string key, object value)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _values[key] = value?.ToString() ?? "null";

            return this;
        }

        public IReadOnlyDictionary<string, string> Values => _values;

        public bool IsEmpty => _values.Count == 0;

        public override string ToString()
        {
            if (_values.Count == 0)
                return string.Empty;

            var parts = new string[_values.Count];
            int index = 0;
            foreach (KeyValuePair<string, string> pair in _values)
            {
                parts[index] = $"{pair.Key}={pair.Value}";
                index++;
            }

            return string.Join(", ", parts);
        }

        public static DiagnosticContext FromDetails(string details)
        {
            return string.IsNullOrWhiteSpace(details)
                ? Empty
                : new DiagnosticContext().Add("details", details);
        }
    }

    public enum DiagnosticStepState
    {
        Pending = 0,
        Started = 1,
        Completed = 2,
        Failed = 3,
        Skipped = 4,
        Missing = 5,
        Blocked = 6,
        Timeout = 7,
    }

    public enum DiagnosticFlowStatus
    {
        Ok = 0,
        Broken = 1,
        Failed = 2,
        Timeout = 3,
    }

    public sealed class DiagnosticStepDefinition
    {
        private readonly string[] _dependsOn;

        public DiagnosticStepDefinition(
            string id,
            bool isRequired = true,
            double timeoutMilliseconds = 0d,
            IEnumerable<string> dependsOn = null)
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Step id cannot be empty.", nameof(id)) : id;
            IsRequired = isRequired;
            TimeoutMilliseconds = Math.Max(0d, timeoutMilliseconds);
            _dependsOn = dependsOn != null ? new List<string>(dependsOn).ToArray() : Array.Empty<string>();
        }

        public string Id { get; }
        public bool IsRequired { get; }
        public double TimeoutMilliseconds { get; }
        public IReadOnlyList<string> DependsOn => _dependsOn;

        public static DiagnosticStepDefinition Required(string id, double timeoutMilliseconds = 0d, params string[] dependsOn)
        {
            return new DiagnosticStepDefinition(id, true, timeoutMilliseconds, dependsOn);
        }

        public static DiagnosticStepDefinition Optional(string id, double timeoutMilliseconds = 0d, params string[] dependsOn)
        {
            return new DiagnosticStepDefinition(id, false, timeoutMilliseconds, dependsOn);
        }
    }

    public sealed class DiagnosticFlowDefinition
    {
        private readonly DiagnosticStepDefinition[] _steps;

        public DiagnosticFlowDefinition(
            string id,
            string displayName,
            string tracePrefix,
            IEnumerable<DiagnosticStepDefinition> steps,
            double timeoutMilliseconds = 6000d,
            bool strictOrder = true)
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Flow id cannot be empty.", nameof(id)) : id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
            TracePrefix = string.IsNullOrWhiteSpace(tracePrefix) ? "FLOW" : tracePrefix;
            _steps = steps != null ? new List<DiagnosticStepDefinition>(steps).ToArray() : Array.Empty<DiagnosticStepDefinition>();
            TimeoutMilliseconds = Math.Max(0d, timeoutMilliseconds);
            StrictOrder = strictOrder;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string TracePrefix { get; }
        public IReadOnlyList<DiagnosticStepDefinition> Steps => _steps;
        public double TimeoutMilliseconds { get; }
        public bool StrictOrder { get; }
    }

    public sealed class DiagnosticStepRecord
    {
        public DiagnosticStepRecord(
            string stepId,
            DiagnosticStepState state,
            double startedAtMilliseconds,
            double completedAtMilliseconds,
            DiagnosticContext context,
            string reason)
        {
            StepId = stepId;
            State = state;
            StartedAtMilliseconds = startedAtMilliseconds;
            CompletedAtMilliseconds = completedAtMilliseconds;
            Context = context ?? DiagnosticContext.Empty;
            Reason = reason;
        }

        public string StepId { get; }
        public DiagnosticStepState State { get; }
        public double StartedAtMilliseconds { get; }
        public double CompletedAtMilliseconds { get; }
        public DiagnosticContext Context { get; }
        public string Details => Context.ToString();
        public string Reason { get; }
    }

    public sealed class DiagnosticStepAnalysis
    {
        public DiagnosticStepAnalysis(int index, string stepId, DiagnosticStepState state, DiagnosticContext context, string reason)
        {
            Index = index;
            StepId = stepId;
            State = state;
            Context = context ?? DiagnosticContext.Empty;
            Reason = reason;
        }

        public int Index { get; }
        public string StepId { get; }
        public DiagnosticStepState State { get; }
        public DiagnosticContext Context { get; }
        public string Details => Context.ToString();
        public string Reason { get; }
    }

    public sealed class DiagnosticFlowAnalysis
    {
        private readonly DiagnosticStepAnalysis[] _steps;

        public DiagnosticFlowAnalysis(
            string flowName,
            DiagnosticFlowId flowId,
            DiagnosticFlowStatus status,
            double elapsedMilliseconds,
            string lastCompletedStepId,
            string rootBreakPointStepId,
            IEnumerable<DiagnosticStepAnalysis> steps)
        {
            FlowName = flowName;
            FlowId = flowId;
            Status = status;
            ElapsedMilliseconds = elapsedMilliseconds;
            LastCompletedStepId = lastCompletedStepId;
            RootBreakPointStepId = rootBreakPointStepId;
            _steps = steps != null ? new List<DiagnosticStepAnalysis>(steps).ToArray() : Array.Empty<DiagnosticStepAnalysis>();
        }

        public string FlowName { get; }
        public DiagnosticFlowId FlowId { get; }
        public string TraceId => FlowId.ToString();
        public DiagnosticFlowStatus Status { get; }
        public double ElapsedMilliseconds { get; }
        public string LastCompletedStepId { get; }
        public string RootBreakPointStepId { get; }
        public IReadOnlyList<DiagnosticStepAnalysis> Steps => _steps;
    }
}
