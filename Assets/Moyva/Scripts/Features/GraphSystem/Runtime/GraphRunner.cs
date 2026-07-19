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

            var outputCardinalityError =
                ValidateLayerOutputCardinality(scope, logs);
            if (outputCardinalityError != null)
                return outputCardinalityError;

            var layerMaskRegistry = EnsureLayerMaskRegistry(context);
            var connectedToOutput = BuildOutputReachability(scope);

            var uniqueNodeError = ValidateUniqueNodes(scope, logs, connectedToOutput);
            if (uniqueNodeError != null)
                return uniqueNodeError;

            using var randomScope = new GraphRandomScope(context.Seed);

            var plan = BuildExecutionPlan(scope, connectedToOutput, logs);
            if (!plan.Success)
                return CreatePlanFailureResult(scope, plan, logs);

            LogExecutionPlan(scope, plan);

            var cache = new Dictionary<string, object[]>();
            var artifacts = new Dictionary<string, object>();

            for (int orderIndex = 0; orderIndex < plan.NodesInExecutionOrder.Count; orderIndex++)
            {
                var node = plan.NodesInExecutionOrder[orderIndex];
                bool isConnectedToOutput = connectedToOutput.Contains(node.NodeId);
                context.Cancellation.ThrowIfCancellationRequested();

                int dependencyCount = plan.GetIncomingConnections(node.NodeId).Count;
                if (!TryGatherInputs(
                        node,
                        cache,
                        plan.IncomingConnectionsByTargetId,
                        context,
                        out var inputs,
                        out var inputError))
                {
                    string message = FormatNodeFailure(scope, node, inputError);
                    if (!isConnectedToOutput)
                        message = AppendMessage(message, "Not connected to Output.");
                    logs.Add(new NodeExecutionLog(
                        node.NodeId,
                        node.Title,
                        isConnectedToOutput ? NodeStatus.Error : NodeStatus.Warning,
                        message,
                        0f,
                        layerId: scope?.LayerId,
                        graphId: scope?.GraphId,
                        orderIndex: orderIndex,
                        inputDependencyCount: dependencyCount,
                        isConnectedToOutput: isConnectedToOutput));
                    if (!isConnectedToOutput)
                        continue;

                    return new GraphExecutionResult(
                        node.NodeId,
                        message,
                        logs,
                        cache,
                        scope?.LayerId,
                        scope?.GraphId,
                        plan.ExecutionOrderNodeIds,
                        artifacts);
                }

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
                    if (!isConnectedToOutput)
                        message = AppendMessage(message, "Not connected to Output.");
                    logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                        isConnectedToOutput ? NodeStatus.Error : NodeStatus.Warning,
                        message, sw.ElapsedMilliseconds,
                        allocOnError,
                        iterOnError,
                        scope?.LayerId,
                        scope?.GraphId,
                        orderIndex,
                        dependencyCount,
                        isConnectedToOutput));
                    if (!isConnectedToOutput)
                        continue;

                    return new GraphExecutionResult(
                        node.NodeId,
                        message,
                        logs,
                        cache,
                        scope?.LayerId,
                        scope?.GraphId,
                        plan.ExecutionOrderNodeIds,
                        artifacts);
                }

                sw.Stop();
                long allocDelta = GetThreadAllocatedBytes() - allocBefore;
                long iterations = context.ConsumeNodeIterations();
                if (iterations <= 0)
                    iterations = EstimateIterationsFromOutputs(output?.Values);

                if (output == null)
                    output = NodeOutput.Error("Node returned null instead of NodeOutput.");

                if (output.Status != NodeStatus.Error
                    && !TryValidateOutput(node, output, context, out var contractError))
                {
                    output = NodeOutput.Error(contractError);
                }

                string outputMessage = output.Status == NodeStatus.Error
                    ? FormatNodeFailure(scope, node, output.Message)
                    : output.Message;
                NodeStatus logStatus = output.Status;
                if (!isConnectedToOutput)
                    logStatus = NodeStatus.Warning;
                if (!isConnectedToOutput)
                    outputMessage = AppendMessage(outputMessage, "Not connected to Output.");

                logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                    logStatus, outputMessage, sw.ElapsedMilliseconds,
                    allocDelta,
                    iterations,
                    scope?.LayerId,
                    scope?.GraphId,
                    orderIndex,
                    dependencyCount,
                    isConnectedToOutput));

                if (output.Status == NodeStatus.Error)
                {
                    if (!isConnectedToOutput)
                        continue;

                    return new GraphExecutionResult(
                        node.NodeId,
                        outputMessage,
                        logs,
                        cache,
                        scope?.LayerId,
                        scope?.GraphId,
                        plan.ExecutionOrderNodeIds,
                        artifacts);
                }

                cache[node.NodeId] = output.Values;
                if (output.Artifact != null)
                    artifacts[node.NodeId] = output.Artifact;
                CaptureLayerMask(node, output.Artifact, layerMaskRegistry);
            }

            return new GraphExecutionResult(
                cache,
                logs,
                scope?.LayerId,
                scope?.GraphId,
                plan.ExecutionOrderNodeIds,
                artifacts);
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

            var outputCardinalityError =
                ValidateLayerOutputCardinality(scope, logs);
            if (outputCardinalityError != null)
                return outputCardinalityError;

            var layerMaskRegistry = EnsureLayerMaskRegistry(context);
            var connectedToOutput = BuildOutputReachability(scope);

            var uniqueNodeError = ValidateUniqueNodes(scope, logs, connectedToOutput);
            if (uniqueNodeError != null)
                return uniqueNodeError;

            using var randomScope = new GraphRandomScope(context.Seed);

            var plan = BuildExecutionPlan(scope, connectedToOutput, logs);
            if (!plan.Success)
                return CreatePlanFailureResult(scope, plan, logs);

            LogExecutionPlan(scope, plan);

            var cache = new Dictionary<string, object[]>();
            var artifacts = new Dictionary<string, object>();

            for (int i = 0; i < plan.NodesInExecutionOrder.Count; i++)
            {
                await YieldControlAsync(randomScope);
                context.Cancellation.ThrowIfCancellationRequested();
                var node = plan.NodesInExecutionOrder[i];
                bool isConnectedToOutput = connectedToOutput.Contains(node.NodeId);
                context.Progress?.Report((float)i / plan.NodesInExecutionOrder.Count);

                int dependencyCount = plan.GetIncomingConnections(node.NodeId).Count;
                if (!TryGatherInputs(
                        node,
                        cache,
                        plan.IncomingConnectionsByTargetId,
                        context,
                        out var inputs,
                        out var inputError))
                {
                    string message = FormatNodeFailure(scope, node, inputError);
                    if (!isConnectedToOutput)
                        message = AppendMessage(message, "Not connected to Output.");
                    logs.Add(new NodeExecutionLog(
                        node.NodeId,
                        node.Title,
                        isConnectedToOutput ? NodeStatus.Error : NodeStatus.Warning,
                        message,
                        0f,
                        layerId: scope?.LayerId,
                        graphId: scope?.GraphId,
                        orderIndex: i,
                        inputDependencyCount: dependencyCount,
                        isConnectedToOutput: isConnectedToOutput));
                    if (!isConnectedToOutput)
                        continue;

                    return new GraphExecutionResult(
                        node.NodeId,
                        message,
                        logs,
                        cache,
                        scope?.LayerId,
                        scope?.GraphId,
                        plan.ExecutionOrderNodeIds,
                        artifacts);
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                long allocBefore = GetThreadAllocatedBytes();
                context.ResetNodeProfiling();

                NodeOutput output;
                try
                {
                    if (node is IAsyncNode asyncNode)
                    {
                        output = await ExecuteAsyncNodeScoped(
                            asyncNode,
                            inputs,
                            context,
                            randomScope);
                    }
                    else
                        output = node.Execute(inputs, context);
                }
                catch (Exception ex)
                {
                    long allocOnError = GetThreadAllocatedBytes() - allocBefore;
                    long iterOnError = context.ConsumeNodeIterations();
                    string message = FormatNodeFailure(scope, node, ex.Message);
                    if (!isConnectedToOutput)
                        message = AppendMessage(message, "Not connected to Output.");
                    logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                        isConnectedToOutput ? NodeStatus.Error : NodeStatus.Warning,
                        message, sw.ElapsedMilliseconds,
                        allocOnError,
                        iterOnError,
                        scope?.LayerId,
                        scope?.GraphId,
                        i,
                        dependencyCount,
                        isConnectedToOutput));
                    if (!isConnectedToOutput)
                        continue;

                    return new GraphExecutionResult(
                        node.NodeId,
                        message,
                        logs,
                        cache,
                        scope?.LayerId,
                        scope?.GraphId,
                        plan.ExecutionOrderNodeIds,
                        artifacts);
                }

                sw.Stop();
                long allocDelta = GetThreadAllocatedBytes() - allocBefore;
                long iterations = context.ConsumeNodeIterations();
                if (iterations <= 0)
                    iterations = EstimateIterationsFromOutputs(output?.Values);

                if (output == null)
                    output = NodeOutput.Error("Node returned null instead of NodeOutput.");

                if (output.Status != NodeStatus.Error
                    && !TryValidateOutput(node, output, context, out var contractError))
                {
                    output = NodeOutput.Error(contractError);
                }

                string outputMessage = output.Status == NodeStatus.Error
                    ? FormatNodeFailure(scope, node, output.Message)
                    : output.Message;
                NodeStatus logStatus = output.Status;
                if (!isConnectedToOutput)
                    logStatus = NodeStatus.Warning;
                if (!isConnectedToOutput)
                    outputMessage = AppendMessage(outputMessage, "Not connected to Output.");

                logs.Add(new NodeExecutionLog(node.NodeId, node.Title,
                    logStatus, outputMessage, sw.ElapsedMilliseconds,
                    allocDelta,
                    iterations,
                    scope?.LayerId,
                    scope?.GraphId,
                    i,
                    dependencyCount,
                    isConnectedToOutput));

                if (output.Status == NodeStatus.Error)
                {
                    if (!isConnectedToOutput)
                        continue;

                    return new GraphExecutionResult(
                        node.NodeId,
                        outputMessage,
                        logs,
                        cache,
                        scope?.LayerId,
                        scope?.GraphId,
                        plan.ExecutionOrderNodeIds,
                        artifacts);
                }

                cache[node.NodeId] = output.Values;
                if (output.Artifact != null)
                    artifacts[node.NodeId] = output.Artifact;
                CaptureLayerMask(node, output.Artifact, layerMaskRegistry);
            }

            context.Progress?.Report(1f);
            return new GraphExecutionResult(
                cache,
                logs,
                scope?.LayerId,
                scope?.GraphId,
                plan.ExecutionOrderNodeIds,
                artifacts);
        }

        private static async Task YieldControlAsync(GraphRandomScope randomScope)
        {
            randomScope.Suspend();
            try
            {
                // Unity's synchronization context resumes this continuation on a
                // later editor/player update. That keeps long multi-node graphs
                // responsive without moving ScriptableObject/TWC work off-thread.
                await Task.Yield();
            }
            finally
            {
                randomScope.Resume();
            }
        }

        private static async Task<NodeOutput> ExecuteAsyncNodeScoped(
            IAsyncNode node,
            object[] inputs,
            NodeContext context,
            GraphRandomScope randomScope)
        {
            Task<NodeOutput> task = node.ExecuteAsync(inputs, context);
            if (task == null)
                return null;
            if (task.IsCompleted)
                return await task;

            // A genuinely asynchronous node must not leak the graph's global
            // random session while Unity processes other editor/player work.
            // Deterministic nodes should use NodeContext random after an await.
            randomScope.Suspend();
            try
            {
                return await task;
            }
            finally
            {
                randomScope.Resume();
            }
        }

        private static bool TryGatherInputs(
            NodeBase node,
            Dictionary<string, object[]> cache,
            IReadOnlyDictionary<string, IReadOnlyList<Connection>> connectionsByTarget,
            NodeContext context,
            out object[] inputs,
            out string error)
        {
            var portDefs = node?.Inputs ?? Array.Empty<PortDefinition>();
            inputs = new object[portDefs.Length];
            error = null;
            var assigned = new bool[portDefs.Length];

            if (node != null
                && connectionsByTarget != null
                && connectionsByTarget.TryGetValue(node.NodeId, out var incoming))
            {
                for (int c = 0; c < incoming.Count; c++)
                {
                    var connection = incoming[c];
                    if (connection == null)
                        continue;
                    if (connection.TargetPortIndex < 0 || connection.TargetPortIndex >= inputs.Length)
                    {
                        error = $"Connection '{connection.ConnectionId}' targets missing input index {connection.TargetPortIndex}.";
                        return false;
                    }
                    if (assigned[connection.TargetPortIndex])
                    {
                        error = $"Input '{portDefs[connection.TargetPortIndex].Name}' has more than one connection.";
                        return false;
                    }
                    if (!cache.TryGetValue(connection.SourceNodeId, out var sourceOutputs))
                    {
                        error = $"Input '{portDefs[connection.TargetPortIndex].Name}' cannot read source node '{connection.SourceNodeId}'.";
                        return false;
                    }
                    if (connection.SourcePortIndex < 0 || connection.SourcePortIndex >= sourceOutputs.Length)
                    {
                        error = $"Connection '{connection.ConnectionId}' reads missing output index {connection.SourcePortIndex}.";
                        return false;
                    }

                    var targetPort = portDefs[connection.TargetPortIndex];
                    if (!TrySelectConnectionValue(
                            sourceOutputs[connection.SourcePortIndex],
                            targetPort,
                            connection.SourceElementIndex,
                            out var value,
                            out error))
                    {
                        error = $"Input '{targetPort.Name}': {error}";
                        return false;
                    }
                    if (!TryValidatePortValue(targetPort, value, context, out error))
                    {
                        error = $"Input '{targetPort.Name}': {error}";
                        return false;
                    }

                    inputs[connection.TargetPortIndex] = value;
                    assigned[connection.TargetPortIndex] = true;
                }
            }

            for (int i = 0; i < portDefs.Length; i++)
            {
                if (portDefs[i] == null)
                {
                    error = $"Input definition at index {i} is null.";
                    return false;
                }
                if (portDefs[i].IsRequired && !assigned[i])
                {
                    error = $"Required input '{portDefs[i].Name}' ({portDefs[i].Id}) is not connected.";
                    return false;
                }
            }

            return true;
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

        private static bool TrySelectConnectionValue(
            object sourceValue,
            PortDefinition targetPort,
            int sourceElementIndex,
            out object value,
            out string error)
        {
            value = sourceValue;
            error = null;
            if (targetPort == null)
            {
                error = "Target port definition is null.";
                return false;
            }
            if (sourceValue == null || targetPort.AcceptsAnyValue || targetPort.ValueType.IsInstanceOfType(sourceValue))
                return true;

            if (!PortDefinition.TryGetIndexableValue(sourceValue, sourceElementIndex, out var element))
            {
                error =
                    $"Value '{sourceValue.GetType().Name}' is not assignable to '{targetPort.ValueType.Name}' " +
                    $"and element index {sourceElementIndex} cannot be resolved.";
                return false;
            }
            if (element != null && !targetPort.ValueType.IsInstanceOfType(element))
            {
                error =
                    $"Element {sourceElementIndex} has type '{element.GetType().Name}', " +
                    $"expected '{targetPort.ValueType.Name}'.";
                return false;
            }

            value = element;
            return true;
        }

        private static bool TryValidateOutput(
            NodeBase node,
            NodeOutput output,
            NodeContext context,
            out string error)
        {
            error = null;
            if (node == null)
            {
                error = "Node is null.";
                return false;
            }
            if (output == null)
            {
                error = "Node returned null instead of NodeOutput.";
                return false;
            }

            var definitions = node.Outputs ?? Array.Empty<PortDefinition>();
            var values = output.Values ?? Array.Empty<object>();
            if (values.Length != definitions.Length)
            {
                error =
                    $"Output contract mismatch: node declares {definitions.Length} port(s) " +
                    $"but returned {values.Length} value(s).";
                return false;
            }

            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                if (definition == null)
                {
                    error = $"Output definition at index {i} is null.";
                    return false;
                }
                if (!TryValidatePortValue(definition, values[i], context, out var portError))
                {
                    error = $"Output '{definition.Name}' ({definition.Id}): {portError}";
                    return false;
                }
            }

            if (output.Artifact is ILayerMaskArtifact layerArtifact
                && layerArtifact.LayerMask != null)
            {
                var mask = layerArtifact.LayerMask;
                int expectedWidth = Math.Max(1, context?.MapSize.x ?? 0);
                int expectedHeight = Math.Max(1, context?.MapSize.y ?? 0);
                if (mask.GetLength(0) != expectedWidth
                    || mask.GetLength(1) != expectedHeight)
                {
                    error =
                        $"Layer output artifact map size is " +
                        $"{mask.GetLength(0)}x{mask.GetLength(1)}, " +
                        $"expected {expectedWidth}x{expectedHeight}.";
                    return false;
                }
            }

            return true;
        }

        private static bool TryValidatePortValue(
            PortDefinition port,
            object value,
            NodeContext context,
            out string error)
        {
            error = null;
            if (port == null)
            {
                error = "Port definition is null.";
                return false;
            }
            if (value == null)
            {
                if (port.AllowNull || (!port.IsRequired && port.Direction == PortDirection.Input))
                    return true;

                error = "Value is null.";
                return false;
            }
            if (!port.AcceptsAnyValue && !port.ValueType.IsInstanceOfType(value))
            {
                error = $"Runtime type is '{value.GetType().Name}', expected '{port.ValueType.Name}'.";
                return false;
            }

            if (port.MapSizePolicy == PortMapSizePolicy.MatchContext
                && value is Array map
                && map.Rank == 2)
            {
                int expectedWidth = Math.Max(1, context?.MapSize.x ?? 0);
                int expectedHeight = Math.Max(1, context?.MapSize.y ?? 0);
                if (map.GetLength(0) != expectedWidth || map.GetLength(1) != expectedHeight)
                {
                    error =
                        $"Map size is {map.GetLength(0)}x{map.GetLength(1)}, " +
                        $"expected {expectedWidth}x{expectedHeight}.";
                    return false;
                }
            }

            return true;
        }

        private static GraphExecutionPlan BuildExecutionPlan(
            GraphExecutionScope scope,
            HashSet<string> connectedToOutput,
            List<NodeExecutionLog> logs)
        {
            if (scope?.Nodes == null)
                return TopologicalSorter.BuildPlan(scope);

            var validNodes = scope.Nodes
                .Where(node => node != null && !string.IsNullOrEmpty(node.NodeId))
                .ToList();
            if (connectedToOutput == null
                || connectedToOutput.Count >= validNodes.Count)
            {
                return TopologicalSorter.BuildPlan(scope);
            }

            var nodeIds = new HashSet<string>(
                validNodes.Select(node => node.NodeId),
                StringComparer.Ordinal);
            var authoritativeNodes = validNodes
                .Where(node => connectedToOutput.Contains(node.NodeId))
                .ToList();
            var detachedNodes = validNodes
                .Where(node => !connectedToOutput.Contains(node.NodeId))
                .ToList();

            var authoritativeConnections = new List<Connection>();
            var detachedConnections = new List<Connection>();
            var validConnections = new List<Connection>();
            var connections = scope.Connections ?? Array.Empty<Connection>();
            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                if (connection == null)
                    continue;

                bool sourceExists = nodeIds.Contains(connection.SourceNodeId);
                bool targetExists = nodeIds.Contains(connection.TargetNodeId);
                if (!sourceExists || !targetExists)
                {
                    bool touchesAuthoritative =
                        (sourceExists && connectedToOutput.Contains(connection.SourceNodeId))
                        || (targetExists && connectedToOutput.Contains(connection.TargetNodeId));
                    if (touchesAuthoritative)
                        return TopologicalSorter.BuildPlan(scope);

                    AddDetachedPlanningWarning(
                        scope,
                        scope.GetNodeById(connection.TargetNodeId)
                        ?? scope.GetNodeById(connection.SourceNodeId),
                        $"Connection '{connection.ConnectionId}' references a missing node.",
                        logs);
                    continue;
                }

                validConnections.Add(connection);
                bool sourceAuthoritative = connectedToOutput.Contains(connection.SourceNodeId);
                bool targetAuthoritative = connectedToOutput.Contains(connection.TargetNodeId);
                if (sourceAuthoritative && targetAuthoritative)
                    authoritativeConnections.Add(connection);
                else if (!sourceAuthoritative && !targetAuthoritative)
                    detachedConnections.Add(connection);
            }

            var authoritativeScope = new GraphExecutionScope(
                scope.Graph,
                scope.LayerId,
                scope.GraphId,
                authoritativeNodes,
                authoritativeConnections);
            var authoritativePlan = TopologicalSorter.BuildPlan(authoritativeScope);
            if (!authoritativePlan.Success)
                return authoritativePlan;

            var detachedScope = new GraphExecutionScope(
                scope.Graph,
                scope.LayerId,
                scope.GraphId,
                detachedNodes,
                detachedConnections);
            var detachedPlan = TopologicalSorter.BuildPlan(detachedScope);
            var detachedOrder = detachedPlan.NodesInExecutionOrder
                .Where(node => node != null)
                .ToList();

            if (!detachedPlan.Success)
            {
                var executableIds = new HashSet<string>(
                    detachedOrder.Select(node => node.NodeId),
                    StringComparer.Ordinal);
                for (int i = 0; i < detachedNodes.Count; i++)
                {
                    var blockedNode = detachedNodes[i];
                    if (blockedNode == null || executableIds.Contains(blockedNode.NodeId))
                        continue;

                    AddDetachedPlanningWarning(
                        scope,
                        blockedNode,
                        detachedPlan.ErrorMessage
                        ?? "Detached branch could not build an execution plan.",
                        logs);
                }
            }

            // Authoritative nodes run first so an unconnected preview branch can
            // never consume shared random state before the real layer result.
            var executionOrder = authoritativePlan.NodesInExecutionOrder
                .Concat(detachedOrder)
                .ToList();
            BuildConnectionMaps(
                executionOrder,
                validConnections,
                out var incoming,
                out var outgoing);
            return GraphExecutionPlan.Valid(
                scope,
                executionOrder,
                incoming,
                outgoing);
        }

        private static HashSet<string> BuildOutputReachability(GraphExecutionScope scope)
        {
            var reachable = new HashSet<string>(StringComparer.Ordinal);
            if (scope?.Nodes == null)
                return reachable;

            var nodesById = scope.Nodes
                .Where(node => node != null && !string.IsNullOrEmpty(node.NodeId))
                .GroupBy(node => node.NodeId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            var stack = new Stack<string>();
            foreach (var node in nodesById.Values)
            {
                if (node is IGraphOutputNode)
                    stack.Push(node.NodeId);
            }

            // Standalone test/subgraph scopes without an explicit Output keep their
            // historical behavior: every node is considered part of the result.
            if (stack.Count == 0)
            {
                reachable.UnionWith(nodesById.Keys);
                return reachable;
            }

            var incomingByTarget = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            var connections = scope.Connections ?? Array.Empty<Connection>();
            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                if (connection == null
                    || !nodesById.ContainsKey(connection.SourceNodeId)
                    || !nodesById.ContainsKey(connection.TargetNodeId))
                {
                    continue;
                }

                if (!incomingByTarget.TryGetValue(connection.TargetNodeId, out var sources))
                {
                    sources = new List<string>();
                    incomingByTarget[connection.TargetNodeId] = sources;
                }

                sources.Add(connection.SourceNodeId);
            }

            while (stack.Count > 0)
            {
                string nodeId = stack.Pop();
                if (!reachable.Add(nodeId))
                    continue;

                if (!incomingByTarget.TryGetValue(nodeId, out var sources))
                    continue;
                for (int i = 0; i < sources.Count; i++)
                {
                    string sourceId = sources[i];
                    if (!string.IsNullOrEmpty(sourceId) && !reachable.Contains(sourceId))
                        stack.Push(sourceId);
                }
            }

            return reachable;
        }

        private static void BuildConnectionMaps(
            IReadOnlyList<NodeBase> executionOrder,
            IReadOnlyList<Connection> connections,
            out IReadOnlyDictionary<string, IReadOnlyList<Connection>> incoming,
            out IReadOnlyDictionary<string, IReadOnlyList<Connection>> outgoing)
        {
            var includedIds = new HashSet<string>(
                executionOrder
                    .Where(node => node != null)
                    .Select(node => node.NodeId),
                StringComparer.Ordinal);
            var incomingMutable = includedIds.ToDictionary(
                id => id,
                _ => new List<Connection>(),
                StringComparer.Ordinal);
            var outgoingMutable = includedIds.ToDictionary(
                id => id,
                _ => new List<Connection>(),
                StringComparer.Ordinal);

            if (connections != null)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    var connection = connections[i];
                    if (connection == null
                        || !includedIds.Contains(connection.SourceNodeId)
                        || !includedIds.Contains(connection.TargetNodeId))
                    {
                        continue;
                    }

                    incomingMutable[connection.TargetNodeId].Add(connection);
                    outgoingMutable[connection.SourceNodeId].Add(connection);
                }
            }

            incoming = incomingMutable.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<Connection>)pair.Value
                    .OrderBy(connection => connection.ConnectionId, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);
            outgoing = outgoingMutable.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<Connection>)pair.Value
                    .OrderBy(connection => connection.ConnectionId, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);
        }

        private static void AddDetachedPlanningWarning(
            GraphExecutionScope scope,
            NodeBase node,
            string details,
            List<NodeExecutionLog> logs)
        {
            if (logs == null)
                return;

            string message = AppendMessage(
                string.IsNullOrWhiteSpace(details)
                    ? "Detached branch was not evaluated."
                    : details,
                "Not connected to Output.");
            logs.Add(new NodeExecutionLog(
                node?.NodeId,
                node?.Title ?? "Detached Node",
                NodeStatus.Warning,
                message,
                0f,
                layerId: scope?.LayerId,
                graphId: scope?.GraphId,
                orderIndex: -1,
                isConnectedToOutput: false));
        }

        private static string AppendMessage(string message, string suffix)
        {
            if (string.IsNullOrWhiteSpace(message))
                return suffix;
            if (string.IsNullOrWhiteSpace(suffix))
                return message;
            return message.TrimEnd() + " " + suffix;
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

        private static GraphExecutionResult ValidateUniqueNodes(
            GraphExecutionScope scope,
            List<NodeExecutionLog> logs,
            ISet<string> connectedToOutput)
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
                bool authoritative = connectedToOutput == null
                    || connectedToOutput.Contains(node.NodeId);
                if (!authoritative)
                {
                    logs.Add(new NodeExecutionLog(
                        node.NodeId,
                        node.Title,
                        NodeStatus.Warning,
                        $"Unique node '{node.Title}' is outside the authoritative graph. Not connected to Output.",
                        0f,
                        layerId: scope?.LayerId,
                        graphId: scope?.GraphId,
                        isConnectedToOutput: false));
                    continue;
                }

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

        private static GraphExecutionResult ValidateLayerOutputCardinality(
            GraphExecutionScope scope,
            List<NodeExecutionLog> logs)
        {
            // A graph-backed layer has one authoritative Output. Scopes without an
            // Output are intentionally still executable for detached node previews
            // and legacy helper layers; GraphValidator reports a missing final
            // Output when the complete graph is validated.
            if (scope?.Graph == null
                || string.IsNullOrEmpty(scope.LayerId)
                || scope.Nodes == null)
            {
                return null;
            }

            var outputNodes = scope.Nodes
                .Where(node =>
                    node is IGraphOutputNode
                    && string.Equals(
                        node.LayerId,
                        scope.LayerId,
                        StringComparison.Ordinal))
                .ToArray();
            if (outputNodes.Length <= 1)
                return null;

            var conflictingOutput = outputNodes[1];
            string message =
                $"Layer '{ResolveLayerName(scope)}' contains multiple Output nodes " +
                $"({outputNodes.Length}). Exactly one authoritative Output is allowed.";
            logs?.Add(new NodeExecutionLog(
                conflictingOutput.NodeId,
                conflictingOutput.Title,
                NodeStatus.Error,
                message,
                0f,
                layerId: scope.LayerId,
                graphId: scope.GraphId,
                isConnectedToOutput: true));
            return new GraphExecutionResult(
                conflictingOutput.NodeId,
                message,
                logs,
                layerId: scope.LayerId,
                graphId: scope.GraphId);
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

        private static void CaptureLayerMask(NodeBase node, object artifact, LayerMaskRegistry registry)
        {
            if (node == null
                || node is not IGraphOutputNode
                || artifact is not ILayerMaskArtifact layerArtifact
                || layerArtifact.LayerMask == null
                || registry == null
                || string.IsNullOrEmpty(node.LayerId))
                return;

            registry.SetLatestMask(node.LayerId, layerArtifact.LayerMask);
        }
    }
}
