using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>
    /// Produces trimmed representations of quad-card meshes. All outputs
    /// are new mesh copies — source mesh assets are never mutated. The
    /// caller owns the returned mesh and must destroy it when discarded.
    /// </summary>
    public static class CardMeshTrimmer
    {
        /// <summary>
        /// Cheap trim: identical geometry, tightened <see cref="Mesh.bounds"/>
        /// matching the visible rect. Bounds-driven consumers (grounding,
        /// footprint, culling) become correct without touching vertices.
        /// </summary>
        public static Mesh CreateBoundsRefitCopy(Mesh source, Bounds visibleLocalBounds)
        {
            if (source == null)
                return null;
            var copy = Object.Instantiate(source);
            copy.bounds = visibleLocalBounds;
            copy.name = source.name + "_visible";
            return copy;
        }

        /// <summary>
        /// Geometric trim: every full-UV card quad is shrunk to
        /// <paramref name="visibleUv"/> (padded by <paramref name="padUv"/>,
        /// clamped to the texture), so transparent padding no longer
        /// occupies geometry or fill-rate. Returns null when the mesh is
        /// not card-shaped or quads share vertices.
        /// </summary>
        public static Mesh CreateUvTrimmedCopy(Mesh source, Rect visibleUv, float padUv = 0f)
        {
            if (source == null)
                return null;

            var quads = new List<CardQuad>(4);
            if (QuadCardProjector.ExtractQuads(source, quads) == 0)
                return null;

            Rect rect = ExpandClamped(visibleUv, padUv);
            Vector3[] vertices = (Vector3[])source.vertices.Clone();
            Vector2[] uvs = (Vector2[])source.uv.Clone();
            var touched = new bool[vertices.Length];

            for (int i = 0; i < quads.Count; i++)
            {
                CardQuad q = quads[i];
                if (touched[q.I00] || touched[q.I10] || touched[q.I01] || touched[q.I11])
                    return null; // a vertex shared between quads cannot move twice

                Remap(vertices, uvs, touched, q, q.I00, new Vector2(rect.xMin, rect.yMin));
                Remap(vertices, uvs, touched, q, q.I10, new Vector2(rect.xMax, rect.yMin));
                Remap(vertices, uvs, touched, q, q.I01, new Vector2(rect.xMin, rect.yMax));
                Remap(vertices, uvs, touched, q, q.I11, new Vector2(rect.xMax, rect.yMax));
            }

            var copy = Object.Instantiate(source);
            copy.vertices = vertices;
            copy.uv = uvs;
            copy.RecalculateBounds();
            copy.name = source.name + "_trimmed";
            return copy;
        }

        private static void Remap(
            Vector3[] vertices, Vector2[] uvs, bool[] touched,
            in CardQuad q, int index, Vector2 uv)
        {
            vertices[index] = q.Bilinear(uv);
            uvs[index] = uv;
            touched[index] = true;
        }

        private static Rect ExpandClamped(Rect rect, float pad)
        {
            if (pad > 0f)
                rect = new Rect(rect.xMin - pad, rect.yMin - pad,
                    rect.width + pad * 2f, rect.height + pad * 2f);
            float xMin = Mathf.Clamp01(rect.xMin);
            float yMin = Mathf.Clamp01(rect.yMin);
            float xMax = Mathf.Clamp01(rect.xMax);
            float yMax = Mathf.Clamp01(rect.yMax);
            return new Rect(xMin, yMin, Mathf.Max(0f, xMax - xMin), Mathf.Max(0f, yMax - yMin));
        }
    }
}
