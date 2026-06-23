using System.Collections.Generic;
using System.Linq;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public sealed class GraphValidationReport
    {
        private readonly List<GraphValidationIssue> _issues = new();

        public IReadOnlyList<GraphValidationIssue> Issues => _issues;
        public bool HasErrors => _issues.Any(issue => issue.Severity == ValidationSeverity.Error);
        public int ErrorCount => _issues.Count(issue => issue.Severity == ValidationSeverity.Error);
        public int WarningCount => _issues.Count(issue => issue.Severity == ValidationSeverity.Warning);

        public void Add(GraphValidationIssue issue)
        {
            if (issue != null)
                _issues.Add(issue);
        }

        public void AddRange(IEnumerable<GraphValidationIssue> issues)
        {
            if (issues == null)
                return;

            foreach (var issue in issues)
                Add(issue);
        }
    }

    public sealed class GraphValidationIssue
    {
        public GraphValidationIssue(
            string code,
            ValidationSeverity severity,
            string message,
            string layerId = null,
            string graphId = null,
            string nodeId = null,
            string connectionId = null,
            bool canAutoFix = false)
        {
            Code = code;
            Severity = severity;
            Message = message;
            LayerId = layerId;
            GraphId = graphId;
            NodeId = nodeId;
            ConnectionId = connectionId;
            CanAutoFix = canAutoFix;
        }

        public string Code { get; }
        public ValidationSeverity Severity { get; }
        public string Message { get; }
        public string LayerId { get; }
        public string GraphId { get; }
        public string NodeId { get; }
        public string ConnectionId { get; }
        public bool CanAutoFix { get; }

        public override string ToString()
        {
            string target = !string.IsNullOrEmpty(NodeId)
                ? "Node"
                : !string.IsNullOrEmpty(ConnectionId)
                    ? "Connection"
                    : !string.IsNullOrEmpty(LayerId)
                        ? "Layer"
                        : "Graph";

            return $"[{Severity}] {Code}: {target}: {Message}";
        }
    }
}