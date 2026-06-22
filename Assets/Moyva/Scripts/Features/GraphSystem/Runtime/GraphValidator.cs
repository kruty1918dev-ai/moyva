using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.GraphSystem.Runtime
{
    /// <summary>
    /// Валідатор структури графа генерації.
    /// Перевіряє: null-вузли, наявність циклів, коректність з'єднань (порти, типи),
    /// та наявність непідключених обов'язкових входів.
    /// </summary>
    public sealed class GraphValidator
    {
        public List<ValidationError> Validate(GraphAsset graph)
        {
            var report = ValidateDetailed(graph);
            return report.Issues
                .Select(issue => new ValidationError(issue.NodeId, issue.ToString(), issue.Severity))
                .ToList();
        }

        public GraphValidationReport ValidateDetailed(GraphAsset graph)
        {
            var report = new GraphValidationReport();
            if (graph == null)
            {
                report.Add(new GraphValidationIssue(
                    "GRAPH_NULL",
                    ValidationSeverity.Error,
                    "GraphAsset не задано."));
                return report;
            }

            graph.EnsureLayerGraphStates();

            ValidateNullNodes(graph, report);
            ValidateNullConnections(graph, report);
            ValidateDuplicateIds(graph, report);
            ValidateLayerStates(graph, report);
            ValidateLayerReferences(graph, report);

            for (int i = 0; i < graph.Layers.Count; i++)
            {
                var layer = graph.Layers[i];
                if (layer == null)
                    continue;

                ValidateLayerDefinition(layer, report);
                ValidateScope(graph.CreateExecutionScope(layer.Id), report);
                ValidateLayerOutputs(graph, layer, report);
            }

            return report;
        }

        public GraphValidationReport ValidateDetailed(GraphExecutionScope scope)
        {
            var report = new GraphValidationReport();
            ValidateScope(scope, report);
            return report;
        }

        private void ValidateNullNodes(GraphAsset graph, GraphValidationReport report)
        {
            if (graph?.Nodes == null)
                return;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                if (graph.Nodes[i] == null)
                {
                    report.Add(new GraphValidationIssue(
                        "NODE_NULL",
                        ValidationSeverity.Error,
                        $"Node at index {i} is null (missing script?).",
                        canAutoFix: true));
                }
            }
        }

        private void ValidateNullConnections(GraphAsset graph, GraphValidationReport report)
        {
            if (graph?.Connections == null)
                return;

            for (int i = 0; i < graph.Connections.Count; i++)
            {
                if (graph.Connections[i] == null)
                {
                    report.Add(new GraphValidationIssue(
                        "CONNECTION_NULL",
                        ValidationSeverity.Error,
                        $"Connection at index {i} is null.",
                        canAutoFix: true));
                }
            }
        }

        private void ValidateDuplicateIds(GraphAsset graph, GraphValidationReport report)
        {
            var nodeIds = new HashSet<string>();
            foreach (var node in graph.Nodes)
            {
                if (node == null)
                    continue;

                if (string.IsNullOrEmpty(node.NodeId) || !nodeIds.Add(node.NodeId))
                {
                    report.Add(new GraphValidationIssue(
                        "NODE_ID_DUPLICATE",
                        ValidationSeverity.Error,
                        "NodeId порожній або дублюється.",
                        nodeId: node.NodeId,
                        canAutoFix: true));
                }
            }

            var connectionIds = new HashSet<string>();
            foreach (var connection in graph.Connections)
            {
                if (connection == null)
                    continue;

                if (string.IsNullOrEmpty(connection.ConnectionId) || !connectionIds.Add(connection.ConnectionId))
                {
                    report.Add(new GraphValidationIssue(
                        "CONNECTION_ID_DUPLICATE",
                        ValidationSeverity.Error,
                        "ConnectionId порожній або дублюється.",
                        connectionId: connection.ConnectionId,
                        canAutoFix: true));
                }
            }
        }

        private void ValidateLayerStates(GraphAsset graph, GraphValidationReport report)
        {
            var layerIds = new HashSet<string>();
            foreach (var layer in graph.Layers)
            {
                if (layer == null)
                    continue;

                if (!layerIds.Add(layer.Id))
                {
                    report.Add(new GraphValidationIssue(
                        "LAYER_ID_DUPLICATE",
                        ValidationSeverity.Error,
                        "LayerId дублюється.",
                        layerId: layer.Id));
                }
            }

            foreach (var node in graph.Nodes)
            {
                if (node == null || GraphAsset.IsGlobalNode(node))
                    continue;

                if (string.IsNullOrEmpty(node.LayerId) || !layerIds.Contains(node.LayerId))
                {
                    report.Add(new GraphValidationIssue(
                        "NODE_LAYER_INVALID",
                        ValidationSeverity.Error,
                        $"Node '{node.Title}' належить до відсутнього шару.",
                        layerId: node.LayerId,
                        nodeId: node.NodeId,
                        canAutoFix: true));
                }
            }
        }

        private void ValidateLayerReferences(GraphAsset graph, GraphValidationReport report)
        {
            if (graph?.Layers == null)
                return;

            var layers = graph.Layers
                .Where(layer => layer != null)
                .ToList();
            var layersById = layers
                .Where(layer => !string.IsNullOrEmpty(layer.Id))
                .GroupBy(layer => layer.Id)
                .ToDictionary(group => group.Key, group => group.First());
            var dependencies = new Dictionary<string, List<string>>();
            var referenceNodeByEdge = new Dictionary<string, NodeBase>();

            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (layer == null || !layer.Enabled)
                    continue;

                if (!dependencies.ContainsKey(layer.Id))
                    dependencies[layer.Id] = new List<string>();

                var nodes = graph.GetNodesForLayer(layer.Id);
                for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
                {
                    var node = nodes[nodeIndex];
                    if (!TryGetLayerReferenceSourceId(node, out string sourceLayerId)
                        || string.IsNullOrEmpty(sourceLayerId))
                        continue;

                    if (!layersById.TryGetValue(sourceLayerId, out var sourceLayer))
                    {
                        report.Add(new GraphValidationIssue(
                            "LAYER_REF_MISSING",
                            ValidationSeverity.Error,
                            $"Layer Ref у шарі '{layer.Name}' посилається на відсутній шар '{sourceLayerId}'. Обери існуючий шар-джерело.",
                            layerId: layer.Id,
                            nodeId: node.NodeId));
                        continue;
                    }

                    if (!sourceLayer.Enabled)
                    {
                        report.Add(new GraphValidationIssue(
                            "LAYER_REF_SOURCE_DISABLED",
                            ValidationSeverity.Error,
                            $"Layer Ref у шарі '{layer.Name}' посилається на вимкнений шар '{sourceLayer.Name}'. Увімкни source layer або обери шар, який буде оброблений раніше.",
                            layerId: layer.Id,
                            nodeId: node.NodeId));
                        continue;
                    }

                    if (sourceLayer.Id == layer.Id)
                    {
                        report.Add(new GraphValidationIssue(
                            "LAYER_REF_SELF",
                            ValidationSeverity.Error,
                            $"Шар '{layer.Name}' не може посилатися сам на себе через Layer Ref.",
                            layerId: layer.Id,
                            nodeId: node.NodeId));
                        continue;
                    }

                    if (sourceLayer.SortingOrder >= layer.SortingOrder)
                    {
                        report.Add(new GraphValidationIssue(
                            "LAYER_REF_FORWARD",
                            ValidationSeverity.Error,
                            $"Шар '{layer.Name}' посилається на '{sourceLayer.Name}', але Ref Layer може посилатися тільки на шари, які виконуються раніше. Перемісти '{sourceLayer.Name}' вище або обери інший source layer.",
                            layerId: layer.Id,
                            nodeId: node.NodeId));
                    }

                    dependencies[layer.Id].Add(sourceLayer.Id);
                    referenceNodeByEdge[$"{layer.Id}->{sourceLayer.Id}"] = node;
                }
            }

            var cycle = FindLayerReferenceCycle(dependencies);
            if (cycle.Count == 0)
                return;

            string cycleText = FormatLayerCycle(cycle, layersById);
            var reportedLayerIds = new HashSet<string>();
            for (int i = 0; i < cycle.Count - 1; i++)
            {
                string issueLayerId = cycle[i];
                if (!reportedLayerIds.Add(issueLayerId))
                    continue;

                string nextLayerId = cycle[i + 1];
                referenceNodeByEdge.TryGetValue($"{issueLayerId}->{nextLayerId}", out var issueNode);
                report.Add(new GraphValidationIssue(
                    "LAYER_REF_CYCLE",
                    ValidationSeverity.Error,
                    $"Циклічна залежність між шарами через Layer Ref: {cycleText}. Розірви один із Ref Layer зв'язків.",
                    layerId: issueLayerId,
                    nodeId: issueNode?.NodeId));
            }
        }

        private static bool TryGetLayerReferenceSourceId(NodeBase node, out string sourceLayerId)
        {
            sourceLayerId = null;
            if (node == null || node.GetType().Name != "LayerMaskReferenceNode")
                return false;

            var property = node.GetType().GetProperty("SourceLayerId", BindingFlags.Instance | BindingFlags.Public);
            sourceLayerId = property?.GetValue(node) as string;
            return true;
        }

        private static List<string> FindLayerReferenceCycle(Dictionary<string, List<string>> dependencies)
        {
            var visiting = new HashSet<string>();
            var visited = new HashSet<string>();
            var stack = new List<string>();

            foreach (var layerId in dependencies.Keys.OrderBy(id => id))
            {
                var cycle = FindLayerReferenceCycleDepthFirst(layerId, dependencies, visiting, visited, stack);
                if (cycle.Count > 0)
                    return cycle;
            }

            return new List<string>();
        }

        private static List<string> FindLayerReferenceCycleDepthFirst(
            string layerId,
            Dictionary<string, List<string>> dependencies,
            HashSet<string> visiting,
            HashSet<string> visited,
            List<string> stack)
        {
            if (visited.Contains(layerId))
                return new List<string>();

            if (visiting.Contains(layerId))
            {
                int start = stack.IndexOf(layerId);
                if (start < 0)
                    return new List<string> { layerId };

                var cycle = stack.Skip(start).ToList();
                cycle.Add(layerId);
                return cycle;
            }

            visiting.Add(layerId);
            stack.Add(layerId);

            if (dependencies.TryGetValue(layerId, out var sources))
            {
                foreach (var sourceLayerId in sources.OrderBy(id => id))
                {
                    var cycle = FindLayerReferenceCycleDepthFirst(sourceLayerId, dependencies, visiting, visited, stack);
                    if (cycle.Count > 0)
                        return cycle;
                }
            }

            stack.RemoveAt(stack.Count - 1);
            visiting.Remove(layerId);
            visited.Add(layerId);
            return new List<string>();
        }

        private static string FormatLayerCycle(
            IReadOnlyList<string> cycle,
            IReadOnlyDictionary<string, GeneratorLayerDefinition> layersById)
        {
            return string.Join(" -> ", cycle.Select(layerId =>
                layersById.TryGetValue(layerId, out var layer)
                    ? layer.Name
                    : layerId));
        }

        private void ValidateScope(GraphExecutionScope scope, GraphValidationReport report)
        {
            if (scope == null)
                return;

            ValidateCycles(scope, report);
            ValidateConnections(scope, report);
            ValidateRequiredInputs(scope, report);
            ValidateTwcModifierNodes(scope, report);
            ValidateObjectPlacementNodes(scope, report);
        }

        private void ValidateCycles(GraphExecutionScope scope, GraphValidationReport report)
        {
            var plan = TopologicalSorter.BuildPlan(scope);
            if (!plan.Success)
            {
                bool hasCycle = plan.CycleNodeIds.Count > 0;
                report.Add(new GraphValidationIssue(
                    hasCycle ? "GRAPH_CYCLE" : "GRAPH_EXECUTION_PLAN_INVALID",
                    ValidationSeverity.Error,
                    plan.ErrorMessage ?? "Graph execution plan is invalid.",
                    layerId: scope.LayerId,
                    graphId: scope.GraphId,
                    nodeId: hasCycle ? plan.CycleNodeIds.FirstOrDefault() : null));
            }
        }

        private void ValidateConnections(GraphExecutionScope scope, GraphValidationReport report)
        {
            foreach (var conn in scope.Connections)
            {
                if (conn == null)
                {
                    report.Add(new GraphValidationIssue(
                        "CONNECTION_NULL",
                        ValidationSeverity.Error,
                        "Connection is null.",
                        layerId: scope.LayerId,
                        graphId: scope.GraphId,
                        canAutoFix: true));
                    continue;
                }

                var source = scope.Graph.GetNodeById(conn.SourceNodeId);
                var target = scope.Graph.GetNodeById(conn.TargetNodeId);

                if (source == null)
                {
                    report.Add(new GraphValidationIssue(
                        "CONNECTION_SOURCE_MISSING",
                        ValidationSeverity.Error,
                        $"Connection references missing source node '{conn.SourceNodeId}'.",
                        layerId: scope.LayerId,
                        graphId: scope.GraphId,
                        connectionId: conn.ConnectionId,
                        canAutoFix: true));
                    continue;
                }
                if (target == null)
                {
                    report.Add(new GraphValidationIssue(
                        "CONNECTION_TARGET_MISSING",
                        ValidationSeverity.Error,
                        $"Connection references missing target node '{conn.TargetNodeId}'.",
                        layerId: scope.LayerId,
                        graphId: scope.GraphId,
                        connectionId: conn.ConnectionId,
                        canAutoFix: true));
                    continue;
                }

                ValidateCrossLayerConnection(scope, conn, source, target, report);

                var sourceOutputs = source.Outputs;
                var targetInputs = target.Inputs;

                if (conn.SourcePortIndex < 0 || conn.SourcePortIndex >= sourceOutputs.Length)
                {
                    report.Add(new GraphValidationIssue(
                        "CONNECTION_SOURCE_PORT_INVALID",
                        ValidationSeverity.Error,
                        $"Output port index {conn.SourcePortIndex} out of range on '{source.Title}'.",
                        layerId: scope.LayerId,
                        graphId: scope.GraphId,
                        nodeId: source.NodeId,
                        connectionId: conn.ConnectionId,
                        canAutoFix: true));
                    continue;
                }
                if (conn.TargetPortIndex < 0 || conn.TargetPortIndex >= targetInputs.Length)
                {
                    report.Add(new GraphValidationIssue(
                        "CONNECTION_TARGET_PORT_INVALID",
                        ValidationSeverity.Error,
                        $"Input port index {conn.TargetPortIndex} out of range on '{target.Title}'.",
                        layerId: scope.LayerId,
                        graphId: scope.GraphId,
                        nodeId: target.NodeId,
                        connectionId: conn.ConnectionId,
                        canAutoFix: true));
                    continue;
                }

                var sourcePort = sourceOutputs[conn.SourcePortIndex];
                var targetPort = targetInputs[conn.TargetPortIndex];

                if (!PortDefinition.AreValueTypesCompatible(sourcePort.ValueType, targetPort.ValueType))
                {
                    report.Add(new GraphValidationIssue(
                        "CONNECTION_TYPE_MISMATCH",
                        ValidationSeverity.Error,
                        $"Type mismatch: '{source.Title}'.{sourcePort.Name} ({sourcePort.ValueType.Name}) -> '{target.Title}'.{targetPort.Name} ({targetPort.ValueType.Name}).",
                        layerId: scope.LayerId,
                        graphId: scope.GraphId,
                        nodeId: target.NodeId,
                        connectionId: conn.ConnectionId,
                        canAutoFix: true));
                }
            }
        }

        private void ValidateCrossLayerConnection(
            GraphExecutionScope scope,
            Connection connection,
            NodeBase source,
            NodeBase target,
            GraphValidationReport report)
        {
            if (GraphAsset.IsGlobalNode(source) || GraphAsset.IsGlobalNode(target))
                return;

            if (source.LayerId == target.LayerId)
                return;

            bool explicitLayerReference = target.GetType().Name == "LayerMaskReferenceNode"
                || source.GetType().Name == "LayerMaskReferenceNode";

            if (explicitLayerReference)
                return;

            report.Add(new GraphValidationIssue(
                "CONNECTION_CROSS_LAYER",
                ValidationSeverity.Error,
                $"Connection crosses layers: '{source.Title}' ({source.LayerId}) -> '{target.Title}' ({target.LayerId}).",
                layerId: scope.LayerId,
                graphId: scope.GraphId,
                nodeId: target.NodeId,
                connectionId: connection.ConnectionId,
                canAutoFix: true));
        }

        private void ValidateRequiredInputs(GraphExecutionScope scope, GraphValidationReport report)
        {
            var connectedInputs = new HashSet<string>();
            var connectedNodeIds = new HashSet<string>();
            foreach (var conn in scope.Connections)
            {
                connectedInputs.Add($"{conn.TargetNodeId}:{conn.TargetPortIndex}");
                connectedNodeIds.Add(conn.SourceNodeId);
                connectedNodeIds.Add(conn.TargetNodeId);
            }

            foreach (var node in scope.Nodes)
            {
                if (node == null) continue;

                // Isolated node (no incoming and no outgoing connections) is treated as unused,
                // not as an invalid graph state.
                if (!connectedNodeIds.Contains(node.NodeId))
                    continue;

                var inputs = node.Inputs;
                for (int i = 0; i < inputs.Length; i++)
                {
                    if (IsOptionalInput(node, inputs[i].Name))
                        continue;

                    string key = $"{node.NodeId}:{i}";
                    if (!connectedInputs.Contains(key))
                    {
                        report.Add(new GraphValidationIssue(
                            "INPUT_REQUIRED_UNCONNECTED",
                            ValidationSeverity.Error,
                            $"Input port '{inputs[i].Name}' on '{node.Title}' is not connected.",
                            layerId: scope.LayerId,
                            graphId: scope.GraphId,
                            nodeId: node.NodeId));
                    }
                }
            }
        }

        private void ValidateObjectPlacementNodes(GraphExecutionScope scope, GraphValidationReport report)
        {
            if (scope?.Nodes == null)
                return;

            var connectedInputs = new HashSet<string>();
            var connectedNodeIds = new HashSet<string>();
            foreach (var conn in scope.Connections)
            {
                if (conn == null)
                    continue;

                connectedInputs.Add($"{conn.TargetNodeId}:{conn.TargetPortIndex}");
                connectedNodeIds.Add(conn.SourceNodeId);
                connectedNodeIds.Add(conn.TargetNodeId);
            }

            foreach (var node in scope.Nodes)
            {
                if (node == null || node.GetType().Name != "ObjectLayerNode")
                    continue;
                if (!connectedNodeIds.Contains(node.NodeId))
                    continue;

                int grassInputIndex = FindInputIndex(node, "Grass");
                bool grassConnected = grassInputIndex >= 0
                    && connectedInputs.Contains($"{node.NodeId}:{grassInputIndex}");
                if (grassConnected || HasConfiguredPrefab(node))
                    continue;

                report.Add(new GraphValidationIssue(
                    "OBJECT_PREFABS_MISSING",
                    ValidationSeverity.Error,
                    $"Object Layer '{node.Title}' не має prefab variants і не має підключеного Grass input. Додай prefab variant або підключи Grass input, інакше об'єкти не будуть створені.",
                    layerId: scope.LayerId,
                    graphId: scope.GraphId,
                    nodeId: node.NodeId));
            }
        }

        private void ValidateTwcModifierNodes(GraphExecutionScope scope, GraphValidationReport report)
        {
            if (scope?.Nodes == null)
                return;

            foreach (var node in scope.Nodes)
            {
                if (node == null || node.GetType().Name != "TwcModifierNode")
                    continue;

                string modifierTypeName = ReadStringProperty(node, "ModifierTypeName");
                object modifier = ReadPropertyValue(node, "Modifier");
                if (string.IsNullOrWhiteSpace(modifierTypeName))
                {
                    report.Add(new GraphValidationIssue(
                        "TWC_MODIFIER_TYPE_MISSING",
                        ValidationSeverity.Error,
                        $"TWC node '{node.Title}' не має типу modifier. Видали ноду або створи її заново з каталогу TileWorldCreator.",
                        layerId: scope.LayerId,
                        graphId: scope.GraphId,
                        nodeId: node.NodeId));
                    continue;
                }

                if (modifier == null)
                {
                    report.Add(new GraphValidationIssue(
                        "TWC_MODIFIER_INSTANCE_MISSING",
                        ValidationSeverity.Error,
                        $"TWC node '{node.Title}' має тип '{modifierTypeName}', але serialized modifier asset відсутній. Відкрий ноду в inspector або створи її заново.",
                        layerId: scope.LayerId,
                        graphId: scope.GraphId,
                        nodeId: node.NodeId));
                }
            }
        }

        private static int FindInputIndex(NodeBase node, string inputName)
        {
            var inputs = node?.Inputs;
            if (inputs == null)
                return -1;

            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i].Name == inputName)
                    return i;
            }

            return -1;
        }

        private static bool HasConfiguredPrefab(NodeBase node)
        {
            var field = node?.GetType().GetField("_prefabs", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field?.GetValue(node) is not IEnumerable prefabs)
                return false;

            foreach (var entry in prefabs)
            {
                if (entry == null)
                    continue;

                var prefabField = entry.GetType().GetField("Prefab", BindingFlags.Instance | BindingFlags.Public);
                if (prefabField?.GetValue(entry) != null)
                    return true;
            }

            return false;
        }

        private static object ReadPropertyValue(NodeBase node, string propertyName)
        {
            var property = node?.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            return property?.GetValue(node);
        }

        private static string ReadStringProperty(NodeBase node, string propertyName) =>
            ReadPropertyValue(node, propertyName) as string;

        private void ValidateLayerDefinition(
            GeneratorLayerDefinition layer,
            GraphValidationReport report)
        {
            if (layer == null)
                return;

            if (string.IsNullOrWhiteSpace(layer.Name))
            {
                report.Add(new GraphValidationIssue(
                    "LAYER_NAME_EMPTY",
                    ValidationSeverity.Error,
                    "Назва шару порожня.",
                    layerId: layer.Id,
                    canAutoFix: true));
            }
        }

        private void ValidateLayerOutputs(
            GraphAsset graph,
            GeneratorLayerDefinition layer,
            GraphValidationReport report)
        {
            if (graph == null || layer == null || !layer.Enabled)
                return;

            var nodes = graph.GetNodesForLayer(layer.Id);
            if (nodes.Count == 0)
            {
                report.Add(new GraphValidationIssue(
                    "LAYER_OUTPUT_MISSING",
                    ValidationSeverity.Error,
                    $"Активний шар '{layer.Name}' не має Output Node. Додай фінальний Output Node і підключи результат шару.",
                    layerId: layer.Id));
                return;
            }

            var outputNodes = nodes
                .Where(node => node != null && node.GetType().Name == "OutputNode")
                .ToList();
            bool hasObjectLayerNode = nodes.Any(node => node != null && node.GetType().Name == "ObjectLayerNode");
            bool hasObjectOutputNode = nodes.Any(node => node != null && node.GetType().Name == "ObjectOutputToTWCNode");
            bool hasTwcTileNode = nodes.Any(node => node != null && node.GetType().Name == "TwcModifierNode");
            bool hasTileSettingsNode = nodes.Any(IsTileSettingsNode);
            bool hasConfiguredTileSettingsNode = nodes.Any(HasConfiguredTileSettingsNode);

            if (outputNodes.Count == 0)
            {
                report.Add(new GraphValidationIssue(
                    "LAYER_OUTPUT_MISSING",
                    ValidationSeverity.Error,
                    $"Шар '{layer.Name}' не має Output Node. Кожен активний шар повинен завершуватися фінальною Output Node.",
                    layerId: layer.Id,
                    canAutoFix: true));
            }
            else if (outputNodes.Count > 1)
            {
                report.Add(new GraphValidationIssue(
                    "LAYER_OUTPUT_MULTIPLE",
                    ValidationSeverity.Error,
                    $"Шар '{layer.Name}' має {outputNodes.Count} Output Node. Залиши одну фінальну Output Node для цього шару.",
                    layerId: layer.Id,
                    nodeId: outputNodes[1].NodeId));
            }
            else
            {
                ValidateSingleLayerOutputNode(graph, layer, outputNodes[0], report);
                ValidateLayerOutputSemantics(layer, outputNodes[0], hasTwcTileNode, hasTileSettingsNode, hasConfiguredTileSettingsNode, hasObjectLayerNode, hasObjectOutputNode, report);
            }

            if (hasObjectLayerNode && !hasObjectOutputNode)
            {
                report.Add(new GraphValidationIssue(
                    "OBJECT_OUTPUT_MISSING",
                    ValidationSeverity.Error,
                    $"Шар '{layer.Name}' має Object Layer, але не має Object Output To TWC. Додай Object Output To TWC і підключи його до Output Node.",
                    layerId: layer.Id,
                    canAutoFix: true));
            }
        }

        private void ValidateLayerOutputSemantics(
            GeneratorLayerDefinition layer,
            NodeBase outputNode,
            bool hasTwcTileNode,
            bool hasTileSettingsNode,
            bool hasConfiguredTileSettingsNode,
            bool hasObjectLayerNode,
            bool hasObjectOutputNode,
            GraphValidationReport report)
        {
            string kindName = ResolveLayerOutputKindName(outputNode);
            if (kindName == "Tiles")
            {
                if (!hasTwcTileNode)
                {
                    report.Add(new GraphValidationIssue(
                        "TILE_OUTPUT_WITHOUT_MASK_PIPELINE",
                        ValidationSeverity.Error,
                        $"Шар '{layer.Name}' позначений як Tiles, але не має жодної TWC/Mask ноди, яка формує маску тайлів. Додай TWC generator/modifier або зміни Output Kind на Masks/InternalData.",
                        layerId: layer.Id,
                        nodeId: outputNode.NodeId));
                }

                if (!hasTileSettingsNode)
                {
                    report.Add(new GraphValidationIssue(
                        "TILE_OUTPUT_WITHOUT_TILE_SETTINGS_NODE",
                        ValidationSeverity.Error,
                        $"Шар '{layer.Name}' позначений як Tiles, але не має Tile Settings node. Додай у цей шар Tile Settings node і вибери TilePreset/tileset для runtime build layer.",
                        layerId: layer.Id,
                        nodeId: outputNode.NodeId,
                        canAutoFix: true));
                }
                else if (!hasConfiguredTileSettingsNode)
                {
                    report.Add(new GraphValidationIssue(
                        "TILE_SETTINGS_NODE_UNCONFIGURED",
                        ValidationSeverity.Error,
                        $"Шар '{layer.Name}' має Tile Settings node, але в ньому не задано TilePreset і не увімкнено Flat Surface. Додай хоча б один TilePreset variant у ноді або увімкни Flat Surface.",
                        layerId: layer.Id,
                        nodeId: outputNode.NodeId));
                }
            }
            else if (hasTileSettingsNode)
            {
                report.Add(new GraphValidationIssue(
                    "TILE_SETTINGS_IGNORED_BY_OUTPUT_KIND",
                    ValidationSeverity.Warning,
                    $"Шар '{layer.Name}' має Tile Settings node, але Output Kind = '{kindName}'. Tile Settings застосовується тільки для Tiles output.",
                    layerId: layer.Id,
                    nodeId: outputNode.NodeId));
            }

            if (kindName == "Objects" && (!hasObjectLayerNode || !hasObjectOutputNode))
            {
                report.Add(new GraphValidationIssue(
                    "OBJECT_OUTPUT_PIPELINE_INCOMPLETE",
                    ValidationSeverity.Error,
                    $"Шар '{layer.Name}' позначений як Objects, але object pipeline неповний. Потрібні Object Layer, Object Output To TWC і підключений Output Node.",
                    layerId: layer.Id,
                    nodeId: outputNode.NodeId));
            }
        }

        private void ValidateSingleLayerOutputNode(
            GraphAsset graph,
            GeneratorLayerDefinition layer,
            NodeBase outputNode,
            GraphValidationReport report)
        {
            var layerConnections = graph.GetConnectionsForLayer(layer.Id, includeGlobal: false);
            var outgoing = layerConnections.FirstOrDefault(connection =>
                connection != null && connection.SourceNodeId == outputNode.NodeId);
            if (outgoing != null)
            {
                report.Add(new GraphValidationIssue(
                    "LAYER_OUTPUT_NOT_FINAL",
                    ValidationSeverity.Error,
                    $"Output Node у шарі '{layer.Name}' має вихідне з'єднання. Output Node повинна бути фінальною нодою шару.",
                    layerId: layer.Id,
                    nodeId: outputNode.NodeId,
                    connectionId: outgoing.ConnectionId,
                    canAutoFix: true));
            }

            var incoming = layerConnections
                .Where(connection => connection != null && connection.TargetNodeId == outputNode.NodeId)
                .ToList();
            if (incoming.Count == 0)
            {
                report.Add(new GraphValidationIssue(
                    "LAYER_OUTPUT_UNCONNECTED",
                    ValidationSeverity.Error,
                    $"Output Node у шарі '{layer.Name}' не підключена. Підключи фінальний результат шару до входу Output Node.",
                    layerId: layer.Id,
                    nodeId: outputNode.NodeId,
                    canAutoFix: true));
                return;
            }

            if (!HasExpectedLayerOutputInput(outputNode, incoming, out string kindName, out string expectedPorts))
            {
                report.Add(new GraphValidationIssue(
                    "LAYER_OUTPUT_KIND_UNCONNECTED",
                    ValidationSeverity.Error,
                    $"Output Node у шарі '{layer.Name}' має тип '{kindName}', але не має підключення до очікуваного входу: {expectedPorts}.",
                    layerId: layer.Id,
                    nodeId: outputNode.NodeId));
            }
        }

        private static bool HasExpectedLayerOutputInput(
            NodeBase outputNode,
            IReadOnlyList<Connection> incoming,
            out string kindName,
            out string expectedPorts)
        {
            kindName = ResolveLayerOutputKindName(outputNode);
            var expected = GetExpectedOutputInputNames(kindName);
            expectedPorts = expected.Length == 0
                ? "будь-який Output input"
                : string.Join(", ", expected);

            if (expected.Length == 0)
                return incoming.Count > 0;

            var inputs = outputNode?.Inputs;
            if (inputs == null)
                return false;

            foreach (var connection in incoming)
            {
                int inputIndex = connection.TargetPortIndex;
                if (inputIndex < 0 || inputIndex >= inputs.Length)
                    continue;

                string inputName = inputs[inputIndex].Name;
                if (expected.Any(portName => PortNameMatches(inputName, portName)))
                    return true;
            }

            return false;
        }

        private static bool IsTileSettingsNode(NodeBase node) =>
            node != null && node.GetType().Name == "TileSettingsNode";

        private static bool HasConfiguredTileSettingsNode(NodeBase node)
        {
            if (!IsTileSettingsNode(node))
                return false;

            var property = node.GetType().GetProperty(
                "HasRenderableTileOutput",
                BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.PropertyType == typeof(bool))
                return (bool)property.GetValue(node);

            return false;
        }

        private static string ResolveLayerOutputKindName(NodeBase outputNode)
        {
            var property = outputNode?.GetType().GetProperty(
                "OutputKind",
                BindingFlags.Instance | BindingFlags.Public);
            var value = property?.GetValue(outputNode);
            return value?.ToString() ?? "Other";
        }

        private static string[] GetExpectedOutputInputNames(string kindName)
        {
            return kindName switch
            {
                "Tiles" => new[] { "BiomeMap", "HeightMap", "Mask" },
                "Objects" => new[] { "ObjectMap", "Data" },
                "Masks" => new[] { "Mask" },
                "InternalData" => new[] { "Data" },
                _ => System.Array.Empty<string>()
            };
        }

        private static bool PortNameMatches(string inputName, string expectedName)
        {
            if (string.IsNullOrWhiteSpace(inputName) || string.IsNullOrWhiteSpace(expectedName))
                return false;

            return inputName == expectedName
                || inputName.StartsWith(expectedName + " ", System.StringComparison.Ordinal);
        }

        private static bool IsOptionalInput(NodeBase node, string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
                return false;

            string nodeTypeName = node?.GetType().Name;
            if (nodeTypeName == "OutputNode")
                return true;

            if (nodeTypeName == "TileSettingsNode" && portName == "Mask")
                return true;

            if (nodeTypeName == "PlacementMaskNode"
                && (portName == "Placement" || portName == "Exclude"))
                return true;

            if (nodeTypeName == "ObjectLayerNode"
                && (portName == "Exclude" || portName == "Grass"))
                return true;

            return portName.IndexOf("optional", System.StringComparison.OrdinalIgnoreCase) >= 0
                || portName.IndexOf("опцій", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}