using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    public sealed partial class GraphRunner
    {
        public GraphExecutionResult Execute(
            GraphAsset graph,
            NodeContext context)
        {
            return ExecuteGraphCoreAsync(graph, context, asynchronous: false)
                .GetAwaiter()
                .GetResult();
        }

        public GraphExecutionResult Execute(
            GraphExecutionScope scope,
            NodeContext context)
        {
            return ExecuteScopeCoreAsync(scope, context, asynchronous: false)
                .GetAwaiter()
                .GetResult();
        }

        public Task<GraphExecutionResult> ExecuteAsync(
            GraphAsset graph,
            NodeContext context)
        {
            return ExecuteGraphCoreAsync(graph, context, asynchronous: true);
        }

        public Task<GraphExecutionResult> ExecuteAsync(
            GraphExecutionScope scope,
            NodeContext context)
        {
            return ExecuteScopeCoreAsync(scope, context, asynchronous: true);
        }

        private async Task<GraphExecutionResult> ExecuteGraphCoreAsync(
            GraphAsset graph,
            NodeContext context,
            bool asynchronous)
        {
            graph?.EnsureLayerGraphStates();
            if (graph == null)
            {
                return await ExecuteScopeCoreAsync(
                    null,
                    context,
                    asynchronous);
            }

            var scopes = graph.CreateEnabledLayerExecutionScopes();
            if (scopes == null || scopes.Count == 0)
            {
                return await ExecuteScopeCoreAsync(
                    graph.CreateExecutionScope(null),
                    context,
                    asynchronous);
            }

            var results = new List<GraphExecutionResult>();
            for (int i = 0; i < scopes.Count; i++)
            {
                var result = await ExecuteScopeCoreAsync(
                    scopes[i],
                    context,
                    asynchronous);
                results.Add(result);
                if (result != null && !result.Success)
                {
                    return GraphExecutionResult.Combine(
                        results,
                        result.LayerId,
                        result.GraphId);
                }
            }

            return GraphExecutionResult.Combine(results);
        }

        private async Task<GraphExecutionResult> ExecuteScopeCoreAsync(
            GraphExecutionScope scope,
            NodeContext context,
            bool asynchronous)
        {
            var logs = new List<NodeExecutionLog>();
            if (context == null)
            {
                return new GraphExecutionResult(
                    null,
                    "NodeContext is null. Graph execution cannot start.",
                    logs,
                    layerId: scope?.LayerId,
                    graphId: scope?.GraphId);
            }

            var cardinalityError = ValidateLayerOutputCardinality(scope, logs);
            if (cardinalityError != null)
                return cardinalityError;

            var maskRegistry = EnsureLayerMaskRegistry(context);
            var participation = GraphNodeParticipationAnalyzer.Analyze(scope);
            var uniqueNodeError = ValidateUniqueNodes(
                scope,
                logs,
                participation);
            if (uniqueNodeError != null)
                return uniqueNodeError;

            using var randomScope = new GraphRandomScope(context.Seed);
            var plan = BuildExecutionPlan(
                scope,
                participation.AuthoritativeNodeIds,
                logs);
            if (!plan.Success)
                return CreatePlanFailureResult(scope, plan, logs);

            LogExecutionPlan(scope, plan);
            var cache = new Dictionary<string, object[]>();
            var artifacts = new Dictionary<string, object>();
            int nodeCount = plan.NodesInExecutionOrder.Count;

            for (int orderIndex = 0; orderIndex < nodeCount; orderIndex++)
            {
                if (asynchronous)
                    await YieldControlAsync(randomScope);
                context.Cancellation.ThrowIfCancellationRequested();

                var node = plan.NodesInExecutionOrder[orderIndex];
                bool connectedToOutput =
                    participation.IsConnectedToOutput(node.NodeId);
                bool authoritative =
                    participation.IsAuthoritative(node.NodeId);
                var nodeParticipation =
                    participation.GetParticipation(node.NodeId);
                if (asynchronous)
                    context.Progress?.Report((float)orderIndex / nodeCount);

                int dependencyCount =
                    plan.GetIncomingConnections(node.NodeId).Count;
                if (!TryGatherInputs(
                        node,
                        cache,
                        plan.IncomingConnectionsByTargetId,
                        context,
                        out var inputs,
                        out var inputError))
                {
                    string message = FormatNodeFailure(
                        scope,
                        node,
                        inputError);
                    if (!authoritative)
                    {
                        message = AppendMessage(
                            message,
                            "Not connected to Output.");
                    }

                    logs.Add(new NodeExecutionLog(
                        node.NodeId,
                        node.Title,
                        authoritative ? NodeStatus.Error : NodeStatus.Warning,
                        message,
                        0f,
                        layerId: scope?.LayerId,
                        graphId: scope?.GraphId,
                        orderIndex: orderIndex,
                        inputDependencyCount: dependencyCount,
                        isConnectedToOutput: connectedToOutput,
                        participation: nodeParticipation,
                        isAuthoritative: authoritative));
                    if (!authoritative)
                        continue;

                    return Failure(
                        scope,
                        node.NodeId,
                        message,
                        logs,
                        cache,
                        plan,
                        artifacts);
                }

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                long allocatedBefore = GetThreadAllocatedBytes();
                context.ResetNodeProfiling();

                NodeOutput output;
                try
                {
                    output = await ExecuteNodeAsync(
                        node,
                        inputs,
                        context,
                        randomScope,
                        asynchronous);
                }
                catch (Exception exception)
                {
                    long allocated =
                        GetThreadAllocatedBytes() - allocatedBefore;
                    long iterations = context.ConsumeNodeIterations();
                    string message = FormatNodeFailure(
                        scope,
                        node,
                        exception.Message);
                    if (!authoritative)
                    {
                        message = AppendMessage(
                            message,
                            "Not connected to Output.");
                    }

                    logs.Add(new NodeExecutionLog(
                        node.NodeId,
                        node.Title,
                        authoritative ? NodeStatus.Error : NodeStatus.Warning,
                        message,
                        stopwatch.ElapsedMilliseconds,
                        allocated,
                        iterations,
                        scope?.LayerId,
                        scope?.GraphId,
                        orderIndex,
                        dependencyCount,
                        connectedToOutput,
                        nodeParticipation,
                        authoritative));
                    if (!authoritative)
                        continue;

                    return Failure(
                        scope,
                        node.NodeId,
                        message,
                        logs,
                        cache,
                        plan,
                        artifacts);
                }

                stopwatch.Stop();
                long allocatedDelta =
                    GetThreadAllocatedBytes() - allocatedBefore;
                long nodeIterations = context.ConsumeNodeIterations();
                if (nodeIterations <= 0)
                {
                    nodeIterations = EstimateIterationsFromOutputs(
                        output?.Values);
                }

                output ??= NodeOutput.Error(
                    "Node returned null instead of NodeOutput.");
                if (output.Status != NodeStatus.Error
                    && !TryValidateOutput(
                        node,
                        output,
                        context,
                        out var contractError))
                {
                    output = NodeOutput.Error(contractError);
                }

                string outputMessage = output.Status == NodeStatus.Error
                    ? FormatNodeFailure(scope, node, output.Message)
                    : output.Message;
                var logStatus = authoritative
                    ? output.Status
                    : NodeStatus.Warning;
                if (!authoritative)
                {
                    outputMessage = AppendMessage(
                        outputMessage,
                        "Not connected to Output.");
                }

                logs.Add(new NodeExecutionLog(
                    node.NodeId,
                    node.Title,
                    logStatus,
                    outputMessage,
                    stopwatch.ElapsedMilliseconds,
                    allocatedDelta,
                    nodeIterations,
                    scope?.LayerId,
                    scope?.GraphId,
                    orderIndex,
                    dependencyCount,
                    connectedToOutput,
                    nodeParticipation,
                    authoritative));

                if (output.Status == NodeStatus.Error)
                {
                    if (!authoritative)
                        continue;

                    return Failure(
                        scope,
                        node.NodeId,
                        outputMessage,
                        logs,
                        cache,
                        plan,
                        artifacts);
                }

                cache[node.NodeId] = output.Values;
                if (output.Artifact != null)
                    artifacts[node.NodeId] = output.Artifact;
                CaptureLayerMask(node, output.Artifact, maskRegistry);
            }

            if (asynchronous)
                context.Progress?.Report(1f);
            return new GraphExecutionResult(
                cache,
                logs,
                scope?.LayerId,
                scope?.GraphId,
                plan.ExecutionOrderNodeIds,
                artifacts);
        }

        private static Task<NodeOutput> ExecuteNodeAsync(
            NodeBase node,
            object[] inputs,
            NodeContext context,
            GraphRandomScope randomScope,
            bool asynchronous)
        {
            if (!asynchronous)
            {
                NodeOutput output = node is IAsyncNode asyncNode
                    ? asyncNode.ExecuteAsync(inputs, context)
                        .GetAwaiter()
                        .GetResult()
                    : node.Execute(inputs, context);
                return Task.FromResult(output);
            }

            return node is IAsyncNode asynchronousNode
                ? ExecuteAsyncNodeScoped(
                    asynchronousNode,
                    inputs,
                    context,
                    randomScope)
                : Task.FromResult(node.Execute(inputs, context));
        }

        private static GraphExecutionResult Failure(
            GraphExecutionScope scope,
            string nodeId,
            string message,
            List<NodeExecutionLog> logs,
            Dictionary<string, object[]> cache,
            GraphExecutionPlan plan,
            Dictionary<string, object> artifacts)
        {
            return new GraphExecutionResult(
                nodeId,
                message,
                logs,
                cache,
                scope?.LayerId,
                scope?.GraphId,
                plan.ExecutionOrderNodeIds,
                artifacts);
        }

        private static async Task YieldControlAsync(
            GraphRandomScope randomScope)
        {
            randomScope.Suspend();
            try
            {
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
    }
}
