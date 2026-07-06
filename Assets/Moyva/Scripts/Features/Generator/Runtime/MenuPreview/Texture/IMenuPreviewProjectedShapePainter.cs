using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewProjectedShapePainter
    {
        void FillTile(Color[] canvas, MenuPreviewTextureLayout layout, Vector2Int center, Color color);
        void StampDot(Color[] canvas, int width, int height, Vector2Int center, int radius, Color color);
    }
}
