using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Forest Cluster", "Features", "Формує лісові масиви за шумом щільності, висотою та необов'язковою маскою. Дозволяє одночасно змінювати біомні тайли під ліс і розставляти дерева як об'єкти.")]
    public sealed class ForestClusterNode : NodeBase
    {
        [Header("Noise")]
        [Tooltip("Налаштування шуму щільності лісу. Визначає, де будуть ядра лісових масивів, а де — розриви та рідколісся.")]
        [SerializeField] private DataNoiseSettings _densityNoise;

        [Header("Forest Settings")]
        [Tooltip("Мінімальна щільність шуму, після якої клітинка вважається придатною для лісу. Підвищення параметра дає рідші, але виразніші кластери.")]
        [SerializeField, Range(0f, 1f)] private float _densityThreshold = 0.45f;
        [Tooltip("Мінімальна висота для росту лісу. Нижчі клітинки будуть виключені зі спавну, навіть якщо шум щільності високий.")]
        [SerializeField, Range(0f, 1f)] private float _minHeight = 0.25f;
        [Tooltip("Максимальна висота для лісу. Дозволяє уникати появи густих дерев у високогір'ї або на скельних вершинах.")]
        [SerializeField, Range(0f, 1f)] private float _maxHeight = 0.65f;
        [Tooltip("Список ID дерев, які може поставити нода в клітинках лісу. Випадковий вибір із цього набору створює різноманітніший вигляд масиву.")]
        [SerializeField, MapObjectId] private string[] _treeObjects = { "tree-oak", "tree-pine", "tree-birch" };
        [Tooltip("Tile ID для густого лісу. Використовується в найщільніших ділянках, щоб карта поверхні відповідала насиченому рослинністю ландшафту.")]
        [SerializeField, TileId] private string _denseForestTile = "forest-dense";
        [Tooltip("Tile ID для рідкого лісу або узлісся. Дозволяє створити плавний перехід від відкритої трави до густого масиву.")]
        [SerializeField, TileId] private string _sparseForestTile = "forest-sparse";
        [Tooltip("Поріг, вище якого ліс вважається густим. Нижчі значення дають більше dense-ділянок, вищі залишають більше рідколісся.")]
        [SerializeField, Range(0f, 1f)] private float _denseThreshold = 0.65f;

        [Header("Tree Clustering")]
        [Tooltip("Мінімальна кількість сусідніх лісових клітин (8-напрямків), щоб у клітині взагалі можна було ставити дерево. Чим вище значення, тим компактніші кластери.")]
        [SerializeField, Range(0, 8)] private int _minForestNeighborsForTree = 3;

        [Tooltip("Степінь підсилення ймовірності дерева у щільних ділянках. >1 концентрує дерева у ядрах кластерів, <1 робить покриття рівномірнішим.")]
        [SerializeField, Range(0.5f, 4f)] private float _treeDensityExponent = 1.8f;

        [Header("Fallback Density Noise")]
        [SerializeField, Min(0.0001f)] private float _fallbackDensityScale = 0.08f;
        [SerializeField, Range(1, 8)] private int _fallbackDensityOctaves = 1;
        [SerializeField, Min(1f)] private float _fallbackDensityLacunarity = 2f;
        [SerializeField, Range(0.01f, 1f)] private float _fallbackDensityPersistence = 0.5f;

        public override string Title => "Forest Cluster";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<bool[,]>("Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap"),
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            var biomeMap = inputs[1] as string[,];
            if (heightMap == null || biomeMap == null)
                return NodeOutput.Error("HeightMap and BiomeMap inputs are required.");

            var mask = inputs[2] as bool[,];

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);

            // Generate forest density noise
            float[,] densityMap;
            if (_densityNoise != null)
            {
                var noiseProvider = context.GetService<INoiseProvider>();
                densityMap = noiseProvider.GenerateNoiseMap(_densityNoise, w, h);
            }
            else
            {
                densityMap = GenerateSimpleNoise(
                    w,
                    h,
                    context.Seed,
                    _fallbackDensityScale,
                    _fallbackDensityOctaves,
                    _fallbackDensityLacunarity,
                    _fallbackDensityPersistence);
            }

            var resultBiome = (string[,])biomeMap.Clone();
            var objectMap = new string[w, h];
            var rng = context.CreateRandom();
            var forestCandidates = new bool[w, h];
            var densityAtCell = new float[w, h];

            // Pass 1: визначаємо лісові клітини та оновлюємо біом.
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float height = heightMap[x, y];

                    // Skip if outside valid height range
                    if (height < _minHeight || height > _maxHeight) continue;

                    // Skip if masked out
                    if (mask != null && !mask[x, y]) continue;

                    float density = densityMap[x, y];
                    if (density < _densityThreshold) continue;

                    forestCandidates[x, y] = true;
                    densityAtCell[x, y] = density;

                    // Set biome tile based on density
                    if (density >= _denseThreshold && !string.IsNullOrEmpty(_denseForestTile))
                        resultBiome[x, y] = _denseForestTile;
                    else if (!string.IsNullOrEmpty(_sparseForestTile))
                        resultBiome[x, y] = _sparseForestTile;
                }
            }

            if (_treeObjects == null || _treeObjects.Length == 0)
                return NodeOutput.Success(resultBiome, objectMap);

            // Pass 2: ставимо дерева тільки в достатньо щільних кластерах.
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!forestCandidates[x, y])
                        continue;

                    int neighborCount = CountForestNeighbors(forestCandidates, x, y, w, h);
                    if (neighborCount < _minForestNeighborsForTree)
                        continue;

                    float normalized = Mathf.InverseLerp(_densityThreshold, 1f, densityAtCell[x, y]);
                    float treeProbability = Mathf.Pow(normalized, _treeDensityExponent);

                    if (rng.NextDouble() < treeProbability)
                    {
                        int treeIdx = rng.Next(_treeObjects.Length);
                        objectMap[x, y] = _treeObjects[treeIdx];
                    }
                }
            }

            return NodeOutput.Success(resultBiome, objectMap);
        }

        private static int CountForestNeighbors(bool[,] forestCandidates, int x, int y, int w, int h)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                        continue;

                    if (forestCandidates[nx, ny])
                        count++;
                }
            }

            return count;
        }

        private static float[,] GenerateSimpleNoise(
            int w,
            int h,
            int seed,
            float scale,
            int octaves,
            float lacunarity,
            float persistence)
        {
            var result = new float[w, h];
            float offsetX = seed * 0.7f;
            float offsetY = seed * 1.3f;
            scale = Mathf.Max(0.0001f, scale);
            octaves = Mathf.Max(1, octaves);
            lacunarity = Mathf.Max(1f, lacunarity);
            persistence = Mathf.Clamp01(persistence);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float value = 0f;
                    float amplitudeSum = 0f;

                    for (int octave = 0; octave < octaves; octave++)
                    {
                        float nx = x * scale * frequency + offsetX;
                        float ny = y * scale * frequency + offsetY;
                        value += Mathf.PerlinNoise(nx, ny) * amplitude;
                        amplitudeSum += amplitude;
                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    result[x, y] = amplitudeSum > 0f ? Mathf.Clamp01(value / amplitudeSum) : 0f;
                }
            }
            return result;
        }
    }
}
