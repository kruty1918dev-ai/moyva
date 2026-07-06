using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewOverlayLayerRenderer : IMenuPreviewOverlayLayerRenderer
    {
        private readonly IMenuPreviewSpriteCacheFactory _cacheFactory;
        private readonly IMenuPreviewSpritePainter _spritePainter;
        private readonly IMenuPreviewProjectedShapePainter _shapePainter;

        public MenuPreviewOverlayLayerRenderer(
            IMenuPreviewSpriteCacheFactory cacheFactory,
            IMenuPreviewSpritePainter spritePainter,
            IMenuPreviewProjectedShapePainter shapePainter)
        {
            _cacheFactory = cacheFactory;
            _spritePainter = spritePainter;
            _shapePainter = shapePainter;
        }

        public void DrawObjects(MenuPreviewTextureBuildContext context)
        {
            var cache = _cacheFactory.BuildObjects(context.Request.ObjectRegistry, context.Settings);
            Draw(context, context.Request.PreviewData.ObjectMap, cache, MenuPreviewTextureBuildConstants.ObjectOverlayScale, "object");
        }

        public void DrawBuildings(MenuPreviewTextureBuildContext context)
        {
            if (context.Request.BuildingRegistry == null && MenuPreviewMapUtility.HasAnyValue(context.Request.PreviewData.BuildingMap))
            {
                Debug.LogWarning("[MenuPreview] BuildingMap has values, but BuildingRegistry is not assigned. Building overlay will be skipped.");
                return;
            }

            var cache = _cacheFactory.BuildBuildings(context.Request.BuildingRegistry, context.Settings);
            Draw(context, context.Request.PreviewData.BuildingMap, cache, MenuPreviewTextureBuildConstants.BuildingOverlayScale, "building");
        }

        private void Draw(
            MenuPreviewTextureBuildContext context,
            string[,] map,
            Dictionary<string, MenuPreviewSpriteData> cache,
            float scale,
            string label)
        {
            if (!MenuPreviewMapUtility.HasAnyValue(map))
                return;

            int sprites = 0;
            int dots = 0;
            var data = context.Request.PreviewData;
            for (int y = 0; y < data.Height; y++)
            for (int x = 0; x < data.Width; x++)
            {
                string id = MenuPreviewIdUtility.Normalize(map[x, y]);
                if (string.IsNullOrEmpty(id))
                    continue;

                if (MenuPreviewIdUtility.TryResolve(cache, id, out var sprite))
                {
                    DrawSprite(context, x, y, sprite, scale);
                    sprites++;
                }
                else
                {
                    DrawDot(context, x, y, MenuPreviewColorUtility.HashColor(id, MenuPreviewTextureBuildConstants.OverlayAlphaScale));
                    dots++;
                }
            }

            if (dots > 0)
                Debug.Log($"[MenuPreview] {label} fallback dots: {dots}, sprite overlays: {sprites}.");
        }

        private void DrawSprite(MenuPreviewTextureBuildContext context, int x, int y, MenuPreviewSpriteData sprite, float scale)
        {
            if (context.Layout.IsProjected)
                _spritePainter.StampCenteredOverlay(context.Canvas, context.Layout.TextureWidth, context.Layout.TextureHeight,
                    context.Layout.GetTileCenter(context.Request.PreviewData, x, y), context.Layout.TileDrawSize, sprite, scale);
            else
                _spritePainter.StampTileOverlay(context.Canvas, context.Layout.TextureWidth, context.Layout.TextureHeight,
                    new Vector2Int(x, y), context.Layout.PixelsPerTile, sprite, scale);
        }

        private void DrawDot(MenuPreviewTextureBuildContext context, int x, int y, Color color)
        {
            var center = context.Layout.IsProjected
                ? context.Layout.GetTileCenter(context.Request.PreviewData, x, y)
                : new Vector2Int(x * context.Layout.PixelsPerTile + context.Layout.PixelsPerTile / 2, y * context.Layout.PixelsPerTile + context.Layout.PixelsPerTile / 2);
            _shapePainter.StampDot(context.Canvas, context.Layout.TextureWidth, context.Layout.TextureHeight, center, context.Layout.DotRadius, color);
        }
    }
}
