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
            return Compile(context, _resolver.ResolveSeed(context));
        }

        public IReadOnlyList<CompiledLayerMap> Compile(IMoyvaTwcGraphBindingContext context, int seed)
        {
            int normalizedSeed = _resolver.NormalizeSeed(seed);
            if (!TryPrepareCompile(context, out var skippedLayerIds))
                return context.LastCompiledLayers;

            Vector2Int mapSize = _resolver.ResolveMapSize(context);
            var compiled = GraphToConfigurationCompiler.Compile(
                context.GraphAsset,
                context.Manager,
                normalizedSeed,
                skippedLayerIds,
                mapSize);
            context.SetLastCompiledLayers(compiled);
            return compiled;
        }

        private bool TryPrepareCompile(
            IMoyvaTwcGraphBindingContext context,
            out HashSet<string> skippedLayerIds)
        {
            skippedLayerIds = null;
            if (!_validation.CanCompile(context, out _))
                return Fail(context);

            var report = _validation.Validate(context.GraphAsset);
            var globalErrors = _validation.GetGlobalErrors(report);
            if (globalErrors.Count > 0)
                return Fail(context);

            skippedLayerIds = _validation.GetInvalidLayerIds(report);
            return true;
        }

        private static bool Fail(IMoyvaTwcGraphBindingContext context)
        {
            context.SetLastCompiledLayers(Array.Empty<CompiledLayerMap>());
            return false;
        }
    }
}
