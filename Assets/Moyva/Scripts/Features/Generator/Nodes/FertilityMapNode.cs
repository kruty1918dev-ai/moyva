using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Fertility Map", "Generators")]
    public sealed class FertilityMapNode : NodeBase
    {
        [Header("Fertility Noise")]
        [SerializeField] private DataNoiseSettings _noiseSettings;

        [Header("Height Influence")]
        [SerializeField, Range(0f, 1f)] private float _optimalMinHeight = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _optimalMaxHeight = 0.55f;
        [SerializeField, Range(0f, 1f)] private float _heightInfluence = 0.4f;

        [Header("Water Bonus")]
        [SerializeField, Range(0f, 1f)] private float _waterProximityBonus = 0.3f;
        [SerializeField, Range(1, 20)] private int _waterBonusRadius = 5;

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
                fertility = new float[w, h];
                float offsetX = 123.4f;
                float offsetY = 567.8f;
                for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                        fertility[x, y] = Mathf.PerlinNoise(x * 0.05f + offsetX, y * 0.05f + offsetY);
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
    }
}
