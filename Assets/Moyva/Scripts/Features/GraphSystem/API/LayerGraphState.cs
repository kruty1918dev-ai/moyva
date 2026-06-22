using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [Serializable]
    public sealed class LayerGraphState
    {
        [SerializeField] private string _graphId;
        [SerializeField] private string _layerId;
        [SerializeField] private int _version = 1;
        [SerializeField] private List<string> _nodeIds = new();
        [SerializeField] private List<string> _connectionIds = new();

        public LayerGraphState()
        {
        }

        public LayerGraphState(string layerId)
        {
            _layerId = layerId;
        }

        public string GraphId
        {
            get
            {
                if (string.IsNullOrEmpty(_graphId))
                    _graphId = Guid.NewGuid().ToString();
                return _graphId;
            }
        }

        public string LayerId
        {
            get => _layerId;
            set => _layerId = value;
        }

        public int Version => _version;
        public IReadOnlyList<string> NodeIds => _nodeIds;
        public IReadOnlyList<string> ConnectionIds => _connectionIds;

        public bool ContainsNode(string nodeId) => _nodeIds.Contains(nodeId);
        public bool ContainsConnection(string connectionId) => _connectionIds.Contains(connectionId);

        public void Clear()
        {
            _nodeIds.Clear();
            _connectionIds.Clear();
        }

        public void AddNode(string nodeId)
        {
            if (!string.IsNullOrEmpty(nodeId) && !_nodeIds.Contains(nodeId))
                _nodeIds.Add(nodeId);
        }

        public void AddConnection(string connectionId)
        {
            if (!string.IsNullOrEmpty(connectionId) && !_connectionIds.Contains(connectionId))
                _connectionIds.Add(connectionId);
        }

        public void RemoveNode(string nodeId) => _nodeIds.Remove(nodeId);
        public void RemoveConnection(string connectionId) => _connectionIds.Remove(connectionId);
    }
}
