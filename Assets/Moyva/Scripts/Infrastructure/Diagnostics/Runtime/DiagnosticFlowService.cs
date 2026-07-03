using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Diagnostics.API;
using Zenject;

namespace Kruty1918.Moyva.Diagnostics.Runtime
{
    internal sealed class DiagnosticFlowService : IDiagnosticFlowService, IDiagnosticFlowReporter, ITickable
    {
        private readonly IDiagnosticClock _clock;
        private readonly IDiagnosticFlowAnalyzer _analyzer;
        private readonly IDiagnosticFlowFormatter _formatter;
        private readonly IDiagnosticRuntimeOptions _options;
        private readonly List<IDiagnosticSink> _sinks;
        private readonly Dictionary<string, DiagnosticFlow> _activeFlowsById = new Dictionary<string, DiagnosticFlow>();
        private readonly Dictionary<string, DiagnosticFlow> _activeFlowsByKey = new Dictionary<string, DiagnosticFlow>();
        private int _nextTraceNumber;

        public DiagnosticFlowService(
            IDiagnosticClock clock,
            IDiagnosticFlowAnalyzer analyzer,
            IDiagnosticFlowFormatter formatter,
            IDiagnosticRuntimeOptions options,
            List<IDiagnosticSink> sinks)
        {
            _clock = clock;
            _analyzer = analyzer;
            _formatter = formatter;
            _options = options ?? DiagnosticRuntimeOptions.Default;
            _sinks = sinks ?? new List<IDiagnosticSink>();
        }

        public IDiagnosticFlow StartFlow(DiagnosticFlowDefinition definition)
        {
            return StartFlow(definition, null, DiagnosticContext.Empty);
        }

        public IDiagnosticFlow StartFlow(DiagnosticFlowDefinition definition, string subject, DiagnosticContext context)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (!_options.IsEnabled)
                return NullDiagnosticFlow.Instance;

            var flow = new DiagnosticFlow(
                definition,
                new DiagnosticFlowId(CreateTraceId(definition)),
                subject,
                context ?? DiagnosticContext.Empty,
                _clock,
                _analyzer,
                _formatter,
                _options,
                _sinks,
                HandleFlowSummaryReported);

            _activeFlowsById[flow.Id.Value] = flow;
            return flow;
        }

        public IDiagnosticFlow GetOrStartFlow(string flowKey, DiagnosticFlowDefinition definition)
        {
            return GetOrStartFlow(flowKey, definition, null, DiagnosticContext.Empty);
        }

        public IDiagnosticFlow GetOrStartFlow(string flowKey, DiagnosticFlowDefinition definition, string subject, DiagnosticContext context)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrWhiteSpace(flowKey))
                throw new ArgumentException("Flow key cannot be empty.", nameof(flowKey));

            if (!_options.IsEnabled)
                return NullDiagnosticFlow.Instance;

            if (_activeFlowsByKey.TryGetValue(flowKey, out DiagnosticFlow flow)
                && !flow.IsSummaryReported)
            {
                return flow;
            }

            flow = (DiagnosticFlow)StartFlow(definition, subject, context);
            _activeFlowsByKey[flowKey] = flow;
            return flow;
        }

        public void ReportFlow(IDiagnosticFlow flow, bool forceTimeout = false)
        {
            flow?.ReportSummary(forceTimeout);
        }

        public void Report(IDiagnosticFlow flow, bool forceTimeout = false)
        {
            flow?.ReportSummary(forceTimeout);
        }

        public void Tick()
        {
            var snapshot = new List<DiagnosticFlow>(_activeFlowsById.Values);
            for (int index = 0; index < snapshot.Count; index++)
            {
                DiagnosticFlow flow = snapshot[index];
                if (flow.IsSummaryReported || flow.Definition.TimeoutMilliseconds <= 0d)
                    continue;

                double elapsed = _clock.NowMilliseconds - flow.StartedAtMilliseconds;
                if (elapsed >= flow.Definition.TimeoutMilliseconds)
                    flow.ReportSummary(forceTimeout: true);
            }
        }

        private string CreateTraceId(DiagnosticFlowDefinition definition)
        {
            _nextTraceNumber++;
            return $"{definition.TracePrefix}-{_nextTraceNumber:00000}";
        }

        private void HandleFlowSummaryReported(DiagnosticFlow flow)
        {
            if (flow == null)
                return;

            _activeFlowsById.Remove(flow.Id.Value);

            var keysToRemove = new List<string>();
            foreach (KeyValuePair<string, DiagnosticFlow> pair in _activeFlowsByKey)
            {
                if (ReferenceEquals(pair.Value, flow))
                    keysToRemove.Add(pair.Key);
            }

            for (int index = 0; index < keysToRemove.Count; index++)
                _activeFlowsByKey.Remove(keysToRemove[index]);
        }

        private sealed class DiagnosticFlow : IDiagnosticFlow
        {
            private readonly IDiagnosticClock _clock;
            private readonly IDiagnosticFlowAnalyzer _analyzer;
            private readonly IDiagnosticFlowFormatter _formatter;
            private readonly IDiagnosticRuntimeOptions _options;
            private readonly List<IDiagnosticSink> _sinks;
            private readonly Action<DiagnosticFlow> _onSummaryReported;
            private readonly Dictionary<string, DiagnosticStepRecord> _records = new Dictionary<string, DiagnosticStepRecord>();

            public DiagnosticFlow(
                DiagnosticFlowDefinition definition,
                DiagnosticFlowId id,
                string subject,
                DiagnosticContext context,
                IDiagnosticClock clock,
                IDiagnosticFlowAnalyzer analyzer,
                IDiagnosticFlowFormatter formatter,
                IDiagnosticRuntimeOptions options,
                List<IDiagnosticSink> sinks,
                Action<DiagnosticFlow> onSummaryReported)
            {
                Definition = definition;
                Id = id;
                Subject = subject;
                Context = context ?? DiagnosticContext.Empty;
                _clock = clock;
                _analyzer = analyzer;
                _formatter = formatter;
                _options = options ?? DiagnosticRuntimeOptions.Default;
                _sinks = sinks;
                _onSummaryReported = onSummaryReported;
                StartedAtMilliseconds = clock.NowMilliseconds;
            }

            public DiagnosticFlowId Id { get; }
            public string TraceId => Id.ToString();
            public DiagnosticFlowDefinition Definition { get; }
            public string Subject { get; }
            public DiagnosticContext Context { get; }
            public double StartedAtMilliseconds { get; }
            public bool IsSummaryReported { get; private set; }
            public IReadOnlyDictionary<string, DiagnosticStepRecord> Records => _records;

            public IDiagnosticStepScope BeginStep(string stepId, DiagnosticContext context)
            {
                MarkStep(stepId, DiagnosticStepState.Started, context, null);
                return new DiagnosticStepScope(this, stepId);
            }

            public IDiagnosticStepScope BeginStep(string stepId, string details = null)
            {
                return BeginStep(stepId, DiagnosticContext.FromDetails(details));
            }

            public void CompleteStep(string stepId, DiagnosticContext context)
            {
                MarkStep(stepId, DiagnosticStepState.Completed, context, null);
                if (IsTerminalStep(stepId))
                    ReportSummary();
            }

            public void CompleteStep(string stepId, string details = null)
            {
                CompleteStep(stepId, DiagnosticContext.FromDetails(details));
            }

            public void FailStep(string stepId, string reason, DiagnosticContext context)
            {
                MarkStep(stepId, DiagnosticStepState.Failed, context, reason);
                ReportSummary();
            }

            public void FailStep(string stepId, string reason, string details = null)
            {
                FailStep(stepId, reason, DiagnosticContext.FromDetails(details));
            }

            public void SkipStep(string stepId, string reason, DiagnosticContext context)
            {
                MarkStep(stepId, DiagnosticStepState.Skipped, context, reason);
            }

            public void SkipStep(string stepId, string reason, string details = null)
            {
                SkipStep(stepId, reason, DiagnosticContext.FromDetails(details));
            }

            public void ReportSummary(bool forceTimeout = false)
            {
                if (IsSummaryReported && !forceTimeout)
                    return;

                IsSummaryReported = true;
                DiagnosticFlowAnalysis analysis = _analyzer.Analyze(this, forceTimeout);
                _onSummaryReported?.Invoke(this);
                if (analysis.Status == DiagnosticFlowStatus.Ok && !_options.EmitOkFlows)
                    return;

                string message = _formatter.Format(analysis);

                for (int index = 0; index < _sinks.Count; index++)
                    _sinks[index]?.Emit(analysis, message);
            }

            private void MarkStep(string stepId, DiagnosticStepState state, DiagnosticContext context, string reason)
            {
                if (string.IsNullOrWhiteSpace(stepId))
                    return;

                double now = _clock.NowMilliseconds;
                double startedAt = now;
                if (_records.TryGetValue(stepId, out DiagnosticStepRecord existing)
                    && existing.StartedAtMilliseconds > 0d)
                {
                    startedAt = existing.StartedAtMilliseconds;
                }

                _records[stepId] = new DiagnosticStepRecord(
                    stepId,
                    state,
                    startedAt,
                    now,
                    context ?? DiagnosticContext.Empty,
                    reason);
            }

            private bool IsTerminalStep(string stepId)
            {
                IReadOnlyList<DiagnosticStepDefinition> steps = Definition.Steps;
                if (steps.Count == 0)
                    return false;

                return steps[steps.Count - 1].Id == stepId;
            }
        }

        private sealed class DiagnosticStepScope : IDiagnosticStepScope
        {
            private readonly IDiagnosticFlow _flow;
            private readonly string _stepId;
            private bool _completed;

            public DiagnosticStepScope(IDiagnosticFlow flow, string stepId)
            {
                _flow = flow;
                _stepId = stepId;
            }

            public void Complete(DiagnosticContext context)
            {
                if (_completed)
                    return;

                _completed = true;
                _flow.CompleteStep(_stepId, context);
            }

            public void Complete(string details = null)
            {
                Complete(DiagnosticContext.FromDetails(details));
            }

            public void Fail(string reason, DiagnosticContext context)
            {
                if (_completed)
                    return;

                _completed = true;
                _flow.FailStep(_stepId, reason, context);
            }

            public void Fail(string reason, string details = null)
            {
                Fail(reason, DiagnosticContext.FromDetails(details));
            }

            public void Skip(string reason, DiagnosticContext context)
            {
                if (_completed)
                    return;

                _completed = true;
                _flow.SkipStep(_stepId, reason, context);
            }

            public void Skip(string reason, string details = null)
            {
                Skip(reason, DiagnosticContext.FromDetails(details));
            }

            public void Dispose()
            {
                if (!_completed)
                    Complete();
            }
        }

        private sealed class NullDiagnosticFlow : IDiagnosticFlow
        {
            public static readonly NullDiagnosticFlow Instance = new NullDiagnosticFlow();
            private static readonly IReadOnlyDictionary<string, DiagnosticStepRecord> EmptyRecords =
                new Dictionary<string, DiagnosticStepRecord>();

            private NullDiagnosticFlow()
            {
            }

            public DiagnosticFlowId Id { get; } = new DiagnosticFlowId("disabled");
            public string TraceId => Id.ToString();
            public DiagnosticFlowDefinition Definition { get; } = new DiagnosticFlowDefinition("disabled", "disabled", "DISABLED", null, 0d);
            public string Subject => null;
            public DiagnosticContext Context => DiagnosticContext.Empty;
            public double StartedAtMilliseconds => 0d;
            public bool IsSummaryReported => true;
            public IReadOnlyDictionary<string, DiagnosticStepRecord> Records => EmptyRecords;

            public IDiagnosticStepScope BeginStep(string stepId, DiagnosticContext context)
            {
                return NullDiagnosticStepScope.Instance;
            }

            public IDiagnosticStepScope BeginStep(string stepId, string details = null)
            {
                return NullDiagnosticStepScope.Instance;
            }

            public void CompleteStep(string stepId, DiagnosticContext context) { }
            public void CompleteStep(string stepId, string details = null) { }
            public void FailStep(string stepId, string reason, DiagnosticContext context) { }
            public void FailStep(string stepId, string reason, string details = null) { }
            public void SkipStep(string stepId, string reason, DiagnosticContext context) { }
            public void SkipStep(string stepId, string reason, string details = null) { }
            public void ReportSummary(bool forceTimeout = false) { }
        }

        private sealed class NullDiagnosticStepScope : IDiagnosticStepScope
        {
            public static readonly NullDiagnosticStepScope Instance = new NullDiagnosticStepScope();

            private NullDiagnosticStepScope()
            {
            }

            public void Complete(DiagnosticContext context) { }
            public void Complete(string details = null) { }
            public void Fail(string reason, DiagnosticContext context) { }
            public void Fail(string reason, string details = null) { }
            public void Skip(string reason, DiagnosticContext context) { }
            public void Skip(string reason, string details = null) { }
            public void Dispose() { }
        }
    }
}
