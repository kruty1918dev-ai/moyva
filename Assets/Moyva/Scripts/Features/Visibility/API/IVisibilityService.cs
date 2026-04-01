using UnityEngine;

namespace Kruty1918.Moyva.Visibility.API
{
    /// <summary>
    /// Сервіс туману війни на основі лічильникової сітки.
    /// Кожен тайл зберігає кількість юнітів, які зараз бачать його.
    /// 0 = туман; ≥1 = видимий.
    /// </summary>
    public interface IVisibilityService
    {
        /// <summary>Чи видимий тайл (лічильник > 0)?</summary>
        bool IsVisible(Vector2Int position);

        /// <summary>Поточне значення лічильника видимості для тайлу.</summary>
        int GetVisibilityCount(Vector2Int position);

        /// <summary>
        /// Текстура видимості: кожен піксель — один тайл.
        /// Білий (1,1,1,1) = видимий; чорний (0,0,0,1) = туман.
        /// Використовується шейдером для відображення туману на екрані.
        /// </summary>
        Texture2D GetVisibilityTexture();

        /// <summary>
        /// Повертає копію внутрішньої сітки видимості.
        /// Використовується для збереження гри (save/load).
        /// </summary>
        int[,] GetRawGrid();

        /// <summary>
        /// Завантажує сітку видимості з раніше збереженого стану.
        /// Повністю замінює поточний стан і перебудовує текстуру.
        /// </summary>
        void LoadFromGrid(int[,] grid);
    }
}
