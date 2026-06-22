using System;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [Serializable]
    public sealed class Connection
    {
        [SerializeField] private string _connectionId;
        [SerializeField] private string _sourceNodeId;
        [SerializeField] private int _sourcePortIndex;
        [SerializeField] private string _targetNodeId;
        [SerializeField] private int _targetPortIndex;
        [SerializeField] private int _sourceElementIndex;

        public string ConnectionId
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionId))
                    _connectionId = Guid.NewGuid().ToString();
                return _connectionId;
            }
        }

        public string SourceNodeId => _sourceNodeId;
        public int SourcePortIndex => _sourcePortIndex;
        public string TargetNodeId => _targetNodeId;
        public int TargetPortIndex => _targetPortIndex;
        public int SourceElementIndex => _sourceElementIndex;

        public Connection(string sourceNodeId, int sourcePortIndex,
            string targetNodeId, int targetPortIndex)
        {
            _connectionId = Guid.NewGuid().ToString();
            _sourceNodeId = sourceNodeId;
            _sourcePortIndex = sourcePortIndex;
            _targetNodeId = targetNodeId;
            _targetPortIndex = targetPortIndex;
        }

        public void SetSourceElementIndex(int index)
        {
            _sourceElementIndex = Mathf.Max(0, index);
        }

        public void ResetConnectionId()
        {
            _connectionId = Guid.NewGuid().ToString();
        }
    }
}
