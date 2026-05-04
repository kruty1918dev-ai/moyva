using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("City Generator", "Cities", "Генерує міста на рівнинах, радіуси і зони (центр/житлова/промислова).")]
    public sealed class CityGeneratorNode : NodeBase
    {
        [SerializeField, Range(1, 64)] private int _targetCities = 6;
        [SerializeField, Range(2, 20)] private int _minCityRadius = 4;
        [SerializeField, Range(4, 40)] private int _maxCityRadius = 10;
        [SerializeField, Range(0f, 1f)] private float _minPlainHeight = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _maxPlainHeight = 0.72f;

        public override string Title => "City Generator";
        public override string Category => "Cities";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("BiomeMap (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("CityMask"),
            PortDefinition.Output<string[,]>("CityZoneMap"),
            PortDefinition.Output<int[,]>("DistrictMap"),
            PortDefinition.Output<float[,]>("DensityMap"),
            PortDefinition.Output<string[,]>("BiomeMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var height = inputs[0] as float[,];
            if (height == null) return NodeOutput.Error("HeightMap is required.");

            int w = height.GetLength(0);
            int h = height.GetLength(1);
            var biome = inputs[1] as string[,] ?? new string[w, h];

            var cityMask = new bool[w, h];
            var zoneMap = new string[w, h];
            var districtMap = new int[w, h];
            var densityMap = new float[w, h];
            var outBiome = (string[,])biome.Clone();

            var rng = context.CreateRandom();
            var centers = new List<Vector2Int>();

            for (int i = 0; i < _targetCities; i++)
            {
                if (!TryFindCityCenter(height, w, h, centers, rng, out var center))
                    break;

                centers.Add(center);
                int radius = rng.Next(_minCityRadius, _maxCityRadius + 1);
                int districtId = i + 1;
                PaintCity(center, radius, districtId, cityMask, zoneMap, districtMap, densityMap, outBiome, context);
            }

            return NodeOutput.Success(cityMask, zoneMap, districtMap, densityMap, outBiome);
        }

        private bool TryFindCityCenter(float[,] height, int w, int h,
            List<Vector2Int> centers, System.Random rng, out Vector2Int center)
        {
            for (int t = 0; t < 500; t++)
            {
                int x = rng.Next(2, Mathf.Max(3, w - 2));
                int y = rng.Next(2, Mathf.Max(3, h - 2));
                float v = height[x, y];
                if (v < _minPlainHeight || v > _maxPlainHeight)
                    continue;

                bool tooClose = false;
                for (int i = 0; i < centers.Count; i++)
                {
                    if ((centers[i] - new Vector2Int(x, y)).sqrMagnitude < (_maxCityRadius * _maxCityRadius * 4))
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    center = new Vector2Int(x, y);
                    return true;
                }
            }

            center = default;
            return false;
        }

        private static void PaintCity(Vector2Int c, int radius, int districtId,
            bool[,] cityMask, string[,] zoneMap, int[,] districtMap, float[,] densityMap,
            string[,] biome, NodeContext context)
        {
            int w = cityMask.GetLength(0);
            int h = cityMask.GetLength(1);
            int centerR = Mathf.Max(1, Mathf.RoundToInt(radius * 0.25f));
            int resR = Mathf.Max(centerR + 1, Mathf.RoundToInt(radius * 0.65f));

            for (int x = Mathf.Max(0, c.x - radius); x <= Mathf.Min(w - 1, c.x + radius); x++)
            {
                for (int y = Mathf.Max(0, c.y - radius); y <= Mathf.Min(h - 1, c.y + radius); y++)
                {
                    int dx = x - c.x;
                    int dy = y - c.y;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > radius) continue;
                    float t = Mathf.Clamp01(1f - d / Mathf.Max(1f, radius));

                    cityMask[x, y] = true;
                    districtMap[x, y] = districtId;
                    if (d <= centerR)
                    {
                        zoneMap[x, y] = "center";
                        biome[x, y] = "city_center";
                        densityMap[x, y] = Mathf.Max(densityMap[x, y], Mathf.Lerp(0.9f, 1f, t));
                    }
                    else if (d <= resR)
                    {
                        zoneMap[x, y] = "residential";
                        biome[x, y] = "city_residential";
                        densityMap[x, y] = Mathf.Max(densityMap[x, y], Mathf.Lerp(0.5f, 0.85f, t));
                    }
                    else
                    {
                        zoneMap[x, y] = "industrial";
                        biome[x, y] = "city_industrial";
                        densityMap[x, y] = Mathf.Max(densityMap[x, y], Mathf.Lerp(0.2f, 0.6f, t));
                    }

                    context.CountIteration();
                }
            }
        }
    }
}
