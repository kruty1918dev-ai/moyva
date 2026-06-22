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
