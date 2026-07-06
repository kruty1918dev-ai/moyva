using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MoyvaTwcGraphValidationText
    {
        public static string FormatIssues(IEnumerable<GraphValidationIssue> issues)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var issue in issues)
                builder.AppendLine($"  - {issue}");
            return builder.ToString();
        }

        public static string FormatReport(GraphValidationReport report)
        {
            return report == null || report.Issues.Count == 0
                ? string.Empty
                : FormatIssues(report.Issues);
        }
    }
}
