using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.Runtime;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphTwcValidationResult
    {
        public GraphValidationReport Report;
        public List<GraphValidationIssue> GlobalErrors = new();
        public HashSet<string> SkippedLayerIds = new();
        public int ErrorCount;
        public int WarningCount;

        public bool HasGlobalErrors => GlobalErrors.Count > 0;
    }
}
