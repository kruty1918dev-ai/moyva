using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    /// <summary>
    /// Виконавець графа генерації: обходить вузли у топологічному порядку,
    /// передає входи/виходи між вузлами через кеш та підтримує як
    /// синхронний, так і асинхронний режими виконання.
    /// </summary>
    public sealed class GraphRunner : IGraphRunner
    {
        public GraphExecutionResult Execute(GraphAsset graph, NodeContext context)
        {
            var logs = new List<NodeExecutionLog>();
            var cache = new Dictionary<string, object[]>();
            var connectionsByTarget = BuildConnectionsByTarget(graph);

            var sorted = TopologicalSorter.Sort(graph);
            if (sorted == null)
                return new GraphExecutionResult(null,
                    "Graph contains cycles — topological sort is impossible.", logs);

            foreach (var node in sorted)
            {
                context.Cancellation.ThrowIfCancellationRequested();

                var inputs = GatherInputs(node, cache, connectionsByTarget);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                long allocBefore = GetThreadAllocatedBytes();
                context.ResetNodeProfiling();

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
                    long allocOnError = GetThreadAllocatedBytes() - allocBefore;
                    long iterOnError = context.ConsumeNodeIterations();
                    logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                        NodeStatus.Error, ex.Message, sw.ElapsedMilliseconds,
                        allocOnError,
                        iterOnError));
                    return new GraphExecutionResult(node.NodeId, ex.Message, logs);
                }

                sw.Stop();
                long allocDelta = GetThreadAllocatedBytes() - allocBefore;
                long iterations = context.ConsumeNodeIterations();
                if (iterations <= 0)
                    iterations = EstimateIterationsFromOutputs(output?.Values);

                logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                    output.Status, output.Message, sw.ElapsedMilliseconds,
                    allocDelta,
                    iterations));

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

            var sorted = TopologicalSorter.Sort(graph);
            if (sorted == null)
                return new GraphExecutionResult(null,
                    "Graph contains cycles — topological sort is impossible.", logs);

            for (int i = 0; i < sorted.Count; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var node = sorted[i];
                context.Progress?.Report((float)i / sorted.Count);

                var inputs = GatherInputs(node, cache, connectionsByTarget);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                long allocBefore = GetThreadAllocatedBytes();
                context.ResetNodeProfiling();

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
                    long allocOnError = GetThreadAllocatedBytes() - allocBefore;
                    long iterOnError = context.ConsumeNodeIterations();
                    logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                        NodeStatus.Error, ex.Message, sw.ElapsedMilliseconds,
                        allocOnError,
                        iterOnError));
                    return new GraphExecutionResult(node.NodeId, ex.Message, logs);
                }

                sw.Stop();
                long allocDelta = GetThreadAllocatedBytes() - allocBefore;
                long iterations = context.ConsumeNodeIterations();
                if (iterations <= 0)
                    iterations = EstimateIterationsFromOutputs(output?.Values);

                logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                    output.Status, output.Message, sw.ElapsedMilliseconds,
                    allocDelta,
                    iterations));

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

        private static long GetThreadAllocatedBytes()
        {
            try
            {
                return GC.GetAllocatedBytesForCurrentThread();
            }
            catch
            {
                return 0;
            }
        }

        private static long EstimateIterationsFromOutputs(object[] values)
        {
            if (values == null || values.Length == 0)
                return 0;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is Array a && a.Rank == 2)
                    return (long)a.GetLength(0) * a.GetLength(1);
            }

            return 0;
        }
    }
}
