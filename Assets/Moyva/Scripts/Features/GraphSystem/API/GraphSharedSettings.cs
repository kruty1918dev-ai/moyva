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
        [Sirenix.OdinInspector.BoxGroup("Map Size")]
        [Tooltip("Authoritative ширина мапи у тайлах. Коли > 0, це значення використовується і Graph Preview, і gameplay generation. 0 = дозволити runtime/menu визначати розмір.")]
        [Min(0)]
        [SerializeField] private int _mapWidth;

        [Tooltip("Authoritative висота мапи у тайлах. Коли > 0, це значення використовується і Graph Preview, і gameplay generation. 0 = дозволити runtime/menu визначати розмір.")]
        [Sirenix.OdinInspector.BoxGroup("Map Size")]
        [Min(0)]
        [SerializeField] private int _mapHeight;

        [Header("Grid Mode")]
        [Sirenix.OdinInspector.BoxGroup("Grid Mode")]
        [SerializeField] private GridTopology _gridTopology = GridTopology.Orthogonal;
        [Sirenix.OdinInspector.BoxGroup("Grid Mode")]
        [SerializeField] private GridProjectionMode _projectionMode = GridProjectionMode.Orthographic3D;
        [Sirenix.OdinInspector.BoxGroup("Grid Mode")]
        [SerializeField] private GridRenderMode _renderMode = GridRenderMode.Mesh3D;
        [Sirenix.OdinInspector.BoxGroup("Grid Mode")]
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

        /// <summary>
        /// Задає єдиний authoritative розмір карти для Graph Preview
        /// та gameplay generation.
        /// </summary>
        public void SetMapSize(int width, int height)
        {
            _mapWidth = Mathf.Max(4, width);
            _mapHeight = Mathf.Max(4, height);
        }

        /// <summary>
        /// Повертає керування розміром runtime/menu launch context.
        /// </summary>
        public void ClearMapSize()
        {
            _mapWidth = 0;
            _mapHeight = 0;
        }

        public GridNeighborhoodMode ResolveNeighborhoodMode()
        {
            if (_neighborhoodMode != GridNeighborhoodMode.Auto)
                return _neighborhoodMode;

            if (_gridTopology == GridTopology.HexAxial)
            {
                return GridNeighborhoodMode.HexAxial6;
            }

            return _projectionMode == GridProjectionMode.Isometric3DPreview
                ? GridNeighborhoodMode.VonNeumann4
                : GridNeighborhoodMode.Moore8;
        }
    }
}
