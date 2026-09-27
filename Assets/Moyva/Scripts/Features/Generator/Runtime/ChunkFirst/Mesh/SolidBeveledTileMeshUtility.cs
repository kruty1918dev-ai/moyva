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
                Vector3.up,
                Vector2.zero);

            // Bevel ring: one mitered trapezoid per side. The outer bottom
            // edges at y = -d form the authored bottom contour the closure
            // skirt extrudes downward on open sides. uvOutward unfolds each
            // face onto the plateau's UV plane so texel density stays uniform
            // across the edge instead of stretching by sqrt(2) on the slope.
            AddQuad(
                positions, normals, uvs, triangles,
                new Vector3(-t, 0f, t),
                new Vector3(-h, -d, h),
                new Vector3(h, -d, h),
                new Vector3(t, 0f, t),
                new Vector3(0f, TopInset, BevelDrop),
                new Vector2(0f, 1f));

            AddQuad(
                positions, normals, uvs, triangles,
                new Vector3(t, 0f, t),
                new Vector3(h, -d, h),
                new Vector3(h, -d, -h),
                new Vector3(t, 0f, -t),
                new Vector3(BevelDrop, TopInset, 0f),
                new Vector2(1f, 0f));

            AddQuad(
                positions, normals, uvs, triangles,
                new Vector3(t, 0f, -t),
                new Vector3(h, -d, -h),
                new Vector3(-h, -d, -h),
                new Vector3(-t, 0f, -t),
                new Vector3(0f, TopInset, -BevelDrop),
                new Vector2(0f, -1f));

            AddQuad(
                positions, normals, uvs, triangles,
                new Vector3(-t, 0f, -t),
                new Vector3(-h, -d, -h),
                new Vector3(-h, -d, h),
                new Vector3(-t, 0f, t),
                new Vector3(-BevelDrop, TopInset, 0f),
                new Vector2(-1f, 0f));

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
            Vector3 expectedNormal,
            Vector2 uvOutward)
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
                uvs.Add(TileSurfaceUv(positions[start + i], uvOutward));
            }
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }

        /*
         * Maps a vertex onto the plateau's UV plane. Plateau vertices keep the
         * world-planar mapping; bevel vertices slide outward by the true slope
         * length, so the shared top edge keeps identical UVs on both faces and
         * a checker keeps the same density across the edge.
         */
        private static Vector2 TileSurfaceUv(Vector3 position, Vector2 uvOutward)
        {
            var uv = new Vector2(position.x + Half, position.z + Half);
            if (uvOutward == Vector2.zero)
                return uv;

            float slopeOverInset =
                Mathf.Sqrt(TopInset * TopInset + BevelDrop * BevelDrop)
                / TopInset;
            float overhang =
                position.x * uvOutward.x
                + position.z * uvOutward.y
                - (Half - TopInset);
            return uv
                + uvOutward
                * (Mathf.Max(0f, overhang) * (slopeOverInset - 1f));
        }
    }
}
