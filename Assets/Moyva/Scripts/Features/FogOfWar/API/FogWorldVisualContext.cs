using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Описує runtime/editor контекст світу, потрібний visual layer для побудови fog presentation.
    /// Не містить gameplay fog state і не замінює <see cref="IFogOfWarService"/>.
    /// </summary>
    public readonly struct FogWorldVisualContext
    {
        /// <summary>
        /// Створює новий visual context для fog updaters.
        /// </summary>
        /// <param name="width">Ширина карти у клітинках.</param>
        /// <param name="height">Висота карти у клітинках.</param>
        /// <param name="gridTopology">Тип топології сітки.</param>
        /// <param name="projectionMode">Projection mode grid/world presentation.</param>
        /// <param name="renderMode">Спосіб візуального рендеру сітки.</param>
        /// <param name="neighborhoodMode">Тип сусідства для сітки.</param>
        /// <param name="cellSize">Розмір клітинки у world units.</param>
        /// <param name="hasMapWorldBounds">Чи є валідні world bounds для карти.</param>
        /// <param name="mapWorldBounds">Bounds карти у світових координатах.</param>
        /// <param name="heightMap">Безпосередня height map generated світу.</param>
        /// <param name="terrainLevelMap">Дискретна terrain level map, якщо світ її надає.</param>
        public FogWorldVisualContext(
            int width,
            int height,
            GridTopology gridTopology,
            GridProjectionMode projectionMode,
            GridRenderMode renderMode,
            GridNeighborhoodMode neighborhoodMode,
            float cellSize,
            bool hasMapWorldBounds,
            Bounds mapWorldBounds,
            float[,] heightMap,
            int[,] terrainLevelMap)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            GridTopology = gridTopology;
            ProjectionMode = projectionMode;
            RenderMode = renderMode;
            NeighborhoodMode = neighborhoodMode;
            CellSize = cellSize > 0.0001f ? cellSize : 1f;
            HasMapWorldBounds = hasMapWorldBounds;
            MapWorldBounds = mapWorldBounds;
            HeightMap = heightMap;
            TerrainLevelMap = terrainLevelMap;
        }

        /// <summary>
        /// Ширина карти у клітинках.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Висота карти у клітинках.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Тип топології сітки для visual builder-ів.
        /// </summary>
        public GridTopology GridTopology { get; }

        /// <summary>
        /// Projection mode, який використовує generated світ.
        /// </summary>
        public GridProjectionMode ProjectionMode { get; }

        /// <summary>
        /// Render mode, який використовує generated світ.
        /// </summary>
        public GridRenderMode RenderMode { get; }

        /// <summary>
        /// Тип сусідства для дискретної сітки.
        /// </summary>
        public GridNeighborhoodMode NeighborhoodMode { get; }

        /// <summary>
        /// Розмір клітинки у world units.
        /// </summary>
        public float CellSize { get; }

        /// <summary>
        /// Чи є валідні world bounds для карти.
        /// </summary>
        public bool HasMapWorldBounds { get; }

        /// <summary>
        /// Bounds карти у світових координатах.
        /// </summary>
        public Bounds MapWorldBounds { get; }

        /// <summary>
        /// Height map generated світу, якщо вона доступна.
        /// </summary>
        public float[,] HeightMap { get; }

        /// <summary>
        /// Дискретна terrain level map generated світу, якщо вона доступна.
        /// </summary>
        public int[,] TerrainLevelMap { get; }

        /// <summary>
        /// Показує, чи контекст має мінімально валідні розміри.
        /// </summary>
        public bool IsValid => Width > 0 && Height > 0;

        /// <summary>
        /// Повертає копію контексту з оновленим розміром карти.
        /// Зручно для visual updaters, які хочуть перевикористати решту world data.
        /// </summary>
        /// <param name="width">Нова ширина карти.</param>
        /// <param name="height">Нова висота карти.</param>
        /// <returns>Новий екземпляр контексту з оновленим розміром.</returns>
        public FogWorldVisualContext WithSize(int width, int height)
        {
            return new FogWorldVisualContext(
                width,
                height,
                GridTopology,
                ProjectionMode,
                RenderMode,
                NeighborhoodMode,
                CellSize,
                HasMapWorldBounds,
                MapWorldBounds,
                HeightMap,
                TerrainLevelMap);
        }
    }
}
