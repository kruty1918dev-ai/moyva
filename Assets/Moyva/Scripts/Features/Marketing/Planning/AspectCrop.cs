using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Planning
{
    /// <summary>
    /// Composition-aware crop math for deriving platform variants from a
    /// master capture. Never a blind center crop: the subject focus point
    /// stays inside the crop and prefers the rule-of-thirds band.
    /// </summary>
    public static class AspectCrop
    {
        /// <summary>
        /// Computes the pixel crop rect in the source image for a target aspect.
        /// <paramref name="focus"/> is the subject focus in normalized source
        /// coords (0..1). <paramref name="maxZoomOut"/> limits how much may be
        /// cropped (1 = allow any crop that fits the aspect).
        /// </summary>
        public static RectInt Compute(int srcW, int srcH, float targetAspect, Vector2 focus)
        {
            if (srcW <= 0 || srcH <= 0 || targetAspect <= 0f)
                return new RectInt(0, 0, Mathf.Max(1, srcW), Mathf.Max(1, srcH));

            float srcAspect = (float)srcW / srcH;
            int cropW, cropH;
            if (targetAspect < srcAspect)
            {
                cropH = srcH;
                cropW = Mathf.Max(1, Mathf.RoundToInt(srcH * targetAspect));
            }
            else
            {
                cropW = srcW;
                cropH = Mathf.Max(1, Mathf.RoundToInt(srcW / targetAspect));
            }

            float focusX = Mathf.Clamp01(focus.x) * srcW;
            float focusY = Mathf.Clamp01(focus.y) * srcH;

            // Prefer keeping the focus near the inner third of the crop
            // (rule of thirds), but clamp fully inside the source.
            float minX = Mathf.Max(0f, focusX - cropW * 0.66f);
            float maxX = Mathf.Min(srcW - cropW, focusX - cropW * 0.33f);
            float x = maxX >= minX
                ? Mathf.Clamp(focusX - cropW * 0.5f, minX, maxX)
                : Mathf.Clamp(focusX - cropW * 0.5f, 0f, srcW - cropW);

            float minY = Mathf.Max(0f, focusY - cropH * 0.66f);
            float maxY = Mathf.Min(srcH - cropH, focusY - cropH * 0.33f);
            float y = maxY >= minY
                ? Mathf.Clamp(focusY - cropH * 0.5f, minY, maxY)
                : Mathf.Clamp(focusY - cropH * 0.5f, 0f, srcH - cropH);

            return new RectInt(Mathf.RoundToInt(x), Mathf.RoundToInt(y), cropW, cropH);
        }

        /// <summary>GPU path: blit source region into a texture of the target
        /// resolution. Returns a new Texture2D the caller must manage.</summary>
        public static Texture2D Apply(Texture2D source, RectInt crop, int outW, int outH)
        {
            int w = Mathf.Max(1, outW);
            int h = Mathf.Max(1, outH);
            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Bilinear,
            };
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            try
            {
                float sx = (float)crop.x / source.width;
                float sy = (float)crop.y / source.height;
                float sw = (float)crop.width / source.width;
                float sh = (float)crop.height / source.height;
                Graphics.Blit(source, rt, new Vector2(sw, sh), new Vector2(sx, sy));
                var output = new Texture2D(w, h, TextureFormat.RGBA32, false);
                output.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                output.Apply(false, false);
                return output;
            }
            finally
            {
                RenderTexture.active = prev;
                rt.Release();
                Object.Destroy(rt);
            }
        }
    }
}
