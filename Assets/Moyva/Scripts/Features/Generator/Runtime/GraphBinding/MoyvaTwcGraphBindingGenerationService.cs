using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MoyvaTwcGraphBindingGenerationService : IMoyvaTwcGraphBindingGenerationService
    {
        private readonly IMoyvaTwcGraphBindingResolver _resolver;
        private readonly IMoyvaTwcGraphCompileService _compiler;
        private readonly IMoyvaTwcGraphValidationService _validation;

        public MoyvaTwcGraphBindingGenerationService(
            IMoyvaTwcGraphBindingResolver resolver,
            IMoyvaTwcGraphCompileService compiler,
            IMoyvaTwcGraphValidationService validation)
        {
            _resolver = resolver;
            _compiler = compiler;
            _validation = validation;
        }

        public void GenerateFromGraph(IMoyvaTwcGraphBindingContext context)
        {
            GenerateFromGraph(context, _resolver.ResolveSeed(context));
        }

        public void GenerateFromGraph(IMoyvaTwcGraphBindingContext context, int seed)
        {
            if (!TryEnterGeneration(context, "Генерація вже виконується, повторний запуск пропущено."))
                return;

            int normalizedSeed = _resolver.NormalizeSeed(seed);
            try
            {
                GenerateInternal(context, normalizedSeed);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Moyva TWC Graph Binding] Помилка генерації мапи: {ex}", context.LogContext);
            }
            finally
            {
                MoyvaTwcGraphEditorProgress.Clear();
                context.SetGenerating(false);
            }
        }

        private void GenerateInternal(IMoyvaTwcGraphBindingContext context, int seed)
        {
            if (context.CompileBeforeGenerate)
                _compiler.Compile(context, seed, false);

            if (context.Manager == null || context.Manager.configuration == null)
            {
                EmitGenerateLog(context, null, null, seed);
                return;
            }

            if (context.GenerateBuildLayersAfterCompile)
                TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(
                    context.Manager,
                    TileWorldCreatorChunkBatchingUtility.ResolveSceneChunkSize());
            else
                TileWorldCreatorLayerOcclusionOptimizer.GenerateBlueprintMap(context.Manager);

            EmitLogicalMapDiagnostics(context, seed);
            var report = _validation.Validate(context.GraphAsset);
            EmitGenerateLog(context, report, _validation.GetInvalidLayerIds(report), seed);
        }

        private void EmitLogicalMapDiagnostics(IMoyvaTwcGraphBindingContext context, int seed)
        {
            var mapSize = _resolver.ResolveMapSize(context);
            var logicalMap = GraphLogicalTileMapBuilder.Build(
                context.GraphAsset,
                context.Manager,
                context.LastCompiledLayers,
                mapSize.x,
                mapSize.y);

            GraphLogicalTileMapDiagnostics.EmitAndCompare(
                "Scene build mask",
                context.GraphAsset,
                seed,
                logicalMap,
                context.LogContext);
        }

        private void EmitGenerateLog(IMoyvaTwcGraphBindingContext context, GraphValidationReport report, System.Collections.Generic.HashSet<string> skipped, int seed)
        {
            GraphGenerationLayerLog.Emit(
                "Graph Binding Generate",
                context.GraphAsset,
                context.Manager,
                context.LastCompiledLayers,
                report,
                skipped,
                seed,
                _resolver.ResolveMapSize(context),
                context.GenerateBuildLayersAfterCompile,
                context.LogContext);
        }

        private static bool TryEnterGeneration(IMoyvaTwcGraphBindingContext context, string warning)
        {
            if (context.IsGenerating)
            {
                Debug.LogWarning($"[Moyva TWC Graph Binding] {warning}", context.LogContext);
                return false;
            }

            context.SetGenerating(true);
            return true;
        }
    }
}
