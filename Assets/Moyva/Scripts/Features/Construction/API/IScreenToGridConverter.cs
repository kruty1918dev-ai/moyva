using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IScreenToGridConverter
    {
        /// <summary>
        /// Перетворити позицію на екрані (пікселі) в координати тайлу на сітці.
        /// Використовує ін'єктовану ортографічну камеру (Z ігнорується).
        /// </summary>
        Vector2Int ScreenToGrid(Vector2 screenPosition);

        /// <summary>
        /// Перетворити вже отриману world-позицію в координати тайлу.
        /// Зручно при drag, коли world-координата вже є.
        /// </summary>
        Vector2Int WorldToGrid(Vector2 worldPosition);
    }
}
