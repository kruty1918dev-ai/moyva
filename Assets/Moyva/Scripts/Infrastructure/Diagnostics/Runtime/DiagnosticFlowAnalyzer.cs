using System.Collections.Generic;
using Kruty1918.Moyva.Diagnostics.API;

namespace Kruty1918.Moyva.Diagnostics.Runtime
{
    internal sealed class DiagnosticFlowAnalyzer : IDiagnosticFlowAnalyzer
    {
        private readonly IDiagnosticClock _clock;

        public DiagnosticFlowAnalyzer(IDiagnosticClock clock)
        {
            _clock = clock;
        }

        public DiagnosticFlowAnalysis Analyze(IDiagnosticFlow flow, bool forceTimeout = false)
        {
            if (flow == null)
            {
                return new DiagnosticFlowAnalysis(
                    "null",
                    new DiagnosticFlowId("null"),
                    DiagnosticFlowStatus.Broken,
                    0d,
                    null,
                    "flow-null",
                    null);
            }

            double now = _clock.NowMilliseconds;
            double elapsed = now - flow.StartedAtMilliseconds;
            bool timedOut = forceTimeout
                || (flow.Definition.TimeoutMilliseconds > 0d && elapsed >= flow.Definition.TimeoutMilliseconds);
            var analyzedSteps = new List<DiagnosticStepAnalysis>();
            string lastCompleted = null;
            string rootBreakPoint = null;
            DiagnosticFlowStatus status = DiagnosticFlowStatus.Ok;
            bool hasBlockingBreak = false;
            var analyzedStateByStep = new Dictionary<string, DiagnosticStepState>();

            IReadOnlyList<DiagnosticStepDefinition> expectedSteps = flow.Definition.Steps;
            for (int index = 0; index < expectedSteps.Count; index++)
            {
                DiagnosticStepDefinition expected = expectedSteps[index];
                DiagnosticStepState state;
                DiagnosticContext context = DiagnosticContext.Empty;
                string reason = null;

                if ((flow.Definition.StrictOrder && hasBlockingBreak)
                    || HasBlockedDependency(expected, analyzedStateByStep, out reason))
                {
                    state = DiagnosticStepState.Blocked;
                }
                else if (flow.Records.TryGetValue(expected.Id, out DiagnosticStepRecord record))
                {
                    state = record.State;
                    context = record.Context;
                    reason = record.Reason;

                    if (state == DiagnosticStepState.Completed)
                    {
                        lastCompleted = expected.Id;
                    }
                    else if (state == DiagnosticStepState.Failed)
                    {
                        status = DiagnosticFlowStatus.Failed;
                        rootBreakPoint = expected.Id;
                        hasBlockingBreak = true;
                    }
                    else if (state == DiagnosticStepState.Skipped && expected.IsRequired)
                    {
                        status = DiagnosticFlowStatus.Broken;
                        rootBreakPoint = expected.Id;
                        hasBlockingBreak = true;
                    }
                    else if (state == DiagnosticStepState.Started && IsStepTimedOut(record, expected, now))
                    {
                        state = DiagnosticStepState.Timeout;
                        status = DiagnosticFlowStatus.Timeout;
                        rootBreakPoint = expected.Id;
                        hasBlockingBreak = true;
                    }
                }
                else if (!expected.IsRequired)
                {
                    state = DiagnosticStepState.Skipped;
                    reason = "optional-not-observed";
                }
                else
                {
                    state = timedOut ? DiagnosticStepState.Timeout : DiagnosticStepState.Missing;
                    status = timedOut ? DiagnosticFlowStatus.Timeout : DiagnosticFlowStatus.Broken;
                    rootBreakPoint = expected.Id;
                    hasBlockingBreak = true;
                }

                analyzedSteps.Add(new DiagnosticStepAnalysis(index + 1, expected.Id, state, context, reason));
                analyzedStateByStep[expected.Id] = state;
            }

            return new DiagnosticFlowAnalysis(
                flow.Definition.DisplayName,
                flow.Id,
                status,
                elapsed,
                lastCompleted,
                rootBreakPoint,
                analyzedSteps);
        }

        private static bool IsStepTimedOut(DiagnosticStepRecord record, DiagnosticStepDefinition expected, double now)
        {
            if (record == null || expected.TimeoutMilliseconds <= 0d)
                return false;

            return now - record.StartedAtMilliseconds >= expected.TimeoutMilliseconds;
        }

        private static bool HasBlockedDependency(
            DiagnosticStepDefinition expected,
            IReadOnlyDictionary<string, DiagnosticStepState> analyzedStateByStep,
            out string reason)
        {
            reason = null;
            if (expected?.DependsOn == null || expected.DependsOn.Count == 0)
                return false;

            for (int index = 0; index < expected.DependsOn.Count; index++)
            {
                string dependency = expected.DependsOn[index];
                if (string.IsNullOrEmpty(dependency))
                    continue;

                if (!analyzedStateByStep.TryGetValue(dependency, out DiagnosticStepState dependencyState)
                    || dependencyState != DiagnosticStepState.Completed)
                {
                    reason = $"dependency-not-completed:{dependency}";
                    return true;
                }
            }

            return false;
        }
    }
}
