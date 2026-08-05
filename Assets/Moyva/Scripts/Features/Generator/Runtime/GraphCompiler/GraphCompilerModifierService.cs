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
        void Append(
            BlueprintLayer blueprint,
            string layerId,
            IReadOnlyList<NodeBase> sortedNodes,
            Configuration config,
            Dictionary<string, string> blueprintGuidByGraphLayerId,
            GraphAsset graph,
            IReadOnlyDictionary<string, bool[,]> precomputedLayerMasks);
    }

    internal sealed class GraphCompilerModifierService
        : IGraphCompilerModifierService
    {
        private readonly IGraphCompilerMaskUtility _maskUtility;

        public GraphCompilerModifierService(
            IGraphCompilerMaskUtility maskUtility)
        {
            _maskUtility = maskUtility;
        }

        public void Append(
            BlueprintLayer blueprint,
            string layerId,
            IReadOnlyList<NodeBase> sortedNodes,
            Configuration config,
            Dictionary<string, string> blueprintGuidByGraphLayerId,
            GraphAsset graph,
            IReadOnlyDictionary<string, bool[,]> precomputedLayerMasks)
        {
            if (blueprint == null)
                return;

            List<NodeBase> layerNodes =
                CollectLayerNodes(
                    sortedNodes,
                    layerId);

            /*
             * GraphEvaluationPipeline вже виконав:
             *
             * Layer references
             * → TWC generators
             * → TWC modifiers
             * → bool operations
             * → OutputNode
             *
             * Отже, precomputed mask є авторитетним фінальним
             * результатом шару.
             *
             * Не можна повторно виконувати native TWC stack,
             * оскільки це створює другий execution context,
             * інший random state і потенційно іншу source mask.
             */
            if (TryAddPrecomputedMaskModifier(
                    blueprint,
                    layerId,
                    graph,
                    config,
                    precomputedLayerMasks))
            {
                return;
            }

            /*
             * Fallback для старих або неповних графів, у яких
             * evaluation не створив фінальну маску.
             */
            AppendLayerReferenceModifiers(
                blueprint,
                layerNodes,
                blueprintGuidByGraphLayerId,
                graph,
                config);

            AppendNativeTwcModifiers(
                blueprint,
                layerNodes,
                config);
        }

        private bool TryAddPrecomputedMaskModifier(
            BlueprintLayer blueprint,
            string layerId,
            GraphAsset graph,
            Configuration config,
            IReadOnlyDictionary<string, bool[,]> precomputedLayerMasks)
        {
            if (blueprint == null
                || string.IsNullOrEmpty(layerId)
                || precomputedLayerMasks == null
                || !precomputedLayerMasks.TryGetValue(
                    layerId,
                    out bool[,] mask)
                || mask == null)
            {
                return false;
            }

            var modifier =
                ScriptableObject.CreateInstance<
                    MoyvaPrecomputedMaskBlueprintModifier>();

            modifier.name =
                "Moyva Authoritative Graph Output Mask";

            modifier.hideFlags =
                HideFlags.HideInHierarchy;

            modifier.isEnabled = true;
            modifier.asset = config;

            modifier.sourceGraphLayerId =
                layerId;

            modifier.sourceLayerName =
                graph?.GetLayerById(layerId)?.Name;

            modifier.SetPositions(
                _maskUtility.EnumeratePositions(mask));

            blueprint.tileMapModifiers.Add(
                modifier);

            return true;
        }

        private static void AppendLayerReferenceModifiers(
            BlueprintLayer blueprint,
            List<NodeBase> layerNodes,
            Dictionary<string, string> blueprintGuidByGraphLayerId,
            GraphAsset graph,
            Configuration config)
        {
            if (blueprint == null || layerNodes == null)
                return;

            foreach (NodeBase node in layerNodes)
            {
                if (node is not LayerMaskReferenceNode referenceNode)
                    continue;

                MoyvaLayerReferenceBlueprintModifier modifier =
                    CreateLayerReferenceModifier(
                        referenceNode,
                        blueprintGuidByGraphLayerId,
                        graph,
                        config);

                if (modifier != null)
                {
                    blueprint.tileMapModifiers.Add(
                        modifier);
                }
            }
        }

        private static void AppendNativeTwcModifiers(
            BlueprintLayer blueprint,
            List<NodeBase> layerNodes,
            Configuration config)
        {
            if (blueprint == null || layerNodes == null)
                return;

            foreach (NodeBase node in layerNodes)
            {
                if (node is not TwcModifierNode twcNode
                    || twcNode.Modifier == null)
                {
                    continue;
                }

                BlueprintModifier clone =
                    Object.Instantiate(
                        twcNode.Modifier);

                clone.name =
                    twcNode.Modifier.name;

                clone.hideFlags =
                    HideFlags.HideInHierarchy;

                clone.isEnabled = true;
                clone.asset = config;

                blueprint.tileMapModifiers.Add(
                    clone);
            }
        }

        private static MoyvaLayerReferenceBlueprintModifier
            CreateLayerReferenceModifier(
                LayerMaskReferenceNode node,
                Dictionary<string, string> blueprintGuidByGraphLayerId,
                GraphAsset graph,
                Configuration config)
        {
            if (node == null
                || string.IsNullOrEmpty(node.SourceLayerId)
                || blueprintGuidByGraphLayerId == null
                || !blueprintGuidByGraphLayerId.TryGetValue(
                    node.SourceLayerId,
                    out string sourceGuid)
                || string.IsNullOrEmpty(sourceGuid))
            {
                return null;
            }

            var modifier =
                ScriptableObject.CreateInstance<
                    MoyvaLayerReferenceBlueprintModifier>();

            modifier.name =
                "Moyva Layer Ref Fallback";

            modifier.hideFlags =
                HideFlags.HideInHierarchy;

            modifier.isEnabled = true;
            modifier.asset = config;

            modifier.sourceGraphLayerId =
                node.SourceLayerId;

            modifier.sourceBlueprintLayerGuid =
                sourceGuid;

            modifier.sourceLayerName =
                graph?.GetLayerById(
                    node.SourceLayerId)?.Name;

            return modifier;
        }

        private static List<NodeBase> CollectLayerNodes(
            IReadOnlyList<NodeBase> sortedNodes,
            string layerId)
        {
            var nodes =
                new List<NodeBase>();

            if (sortedNodes == null)
                return nodes;

            foreach (NodeBase node in sortedNodes)
            {
                if (BelongsToLayer(
                        node,
                        layerId))
                {
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private static bool BelongsToLayer(
            NodeBase node,
            string layerId)
        {
            if (node == null)
                return false;

            return !string.IsNullOrEmpty(node.LayerId)
                ? node.LayerId == layerId
                : GraphAsset.IsGlobalNode(node);
        }
    }
}