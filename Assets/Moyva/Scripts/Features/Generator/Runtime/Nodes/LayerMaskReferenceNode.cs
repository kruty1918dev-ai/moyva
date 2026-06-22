using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Layer Ref", "Layers", "Повертає bool-маску іншого шару за його Layer ID, щоб далі комбінувати її в поточному шарі.")]
    public sealed class LayerMaskReferenceNode : NodeBase, IPreviewableNode
    {
        [SerializeField, HideInInspector] private string _sourceLayerId;

        public override string Title => "Layer Ref";
        public override string Category => "Layers";

        public string SourceLayerId => _sourceLayerId;

        public void SetSourceLayerId(string layerId)
        {
            _sourceLayerId = layerId;
        }

        public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask"),
            PortDefinition.Output<Texture2D>("Texture")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (context == null)
                return NodeOutput.Error("NodeContext відсутній.");

            if (string.IsNullOrEmpty(_sourceLayerId))
            {
                var empty = CreateEmptyMask(context);
                return NodeOutput.Warning("Для Layer Ref не вибрано шар-джерело.", empty, BuildTexture(empty));
            }

            if (!context.TryGetService<LayerMaskRegistry>(out var registry) || registry == null)
            {
                var empty = CreateEmptyMask(context);
                return NodeOutput.Warning("LayerMaskRegistry недоступний у NodeContext.", empty, BuildTexture(empty));
            }

            if (!registry.TryGetLatestMask(_sourceLayerId, out var mask) || mask == null)
            {
                var empty = CreateEmptyMask(context);
                return NodeOutput.Warning("Для вибраного шару ще немає згенерованої маски.", empty, BuildTexture(empty));
            }

            return NodeOutput.Success(mask, BuildTexture(mask));
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            return null;
        }

        private static bool[,] CreateEmptyMask(NodeContext context)
        {
            int w = Mathf.Max(1, context.MapSize.x);
            int h = Mathf.Max(1, context.MapSize.y);
            return new bool[w, h];
        }

        private static Texture2D BuildTexture(bool[,] mask)
        {
            int w = mask.GetLength(0);
            int h = mask.GetLength(1);

            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    pixels[y * w + x] = mask[x, y] ? Color.white : Color.black;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
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

            graph.EnsureLayerGraphStates();

            var layers = graph.Layers?
                .Where(layer => layer != null && layer.Enabled)
                .Where(layer => skippedLayerIds == null || !skippedLayerIds.Contains(layer.Id))
                .OrderBy(layer => layer.SortingOrder)
                .ToList();

            if (layers == null || layers.Count == 0)
                return;

            var allowedLayerIds = new HashSet<string>(layers.Select(layer => layer.Id));
            var visiting = new HashSet<string>();
            var visited = new HashSet<string>();
            var runner = new GraphRunner();
            int safeSeed = seed == 0 ? 1 : seed;
            var safeMapSize = new Vector2Int(Mathf.Max(1, mapSize.x), Mathf.Max(1, mapSize.y));
            int previousGlobalSeed = GlobalSeed.Current;
            var previousRandomState = UnityEngine.Random.state;

            try
            {
                GlobalSeed.Set(safeSeed);
                UnityEngine.Random.InitState(safeSeed);

                for (int i = 0; i < layers.Count; i++)
                {
                    PrewarmLayer(
                        graph,
                        layers[i].Id,
                        safeSeed,
                        safeMapSize,
                        registry,
                        runner,
                        configureContext,
                        allowedLayerIds,
                        visiting,
                        visited);
                }
            }
            finally
            {
                GlobalSeed.Set(previousGlobalSeed);
                UnityEngine.Random.state = previousRandomState;
            }
        }

        private static void PrewarmLayer(
            GraphAsset graph,
            string layerId,
            int seed,
            Vector2Int mapSize,
            LayerMaskRegistry registry,
            GraphRunner runner,
            Action<NodeContext> configureContext,
            HashSet<string> allowedLayerIds,
            HashSet<string> visiting,
            HashSet<string> visited)
        {
            if (string.IsNullOrEmpty(layerId)
                || !allowedLayerIds.Contains(layerId)
                || visited.Contains(layerId))
                return;

            if (!visiting.Add(layerId))
            {
                Debug.LogWarning($"[MoyvaLayerRef] Circular layer reference detected while prewarming layer '{layerId}'.");
                return;
            }

            try
            {
                var scope = graph.CreateExecutionScope(layerId);
                foreach (var sourceLayerId in EnumerateReferenceSourceLayerIds(scope))
                {
                    PrewarmLayer(
                        graph,
                        sourceLayerId,
                        seed,
                        mapSize,
                        registry,
                        runner,
                        configureContext,
                        allowedLayerIds,
                        visiting,
                        visited);
                }

                var context = CreatePrewarmContext(graph, seed, mapSize, registry, configureContext);
                var result = runner.Execute(scope, context);
                if (result == null || !result.Success)
                {
                    Debug.LogWarning(
                        $"[MoyvaLayerRef] Failed to prewarm layer '{layerId}' mask: {result?.ErrorMessage ?? "unknown graph execution error"}");
                }

                visited.Add(layerId);
            }
            finally
            {
                visiting.Remove(layerId);
            }
        }

        private static IEnumerable<string> EnumerateReferenceSourceLayerIds(GraphExecutionScope scope)
        {
            if (scope?.Nodes == null)
                yield break;

            var seen = new HashSet<string>();
            for (int i = 0; i < scope.Nodes.Count; i++)
            {
                if (scope.Nodes[i] is not LayerMaskReferenceNode referenceNode)
                    continue;

                string sourceLayerId = referenceNode.SourceLayerId;
                if (!string.IsNullOrEmpty(sourceLayerId) && seen.Add(sourceLayerId))
                    yield return sourceLayerId;
            }
        }

        private static NodeContext CreatePrewarmContext(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            LayerMaskRegistry registry,
            Action<NodeContext> configureContext)
        {
            var context = new NodeContext(seed)
            {
                MapSize = mapSize
            };

            var sharedSettings = graph?.SharedSettings;
            if (sharedSettings != null)
            {
                context.ApplySharedSettings(sharedSettings);
                context.RegisterService(sharedSettings);
            }

            configureContext?.Invoke(context);
            context.RegisterService(registry);
            return context;
        }
    }
}
