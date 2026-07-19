using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerModifierService
    {
        void Append(BlueprintLayer blueprint, string layerId, IReadOnlyList<NodeBase> sortedNodes,
            Configuration config, Dictionary<string, string> blueprintGuidByGraphLayerId,
            GraphAsset graph, IReadOnlyDictionary<string, bool[,]> precomputedLayerMasks);
    }

    internal sealed class GraphCompilerModifierService : IGraphCompilerModifierService
    {
        private readonly IGraphCompilerMaskUtility _maskUtility;

        public GraphCompilerModifierService(IGraphCompilerMaskUtility maskUtility)
        {
            _maskUtility = maskUtility;
        }

        public void Append(BlueprintLayer blueprint, string layerId, IReadOnlyList<NodeBase> sortedNodes,
            Configuration config, Dictionary<string, string> blueprintGuidByGraphLayerId,
            GraphAsset graph, IReadOnlyDictionary<string, bool[,]> precomputedLayerMasks)
        {
            var layerNodes = CollectLayerNodes(sortedNodes, layerId);
            if (!UsesNativeTwcBlueprintStack(layerNodes)
                && TryAddPrecomputedMaskModifier(blueprint, layerId, graph, config, precomputedLayerMasks))
                return;

            AppendLayerReferenceModifiers(blueprint, layerNodes, blueprintGuidByGraphLayerId, graph, config);
            AppendNativeTwcModifiers(blueprint, layerNodes, config);
        }

        private bool TryAddPrecomputedMaskModifier(BlueprintLayer blueprint, string layerId,
            GraphAsset graph, Configuration config, IReadOnlyDictionary<string, bool[,]> precomputedLayerMasks)
        {
            if (blueprint == null || string.IsNullOrEmpty(layerId) || precomputedLayerMasks == null
                || !precomputedLayerMasks.TryGetValue(layerId, out var mask) || mask == null)
                return false;

            var modifier = ScriptableObject.CreateInstance<MoyvaPrecomputedMaskBlueprintModifier>();
            modifier.name = "Moyva Graph Output Mask";
            modifier.hideFlags = HideFlags.HideInHierarchy;
            modifier.isEnabled = true;
            modifier.asset = config;
            modifier.sourceGraphLayerId = layerId;
            modifier.sourceLayerName = graph?.GetLayerById(layerId)?.Name;
            modifier.SetPositions(_maskUtility.EnumeratePositions(mask));
            blueprint.tileMapModifiers.Add(modifier);
            return true;
        }

        private static void AppendLayerReferenceModifiers(BlueprintLayer blueprint, List<NodeBase> layerNodes,
            Dictionary<string, string> blueprintGuidByGraphLayerId, GraphAsset graph, Configuration config)
        {
            foreach (var node in layerNodes)
            {
                if (node is not LayerMaskReferenceNode referenceNode)
                    continue;
                var modifier = CreateLayerReferenceModifier(referenceNode, blueprintGuidByGraphLayerId, graph, config);
                if (modifier != null)
                    blueprint.tileMapModifiers.Add(modifier);
            }
        }

        private static void AppendNativeTwcModifiers(BlueprintLayer blueprint, List<NodeBase> layerNodes, Configuration config)
        {
            foreach (var node in layerNodes)
            {
                if (node is not TwcModifierNode twcNode || twcNode.Modifier == null)
                    continue;
                var clone = Object.Instantiate(twcNode.Modifier);
                clone.name = twcNode.Modifier.name;
                clone.hideFlags = HideFlags.HideInHierarchy;
                clone.isEnabled = true;
                clone.asset = config;
                blueprint.tileMapModifiers.Add(clone);
            }
        }

        private static MoyvaLayerReferenceBlueprintModifier CreateLayerReferenceModifier(
            LayerMaskReferenceNode node, Dictionary<string, string> blueprintGuidByGraphLayerId,
            GraphAsset graph, Configuration config)
        {
            if (node == null || string.IsNullOrEmpty(node.SourceLayerId)
                || blueprintGuidByGraphLayerId == null
                || !blueprintGuidByGraphLayerId.TryGetValue(node.SourceLayerId, out var sourceGuid)
                || string.IsNullOrEmpty(sourceGuid))
                return null;

            var modifier = ScriptableObject.CreateInstance<MoyvaLayerReferenceBlueprintModifier>();
            modifier.name = "Moyva Layer Ref";
            modifier.hideFlags = HideFlags.HideInHierarchy;
            modifier.isEnabled = true;
            modifier.asset = config;
            modifier.sourceGraphLayerId = node.SourceLayerId;
            modifier.sourceBlueprintLayerGuid = sourceGuid;
            modifier.sourceLayerName = graph?.GetLayerById(node.SourceLayerId)?.Name;
            return modifier;
        }

        private static List<NodeBase> CollectLayerNodes(IReadOnlyList<NodeBase> sortedNodes, string layerId)
        {
            var nodes = new List<NodeBase>();
            if (sortedNodes == null)
                return nodes;
            foreach (var node in sortedNodes)
            {
                if (BelongsToLayer(node, layerId))
                    nodes.Add(node);
            }

            return nodes;
        }

        private static bool UsesNativeTwcBlueprintStack(IReadOnlyList<NodeBase> layerNodes)
        {
            if (layerNodes == null)
                return false;
            foreach (var node in layerNodes)
            {
                // LayerMaskReferenceNode is already resolved by GraphEvaluationPipeline
                // and included in the authoritative OutputNode mask. Only actual native
                // TWC modifiers should switch the layer to the native blueprint stack.
                if (node is TwcModifierNode)
                    return true;
            }
            return false;
        }

        private static bool BelongsToLayer(NodeBase node, string layerId)
        {
            if (node == null)
                return false;
            return !string.IsNullOrEmpty(node.LayerId)
                ? node.LayerId == layerId
                : GraphAsset.IsGlobalNode(node);
        }
    }
}
