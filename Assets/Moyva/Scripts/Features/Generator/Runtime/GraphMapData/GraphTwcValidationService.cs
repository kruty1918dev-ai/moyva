using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcValidationService
    {
        GraphTwcValidationResult Validate(GraphAsset graph);
        string FormatReport(GraphValidationReport report);
        string FormatIssues(IEnumerable<GraphValidationIssue> issues);
    }

    internal sealed class GraphTwcValidationService : IGraphTwcValidationService
    {
        public GraphTwcValidationResult Validate(GraphAsset graph)
        {
            var report = new GraphValidator().ValidateDetailed(graph);
            var result = new GraphTwcValidationResult
            {
                Report = report,
                ErrorCount = CountIssues(report, ValidationSeverity.Error),
                WarningCount = CountIssues(report, ValidationSeverity.Warning)
            };

            if (report == null)
                return result;

            foreach (var issue in report.Issues)
            {
                if (issue == null || issue.Severity != ValidationSeverity.Error)
                    continue;
                if (string.IsNullOrEmpty(issue.LayerId))
                    result.GlobalErrors.Add(issue);
                else
                    result.SkippedLayerIds.Add(issue.LayerId);
            }

            return result;
        }

        public string FormatReport(GraphValidationReport report)
        {
            return report == null || report.Issues.Count == 0
                ? string.Empty
                : FormatIssues(report.Issues);
        }

        public string FormatIssues(IEnumerable<GraphValidationIssue> issues)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var issue in issues)
                builder.AppendLine($"  - {issue}");
            return builder.ToString();
        }

        private static int CountIssues(GraphValidationReport report, ValidationSeverity severity)
        {
            if (report?.Issues == null)
                return 0;

            int count = 0;
            foreach (var issue in report.Issues)
            {
                if (issue != null && issue.Severity == severity)
                    count++;
            }

            return count;
        }
    }
}
