using System.Collections.Generic;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public sealed class GraphExecutionResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }
        public string ErrorNodeId { get; }
        public IReadOnlyList<NodeExecutionLog> Logs => _logs;

        private readonly Dictionary<string, object[]> _nodeOutputs;
        private readonly List<NodeExecutionLog> _logs;

        internal GraphExecutionResult(Dictionary<string, object[]> nodeOutputs,
            List<NodeExecutionLog> logs)
        {
            _nodeOutputs = nodeOutputs;
            _logs = logs;
            Success = true;
        }

        internal GraphExecutionResult(string errorNodeId, string errorMessage,
            List<NodeExecutionLog> logs,
            Dictionary<string, object[]> partialNodeOutputs = null)
        {
            _nodeOutputs = partialNodeOutputs != null
                ? new Dictionary<string, object[]>(partialNodeOutputs)
                : new Dictionary<string, object[]>();
            _logs = logs;
            Success = false;
            ErrorNodeId = errorNodeId;
            ErrorMessage = errorMessage;
        }

        public object[] GetOutputs(string nodeId) =>
            _nodeOutputs.TryGetValue(nodeId, out var outputs) ? outputs : null;

        public T GetOutput<T>(string nodeId, int portIndex = 0)
        {
            var outputs = GetOutputs(nodeId);
            if (outputs == null || portIndex >= outputs.Length) return default;
            return (T)outputs[portIndex];
        }
    }

    public sealed class NodeExecutionLog
    {
        public string NodeId { get; }
        public string NodeTitle { get; }
        public NodeStatus Status { get; }
        public string Message { get; }
        public float DurationMs { get; }
        public long AllocationBytes { get; }
        public long IterationCount { get; }

        public NodeExecutionLog(string nodeId, string nodeTitle,
            NodeStatus status, string message, float durationMs,
            long allocationBytes = 0,
            long iterationCount = 0)
        {
            NodeId = nodeId;
            NodeTitle = nodeTitle;
            Status = status;
            Message = message;
            DurationMs = durationMs;
            AllocationBytes = allocationBytes;
            IterationCount = iterationCount;
        }
    }
}
