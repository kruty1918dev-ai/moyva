using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMoyvaTwcGraphBindingLayerPreviewService
    {
        IReadOnlyList<string> GetGraphLayerNames(IMoyvaTwcGraphBindingContext context);
        void GenerateLayerPreview(IMoyvaTwcGraphBindingContext context, string layerName);
        void GenerateLayerPreview(IMoyvaTwcGraphBindingContext context, string layerName, int seed);
        void ClearGeneratedMap(IMoyvaTwcGraphBindingContext context);
    }
}
