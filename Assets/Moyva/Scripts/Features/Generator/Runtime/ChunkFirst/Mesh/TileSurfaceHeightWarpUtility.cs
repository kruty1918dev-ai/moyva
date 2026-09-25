using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Warps a fragment mesh onto the bilinear height field defined by the
    /// four corner surface heights of its footprint. Each vertex is displaced
    /// vertically so the fragment's surface follows the shared field; because
    /// neighbouring fragments share corner heights, their borders meet
    /// continuously and produce smooth slopes instead of terraced cliffs.
    ///
    /// The displacement is evaluated in the fragment's rotation-aligned,
    /// translation-free space (linear matrix applied to the local vertex), so
    /// the result depends only on the mesh, its orientation/scale, the
    /// footprint extent and the corner heights — never on world position.
    /// </summary>
    internal static class TileSurfaceHeightWarpUtility
    {
        private const float FlatEpsilon = 0.001f;
        private const float ScaleEpsilon = 0.0001f;

        public static bool TryCreate(
            in TileMeshSource source,
            Mesh input,
            out Mesh warped)
        {
            warped = null;
            if (!source.HasCornerHeights
                || !source.HasTileFootprint
                || input == null
                || input.vertexCount == 0)
            {
                return false;
            }

            TileMeshCornerHeights corners = source.CornerHeights;
            if (corners.IsFlat(FlatEpsilon)
                || !IsFinite(corners.NorthWest)
                || !IsFinite(corners.NorthEast)
                || !IsFinite(corners.SouthWest)
                || !IsFinite(corners.SouthEast)
                || !IsFinite(corners.Reference))
            {
                return false;
            }

            Matrix4x4 matrix = source.LocalMatrix;
            if (Mathf.Abs(matrix.m11) <= ScaleEpsilon)
                return false;

            float invDoubleHalf = 0.5f / Mathf.Max(ScaleEpsilon, source.TileHalfExtent);
            float invScaleY = 1f / matrix.m11;
            Quaternion rotation = matrix.rotation;
            Quaternion inverseRotation = Quaternion.Inverse(rotation);
            float doubleHalf = 2f * source.TileHalfExtent;

            Vector3[] vertices = input.vertices;
            Vector3[] normals = input.normals;
            bool hasNormals = normals != null && normals.Length == vertices.Length;
            var warpedNormals = hasNormals ? new Vector3[normals.Length] : null;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                Vector3 aligned = matrix.MultiplyVector(vertex);
                float u = Mathf.Clamp01(aligned.x * invDoubleHalf + 0.5f);
                float v = Mathf.Clamp01(aligned.z * invDoubleHalf + 0.5f);

                float delta = corners.Evaluate(u, v) - corners.Reference;
                vertex.y += delta * invScaleY;
                vertices[i] = vertex;

                if (hasNormals)
                {
                    float gradientX =
                        (Mathf.Lerp(corners.SouthEast, corners.NorthEast, v)
                         - Mathf.Lerp(corners.SouthWest, corners.NorthWest, v))
                        / doubleHalf;
                    float gradientZ =
                        (Mathf.Lerp(corners.NorthWest, corners.NorthEast, u)
                         - Mathf.Lerp(corners.SouthWest, corners.SouthEast, u))
                        / doubleHalf;

                    Vector3 worldNormal = rotation * normals[i];
                    worldNormal = new Vector3(
                        worldNormal.x - gradientX * worldNormal.y,
                        worldNormal.y,
                        worldNormal.z - gradientZ * worldNormal.y);
                    warpedNormals[i] =
                        (inverseRotation * worldNormal).normalized;
                }
            }

            warped = Object.Instantiate(input);
            warped.name = input.name + "_Warped";
            warped.vertices = vertices;
            if (hasNormals)
                warped.normals = warpedNormals;
            warped.RecalculateBounds();
            return true;
        }

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
