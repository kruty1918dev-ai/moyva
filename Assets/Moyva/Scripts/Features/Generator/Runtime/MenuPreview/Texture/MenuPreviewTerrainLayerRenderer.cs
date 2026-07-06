using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewTerrainLayerRenderer : IMenuPreviewTerrainLayerRenderer
    {
        private readonly IMenuPreviewSpritePainter _spritePainter;
        private readonly IMenuPreviewProjectedShapePainter _shapePainter;

        public MenuPreviewTerrainLayerRenderer(
            IMenuPreviewSpritePainter spritePainter,
            IMenuPreviewProjectedShapePainter shapePainter)
        {
            _spritePainter = spritePainter;
            _shapePainter = shapePainter;
        }

        public void Draw(MenuPreviewTextureBuildContext context)
        {
            var data = context.Request.PreviewData;
            if (!MenuPreviewMapUtility.HasAnyValue(data.BiomeMap))
            {
                DrawHeightFallback(context);
                return;
            }

            int hits = 0;
            int misses = 0;
            var missed = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for (int y = 0; y < data.Height; y++)
            for (int x = 0; x < data.Width; x++)
            {
                string id = MenuPreviewIdUtility.Normalize(data.BiomeMap[x, y]);
                if (MenuPreviewIdUtility.TryResolve(context.TileSprites, id, out var sprite))
                {
                    DrawTileSprite(context, x, y, sprite);
                    hits++;
                    continue;
                }

                DrawFallbackTile(context, x, y, id, missed);
                misses++;
            }

            Debug.Log($"[MenuPreview] Terrain render: {hits} hits, {misses} misses. " +
                (missed.Count > 0 ? $"Unknown tile IDs: [{string.Join(", ", missed)}]" : "All IDs resolved."));
        }

        private void DrawHeightFallback(MenuPreviewTextureBuildContext context)
        {
            var data = context.Request.PreviewData;
            for (int y = 0; y < data.Height; y++)
            for (int x = 0; x < data.Width; x++)
            {
                float t = data.HeightMap == null ? 0f : context.Layout.GetNormalizedHeight(data, x, y);
                DrawTileColor(context, x, y, new Color(t, t, t, 1f));
            }
        }

        private void DrawFallbackTile(MenuPreviewTextureBuildContext context, int x, int y, string id, HashSet<string> missed)
        {
            if (!string.IsNullOrEmpty(id))
                missed.Add(id);

            if (context.FallbackTile.IsValid)
                DrawTileSprite(context, x, y, context.FallbackTile);
            else
                DrawTileColor(context, x, y, Color.black);
        }

        private void DrawTileSprite(MenuPreviewTextureBuildContext context, int x, int y, MenuPreviewSpriteData sprite)
        {
            if (context.Layout.IsProjected)
            {
                var center = context.Layout.GetTileCenter(context.Request.PreviewData, x, y);
                _spritePainter.StampCentered(context.Canvas, context.Layout.TextureWidth, context.Layout.TextureHeight,
                    center, context.Layout.TileDrawSize, sprite, context.Layout.GetHeightShade(context.Request.PreviewData, x, y));
                return;
            }

            _spritePainter.StampTile(context.Canvas, context.Layout.TextureWidth, new Vector2Int(x, y), context.Layout.PixelsPerTile, sprite);
        }

        private void DrawTileColor(MenuPreviewTextureBuildContext context, int x, int y, Color color)
        {
            if (context.Layout.IsProjected)
            {
                _shapePainter.FillTile(context.Canvas, context.Layout, context.Layout.GetTileCenter(context.Request.PreviewData, x, y), color);
                return;
            }

            int startX = x * context.Layout.PixelsPerTile;
            int startY = y * context.Layout.PixelsPerTile;
            for (int py = 0; py < context.Layout.PixelsPerTile; py++)
            for (int px = 0; px < context.Layout.PixelsPerTile; px++)
                context.Canvas[(startY + py) * context.Layout.TextureWidth + startX + px] = color;
        }
    }
}
