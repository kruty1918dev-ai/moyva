using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal interface IMapVisualRendererCollector
    {
        void CollectPreferredRoots(List<Renderer> renderers);
        void CollectScene(List<Renderer> renderers);
    }
}
