using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    public readonly struct GridActionOverlayCell
    {
        public GridActionOverlayCell(
            Vector2Int position,
            GridActionOverlayVisualState visualState,
            string reason = null)
        {
            Position = position;
            VisualState = visualState;
            Reason = reason;
        }

        public Vector2Int Position { get; }
        public GridActionOverlayVisualState VisualState { get; }
        public string Reason { get; }
    }
}
