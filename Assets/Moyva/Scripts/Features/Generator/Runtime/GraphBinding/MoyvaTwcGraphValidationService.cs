using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MoyvaTwcGraphValidationService : IMoyvaTwcGraphValidationService
    {
        public bool CanCompile(IMoyvaTwcGraphBindingContext context, out string reason)
        {
            if (context.Manager == null)
            {
                reason = "TileWorldCreatorManager відсутній.";
                return false;
            }

            if (context.Manager.configuration == null)
            {
                reason = "TWC Configuration не задано.";
                return false;
            }

            if (context.GraphAsset == null)
            {
                reason = "GraphAsset не задано.";
                return false;
            }

            reason = null;
            return true;
        }

        public GraphValidationReport Validate(GraphAsset graph)
        {
            return graph != null ? new GraphValidator().ValidateDetailed(graph) : null;
        }

        public List<GraphValidationIssue> GetGlobalErrors(GraphValidationReport report)
        {
            var result = new List<GraphValidationIssue>();
            if (report == null)
                return result;

            foreach (var issue in report.Issues)
            {
                if (issue.Severity == ValidationSeverity.Error && string.IsNullOrEmpty(issue.LayerId))
                    result.Add(issue);
            }

            return result;
        }

        public HashSet<string> GetInvalidLayerIds(GraphValidationReport report)
        {
            var result = new HashSet<string>();
            if (report == null)
                return result;

            foreach (var issue in report.Issues)
            {
                if (issue.Severity == ValidationSeverity.Error && !string.IsNullOrEmpty(issue.LayerId))
                    result.Add(issue.LayerId);
            }

            return result;
        }
    }
}
