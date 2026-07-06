using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMoyvaTwcGraphValidationService
    {
        bool CanCompile(IMoyvaTwcGraphBindingContext context, out string reason);
        GraphValidationReport Validate(GraphAsset graph);
        List<GraphValidationIssue> GetGlobalErrors(GraphValidationReport report);
        HashSet<string> GetInvalidLayerIds(GraphValidationReport report);
    }
}
