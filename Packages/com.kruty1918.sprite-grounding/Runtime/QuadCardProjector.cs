using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>
    /// One textured quad detected inside a card mesh: four vertices whose
    /// UVs cover the full 0..1 texture corners. <see cref="Bilinear"/> maps
    /// any texture-space point into mesh-local space.
    /// </summary>
    public readonly struct CardQuad
    {
        /// <summary>Vertex indices per UV corner: (0,0) (1,0) (0,1) (1,1).</summary>
        public readonly int I00, I10, I01, I11;
        /// <summary>Mesh-local positions of the same corners.</summary>
        public readonly Vector3 P00, P10, P01, P11;

        public CardQuad(
            int i00, int i10, int i01, int i11,
            Vector3 p00, Vector3 p10, Vector3 p01, Vector3 p11)
        {
            I00 = i00; I10 = i10; I01 = i01; I11 = i11;
            P00 = p00; P10 = p10; P01 = p01; P11 = p11;
        }

        public Vector3 Bilinear(Vector2 uv)
            => Vector3.Lerp(
                Vector3.Lerp(P00, P10, uv.x),
                Vector3.Lerp(P01, P11, uv.x),
                uv.y);
    }

    /// <summary>
    /// Recovers the UV→local mapping of quad-card meshes (cross cards,
    /// clumps, single quads). A quad is detected when two triangles share
    /// an edge and their four vertices carry the four full-texture UV
    /// corners. Non-card geometry (partial UVs, shared verts) is ignored,
    /// so arbitrary meshes degrade to "not a card".
    /// </summary>
    public static class QuadCardProjector
    {
        private const float CornerEpsilon = 0.02f;

        /// <summary>
        /// Extracts card quads into <paramref name="results"/> (appended,
        /// not cleared). Returns the number of quads found; iteration order
        /// follows triangle order so output is deterministic.
        /// </summary>
        public static int ExtractQuads(Mesh mesh, List<CardQuad> results)
        {
            if (mesh == null || !mesh.isReadable || results == null)
                return 0;

            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = mesh.uv;
            int[] triangles = mesh.triangles;
            int triCount = triangles.Length / 3;
            if (triCount == 0 || uvs.Length != vertices.Length)
                return 0;

            int[] cornerOf = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                cornerOf[i] = CornerKey(uvs[i]);

            int start = results.Count;

            // Map every shared edge to its incident triangles; quads form
            // across the diagonal edge shared by exactly two triangles.
            var edgeToTris = new Dictionary<long, List<int>>(triangles.Length * 3);
            for (int t = 0; t < triCount; t++)
            {
                int a = triangles[t * 3];
                int b = triangles[t * 3 + 1];
                int c = triangles[t * 3 + 2];
                AddEdge(edgeToTris, a, b, t);
                AddEdge(edgeToTris, b, c, t);
                AddEdge(edgeToTris, c, a, t);
            }

            var usedTris = new bool[triCount];
            foreach (var pair in edgeToTris)
            {
                List<int> tris = pair.Value;
                if (tris.Count < 2)
                    continue;

                for (int i = 0; i < tris.Count; i++)
                {
                    if (usedTris[tris[i]])
                        continue;
                    for (int j = i + 1; j < tris.Count; j++)
                    {
                        if (usedTris[tris[j]])
                            continue;
                        if (TryMerge(triangles, tris[i], tris[j], cornerOf, vertices,
                                out CardQuad quad))
                        {
                            usedTris[tris[i]] = true;
                            usedTris[tris[j]] = true;
                            results.Add(quad);
                            break;
                        }
                    }
                }
            }

            return results.Count - start;
        }

        /// <summary>Mesh-local bounds of the quads clipped to <paramref name="uvRect"/>.</summary>
        public static Bounds ProjectLocalBounds(IReadOnlyList<CardQuad> quads, Rect uvRect)
        {
            if (quads == null || quads.Count == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            var bounds = new Bounds(quads[0].Bilinear(uvRect.min), Vector3.zero);
            for (int i = 0; i < quads.Count; i++)
            {
                bounds.Encapsulate(quads[i].Bilinear(new Vector2(uvRect.xMin, uvRect.yMin)));
                bounds.Encapsulate(quads[i].Bilinear(new Vector2(uvRect.xMax, uvRect.yMin)));
                bounds.Encapsulate(quads[i].Bilinear(new Vector2(uvRect.xMin, uvRect.yMax)));
                bounds.Encapsulate(quads[i].Bilinear(new Vector2(uvRect.xMax, uvRect.yMax)));
            }
            return bounds;
        }

        /// <summary>
        /// Average local position of a UV point across all quads — for
        /// yawed cross cards the average lands on the card set's centre
        /// axis, which is the physical ground-contact line.
        /// </summary>
        public static Vector3 ProjectPoint(IReadOnlyList<CardQuad> quads, Vector2 uv)
        {
            if (quads == null || quads.Count == 0)
                return Vector3.zero;

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < quads.Count; i++)
                sum += quads[i].Bilinear(uv);
            return sum / quads.Count;
        }

        private static void AddEdge(Dictionary<long, List<int>> map, int a, int b, int tri)
        {
            long key = a < b
                ? ((long)a << 32) | (uint)b
                : ((long)b << 32) | (uint)a;
            if (!map.TryGetValue(key, out List<int> list))
                map[key] = list = new List<int>(2);
            list.Add(tri);
        }

        private static int CornerKey(Vector2 uv)
        {
            int u = uv.x < CornerEpsilon ? 0
                : uv.x > 1f - CornerEpsilon ? 1
                : -1;
            int v = uv.y < CornerEpsilon ? 0
                : uv.y > 1f - CornerEpsilon ? 2
                : -1;
            return u < 0 || v < 0 ? -1 : u | v;
        }

        private static bool TryMerge(
            int[] triangles, int triA, int triB, int[] cornerOf,
            Vector3[] vertices, out CardQuad quad)
        {
            quad = default;
            Span<int> idx = stackalloc int[6];
            idx[0] = triangles[triA * 3];
            idx[1] = triangles[triA * 3 + 1];
            idx[2] = triangles[triA * 3 + 2];
            idx[3] = triangles[triB * 3];
            idx[4] = triangles[triB * 3 + 1];
            idx[5] = triangles[triB * 3 + 2];

            var corners = new int[4];
            Array.Fill(corners, -1);
            int distinct = 0;
            var seen = new HashSet<int>();
            for (int i = 0; i < 6; i++)
            {
                int v = idx[i];
                if (!seen.Add(v))
                    continue;
                distinct++;
                int corner = cornerOf[v];
                if (corner < 0 || corners[corner] != -1)
                    return false;
                corners[corner] = v;
            }

            if (distinct != 4
                || corners[0] < 0 || corners[1] < 0 || corners[2] < 0 || corners[3] < 0)
                return false;

            quad = new CardQuad(
                corners[0], corners[1], corners[2], corners[3],
                vertices[corners[0]], vertices[corners[1]],
                vertices[corners[2]], vertices[corners[3]]);
            return true;
        }
    }
}
