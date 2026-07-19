using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Єдиний editor-шлях створення вузлів із перевіркою каталогу, портів і Undo.
    /// </summary>
    public static class GraphNodeFactory
    {
        public static bool TryCreate(
            GraphAsset graph,
            Type nodeType,
            string layerId,
            Vector2 position,
            out NodeBase node,
            out string error)
        {
            node = null;
            if (!GraphNodeCatalog.TryGet(nodeType, out var entry))
            {
                error = $"Тип '{nodeType?.FullName ?? "<null>"}' відсутній у каталозі вузлів.";
                return false;
            }

            return TryCreate(
                graph,
                entry,
                layerId,
                position,
                initializer: null,
                out node,
                out error);
        }

        public static bool TryCreate(
            GraphAsset graph,
            NodeCatalogEntry entry,
            string layerId,
            Vector2 position,
            out NodeBase node,
            out string error)
        {
            return TryCreate(
                graph,
                entry,
                layerId,
                position,
                initializer: null,
                out node,
                out error);
        }

        public static bool TryCreate(
            GraphAsset graph,
            Type nodeType,
            string layerId,
            Vector2 position,
            Action<NodeBase> initializer,
            out NodeBase node,
            out string error)
        {
            node = null;
            if (!GraphNodeCatalog.TryGet(nodeType, out var entry))
            {
                error = $"Тип '{nodeType?.FullName ?? "<null>"}' відсутній у каталозі вузлів.";
                return false;
            }

            return TryCreate(
                graph,
                entry,
                layerId,
                position,
                initializer,
                out node,
                out error);
        }

        public static bool TryCreate(
            GraphAsset graph,
            NodeCatalogEntry entry,
            string layerId,
            Vector2 position,
            Action<NodeBase> initializer,
            out NodeBase node,
            out string error)
        {
            node = null;
            error = null;
            if (graph == null)
            {
                error = "GraphAsset не призначено.";
                return false;
            }
            if (entry?.Descriptor == null)
            {
                error = "Descriptor вузла відсутній.";
                return false;
            }
            if (!entry.Descriptor.IsCreatable
                && entry.IsTwcModifier
                && string.Equals(
                    entry.Descriptor.UnavailableReason,
                    "Contract smoke-test is pending.",
                    StringComparison.Ordinal))
            {
                TwcModifierContractProbe.TryValidateNow(
                    entry.TwcModifierType,
                    entry.Descriptor.Inputs.Count == 0,
                    out _);
                GraphNodeCatalog.Invalidate();
                GraphNodeCatalog.TryGetTwcModifier(
                    entry.TwcModifierType,
                    out entry);
            }
            if (entry?.Descriptor == null || !entry.Descriptor.IsCreatable)
            {
                error = entry?.Descriptor?.UnavailableReason
                        ?? (entry?.Descriptor != null
                            ? $"Вузол '{entry.Descriptor.Title}' прихований або застарілий."
                            : "Descriptor вузла не вдалося оновити після contract-smoke-test.");
                return false;
            }

            Type nodeType = entry.Descriptor.NodeType;
            if (nodeType == null || !typeof(NodeBase).IsAssignableFrom(nodeType))
            {
                error = "Descriptor містить некоректний C#-тип вузла.";
                return false;
            }
            string resolvedLayerId =
                Attribute.IsDefined(nodeType, typeof(StaticGraphNodeAttribute))
                    ? layerId
                    : string.IsNullOrEmpty(layerId)
                        ? graph.EnsureDefaultLayer()
                        : layerId;
            if (Attribute.IsDefined(nodeType, typeof(UniqueNodeAttribute))
                && ContainsNodeOfType(graph, nodeType))
            {
                error = $"Граф уже містить унікальний вузол '{entry.Descriptor.Title}'.";
                return false;
            }
            if (typeof(IGraphOutputNode).IsAssignableFrom(nodeType)
                && ContainsLayerOutput(graph, resolvedLayerId))
            {
                error =
                    $"Шар '{graph.GetLayerById(resolvedLayerId)?.Name ?? resolvedLayerId}' " +
                    "вже має фінальний Output.";
                return false;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Create {entry.Descriptor.Title}");
            Undo.RecordObject(graph, $"Create {entry.Descriptor.Title}");

            try
            {
                node = graph.AddNode(nodeType, false, resolvedLayerId);
                if (node == null)
                    throw new InvalidOperationException("GraphAsset відхилив створення вузла.");

                string generatedNodeId = node.NodeId;
                Undo.RegisterCreatedObjectUndo(node, $"Create {entry.Descriptor.Title}");
                if (entry.IsTwcModifier)
                {
                    if (node is not TwcModifierNode twcNode)
                        throw new InvalidOperationException("TWC descriptor створив неправильний тип вузла.");

                    twcNode.ConfigureModifier(entry.TwcModifierType);
                    if (twcNode.ModifierAsset == null)
                        throw new InvalidOperationException("Не вдалося створити TWC modifier subasset.");
                    Undo.RegisterCreatedObjectUndo(
                        twcNode.ModifierAsset,
                        $"Create {entry.Descriptor.Title} Modifier");
                }

                initializer?.Invoke(node);
                node.NodeId = generatedNodeId;
                if (!Attribute.IsDefined(nodeType, typeof(StaticGraphNodeAttribute)))
                    node.LayerId = resolvedLayerId;
                node.EditorPosition = position;
                if (!ValidateCreatedNode(node, entry.Descriptor, out error))
                    throw new InvalidOperationException(error);

                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(graph);
                graph.EnsureLayerGraphStates();
                Undo.CollapseUndoOperations(undoGroup);
                return true;
            }
            catch (Exception exception)
            {
                error = string.IsNullOrWhiteSpace(error) ? exception.Message : error;
                Undo.RevertAllDownToGroup(undoGroup);
                node = null;
                return false;
            }
        }

        internal static bool TryCreateManagedStaticNode(
            GraphAsset graph,
            Type nodeType,
            Vector2 position,
            out NodeBase node,
            out string error)
        {
            node = null;
            error = null;
            if (graph == null
                || nodeType == null
                || !typeof(NodeBase).IsAssignableFrom(nodeType)
                || !Attribute.IsDefined(nodeType, typeof(StaticGraphNodeAttribute)))
            {
                error = "Некоректний managed static node type.";
                return false;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Create {nodeType.Name}");
            Undo.RecordObject(graph, $"Create {nodeType.Name}");
            try
            {
                node = graph.AddNode(nodeType, allowStaticGraphNode: true);
                if (node == null)
                {
                    throw new InvalidOperationException(
                        $"Не вдалося створити managed static node '{nodeType.Name}'.");
                }

                node.EditorPosition = position;
                Undo.RegisterCreatedObjectUndo(node, $"Create {nodeType.Name}");
                if (!ValidateStableIds(
                        node.Inputs ?? Array.Empty<PortDefinition>(),
                        out error)
                    || !ValidateStableIds(
                        node.Outputs ?? Array.Empty<PortDefinition>(),
                        out error))
                {
                    throw new InvalidOperationException(error);
                }

                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(graph);
                graph.EnsureLayerGraphStates();
                Undo.CollapseUndoOperations(undoGroup);
                return true;
            }
            catch (Exception exception)
            {
                error = string.IsNullOrWhiteSpace(error)
                    ? exception.Message
                    : error;
                Undo.RevertAllDownToGroup(undoGroup);
                node = null;
                return false;
            }
        }

        private static bool ValidateCreatedNode(
            NodeBase node,
            NodeDescriptor descriptor,
            out string error)
        {
            error = null;
            var inputs = node.Inputs ?? Array.Empty<PortDefinition>();
            var outputs = node.Outputs ?? Array.Empty<PortDefinition>();
            if (inputs.Length != descriptor.Inputs.Count
                || outputs.Length != descriptor.Outputs.Count)
            {
                error =
                    $"Контракт портів змінився під час створення: " +
                    $"inputs {inputs.Length}/{descriptor.Inputs.Count}, " +
                    $"outputs {outputs.Length}/{descriptor.Outputs.Count}.";
                return false;
            }

            if (!ValidateStableIds(inputs, out error)
                || !ValidateStableIds(outputs, out error))
                return false;

            return ValidatePortContracts(
                       inputs,
                       descriptor.Inputs,
                       out error)
                   && ValidatePortContracts(
                       outputs,
                       descriptor.Outputs,
                       out error);
        }

        private static bool ValidatePortContracts(
            IReadOnlyList<PortDefinition> actualPorts,
            IReadOnlyList<PortDefinition> descriptorPorts,
            out string error)
        {
            for (int i = 0; i < actualPorts.Count; i++)
            {
                var actual = actualPorts[i];
                var expected = descriptorPorts[i];
                if (actual == null || expected == null)
                {
                    error = $"Контракт порту з індексом {i} містить null.";
                    return false;
                }
                if (!string.Equals(
                        actual.Id,
                        expected.Id,
                        StringComparison.Ordinal))
                {
                    error =
                        $"Stable port ID змінився з '{expected.Id}' на " +
                        $"'{actual.Id}' під час створення.";
                    return false;
                }
                if (actual.Direction != expected.Direction
                    || actual.IsRequired != expected.IsRequired
                    || actual.AllowNull != expected.AllowNull)
                {
                    error =
                        $"Порт '{actual.Id}' змінив direction/required/null " +
                        "контракт під час створення.";
                    return false;
                }
                if (!expected.AcceptsAnyValue
                    && (actual.AcceptsAnyValue
                        || actual.ValueType != expected.ValueType))
                {
                    error =
                        $"Порт '{actual.Id}' змінив тип із " +
                        $"'{expected.ValueType.Name}' на " +
                        $"'{actual.ValueType.Name}' під час створення.";
                    return false;
                }
                if (expected.MapSizePolicy != PortMapSizePolicy.Variable
                    && actual.MapSizePolicy != expected.MapSizePolicy)
                {
                    error =
                        $"Порт '{actual.Id}' змінив map-size policy з " +
                        $"'{expected.MapSizePolicy}' на " +
                        $"'{actual.MapSizePolicy}' під час створення.";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private static bool ValidateStableIds(
            IReadOnlyList<PortDefinition> ports,
            out string error)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < ports.Count; i++)
            {
                string id = ports[i]?.Id;
                if (string.IsNullOrWhiteSpace(id))
                {
                    error = $"Порт із індексом {i} не має stable ID.";
                    return false;
                }
                if (!ids.Add(id))
                {
                    error = $"Stable port ID '{id}' дублюється.";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private static bool ContainsNodeOfType(GraphAsset graph, Type nodeType)
        {
            var nodes = graph?.Nodes;
            if (nodes == null)
                return false;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null && nodes[i].GetType() == nodeType)
                    return true;
            }

            return false;
        }

        private static bool ContainsLayerOutput(
            GraphAsset graph,
            string layerId)
        {
            if (graph?.Nodes == null || string.IsNullOrEmpty(layerId))
                return false;

            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                var node = graph.Nodes[i];
                if (node is IGraphOutputNode
                    && string.Equals(
                        node.LayerId,
                        layerId,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
