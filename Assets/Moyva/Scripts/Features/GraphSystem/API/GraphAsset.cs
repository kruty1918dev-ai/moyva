using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/Graph Asset", fileName = "NewGeneratorGraph")]
    public sealed class GraphAsset : ScriptableObject
    {
        [SerializeField] private List<NodeBase> _nodes = new();
        [SerializeField] private List<Connection> _connections = new();
        [SerializeField] private int _version = 2;

        [SerializeField] private GraphSharedSettings _sharedSettings = new();
        [SerializeField] private LayerGraphState _globalGraphState = new(string.Empty);

        [Header("Layers")]
        [Tooltip("Шари генератора. Кожен шар має власний підграф і компілюється в blueprint-шар TileWorldCreator.")]
        [SerializeField] private List<GeneratorLayerDefinition> _layers = new();
        [SerializeField] private List<LayerGraphState> _layerGraphStates = new();

        [Header("Tile Registry")]
        [Tooltip("Реєстр тайлів цього графа.")]
        [SerializeField] private TileRegistrySO _tileRegistry;

        public IReadOnlyList<NodeBase> Nodes => _nodes;
        public IReadOnlyList<Connection> Connections => _connections;
        public int Version => _version;
        public IReadOnlyList<GeneratorLayerDefinition> Layers => _layers;
        public IReadOnlyList<LayerGraphState> LayerGraphStates => _layerGraphStates;
        public LayerGraphState GlobalGraphState => _globalGraphState ??= new LayerGraphState(string.Empty);

        /// <summary>
        /// Спільні налаштування графа (розмір мапи тощо).
        /// Змінюються прямо в інспекторі GraphAsset — без потреби в окремому ноді.
        /// </summary>
        public GraphSharedSettings SharedSettings => _sharedSettings ??= new GraphSharedSettings();
        public TileRegistrySO TileRegistry => _tileRegistry;

        public NodeBase GetNodeById(string nodeId)
        {
            EnsureLayerGraphStates();
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
            EnsureLayerGraphStates();
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

        public LayerGraphState GetLayerGraphState(string layerId)
        {
            EnsureLayerGraphStates();

            if (string.IsNullOrEmpty(layerId))
                return GlobalGraphState;

            for (int i = 0; i < _layerGraphStates.Count; i++)
            {
                var state = _layerGraphStates[i];
                if (state != null && state.LayerId == layerId)
                    return state;
            }

            return null;
        }

        public List<Connection> GetConnectionsForLayer(string layerId, bool includeGlobal = true)
        {
            EnsureLayerGraphStates();

            var result = new List<Connection>();
            var state = GetLayerGraphState(layerId);
            AddConnectionsFromState(state, result);

            if (includeGlobal && !string.IsNullOrEmpty(layerId))
                AddConnectionsFromState(GlobalGraphState, result);

            return result;
        }

        public GraphExecutionScope CreateExecutionScope(string layerId, bool includeGlobal = true)
        {
            EnsureLayerGraphStates();

            if (string.IsNullOrEmpty(layerId))
            {
                return new GraphExecutionScope(
                    this,
                    null,
                    GlobalGraphState.GraphId,
                    new List<NodeBase>(_nodes),
                    new List<Connection>(_connections));
            }

            var state = GetLayerGraphState(layerId);
            var nodes = new List<NodeBase>();
            var connections = new List<Connection>();

            if (includeGlobal)
                AddNodesFromState(GlobalGraphState, nodes);
            AddNodesFromState(state, nodes);

            if (includeGlobal)
                AddConnectionsFromState(GlobalGraphState, connections);
            AddConnectionsFromState(state, connections);

            return new GraphExecutionScope(
                this,
                layerId,
                state?.GraphId,
                nodes,
                connections);
        }

        public List<GraphExecutionScope> CreateEnabledLayerExecutionScopes(bool includeGlobal = true)
        {
            EnsureLayerGraphStates();

            var result = new List<GraphExecutionScope>();
            var orderedLayers = _layers
                .Where(layer => layer != null && layer.Enabled)
                .OrderBy(layer => layer.SortingOrder)
                .ThenBy(layer => layer.Name, StringComparer.Ordinal)
                .ToList();

            for (int i = 0; i < orderedLayers.Count; i++)
            {
                var layer = orderedLayers[i];
                result.Add(CreateExecutionScope(layer.Id, includeGlobal));
            }

            return result;
        }

        public bool EnsureLayerGraphStates()
        {
            bool changed = false;

            _nodes ??= new List<NodeBase>();
            _connections ??= new List<Connection>();
            _layers ??= new List<GeneratorLayerDefinition>();
            _layerGraphStates ??= new List<LayerGraphState>();
            _globalGraphState ??= new LayerGraphState(string.Empty);

            if (_layers.Count == 0)
            {
                _layers.Add(new GeneratorLayerDefinition("Base"));
                changed = true;
            }

            string defaultLayerId = _layers[0].Id;
            var validLayerIds = new HashSet<string>();
            for (int i = 0; i < _layers.Count; i++)
            {
                var layer = _layers[i];
                if (layer == null)
                    continue;

                validLayerIds.Add(layer.Id);
                if (FindLayerGraphState(layer.Id) == null)
                {
                    _layerGraphStates.Add(new LayerGraphState(layer.Id));
                    changed = true;
                }
            }

            for (int i = _layerGraphStates.Count - 1; i >= 0; i--)
            {
                var state = _layerGraphStates[i];
                if (state == null || string.IsNullOrEmpty(state.LayerId) || !validLayerIds.Contains(state.LayerId))
                {
                    _layerGraphStates.RemoveAt(i);
                    changed = true;
                }
            }

            for (int i = _connections.Count - 1; i >= 0; i--)
            {
                if (_connections[i] == null)
                {
                    _connections.RemoveAt(i);
                    changed = true;
                }
            }

            changed |= NormalizeGraphIds() > 0;

            GlobalGraphState.Clear();
            for (int i = 0; i < _layerGraphStates.Count; i++)
                _layerGraphStates[i]?.Clear();

            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                if (node == null)
                    continue;

                _ = node.NodeId;
                bool globalNode = IsGlobalNode(node);
                if (globalNode)
                {
                    if (!string.IsNullOrEmpty(node.LayerId))
                    {
                        node.LayerId = string.Empty;
                        changed = true;
                    }
                    GlobalGraphState.AddNode(node.NodeId);
                    continue;
                }

                if (string.IsNullOrEmpty(node.LayerId) || !validLayerIds.Contains(node.LayerId))
                {
                    node.LayerId = defaultLayerId;
                    changed = true;
                }

                FindOrCreateLayerGraphState(node.LayerId, ref changed).AddNode(node.NodeId);
            }

            for (int i = 0; i < _connections.Count; i++)
            {
                var connection = _connections[i];
                if (connection == null)
                    continue;

                _ = connection.ConnectionId;
                var source = FindNodeByIdNoSync(connection.SourceNodeId);
                var target = FindNodeByIdNoSync(connection.TargetNodeId);
                string connectionLayerId = ResolveConnectionLayerId(source, target);

                if (string.IsNullOrEmpty(connectionLayerId))
                    GlobalGraphState.AddConnection(connection.ConnectionId);
                else
                    FindOrCreateLayerGraphState(connectionLayerId, ref changed).AddConnection(connection.ConnectionId);
            }

            if (changed)
            {
                _version = Mathf.Max(_version, 2);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }

            return changed;
        }

        /// <summary>
        /// Автоматично нормалізує службові ідентифікатори графа.
        /// Користувач не повинен вручну керувати NodeId/ConnectionId: під час copy/paste,
        /// import або ручного дублювання Unity sub-assets ці значення мають бути виправлені тут.
        /// </summary>
        public int NormalizeGraphIds()
        {
            _nodes ??= new List<NodeBase>();
            _connections ??= new List<Connection>();

            int changed = 0;
            changed += NormalizeNodeIdsInternal();
            changed += NormalizeConnectionIdsInternal();

#if UNITY_EDITOR
            if (changed > 0)
                UnityEditor.EditorUtility.SetDirty(this);
#endif

            return changed;
        }

        private int NormalizeNodeIdsInternal()
        {
            var seen = new HashSet<string>();
            int changed = 0;

            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                if (node == null)
                    continue;

                string id = node.NodeId;
                if (!string.IsNullOrEmpty(id) && seen.Add(id))
                    continue;

                do
                {
                    node.NodeId = Guid.NewGuid().ToString();
                    id = node.NodeId;
                }
                while (string.IsNullOrEmpty(id) || !seen.Add(id));

                changed++;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(node);
#endif
            }

            return changed;
        }

        private int NormalizeConnectionIdsInternal()
        {
            var seen = new HashSet<string>();
            int changed = 0;

            for (int i = 0; i < _connections.Count; i++)
            {
                var connection = _connections[i];
                if (connection == null)
                    continue;

                string id = connection.ConnectionId;
                if (!string.IsNullOrEmpty(id) && seen.Add(id))
                    continue;

                do
                {
                    connection.ResetConnectionId();
                    id = connection.ConnectionId;
                }
                while (string.IsNullOrEmpty(id) || !seen.Add(id));

                changed++;
            }

            return changed;
        }

        /// <summary>
        /// Гарантує наявність хоча б одного шару. Повертає id першого шару.
        /// </summary>
        public string EnsureDefaultLayer()
        {
            _layers ??= new List<GeneratorLayerDefinition>();
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
            EnsureLayerGraphStates();
            _connections.RemoveAll(c =>
                c.TargetNodeId == targetNodeId && c.TargetPortIndex == targetPort);

            var connection = new Connection(sourceNodeId, sourcePort,
                targetNodeId, targetPort);
            _connections.Add(connection);
            EnsureLayerGraphStates();
            return connection;
        }

        public Connection AddConnection(string sourceNodeId, int sourcePort,
            string targetNodeId, int targetPort, int sourceElementIndex)
        {
            var connection = AddConnection(sourceNodeId, sourcePort, targetNodeId, targetPort);
            connection.SetSourceElementIndex(sourceElementIndex);
            return connection;
        }

        public void RemoveConnection(Connection connection)
        {
            _connections.Remove(connection);
            EnsureLayerGraphStates();
        }

        public int RemoveConnectionsForNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return 0;

            return RemoveConnections(c => c.SourceNodeId == nodeId || c.TargetNodeId == nodeId);
        }

        public int RemoveConnectionsForNodes(IEnumerable<string> nodeIds)
        {
            if (nodeIds == null)
                return 0;

            var ids = new HashSet<string>(nodeIds.Where(id => !string.IsNullOrEmpty(id)));
            if (ids.Count == 0)
                return 0;

            return RemoveConnections(c => ids.Contains(c.SourceNodeId) || ids.Contains(c.TargetNodeId));
        }

        public int RemoveConnectionsByIds(IEnumerable<string> connectionIds)
        {
            if (connectionIds == null)
                return 0;

            var ids = new HashSet<string>(connectionIds.Where(id => !string.IsNullOrEmpty(id)));
            if (ids.Count == 0)
                return 0;

            return RemoveConnections(c => c != null && ids.Contains(c.ConnectionId));
        }

        private int RemoveConnections(Predicate<Connection> match)
        {
            if (match == null)
                return 0;

            int removed = _connections.RemoveAll(match);
            EnsureLayerGraphStates();
            return removed;
        }

        private NodeBase FindNodeByIdNoSync(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return null;

            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                if (node != null && node.NodeId == nodeId)
                    return node;
            }

            return null;
        }

        private LayerGraphState FindLayerGraphState(string layerId)
        {
            for (int i = 0; i < _layerGraphStates.Count; i++)
            {
                var state = _layerGraphStates[i];
                if (state != null && state.LayerId == layerId)
                    return state;
            }

            return null;
        }

        private LayerGraphState FindOrCreateLayerGraphState(string layerId, ref bool changed)
        {
            var state = FindLayerGraphState(layerId);
            if (state != null)
                return state;

            state = new LayerGraphState(layerId);
            _layerGraphStates.Add(state);
            changed = true;
            return state;
        }

        private void AddNodesFromState(LayerGraphState state, List<NodeBase> nodes)
        {
            if (state == null || nodes == null)
                return;

            for (int i = 0; i < state.NodeIds.Count; i++)
            {
                var node = FindNodeByIdNoSync(state.NodeIds[i]);
                if (node != null && !nodes.Contains(node))
                    nodes.Add(node);
            }
        }

        private void AddConnectionsFromState(LayerGraphState state, List<Connection> connections)
        {
            if (state == null || connections == null)
                return;

            for (int i = 0; i < state.ConnectionIds.Count; i++)
            {
                var connection = FindConnectionById(state.ConnectionIds[i]);
                if (connection != null && !connections.Contains(connection))
                    connections.Add(connection);
            }
        }

        private Connection FindConnectionById(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
                return null;

            for (int i = 0; i < _connections.Count; i++)
            {
                var connection = _connections[i];
                if (connection != null && connection.ConnectionId == connectionId)
                    return connection;
            }

            return null;
        }

        private static string ResolveConnectionLayerId(NodeBase source, NodeBase target)
        {
            string sourceLayerId = source != null && !IsGlobalNode(source) ? source.LayerId : null;
            string targetLayerId = target != null && !IsGlobalNode(target) ? target.LayerId : null;

            if (!string.IsNullOrEmpty(sourceLayerId))
                return sourceLayerId;

            return targetLayerId;
        }

        public static bool IsGlobalNode(NodeBase node)
        {
            if (node == null)
                return false;

            return Attribute.IsDefined(node.GetType(), typeof(StaticGraphNodeAttribute));
        }

#if UNITY_EDITOR
        public GeneratorLayerDefinition AddLayer(string name = "Layer")
        {
            EnsureLayerGraphStates();
            var layer = new GeneratorLayerDefinition(name);
            _layers.Add(layer);
            _layerGraphStates.Add(new LayerGraphState(layer.Id));
            UnityEditor.EditorUtility.SetDirty(this);
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
            UnityEditor.EditorUtility.SetDirty(this);
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

            var node = CreateInstance(nodeType) as NodeBase;
            if (node == null) return null;

            node.name = nodeType.Name;
            node.hideFlags = HideFlags.HideInHierarchy;
            if (!Attribute.IsDefined(nodeType, typeof(StaticGraphNodeAttribute)))
                node.LayerId = string.IsNullOrEmpty(layerId) ? EnsureDefaultLayer() : layerId;
            _nodes.Add(node);
            EnsureLayerGraphStates();

            UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
            UnityEditor.EditorUtility.SetDirty(this);
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
                UnityEditor.Undo.DestroyObjectImmediate(node);
            }
            else
            {
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(node);
                DestroyImmediate(node, true);
            }
            UnityEditor.EditorUtility.SetDirty(this);
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
                    UnityEditor.Undo.DestroyObjectImmediate(node);
                }
                else
                {
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(node);
                    DestroyImmediate(node, true);
                }
            }

            if (removed > 0)
            {
                EnsureLayerGraphStates();
                UnityEditor.EditorUtility.SetDirty(this);
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
                EnsureLayerGraphStates();

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
            _globalGraphState.Clear();
            _layerGraphStates.Clear();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}