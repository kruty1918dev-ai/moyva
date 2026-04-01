using UnityEngine;

namespace Kruty1918.Moyva.Buildings.API
{
    /// <summary>
    /// Сервіс для будування стін.
    /// Показує точки з'єднання навколо розміщеної стіни і дозволяє тягнути шлях стіни.
    /// </summary>
    public interface IWallService
    {
        /// <summary>Показати 8 точок з'єднання навколо стіни на позиції</summary>
        void ShowConnectionPoints(Vector2Int wallPosition);

        /// <summary>Приховати всі активні точки з'єднання</summary>
        void HideConnectionPoints();

        /// <summary>
        /// Намалювати шлях стіни від startPosition до endPosition
        /// (використовує алгоритм Брезенхема для визначення тайлів між двома точками)
        /// </summary>
        void DrawWallPath(Vector2Int startPosition, Vector2Int endPosition);
    }
}
