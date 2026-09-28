using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Runtime-generated water surface for the simple-stable profile: one flat
    /// 1x1 quad (unit footprint, pivot at cell centre, top at Y = 0). Unlike
    /// the authored water tiles it has no bevels, walls or underwater volume —
    /// the quad is the whole water surface, which keeps shore seams and level
    /// drops free of floating cyan plates.
    /// </summary>
    internal static class SimpleWaterQuadMeshUtility
    {
        private const float Half = 0.5f;

        private static Mesh _mesh;

        public static Mesh GetOrCreate()
        {
            if (_mesh != null)
                return _mesh;

            var mesh = new Mesh
            {
                name = "SimpleWaterQuad",
                vertices = new[]
                {
                    new Vector3(-Half, 0f, -Half),
                    new Vector3(-Half, 0f, Half),
                    new Vector3(Half, 0f, Half),
                    new Vector3(Half, 0f, -Half),
                },
                normals = new[]
                {
                    Vector3.up, Vector3.up, Vector3.up, Vector3.up
                },
                uv = new[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(1f, 0f),
                },
                triangles = new[] { 0, 1, 2, 0, 2, 3 },
            };
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            _mesh = mesh;
            return _mesh;
        }

        private static Mesh _sideMesh;

        /// <summary>
        /// Unit vertical water face for level drops: spans X in [-0.5, 0.5],
        /// Y in [-1, 0], lies in the Z = 0 plane and faces +Z. Scale Y by the
        /// drop height and rotate so +Z points out of the higher water cell.
        /// </summary>
        public static Mesh GetOrCreateSide()
        {
            if (_sideMesh != null)
                return _sideMesh;

            var mesh = new Mesh
            {
                name = "SimpleWaterSide",
                vertices = new[]
                {
                    new Vector3(Half, -1f, 0f),
                    new Vector3(Half, 0f, 0f),
                    new Vector3(-Half, 0f, 0f),
                    new Vector3(-Half, -1f, 0f),
                },
                normals = new[]
                {
                    Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward
                },
                uv = new[]
                {
                    new Vector2(1f, 0f),
                    new Vector2(1f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, 0f),
                },
                // The camera can orbit around a drop. Both sides need to
                // hide the terrain seam beneath the water edge.
                triangles = new[] { 0, 1, 2, 0, 2, 3, 2, 1, 0, 3, 2, 0 },
            };
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            _sideMesh = mesh;
            return _sideMesh;
        }
    }
}
