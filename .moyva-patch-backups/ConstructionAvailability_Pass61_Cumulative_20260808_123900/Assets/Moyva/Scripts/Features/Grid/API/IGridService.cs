using System;
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
    /// Explicit tile identity contract.
    /// Separates "the cell exists" from "the cell has an assigned TileTypeId".
    /// </summary>
    public interface IGridTileIdentityQuery
    {
        bool ContainsCell(Vector2Int position);

        string GetTileTypeId(Vector2Int position);

        bool TryGetTileTypeId(
            Vector2Int position,
            out string tileTypeId);
    }

    public static class GridTileIdentityExtensions
    {
        public static bool ContainsCell(
            this IGridService grid,
            Vector2Int position)
        {
            if (grid == null)
                return false;

            if (grid is IGridTileIdentityQuery identity)
                return identity.ContainsCell(position);

            return position.x >= 0
                && position.y >= 0
                && position.x < grid.GridWidth
                && position.y < grid.GridHeight;
        }

        public static bool TryGetTileTypeId(
            this IGridService grid,
            Vector2Int position,
            out string tileTypeId)
        {
            if (grid is IGridTileIdentityQuery identity)
            {
                return identity.TryGetTileTypeId(
                    position,
                    out tileTypeId);
            }

            tileTypeId = null;
            return grid != null
                && grid.TryGetTileData(
                    position,
                    out tileTypeId)
                && !string.IsNullOrWhiteSpace(tileTypeId);
        }

        public static string GetTileTypeId(
            this IGridService grid,
            Vector2Int position)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            if (grid is IGridTileIdentityQuery identity)
                return identity.GetTileTypeId(position);

            if (!grid.ContainsCell(position))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(position),
                    "Position is out of grid bounds.");
            }

            if (grid.TryGetTileTypeId(
                    position,
                    out string tileTypeId))
            {
                return tileTypeId;
            }

            throw new InvalidOperationException(
                $"Grid cell {position} does not have an assigned TileTypeId.");
        }
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
