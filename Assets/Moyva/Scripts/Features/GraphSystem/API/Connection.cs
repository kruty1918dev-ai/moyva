using System;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [Serializable]
    public sealed class Connection
    {
        [SerializeField] private string _sourceNodeId;
        [SerializeField] private int _sourcePortIndex;
        [SerializeField] private string _targetNodeId;
        [SerializeField] private int _targetPortIndex;

        public string SourceNodeId => _sourceNodeId;
        public int SourcePortIndex => _sourcePortIndex;
        public string TargetNodeId => _targetNodeId;
        public int TargetPortIndex => _targetPortIndex;

        public Connection(string sourceNodeId, int sourcePortIndex,
            string targetNodeId, int targetPortIndex)
        {
            _sourceNodeId = sourceNodeId;
            _sourcePortIndex = sourcePortIndex;
            _targetNodeId = targetNodeId;
            _targetPortIndex = targetPortIndex;
        }
    }
}
