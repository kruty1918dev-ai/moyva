using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/Graph Asset", fileName = "NewGeneratorGraph")]
    public sealed class GraphAsset : ScriptableObject
    {
        [SerializeField] private List<NodeBase> _nodes = new();
        [SerializeField] private List<Connection> _connections = new();
        [SerializeField] private int _version = 1;

        [SerializeField] private GraphSharedSettings _sharedSettings = new();

        [Header("Layers")]
        [Tooltip("Шари генератора. Кожен шар має власний підграф і компілюється в blueprint-шар TileWorldCreator.")]
        [SerializeField] private List<GeneratorLayerDefinition> _layers = new();

        [Header("Tile Registry")]
        [Tooltip("Реєстр тайлів цього графа.")]
        [SerializeField] private TileRegistrySO _tileRegistry;

        public IReadOnlyList<NodeBase> Nodes => _nodes;
        public IReadOnlyList<Connection> Connections => _connections;
        public int Version => _version;
        public IReadOnlyList<GeneratorLayerDefinition> Layers => _layers;

        /// <summary>
        /// Спільні налаштування графа (розмір мапи тощо).
        /// Змінюються прямо в інспекторі GraphAsset — без потреби в окремому ноді.
        /// </summary>
        public GraphSharedSettings SharedSettings => _sharedSettings ??= new GraphSharedSettings();
        public TileRegistrySO TileRegistry => _tileRegistry;

        public NodeBase GetNodeById(string nodeId)
        {
            for (int i = 0; i < _nodes.Count; i++)
                if (_nodes[i] != null && _nodes[i].NodeId == nodeId)
                    return _nodes[i];
            return null;
        }

        public GeneratorLayerDefinition GetLayerById(string layerId)
        {
            if (string.IsNullOrEmpty(layerId))
                return null;
            for (int i = 0; i < _layers.Count; i++)
                if (_layers[i] != null && _layers[i].Id == layerId)
                    return _layers[i];
            return null;
        }

        /// <summary>
        /// Повертає вузли, що належать вказаному шару. Якщо <paramref name="layerId"/>
        /// порожній — повертає вузли без призначеного шару (глобальні).
        /// </summary>
        public List<NodeBase> GetNodesForLayer(string layerId)
        {
            var result = new List<NodeBase>();
            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                if (node == null) continue;

                if (string.IsNullOrEmpty(layerId))
                {
                    if (string.IsNullOrEmpty(node.LayerId))
                        result.Add(node);
                }
                else if (node.LayerId == layerId)
                {
                    result.Add(node);
                }
            }
            return result;
        }

        /// <summary>
        /// Гарантує наявність хоча б одного шару. Повертає id першого шару.
        /// </summary>
        public string EnsureDefaultLayer()
        {
            if (_layers.Count == 0)
            {
                var layer = new GeneratorLayerDefinition("Base");
                _layers.Add(layer);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
                return layer.Id;
            }
            return _layers[0].Id;
        }

        public Connection AddConnection(string sourceNodeId, int sourcePort,
            string targetNodeId, int targetPort)
        {
            _connections.RemoveAll(c =>
                c.TargetNodeId == targetNodeId && c.TargetPortIndex == targetPort);

            var connection = new Connection(sourceNodeId, sourcePort,
                targetNodeId, targetPort);
            _connections.Add(connection);
            return connection;
        }

        public Connection AddConnection(string sourceNodeId, int sourcePort,
            string targetNodeId, int targetPort, int sourceElementIndex)
        {
            var connection = AddConnection(sourceNodeId, sourcePort, targetNodeId, targetPort);
            connection.SetSourceElementIndex(sourceElementIndex);
            return connection;
        }

        public void RemoveConnection(Connection connection) =>
            _connections.Remove(connection);

        public void RemoveConnectionsForNode(string nodeId) =>
            _connections.RemoveAll(c =>
                c.SourceNodeId == nodeId || c.TargetNodeId == nodeId);

#if UNITY_EDITOR
        public GeneratorLayerDefinition AddLayer(string name = "Layer")
        {
            var layer = new GeneratorLayerDefinition(name);
            _layers.Add(layer);
            UnityEditor.EditorUtility.SetDirty(this);
            return layer;
        }

        /// <summary>
        /// Видаляє шар та всі вузли його підграфа. Не дозволяє видалити останній шар.
        /// </summary>
        public bool RemoveLayer(string layerId)
        {
            if (string.IsNullOrEmpty(layerId) || _layers.Count <= 1)
                return false;

            var layer = GetLayerById(layerId);
            if (layer == null)
                return false;

            var layerNodes = GetNodesForLayer(layerId);
            for (int i = 0; i < layerNodes.Count; i++)
                RemoveNode(layerNodes[i]);

            _layers.Remove(layer);
            UnityEditor.EditorUtility.SetDirty(this);
            return true;
        }

        public NodeBase AddNode(Type nodeType, bool allowStaticGraphNode = false)
        {
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

            var node = CreateInstance(nodeType) as NodeBase;
            if (node == null) return null;

            node.name = nodeType.Name;
            node.hideFlags = HideFlags.HideInHierarchy;
            _nodes.Add(node);

            UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
            UnityEditor.EditorUtility.SetDirty(this);
            return node;
        }

        public T AddNode<T>(bool allowStaticGraphNode = false) where T : NodeBase => AddNode(typeof(T), allowStaticGraphNode) as T;

        public void RemoveNode(NodeBase node)
        {
            if (node == null) return;

            if (Attribute.IsDefined(node.GetType(), typeof(StaticGraphNodeAttribute)))
            {
                Debug.LogWarning($"Static graph node '{node.Title}' is required and cannot be removed.");
                return;
            }

            RemoveConnectionsForNode(node.NodeId);
            _nodes.Remove(node);

            UnityEditor.AssetDatabase.RemoveObjectFromAsset(node);
            DestroyImmediate(node, true);
            UnityEditor.EditorUtility.SetDirty(this);
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

                UnityEditor.EditorUtility.SetDirty(this);
            }

            return removed;
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
            UnityEditor.EditorUtility.SetDirty(this);
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
                UnityEditor.EditorUtility.SetDirty(this);

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
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(node);
                    DestroyImmediate(node, true);
                }
            }
            _nodes.Clear();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
