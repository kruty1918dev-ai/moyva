using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.GraphSystem.API
{
public sealed partial class GraphAsset
    {
#if UNITY_EDITOR
        public GeneratorLayerDefinition AddLayer(string name = "Layer")
        {
            EnsureLayerGraphStates();
            var layer = new GeneratorLayerDefinition(name);
            _layers.Add(layer);
            _layerGraphStates.Add(new LayerGraphState(layer.Id));
            ; // JSON source of truth: no ScriptableObject dirty flag.
            return layer;
        }

        /// <summary>
        /// Видаляє шар та всі вузли його підграфа. Не дозволяє видалити останній шар.
        /// </summary>
        public bool RemoveLayer(string layerId, bool registerUndo = false)
        {
            if (string.IsNullOrEmpty(layerId) || _layers.Count <= 1)
                return false;

            var layer = GetLayerById(layerId);
            if (layer == null)
                return false;

            EnsureLayerGraphStates();
            var layerNodes = GetNodesForLayer(layerId);
            RemoveNodesCascade(layerNodes, registerUndo);

            _layers.Remove(layer);
            _layerGraphStates.RemoveAll(state => state == null || state.LayerId == layerId);
            ; // JSON source of truth: no ScriptableObject dirty flag.
            return true;
        }

        public NodeBase AddNode(Type nodeType, bool allowStaticGraphNode = false) =>
            AddNode(nodeType, allowStaticGraphNode, null);

        public NodeBase AddNode(Type nodeType, bool allowStaticGraphNode, string layerId)
        {
            EnsureLayerGraphStates();
            if (!allowStaticGraphNode && nodeType != null && Attribute.IsDefined(nodeType, typeof(StaticGraphNodeAttribute)))
            {
                Debug.LogWarning($"Static graph node '{nodeType.Name}' is managed automatically and cannot be added manually.");
                return null;
            }

            if (nodeType != null && Attribute.IsDefined(nodeType, typeof(UniqueNodeAttribute)))
            {
                for (int i = 0; i < _nodes.Count; i++)
                {
                    if (_nodes[i] != null && _nodes[i].GetType() == nodeType)
                    {
                        Debug.LogWarning($"Graph already contains unique node '{nodeType.Name}'.");
                        return null;
                    }
                }
            }

            var node = MoyvaJsonObjectFactory.Create(nodeType) as NodeBase;
            if (node == null) return null;

            node.name = nodeType.Name;
            ; // JSON config object has no Unity hideFlags.
            if (!Attribute.IsDefined(nodeType, typeof(StaticGraphNodeAttribute)))
                node.LayerId = string.IsNullOrEmpty(layerId) ? EnsureDefaultLayer() : layerId;
            _nodes.Add(node);
            EnsureLayerGraphStates();

            ; // JSON config object is not stored as a Unity subasset.
            ; // JSON source of truth: no ScriptableObject dirty flag.
            return node;
        }

        public T AddNode<T>(bool allowStaticGraphNode = false) where T : NodeBase => AddNode(typeof(T), allowStaticGraphNode) as T;

        public void RemoveNode(NodeBase node) => RemoveNodeCascade(node, false);

        public void RemoveNodeCascade(NodeBase node, bool registerUndo = false)
        {
            if (node == null) return;

            if (Attribute.IsDefined(node.GetType(), typeof(StaticGraphNodeAttribute)))
            {
                Debug.LogWarning($"Static graph node '{node.Title}' is required and cannot be removed.");
                return;
            }

            RemoveConnectionsForNode(node.NodeId);
            _nodes.Remove(node);
            EnsureLayerGraphStates();

            if (registerUndo)
            {
                MoyvaJsonObjectFactory.DestroyImmediate(node);
            }
            else
            {
                ; // JSON config object is not stored as a Unity subasset.
                MoyvaJsonObjectFactory.DestroyImmediate(node, true);
            }
            ; // JSON source of truth: no ScriptableObject dirty flag.
        }

        public int RemoveNodesCascade(IEnumerable<NodeBase> nodes, bool registerUndo = false)
        {
            if (nodes == null)
                return 0;

            var candidates = nodes
                .Where(node => node != null && !Attribute.IsDefined(node.GetType(), typeof(StaticGraphNodeAttribute)))
                .Distinct()
                .ToList();
            if (candidates.Count == 0)
                return 0;

            RemoveConnectionsForNodes(candidates.Select(node => node.NodeId));

            int removed = 0;
            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                var node = candidates[i];
                if (!_nodes.Remove(node))
                    continue;

                removed++;
                if (registerUndo)
                {
                    MoyvaJsonObjectFactory.DestroyImmediate(node);
                }
                else
                {
                    ; // JSON config object is not stored as a Unity subasset.
                    MoyvaJsonObjectFactory.DestroyImmediate(node, true);
                }
            }

            if (removed > 0)
            {
                EnsureLayerGraphStates();
                ; // JSON source of truth: no ScriptableObject dirty flag.
            }

            return removed;
        }

        /// <summary>
        /// Видаляє null-записи з Nodes (наприклад, після видалення скрипту)
        /// та очищає з'єднання, що вказують на неіснуючі вузли.
        /// Повертає кількість видалених null-нод.
        /// </summary>
        public int RemoveNullNodes()
        {
            int removed = 0;
            for (int i = _nodes.Count - 1; i >= 0; i--)
            {
                if (_nodes[i] == null)
                {
                    _nodes.RemoveAt(i);
                    removed++;
                }
            }

            if (removed > 0)
            {
                // Remove connections whose source or target no longer exists
                var validIds = new HashSet<string>();
                foreach (var n in _nodes)
                    if (n != null) validIds.Add(n.NodeId);

                _connections.RemoveAll(c =>
                    !validIds.Contains(c.SourceNodeId) || !validIds.Contains(c.TargetNodeId));
                EnsureLayerGraphStates();

                ; // JSON source of truth: no ScriptableObject dirty flag.
            }

            return removed;
        }

        public int RemoveInvalidConnections()
        {
            _nodes ??= new List<NodeBase>();
            _connections ??= new List<Connection>();

            var nodesById = new Dictionary<string, NodeBase>();
            foreach (var node in _nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.NodeId))
                    continue;
                if (!nodesById.ContainsKey(node.NodeId))
                    nodesById.Add(node.NodeId, node);
            }

            int removed = 0;
            for (int i = _connections.Count - 1; i >= 0; i--)
            {
                var connection = _connections[i];
                if (!IsConnectionValid(connection, nodesById))
                {
                    _connections.RemoveAt(i);
                    removed++;
                }
            }

            if (removed > 0)
            {
                EnsureLayerGraphStates();
                ; // JSON source of truth: no ScriptableObject dirty flag.
            }

            return removed;
        }

        private static bool IsConnectionValid(Connection connection, IReadOnlyDictionary<string, NodeBase> nodesById)
        {
            if (connection == null || nodesById == null)
                return false;
            if (!nodesById.TryGetValue(connection.SourceNodeId, out var source))
                return false;
            if (!nodesById.TryGetValue(connection.TargetNodeId, out var target))
                return false;

            var outputs = source.Outputs;
            var inputs = target.Inputs;
            if (outputs == null || inputs == null)
                return false;
            if (connection.SourcePortIndex < 0 || connection.SourcePortIndex >= outputs.Length)
                return false;
            if (connection.TargetPortIndex < 0 || connection.TargetPortIndex >= inputs.Length)
                return false;

            var sourceType = outputs[connection.SourcePortIndex].ValueType;
            var targetType = inputs[connection.TargetPortIndex].ValueType;
            return PortDefinition.AreValueTypesCompatible(sourceType, targetType)
                || IsLegacyMaskOutputConnection(connection, sourceType, target);
        }

        private static bool IsLegacyMaskOutputConnection(Connection connection, Type sourceType, NodeBase target)
        {
            return connection != null
                && sourceType == typeof(bool[,])
                && target?.GetType().Name == "OutputNode"
                && ResolveLayerOutputKindName(target) == "Masks"
                && connection.TargetPortIndex == 0;
        }

        private static string ResolveLayerOutputKindName(NodeBase outputNode)
        {
            var property = outputNode?.GetType().GetProperty("OutputKind");
            var value = property?.GetValue(outputNode);
            return value?.ToString() ?? "Other";
        }

        public void ReorderNodes(IReadOnlyList<NodeBase> orderedNodes)
        {
            if (orderedNodes == null || orderedNodes.Count == 0)
                return;

            var ordered = new List<NodeBase>(_nodes.Count);
            var seen = new HashSet<NodeBase>();

            for (int index = 0; index < orderedNodes.Count; index++)
            {
                var node = orderedNodes[index];
                if (node != null && _nodes.Contains(node) && seen.Add(node))
                    ordered.Add(node);
            }

            for (int index = 0; index < _nodes.Count; index++)
            {
                var node = _nodes[index];
                if (node != null && seen.Add(node))
                    ordered.Add(node);
            }

            _nodes.Clear();
            _nodes.AddRange(ordered);
            ; // JSON source of truth: no ScriptableObject dirty flag.
        }

        /// <summary>
        /// Reconnects chains that pass through missing node IDs still referenced by connections.
        /// This repairs legacy graphs where an intermediate passthrough node script was removed.
        /// Returns the number of missing node IDs that were processed.
        /// </summary>
        public int RepairMissingNodeConnections()
        {
            var validIds = new HashSet<string>();
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i] != null)
                    validIds.Add(_nodes[i].NodeId);
            }

            var missingIds = new HashSet<string>();
            for (int i = 0; i < _connections.Count; i++)
            {
                var connection = _connections[i];
                if (connection == null)
                    continue;
                if (!validIds.Contains(connection.SourceNodeId))
                    missingIds.Add(connection.SourceNodeId);
                if (!validIds.Contains(connection.TargetNodeId))
                    missingIds.Add(connection.TargetNodeId);
            }

            int repaired = 0;
            foreach (var missingId in missingIds)
            {
                if (string.IsNullOrEmpty(missingId))
                    continue;

                var incoming = new List<Connection>();
                var outgoing = new List<Connection>();

                for (int i = 0; i < _connections.Count; i++)
                {
                    var connection = _connections[i];
                    if (connection == null)
                        continue;
                    if (connection.TargetNodeId == missingId && connection.SourceNodeId != missingId)
                        incoming.Add(connection);
                    if (connection.SourceNodeId == missingId && connection.TargetNodeId != missingId)
                        outgoing.Add(connection);
                }

                for (int i = 0; i < incoming.Count; i++)
                {
                    var source = incoming[i];
                    for (int j = 0; j < outgoing.Count; j++)
                    {
                        var target = outgoing[j];
                        var newConnection = AddConnection(
                            source.SourceNodeId,
                            source.SourcePortIndex,
                            target.TargetNodeId,
                            target.TargetPortIndex);
                        newConnection.SetSourceElementIndex(source.SourceElementIndex);
                    }
                }

                _connections.RemoveAll(c =>
                    c.SourceNodeId == missingId || c.TargetNodeId == missingId);

                if (incoming.Count > 0 || outgoing.Count > 0)
                    repaired++;
            }

            if (repaired > 0)
                EnsureLayerGraphStates();

            return repaired;
        }

        public void ClearAll()
        {
            _connections.Clear();
            for (int i = _nodes.Count - 1; i >= 0; i--)
            {
                var node = _nodes[i];
                if (node != null)
                {
                    MoyvaJsonObjectFactory.DestroyImmediate(node, true);
                }
            }
            _nodes.Clear();
            _globalGraphState.Clear();
            _layerGraphStates.Clear();
            ; // JSON source of truth: no ScriptableObject dirty flag.
        }
#endif
    }
}
