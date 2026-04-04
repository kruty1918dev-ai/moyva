using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [Serializable]
    public sealed class GraphPreset
    {
        [SerializeField] public int version = 1;
        [SerializeField] public List<NodePresetEntry> nodes = new();
        [SerializeField] public List<ConnectionEntry> connections = new();
    }

    [Serializable]
    public sealed class NodePresetEntry
    {
        [SerializeField] public string nodeTypeAssemblyQualifiedName;
        [SerializeField] public string originalNodeId;
        [SerializeField] public Vector2 position;
        [SerializeField] public string jsonData;
    }

    [Serializable]
    public sealed class ConnectionEntry
    {
        [SerializeField] public string sourceNodeId;
        [SerializeField] public int sourcePortIndex;
        [SerializeField] public string targetNodeId;
        [SerializeField] public int targetPortIndex;
    }
}
