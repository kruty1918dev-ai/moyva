using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphGenerationLayerIssueService
    {
        Dictionary<string, List<GraphValidationIssue>> GroupByLayer(GraphValidationReport report);
        List<GraphValidationIssue> GetGlobalIssues(GraphValidationReport report);
        string FormatIssueCodes(IReadOnlyList<GraphValidationIssue> issues);
    }

    internal sealed class GraphGenerationLayerIssueService : IGraphGenerationLayerIssueService
    {
        public Dictionary<string, List<GraphValidationIssue>> GroupByLayer(GraphValidationReport report)
        {
            var result = new Dictionary<string, List<GraphValidationIssue>>(StringComparer.Ordinal);
            if (report?.Issues == null)
                return result;

            foreach (var issue in report.Issues)
            {
                if (issue == null || string.IsNullOrEmpty(issue.LayerId))
                    continue;
                if (!result.TryGetValue(issue.LayerId, out var issues))
                    result[issue.LayerId] = issues = new List<GraphValidationIssue>();
                issues.Add(issue);
            }

            return result;
        }

        public List<GraphValidationIssue> GetGlobalIssues(GraphValidationReport report)
        {
            return report?.Issues?
                .Where(issue => issue != null && string.IsNullOrEmpty(issue.LayerId))
                .ToList() ?? new List<GraphValidationIssue>();
        }

        public string FormatIssueCodes(IReadOnlyList<GraphValidationIssue> issues)
        {
            if (issues == null || issues.Count == 0)
                return "unknown validation error";

            var builder = new StringBuilder();
            for (int i = 0; i < issues.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                builder.Append(issues[i].Code);
            }

            return builder.ToString();
        }
    }
}
