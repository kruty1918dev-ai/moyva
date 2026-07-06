using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMoyvaTwcGraphCompileService
    {
        IReadOnlyList<CompiledLayerMap> Compile(IMoyvaTwcGraphBindingContext context);
        IReadOnlyList<CompiledLayerMap> Compile(IMoyvaTwcGraphBindingContext context, int seed);
        IReadOnlyList<CompiledLayerMap> Compile(IMoyvaTwcGraphBindingContext context, int seed, bool emitLayerLog);
    }
}
