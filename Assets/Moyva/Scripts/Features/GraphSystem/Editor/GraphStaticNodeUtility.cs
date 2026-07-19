using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    internal static class GraphStaticNodeUtility
    {
        internal static bool IsStaticGraphNode(Type nodeType) =>
            nodeType != null && Attribute.IsDefined(nodeType, typeof(StaticGraphNodeAttribute));

        internal static bool IsStaticGraphNode(NodeBase node) =>
            node != null && IsStaticGraphNode(node.GetType());

        internal static bool EnsureStaticNodes(GraphAsset graphAsset)
        {
            if (graphAsset == null)
                return false;

            bool changed = false;
            var nodeTypes = TypeCache.GetTypesDerivedFrom<NodeBase>()
                .Where(type => !type.IsAbstract && !type.IsGenericType)
                .Where(IsStaticGraphNode)
                .OrderBy(type => type.Name)
                .ToArray();

            for (int i = 0; i < nodeTypes.Length; i++)
            {
                var nodeType = nodeTypes[i];
                bool exists = graphAsset.Nodes.Any(node => node != null && node.GetType() == nodeType);
                if (exists)
                    continue;

                if (!GraphNodeFactory.TryCreateManagedStaticNode(
                        graphAsset,
                        nodeType,
                        new Vector2(50f, -360f - i * 120f),
                        out var node,
                        out string error))
                {
                    Debug.LogWarning($"[GraphStaticNodeUtility] {error}");
                    continue;
                }

                changed = true;
            }

            changed |= MigrateLegacySeedConnections(graphAsset);
            changed |= graphAsset.EnsureLayerGraphStates();

            if (changed)
                EditorUtility.SetDirty(graphAsset);

            return changed;
        }

        internal static bool MigrateLegacySeedConnections(GraphAsset graphAsset)
        {
            if (graphAsset?.Nodes == null || graphAsset.Connections == null)
                return false;

            var seedNodeIds = new HashSet<string>(graphAsset.Nodes
                .Where(node => node is ISeedProvider)
                .Select(node => node.NodeId));

            if (seedNodeIds.Count == 0)
                return false;

            bool changed = false;
            var connections = graphAsset.Connections.ToList();
            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                bool touchesSeed = seedNodeIds.Contains(connection.SourceNodeId)
                    || seedNodeIds.Contains(connection.TargetNodeId);

                if (touchesSeed)
                {
                    graphAsset.RemoveConnection(connection);
                    changed = true;
                    continue;
                }
            }

            return changed;
        }
    }
}
