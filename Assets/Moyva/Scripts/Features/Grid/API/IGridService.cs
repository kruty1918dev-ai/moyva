using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Контракт доступу до даних тайлової сітки світу.
    /// Залежить від координат <see cref="Vector2Int"/> і повертає/змінює TileTypeId у комірці.
    /// </summary>
    public interface IGridService
    {
        /// <summary>
        /// Повертає TileTypeId у вказаній позиції.
        /// </summary>
        /// <param name="position">Координата комірки у сітці.</param>
        /// <returns>Ідентифікатор типу тайла.</returns>
        public string GetTileData(Vector2Int position);

        /// <summary>
        /// Безпечне читання TileTypeId без винятку поза межами сітки.
        /// </summary>
        /// <param name="position">Координата комірки у сітці.</param>
        /// <param name="tileTypeId">Знайдений TileTypeId, якщо позиція валідна.</param>
        /// <returns><see langword="true"/>, якщо позиція валідна; інакше <see langword="false"/>.</returns>
        public bool TryGetTileData(Vector2Int position, out string tileTypeId);

        /// <summary>
        /// Записує TileTypeId у вказану позицію сітки.
        /// </summary>
        /// <param name="position">Координата комірки у сітці.</param>
        /// <param name="tileTypeId">Ідентифікатор типу тайла для запису.</param>
        public void SetTileData(Vector2Int position, string tileTypeId);

        /// <summary>
        /// Поточна ширина сітки у тайлах.
        /// </summary>
        public int GridWidth { get; }

        /// <summary>
        /// Поточна висота сітки у тайлах.
        /// </summary>
        public int GridHeight { get; }
    }

    /// <summary>
    /// Контракт зміни розмірів сітки під час виконання.
    /// </summary>
    public interface IGridResizeService
    {
        /// <summary>
        /// Змінює розмір сітки зі збереженням перетину існуючих даних.
        /// </summary>
        /// <param name="width">Нова ширина у тайлах.</param>
        /// <param name="height">Нова висота у тайлах.</param>
        public void Resize(int width, int height);
    }
}
