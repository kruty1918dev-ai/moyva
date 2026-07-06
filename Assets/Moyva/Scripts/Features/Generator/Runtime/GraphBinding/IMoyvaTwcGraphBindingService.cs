using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMoyvaTwcGraphBindingService
    {
        bool CanCompile(IMoyvaTwcGraphBindingContext context, out string reason);
        IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration(IMoyvaTwcGraphBindingContext context);
        IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration(IMoyvaTwcGraphBindingContext context, int seed);
        void GenerateFromGraph(IMoyvaTwcGraphBindingContext context);
        void GenerateFromGraph(IMoyvaTwcGraphBindingContext context, int seed);
        IReadOnlyList<string> GetGraphLayerNames(IMoyvaTwcGraphBindingContext context);
        void GenerateLayerPreview(IMoyvaTwcGraphBindingContext context, string layerName);
        void GenerateLayerPreview(IMoyvaTwcGraphBindingContext context, string layerName, int seed);
        void ClearGeneratedMap(IMoyvaTwcGraphBindingContext context);
    }
}
