using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    public sealed class GraphExecutionPlan
    {
        private static readonly IReadOnlyList<NodeBase> EmptyNodes = System.Array.Empty<NodeBase>();
        private static readonly IReadOnlyList<string> EmptyStrings = System.Array.Empty<string>();
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<Connection>> EmptyConnections =
            new Dictionary<string, IReadOnlyList<Connection>>();

        private GraphExecutionPlan(
            GraphExecutionScope scope,
            bool success,
            string errorMessage,
            IReadOnlyList<NodeBase> nodesInExecutionOrder,
            IReadOnlyList<string> cycleNodeIds,
            IReadOnlyDictionary<string, IReadOnlyList<Connection>> incomingConnectionsByTargetId,
            IReadOnlyDictionary<string, IReadOnlyList<Connection>> outgoingConnectionsBySourceId)
        {
            Scope = scope;
            Success = success;
            ErrorMessage = errorMessage;
            NodesInExecutionOrder = nodesInExecutionOrder ?? EmptyNodes;
            ExecutionOrderNodeIds = NodesInExecutionOrder
                .Where(node => node != null)
                .Select(node => node.NodeId)
                .ToArray();
            CycleNodeIds = cycleNodeIds ?? EmptyStrings;
            IncomingConnectionsByTargetId = incomingConnectionsByTargetId ?? EmptyConnections;
            OutgoingConnectionsBySourceId = outgoingConnectionsBySourceId ?? EmptyConnections;
        }

        public GraphExecutionScope Scope { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }
        public string LayerId => Scope?.LayerId;
        public string GraphId => Scope?.GraphId;
        public IReadOnlyList<NodeBase> NodesInExecutionOrder { get; }
        public IReadOnlyList<string> ExecutionOrderNodeIds { get; }
        public IReadOnlyList<string> CycleNodeIds { get; }
        public IReadOnlyDictionary<string, IReadOnlyList<Connection>> IncomingConnectionsByTargetId { get; }
        public IReadOnlyDictionary<string, IReadOnlyList<Connection>> OutgoingConnectionsBySourceId { get; }

        public static GraphExecutionPlan Valid(
            GraphExecutionScope scope,
            IReadOnlyList<NodeBase> nodesInExecutionOrder,
            IReadOnlyDictionary<string, IReadOnlyList<Connection>> incomingConnectionsByTargetId,
            IReadOnlyDictionary<string, IReadOnlyList<Connection>> outgoingConnectionsBySourceId) =>
            new(
                scope,
                true,
                null,
                nodesInExecutionOrder,
                EmptyStrings,
                incomingConnectionsByTargetId,
                outgoingConnectionsBySourceId);

        public static GraphExecutionPlan Invalid(
            GraphExecutionScope scope,
            string errorMessage,
            IReadOnlyList<NodeBase> partialExecutionOrder = null,
            IReadOnlyList<string> cycleNodeIds = null,
            IReadOnlyDictionary<string, IReadOnlyList<Connection>> incomingConnectionsByTargetId = null,
            IReadOnlyDictionary<string, IReadOnlyList<Connection>> outgoingConnectionsBySourceId = null) =>
            new(
                scope,
                false,
                errorMessage,
                partialExecutionOrder,
                cycleNodeIds,
                incomingConnectionsByTargetId,
                outgoingConnectionsBySourceId);

        public IReadOnlyList<Connection> GetIncomingConnections(string targetNodeId)
        {
            if (string.IsNullOrEmpty(targetNodeId)
                || !IncomingConnectionsByTargetId.TryGetValue(targetNodeId, out var connections))
                return System.Array.Empty<Connection>();

            return connections;
        }

        public IReadOnlyList<Connection> GetOutgoingConnections(string sourceNodeId)
        {
            if (string.IsNullOrEmpty(sourceNodeId)
                || !OutgoingConnectionsBySourceId.TryGetValue(sourceNodeId, out var connections))
                return System.Array.Empty<Connection>();

            return connections;
        }
    }
}
