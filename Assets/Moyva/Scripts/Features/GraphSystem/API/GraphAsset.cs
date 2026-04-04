using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/Graph Asset", fileName = "NewGeneratorGraph")]
    public sealed class GraphAsset : ScriptableObject
    {
        [SerializeField] private List<NodeBase> _nodes = new();
        [SerializeField] private List<Connection> _connections = new();
        [SerializeField] private int _version = 1;

        public IReadOnlyList<NodeBase> Nodes => _nodes;
        public IReadOnlyList<Connection> Connections => _connections;
        public int Version => _version;

        public NodeBase GetNodeById(string nodeId)
        {
            for (int i = 0; i < _nodes.Count; i++)
                if (_nodes[i] != null && _nodes[i].NodeId == nodeId)
                    return _nodes[i];
            return null;
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

        public void RemoveConnection(Connection connection) =>
            _connections.Remove(connection);

        public void RemoveConnectionsForNode(string nodeId) =>
            _connections.RemoveAll(c =>
                c.SourceNodeId == nodeId || c.TargetNodeId == nodeId);

#if UNITY_EDITOR
        public NodeBase AddNode(Type nodeType)
        {
            var node = CreateInstance(nodeType) as NodeBase;
            if (node == null) return null;

            node.name = nodeType.Name;
            node.hideFlags = HideFlags.HideInHierarchy;
            _nodes.Add(node);

            UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
            UnityEditor.EditorUtility.SetDirty(this);
            return node;
        }

        public T AddNode<T>() where T : NodeBase => AddNode(typeof(T)) as T;

        public void RemoveNode(NodeBase node)
        {
            RemoveConnectionsForNode(node.NodeId);
            _nodes.Remove(node);

            UnityEditor.AssetDatabase.RemoveObjectFromAsset(node);
            DestroyImmediate(node, true);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
