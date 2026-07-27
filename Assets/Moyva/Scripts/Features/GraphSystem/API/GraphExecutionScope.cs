using System;
using System.Collections.Generic;
using System.Linq;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public sealed class GraphExecutionScope
    {
        private readonly Dictionary<string, NodeBase> _nodeById;

        public GraphExecutionScope(
            GraphAsset graph,
            string layerId,
            string graphId,
            IReadOnlyList<NodeBase> nodes,
            IReadOnlyList<Connection> connections)
        {
            Graph = graph;
            LayerId = layerId;
            GraphId = graphId;
            Nodes = nodes ?? System.Array.Empty<NodeBase>();
            Connections = connections ?? System.Array.Empty<Connection>();
            _nodeById = Nodes
                .Where(node => node != null && !string.IsNullOrEmpty(node.NodeId))
                .GroupBy(node => node.NodeId)
                .ToDictionary(group => group.Key, group => group.First());
        }

        public GraphAsset Graph { get; }
        public string LayerId { get; }
        public string GraphId { get; }
        public IReadOnlyList<NodeBase> Nodes { get; }
        public IReadOnlyList<Connection> Connections { get; }
        public GraphSharedSettings SharedSettings => Graph?.SharedSettings;

        public NodeBase GetNodeById(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return null;

            return _nodeById.TryGetValue(nodeId, out var node) ? node : null;
        }
    }
}

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    using Kruty1918.Moyva.GraphSystem.API;

    /// <summary>
    /// Immutable participation classification for one execution scope.
    /// It deliberately keeps physical Output reachability separate from
    /// authoritativeness so global/static settings never require fake ports.
    /// </summary>
    public sealed class GraphNodeParticipationAnalysis
    {
        private readonly HashSet<string> _connectedToOutput;
        private readonly HashSet<string> _globalContext;
        private readonly HashSet<string> _authoritative;

        internal GraphNodeParticipationAnalysis(
            HashSet<string> connectedToOutput,
            HashSet<string> globalContext,
            HashSet<string> authoritative)
        {
            _connectedToOutput = connectedToOutput
                ?? new HashSet<string>(StringComparer.Ordinal);
            _globalContext = globalContext
                ?? new HashSet<string>(StringComparer.Ordinal);
            _authoritative = authoritative
                ?? new HashSet<string>(StringComparer.Ordinal);
        }

        public IReadOnlyCollection<string> ConnectedToOutputNodeIds =>
            _connectedToOutput;
        public IReadOnlyCollection<string> AuthoritativeNodeIds =>
            _authoritative;

        public bool IsConnectedToOutput(string nodeId) =>
            !string.IsNullOrEmpty(nodeId)
            && _connectedToOutput.Contains(nodeId);

        public bool IsAuthoritative(string nodeId) =>
            !string.IsNullOrEmpty(nodeId)
            && _authoritative.Contains(nodeId);

        public GraphNodeParticipation GetParticipation(string nodeId)
        {
            if (!string.IsNullOrEmpty(nodeId) && _globalContext.Contains(nodeId))
                return GraphNodeParticipation.GlobalContext;

            return IsConnectedToOutput(nodeId)
                ? GraphNodeParticipation.OutputPath
                : GraphNodeParticipation.Detached;
        }
    }

    /// <summary>
    /// Single source of truth for Output reachability and global-node authority.
    /// </summary>
    public static class GraphNodeParticipationAnalyzer
    {
        public static GraphNodeParticipationAnalysis Analyze(
            GraphExecutionScope scope)
        {
            var connectedToOutput = new HashSet<string>(StringComparer.Ordinal);
            var globalContext = new HashSet<string>(StringComparer.Ordinal);
            var authoritative = new HashSet<string>(StringComparer.Ordinal);
            if (scope?.Nodes == null)
            {
                return new GraphNodeParticipationAnalysis(
                    connectedToOutput,
                    globalContext,
                    authoritative);
            }

            var nodesById = scope.Nodes
                .Where(node =>
                    node != null
                    && !string.IsNullOrEmpty(node.NodeId))
                .GroupBy(node => node.NodeId, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.Ordinal);

            foreach (var node in nodesById.Values)
            {
                if (GraphAsset.IsGlobalNode(node))
                    globalContext.Add(node.NodeId);
            }

            var stack = new Stack<string>();
            foreach (var node in nodesById.Values)
            {
                if (node is IGraphOutputNode)
                    stack.Push(node.NodeId);
            }

            // Scopes without an explicit Output are standalone evaluation
            // boundaries. Preserve their historical implicit-result behavior
            // without claiming a physical connection that does not exist.
            if (stack.Count == 0)
            {
                authoritative.UnionWith(nodesById.Keys);
            }
            else
            {
                var incomingByTarget =
                    new Dictionary<string, List<string>>(StringComparer.Ordinal);
                var connections =
                    scope.Connections ?? Array.Empty<Connection>();
                for (int i = 0; i < connections.Count; i++)
                {
                    var connection = connections[i];
                    if (connection == null
                        || !nodesById.ContainsKey(connection.SourceNodeId)
                        || !nodesById.ContainsKey(connection.TargetNodeId))
                    {
                        continue;
                    }

                    if (!incomingByTarget.TryGetValue(
                            connection.TargetNodeId,
                            out var sources))
                    {
                        sources = new List<string>();
                        incomingByTarget[connection.TargetNodeId] = sources;
                    }

                    sources.Add(connection.SourceNodeId);
                }

                while (stack.Count > 0)
                {
                    string nodeId = stack.Pop();
                    if (!connectedToOutput.Add(nodeId)
                        || !incomingByTarget.TryGetValue(
                            nodeId,
                            out var sources))
                    {
                        continue;
                    }

                    for (int i = 0; i < sources.Count; i++)
                    {
                        string sourceId = sources[i];
                        if (!string.IsNullOrEmpty(sourceId)
                            && !connectedToOutput.Contains(sourceId))
                        {
                            stack.Push(sourceId);
                        }
                    }
                }
            }

            authoritative.UnionWith(connectedToOutput);
            authoritative.UnionWith(globalContext);
            return new GraphNodeParticipationAnalysis(
                connectedToOutput,
                globalContext,
                authoritative);
        }
    }
}
