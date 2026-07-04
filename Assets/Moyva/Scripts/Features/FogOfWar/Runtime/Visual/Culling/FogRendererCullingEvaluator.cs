using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Допоміжний оцінювач, що визначає чи потрібно рендерити об'єкт
    /// залежно від покриття клітин туманом.
    /// </summary>
    internal static class FogRendererCullingEvaluator
    {
        private const float BoundsEdgeEpsilon = 0.001f;

        /// <summary>
        /// Перевіряє, чи варто рендерити об'єкт з огляду на стан туману в області його меж.
        /// Повертає true якщо хоча б одна клітина в області не є невідкритою (Unexplored).
        /// </summary>
        /// <param name="worldBounds">Світові межі об'єкта.</param>
        /// <param name="fogService">Gameplay fog service.</param>
        /// <param name="gridService">Grid service для перетворення bounds у діапазон клітин.</param>
        /// <param name="boundsPaddingCells">Додатковий padding у клітинках.</param>
        /// <param name="gridProjection">Необов'язковий projected grid adapter.</param>
        /// <returns><see langword="true"/>, якщо об'єкт слід рендерити.</returns>
        public static bool ShouldRender(Bounds worldBounds, IFogStateReader fogService, IGridService gridService, float boundsPaddingCells, IGridProjection gridProjection = null)
        {
            if (fogService == null || gridService == null)
                return true;

            if (!TryGetCoveredTileRange(worldBounds, gridService, boundsPaddingCells, out var min, out var max, gridProjection))
                return true;

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    if (fogService.GetFogState(new Vector2Int(x, y)) != FogStateType.Unexplored)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Обчислює діапазон клітин, що покриває передані світові межі, з урахуванням паддінгу.
        /// Повертає false якщо область повністю виходить за межі сітки.
        /// </summary>
        /// <param name="worldBounds">Світові межі об'єкта.</param>
        /// <param name="gridService">Grid service карти.</param>
        /// <param name="boundsPaddingCells">Додатковий padding у клітинках.</param>
        /// <param name="min">Мінімальна клітинка покритого діапазону.</param>
        /// <param name="max">Максимальна клітинка покритого діапазону.</param>
        /// <param name="gridProjection">Необов'язковий projected grid adapter.</param>
        /// <returns><see langword="true"/>, якщо bounds перетинають карту і мають валідний tile range.</returns>
        internal static bool TryGetCoveredTileRange(
            Bounds worldBounds,
            IGridService gridService,
            float boundsPaddingCells,
            out Vector2Int min,
            out Vector2Int max,
            IGridProjection gridProjection = null)
        {
            min = default;
            max = default;

            if (gridService == null || gridService.GridWidth <= 0 || gridService.GridHeight <= 0)
                return false;

            if (gridProjection != null)
                return TryGetProjectedCoveredTileRange(worldBounds, gridService, boundsPaddingCells, gridProjection, out min, out max);

            float padding = Mathf.Max(0f, boundsPaddingCells);
            int rawMinX = Mathf.FloorToInt(worldBounds.min.x + 0.5f - padding);
            int rawMinY = Mathf.FloorToInt(worldBounds.min.y + 0.5f - padding);
            int rawMaxX = Mathf.FloorToInt(worldBounds.max.x + 0.5f - BoundsEdgeEpsilon + padding);
            int rawMaxY = Mathf.FloorToInt(worldBounds.max.y + 0.5f - BoundsEdgeEpsilon + padding);

            if (rawMaxX < 0 || rawMaxY < 0 || rawMinX >= gridService.GridWidth || rawMinY >= gridService.GridHeight)
                return false;

            min = new Vector2Int(
                Mathf.Clamp(rawMinX, 0, gridService.GridWidth - 1),
                Mathf.Clamp(rawMinY, 0, gridService.GridHeight - 1));

            max = new Vector2Int(
                Mathf.Clamp(rawMaxX, 0, gridService.GridWidth - 1),
                Mathf.Clamp(rawMaxY, 0, gridService.GridHeight - 1));

            return min.x <= max.x && min.y <= max.y;
        }

        private static bool TryGetProjectedCoveredTileRange(
            Bounds worldBounds,
            IGridService gridService,
            float boundsPaddingCells,
            IGridProjection gridProjection,
            out Vector2Int min,
            out Vector2Int max)
        {
            min = default;
            max = default;

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            Vector3 boundsMin = worldBounds.min;
            Vector3 boundsMax = worldBounds.max;
            for (int xIndex = 0; xIndex < 2; xIndex++)
            for (int yIndex = 0; yIndex < 2; yIndex++)
            for (int zIndex = 0; zIndex < 2; zIndex++)
            {
                var corner = new Vector3(
                    xIndex == 0 ? boundsMin.x : boundsMax.x,
                    yIndex == 0 ? boundsMin.y : boundsMax.y,
                    zIndex == 0 ? boundsMin.z : boundsMax.z);
                Vector2Int grid = gridProjection.WorldToGrid(corner);
                minX = Mathf.Min(minX, grid.x);
                minY = Mathf.Min(minY, grid.y);
                maxX = Mathf.Max(maxX, grid.x);
                maxY = Mathf.Max(maxY, grid.y);
            }

            int padding = Mathf.CeilToInt(Mathf.Max(0f, boundsPaddingCells));
            int rawMinX = minX - padding;
            int rawMinY = minY - padding;
            int rawMaxX = maxX + padding;
            int rawMaxY = maxY + padding;

            if (rawMaxX < 0 || rawMaxY < 0 || rawMinX >= gridService.GridWidth || rawMinY >= gridService.GridHeight)
                return false;

            min = new Vector2Int(
                Mathf.Clamp(rawMinX, 0, gridService.GridWidth - 1),
                Mathf.Clamp(rawMinY, 0, gridService.GridHeight - 1));

            max = new Vector2Int(
                Mathf.Clamp(rawMaxX, 0, gridService.GridWidth - 1),
                Mathf.Clamp(rawMaxY, 0, gridService.GridHeight - 1));

            return min.x <= max.x && min.y <= max.y;
        }
    }
}
