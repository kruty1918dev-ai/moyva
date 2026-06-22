using UnityEngine;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Grid.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Контейнер згенерованих даних світу: карти біомів, об'єктів, висот та будівель.
    /// Підтримує глибоке клонування для безпечної передачі між підсистемами.
    /// </summary>
    internal sealed class GeneratedWorldData
    {
        public int Width;
        public int Height;
        public GridTopology GridTopology = GridTopology.Orthogonal;
        public GridProjectionMode ProjectionMode = GridProjectionMode.Orthographic3D;
        public GridRenderMode RenderMode = GridRenderMode.Mesh3D;
        public GridNeighborhoodMode NeighborhoodMode = GridNeighborhoodMode.Moore8;
        public string[,] BiomeMap;
        public string[,] ObjectMap;
        public float[,] HeightMap;
        public int[,] TerrainLevelMap;
        public string[,] BuildingMap;
        public string WorldName;
        public int Seed;
        public int Size;
        public int MapType;
        public int Difficulty;
        public SpawnPositionAssignment[] SpawnPositions;

        /// <summary>
        /// індекс шару - це його порядок у ордер лейері шарів тобто 0 - це шар який є в самомум низу чим індекс вищий то тим шар рендеру вищий іншими словами 1 буде перекравати 0 
        /// </summary>
        public WorldLayerData[] LayerData;

        public GeneratedWorldData Clone()
        {
            return new GeneratedWorldData
            {
                Width = Width,
                Height = Height,
                GridTopology = GridTopology,
                ProjectionMode = ProjectionMode,
                RenderMode = RenderMode,
                NeighborhoodMode = NeighborhoodMode,
                BiomeMap = MapArrayUtils.CloneStringMap(BiomeMap),
                ObjectMap = MapArrayUtils.CloneStringMap(ObjectMap),
                HeightMap = MapArrayUtils.CloneFloatMap(HeightMap),
                TerrainLevelMap = MapArrayUtils.CloneIntMap(TerrainLevelMap),
                BuildingMap = MapArrayUtils.CloneStringMap(BuildingMap),
                WorldName = WorldName,
                Seed = Seed,
                Size = Size,
                MapType = MapType,
                Difficulty = Difficulty,
                SpawnPositions = SpawnPositions != null ? (SpawnPositionAssignment[])SpawnPositions.Clone() : null,
                LayerData = LayerData != null ? (WorldLayerData[])LayerData.Clone() : null,
            };
        }
    }

    internal static class GeneratedWorldBoundsUtility
    {
        private const float MinCellSize = 0.0001f;

        public static bool TryCreateTileWorldBounds(
            Transform root,
            int width,
            int height,
            float cellSize,
            out Bounds bounds)
        {
            bounds = default;

            if (root == null || width <= 0 || height <= 0 || cellSize <= MinCellSize)
                return false;

            float halfCell = cellSize * 0.5f;
            Vector3 localMin = new Vector3(-halfCell, 0f, -halfCell);
            Vector3 localMax = new Vector3(
                (width - 1) * cellSize + halfCell,
                1f,
                (height - 1) * cellSize + halfCell);

            Vector3 worldMin = root.TransformPoint(localMin);
            Vector3 worldMax = worldMin;
            Encapsulate(root.TransformPoint(new Vector3(localMax.x, localMin.y, localMin.z)), ref worldMin, ref worldMax);
            Encapsulate(root.TransformPoint(new Vector3(localMin.x, localMax.y, localMin.z)), ref worldMin, ref worldMax);
            Encapsulate(root.TransformPoint(new Vector3(localMin.x, localMin.y, localMax.z)), ref worldMin, ref worldMax);
            Encapsulate(root.TransformPoint(new Vector3(localMax.x, localMax.y, localMin.z)), ref worldMin, ref worldMax);
            Encapsulate(root.TransformPoint(new Vector3(localMax.x, localMin.y, localMax.z)), ref worldMin, ref worldMax);
            Encapsulate(root.TransformPoint(new Vector3(localMin.x, localMax.y, localMax.z)), ref worldMin, ref worldMax);
            Encapsulate(root.TransformPoint(localMax), ref worldMin, ref worldMax);

            bounds = new Bounds((worldMin + worldMax) * 0.5f, worldMax - worldMin);
            return IsFinite(bounds.center) && IsFinite(bounds.size);
        }

        private static void Encapsulate(Vector3 point, ref Vector3 min, ref Vector3 max)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        private static bool IsFinite(Vector3 value)
            => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    internal struct WorldLayerData
    {
        /// <summary>
        /// Ідентифікатор тайлу який вертається при запиті ширу. Яким тайлом то є 
        /// </summary>
        public string LayerTileID;

        /// <summary>
        /// зегенрована текстура розміром в усю мапу
        /// </summary>
        public Texture2D TileTexture;

        /// <summary>
        /// Пряме посилання на шейдер шару (включно з Shader Graph).
        /// Якщо задано, має пріоритет над LayerShaderName.
        /// </summary>
        public Shader LayerShader;

        /// <summary>
        /// Шлях шейдера для рендеру цього шару (наприклад "Sprites/Default").
        /// Якщо порожнє, використовується дефолтний матеріал SpriteRenderer.
        /// </summary>
        public string LayerShaderName;

        /// <summary>
        /// Ім'я Sorting Layer для SpriteRenderer шару. Порожнє = "Default".
        /// </summary>
        public string SortingLayerName;

        /// <summary>
        /// Sorting Order шару у вказаному Sorting Layer.
        /// </summary>
        public int SortingOrder;
    }
}
