using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Керує відображенням точок з'єднання стін та прокладанням шляху стін
    /// між двома позиціями (Bresenham).
    /// </summary>
    public interface IWallConnectionService
    {
        /// <summary>Показує 8 точок з'єднання навколо вказаної позиції стіни.</summary>
        void ShowConnectionPoints(Vector2Int wallPosition);

        /// <summary>Ховає точки з'єднання.</summary>
        void HideConnectionPoints();

        /// <summary>Прокладає шлях стін від <paramref name="from"/> до <paramref name="to"/>.</summary>
        void PlaceWallPath(Vector2Int from, Vector2Int to);
    }
}
