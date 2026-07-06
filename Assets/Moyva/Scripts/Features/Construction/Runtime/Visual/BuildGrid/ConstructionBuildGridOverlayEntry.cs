using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal readonly struct ConstructionBuildGridOverlayEntry
    {
        public ConstructionBuildGridOverlayEntry(Mesh mesh, Matrix4x4 matrix, int layer, Vector4 edgeMask, Renderer sourceRenderer)
        {
            Mesh = mesh;
            Matrix = matrix;
            Layer = layer;
            EdgeMask = edgeMask;
            SourceRenderer = sourceRenderer;
        }

        public Mesh Mesh { get; }
        public Matrix4x4 Matrix { get; }
        public int Layer { get; }
        public Vector4 EdgeMask { get; }
        public Renderer SourceRenderer { get; }
    }
}
