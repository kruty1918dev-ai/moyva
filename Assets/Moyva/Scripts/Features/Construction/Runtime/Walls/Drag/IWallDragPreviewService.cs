using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IWallDragPreviewService
    {
        void PreviewDrag(Vector2Int startPosition, Vector2 touchWorldPosition);
    }
}
