using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Містить результат обчислення видимості для окремої клітинки.
    /// </summary>
    public readonly struct FogTileVisibility
    {
        /// <summary>
        /// Створює запис про видимість клітинки.
        /// </summary>
        /// <param name="tile">Клітинка, для якої обчислено видимість.</param>
        /// <param name="visibility">Нормалізована величина видимості у діапазоні [0..1].</param>
        public FogTileVisibility(Vector2Int tile, float visibility)
        {
            Tile = tile;
            Visibility = Mathf.Clamp01(visibility);
        }

        /// <summary>
        /// Клітинка, для якої обчислена видимість.
        /// </summary>
        public Vector2Int Tile { get; }

        /// <summary>
        /// Нормалізована величина видимості у діапазоні [0..1].
        /// </summary>
        public float Visibility { get; }

        /// <summary>
        /// Перевіряє, чи достатня видимість клітинки для вказаного порогу.
        /// </summary>
        /// <param name="threshold">Поріг видимості у діапазоні [0..1].</param>
        /// <returns><see langword="true"/>, якщо клітинка проходить поріг.</returns>
        public bool IsVisible(float threshold) => Visibility >= Mathf.Clamp01(threshold);
    }

    /// <summary>
    /// Ізолює алгоритм обчислення видимості клітинок.
    /// Реалізацію може викликати <see cref="IFogOfWarService"/>, але інші системи не повинні залежати
    /// від конкретного LOS-алгоритму чи height-aware реалізації.
    /// </summary>
    public interface IFogVisibilityResolver
    {
        /// <summary>
        /// Передає height map, який використовуватиметься під час наступних visibility-розрахунків.
        /// </summary>
        /// <param name="heightMap">Мапа висот generated світу у клітинках.</param>
        void SetHeightMap(float[,] heightMap);

        /// <summary>
        /// Повертає лише клітинки, які проходять visibility threshold.
        /// Це зручний wrapper над повним обчисленням видимості.
        /// </summary>
        /// <param name="origin">Клітинка спостерігача.</param>
        /// <param name="visionRange">Базовий радіус огляду.</param>
        /// <param name="mapWidth">Ширина карти.</param>
        /// <param name="mapHeight">Висота карти.</param>
        /// <param name="observerModifiers">Модифікатори огляду спостерігача.</param>
        /// <returns>Список клітинок, які вважаються видимими.</returns>
        IReadOnlyList<Vector2Int> ComputeVisibleTiles(
            Vector2Int origin, int visionRange, int mapWidth, int mapHeight, FogVisionModifiers observerModifiers = default);

        /// <summary>
        /// Повертає детальний visibility score для всіх досяжних клітинок у радіусі пошуку.
        /// </summary>
        /// <param name="origin">Клітинка спостерігача.</param>
        /// <param name="visionRange">Базовий радіус огляду.</param>
        /// <param name="mapWidth">Ширина карти.</param>
        /// <param name="mapHeight">Висота карти.</param>
        /// <param name="observerModifiers">Модифікатори огляду спостерігача.</param>
        /// <returns>Список клітинок разом із нормалізованим рівнем видимості.</returns>
        IReadOnlyList<FogTileVisibility> ComputeVisibility(
            Vector2Int origin, int visionRange, int mapWidth, int mapHeight, FogVisionModifiers observerModifiers = default);
    }
}
