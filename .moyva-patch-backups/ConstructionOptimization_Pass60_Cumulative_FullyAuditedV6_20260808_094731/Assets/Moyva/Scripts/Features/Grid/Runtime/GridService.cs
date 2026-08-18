using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    /// <summary>
    /// Внутрішня реалізація сервісу сітки.
    /// Підтримує читання/запис тайлів (<see cref="IGridService"/>)
    /// та зміну розміру карти в runtime (<see cref="IGridResizeService"/>).
    /// </summary>
    internal sealed class GridService : IGridService, IGridResizeService
    {
        /// <summary>
        /// Двовимірний буфер TileTypeId за координатами [x, y].
        /// </summary>
        private string[,] _grid;

        /// <summary>
        /// Поточна ширина сітки.
        /// </summary>
        public int GridWidth { get; private set; }

        /// <summary>
        /// Поточна висота сітки.
        /// </summary>
        public int GridHeight { get; private set; }

        /// <summary>
        /// Створює сервіс сітки з безпечними мінімальними розмірами.
        /// </summary>
        /// <param name="gridWidth">Бажана ширина.</param>
        /// <param name="gridHeight">Бажана висота.</param>
        public GridService(int gridWidth, int gridHeight)
        {
            // 1) Захищаємося від невалідних розмірів через кламп до мінімуму 1.
            GridWidth = Mathf.Max(1, gridWidth);
            GridHeight = Mathf.Max(1, gridHeight);

            // 2) Ініціалізуємо внутрішній буфер для зберігання TileTypeId.
            _grid = new string[GridWidth, GridHeight];
        }

        /// <summary>
        /// Змінює розміри сітки, зберігаючи дані в межах перетину старого і нового розмірів.
        /// </summary>
        /// <param name="width">Нова ширина.</param>
        /// <param name="height">Нова висота.</param>
        public void Resize(int width, int height)
        {
            // 1) Нормалізуємо вхідні значення, щоб уникнути нульових/від'ємних розмірів.
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);

            // 2) Якщо розміри не змінилися, уникаємо зайвого перевиділення масиву.
            if (safeWidth == GridWidth && safeHeight == GridHeight)
                return;

            // 3) Створюємо новий буфер потрібного розміру.
            var resized = new string[safeWidth, safeHeight];

            // 4) Визначаємо межі копіювання (тільки перетин старої/нової області).
            int copyWidth = Mathf.Min(GridWidth, safeWidth);
            int copyHeight = Mathf.Min(GridHeight, safeHeight);

            // 5) Копіюємо дані комірок з попереднього буфера в новий.
            for (int x = 0; x < copyWidth; x++)
                for (int y = 0; y < copyHeight; y++)
                    resized[x, y] = _grid[x, y];

            // 6) Атомарно замінюємо буфер і синхронізуємо публічні розміри.
            _grid = resized;
            GridWidth = safeWidth;
            GridHeight = safeHeight;
        }

        /// <summary>
        /// Повертає TileTypeId за координатою або кидає виняток, якщо координата невалідна.
        /// </summary>
        /// <param name="position">Координата комірки.</param>
        /// <returns>TileTypeId у комірці.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Коли координата поза межами сітки.</exception>
        public string GetTileData(Vector2Int position)
        {
            // 1) Перевіряємо, що позиція в межах сітки.
            if (IsValidPosition(position))
                // 2) Повертаємо значення з внутрішнього буфера.
                return _grid[position.x, position.y];

            // 3) Поза межами — сигналізуємо про помилку виклику.
            throw new System.ArgumentOutOfRangeException(nameof(position), "Position is out of grid bounds.");
        }

        /// <summary>
        /// Записує TileTypeId у комірку або кидає виняток для невалідної координати.
        /// </summary>
        /// <param name="position">Координата комірки.</param>
        /// <param name="tileTypeId">TileTypeId для запису.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Коли координата поза межами сітки.</exception>
        public void SetTileData(Vector2Int position, string tileTypeId)
        {
            // 1) Гарантуємо валідність позиції перед записом.
            if (!IsValidPosition(position))
                throw new System.ArgumentOutOfRangeException(nameof(position), "Position is out of grid bounds.");

            // 2) Записуємо значення в буфер.
            _grid[position.x, position.y] = tileTypeId;
        }

        /// <summary>
        /// Перевіряє, що координата належить поточним межам сітки.
        /// </summary>
        /// <param name="position">Координата комірки.</param>
        /// <returns><see langword="true"/>, якщо позиція валідна.</returns>
        private bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < GridWidth && position.y >= 0 && position.y < GridHeight;
        }

        /// <summary>
        /// Безпечне читання TileTypeId без винятку.
        /// </summary>
        /// <param name="position">Координата комірки.</param>
        /// <param name="tileTypeId">Зчитаний TileTypeId або значення за замовчуванням.</param>
        /// <returns><see langword="true"/>, якщо читання успішне.</returns>
        public bool TryGetTileData(Vector2Int position, out string tileTypeId)
        {
            // 1) Якщо позиція валідна — повертаємо значення з буфера.
            if (IsValidPosition(position))
            {
                tileTypeId = _grid[position.x, position.y];
                return true;
            }

            // 2) Для невалідної позиції повертаємо default і false.
            tileTypeId = default;
            return false;
        }
    }
}
