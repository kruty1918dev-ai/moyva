using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewTextureLayoutService : IMenuPreviewTextureLayoutService
    {
        public MenuPreviewTextureLayout Create(MenuWorldPreviewTextureBuildRequest request, MoyvaProjectSettingsSO settings)
        {
            int pixels = Mathf.Max(1, request.PixelsPerTile);
            int maxEdge = Mathf.Max(64, request.MaxTextureEdge);
            var layout = CreateUnscaled(request.PreviewData, pixels, settings);
            int longest = Mathf.Max(layout.TextureWidth, layout.TextureHeight);
            if (longest <= maxEdge)
                return layout;

            pixels = Mathf.Max(1, Mathf.FloorToInt(pixels * (maxEdge / (float)longest)));
            return CreateUnscaled(request.PreviewData, pixels, settings);
        }

        private static MenuPreviewTextureLayout CreateUnscaled(
            MenuWorldPreviewData data,
            int pixels,
            MoyvaProjectSettingsSO settings)
        {
            bool hex = settings.DefaultGridTopology == GridTopology.HexAxial;
            bool iso = settings.DefaultProjectionMode == GridProjectionMode.Isometric3DPreview;
            bool projected = iso || hex || settings.DefaultProjectionMode == GridProjectionMode.Orthographic3D;
            int heightPixels = ResolveHeightPixels(data, pixels, settings, out var heightRange);

            if (iso)
                return CreateIso(data, pixels, settings.DefaultProjectionMode, heightPixels, heightRange);
            if (hex)
                return CreateHex(data, pixels, settings, heightPixels, heightRange);

            return new MenuPreviewTextureLayout(
                settings.DefaultProjectionMode,
                pixels,
                data.Width * pixels,
                data.Height * pixels + heightPixels,
                new Vector2Int(pixels, pixels),
                new Vector2Int(0, heightPixels),
                new Vector2Int(pixels, pixels),
                heightPixels,
                heightRange,
                projected,
                false,
                false,
                false);
        }

        private static MenuPreviewTextureLayout CreateIso(
            MenuWorldPreviewData data,
            int pixels,
            GridProjectionMode mode,
            int heightPixels,
            Vector2 heightRange)
        {
            int halfW = Mathf.Max(2, pixels);
            int halfH = Mathf.Max(1, Mathf.RoundToInt(pixels * 0.5f));
            return new MenuPreviewTextureLayout(
                mode, pixels, (data.Width + data.Height + 2) * halfW + halfW * 2,
                (data.Width + data.Height + 2) * halfH + halfH * 2 + heightPixels,
                new Vector2Int(halfW * 2, halfH * 2),
                new Vector2Int(data.Height * halfW + halfW, heightPixels + halfH + 1),
                new Vector2Int(halfW, halfH),
                heightPixels, heightRange, true, false, false, true);
        }

        private static MenuPreviewTextureLayout CreateHex(
            MenuWorldPreviewData data,
            int pixels,
            MoyvaProjectSettingsSO settings,
            int heightPixels,
            Vector2 heightRange)
        {
            bool pointy = settings.HexOrientation == HexOrientation.PointyTop;
            int radius = Mathf.Max(2, pixels);
            int w = pointy ? Mathf.RoundToInt(Mathf.Sqrt(3f) * radius) : radius * 2;
            int h = pointy ? radius * 2 : Mathf.RoundToInt(Mathf.Sqrt(3f) * radius);
            var step = pointy ? new Vector2Int(w, Mathf.RoundToInt(radius * 1.5f)) : new Vector2Int(Mathf.RoundToInt(radius * 1.5f), h);
            int texW = pointy ? data.Width * w + w * 2 : (data.Width - 1) * step.x + w * 2;
            int texH = pointy ? (data.Height - 1) * step.y + h * 2 + heightPixels : data.Height * h + h + heightPixels;
            return new MenuPreviewTextureLayout(settings.DefaultProjectionMode, pixels, texW, texH,
                new Vector2Int(w, h), new Vector2Int(w, heightPixels + h / 2 + 1), step,
                heightPixels, heightRange, true, true, pointy, false);
        }

        private static int ResolveHeightPixels(MenuWorldPreviewData data, int pixels, MoyvaProjectSettingsSO settings, out Vector2 range)
        {
            range = MenuPreviewMapUtility.CalculateHeightRange(data);
            return settings.UseHeightForPreview && data.HeightMap != null
                ? Mathf.RoundToInt(pixels * Mathf.Max(0f, settings.HeightScale) * 3f)
                : 0;
        }
    }
}
