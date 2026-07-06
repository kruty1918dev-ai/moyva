using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMoyvaTwcGraphBindingResolver
    {
        int ResolveSeed(IMoyvaTwcGraphBindingContext context);
        Vector2Int ResolveMapSize(IMoyvaTwcGraphBindingContext context);
        int NormalizeSeed(int seed);
    }
}
