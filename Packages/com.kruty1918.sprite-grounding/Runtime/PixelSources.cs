using System;
using UnityEngine;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>
    /// Read access to source pixels in bottom-left row-major order:
    /// <c>pixels[y * Width + x]</c> with <c>y = 0</c> at the bottom row —
    /// the same convention as <see cref="Texture2D.GetPixels32()"/>.
    /// </summary>
    public interface IPixelSource
    {
        int Width { get; }
        int Height { get; }

        /// <summary>
        /// Copies all pixels into <paramref name="destination"/>
        /// (length must be at least <c>Width * Height</c>). Returns false
        /// when pixels are not obtainable in the current context
        /// (non-readable texture, missing asset, off-main-thread).
        /// </summary>
        bool TryCopyPixels(Color32[] destination);
    }

    /// <summary>Pixel source over a caller-owned array (tests, pre-read data).</summary>
    public sealed class ArrayPixelSource : IPixelSource
    {
        private readonly Color32[] _pixels;

        public ArrayPixelSource(Color32[] pixels, int width, int height)
        {
            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (pixels.Length < width * height)
                throw new ArgumentException("Pixel array is smaller than width * height.", nameof(pixels));

            _pixels = pixels;
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }

        public bool TryCopyPixels(Color32[] destination)
        {
            if (destination == null || destination.Length < Width * Height)
                return false;
            Array.Copy(_pixels, destination, Width * Height);
            return true;
        }
    }

    /// <summary>
    /// Pixel source over a readable <see cref="Texture2D"/>.
    /// Returns false from <see cref="TryCopyPixels"/> when the texture is
    /// not readable — use <see cref="RenderTexturePixelSource"/> instead.
    /// </summary>
    public sealed class TexturePixelSource : IPixelSource
    {
        private readonly Texture2D _texture;

        public TexturePixelSource(Texture2D texture)
        {
            _texture = texture;
        }

        public Texture2D Texture => _texture;
        public int Width => _texture != null ? _texture.width : 0;
        public int Height => _texture != null ? _texture.height : 0;

        public bool TryCopyPixels(Color32[] destination)
        {
            if (_texture == null || !_texture.isReadable
                || destination == null || destination.Length < Width * Height)
                return false;

            try
            {
                Color32[] pixels = _texture.GetPixels32(0);
                Array.Copy(pixels, destination, Width * Height);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Pixel source cropping a <see cref="Sprite"/> to its texture rect.
    /// Requires the underlying texture to be readable.
    /// </summary>
    public sealed class SpritePixelSource : IPixelSource
    {
        private readonly Sprite _sprite;

        public SpritePixelSource(Sprite sprite)
        {
            _sprite = sprite;
        }

        public int Width => _sprite != null ? (int)_sprite.textureRect.width : 0;
        public int Height => _sprite != null ? (int)_sprite.textureRect.height : 0;

        public bool TryCopyPixels(Color32[] destination)
        {
            Texture2D texture = _sprite != null ? _sprite.texture : null;
            if (texture == null || !texture.isReadable
                || destination == null || destination.Length < Width * Height)
                return false;

            try
            {
                Color32[] all = texture.GetPixels32(0);
                // textureRect is expressed in bottom-left pixel space,
                // matching GetPixels32 row order.
                Rect tr = _sprite.textureRect;
                var rect = new RectInt(
                    Mathf.RoundToInt(tr.x), Mathf.RoundToInt(tr.y),
                    Mathf.RoundToInt(tr.width), Mathf.RoundToInt(tr.height));
                for (int y = 0; y < rect.height; y++)
                {
                    int src = (rect.y + y) * texture.width + rect.x;
                    int dst = y * rect.width;
                    Array.Copy(all, src, destination, dst, rect.width);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Pixel source for non-readable textures: blits into a temporary
    /// render texture and reads pixels back. Main thread only; intended
    /// for one-shot analysis per texture, never per frame.
    /// </summary>
    public sealed class RenderTexturePixelSource : IPixelSource
    {
        private readonly Texture2D _texture;

        public RenderTexturePixelSource(Texture2D texture)
        {
            _texture = texture;
        }

        public Texture2D Texture => _texture;
        public int Width => _texture != null ? _texture.width : 0;
        public int Height => _texture != null ? _texture.height : 0;

        public bool TryCopyPixels(Color32[] destination)
        {
            if (_texture == null
                || destination == null || destination.Length < Width * Height)
                return false;

            RenderTexture previous = RenderTexture.active;
            RenderTexture target = RenderTexture.GetTemporary(
                Width, Height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Texture2D scratch = null;
            try
            {
                Graphics.Blit(_texture, target);
                RenderTexture.active = target;
                scratch = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                scratch.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
                scratch.Apply();
                RenderTexture.active = previous;

                Color32[] pixels = scratch.GetPixels32();
                Array.Copy(pixels, destination, Width * Height);
                return true;
            }
            catch (Exception)
            {
                RenderTexture.active = previous;
                return false;
            }
            finally
            {
                if (scratch != null)
                    UnityEngine.Object.DestroyImmediate(scratch);
                RenderTexture.ReleaseTemporary(target);
            }
        }
    }

    /// <summary>
    /// Picks the cheapest working pixel source for a texture:
    /// direct read when readable, GPU readback otherwise.
    /// </summary>
    public static class PixelSource
    {
        public static IPixelSource ForTexture(Texture2D texture)
        {
            if (texture == null)
                return null;
            return texture.isReadable
                ? (IPixelSource)new TexturePixelSource(texture)
                : new RenderTexturePixelSource(texture);
        }
    }
}
