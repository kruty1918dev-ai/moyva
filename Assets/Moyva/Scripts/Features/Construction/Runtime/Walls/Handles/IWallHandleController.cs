using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IWallHandleController
    {
        void Show(Vector2Int wallPosition);
        void TrackDragStart(Vector2Int startPosition);
        void EndDrag();
    }
}
