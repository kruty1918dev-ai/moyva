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
            if (!TryEnterPreview(context, layerName))
                return;

            try
            {
                GeneratePreviewInternal(context, layerName, seed);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Moyva TWC Graph Binding] Помилка preview шару '{layerName}': {ex}", context.LogContext);
            }
            finally
            {
                MoyvaTwcGraphEditorProgress.Clear();
                context.SetGenerating(false);
            }
        }

        public void ClearGeneratedMap(IMoyvaTwcGraphBindingContext context)
        {
            var config = context.Manager?.configuration;
            if (config == null)
            {
                Debug.LogWarning("[Moyva TWC Graph Binding] Сховище шарів не задано — нема чого очищати.", context.LogContext);
                return;
            }

            GuardChunkFirstVisualBuild(context);
            context.Manager.ResetConfiguration();
            context.Manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
        }

        private static void GuardChunkFirstVisualBuild(IMoyvaTwcGraphBindingContext context)
        {
            if (!TileWorldCreatorChunkFirstGuard.IsActive)
                return;

            Debug.LogError("[Moyva TWC Graph Binding] ExecuteBuildLayers reached during chunk-first mode.", context?.LogContext);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            throw new InvalidOperationException("TWC visual build is forbidden during chunk-first generation.");
#endif
        }

        private void GeneratePreviewInternal(IMoyvaTwcGraphBindingContext context, string layerName, int seed)
        {
            seed = GlobalSeed.InitializeDeterministic(_resolver.NormalizeSeed(seed));

            if (context.CompileBeforeGenerate)
                _compiler.Compile(context, seed);

            var config = context.Manager?.configuration;
            if (config?.blueprintLayerFolders == null)
                return;

            if (!_layerStates.EnableOnly(config, layerName, out var previousStates))
            {
                Debug.LogWarning($"[Moyva TWC Graph Binding] Шар '{layerName}' не знайдено серед blueprint-шарів.", context.LogContext);
                _layerStates.Restore(previousStates);
                return;
            }

            try
            {
                TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(
                    context.Manager,
                    TileWorldCreatorChunkBatchingUtility.ResolveSceneChunkSize());
            }
            finally
            {
                _layerStates.Restore(previousStates);
            }
        }

        private static bool TryEnterPreview(IMoyvaTwcGraphBindingContext context, string layerName)
        {
            if (context.IsGenerating)
            {
                Debug.LogWarning("[Moyva TWC Graph Binding] Генерація вже виконується, preview шару пропущено.", context.LogContext);
                return false;
            }

            if (string.IsNullOrWhiteSpace(layerName))
            {
                Debug.LogWarning("[Moyva TWC Graph Binding] Назву шару не задано.", context.LogContext);
                return false;
            }

            context.SetGenerating(true);
            return true;
        }
    }
}
