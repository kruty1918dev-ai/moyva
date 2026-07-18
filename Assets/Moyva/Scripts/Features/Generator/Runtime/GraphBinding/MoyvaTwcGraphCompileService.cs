using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MoyvaTwcGraphCompileService : IMoyvaTwcGraphCompileService
    {
        private readonly IMoyvaTwcGraphBindingResolver _resolver;
        private readonly IMoyvaTwcGraphValidationService _validation;

        public MoyvaTwcGraphCompileService(
            IMoyvaTwcGraphBindingResolver resolver,
            IMoyvaTwcGraphValidationService validation)
        {
            _resolver = resolver;
            _validation = validation;
        }

        public IReadOnlyList<CompiledLayerMap> Compile(IMoyvaTwcGraphBindingContext context)
        {
            return Compile(context, _resolver.ResolveSeed(context), true);
        }

        public IReadOnlyList<CompiledLayerMap> Compile(IMoyvaTwcGraphBindingContext context, int seed)
        {
            return Compile(context, seed, true);
        }

        public IReadOnlyList<CompiledLayerMap> Compile(IMoyvaTwcGraphBindingContext context, int seed, bool emitLayerLog)
        {
            int normalizedSeed = _resolver.NormalizeSeed(seed);
            if (!TryPrepareCompile(context, normalizedSeed, emitLayerLog, out var report, out var skippedLayerIds))
                return context.LastCompiledLayers;

            Vector2Int mapSize = _resolver.ResolveMapSize(context);
            var compiled = GraphToConfigurationCompiler.Compile(
                context.GraphAsset,
                context.Manager,
                normalizedSeed,
                skippedLayerIds,
                mapSize);
            context.SetLastCompiledLayers(compiled);
            EmitLayerLog(context, report, skippedLayerIds, normalizedSeed, emitLayerLog);
            return compiled;
        }

        private bool TryPrepareCompile(
            IMoyvaTwcGraphBindingContext context,
            int normalizedSeed,
            bool emitLayerLog,
            out Kruty1918.Moyva.GraphSystem.API.GraphValidationReport report,
            out HashSet<string> skippedLayerIds)
        {
            report = null;
            skippedLayerIds = null;
            if (!_validation.CanCompile(context, out string reason))
                return Fail(context, normalizedSeed, emitLayerLog, $"Неможливо скомпілювати граф: {reason}", null);

            report = _validation.Validate(context.GraphAsset);
            var globalErrors = _validation.GetGlobalErrors(report);
            if (globalErrors.Count > 0)
                return Fail(context, normalizedSeed, emitLayerLog,
                    $"Неможливо скомпілювати граф: {globalErrors.Count} global validation error(s).\n{MoyvaTwcGraphValidationText.FormatIssues(globalErrors)}", report);

            skippedLayerIds = _validation.GetInvalidLayerIds(report);
            if (skippedLayerIds.Count > 0)
                Debug.LogWarning($"[Moyva TWC Graph Binding] {skippedLayerIds.Count} layer(s) skipped because of validation errors.\n{MoyvaTwcGraphValidationText.FormatReport(report)}", context.LogContext);
            return true;
        }

        private bool Fail(IMoyvaTwcGraphBindingContext context, int seed, bool emitLayerLog, string message, object report)
        {
            Debug.LogWarning($"[Moyva TWC Graph Binding] {message}", context.LogContext);
            context.SetLastCompiledLayers(Array.Empty<CompiledLayerMap>());
            EmitLayerLog(context, report, null, seed, emitLayerLog);
            return false;
        }

        private void EmitLayerLog(IMoyvaTwcGraphBindingContext context, object report, HashSet<string> skipped, int seed, bool emit)
        {
            if (!emit)
                return;

            GraphGenerationLayerLog.Emit(
                "Graph Binding Compile",
                context.GraphAsset,
                context.Manager,
                context.LastCompiledLayers,
                report as Kruty1918.Moyva.GraphSystem.API.GraphValidationReport,
                skipped,
                seed,
                _resolver.ResolveMapSize(context),
                false,
                context.LogContext);
        }
    }
}
