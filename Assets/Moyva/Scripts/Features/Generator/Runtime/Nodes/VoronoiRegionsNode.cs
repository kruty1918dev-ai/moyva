using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Voronoi Regions", "Generators", "Розбиває мапу на області Вороного навколо випадкових центрів. Зручно для формування регіонів, провінцій, біомних зон або подальшого локального розподілу правил генерації.")]
    public sealed class VoronoiRegionsNode : NodeBase
    {
        [Header("Voronoi Settings")]
        [Tooltip("Кількість регіональних центрів, навколо яких будуть побудовані області. Більше значення дає дрібнішу сітку регіонів.")]
        [SerializeField, Range(2, 100)] private int _regionCount = 12;
        [Tooltip("Seed для випадкового розташування центрів регіонів. Дозволяє відтворювати однаковий поділ мапи при повторній генерації.")]
        [SerializeField] private int _seed;
        [Tooltip("Якщо увімкнено, нода додатково поверне карту відстані до найближчого центру регіону. Це корисний допоміжний шар для побудови меж, ядер регіонів або масок згасання.")]
        [SerializeField] private bool _distanceMap;

        public override string Title => "Voronoi Regions";
        public override string Category => "Generators";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<int>("MapWidth"),
            PortDefinition.Input<int>("MapHeight")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<int[,]>("RegionIds"),
            PortDefinition.Output<float[,]>("DistanceMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs[0] is not int width || inputs[1] is not int height)
                return NodeOutput.Error("MapWidth and MapHeight inputs are required.");

            if (width <= 0 || height <= 0)
                return NodeOutput.Error("Map dimensions must be positive.");

            var rng = new System.Random(_seed);
            var seeds = new Vector2Int[_regionCount];
            for (int i = 0; i < _regionCount; i++)
                seeds[i] = new Vector2Int(rng.Next(width), rng.Next(height));

            var regions = new int[width, height];
            var distances = _distanceMap ? new float[width, height] : null;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float minDist = float.MaxValue;
                    int closest = 0;

                    for (int i = 0; i < _regionCount; i++)
                    {
                        float dx = x - seeds[i].x;
                        float dy = y - seeds[i].y;
                        float dist = dx * dx + dy * dy;
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closest = i;
                        }
                    }

                    regions[x, y] = closest;
                    if (distances != null)
                        distances[x, y] = Mathf.Sqrt(minDist);
                }
            }

            // Normalize distance map
            if (distances != null)
            {
                float max = 0f;
                foreach (float d in distances)
                    if (d > max) max = d;

                if (max > 0f)
                {
                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                            distances[x, y] /= max;
                }
            }

            return NodeOutput.Success(regions, _distanceMap ? distances : null);
        }
    }
}
