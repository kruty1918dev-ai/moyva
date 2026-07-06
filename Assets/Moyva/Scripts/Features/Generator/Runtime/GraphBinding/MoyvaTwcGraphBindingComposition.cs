namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MoyvaTwcGraphBindingComposition
    {
        public static IMoyvaTwcGraphBindingService Create()
        {
            var resolver = new MoyvaTwcGraphBindingResolver();
            var validation = new MoyvaTwcGraphValidationService();
            var compiler = new MoyvaTwcGraphCompileService(resolver, validation);
            var generation = new MoyvaTwcGraphBindingGenerationService(resolver, compiler, validation);
            var layerStates = new MoyvaTwcGraphLayerStateService();
            var preview = new MoyvaTwcGraphBindingLayerPreviewService(resolver, compiler, layerStates);
            return new MoyvaTwcGraphBindingService(validation, compiler, generation, preview);
        }
    }
}
