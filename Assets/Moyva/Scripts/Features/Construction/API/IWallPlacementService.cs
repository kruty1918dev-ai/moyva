using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IWallPlacementService
    {
        /// <summary>Показати 8 ручок-кнопок навколо вже розміщеної стіни/воріт.</summary>
        void ShowWallHandles(Vector2Int wallPosition);

        /// <summary>
        /// Drag від стартової позиції до поточної позиції дотику в світових координатах.
        /// Будує path за алгоритмом Bresenham і намагається додати preview на кожному тайлі.
        /// </summary>
        void DragWall(Vector2Int startPosition, Vector2 touchWorldPosition);

        /// <summary>Повертає точки path між start і end (включно).</summary>
        IReadOnlyList<Vector2Int> BuildPath(Vector2Int startPosition, Vector2Int endPosition);

        /// <summary>Чи належить id до стінної колекції (стіна або ворота).</summary>
        bool IsWallOrGate(string buildingId);

        /// <summary>Чи є id саме стіною.</summary>
        bool IsWall(string buildingId);

        /// <summary>Чи є id саме воротами.</summary>
        bool IsGate(string buildingId);

        /// <summary>Чи можна замінити стіну на цій позиції воротами gateBuildingId.</summary>
        bool CanReplaceWallWithGate(Vector2Int position, string gateBuildingId, out string replacedWallId);

        /// <summary>
        /// Підібрати prefab і rotation для already placed wall/gate сегмента з урахуванням сусідів.
        /// Для не-wall елементів повертає false.
        /// </summary>
        bool TryResolvePlacedVisual(Vector2Int position, string occupantId, out GameObject prefab, out Quaternion rotation);

        /// <summary>Завершити drag. Ручки зникають, pending-розміщення залишаються.</summary>
        void EndDrag();
    }
}
