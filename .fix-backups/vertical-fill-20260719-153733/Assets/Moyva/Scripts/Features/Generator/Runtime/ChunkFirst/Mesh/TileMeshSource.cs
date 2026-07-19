using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal readonly struct TileMeshSource
    {
        public TileMeshSource(Mesh mesh, Material[] materials, Matrix4x4 localMatrix)
        {
            Mesh = mesh;
            Materials = materials;
            LocalMatrix = localMatrix;
        }

        public Mesh Mesh { get; }
        public Material[] Materials { get; }
        public Matrix4x4 LocalMatrix { get; }
        public bool IsValid => Mesh != null && Mesh.vertexCount > 0;
    }
}
