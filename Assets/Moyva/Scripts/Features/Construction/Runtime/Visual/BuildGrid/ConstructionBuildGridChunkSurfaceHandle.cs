
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal readonly struct ConstructionBuildGridChunkSurfaceHandle
    {
        public ConstructionBuildGridChunkSurfaceHandle(
            MapChunkCoord coord,
            GameObject gameObject,
            Mesh mesh,
            MeshRenderer renderer)
        {
            Coord = coord;
            GameObject = gameObject;
            Mesh = mesh;
            Renderer = renderer;
        }

        public MapChunkCoord Coord { get; }
        public GameObject GameObject { get; }
        public Mesh Mesh { get; }
        public MeshRenderer Renderer { get; }
    }
}
