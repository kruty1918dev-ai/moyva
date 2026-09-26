using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Assigns flat per-face normals so generated tile geometry keeps the
    /// authored faceted low-poly look instead of smooth-shaded surfaces.
    /// </summary>
    internal static class FacetNormalsMeshUtility
    {
        /// <summary>
        /// Writes the triangle's geometric normal onto each of its vertices.
        /// Generated skirts and walls emit four unshared vertices per quad, so
        /// every vertex receives exactly its own face normal. On meshes that
        /// share vertices across faces the last written face wins — acceptable
        /// for missing-normal fallback paths, which are exceptional.
        /// </summary>
        public static void Apply(Mesh mesh)
        {
            if (mesh == null || mesh.subMeshCount == 0)
                return;

            Vector3[] vertices = mesh.vertices;
            var normals = new Vector3[vertices.Length];
            bool anySubMesh = false;
            for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
            {
                int[] triangles = mesh.GetTriangles(subMesh);
                if (triangles.Length == 0)
                    continue;

                anySubMesh = true;
                AssignTriangleNormals(vertices, normals, triangles);
            }

            if (!anySubMesh)
            {
                int[] triangles = mesh.triangles;
                AssignTriangleNormals(vertices, normals, triangles);
            }

            mesh.normals = normals;
        }

        private static void AssignTriangleNormals(
            Vector3[] vertices,
            Vector3[] normals,
            IList<int> triangles)
        {
            for (int i = 0; i + 2 < triangles.Count; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];
                if (a >= vertices.Length || b >= vertices.Length || c >= vertices.Length)
                    continue;

                Vector3 faceNormal = Vector3.Cross(
                    vertices[b] - vertices[a],
                    vertices[c] - vertices[a]);
                if (faceNormal.sqrMagnitude <= 1e-12f)
                    faceNormal = Vector3.up;
                else
                    faceNormal.Normalize();

                normals[a] = faceNormal;
                normals[b] = faceNormal;
                normals[c] = faceNormal;
            }
        }
    }
}
