using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal interface IMapVisualRendererFilter
    {
        bool CanRegister(Renderer renderer);
        bool CanPartition(Renderer renderer, IMapVisualChunkRootService roots);
    }
}
