using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Fertility Map", "Generators", "Будує карту родючості на основі шуму, висоти й близькості до води. Використовується як допоміжний шар для лісів, полів, поселень та інших систем, яким важлива якість ґрунту.")]
    public sealed class FertilityMapNode : NodeBase
    {
        [Header("Fertility Noise")]
        [Tooltip("Налаштування базового шуму родючості. Якщо не задано, нода побудує простий шум самостійно, але окремий asset дає більше контролю над характером карти.")]
        [SerializeField] private DataNoiseSettings _noiseSettings;

        [Header("Height Influence")]
        [Tooltip("Нижня межа оптимальної висоти для родючих земель. Нижче цього значення ділянка вважається менш придатною для рослинності або сільського господарства.")]
        [SerializeField, Range(0f, 1f)] private float _optimalMinHeight = 0.25f;
        [Tooltip("Верхня межа оптимальної висоти для родючості. Допомагає не робити однаково родючими низини, середні висоти та гірські плато.")]
        [SerializeField, Range(0f, 1f)] private float _optimalMaxHeight = 0.55f;
        [Tooltip("Наскільки сильно оптимальна висота впливає на фінальну карту родючості. 0 лишає лише шум, 1 майже повністю підкорює результат висотному правилу.")]
        [SerializeField, Range(0f, 1f)] private float _heightInfluence = 0.4f;

        [Header("Water Bonus")]
        [Tooltip("Додатковий бонус до родючості біля води. Дає змогу підсилити привабливість берегових зон для лісів, поселень чи полів.")]
        [SerializeField, Range(0f, 1f)] private float _waterProximityBonus = 0.3f;
        [Tooltip("Радіус пошуку води для бонусу родючості. Клітинки за межами цього радіуса не отримають перевагу від близькості до водойми.")]
        [SerializeField, Range(1, 20)] private int _waterBonusRadius = 5;

        [Header("Fallback Fertility Noise")]
        [SerializeField, Min(0.0001f)] private float _fallbackNoiseScale = 0.05f;
        [SerializeField, Range(1, 8)] private int _fallbackNoiseOctaves = 1;
        [SerializeField, Min(1f)] private float _fallbackNoiseLacunarity = 2f;
        [SerializeField, Range(0.01f, 1f)] private float _fallbackNoisePersistence = 0.5f;

        public override string Title => "Fertility Map";
        public override string Category => "Generators";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<bool[,]>("WaterMask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("FertilityMap"),
            PortDefinition.Output<bool[,]>("FertileMask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");

            var waterMask = inputs[1] as bool[,];

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);

            // Base fertility from noise
            float[,] fertility;
            if (_noiseSettings != null)
            {
                var noiseProvider = context.GetService<INoiseProvider>();
                fertility = noiseProvider.GenerateNoiseMap(_noiseSettings, w, h);
            }
            else
            {
                fertility = GenerateFallbackNoise(
                    w,
                    h,
                    context.Seed,
                    _fallbackNoiseScale,
                    _fallbackNoiseOctaves,
                    _fallbackNoiseLacunarity,
                    _fallbackNoisePersistence);
            }

            // Height influence — optimal height range gets bonus
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float height = heightMap[x, y];
                    float heightBonus = 0f;

                    if (height >= _optimalMinHeight && height <= _optimalMaxHeight)
                    {
                        float mid = (_optimalMinHeight + _optimalMaxHeight) / 2f;
                        float range = (_optimalMaxHeight - _optimalMinHeight) / 2f;
                        heightBonus = 1f - Mathf.Abs(height - mid) / range;
                    }

                    fertility[x, y] = Mathf.Lerp(fertility[x, y],
                        Mathf.Clamp01(fertility[x, y] + heightBonus), _heightInfluence);
                }
            }

            // Water proximity bonus
            if (waterMask != null && _waterProximityBonus > 0)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        if (waterMask[x, y]) continue;

                        float minDist = FindMinWaterDist(waterMask, x, y, w, h, _waterBonusRadius);
                        if (minDist <= _waterBonusRadius)
                        {
                            float bonus = _waterProximityBonus * (1f - minDist / _waterBonusRadius);
                            fertility[x, y] = Mathf.Clamp01(fertility[x, y] + bonus);
                        }
                    }
                }
            }

            // Generate fertile mask (above 0.5 = fertile)
            var fertileMask = new bool[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    fertileMask[x, y] = fertility[x, y] > 0.5f;

            return NodeOutput.Success(fertility, fertileMask);
        }

        private static float FindMinWaterDist(bool[,] waterMask, int cx, int cy, int w, int h, int radius)
        {
            float minDist = float.MaxValue;
            int r2 = radius * radius;

            int minX = Mathf.Max(0, cx - radius);
            int maxX = Mathf.Min(w - 1, cx + radius);
            int minY = Mathf.Max(0, cy - radius);
            int maxY = Mathf.Min(h - 1, cy + radius);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!waterMask[x, y]) continue;
                    int dx = x - cx;
                    int dy = y - cy;
                    float dist2 = dx * dx + dy * dy;
                    if (dist2 < r2 && dist2 < minDist * minDist)
                        minDist = Mathf.Sqrt(dist2);
                }
            }

            return minDist;
        }

        private static float[,] GenerateFallbackNoise(
            int w,
            int h,
            int seed,
            float scale,
            int octaves,
            float lacunarity,
            float persistence)
        {
            var result = new float[w, h];
            var rng = new System.Random(seed);
            float offsetX = (float)rng.NextDouble() * 9999f;
            float offsetY = (float)rng.NextDouble() * 9999f;
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
                        value += Mathf.PerlinNoise(x * scale * frequency + offsetX, y * scale * frequency + offsetY) * amplitude;
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
