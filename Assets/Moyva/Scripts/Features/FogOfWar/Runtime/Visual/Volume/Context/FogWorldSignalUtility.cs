using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Shared helper для читання map bounds і базового розміру карти з <see cref="WorldGeneratedDataSignal"/>.
    /// Не містить gameplay fog state і може перевикористовуватись іншими fog runtime helpers.
    /// </summary>
    internal static class FogWorldSignalUtility
    {
        /// <summary>
        /// Спробувати дістати валідні map world bounds із сигналу генерації світу.
        /// </summary>
        /// <param name="signal">Сигнал генерації світу.</param>
        /// <param name="bounds">Результуючі world bounds карти.</param>
        /// <returns><see langword="true"/>, якщо bounds присутні й валідні.</returns>
        public static bool TryResolveMapWorldBounds(WorldGeneratedDataSignal signal, out Bounds bounds)
        {
            bounds = default;
            if (!signal.HasMapWorldBounds
                || !IsFinite(signal.MapWorldBoundsCenter)
                || !IsFinite(signal.MapWorldBoundsSize))
            {
                return false;
            }

            Vector3 size = new Vector3(
                Mathf.Abs(signal.MapWorldBoundsSize.x),
                Mathf.Abs(signal.MapWorldBoundsSize.y),
                Mathf.Abs(signal.MapWorldBoundsSize.z));
            if (size.x <= 0.0001f || size.z <= 0.0001f)
                return false;

            bounds = new Bounds(signal.MapWorldBoundsCenter, size);
            return true;
        }

        /// <summary>
        /// Повертає найбільш надійний розмір базової карти, комбінуючи дані з різних generated map sources.
        /// </summary>
        /// <param name="signal">Сигнал генерації світу.</param>
        /// <returns>Безпечний розмір карти у клітинках.</returns>
        public static Vector2Int ResolveBaseMapSize(WorldGeneratedDataSignal signal)
        {
            int width = Mathf.Max(0, signal.Width);
            int height = Mathf.Max(0, signal.Height);

            ApplyBaseMapSize(signal.TileMap, ref width, ref height);
            ApplyBaseMapSize(signal.ObjectMap, ref width, ref height);
            ApplyBaseMapSize(signal.HeightMap, ref width, ref height);
            ApplyBaseMapSize(signal.TerrainLevelMap, ref width, ref height);

            return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
        }

        private static void ApplyBaseMapSize<T>(T[,] map, ref int width, ref int height)
        {
            if (map == null)
                return;

            int mapWidth = map.GetLength(0);
            int mapHeight = map.GetLength(1);
            if (mapWidth <= 0 || mapHeight <= 0)
                return;

            width = width > 0 ? Mathf.Min(width, mapWidth) : mapWidth;
            height = height > 0 ? Mathf.Min(height, mapHeight) : mapHeight;
        }

        private static bool IsFinite(Vector3 value)
            => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
