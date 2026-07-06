using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewSpritePainter : IMenuPreviewSpritePainter
    {
        public void StampTile(Color[] canvas, int canvasWidth, Vector2Int tile, int pixelsPerTile, MenuPreviewSpriteData sprite)
        {
            Stamp(canvas, canvasWidth, int.MaxValue, tile * pixelsPerTile, new Vector2Int(pixelsPerTile, pixelsPerTile), sprite, 1f);
        }

        public void StampTileOverlay(
            Color[] canvas,
            int width,
            int height,
            Vector2Int tile,
            int pixelsPerTile,
            MenuPreviewSpriteData sprite,
            float scale)
        {
            Vector2Int size = ResolveOverlaySize(sprite, pixelsPerTile, scale);
            var origin = new Vector2Int(
                tile.x * pixelsPerTile + (pixelsPerTile - size.x) / 2,
                tile.y * pixelsPerTile + pixelsPerTile - size.y);
            Stamp(canvas, width, height, origin, size, sprite, MenuPreviewTextureBuildConstants.OverlayAlphaScale);
        }

        public void StampCentered(
            Color[] canvas,
            int width,
            int height,
            Vector2Int center,
            Vector2Int size,
            MenuPreviewSpriteData sprite,
            float shade)
        {
            var origin = new Vector2Int(center.x - size.x / 2, center.y - size.y / 2);
            Stamp(canvas, width, height, origin, size, sprite, 1f, shade);
        }

        public void StampCenteredOverlay(
            Color[] canvas,
            int width,
            int height,
            Vector2Int center,
            Vector2Int tileSize,
            MenuPreviewSpriteData sprite,
            float scale)
        {
            Vector2Int size = ResolveOverlaySize(sprite, tileSize.y, scale);
            var origin = new Vector2Int(center.x - size.x / 2, center.y + tileSize.y / 2 - size.y);
            Stamp(canvas, width, height, origin, size, sprite, MenuPreviewTextureBuildConstants.OverlayAlphaScale);
        }

        private static Vector2Int ResolveOverlaySize(MenuPreviewSpriteData sprite, int baseSize, float scale)
        {
            int height = Mathf.Max(1, Mathf.RoundToInt(baseSize * Mathf.Max(scale, 0.1f)));
            int width = Mathf.Max(1, Mathf.RoundToInt(height * (sprite.Width / (float)Mathf.Max(1, sprite.Height))));
            return new Vector2Int(width, height);
        }

        private static void Stamp(
            Color[] canvas,
            int width,
            int height,
            Vector2Int origin,
            Vector2Int size,
            MenuPreviewSpriteData sprite,
            float alphaScale,
            float shade = 1f)
        {
            if (!sprite.IsValid)
                return;

            for (int py = 0; py < size.y; py++)
            {
                int canvasY = origin.y + py;
                if (canvasY < 0 || canvasY >= height)
                    continue;

                int sourceY = py * sprite.Height / size.y;
                for (int px = 0; px < size.x; px++)
                {
                    int canvasX = origin.x + px;
                    if (canvasX < 0 || canvasX >= width)
                        continue;

                    int sourceX = px * sprite.Width / size.x;
                    Color source = MenuPreviewColorUtility.Shade(sprite.Pixels[sourceY * sprite.Width + sourceX], shade);
                    source.a *= alphaScale;
                    if (source.a < 0.05f)
                        continue;

                    int index = canvasY * width + canvasX;
                    canvas[index] = source.a >= 0.95f ? source : MenuPreviewColorUtility.AlphaBlend(canvas[index], source);
                }
            }
        }
    }
}
