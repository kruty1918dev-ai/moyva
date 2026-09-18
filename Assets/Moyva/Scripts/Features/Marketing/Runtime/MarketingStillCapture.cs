using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Render-to-RenderTexture still capture — exact resolution, independent
    /// of GameView size, deterministic. Also produces FrameMetrics for the
    /// golden-frame evaluator.
    /// </summary>
    public sealed class MarketingStillCapture
    {
        /// <summary>Render the camera at w×h and return a readable Texture2D.</summary>
        public Texture2D Render(Camera camera, int w, int h, int msaa = 4)
        {
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = Mathf.Clamp(msaa, 1, 8),
            };
            var prevTarget = camera.targetTexture;
            var prevActive = RenderTexture.active;
            try
            {
                camera.targetTexture = rt;
                camera.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply(false, false);
                return tex;
            }
            finally
            {
                camera.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                rt.Release();
                Object.Destroy(rt);
            }
        }

        /// <summary>
        /// Compute FrameMetrics from a captured frame. Pixel stats are sampled
        /// (stride) to keep the check fast even at 4K.
        /// </summary>
        public FrameMetrics Measure(
            Texture2D frame, Camera camera, Vector3 subjectPos, float subjectRadius,
            float safeMargin, float aspect)
        {
            var m = new FrameMetrics { subjectInsideSafeArea = true };

            if (frame != null)
            {
                int stride = Mathf.Max(1, frame.width / 128);
                int black = 0, white = 0, magenta = 0, total = 0;
                double lumSum = 0, lumSq = 0;
                var pixels = frame.GetPixels32();
                for (int i = 0; i < pixels.Length; i += stride)
                {
                    var p = pixels[i];
                    float lum = (0.2126f * p.r + 0.7152f * p.g + 0.0722f * p.b) / 255f;
                    lumSum += lum;
                    lumSq += lum * lum;
                    if (p.r < 16 && p.g < 16 && p.b < 16) black++;
                    if (p.r > 239 && p.g > 239 && p.b > 239) white++;
                    if (p.r > 200 && p.b > 200 && p.g < 60) magenta++;
                    total++;
                }
                m.blackFraction = (float)black / total;
                m.whiteFraction = (float)white / total;
                m.magentaFraction = (float)magenta / total;
                m.luminanceMean = (float)(lumSum / total);
                double var = lumSq / total - m.luminanceMean * (double)m.luminanceMean;
                m.luminanceVariance = Mathf.Max(0f, (float)var);
            }

            if (camera != null)
            {
                Vector3 vp = camera.WorldToViewportPoint(subjectPos);
                m.subjectInsideSafeArea =
                    vp.z > 0f
                    && vp.x >= safeMargin && vp.x <= 1f - safeMargin
                    && vp.y >= safeMargin && vp.y <= 1f - safeMargin;

                // Screen-space subject footprint estimate.
                Vector3 edge = subjectPos + camera.transform.right * subjectRadius;
                Vector3 ve = camera.WorldToViewportPoint(edge);
                float rScreen = Mathf.Abs(ve.x - vp.x);
                m.subjectScreenArea = Mathf.Clamp01(Mathf.PI * rScreen * rScreen);

                float idealX = 0.5f, idealY = 0.45f;
                m.subjectCenterOffset = Mathf.Clamp01(
                    Vector2.Distance(new Vector2(vp.x, vp.y), new Vector2(idealX, idealY)) / 0.7f);
                m.subjectClippedFraction = vp.z <= 0f ? 1f
                    : Mathf.Clamp01(Mathf.Max(0f, rScreen - Mathf.Min(vp.x, 1f - vp.x)) / Mathf.Max(rScreen, 1e-4f));
            }
            return m;
        }
    }
}
