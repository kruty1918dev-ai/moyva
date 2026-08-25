using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MoyvaTwcGraphBindingLayerPreviewService : IMoyvaTwcGraphBindingLayerPreviewService
    {
        private readonly IMoyvaTwcGraphBindingResolver _resolver;
        private readonly IMoyvaTwcGraphCompileService _compiler;
        private readonly IMoyvaTwcGraphLayerStateService _layerStates;

        public MoyvaTwcGraphBindingLayerPreviewService(
            IMoyvaTwcGraphBindingResolver resolver,
            IMoyvaTwcGraphCompileService compiler,
            IMoyvaTwcGraphLayerStateService layerStates)
        {
            _resolver = resolver;
            _compiler = compiler;
            _layerStates = layerStates;
        }

        public IReadOnlyList<string> GetGraphLayerNames(IMoyvaTwcGraphBindingContext context)
        {
            if (context.GraphAsset?.Layers == null)
                return Array.Empty<string>();

            var names = new List<string>();
            foreach (var layer in context.GraphAsset.Layers)
            {
                if (layer != null && !string.IsNullOrWhiteSpace(layer.Name))
                    names.Add(layer.Name);
            }

            return names;
        }

        public void GenerateLayerPreview(IMoyvaTwcGraphBindingContext context, string layerName)
        {
            GenerateLayerPreview(context, layerName, _resolver.ResolveSeed(context));
        }

        public void GenerateLayerPreview(IMoyvaTwcGraphBindingContext context, string layerName, int seed)
        {
            Debug.LogError(
                "[Moyva TWC Graph Binding] Per-layer scene preview is disabled because it uses the legacy TWC build path. Use 'Generate Moyva Map' for an authoritative chunk-first build.",
                context?.LogContext);
        }

        public void ClearGeneratedMap(IMoyvaTwcGraphBindingContext context)
        {
            var cleanup = new ChunkFirstTwcVisualCleanupService();
            cleanup.ClearVisualBuildOutput(context?.Manager);

            GameObject chunkRoot = GameObject.Find("MapVisualChunks");
            if (chunkRoot == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(chunkRoot);
            else
                UnityEngine.Object.DestroyImmediate(chunkRoot);
        }
    }
}
