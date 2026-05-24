using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [Serializable]
    public class ScatterLayer
    {
        [Tooltip("ID об'єкта, який спавниться цим шаром. Зазвичай це конкретний тип дерева, куща, каменя чи декоративного елемента.")]
        public string ObjectId = "tree";
        [Tooltip("Мінімальна висота, на якій цей шар дозволяє спавн об'єкта. Допомагає розділити низинні, рівнинні й високогірні декорації.")]
        [Range(0f, 1f)] public float MinHeight = 0.2f;
        [Tooltip("Максимальна висота для цього шару. Разом із MinHeight визначає висотний коридор застосування.")]
        [Range(0f, 1f)] public float MaxHeight = 0.5f;
        [Tooltip("Базова густота спавну цього шару. Визначає, наскільки часто об'єкт намагатиметься з'явитися в дозволених клітинках.")]
        [Range(0f, 1f)] public float Density = 0.3f;
        [Tooltip("Сила кластеризації цього шару. Більші значення роблять розподіл більш плямистим і згрупованим, менші дають рівномірніший шумовий спавн.")]
        [Range(0f, 1f)] public float ClusterStrength = 0.5f;
    }

    [NodeInfo("Multi-Layer Scatter", "Features", "Розкидає кілька шарів об'єктів по карті за висотою, густиною та кластеризацією. Це універсальна нода для рослинності, каміння, квітів та іншого дрібного наповнення світу.")]
    public sealed class MultiLayerScatterNode : NodeBase
    {
        [Header("Scatter Layers")]
        [Tooltip("Набір шарів спавну. Кожен шар описує окремий тип об'єкта зі своїм висотним діапазоном, густиною та силою кластеризації.")]
        [SerializeField] private ScatterLayer[] _layers = new[]
        {
            new ScatterLayer { ObjectId = "tree-oak", MinHeight = 0.3f, MaxHeight = 0.5f, Density = 0.25f, ClusterStrength = 0.6f },
            new ScatterLayer { ObjectId = "bush", MinHeight = 0.2f, MaxHeight = 0.45f, Density = 0.15f, ClusterStrength = 0.3f },
            new ScatterLayer { ObjectId = "rock-small", MinHeight = 0.5f, MaxHeight = 0.7f, Density = 0.2f, ClusterStrength = 0.2f },
            new ScatterLayer { ObjectId = "flower", MinHeight = 0.25f, MaxHeight = 0.4f, Density = 0.1f, ClusterStrength = 0.1f }
        };
        [Tooltip("Якщо увімкнено, нода не буде ставити нові об'єкти в клітинки, де вже є щось у вхідному ObjectMap. Це захищає важливі об'єкти від випадкового перезапису.")]
        [SerializeField] private bool _avoidExistingObjects = true;

        [Header("Cluster Noise")]
        [SerializeField, Min(0.0001f)] private float _clusterNoiseScale = 0.12f;
        [SerializeField, Range(1, 8)] private int _clusterOctaves = 3;
        [SerializeField, Min(1f)] private float _clusterLacunarity = 2f;
        [SerializeField, Range(0.01f, 1f)] private float _clusterPersistence = 0.5f;

        public override string Title => "Multi-Layer Scatter";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("ObjectMap"),
            PortDefinition.Input<bool[,]>("Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");

            var mask = inputs[2] as bool[,];

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);

            var existing = inputs[1] as string[,];
            var result = MapArrayUtils.CloneStringMapOrCreate(existing, w, h);

            if (_layers == null || _layers.Length == 0)
                return NodeOutput.Success(result);

            var rng = context.CreateRandom();

            // Generate cluster noise per layer
            var clusterMaps = new float[_layers.Length][,];
            for (int i = 0; i < _layers.Length; i++)
            {
                clusterMaps[i] = GenerateClusterNoise(
                    w,
                    h,
                    context.Seed,
                    _clusterNoiseScale,
                    _clusterOctaves,
                    _clusterLacunarity,
                    _clusterPersistence);
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (_avoidExistingObjects && !string.IsNullOrEmpty(result[x, y]))
                        continue;
                    if (mask != null && !mask[x, y])
                        continue;

                    float height = heightMap[x, y];

                    for (int i = 0; i < _layers.Length; i++)
                    {
                        var layer = _layers[i];
                        if (height < layer.MinHeight || height > layer.MaxHeight)
                            continue;

                        // Cluster-influenced density
                        float clusterInfluence = Mathf.Lerp(1f, clusterMaps[i][x, y], layer.ClusterStrength);
                        float effectiveDensity = layer.Density * clusterInfluence;

                        if (rng.NextDouble() < effectiveDensity)
                        {
                            result[x, y] = layer.ObjectId;
                            break; // First placed layer wins
                        }
                    }
                }
            }

            return NodeOutput.Success(result);
        }

        private static float[,] GenerateClusterNoise(
            int w,
            int h,
            int seed,
            float scale,
            int octaves,
            float lacunarity,
            float persistence)
        {
            var result = new float[w, h];
            float offsetX = seed * 0.73f;
            float offsetY = seed * 1.17f;
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
