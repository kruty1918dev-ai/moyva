using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    /// <summary>
    /// Виконавець графа генерації: обходить вузли у топологічному порядку,
    /// передає входи/виходи між вузлами через кеш та підтримує як
    /// синхронний, так і асинхронний режими виконання.
    /// </summary>
    public sealed partial class GraphRunner : IGraphRunner
    {
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

        private static GraphExecutionPlan BuildExecutionPlan(
            GraphExecutionScope scope,
            IReadOnlyCollection<string> authoritativeNodeIds,
            List<NodeExecutionLog> logs)
        {
            if (scope?.Nodes == null)
                return TopologicalSorter.BuildPlan(scope);

            var authoritativeIds = authoritativeNodeIds as ISet<string>
                ?? new HashSet<string>(
                    authoritativeNodeIds ?? Array.Empty<string>(),
                    StringComparer.Ordinal);
            var validNodes = scope.Nodes
                .Where(node => node != null && !string.IsNullOrEmpty(node.NodeId))
                .ToList();
            if (authoritativeIds.Count >= validNodes.Count)
            {
                return TopologicalSorter.BuildPlan(scope);
            }

            var nodeIds = new HashSet<string>(
                validNodes.Select(node => node.NodeId),
                StringComparer.Ordinal);
            var authoritativeNodes = validNodes
                .Where(node => authoritativeIds.Contains(node.NodeId))
                .ToList();
            var detachedNodes = validNodes
                .Where(node => !authoritativeIds.Contains(node.NodeId))
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
                        (sourceExists && authoritativeIds.Contains(connection.SourceNodeId))
                        || (targetExists && authoritativeIds.Contains(connection.TargetNodeId));
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
                bool sourceAuthoritative =
                    authoritativeIds.Contains(connection.SourceNodeId);
                bool targetAuthoritative =
                    authoritativeIds.Contains(connection.TargetNodeId);
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
            GraphNodeParticipationAnalysis participation)
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
                bool isConnectedToOutput =
                    participation?.IsConnectedToOutput(node.NodeId) ?? true;
                bool authoritative =
                    participation?.IsAuthoritative(node.NodeId) ?? true;
                GraphNodeParticipation nodeParticipation =
                    participation?.GetParticipation(node.NodeId)
                    ?? GraphNodeParticipation.OutputPath;
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
                        isConnectedToOutput: isConnectedToOutput,
                        participation: nodeParticipation,
                        isAuthoritative: false));
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
                    graphId: scope?.GraphId,
                    isConnectedToOutput: isConnectedToOutput,
                    participation: nodeParticipation,
                    isAuthoritative: true));
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
