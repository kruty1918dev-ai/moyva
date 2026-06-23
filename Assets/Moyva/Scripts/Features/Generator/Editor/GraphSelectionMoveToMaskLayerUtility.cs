#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Editor-only refactor command:
    ///
    /// Selected nodes in one source layer:
    ///   selected subgraph -> new helper mask layer -> Output(Masks)
    ///
    /// Original layer:
    ///   selected subgraph is removed from that layer by moving the same node objects to the new layer
    ///   LayerMaskReferenceNode(new helper layer) is created
    ///   outgoing connections from the selected terminal node are rewired to the LayerRef output
    ///
    /// This intentionally performs a safe MOVE, not deep clone + delete.
    /// Reason: TWC modifier nodes and Unity object references/sub-assets are safer when the same node object is moved
    /// to the new layer. The visible result is the same as "copy to new layer, delete from old layer".
    /// </summary>
    internal static class GraphSelectionMoveToMaskLayerUtility
    {
        internal sealed class Result
        {
            public bool Success;
            public string Message;
            public GeneratorLayerDefinition SourceLayer;
            public GeneratorLayerDefinition NewLayer;
            public LayerMaskReferenceNode ReplacementLayerRef;
            public OutputNode NewOutputNode;
            public readonly List<NodeBase> MovedNodes = new();
            public readonly List<Connection> CreatedConnections = new();
            public readonly List<Connection> RemovedConnections = new();
        }

        private readonly struct Terminal
        {
            public readonly NodeBase Node;
            public readonly int OutputIndex;
            public readonly List<Connection> OutgoingBoundaryConnections;

            public Terminal(NodeBase node, int outputIndex, List<Connection> outgoingBoundaryConnections)
            {
                Node = node;
                OutputIndex = outputIndex;
                OutgoingBoundaryConnections = outgoingBoundaryConnections ?? new List<Connection>();
            }

            public bool IsValid => Node != null && OutputIndex >= 0;
        }

        public static Result MoveSelectedNodesToNewMaskLayer(
            GraphAsset graph,
            IReadOnlyList<NodeBase> selectedNodes,
            string layerName = null)
        {
            var result = new Result();

            if (graph == null)
                return Fail(result, "GraphAsset не задано.");

            graph.EnsureLayerGraphStates();

            var nodes = (selectedNodes ?? Array.Empty<NodeBase>())
                .Where(node => node != null)
                .Where(node => node is not OutputNode)
                .Where(node => node is not TileSettingsNode)
                .Distinct()
                .ToList();

            if (nodes.Count == 0)
                return Fail(result, "Немає валідних виділених нодів. OutputNode і TileSettingsNode не переносяться.");

            string sourceLayerId = nodes[0].LayerId;
            if (string.IsNullOrWhiteSpace(sourceLayerId))
                return Fail(result, "Перший виділений нод не має LayerId.");

            if (nodes.Any(node => node.LayerId != sourceLayerId))
                return Fail(result, "Виділення має містити ноди тільки з одного шару.");

            var sourceLayer = graph.GetLayerById(sourceLayerId);
            if (sourceLayer == null)
                return Fail(result, $"Не знайдено source layer '{sourceLayerId}'.");

            result.SourceLayer = sourceLayer;

            var allConnections = graph.Connections?.Where(connection => connection != null).ToList()
                                ?? new List<Connection>();
            var selectedIds = new HashSet<string>(nodes.Select(node => node.NodeId));

            var incomingBoundary = allConnections
                .Where(connection =>
                    selectedIds.Contains(connection.TargetNodeId)
                    && !selectedIds.Contains(connection.SourceNodeId))
                .ToList();

            if (incomingBoundary.Count > 0)
            {
                return Fail(
                    result,
                    "Виділений subgraph має incoming connections з невиділених нодів. " +
                    "Додай upstream-ноди до виділення або спочатку винеси їх в окремий helper layer.");
            }

            Terminal terminal = ResolveTerminal(graph, nodes, allConnections, selectedIds);
            if (!terminal.IsValid)
            {
                return Fail(
                    result,
                    "Не знайдено один фінальний bool[,] output у виділенні. " +
                    "Виділення має мати рівно один terminal mask output, який заміниться Layer Ref-ом.");
            }

            Undo.RegisterCompleteObjectUndo(graph, "Move Selected Nodes To Mask Layer");

            string newLayerName = string.IsNullOrWhiteSpace(layerName)
                ? BuildUniqueLayerName(graph, sourceLayer.Name + " Mask")
                : BuildUniqueLayerName(graph, layerName);

            InsertHelperLayerBeforeSource(graph, sourceLayer);

            var newLayer = graph.AddLayer(newLayerName);
            if (newLayer == null)
                return Fail(result, "Не вдалося створити новий helper mask layer.");

            result.NewLayer = newLayer;
            newLayer.Enabled = true;
            newLayer.SortingOrder = sourceLayer.SortingOrder - 1;
            newLayer.DefaultHeight = 0f;
            newLayer.GenerateFlatSurface = false;
            newLayer.ExtraWidthCells = 0;
            newLayer.ExtraLengthCells = 0;
            newLayer.Color = new Color(0.45f, 0.72f, 1f, 1f);
            newLayer.BuildLayerKey = string.Empty;

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].LayerId = newLayer.Id;
                result.MovedNodes.Add(nodes[i]);
                EditorUtility.SetDirty(nodes[i]);
            }

            Vector2 terminalPosition = terminal.Node.EditorPosition;
            var outputNode = graph.AddNode(typeof(OutputNode), false, newLayer.Id) as OutputNode;
            if (outputNode == null)
                return Fail(result, "Не вдалося створити OutputNode у helper mask layer.");

            outputNode.LayerId = newLayer.Id;
            outputNode.EditorPosition = terminalPosition + new Vector2(340f, 0f);
            SetOutputNodeKindToMasks(outputNode);
            result.NewOutputNode = outputNode;

            var outputConnection = graph.AddConnection(terminal.Node.NodeId, terminal.OutputIndex, outputNode.NodeId, 0);
            if (outputConnection != null)
                result.CreatedConnections.Add(outputConnection);

            for (int i = 0; i < terminal.OutgoingBoundaryConnections.Count; i++)
            {
                var oldConnection = terminal.OutgoingBoundaryConnections[i];
                if (RemoveConnection(graph, oldConnection))
                    result.RemovedConnections.Add(oldConnection);
            }

            var layerRef = graph.AddNode(typeof(LayerMaskReferenceNode), false, sourceLayer.Id) as LayerMaskReferenceNode;
            if (layerRef == null)
                return Fail(result, "Не вдалося створити LayerMaskReferenceNode у source layer.");

            layerRef.LayerId = sourceLayer.Id;
            layerRef.EditorPosition = terminalPosition;
            layerRef.SetSourceLayerId(newLayer.Id);
            result.ReplacementLayerRef = layerRef;

            for (int i = 0; i < terminal.OutgoingBoundaryConnections.Count; i++)
            {
                var oldConnection = terminal.OutgoingBoundaryConnections[i];
                if (oldConnection == null)
                    continue;

                var replacement = graph.AddConnection(
                    layerRef.NodeId,
                    0,
                    oldConnection.TargetNodeId,
                    oldConnection.TargetPortIndex);

                if (replacement != null)
                    result.CreatedConnections.Add(replacement);
            }

            graph.EnsureLayerGraphStates();
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();

            result.Success = true;
            result.Message =
                $"Перенесено {nodes.Count} node(s) у helper mask layer '{newLayer.Name}'. " +
                $"Створено Output(Masks), Layer Ref і перепідключено {terminal.OutgoingBoundaryConnections.Count} outgoing connection(s).";

            return result;
        }

        private static Terminal ResolveTerminal(
            GraphAsset graph,
            IReadOnlyList<NodeBase> selectedNodes,
            IReadOnlyList<Connection> allConnections,
            HashSet<string> selectedIds)
        {
            var outgoingBoundary = allConnections
                .Where(connection =>
                    selectedIds.Contains(connection.SourceNodeId)
                    && !selectedIds.Contains(connection.TargetNodeId))
                .ToList();

            if (outgoingBoundary.Count > 0)
            {
                var terminalGroups = outgoingBoundary
                    .GroupBy(connection => (connection.SourceNodeId, connection.SourcePortIndex))
                    .ToList();

                if (terminalGroups.Count != 1)
                    return default;

                var group = terminalGroups[0];
                var node = graph.GetNodeById(group.Key.SourceNodeId);
                if (FindBoolOutputIndex(node, group.Key.SourcePortIndex) < 0)
                    return default;

                return new Terminal(node, group.Key.SourcePortIndex, group.ToList());
            }

            var internalConnections = allConnections
                .Where(connection =>
                    selectedIds.Contains(connection.SourceNodeId)
                    && selectedIds.Contains(connection.TargetNodeId))
                .ToList();

            var terminalCandidates = selectedNodes
                .SelectMany(node => EnumerateBoolOutputs(node)
                    .Select(outputIndex => new
                    {
                        Node = node,
                        OutputIndex = outputIndex
                    }))
                .Where(candidate => !internalConnections.Any(connection =>
                    connection.SourceNodeId == candidate.Node.NodeId
                    && connection.SourcePortIndex == candidate.OutputIndex))
                .ToList();

            if (terminalCandidates.Count != 1)
                return default;

            return new Terminal(terminalCandidates[0].Node, terminalCandidates[0].OutputIndex, new List<Connection>());
        }

        private static int FindBoolOutputIndex(NodeBase node, int requiredOutputIndex)
        {
            if (node?.Outputs == null)
                return -1;

            if (requiredOutputIndex < 0 || requiredOutputIndex >= node.Outputs.Length)
                return -1;

            return node.Outputs[requiredOutputIndex].ValueType == typeof(bool[,])
                ? requiredOutputIndex
                : -1;
        }

        private static IEnumerable<int> EnumerateBoolOutputs(NodeBase node)
        {
            if (node?.Outputs == null)
                yield break;

            for (int i = 0; i < node.Outputs.Length; i++)
            {
                if (node.Outputs[i].ValueType == typeof(bool[,]))
                    yield return i;
            }
        }

        private static void InsertHelperLayerBeforeSource(GraphAsset graph, GeneratorLayerDefinition sourceLayer)
        {
            if (graph?.Layers == null || sourceLayer == null)
                return;

            int sourceOrder = sourceLayer.SortingOrder;
            foreach (var layer in graph.Layers)
            {
                if (layer == null || layer == sourceLayer)
                    continue;

                if (layer.SortingOrder >= sourceOrder)
                    layer.SortingOrder += 1;
            }

            sourceLayer.SortingOrder = sourceOrder + 1;
        }

        private static string BuildUniqueLayerName(GraphAsset graph, string baseName)
        {
            baseName = string.IsNullOrWhiteSpace(baseName) ? "Mask Layer" : baseName.Trim();

            var names = new HashSet<string>(
                graph?.Layers?.Where(layer => layer != null).Select(layer => layer.Name)
                ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            if (!names.Contains(baseName))
                return baseName;

            for (int i = 2; i < 10000; i++)
            {
                string candidate = $"{baseName} {i}";
                if (!names.Contains(candidate))
                    return candidate;
            }

            return baseName + " " + Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        private static void SetOutputNodeKindToMasks(OutputNode outputNode)
        {
            if (outputNode == null)
                return;

            var serialized = new SerializedObject(outputNode);

            if (TrySetSerializedEnum(serialized, "_outputKind", "Masks")
                || TrySetSerializedEnum(serialized, "_kind", "Masks")
                || TrySetSerializedEnum(serialized, "outputKind", "Masks")
                || TrySetSerializedEnum(serialized, "OutputKind", "Masks"))
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(outputNode);
                return;
            }

            TrySetMemberByName(outputNode, "OutputKind", "Masks");
            TrySetMemberByName(outputNode, "_outputKind", "Masks");
            EditorUtility.SetDirty(outputNode);
        }

        private static bool TrySetSerializedEnum(SerializedObject serialized, string propertyName, string enumName)
        {
            var prop = serialized.FindProperty(propertyName);
            if (prop == null)
                return false;

            if (prop.propertyType == SerializedPropertyType.Enum)
            {
                int index = Array.FindIndex(prop.enumNames, name => string.Equals(name, enumName, StringComparison.OrdinalIgnoreCase));
                if (index < 0)
                    index = Array.FindIndex(prop.enumDisplayNames, name => string.Equals(name, enumName, StringComparison.OrdinalIgnoreCase));

                if (index < 0)
                    return false;

                prop.enumValueIndex = index;
                return true;
            }

            if (prop.propertyType == SerializedPropertyType.String)
            {
                prop.stringValue = enumName;
                return true;
            }

            return false;
        }

        private static bool TrySetMemberByName(object target, string memberName, string enumName)
        {
            if (target == null)
                return false;

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var type = target.GetType();

            var prop = type.GetProperty(memberName, flags);
            if (prop != null && prop.CanWrite)
            {
                object converted = ConvertValue(prop.PropertyType, enumName);
                if (converted != null)
                {
                    prop.SetValue(target, converted);
                    return true;
                }
            }

            var field = type.GetField(memberName, flags);
            if (field != null)
            {
                object converted = ConvertValue(field.FieldType, enumName);
                if (converted != null)
                {
                    field.SetValue(target, converted);
                    return true;
                }
            }

            return false;
        }

        private static object ConvertValue(Type type, string enumName)
        {
            if (type == typeof(string))
                return enumName;

            if (type != null && type.IsEnum && Enum.TryParse(type, enumName, true, out object value))
                return value;

            return null;
        }

        private static bool RemoveConnection(GraphAsset graph, Connection connection)
        {
            if (graph == null || connection == null)
                return false;

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var graphType = graph.GetType();

            foreach (var methodName in new[] { "RemoveConnection", "DeleteConnection" })
            {
                var methods = graphType.GetMethods(flags).Where(method => method.Name == methodName).ToList();
                foreach (var method in methods)
                {
                    var parameters = method.GetParameters();
                    try
                    {
                        if (parameters.Length == 1 && parameters[0].ParameterType.IsInstanceOfType(connection))
                        {
                            method.Invoke(graph, new object[] { connection });
                            return true;
                        }

                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                        {
                            method.Invoke(graph, new object[] { connection.ConnectionId });
                            return true;
                        }
                    }
                    catch
                    {
                        // Fall through to list removal.
                    }
                }
            }

            if (graph.Connections is IList list)
                return list.Contains(connection) && RemoveFromIList(list, connection);

            var field = graphType.GetField("_connections", flags);
            if (field?.GetValue(graph) is IList fieldList)
                return fieldList.Contains(connection) && RemoveFromIList(fieldList, connection);

            return false;
        }

        private static bool RemoveFromIList(IList list, object value)
        {
            if (list == null || value == null)
                return false;

            int index = list.IndexOf(value);
            if (index < 0)
                return false;

            list.RemoveAt(index);
            return true;
        }

        private static Result Fail(Result result, string message)
        {
            result.Success = false;
            result.Message = message;
            return result;
        }
    }
}
#endif