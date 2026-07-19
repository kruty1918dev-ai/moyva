using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Layer Mask Reference",
        "Layers",
        "Повертає фінальну маску іншого шару за його стабільним Layer ID.",
        StableId = "moyva.layers.mask-reference",
        Order = 10,
        PreviewOutput = "out.mask")]
    public sealed class LayerMaskReferenceNode : NodeBase
    {
        [SerializeField, HideInInspector] private string _sourceLayerId;

        public override string Title => "Layer Mask Reference";
        public override string Category => "Layers";

        public string SourceLayerId => _sourceLayerId;

        public void SetSourceLayerId(string layerId)
        {
            _sourceLayerId = layerId;
        }

        public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask", "out.mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (context == null)
                return NodeOutput.Error("NodeContext відсутній.");

            if (string.IsNullOrEmpty(_sourceLayerId))
            {
                var empty = CreateEmptyMask(context);
                return NodeOutput.Warning("Для Layer Mask Reference не вибрано шар-джерело.", empty);
            }

            if (!context.TryGetService<LayerMaskRegistry>(out var registry) || registry == null)
            {
                var empty = CreateEmptyMask(context);
                return NodeOutput.Warning("LayerMaskRegistry недоступний у NodeContext.", empty);
            }

            if (!registry.TryGetLatestMask(_sourceLayerId, out var mask) || mask == null)
            {
                var empty = CreateEmptyMask(context);
                return NodeOutput.Warning("Для вибраного шару ще немає згенерованої фінальної маски.", empty);
            }

            return NodeOutput.Success(mask);
        }

        private static bool[,] CreateEmptyMask(NodeContext context)
        {
            int w = Mathf.Max(1, context.MapSize.x);
            int h = Mathf.Max(1, context.MapSize.y);
            return new bool[w, h];
        }

    }

    internal static class LayerMaskPrewarmUtility
    {
        public static void PrewarmAllLayerMasks(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            LayerMaskRegistry registry,
            Action<NodeContext> configureContext = null,
            ISet<string> skippedLayerIds = null)
        {
            if (graph == null || registry == null)
                return;

            var snapshot = GraphEvaluationPipeline.Evaluate(
                graph,
                seed,
                mapSize,
                configureContext: configureContext,
                skippedLayerIds: skippedLayerIds);
            foreach (var pair in snapshot.LayerOutputs)
            {
                if (pair.Value is ILayerMaskArtifact artifact
                    && artifact.LayerMask != null)
                    registry.SetLatestMask(pair.Key, artifact.LayerMask);
            }
        }

    }
}
