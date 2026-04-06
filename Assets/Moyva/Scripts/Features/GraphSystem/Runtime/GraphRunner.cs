using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    public sealed class GraphRunner : IGraphRunner
    {
        public GraphExecutionResult Execute(GraphAsset graph, NodeContext context)
        {
            var logs = new List<NodeExecutionLog>();
            var cache = new Dictionary<string, object[]>();
            var connectionsByTarget = BuildConnectionsByTarget(graph);

            List<NodeBase> sorted;
            try
            {
                sorted = TopologicalSort(graph);
            }
            catch (InvalidOperationException ex)
            {
                return new GraphExecutionResult(null, ex.Message, logs);
            }

            foreach (var node in sorted)
            {
                context.Cancellation.ThrowIfCancellationRequested();

                var inputs = GatherInputs(node, cache, connectionsByTarget);
                var sw = System.Diagnostics.Stopwatch.StartNew();

                NodeOutput output;
                try
                {
                    if (node is IAsyncNode asyncNode)
                        output = asyncNode.ExecuteAsync(inputs, context)
                            .GetAwaiter().GetResult();
                    else
                        output = node.Execute(inputs, context);
                }
                catch (Exception ex)
                {
                    logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                        NodeStatus.Error, ex.Message, sw.ElapsedMilliseconds));
                    return new GraphExecutionResult(node.NodeId, ex.Message, logs);
                }

                sw.Stop();
                logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                    output.Status, output.Message, sw.ElapsedMilliseconds));

                if (output.Status == NodeStatus.Error)
                    return new GraphExecutionResult(node.NodeId, output.Message, logs);

                if (output.Values != null)
                    cache[node.NodeId] = output.Values;
            }

            return new GraphExecutionResult(cache, logs);
        }

        public async Task<GraphExecutionResult> ExecuteAsync(GraphAsset graph,
            NodeContext context)
        {
            var logs = new List<NodeExecutionLog>();
            var cache = new Dictionary<string, object[]>();
            var connectionsByTarget = BuildConnectionsByTarget(graph);

            List<NodeBase> sorted;
            try
            {
                sorted = TopologicalSort(graph);
            }
            catch (InvalidOperationException ex)
            {
                return new GraphExecutionResult(null, ex.Message, logs);
            }

            for (int i = 0; i < sorted.Count; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var node = sorted[i];
                context.Progress?.Report((float)i / sorted.Count);

                var inputs = GatherInputs(node, cache, connectionsByTarget);
                var sw = System.Diagnostics.Stopwatch.StartNew();

                NodeOutput output;
                try
                {
                    if (node is IAsyncNode asyncNode)
                        output = await asyncNode.ExecuteAsync(inputs, context);
                    else
                        output = node.Execute(inputs, context);
                }
                catch (Exception ex)
                {
                    logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                        NodeStatus.Error, ex.Message, sw.ElapsedMilliseconds));
                    return new GraphExecutionResult(node.NodeId, ex.Message, logs);
                }

                sw.Stop();
                logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                    output.Status, output.Message, sw.ElapsedMilliseconds));

                if (output.Status == NodeStatus.Error)
                    return new GraphExecutionResult(node.NodeId, output.Message, logs);

                if (output.Values != null)
                    cache[node.NodeId] = output.Values;
            }

            context.Progress?.Report(1f);
            return new GraphExecutionResult(cache, logs);
        }

        private object[] GatherInputs(NodeBase node,
            Dictionary<string, object[]> cache,
            Dictionary<string, List<Connection>> connectionsByTarget)
        {
            var portDefs = node.Inputs;
            var inputs = new object[portDefs.Length];

            if (!connectionsByTarget.TryGetValue(node.NodeId, out var incoming))
                return inputs;

            for (int c = 0; c < incoming.Count; c++)
            {
                var conn = incoming[c];

                if (cache.TryGetValue(conn.SourceNodeId, out var sourceOutputs)
                    && conn.SourcePortIndex < sourceOutputs.Length)
                {
                    if (conn.TargetPortIndex < inputs.Length)
                        inputs[conn.TargetPortIndex] = sourceOutputs[conn.SourcePortIndex];
                }
            }

            return inputs;
        }

        private static Dictionary<string, List<Connection>> BuildConnectionsByTarget(GraphAsset graph)
        {
            var index = new Dictionary<string, List<Connection>>();
            var connections = graph.Connections;

            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                if (!index.TryGetValue(connection.TargetNodeId, out var list))
                {
                    list = new List<Connection>();
                    index[connection.TargetNodeId] = list;
                }

                list.Add(connection);
            }

            return index;
        }

        /// <summary>
        /// Kahn's algorithm for topological sort with cycle detection.
        /// </summary>
        private List<NodeBase> TopologicalSort(GraphAsset graph)
        {
            var nodeMap = new Dictionary<string, NodeBase>();
            var inDegree = new Dictionary<string, int>();
            var adjacency = new Dictionary<string, List<string>>();

            foreach (var node in graph.Nodes)
            {
                if (node == null) continue;
                nodeMap[node.NodeId] = node;
                inDegree[node.NodeId] = 0;
                adjacency[node.NodeId] = new List<string>();
            }

            foreach (var conn in graph.Connections)
            {
                if (!nodeMap.ContainsKey(conn.SourceNodeId)
                    || !nodeMap.ContainsKey(conn.TargetNodeId))
                    continue;

                adjacency[conn.SourceNodeId].Add(conn.TargetNodeId);
                inDegree[conn.TargetNodeId]++;
            }

            var queue = new Queue<string>();
            foreach (var kvp in inDegree)
                if (kvp.Value == 0)
                    queue.Enqueue(kvp.Key);

            var sorted = new List<NodeBase>();
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                sorted.Add(nodeMap[current]);

                foreach (var neighbor in adjacency[current])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                        queue.Enqueue(neighbor);
                }
            }

            if (sorted.Count != nodeMap.Count)
                throw new InvalidOperationException(
                    "Graph contains cycles — topological sort is impossible.");

            return sorted;
        }
    }
}
