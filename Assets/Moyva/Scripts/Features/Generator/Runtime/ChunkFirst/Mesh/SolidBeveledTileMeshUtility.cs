using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Runtime-generated single-piece terrain tile: one solid top face plus a
    /// 45-degree bevel ring around the perimeter. Unlike the authored
    /// dual-grid fragments, the top is not split into quadrants, so no cross
    /// seam runs through the tile centre. Side walls are not authored here —
    /// the chunk pipeline extrudes the bevel bottom ring down to each lower
    /// neighbour's surface through the authored-contour closure, and equal or
    /// higher neighbours keep the side occluded.
    /// Unit footprint (1x1 m), pivot at centre, plateau top at Y = 0 — the same
    /// contract the cliff asset builder bakes into the shared meshes.
    /// </summary>
    internal static class SolidBeveledTileMeshUtility
    {
        private const float Half = 0.5f;
        private const float TopInset = 0.0375f;
        private const float BevelDrop = 0.0375f;

        private static Mesh _mesh;

        public static Mesh GetOrCreate()
        {
            if (_mesh != null)
                return _mesh;

            float t = Half - TopInset;
            float h = Half;
            float d = BevelDrop;

            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            // One solid top face, inset from the perimeter.
            AddQuad(
                positions, normals, uvs, triangles,
                new Vector3(-t, 0f, -t),
                new Vector3(-t, 0f, t),
                new Vector3(t, 0f, t),
                new Vector3(t, 0f, -t),
                Vector3.up);

            // Bevel ring: one mitered trapezoid per side. The outer bottom
            // edges at y = -d form the authored bottom contour the closure
            // skirt extrudes downward on open sides.
            AddQuad(
                positions, normals, uvs, triangles,
                new Vector3(-t, 0f, t),
                new Vector3(-h, -d, h),
                new Vector3(h, -d, h),
                new Vector3(t, 0f, t),
                new Vector3(0f, TopInset, BevelDrop));

            AddQuad(
                positions, normals, uvs, triangles,
                new Vector3(t, 0f, t),
                new Vector3(h, -d, h),
                new Vector3(h, -d, -h),
                new Vector3(t, 0f, -t),
                new Vector3(BevelDrop, TopInset, 0f));

            AddQuad(
                positions, normals, uvs, triangles,
                new Vector3(t, 0f, -t),
                new Vector3(h, -d, -h),
                new Vector3(-h, -d, -h),
                new Vector3(-t, 0f, -t),
                new Vector3(0f, TopInset, -BevelDrop));

            AddQuad(
                positions, normals, uvs, triangles,
                new Vector3(-t, 0f, -t),
                new Vector3(-h, -d, -h),
                new Vector3(-h, -d, h),
                new Vector3(-t, 0f, t),
                new Vector3(-BevelDrop, TopInset, 0f));

            var mesh = new Mesh
            {
                name = "SolidBeveledTile",
                vertices = positions.ToArray(),
                normals = normals.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray(),
            };
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            _mesh = mesh;
            return _mesh;
        }

        private static void AddQuad(
            List<Vector3> positions,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            Vector3 expectedNormal)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            bool flip = normal.sqrMagnitude > 1e-12f
                        && Vector3.Dot(normal, expectedNormal) < 0f;
            if (flip)
            {
                (b, d) = (d, b);
                normal = -normal;
            }
            normal = normal.sqrMagnitude > 1e-12f ? normal.normalized : expectedNormal.normalized;

            int start = positions.Count;
            positions.Add(a);
            positions.Add(b);
            positions.Add(c);
            positions.Add(d);
            for (int i = 0; i < 4; i++)
            {
                normals.Add(normal);
                Vector3 p = positions[start + i];
                uvs.Add(new Vector2(p.x + Half, p.z + Half));
            }
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }
    }
}
