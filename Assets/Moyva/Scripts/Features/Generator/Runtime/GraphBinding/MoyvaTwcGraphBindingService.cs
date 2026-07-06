using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MoyvaTwcGraphBindingService : IMoyvaTwcGraphBindingService
    {
        private readonly IMoyvaTwcGraphValidationService _validation;
        private readonly IMoyvaTwcGraphCompileService _compiler;
        private readonly IMoyvaTwcGraphBindingGenerationService _generation;
        private readonly IMoyvaTwcGraphBindingLayerPreviewService _layerPreview;

        public MoyvaTwcGraphBindingService(
            IMoyvaTwcGraphValidationService validation,
            IMoyvaTwcGraphCompileService compiler,
            IMoyvaTwcGraphBindingGenerationService generation,
            IMoyvaTwcGraphBindingLayerPreviewService layerPreview)
        {
            _validation = validation;
            _compiler = compiler;
            _generation = generation;
            _layerPreview = layerPreview;
        }

        public bool CanCompile(IMoyvaTwcGraphBindingContext context, out string reason)
        {
            return _validation.CanCompile(context, out reason);
        }

        public IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration(IMoyvaTwcGraphBindingContext context)
        {
            return _compiler.Compile(context);
        }

        public IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration(IMoyvaTwcGraphBindingContext context, int seed)
        {
            return _compiler.Compile(context, seed);
        }

        public void GenerateFromGraph(IMoyvaTwcGraphBindingContext context)
        {
            _generation.GenerateFromGraph(context);
        }

        public void GenerateFromGraph(IMoyvaTwcGraphBindingContext context, int seed)
        {
            _generation.GenerateFromGraph(context, seed);
        }

        public IReadOnlyList<string> GetGraphLayerNames(IMoyvaTwcGraphBindingContext context)
        {
            return _layerPreview.GetGraphLayerNames(context);
        }

        public void GenerateLayerPreview(IMoyvaTwcGraphBindingContext context, string layerName)
        {
            _layerPreview.GenerateLayerPreview(context, layerName);
        }

        public void GenerateLayerPreview(IMoyvaTwcGraphBindingContext context, string layerName, int seed)
        {
            _layerPreview.GenerateLayerPreview(context, layerName, seed);
        }

        public void ClearGeneratedMap(IMoyvaTwcGraphBindingContext context)
        {
            _layerPreview.ClearGeneratedMap(context);
        }
    }
}
