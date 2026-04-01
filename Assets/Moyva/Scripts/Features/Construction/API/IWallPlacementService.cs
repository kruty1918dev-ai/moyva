using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IWallPlacementService
    {
        /// <summary>
        /// Показати 8 ручок-кнопок навколо вже розміщеної стіни.
        /// Надсилає ShowWallHandlesSignal — UI підписується і малює кнопки.
        /// </summary>
        void ShowWallHandles(Vector2Int wallPosition);

        /// <summary>
        /// Drag від стартової позиції до поточної позиції дотику в світових координатах.
        /// Будує лінію стін за алгоритмом Bresenham між startPosition і
        /// відповідним grid-тайлом touchWorldPosition.
        /// Кожен тайл на шляху передається до IConstructionService.TryPreviewAt().
        /// </summary>
        void DragWall(Vector2Int startPosition, Vector2 touchWorldPosition);

        /// <summary>
        /// Завершити drag. Ручки зникають, pending-розміщення залишаються.
        /// </summary>
        void EndDrag();
    }
}
