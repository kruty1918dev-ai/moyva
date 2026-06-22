using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    /// <summary>
    /// Утиліта для топологічного сортування графів (алгоритм Кана).
    /// Використовується GraphRunner для визначення порядку виконання
    /// та GraphValidator для перевірки наявності циклів.
    /// </summary>
    public static class TopologicalSorter
    {
        /// <summary>
        /// Виконує топологічне сортування вузлів графа алгоритмом Кана.
        /// Повертає відсортований список вузлів або null, якщо граф містить цикли.
        /// </summary>
        public static List<NodeBase> Sort(GraphAsset graph)
        {
            graph?.EnsureLayerGraphStates();
            return graph == null ? null : Sort(graph.CreateExecutionScope(null));
        }

        public static List<NodeBase> Sort(GraphExecutionScope scope)
        {
            var plan = BuildPlan(scope);
            return plan.Success ? plan.NodesInExecutionOrder.ToList() : null;
        }

        public static List<NodeBase> Sort(
            IReadOnlyList<NodeBase> nodes,
            IReadOnlyList<Connection> connections)
        {
            var scope = new GraphExecutionScope(null, null, null, nodes, connections);
            var plan = BuildPlan(scope);
            return plan.Success ? plan.NodesInExecutionOrder.ToList() : null;
        }

        public static GraphExecutionPlan BuildPlan(GraphExecutionScope scope)
        {
            var nodeMap = new Dictionary<string, NodeBase>();
            var inDegree = new Dictionary<string, int>();
            var adjacency = new Dictionary<string, List<string>>();
            var incomingConnections = new Dictionary<string, List<Connection>>();
            var outgoingConnections = new Dictionary<string, List<Connection>>();

            if (scope == null)
                return GraphExecutionPlan.Invalid(null, "Execution scope is null.");

            var nodes = scope.Nodes;
            var connections = scope.Connections;
            if (nodes == null || connections == null)
                return GraphExecutionPlan.Invalid(scope, "Execution scope has no node or connection collection.");

            foreach (var node in nodes.Where(node => node != null).OrderBy(node => node.NodeId, System.StringComparer.Ordinal))
            {
                if (string.IsNullOrEmpty(node.NodeId))
                    return GraphExecutionPlan.Invalid(scope, $"Node '{node.Title}' has an empty NodeId.");

                if (nodeMap.ContainsKey(node.NodeId))
                    return GraphExecutionPlan.Invalid(scope, $"Duplicate NodeId '{node.NodeId}' found.");

                nodeMap[node.NodeId] = node;
                inDegree[node.NodeId] = 0;
                adjacency[node.NodeId] = new List<string>();
                incomingConnections[node.NodeId] = new List<Connection>();
                outgoingConnections[node.NodeId] = new List<Connection>();
            }

            foreach (var conn in connections.Where(connection => connection != null)
                         .OrderBy(connection => connection.ConnectionId, System.StringComparer.Ordinal))
            {
                if (!nodeMap.ContainsKey(conn.SourceNodeId)
                    || !nodeMap.ContainsKey(conn.TargetNodeId))
                {
                    return GraphExecutionPlan.Invalid(
                        scope,
                        $"Connection '{conn.ConnectionId}' references missing node(s): source '{conn.SourceNodeId}', target '{conn.TargetNodeId}'.",
                        incomingConnectionsByTargetId: FreezeConnectionMap(incomingConnections),
                        outgoingConnectionsBySourceId: FreezeConnectionMap(outgoingConnections));
                }

                adjacency[conn.SourceNodeId].Add(conn.TargetNodeId);
                inDegree[conn.TargetNodeId]++;
                outgoingConnections[conn.SourceNodeId].Add(conn);
                incomingConnections[conn.TargetNodeId].Add(conn);
            }

            foreach (var neighbors in adjacency.Values)
                neighbors.Sort(System.StringComparer.Ordinal);

            var ready = new SortedSet<string>(System.StringComparer.Ordinal);
            foreach (var kvp in inDegree)
                if (kvp.Value == 0)
                    ready.Add(kvp.Key);

            var sorted = new List<NodeBase>();
            while (ready.Count > 0)
            {
                var current = ready.Min;
                ready.Remove(current);
                sorted.Add(nodeMap[current]);

                foreach (var neighbor in adjacency[current])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                        ready.Add(neighbor);
                }
            }

            if (sorted.Count != nodeMap.Count)
            {
                var remainingNodeIds = inDegree
                    .Where(pair => pair.Value > 0)
                    .Select(pair => pair.Key)
                    .OrderBy(id => id, System.StringComparer.Ordinal)
                    .ToHashSet();
                var cycleNodeIds = FindCycleNodeIds(adjacency, remainingNodeIds);
                return GraphExecutionPlan.Invalid(
                    scope,
                    FormatCycleMessage(scope, cycleNodeIds),
                    sorted,
                    cycleNodeIds,
                    FreezeConnectionMap(incomingConnections),
                    FreezeConnectionMap(outgoingConnections));
            }

            return GraphExecutionPlan.Valid(
                scope,
                sorted,
                FreezeConnectionMap(incomingConnections),
                FreezeConnectionMap(outgoingConnections));
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<Connection>> FreezeConnectionMap(
            Dictionary<string, List<Connection>> source)
        {
            var result = new Dictionary<string, IReadOnlyList<Connection>>();
            if (source == null)
                return result;

            foreach (var pair in source)
            {
                result[pair.Key] = pair.Value
                    .OrderBy(connection => connection.ConnectionId, System.StringComparer.Ordinal)
                    .ToArray();
            }

            return result;
        }

        private static IReadOnlyList<string> FindCycleNodeIds(
            Dictionary<string, List<string>> adjacency,
            HashSet<string> candidates)
        {
            var visiting = new HashSet<string>();
            var visited = new HashSet<string>();
            var stack = new List<string>();

            foreach (var nodeId in candidates.OrderBy(id => id, System.StringComparer.Ordinal))
            {
                var cycle = FindCycleDepthFirst(nodeId, adjacency, candidates, visiting, visited, stack);
                if (cycle.Count > 0)
                    return cycle;
            }

            return candidates.OrderBy(id => id, System.StringComparer.Ordinal).ToArray();
        }

        private static IReadOnlyList<string> FindCycleDepthFirst(
            string nodeId,
            Dictionary<string, List<string>> adjacency,
            HashSet<string> candidates,
            HashSet<string> visiting,
            HashSet<string> visited,
            List<string> stack)
        {
            if (visited.Contains(nodeId))
                return System.Array.Empty<string>();

            if (visiting.Contains(nodeId))
            {
                int start = stack.IndexOf(nodeId);
                if (start < 0)
                    return new[] { nodeId };

                var cycle = stack.Skip(start).ToList();
                cycle.Add(nodeId);
                return cycle;
            }

            visiting.Add(nodeId);
            stack.Add(nodeId);

            if (adjacency.TryGetValue(nodeId, out var neighbors))
            {
                foreach (var neighbor in neighbors.Where(candidates.Contains).OrderBy(id => id, System.StringComparer.Ordinal))
                {
                    var cycle = FindCycleDepthFirst(neighbor, adjacency, candidates, visiting, visited, stack);
                    if (cycle.Count > 0)
                        return cycle;
                }
            }

            stack.RemoveAt(stack.Count - 1);
            visiting.Remove(nodeId);
            visited.Add(nodeId);
            return System.Array.Empty<string>();
        }

        private static string FormatCycleMessage(GraphExecutionScope scope, IReadOnlyList<string> cycleNodeIds)
        {
            if (cycleNodeIds == null || cycleNodeIds.Count == 0)
                return "Graph contains a dependency cycle.";

            string path = string.Join(" -> ", cycleNodeIds.Select(nodeId =>
            {
                var node = scope?.GetNodeById(nodeId);
                string title = node?.Title ?? "Missing Node";
                string shortId = string.IsNullOrEmpty(nodeId)
                    ? "no-id"
                    : nodeId.Substring(0, System.Math.Min(8, nodeId.Length));
                return $"{title} ({shortId})";
            }));

            return "Graph contains a dependency cycle: " + path;
        }
    }
}
