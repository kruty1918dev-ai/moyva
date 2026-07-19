using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal readonly struct TileMeshSource
    {
        public TileMeshSource(
            Mesh mesh,
            Material[] materials,
            Matrix4x4 localMatrix,
            float visibleBottomY = float.NaN)
        {
            Mesh = mesh;
            Materials = materials;
            LocalMatrix = localMatrix;
            VisibleBottomY = visibleBottomY;
        }

        public Mesh Mesh { get; }
        public Material[] Materials { get; }
        public Matrix4x4 LocalMatrix { get; }
        public float VisibleBottomY { get; }
        public bool HasVisibleBottomY => !float.IsNaN(VisibleBottomY);
        public bool IsValid => Mesh != null && Mesh.vertexCount > 0;
    }
}
