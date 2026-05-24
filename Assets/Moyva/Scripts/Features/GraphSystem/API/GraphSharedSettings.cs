using System;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    /// <summary>
    /// Спільні налаштування графа — пряма частина GraphAsset, не є нодом.
    /// Зберігають дані, які мають бути доступні всім нодам під час виконання
    /// без необхідності розміщувати додатковий вузол у граф.
    /// Реєструються в NodeContext як сервіс: context.TryGetService&lt;GraphSharedSettings&gt;.
    /// </summary>
    [Serializable]
    public sealed class GraphSharedSettings
    {
        [Header("Map Size")]
        [Tooltip("Ширина мапи (у тайлах). 0 = брати розмір із GridService (зовнішнє джерело).")]
        [SerializeField] private int _mapWidth;

        [Tooltip("Висота мапи (у тайлах). 0 = брати розмір із GridService (зовнішнє джерело).")]
        [SerializeField] private int _mapHeight;

        [Header("Grid Mode")]
        [SerializeField] private GridTopology _gridTopology = GridTopology.Orthogonal;
        [SerializeField] private GridProjectionMode _projectionMode = GridProjectionMode.Orthographic2D;
        [SerializeField] private GridRenderMode _renderMode = GridRenderMode.Sprite2D;
        [SerializeField] private GridNeighborhoodMode _neighborhoodMode = GridNeighborhoodMode.Auto;

        public int MapWidth => _mapWidth;
        public int MapHeight => _mapHeight;
        public Vector2Int MapSize => new Vector2Int(_mapWidth, _mapHeight);
        public GridTopology GridTopology => _gridTopology;
        public GridProjectionMode ProjectionMode => _projectionMode;
        public GridRenderMode RenderMode => _renderMode;
        public GridNeighborhoodMode NeighborhoodMode => _neighborhoodMode;

        /// <summary>
        /// true — якщо обидва виміри задані в налаштуваннях графа (>0).
        /// Якщо true, GraphBasedMapDataGenerator ігнорує зовнішні width/height і
        /// використовує ці значення.
        /// </summary>
        public bool HasMapSize => _mapWidth > 0 && _mapHeight > 0;

        public GridNeighborhoodMode ResolveNeighborhoodMode()
        {
            if (_neighborhoodMode != GridNeighborhoodMode.Auto)
                return _neighborhoodMode;

            if (_gridTopology == GridTopology.HexAxial
                || _projectionMode == GridProjectionMode.HexPointy2D
                || _projectionMode == GridProjectionMode.HexFlat2D)
            {
                return GridNeighborhoodMode.HexAxial6;
            }

            return _projectionMode == GridProjectionMode.Isometric2D
                || _projectionMode == GridProjectionMode.Isometric3DPreview
                    ? GridNeighborhoodMode.VonNeumann4
                    : GridNeighborhoodMode.Moore8;
        }
    }
}
