
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridChunkSurfaceBuilder
    {
        bool TryBuild(MapChunkDescriptor descriptor, out Mesh mesh);
    }
}
