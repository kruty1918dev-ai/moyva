using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MenuPreviewSpriteTextureReader
    {
        public static Color[] TryReadViaRenderTexture(Sprite sprite, int x, int y, int width, int height)
        {
            RenderTexture temporary = null;
            RenderTexture previous = RenderTexture.active;
            Texture2D readable = null;

            try
            {
                int textureWidth = Mathf.Max(1, sprite.texture.width);
                int textureHeight = Mathf.Max(1, sprite.texture.height);
                temporary = RenderTexture.GetTemporary(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(sprite.texture, temporary);

                readable = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
                RenderTexture.active = temporary;
                readable.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0, false);
                readable.Apply(false, false);

                int safeX = Mathf.Clamp(x, 0, textureWidth - 1);
                int safeY = Mathf.Clamp(y, 0, textureHeight - 1);
                int safeW = Mathf.Clamp(width, 1, textureWidth - safeX);
                int safeH = Mathf.Clamp(height, 1, textureHeight - safeY);
                return readable.GetPixels(safeX, safeY, safeW, safeH);
            }
            catch
            {
                return null;
            }
            finally
            {
                RenderTexture.active = previous;
                if (readable != null)
                    Destroy(readable);
                if (temporary != null)
                    RenderTexture.ReleaseTemporary(temporary);
            }
        }

        private static void Destroy(Object instance)
        {
            if (Application.isPlaying)
                Object.Destroy(instance);
            else
                Object.DestroyImmediate(instance);
        }
    }
}
