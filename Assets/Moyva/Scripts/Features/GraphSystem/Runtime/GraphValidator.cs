using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    /// <summary>
    /// Validates graph structure without depending on feature-owned node types.
    /// Feature semantics are validated by the feature that defines those nodes.
    /// </summary>
    public sealed class GraphValidator
    {
        public List<ValidationError> Validate(GraphAsset graph)
        {
            return ValidateDetailed(graph).Issues
                .Select(issue => new ValidationError(
                    issue.NodeId,
                    issue.ToString(),
                    issue.Severity))
                .ToList();
        }

        public GraphValidationReport ValidateDetailed(GraphAsset graph)
        {
            var report = new GraphValidationReport();
            if (graph == null)
            {
                Add(report, "GRAPH_NULL", ValidationSeverity.Error,
                    "GraphAsset is not assigned.");
                return report;
            }

            graph.EnsureLayerGraphStates();
            ValidateCollections(graph, report);
            ValidateIds(graph, report);
            ValidateUniqueNodes(graph, report);
            ValidateLayers(graph, report);
            ValidateConnectionLayers(graph, report);

            foreach (var layer in graph.Layers)
            {
                if (layer == null)
                    continue;

                if (string.IsNullOrWhiteSpace(layer.Name))
                {
                    Add(report, "LAYER_NAME_EMPTY", ValidationSeverity.Error,
                        "Layer name is empty.", layerId: layer.Id,
                        canAutoFix: true);
                }

                ValidateScope(graph.CreateExecutionScope(layer.Id), report);
            }

            return report;
        }

        public GraphValidationReport ValidateDetailed(GraphExecutionScope scope)
        {
            var report = new GraphValidationReport();
            ValidateScope(scope, report);
            return report;
        }

        private static void ValidateCollections(
            GraphAsset graph,
            GraphValidationReport report)
        {
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (graph.Nodes[i] == null)
                {
                    Add(report, "NODE_NULL", ValidationSeverity.Error,
                        $"Node at index {i} is null (missing script?).",
                        canAutoFix: true);
                }
            }

            for (int i = 0; i < graph.Connections.Count; i++)
            {
                if (graph.Connections[i] == null)
                {
                    Add(report, "CONNECTION_NULL", ValidationSeverity.Error,
                        $"Connection at index {i} is null.",
                        canAutoFix: true);
                }
            }
        }

        private static void ValidateIds(
            GraphAsset graph,
            GraphValidationReport report)
        {
            var nodeIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var node in graph.Nodes)
            {
                if (node == null)
                    continue;
                if (!string.IsNullOrEmpty(node.NodeId)
                    && nodeIds.Add(node.NodeId))
                {
                    continue;
                }

                Add(report, "NODE_ID_DUPLICATE", ValidationSeverity.Error,
                    "NodeId is empty or duplicated.", nodeId: node.NodeId,
                    canAutoFix: true);
            }

            var connectionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var connection in graph.Connections)
            {
                if (connection == null)
                    continue;
                if (!string.IsNullOrEmpty(connection.ConnectionId)
                    && connectionIds.Add(connection.ConnectionId))
                {
                    continue;
                }

                Add(report, "CONNECTION_ID_DUPLICATE", ValidationSeverity.Error,
                    "ConnectionId is empty or duplicated.",
                    connectionId: connection.ConnectionId,
                    canAutoFix: true);
            }
        }

        private static void ValidateUniqueNodes(
            GraphAsset graph,
            GraphValidationReport report)
        {
            var participation = GraphNodeParticipationAnalyzer.Analyze(
                graph.CreateExecutionScope(null));
            var firstByType = new Dictionary<Type, NodeBase>();

            foreach (var node in graph.Nodes)
            {
                if (node == null
                    || !Attribute.IsDefined(
                        node.GetType(),
                        typeof(UniqueNodeAttribute))
                    || !participation.IsAuthoritative(node.NodeId))
                {
                    continue;
                }

                if (!firstByType.TryGetValue(node.GetType(), out var first))
                {
                    firstByType[node.GetType()] = node;
                    continue;
                }

                Add(report, "NODE_UNIQUE_DUPLICATE", ValidationSeverity.Error,
                    $"Graph contains multiple unique nodes of type " +
                    $"'{node.GetType().Name}'. First node: {first.NodeId}.",
                    layerId: node.LayerId,
                    nodeId: node.NodeId);
            }
        }

        private static void ValidateLayers(
            GraphAsset graph,
            GraphValidationReport report)
        {
            var layerIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var layer in graph.Layers)
            {
                if (layer == null)
                    continue;
                if (!layerIds.Add(layer.Id))
                {
                    Add(report, "LAYER_ID_DUPLICATE", ValidationSeverity.Error,
                        "LayerId is duplicated.", layerId: layer.Id);
                }
            }

            foreach (var node in graph.Nodes)
            {
                if (node == null || GraphAsset.IsGlobalNode(node))
                    continue;
                if (!string.IsNullOrEmpty(node.LayerId)
                    && layerIds.Contains(node.LayerId))
                {
                    continue;
                }

                Add(report, "NODE_LAYER_INVALID", ValidationSeverity.Error,
                    $"Node '{node.Title}' belongs to a missing layer.",
                    layerId: node.LayerId,
                    nodeId: node.NodeId,
                    canAutoFix: true);
            }
        }

        private static void ValidateConnectionLayers(
            GraphAsset graph,
            GraphValidationReport report)
        {
            foreach (var connection in graph.Connections)
            {
                if (connection == null)
                    continue;

                var source = graph.GetNodeById(connection.SourceNodeId);
                var target = graph.GetNodeById(connection.TargetNodeId);
                if (source == null
                    || target == null
                    || GraphAsset.IsGlobalNode(source)
                    || GraphAsset.IsGlobalNode(target)
                    || source.LayerId == target.LayerId
                    || source is IGraphLayerReferenceNode
                    || target is IGraphLayerReferenceNode)
                {
                    continue;
                }

                Add(report, "CONNECTION_CROSS_LAYER", ValidationSeverity.Error,
                    $"Connection crosses layers: '{source.Title}' " +
                    $"({source.LayerId}) -> '{target.Title}' ({target.LayerId}).",
                    layerId: source.LayerId,
                    nodeId: target.NodeId,
                    connectionId: connection.ConnectionId,
                    canAutoFix: true);
            }
        }

        private static void ValidateScope(
            GraphExecutionScope scope,
            GraphValidationReport report)
        {
            if (scope == null)
                return;

            var participation = GraphNodeParticipationAnalyzer.Analyze(scope);
            var authoritativeNodeIds = new HashSet<string>(
                participation.AuthoritativeNodeIds,
                StringComparer.Ordinal);
            ValidateCycles(scope, authoritativeNodeIds, report);
            ValidateConnections(scope, authoritativeNodeIds, report);
            ValidateRequiredInputs(scope, authoritativeNodeIds, report);
        }

        private static void ValidateCycles(
            GraphExecutionScope scope,
            ISet<string> authoritativeNodeIds,
            GraphValidationReport report)
        {
            var plan = TopologicalSorter.BuildPlan(scope);
            if (plan.Success)
                return;

            bool hasCycle = plan.CycleNodeIds.Count > 0;
            bool authoritative = !hasCycle
                || plan.CycleNodeIds.Any(authoritativeNodeIds.Contains);
            var severity = authoritative
                ? ValidationSeverity.Error
                : ValidationSeverity.Warning;
            Add(report,
                hasCycle ? "GRAPH_CYCLE" : "GRAPH_EXECUTION_PLAN_INVALID",
                severity,
                Detached(plan.ErrorMessage ?? "Graph execution plan is invalid.", severity),
                layerId: scope.LayerId,
                graphId: scope.GraphId,
                nodeId: hasCycle ? plan.CycleNodeIds.FirstOrDefault() : null);
        }

        private static void ValidateConnections(
            GraphExecutionScope scope,
            ISet<string> authoritativeNodeIds,
            GraphValidationReport report)
        {
            foreach (var connection in scope.Connections)
            {
                if (connection == null)
                    continue;

                var source = scope.GetNodeById(connection.SourceNodeId);
                var target = scope.GetNodeById(connection.TargetNodeId);
                var severity = IsAuthoritative(
                        scope,
                        authoritativeNodeIds,
                        target,
                        connection)
                    ? ValidationSeverity.Error
                    : ValidationSeverity.Warning;

                if (source == null || target == null)
                {
                    string missingId = source == null
                        ? connection.SourceNodeId
                        : connection.TargetNodeId;
                    Add(report,
                        source == null
                            ? "CONNECTION_SOURCE_MISSING"
                            : "CONNECTION_TARGET_MISSING",
                        severity,
                        Detached($"Connection references missing node '{missingId}'.", severity),
                        layerId: scope.LayerId,
                        graphId: scope.GraphId,
                        connectionId: connection.ConnectionId,
                        canAutoFix: true);
                    continue;
                }

                var outputs = source.Outputs ?? Array.Empty<PortDefinition>();
                var inputs = target.Inputs ?? Array.Empty<PortDefinition>();
                if (connection.SourcePortIndex < 0
                    || connection.SourcePortIndex >= outputs.Length)
                {
                    AddInvalidPort(report, scope, source, connection, severity,
                        sourcePort: true);
                    continue;
                }
                if (connection.TargetPortIndex < 0
                    || connection.TargetPortIndex >= inputs.Length)
                {
                    AddInvalidPort(report, scope, target, connection, severity,
                        sourcePort: false);
                    continue;
                }

                var sourcePort = outputs[connection.SourcePortIndex];
                var targetPort = inputs[connection.TargetPortIndex];
                bool compatible = sourcePort.IsCompatibleWith(targetPort)
                    || target is IGraphConnectionCompatibility custom
                    && custom.AcceptsConnection(
                        sourcePort,
                        connection.TargetPortIndex);
                if (compatible)
                    continue;

                Add(report, "CONNECTION_TYPE_MISMATCH", severity,
                    Detached(
                        $"Type mismatch: '{source.Title}'.{sourcePort.Name} " +
                        $"({sourcePort.ValueType.Name}) -> '{target.Title}'." +
                        $"{targetPort.Name} ({targetPort.ValueType.Name}).",
                        severity),
                    layerId: scope.LayerId,
                    graphId: scope.GraphId,
                    nodeId: target.NodeId,
                    connectionId: connection.ConnectionId,
                    canAutoFix: true);
            }
        }

        private static void AddInvalidPort(
            GraphValidationReport report,
            GraphExecutionScope scope,
            NodeBase node,
            Connection connection,
            ValidationSeverity severity,
            bool sourcePort)
        {
            int index = sourcePort
                ? connection.SourcePortIndex
                : connection.TargetPortIndex;
            Add(report,
                sourcePort
                    ? "CONNECTION_SOURCE_PORT_INVALID"
                    : "CONNECTION_TARGET_PORT_INVALID",
                severity,
                Detached(
                    $"{(sourcePort ? "Output" : "Input")} port index {index} " +
                    $"is out of range on '{node.Title}'.",
                    severity),
                layerId: scope.LayerId,
                graphId: scope.GraphId,
                nodeId: node.NodeId,
                connectionId: connection.ConnectionId,
                canAutoFix: true);
        }

        private static void ValidateRequiredInputs(
            GraphExecutionScope scope,
            ISet<string> authoritativeNodeIds,
            GraphValidationReport report)
        {
            var connected = new HashSet<string>(StringComparer.Ordinal);
            foreach (var connection in scope.Connections)
            {
                if (connection != null)
                    connected.Add($"{connection.TargetNodeId}:{connection.TargetPortIndex}");
            }

            foreach (var node in scope.Nodes)
            {
                if (node == null)
                    continue;

                var inputs = node.Inputs ?? Array.Empty<PortDefinition>();
                for (int i = 0; i < inputs.Length; i++)
                {
                    if (inputs[i] == null
                        || !inputs[i].IsRequired
                        || connected.Contains($"{node.NodeId}:{i}"))
                    {
                        continue;
                    }

                    var severity = authoritativeNodeIds.Contains(node.NodeId)
                        ? ValidationSeverity.Error
                        : ValidationSeverity.Warning;
                    Add(report, "INPUT_REQUIRED_UNCONNECTED", severity,
                        Detached(
                            $"Input port '{inputs[i].Name}' on '{node.Title}' is not connected.",
                            severity),
                        layerId: scope.LayerId,
                        graphId: scope.GraphId,
                        nodeId: node.NodeId);
                }
            }
        }

        private static bool IsAuthoritative(
            GraphExecutionScope scope,
            ISet<string> authoritativeNodeIds,
            NodeBase target,
            Connection connection)
        {
            if (!scope.Nodes.Any(node => node is IGraphOutputNode))
                return true;

            string nodeId = target?.NodeId ?? connection?.TargetNodeId;
            return !string.IsNullOrEmpty(nodeId)
                && authoritativeNodeIds.Contains(nodeId);
        }

        private static string Detached(
            string message,
            ValidationSeverity severity)
        {
            return severity == ValidationSeverity.Warning
                ? (message ?? string.Empty).TrimEnd() + " Not connected to Output."
                : message;
        }

        private static void Add(
            GraphValidationReport report,
            string code,
            ValidationSeverity severity,
            string message,
            string layerId = null,
            string graphId = null,
            string nodeId = null,
            string connectionId = null,
            bool canAutoFix = false)
        {
            report.Add(new GraphValidationIssue(
                code,
                severity,
                message,
                layerId,
                graphId,
                nodeId,
                connectionId,
                canAutoFix));
        }
    }
}
