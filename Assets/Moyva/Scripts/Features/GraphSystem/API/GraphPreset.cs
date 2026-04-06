using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    [Serializable]
    public sealed class GraphPreset
    {
        [SerializeField] public int version = 2;
        [SerializeField] public List<NodePresetEntry> nodes = new();
        [SerializeField] public List<ConnectionEntry> connections = new();
        [SerializeField] public List<ScriptableObjectEntry> scriptableObjects = new();
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

    /// <summary>
    /// Embedded ScriptableObject data inside a preset.
    /// Stores the full JSON of each referenced SO so it can be recreated on import.
    /// </summary>
    [Serializable]
    public sealed class ScriptableObjectEntry
    {
        /// <summary>Original asset GUID in the source project.</summary>
        [SerializeField] public string originalGuid;
        /// <summary>Assembly-qualified type name of the ScriptableObject.</summary>
        [SerializeField] public string typeAssemblyQualifiedName;
        /// <summary>Display / asset name.</summary>
        [SerializeField] public string assetName;
        /// <summary>Full JSON from EditorJsonUtility.ToJson.</summary>
        [SerializeField] public string jsonData;
    }
}
