using System.Text;
using Kruty1918.Moyva.Diagnostics.API;

namespace Kruty1918.Moyva.Diagnostics.Runtime
{
    internal sealed class DiagnosticFlowFormatter : IDiagnosticFlowFormatter
    {
        public string Format(DiagnosticFlowAnalysis analysis)
        {
            if (analysis == null)
                return "[MoyvaFlow] null analysis";

            var builder = new StringBuilder(512);
            builder.Append("[MoyvaFlow] ")
                .Append(analysis.FlowName)
                .Append(" trace=")
                .Append(analysis.TraceId)
                .Append(" status=")
                .Append(FormatStatus(analysis.Status))
                .Append(" elapsed=")
                .Append((int)analysis.ElapsedMilliseconds)
                .AppendLine("ms");

            for (int i = 0; i < analysis.Steps.Count; i++)
            {
                DiagnosticStepAnalysis step = analysis.Steps[i];
                builder.Append(step.Index.ToString("00"))
                    .Append(' ')
                    .Append(FormatStepState(step.State).PadRight(8))
                    .Append(' ')
                    .Append(step.StepId);

                if (!string.IsNullOrEmpty(step.Reason))
                    builder.Append(" reason=").Append(step.Reason);
                if (!string.IsNullOrEmpty(step.Details))
                    builder.Append(" details=").Append(step.Details);

                builder.AppendLine();
            }

            if (!string.IsNullOrEmpty(analysis.RootBreakPointStepId))
            {
                builder.AppendLine()
                    .AppendLine("Break point:")
                    .AppendLine(analysis.RootBreakPointStepId);
            }

            if (!string.IsNullOrEmpty(analysis.LastCompletedStepId))
            {
                builder.AppendLine("Last completed:")
                    .AppendLine(analysis.LastCompletedStepId);
            }

            return builder.ToString();
        }

        private static string FormatStatus(DiagnosticFlowStatus status)
        {
            switch (status)
            {
                case DiagnosticFlowStatus.Ok:
                    return "OK";
                case DiagnosticFlowStatus.Broken:
                    return "BROKEN";
                case DiagnosticFlowStatus.Failed:
                    return "FAILED";
                case DiagnosticFlowStatus.Timeout:
                    return "TIMEOUT";
                default:
                    return status.ToString().ToUpperInvariant();
            }
        }

        private static string FormatStepState(DiagnosticStepState state)
        {
            return state == DiagnosticStepState.Completed
                ? "OK"
                : state.ToString().ToUpperInvariant();
        }
    }
}
