using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct MenuPreviewTextureLayout
    {
        public MenuPreviewTextureLayout(
            GridProjectionMode mode,
            int pixelsPerTile,
            int textureWidth,
            int textureHeight,
            Vector2Int tileDrawSize,
            Vector2Int offset,
            Vector2Int step,
            int heightPixelRange,
            Vector2 heightRange,
            bool projected,
            bool hex,
            bool hexPointy,
            bool isometricLike)
        {
            ProjectionMode = mode;
            PixelsPerTile = pixelsPerTile;
            TextureWidth = textureWidth;
            TextureHeight = textureHeight;
            TileDrawSize = tileDrawSize;
            Offset = offset;
            Step = step;
            HeightPixelRange = heightPixelRange;
            HeightRange = heightRange;
            IsProjected = projected;
            IsHex = hex;
            IsHexPointy = hexPointy;
            IsIsometricLike = isometricLike;
        }

        public GridProjectionMode ProjectionMode { get; }
        public int PixelsPerTile { get; }
        public int TextureWidth { get; }
        public int TextureHeight { get; }
        public Vector2Int TileDrawSize { get; }
        public Vector2Int Offset { get; }
        public Vector2Int Step { get; }
        public int DotRadius => Mathf.Max(1, Mathf.RoundToInt(PixelsPerTile * 0.35f));
        public int HeightPixelRange { get; }
        public Vector2 HeightRange { get; }
        public bool UseHeight => HeightPixelRange > 0;
        public bool IsProjected { get; }
        public bool IsHex { get; }
        public bool IsHexPointy { get; }
        public bool IsIsometricLike { get; }

        public Vector2Int GetTileCenter(MenuWorldPreviewData data, int x, int y)
        {
            int heightOffset = Mathf.RoundToInt(GetNormalizedHeight(data, x, y) * HeightPixelRange);
            if (IsIsometricLike)
                return new Vector2Int(Offset.x + (x - y) * Step.x, Offset.y + (x + y) * Step.y - heightOffset);
            if (IsHex && IsHexPointy)
                return new Vector2Int(Offset.x + x * Step.x + ((y & 1) != 0 ? Step.x / 2 : 0), Offset.y + y * Step.y - heightOffset);
            if (IsHex)
                return new Vector2Int(Offset.x + x * Step.x, Offset.y + y * Step.y + ((x & 1) != 0 ? Step.y / 2 : 0) - heightOffset);

            return new Vector2Int(
                Offset.x + x * PixelsPerTile + PixelsPerTile / 2,
                Offset.y + y * PixelsPerTile + PixelsPerTile / 2 - heightOffset);
        }

        public float GetNormalizedHeight(MenuWorldPreviewData data, int x, int y)
        {
            if (!UseHeight || data.HeightMap == null)
                return 0f;

            float span = Mathf.Max(0.0001f, HeightRange.y - HeightRange.x);
            return Mathf.Clamp01((data.HeightMap[x, y] - HeightRange.x) / span);
        }

        public float GetHeightShade(MenuWorldPreviewData data, int x, int y)
        {
            return UseHeight ? Mathf.Lerp(0.78f, 1.18f, GetNormalizedHeight(data, x, y)) : 1f;
        }
    }
}
