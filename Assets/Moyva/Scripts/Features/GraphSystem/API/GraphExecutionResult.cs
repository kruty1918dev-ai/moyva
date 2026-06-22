using System.Collections.Generic;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public sealed class GraphExecutionResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }
        public string ErrorNodeId { get; }
        public string LayerId { get; }
        public string GraphId { get; }
        public string ErrorLayerId => Success ? null : LayerId;
        public string ErrorGraphId => Success ? null : GraphId;
        public IReadOnlyList<NodeExecutionLog> Logs => _logs;
        public IReadOnlyList<string> ExecutionOrderNodeIds => _executionOrderNodeIds;

        private readonly Dictionary<string, object[]> _nodeOutputs;
        private readonly List<NodeExecutionLog> _logs;
        private readonly IReadOnlyList<string> _executionOrderNodeIds;

        internal GraphExecutionResult(Dictionary<string, object[]> nodeOutputs,
            List<NodeExecutionLog> logs,
            string layerId = null,
            string graphId = null,
            IReadOnlyList<string> executionOrderNodeIds = null)
        {
            _nodeOutputs = nodeOutputs;
            _logs = logs;
            LayerId = layerId;
            GraphId = graphId;
            _executionOrderNodeIds = executionOrderNodeIds ?? System.Array.Empty<string>();
            Success = true;
        }

        internal GraphExecutionResult(string errorNodeId, string errorMessage,
            List<NodeExecutionLog> logs,
            Dictionary<string, object[]> partialNodeOutputs = null,
            string layerId = null,
            string graphId = null,
            IReadOnlyList<string> executionOrderNodeIds = null)
        {
            _nodeOutputs = partialNodeOutputs != null
                ? new Dictionary<string, object[]>(partialNodeOutputs)
                : new Dictionary<string, object[]>();
            _logs = logs;
            LayerId = layerId;
            GraphId = graphId;
            _executionOrderNodeIds = executionOrderNodeIds ?? System.Array.Empty<string>();
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

        /// <summary>
        /// Об'єднує результати кількох layer-scope виконань в один результат графа.
        /// Потрібно для multi-layer генерації: кожен шар виконується окремо, але
        /// превʼю, логи та runtime binding мають бачити єдиний набір виходів усіх нод.
        /// </summary>
        public static GraphExecutionResult Combine(
            IReadOnlyList<GraphExecutionResult> results,
            string layerId = null,
            string graphId = null)
        {
            var combinedOutputs = new Dictionary<string, object[]>();
            var combinedLogs = new List<NodeExecutionLog>();
            var combinedExecutionOrder = new List<string>();
            GraphExecutionResult firstFailure = null;

            if (results != null)
            {
                for (int resultIndex = 0; resultIndex < results.Count; resultIndex++)
                {
                    var result = results[resultIndex];
                    if (result == null)
                        continue;

                    if (result.Logs != null)
                        combinedLogs.AddRange(result.Logs);

                    if (result.ExecutionOrderNodeIds != null)
                    {
                        for (int idIndex = 0; idIndex < result.ExecutionOrderNodeIds.Count; idIndex++)
                        {
                            string nodeId = result.ExecutionOrderNodeIds[idIndex];
                            if (string.IsNullOrEmpty(nodeId))
                                continue;

                            combinedExecutionOrder.Add(nodeId);

                            var outputs = result.GetOutputs(nodeId);
                            if (outputs != null)
                                combinedOutputs[nodeId] = outputs;
                        }
                    }

                    if (!result.Success && firstFailure == null)
                        firstFailure = result;
                }
            }

            if (firstFailure != null)
            {
                return new GraphExecutionResult(
                    firstFailure.ErrorNodeId,
                    firstFailure.ErrorMessage,
                    combinedLogs,
                    combinedOutputs,
                    firstFailure.LayerId ?? layerId,
                    firstFailure.GraphId ?? graphId,
                    combinedExecutionOrder);
            }

            return new GraphExecutionResult(
                combinedOutputs,
                combinedLogs,
                layerId,
                graphId,
                combinedExecutionOrder);
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
        public string LayerId { get; }
        public string GraphId { get; }
        public int OrderIndex { get; }
        public int InputDependencyCount { get; }

        public NodeExecutionLog(string nodeId, string nodeTitle,
            NodeStatus status, string message, float durationMs,
            long allocationBytes = 0,
            long iterationCount = 0,
            string layerId = null,
            string graphId = null,
            int orderIndex = -1,
            int inputDependencyCount = 0)
        {
            NodeId = nodeId;
            NodeTitle = nodeTitle;
            Status = status;
            Message = message;
            DurationMs = durationMs;
            AllocationBytes = allocationBytes;
            IterationCount = iterationCount;
            LayerId = layerId;
            GraphId = graphId;
            OrderIndex = orderIndex;
            InputDependencyCount = inputDependencyCount;
        }
    }
}