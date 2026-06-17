using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    internal static class ObjectPlacementPreviewUtility
    {
        public static Texture2D BuildMaskTexture(bool[,] mask, Color on, Color off)
        {
            int w = Mathf.Max(1, mask?.GetLength(0) ?? 0);
            int h = Mathf.Max(1, mask?.GetLength(1) ?? 0);
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                    texture.SetPixel(x, y, mask != null && mask[x, y] ? on : off);
            }

            texture.Apply(false, false);
            return texture;
        }

        public static Texture2D BuildScatterTexture(
            ScatterMask mask,
            IReadOnlyList<ScatterCandidate> candidates)
        {
            int w = Mathf.Max(1, mask?.Width ?? 0);
            int h = Mathf.Max(1, mask?.Height ?? 0);
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool allowed = mask != null && mask.IsAllowed(x, y);
                    texture.SetPixel(x, y, allowed
                        ? new Color(0.10f, 0.19f, 0.14f, 1f)
                        : new Color(0.035f, 0.04f, 0.055f, 1f));
                }
            }

            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    var cell = candidates[i].Cell;
                    if (cell.x >= 0 && cell.x < w && cell.y >= 0 && cell.y < h)
                        texture.SetPixel(cell.x, cell.y, new Color(0.78f, 0.95f, 0.45f, 1f));
                }
            }

            texture.Apply(false, false);
            return texture;
        }
    }
}
