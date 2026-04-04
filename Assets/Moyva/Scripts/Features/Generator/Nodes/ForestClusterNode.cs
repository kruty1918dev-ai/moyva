using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Forest Cluster", "Features")]
    public sealed class ForestClusterNode : NodeBase
    {
        [Header("Noise")]
        [SerializeField] private DataNoiseSettings _densityNoise;

        [Header("Forest Settings")]
        [SerializeField, Range(0f, 1f)] private float _densityThreshold = 0.45f;
        [SerializeField, Range(0f, 1f)] private float _minHeight = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _maxHeight = 0.65f;
        [SerializeField] private string[] _treeObjects = { "tree_oak", "tree_pine", "tree_birch" };
        [SerializeField] private string _denseForestTile = "forest_dense";
        [SerializeField] private string _sparseForestTile = "forest_sparse";
        [SerializeField, Range(0f, 1f)] private float _denseThreshold = 0.65f;
        [SerializeField] private int _seed = 42;

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
                densityMap = GenerateSimpleNoise(w, h, _seed);
            }

            var resultBiome = (string[,])biomeMap.Clone();
            var objectMap = new string[w, h];
            var rng = new System.Random(_seed);

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

                    // Set biome tile based on density
                    if (density >= _denseThreshold && !string.IsNullOrEmpty(_denseForestTile))
                        resultBiome[x, y] = _denseForestTile;
                    else if (!string.IsNullOrEmpty(_sparseForestTile))
                        resultBiome[x, y] = _sparseForestTile;

                    // Place tree objects with density-based probability
                    if (_treeObjects != null && _treeObjects.Length > 0)
                    {
                        float treeProbability = Mathf.InverseLerp(_densityThreshold, 1f, density);
                        if (rng.NextDouble() < treeProbability)
                        {
                            int treeIdx = rng.Next(_treeObjects.Length);
                            objectMap[x, y] = _treeObjects[treeIdx];
                        }
                    }
                }
            }

            return NodeOutput.Success(resultBiome, objectMap);
        }

        private static float[,] GenerateSimpleNoise(int w, int h, int seed)
        {
            var result = new float[w, h];
            float offsetX = seed * 0.7f;
            float offsetY = seed * 1.3f;
            float scale = 0.08f;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float nx = x * scale + offsetX;
                    float ny = y * scale + offsetY;
                    result[x, y] = Mathf.PerlinNoise(nx, ny);
                }
            }
            return result;
        }
    }
}
