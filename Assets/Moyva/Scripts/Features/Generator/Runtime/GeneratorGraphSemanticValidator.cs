using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Validates semantics owned by Generator without leaking its node types into GraphSystem.
    /// </summary>
    internal static class GeneratorGraphSemanticValidator
    {
        public static GraphValidationReport Validate(GraphAsset graph)
        {
            var report = new GraphValidator().ValidateDetailed(graph);
            if (graph == null)
                return report;

            graph.EnsureLayerGraphStates();
            ValidateLayerReferences(graph, report);
            foreach (var layer in graph.Layers)
            {
                if (layer == null)
                    continue;

                var scope = graph.CreateExecutionScope(layer.Id);
                var authoritative = new HashSet<string>(
                    GraphNodeParticipationAnalyzer
                        .Analyze(scope)
                        .AuthoritativeNodeIds,
                    StringComparer.Ordinal);
                ValidateTwcModifiers(scope, authoritative, report);
                ValidateObjectPlacement(scope, authoritative, report);
                ValidateSubgraphs(scope, authoritative, report);
                ValidateLayerOutput(graph, layer, report);
            }

            return report;
        }

        private static void ValidateLayerReferences(
            GraphAsset graph,
            GraphValidationReport report)
        {
            var layers = graph.Layers
                .Where(layer => layer != null)
                .ToList();
            var byId = layers
                .Where(layer => !string.IsNullOrEmpty(layer.Id))
                .GroupBy(layer => layer.Id)
                .ToDictionary(group => group.Key, group => group.First());
            var dependencies = new Dictionary<string, List<string>>(
                StringComparer.Ordinal);
            var nodeByEdge = new Dictionary<string, LayerMaskReferenceNode>(
                StringComparer.Ordinal);

            foreach (var layer in layers.Where(layer => layer.Enabled))
            {
                dependencies.TryAdd(layer.Id, new List<string>());
                foreach (var node in graph.GetNodesForLayer(layer.Id)
                             .OfType<LayerMaskReferenceNode>())
                {
                    string sourceId = node.SourceLayerId;
                    if (string.IsNullOrEmpty(sourceId))
                        continue;

                    if (!byId.TryGetValue(sourceId, out var source))
                    {
                        Add(report, "LAYER_REF_MISSING", ValidationSeverity.Error,
                            $"Layer reference points to missing layer '{sourceId}'.",
                            layer.Id, node.NodeId);
                        continue;
                    }
                    if (!source.Enabled)
                    {
                        Add(report, "LAYER_REF_SOURCE_DISABLED", ValidationSeverity.Error,
                            $"Layer reference points to disabled layer '{source.Name}'.",
                            layer.Id, node.NodeId);
                        continue;
                    }
                    if (source.Id == layer.Id)
                    {
                        Add(report, "LAYER_REF_SELF", ValidationSeverity.Error,
                            $"Layer '{layer.Name}' cannot reference itself.",
                            layer.Id, node.NodeId);
                        continue;
                    }
                    if (source.SortingOrder >= layer.SortingOrder)
                    {
                        Add(report, "LAYER_REF_FORWARD", ValidationSeverity.Error,
                            $"Layer '{layer.Name}' can reference only an earlier layer; " +
                            $"'{source.Name}' has order {source.SortingOrder}.",
                            layer.Id, node.NodeId);
                    }

                    dependencies[layer.Id].Add(source.Id);
                    nodeByEdge[$"{layer.Id}->{source.Id}"] = node;
                }
            }

            var cycle = FindCycle(dependencies);
            if (cycle.Count == 0)
                return;

            string cycleText = string.Join(" -> ", cycle.Select(id =>
                byId.TryGetValue(id, out var layer) ? layer.Name : id));
            var reported = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < cycle.Count - 1; i++)
            {
                string layerId = cycle[i];
                if (!reported.Add(layerId))
                    continue;

                nodeByEdge.TryGetValue(
                    $"{layerId}->{cycle[i + 1]}",
                    out var node);
                Add(report, "LAYER_REF_CYCLE", ValidationSeverity.Error,
                    $"Layer reference cycle: {cycleText}.",
                    layerId, node?.NodeId);
            }
        }

        private static List<string> FindCycle(
            IReadOnlyDictionary<string, List<string>> dependencies)
        {
            var visiting = new HashSet<string>(StringComparer.Ordinal);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            var stack = new List<string>();
            foreach (string layerId in dependencies.Keys.OrderBy(id => id))
            {
                var cycle = FindCycle(layerId, dependencies, visiting, visited, stack);
                if (cycle.Count > 0)
                    return cycle;
            }

            return new List<string>();
        }

        private static List<string> FindCycle(
            string layerId,
            IReadOnlyDictionary<string, List<string>> dependencies,
            ISet<string> visiting,
            ISet<string> visited,
            IList<string> stack)
        {
            if (visited.Contains(layerId))
                return new List<string>();
            if (visiting.Contains(layerId))
            {
                int start = stack.IndexOf(layerId);
                var cycle = start >= 0
                    ? stack.Skip(start).ToList()
                    : new List<string>();
                cycle.Add(layerId);
                return cycle;
            }

            visiting.Add(layerId);
            stack.Add(layerId);
            if (dependencies.TryGetValue(layerId, out var sources))
            {
                foreach (string source in sources.OrderBy(id => id))
                {
                    var cycle = FindCycle(
                        source, dependencies, visiting, visited, stack);
                    if (cycle.Count > 0)
                        return cycle;
                }
            }

            stack.RemoveAt(stack.Count - 1);
            visiting.Remove(layerId);
            visited.Add(layerId);
            return new List<string>();
        }

        private static void ValidateTwcModifiers(
            GraphExecutionScope scope,
            ISet<string> authoritative,
            GraphValidationReport report)
        {
            foreach (var node in scope.Nodes.OfType<TwcModifierNode>())
            {
                var severity = Severity(node, authoritative);
                if (string.IsNullOrWhiteSpace(node.ModifierTypeName))
                {
                    Add(report, "TWC_MODIFIER_TYPE_MISSING", severity,
                        Detached($"TWC node '{node.Title}' has no modifier type.", severity),
                        scope.LayerId, node.NodeId, scope.GraphId);
                }
                else if (node.Modifier == null)
                {
                    Add(report, "TWC_MODIFIER_INSTANCE_MISSING", severity,
                        Detached(
                            $"TWC node '{node.Title}' has type " +
                            $"'{node.ModifierTypeName}', but its modifier is missing.",
                            severity),
                        scope.LayerId, node.NodeId, scope.GraphId);
                }
            }
        }

        private static void ValidateObjectPlacement(
            GraphExecutionScope scope,
            ISet<string> authoritative,
            GraphValidationReport report)
        {
            var connectedNodes = new HashSet<string>(StringComparer.Ordinal);
            var connectedInputs = new HashSet<string>(StringComparer.Ordinal);
            foreach (var connection in scope.Connections)
            {
                if (connection == null)
                    continue;
                connectedNodes.Add(connection.SourceNodeId);
                connectedNodes.Add(connection.TargetNodeId);
                connectedInputs.Add(
                    $"{connection.TargetNodeId}:{connection.TargetPortIndex}");
            }

            foreach (var node in scope.Nodes.OfType<ObjectLayerNode>())
            {
                if (!connectedNodes.Contains(node.NodeId))
                    continue;

                int grassIndex = Array.FindIndex(
                    node.Inputs,
                    input => input?.Name == "Grass");
                bool grassConnected = grassIndex >= 0
                    && connectedInputs.Contains($"{node.NodeId}:{grassIndex}");
                if (grassConnected || node.HasConfiguredPrefab)
                    continue;

                var severity = Severity(node, authoritative);
                Add(report, "OBJECT_PREFABS_MISSING", severity,
                    Detached(
                        $"Object Layer '{node.Title}' has neither prefab variants " +
                        "nor a connected Grass input.",
                        severity),
                    scope.LayerId, node.NodeId, scope.GraphId);
            }
        }

        private static void ValidateSubgraphs(
            GraphExecutionScope scope,
            ISet<string> authoritative,
            GraphValidationReport report)
        {
            foreach (var node in scope.Nodes.OfType<SubgraphNode>())
            {
                if (node.Subgraph == null)
                    continue;

                var severity = Severity(node, authoritative);
                if (ReferenceEquals(scope.Graph, node.Subgraph))
                {
                    Add(report, "SUBGRAPH_SELF_REFERENCE", severity,
                        Detached("Subgraph cannot reference its containing graph.", severity),
                        scope.LayerId, node.NodeId, scope.GraphId);
                    continue;
                }

                node.Subgraph.EnsureLayerGraphStates();
                var outputs = node.Subgraph.Nodes.OfType<OutputNode>().ToList();
                if (string.IsNullOrEmpty(node.OutputLayerId))
                {
                    if (outputs.Count != 1)
                    {
                        Add(report, "SUBGRAPH_OUTPUT_AMBIGUOUS", severity,
                            Detached(
                                $"Subgraph has {outputs.Count} Output nodes; select a layer.",
                                severity),
                            scope.LayerId, node.NodeId, scope.GraphId);
                    }
                    continue;
                }

                int matches = outputs.Count(output =>
                    output.LayerId == node.OutputLayerId);
                if (matches == 1)
                    continue;

                Add(report,
                    matches == 0
                        ? "SUBGRAPH_OUTPUT_LAYER_MISSING"
                        : "SUBGRAPH_OUTPUT_LAYER_MULTIPLE",
                    severity,
                    Detached(
                        matches == 0
                            ? $"Subgraph has no Output in layer '{node.OutputLayerId}'."
                            : $"Subgraph has multiple Outputs in layer '{node.OutputLayerId}'.",
                        severity),
                    scope.LayerId, node.NodeId, scope.GraphId);
            }
        }

        private static void ValidateLayerOutput(
            GraphAsset graph,
            GeneratorLayerDefinition layer,
            GraphValidationReport report)
        {
            if (!layer.Enabled)
                return;

            var nodes = graph.GetNodesForLayer(layer.Id);
            var outputs = nodes.OfType<OutputNode>().ToList();
            if (outputs.Count != 1)
            {
                Add(report,
                    outputs.Count == 0
                        ? "LAYER_OUTPUT_MISSING"
                        : "LAYER_OUTPUT_MULTIPLE",
                    ValidationSeverity.Error,
                    outputs.Count == 0
                        ? $"Active layer '{layer.Name}' has no Output node."
                        : $"Layer '{layer.Name}' has {outputs.Count} Output nodes.",
                    layer.Id,
                    outputs.Count > 1 ? outputs[1].NodeId : null,
                    canAutoFix: true);
            }
            else
            {
                ValidateFinalOutput(graph, layer, outputs[0], report);
                ValidateOutputKind(nodes, layer, outputs[0], report);
            }

            bool hasObjectLayer = nodes.OfType<ObjectLayerNode>().Any();
            bool hasObjectOutput = nodes.OfType<ObjectOutputToTWCNode>().Any();
            if (hasObjectLayer && !hasObjectOutput)
            {
                Add(report, "OBJECT_OUTPUT_MISSING", ValidationSeverity.Error,
                    $"Layer '{layer.Name}' has Object Layer but no Object Output To TWC.",
                    layer.Id, canAutoFix: true);
            }
        }

        private static void ValidateFinalOutput(
            GraphAsset graph,
            GeneratorLayerDefinition layer,
            OutputNode output,
            GraphValidationReport report)
        {
            var connections = graph.GetConnectionsForLayer(
                layer.Id,
                includeGlobal: false);
            var outgoing = connections.FirstOrDefault(connection =>
                connection?.SourceNodeId == output.NodeId);
            if (outgoing != null)
            {
                Add(report, "LAYER_OUTPUT_NOT_FINAL", ValidationSeverity.Error,
                    $"Output in layer '{layer.Name}' has an outgoing connection.",
                    layer.Id, output.NodeId,
                    connectionId: outgoing.ConnectionId,
                    canAutoFix: true);
            }

            var incoming = connections.Where(connection =>
                connection?.TargetNodeId == output.NodeId).ToList();
            if (incoming.Count == 0)
            {
                Add(report, "LAYER_OUTPUT_UNCONNECTED", ValidationSeverity.Error,
                    $"Output in layer '{layer.Name}' is not connected.",
                    layer.Id, output.NodeId, canAutoFix: true);
                return;
            }

            string[] expected = ExpectedInputs(output.OutputKind);
            if (expected.Length == 0
                || incoming.Any(connection => IsExpectedInput(
                    graph, output, connection, expected)))
            {
                return;
            }

            Add(report, "LAYER_OUTPUT_KIND_UNCONNECTED", ValidationSeverity.Error,
                $"Output kind '{output.OutputKind}' expects: {string.Join(", ", expected)}.",
                layer.Id, output.NodeId);
        }

        private static bool IsExpectedInput(
            GraphAsset graph,
            OutputNode output,
            Connection connection,
            IReadOnlyCollection<string> expected)
        {
            int inputIndex = connection.TargetPortIndex;
            var inputs = output.Inputs;
            if (inputIndex >= 0 && inputIndex < inputs.Length)
            {
                string name = inputs[inputIndex].Name;
                if (expected.Any(value =>
                        name == value
                        || name.StartsWith(value + " ", StringComparison.Ordinal)))
                {
                    return true;
                }
            }

            if (output.OutputKind != LayerOutputKind.Masks
                || inputIndex != OutputNode.BiomeMapInputIndex)
            {
                return false;
            }

            var source = graph.GetNodeById(connection.SourceNodeId);
            return source?.Outputs != null
                && connection.SourcePortIndex >= 0
                && connection.SourcePortIndex < source.Outputs.Length
                && source.Outputs[connection.SourcePortIndex].ValueType
                    == typeof(bool[,]);
        }

        private static void ValidateOutputKind(
            IReadOnlyCollection<NodeBase> nodes,
            GeneratorLayerDefinition layer,
            OutputNode output,
            GraphValidationReport report)
        {
            var tileSettings = nodes.OfType<TileSettingsNode>().ToList();
            bool hasObjectLayer = nodes.OfType<ObjectLayerNode>().Any();
            bool hasObjectOutput = nodes.OfType<ObjectOutputToTWCNode>().Any();

            if (output.OutputKind == LayerOutputKind.Tiles)
            {
                if (tileSettings.Count == 0)
                {
                    Add(report, "TILE_OUTPUT_WITHOUT_TILE_SETTINGS", ValidationSeverity.Error,
                        $"Layer '{layer.Name}' outputs Tiles without Tile Settings.",
                        layer.Id, output.NodeId);
                }
                else if (!tileSettings.Any(node => node.HasRenderableTileOutput))
                {
                    Add(report, "TILE_SETTINGS_NODE_UNCONFIGURED", ValidationSeverity.Error,
                        $"Layer '{layer.Name}' has no configured Tile Settings output.",
                        layer.Id, output.NodeId);
                }
            }
            else if (tileSettings.Count > 0)
            {
                Add(report, "TILE_SETTINGS_IGNORED_BY_OUTPUT_KIND", ValidationSeverity.Warning,
                    $"Tile Settings is ignored for output kind '{output.OutputKind}'.",
                    layer.Id, output.NodeId);
            }

            if (output.OutputKind == LayerOutputKind.Objects
                && (!hasObjectLayer || !hasObjectOutput))
            {
                Add(report, "OBJECT_OUTPUT_PIPELINE_INCOMPLETE", ValidationSeverity.Error,
                    $"Layer '{layer.Name}' has an incomplete object output pipeline.",
                    layer.Id, output.NodeId);
            }
        }

        private static string[] ExpectedInputs(LayerOutputKind kind)
        {
            return kind switch
            {
                LayerOutputKind.Tiles => new[] { "Biome Map", "Height Map", "Mask" },
                LayerOutputKind.Objects => new[] { "Object Map", "Data" },
                LayerOutputKind.Masks => new[] { "Mask" },
                LayerOutputKind.InternalData => new[] { "Data" },
                _ => Array.Empty<string>()
            };
        }

        private static ValidationSeverity Severity(
            NodeBase node,
            ISet<string> authoritative)
        {
            return authoritative.Contains(node.NodeId)
                ? ValidationSeverity.Error
                : ValidationSeverity.Warning;
        }

        private static string Detached(
            string message,
            ValidationSeverity severity)
        {
            return severity == ValidationSeverity.Warning
                ? message.TrimEnd() + " Not connected to Output."
                : message;
        }

        private static void Add(
            GraphValidationReport report,
            string code,
            ValidationSeverity severity,
            string message,
            string layerId = null,
            string nodeId = null,
            string graphId = null,
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
