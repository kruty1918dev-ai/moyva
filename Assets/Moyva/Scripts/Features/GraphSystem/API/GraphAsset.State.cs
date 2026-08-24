using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
public sealed partial class GraphAsset
    {
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
                ; // JSON source of truth: no ScriptableObject dirty flag.
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
                ; // JSON source of truth: no ScriptableObject dirty flag.
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
                ; // JSON source of truth: no ScriptableObject dirty flag.
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

    }
}
