using UnityEngine;
using Kruty1918.Moyva.Signals;

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
        public string[,] BiomeMap;
        public string[,] ObjectMap;
        public float[,] HeightMap;
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
                BiomeMap = MapArrayUtils.CloneStringMap(BiomeMap),
                ObjectMap = MapArrayUtils.CloneStringMap(ObjectMap),
                HeightMap = MapArrayUtils.CloneFloatMap(HeightMap),
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