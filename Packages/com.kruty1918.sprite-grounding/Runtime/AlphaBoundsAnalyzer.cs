using System;
using UnityEngine;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>
    /// Detects the visible (opaque) rect of an image by scanning its alpha
    /// channel. Pure function of pixels + parameters — same input always
    /// yields the same rect on every machine and call order.
    /// </summary>
    public static class AlphaBoundsAnalyzer
    {
        /// <summary>
        /// Byte cutoff matching shader alpha-clip semantics: a pixel is
        /// visible iff <c>alpha / 255 &gt;= threshold</c>.
        /// </summary>
        public static byte ThresholdByte(float alphaThreshold)
            => (byte)Mathf.Clamp(
                Mathf.CeilToInt(Mathf.Clamp01(alphaThreshold) * 255f - 1e-4f),
                0, 255);

        /// <summary>
        /// Scans a bottom-left row-major pixel buffer and returns the
        /// minimal rect containing all opaque pixels, expanded by
        /// <paramref name="paddingPixels"/> and clamped to the image.
        /// </summary>
        public static VisibleBounds AnalyzePixels(
            ReadOnlySpan<Color32> pixels,
            int width,
            int height,
            float alphaThreshold,
            int paddingPixels = 0)
        {
            if (pixels.Length < width * height || width <= 0 || height <= 0)
                return VisibleBounds.Empty;

            byte cutoff = ThresholdByte(alphaThreshold);
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                for (int x = 0; x < width; x++)
                {
                    if (pixels[row + x].a < cutoff)
                        continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < 0)
                return VisibleBounds.Empty;

            int pad = Math.Max(0, paddingPixels);
            minX = Math.Max(0, minX - pad);
            minY = Math.Max(0, minY - pad);
            maxX = Math.Min(width - 1, maxX + pad);
            maxY = Math.Min(height - 1, maxY + pad);

            var pixelRect = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
            var uvRect = new Rect(
                minX / (float)width,
                minY / (float)height,
                pixelRect.width / (float)width,
                pixelRect.height / (float)height);
            return new VisibleBounds(pixelRect, uvRect, true);
        }

        /// <summary>
        /// Reads the source and analyzes it. Returns false when the pixels
        /// cannot be obtained; <paramref name="buffer"/> may carry a reusable
        /// <c>Width * Height</c> scratch array to avoid allocation.
        /// </summary>
        public static bool TryAnalyze(
            IPixelSource source,
            float alphaThreshold,
            int paddingPixels,
            out VisibleBounds bounds,
            Color32[] buffer = null)
        {
            bounds = default;
            if (source == null || source.Width <= 0 || source.Height <= 0)
                return false;

            if (buffer == null || buffer.Length < source.Width * source.Height)
                buffer = new Color32[source.Width * source.Height];

            if (!source.TryCopyPixels(buffer))
                return false;

            bounds = AnalyzePixels(buffer, source.Width, source.Height, alphaThreshold, paddingPixels);
            return true;
        }
    }
}
