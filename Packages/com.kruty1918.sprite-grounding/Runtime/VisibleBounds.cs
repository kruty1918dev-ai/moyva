using UnityEngine;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>
    /// Opaque-content rect of a source image. <see cref="Pixels"/> is in
    /// bottom-left row-major pixel space (y = 0 is the bottom row);
    /// <see cref="Uv"/> is the equivalent normalized rect (v = 0 at the
    /// bottom, matching Unity UVs).
    /// </summary>
    public readonly struct VisibleBounds
    {
        public VisibleBounds(RectInt pixels, Rect uv, bool hasContent)
        {
            Pixels = pixels;
            Uv = uv;
            HasContent = hasContent;
        }

        public static VisibleBounds Empty => new VisibleBounds(default, default, false);

        public readonly RectInt Pixels;
        public readonly Rect Uv;
        public readonly bool HasContent;
    }
}
