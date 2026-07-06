using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewSpritePainter
    {
        void StampTile(Color[] canvas, int canvasWidth, Vector2Int tile, int pixelsPerTile, MenuPreviewSpriteData sprite);
        void StampTileOverlay(Color[] canvas, int width, int height, Vector2Int tile, int pixelsPerTile, MenuPreviewSpriteData sprite, float scale);
        void StampCentered(Color[] canvas, int width, int height, Vector2Int center, Vector2Int size, MenuPreviewSpriteData sprite, float shade);
        void StampCenteredOverlay(Color[] canvas, int width, int height, Vector2Int center, Vector2Int tileSize, MenuPreviewSpriteData sprite, float scale);
    }
}
