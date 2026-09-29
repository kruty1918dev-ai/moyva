using UnityEngine;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>
    /// Builds cropped sprites over the visible bounds. The pivot is the
    /// support point normalized inside the trimmed rect, so placing the
    /// sprite at the ground lands its visible bottom edge on the surface.
    /// </summary>
    public static class TrimmedSpriteFactory
    {
        /// <summary>
        /// Creates a sprite over <paramref name="bounds"/> pixels of
        /// <paramref name="texture"/> (bottom-left pixel space, same as
        /// <see cref="Texture2D.GetPixels32"/>). <paramref name="supportUv"/>
        /// is normalized over the full texture; the resulting pivot is the
        /// same point expressed inside the trimmed rect.
        /// </summary>
        public static Sprite CreateTrimmed(
            Texture2D texture,
            in VisibleBounds bounds,
            Vector2 supportUv,
            float pixelsPerUnit = 100f)
        {
            if (texture == null || !bounds.HasContent)
                return null;

            Vector2 pivot = PivotInside(bounds.Uv, supportUv);
            var rect = new Rect(bounds.Pixels.x, bounds.Pixels.y, bounds.Pixels.width, bounds.Pixels.height);
            return Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        }

        /// <summary>
        /// Same for a sprite whose visible bounds were measured relative to
        /// the sprite's own texture rect (e.g. via <see cref="SpritePixelSource"/>).
        /// </summary>
        public static Sprite CreateTrimmed(
            Sprite source,
            in VisibleBounds boundsInSpriteSpace,
            Vector2 supportUvInSpriteSpace,
            float pixelsPerUnit = 0f)
        {
            if (source == null || !boundsInSpriteSpace.HasContent)
                return null;

            Rect tr = source.textureRect;
            var sr = new RectInt(
                Mathf.RoundToInt(tr.x), Mathf.RoundToInt(tr.y),
                Mathf.RoundToInt(tr.width), Mathf.RoundToInt(tr.height));
            var textureRect = new Rect(
                sr.x + boundsInSpriteSpace.Pixels.x,
                sr.y + boundsInSpriteSpace.Pixels.y,
                boundsInSpriteSpace.Pixels.width,
                boundsInSpriteSpace.Pixels.height);
            Vector2 supportTextureUv = new Vector2(
                (sr.x + supportUvInSpriteSpace.x * sr.width) / source.texture.width,
                (sr.y + supportUvInSpriteSpace.y * sr.height) / source.texture.height);

            var spriteBounds = new VisibleBounds(
                new RectInt((int)textureRect.x, (int)textureRect.y,
                    (int)textureRect.width, (int)textureRect.height),
                new Rect(
                    textureRect.x / source.texture.width,
                    textureRect.y / source.texture.height,
                    textureRect.width / source.texture.width,
                    textureRect.height / source.texture.height),
                true);
            return CreateTrimmed(
                source.texture,
                spriteBounds,
                supportTextureUv,
                pixelsPerUnit > 0f ? pixelsPerUnit : source.pixelsPerUnit);
        }

        private static Vector2 PivotInside(Rect rect, Vector2 uv)
        {
            if (rect.width <= 0f || rect.height <= 0f)
                return new Vector2(0.5f, 0f);
            return new Vector2(
                Mathf.Clamp01((uv.x - rect.xMin) / rect.width),
                Mathf.Clamp01((uv.y - rect.yMin) / rect.height));
        }
    }
}
