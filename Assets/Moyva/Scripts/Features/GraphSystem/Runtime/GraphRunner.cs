using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            graph?.EnsureLayerGraphStates();
            if (graph == null)
                return Execute(graph?.CreateExecutionScope(null), context);

            var scopes = graph.CreateEnabledLayerExecutionScopes();
            if (scopes == null || scopes.Count == 0)
                return Execute(graph.CreateExecutionScope(null), context);

            return ExecuteScopes(scopes, context);
        }

        public GraphExecutionResult Execute(GraphExecutionScope scope, NodeContext context)
        {
            var logs = new List<NodeExecutionLog>();
            if (context == null)
                return new GraphExecutionResult(
                    null,
                    "NodeContext is null. Graph execution cannot start.",
                    logs,
                    layerId: scope?.LayerId,
                    graphId: scope?.GraphId);

            var layerMaskRegistry = EnsureLayerMaskRegistry(context);

            var uniqueNodeError = ValidateUniqueNodes(scope, logs);
            if (uniqueNodeError != null)
                return uniqueNodeError;

            GlobalSeed.Set(context.Seed);

            var plan = TopologicalSorter.BuildPlan(scope);
            if (!plan.Success)
                return CreatePlanFailureResult(scope, plan, logs);

            LogExecutionPlan(scope, plan);

            var cache = new Dictionary<string, object[]>();

            for (int orderIndex = 0; orderIndex < plan.NodesInExecutionOrder.Count; orderIndex++)
            {
                var node = plan.NodesInExecutionOrder[orderIndex];
                context.Cancellation.ThrowIfCancellationRequested();

                var inputs = GatherInputs(node, cache, plan.IncomingConnectionsByTargetId);
                int dependencyCount = plan.GetIncomingConnections(node.NodeId).Count;
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
                    string message = FormatNodeFailure(scope, node, ex.Message);
                    logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                        NodeStatus.Error, message, sw.ElapsedMilliseconds,
                        allocOnError,
                        iterOnError,
                        scope?.LayerId,
                        scope?.GraphId,
                        orderIndex,
                        dependencyCount));
                    return new GraphExecutionResult(
                        node.NodeId,
                        message,
                        logs,
                        cache,
                        scope?.LayerId,
                        scope?.GraphId,
                        plan.ExecutionOrderNodeIds);
                }

                sw.Stop();
                long allocDelta = GetThreadAllocatedBytes() - allocBefore;
                long iterations = context.ConsumeNodeIterations();
                if (iterations <= 0)
                    iterations = EstimateIterationsFromOutputs(output?.Values);

                string outputMessage = output.Status == NodeStatus.Error
                    ? FormatNodeFailure(scope, node, output.Message)
                    : output.Message;

                logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                    output.Status, outputMessage, sw.ElapsedMilliseconds,
                    allocDelta,
                    iterations,
                    scope?.LayerId,
                    scope?.GraphId,
                    orderIndex,
                    dependencyCount));

                if (output.Status == NodeStatus.Error)
                {
                    return new GraphExecutionResult(
                        node.NodeId,
                        outputMessage,
                        logs,
                        cache,
                        scope?.LayerId,
                        scope?.GraphId,
                        plan.ExecutionOrderNodeIds);
                }

                if (output.Values != null)
                {
                    cache[node.NodeId] = output.Values;
                    CaptureLayerMask(node, output.Values, layerMaskRegistry);
                }
            }

            return new GraphExecutionResult(
                cache,
                logs,
                scope?.LayerId,
                scope?.GraphId,
                plan.ExecutionOrderNodeIds);
        }

        public async Task<GraphExecutionResult> ExecuteAsync(GraphAsset graph,
            NodeContext context)
        {
            graph?.EnsureLayerGraphStates();
            if (graph == null)
                return await ExecuteAsync(graph?.CreateExecutionScope(null), context);

            var scopes = graph.CreateEnabledLayerExecutionScopes();
            if (scopes == null || scopes.Count == 0)
                return await ExecuteAsync(graph.CreateExecutionScope(null), context);

            return await ExecuteScopesAsync(scopes, context);
        }

        private GraphExecutionResult ExecuteScopes(
            IReadOnlyList<GraphExecutionScope> scopes,
            NodeContext context)
        {
            var results = new List<GraphExecutionResult>();
            if (scopes == null || scopes.Count == 0)
                return GraphExecutionResult.Combine(results);

            for (int i = 0; i < scopes.Count; i++)
            {
                var result = Execute(scopes[i], context);
                results.Add(result);

                if (result != null && !result.Success)
                    return GraphExecutionResult.Combine(results, result.LayerId, result.GraphId);
            }

            return GraphExecutionResult.Combine(results);
        }

        private async Task<GraphExecutionResult> ExecuteScopesAsync(
            IReadOnlyList<GraphExecutionScope> scopes,
            NodeContext context)
        {
            var results = new List<GraphExecutionResult>();
            if (scopes == null || scopes.Count == 0)
                return GraphExecutionResult.Combine(results);

            for (int i = 0; i < scopes.Count; i++)
            {
                var result = await ExecuteAsync(scopes[i], context);
                results.Add(result);

                if (result != null && !result.Success)
                    return GraphExecutionResult.Combine(results, result.LayerId, result.GraphId);
            }

            return GraphExecutionResult.Combine(results);
        }

        public async Task<GraphExecutionResult> ExecuteAsync(GraphExecutionScope scope,
            NodeContext context)
        {
            var logs = new List<NodeExecutionLog>();
            if (context == null)
                return new GraphExecutionResult(
                    null,
                    "NodeContext is null. Graph execution cannot start.",
                    logs,
                    layerId: scope?.LayerId,
                    graphId: scope?.GraphId);

            var layerMaskRegistry = EnsureLayerMaskRegistry(context);

            var uniqueNodeError = ValidateUniqueNodes(scope, logs);
            if (uniqueNodeError != null)
                return uniqueNodeError;

            GlobalSeed.Set(context.Seed);

            var plan = TopologicalSorter.BuildPlan(scope);
            if (!plan.Success)
                return CreatePlanFailureResult(scope, plan, logs);

            LogExecutionPlan(scope, plan);

            var cache = new Dictionary<string, object[]>();

            for (int i = 0; i < plan.NodesInExecutionOrder.Count; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var node = plan.NodesInExecutionOrder[i];
                context.Progress?.Report((float)i / plan.NodesInExecutionOrder.Count);

                var inputs = GatherInputs(node, cache, plan.IncomingConnectionsByTargetId);
                int dependencyCount = plan.GetIncomingConnections(node.NodeId).Count;
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
                    string message = FormatNodeFailure(scope, node, ex.Message);
                    logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                        NodeStatus.Error, message, sw.ElapsedMilliseconds,
                        allocOnError,
                        iterOnError,
                        scope?.LayerId,
                        scope?.GraphId,
                        i,
                        dependencyCount));
                    return new GraphExecutionResult(
                        node.NodeId,
                        message,
                        logs,
                        cache,
                        scope?.LayerId,
                        scope?.GraphId,
                        plan.ExecutionOrderNodeIds);
                }

                sw.Stop();
                long allocDelta = GetThreadAllocatedBytes() - allocBefore;
                long iterations = context.ConsumeNodeIterations();
                if (iterations <= 0)
                    iterations = EstimateIterationsFromOutputs(output?.Values);

                string outputMessage = output.Status == NodeStatus.Error
                    ? FormatNodeFailure(scope, node, output.Message)
                    : output.Message;

                logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                    output.Status, outputMessage, sw.ElapsedMilliseconds,
                    allocDelta,
                    iterations,
                    scope?.LayerId,
                    scope?.GraphId,
                    i,
                    dependencyCount));

                if (output.Status == NodeStatus.Error)
                {
                    return new GraphExecutionResult(
                        node.NodeId,
                        outputMessage,
                        logs,
                        cache,
                        scope?.LayerId,
                        scope?.GraphId,
                        plan.ExecutionOrderNodeIds);
                }

                if (output.Values != null)
                {
                    cache[node.NodeId] = output.Values;
                    CaptureLayerMask(node, output.Values, layerMaskRegistry);
                }
            }

            context.Progress?.Report(1f);
            return new GraphExecutionResult(
                cache,
                logs,
                scope?.LayerId,
                scope?.GraphId,
                plan.ExecutionOrderNodeIds);
        }

        private object[] GatherInputs(NodeBase node,
            Dictionary<string, object[]> cache,
            IReadOnlyDictionary<string, IReadOnlyList<Connection>> connectionsByTarget)
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
                    {
                        var value = sourceOutputs[conn.SourcePortIndex];
                        var targetType = portDefs[conn.TargetPortIndex].ValueType;
                        inputs[conn.TargetPortIndex] = SelectConnectionValue(value, targetType, conn.SourceElementIndex);
                    }
                }
            }

            return inputs;
        }

        private static GraphExecutionResult CreatePlanFailureResult(
            GraphExecutionScope scope,
            GraphExecutionPlan plan,
            List<NodeExecutionLog> logs)
        {
            string message = FormatPlanningFailure(scope, plan);
            string nodeId = plan?.CycleNodeIds?.FirstOrDefault();
            var node = scope?.GetNodeById(nodeId);
            if (node != null)
            {
                logs.Add(new NodeExecutionLog(
                    node.NodeId,
                    node.Title,
                    NodeStatus.Error,
                    message,
                    0f,
                    layerId: scope?.LayerId,
                    graphId: scope?.GraphId,
                    orderIndex: -1,
                    inputDependencyCount: plan.GetIncomingConnections(node.NodeId).Count));
            }

            Debug.LogWarning("[GraphRunner] " + message);
            return new GraphExecutionResult(
                nodeId,
                message,
                logs,
                layerId: scope?.LayerId,
                graphId: scope?.GraphId,
                executionOrderNodeIds: plan?.ExecutionOrderNodeIds);
        }

        private static string FormatPlanningFailure(GraphExecutionScope scope, GraphExecutionPlan plan)
        {
            string layerName = ResolveLayerName(scope);
            string error = string.IsNullOrWhiteSpace(plan?.ErrorMessage)
                ? "Graph execution plan could not be built."
                : plan.ErrorMessage;
            return $"Layer '{layerName}' cannot build execution plan: {error}";
        }

        private static string FormatNodeFailure(GraphExecutionScope scope, NodeBase node, string message)
        {
            string layerName = ResolveLayerName(scope);
            string nodeName = node?.Title ?? "Unknown Node";
            string nodeId = ShortId(node?.NodeId);
            string details = string.IsNullOrWhiteSpace(message) ? "Unknown node execution error." : message;
            return $"Layer '{layerName}' node '{nodeName}' ({nodeId}) failed: {details}";
        }

        private static void LogExecutionPlan(GraphExecutionScope scope, GraphExecutionPlan plan)
        {
            if (plan == null || !plan.Success)
                return;

            var sb = new StringBuilder(256);
            sb.Append("[GraphRunner] Execution plan");
            sb.Append($" layer='{ResolveLayerName(scope)}'");
            sb.Append($" graph='{scope?.GraphId ?? "global"}'");
            sb.AppendLine($": {plan.NodesInExecutionOrder.Count} node(s)");

            for (int i = 0; i < plan.NodesInExecutionOrder.Count; i++)
            {
                var node = plan.NodesInExecutionOrder[i];
                int deps = plan.GetIncomingConnections(node.NodeId).Count;
                sb.AppendLine($"  {i:00}. {node.Title} ({ShortId(node.NodeId)}) deps={deps}");
            }

            Debug.Log(sb.ToString());
        }

        private static string ResolveLayerName(GraphExecutionScope scope)
        {
            if (scope == null)
                return "Unknown";
            if (string.IsNullOrEmpty(scope.LayerId))
                return "Global";

            return scope.Graph?.GetLayerById(scope.LayerId)?.Name
                ?? scope.LayerId
                ?? "Unknown";
        }

        private static string ShortId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "no-id";

            return id.Substring(0, Math.Min(8, id.Length));
        }

        private static object SelectConnectionValue(object value, Type targetType, int sourceElementIndex)
        {
            if (value == null || targetType == null)
                return value;
            if (targetType.IsInstanceOfType(value) || targetType == typeof(object))
                return value;

            return PortDefinition.TryGetIndexableValue(value, sourceElementIndex, out var element)
                ? element
                : value;
        }

        private static Dictionary<string, List<Connection>> BuildConnectionsByTarget(
            IReadOnlyList<Connection> connections)
        {
            var index = new Dictionary<string, List<Connection>>();
            if (connections == null)
                return index;

            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                if (connection == null)
                    continue;

                if (!index.TryGetValue(connection.TargetNodeId, out var list))
                {
                    list = new List<Connection>();
                    index[connection.TargetNodeId] = list;
                }

                list.Add(connection);
            }

            return index;
        }

        private static GraphExecutionResult ValidateUniqueNodes(GraphExecutionScope scope, List<NodeExecutionLog> logs)
        {
            if (scope?.Nodes == null)
                return null;

            var seen = new Dictionary<Type, NodeBase>();
            foreach (var node in scope.Nodes)
            {
                if (node == null)
                    continue;

                var nodeType = node.GetType();
                if (!Attribute.IsDefined(nodeType, typeof(UniqueNodeAttribute)))
                    continue;

                if (!seen.TryGetValue(nodeType, out var firstNode))
                {
                    seen[nodeType] = node;
                    continue;
                }

                string message = $"Graph contains multiple unique nodes of type '{nodeType.Name}'. Keep only one. First node: {firstNode.NodeId}.";
                logs.Add(new NodeExecutionLog(
                    node.NodeId,
                    node.Title,
                    NodeStatus.Error,
                    message,
                    0f,
                    layerId: scope?.LayerId,
                    graphId: scope?.GraphId));
                return new GraphExecutionResult(
                    node.NodeId,
                    message,
                    logs,
                    layerId: scope?.LayerId,
                    graphId: scope?.GraphId);
            }

            return null;
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

        private static LayerMaskRegistry EnsureLayerMaskRegistry(NodeContext context)
        {
            if (context != null && context.TryGetService<LayerMaskRegistry>(out var existing) && existing != null)
                return existing;

            var created = new LayerMaskRegistry();
            context?.RegisterService(created);
            return created;
        }

        private static void CaptureLayerMask(NodeBase node, object[] outputs, LayerMaskRegistry registry)
        {
            if (node == null || outputs == null || registry == null || string.IsNullOrEmpty(node.LayerId))
                return;

            for (int i = 0; i < outputs.Length; i++)
            {
                if (outputs[i] is bool[,] mask)
                {
                    registry.SetLatestMask(node.LayerId, mask);
                    return;
                }
            }
        }
    }
}