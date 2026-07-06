using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphGenerationLayerIssueService
    {
        Dictionary<string, List<GraphValidationIssue>> GroupByLayer(GraphValidationReport report);
        List<GraphValidationIssue> GetGlobalIssues(GraphValidationReport report);
        string FormatIssueCodes(IReadOnlyList<GraphValidationIssue> issues);
    }
}
