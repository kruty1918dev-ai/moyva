using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewProjectedShapePainter : IMenuPreviewProjectedShapePainter
    {
        public void FillTile(Color[] canvas, MenuPreviewTextureLayout layout, Vector2Int center, Color color)
        {
            if (layout.IsHex)
            {
                StampDot(canvas, layout.TextureWidth, layout.TextureHeight, center, layout.DotRadius, color);
                return;
            }

            if (layout.IsIsometricLike)
            {
                FillDiamond(canvas, layout, center, color);
                return;
            }

            FillRect(canvas, layout.TextureWidth, layout.TextureHeight, center, layout.TileDrawSize, color);
        }

        public void StampDot(Color[] canvas, int width, int height, Vector2Int center, int radius, Color color)
        {
            radius = Mathf.Max(1, radius);
            int radiusSquared = radius * radius;
            for (int y = -radius; y <= radius; y++)
            {
                int canvasY = center.y + y;
                if (canvasY < 0 || canvasY >= height)
                    continue;

                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y > radiusSquared)
                        continue;

                    int canvasX = center.x + x;
                    if (canvasX < 0 || canvasX >= width)
                        continue;

                    int index = canvasY * width + canvasX;
                    canvas[index] = MenuPreviewColorUtility.AlphaBlend(canvas[index], color);
                }
            }
        }

        private static void FillDiamond(Color[] canvas, MenuPreviewTextureLayout layout, Vector2Int center, Color color)
        {
            int halfWidth = Mathf.Max(1, layout.TileDrawSize.x / 2);
            int halfHeight = Mathf.Max(1, layout.TileDrawSize.y / 2);
            for (int y = -halfHeight; y <= halfHeight; y++)
            {
                int canvasY = center.y + y;
                if (canvasY < 0 || canvasY >= layout.TextureHeight)
                    continue;

                int span = Mathf.Max(1, Mathf.RoundToInt(halfWidth * (1f - Mathf.Abs(y) / (float)halfHeight)));
                for (int x = -span; x <= span; x++)
                {
                    int canvasX = center.x + x;
                    if (canvasX < 0 || canvasX >= layout.TextureWidth)
                        continue;

                    int index = canvasY * layout.TextureWidth + canvasX;
                    canvas[index] = MenuPreviewColorUtility.AlphaBlend(canvas[index], color);
                }
            }
        }

        private static void FillRect(Color[] canvas, int width, int height, Vector2Int center, Vector2Int size, Color color)
        {
            int startX = center.x - size.x / 2;
            int startY = center.y - size.y / 2;
            for (int y = 0; y < size.y; y++)
            {
                int canvasY = startY + y;
                if (canvasY < 0 || canvasY >= height)
                    continue;

                for (int x = 0; x < size.x; x++)
                {
                    int canvasX = startX + x;
                    if (canvasX >= 0 && canvasX < width)
                        canvas[canvasY * width + canvasX] = color;
                }
            }
        }
    }
}
