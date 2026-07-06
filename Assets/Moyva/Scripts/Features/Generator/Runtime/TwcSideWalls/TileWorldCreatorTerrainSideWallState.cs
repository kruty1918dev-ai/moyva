using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorTerrainSideWallState
    {
        public readonly List<Vector3> Vertices = new List<Vector3>(4096);
        public readonly List<int> Triangles = new List<int>(6144);
        public readonly List<Vector2> Uvs = new List<Vector2>(4096);
        public readonly List<string> Samples = new List<string>(16);
        public readonly List<string> ArtifactSamples = new List<string>(24);

        public Mesh Mesh;
        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;
        public Material RuntimeMaterial;
        public TileWorldCreatorTerrainSideWallConfig LastConfig;

        public void ClearBuildBuffers()
        {
            Vertices.Clear();
            Triangles.Clear();
            Uvs.Clear();
            Samples.Clear();
            ArtifactSamples.Clear();
        }
    }
}
