using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewKingdomPlacementGeometry : IMenuPreviewKingdomPlacementGeometry
    {
        public bool IsInBounds(MenuWorldPreviewKingdomPlacementContext context, Vector2Int position)
        {
            var data = context.Data;
            return position.x >= 0 && position.y >= 0 && position.x < data.Width && position.y < data.Height;
        }

        public int Manhattan(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        public RectInt ClampToMap(MenuWorldPreviewKingdomPlacementContext context, RectInt zone)
        {
            var data = context.Data;
            int xMin = Mathf.Clamp(zone.xMin, 0, data.Width);
            int yMin = Mathf.Clamp(zone.yMin, 0, data.Height);
            int xMax = Mathf.Clamp(zone.xMax, 0, data.Width);
            int yMax = Mathf.Clamp(zone.yMax, 0, data.Height);
            return new RectInt(xMin, yMin, Mathf.Max(0, xMax - xMin), Mathf.Max(0, yMax - yMin));
        }
    }
}
