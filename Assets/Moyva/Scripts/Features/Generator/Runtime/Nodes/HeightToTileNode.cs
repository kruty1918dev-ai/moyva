using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Height To Tile", "Converters", "Конвертує числову карту висот у базову карту тайлів за шарами висоти. Це міст між сирим рельєфом і візуальною поверхнею, яку потім можуть змінювати біоми та фічі.")]
    public sealed class HeightToTileNode : NodeBase
    {
        [Header("Height Map Settings")]
        [Tooltip("Налаштування шарів висоти, що визначають, який Tile ID отримує кожна клітинка залежно від значення heightMap.")]
        [SerializeField] private HeightMapSettings _heightMapSettings;

        [Header("Layer Mask Output")]
        [Tooltip("Індекс шару HeightLayers, для якого вихід LayerMask буде true. Якщо індекс поза межами, маска буде порожня.")]
        [SerializeField, Min(0)] private int _maskLayerIndex;

        public override string Title => "Height To Tile";
        public override string Category => "Converters";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("TileMap"),
            PortDefinition.Output<bool[,]>("LayerMask"),
            PortDefinition.Output<int[,]>("LayerIndexMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("No input HeightMap provided.");
            if (_heightMapSettings == null)
                return NodeOutput.Error("HeightMapSettings not assigned.");

            string[,] result = GenerateFromLocalSettings(heightMap, context, out var layerMask, out var layerIndexMap);

            return result != null
                ? NodeOutput.Success(result, layerMask, layerIndexMap)
                : NodeOutput.Error("VirtualHeightMapGenerator returned null.");
        }

        private string[,] GenerateFromLocalSettings(
            float[,] heightMap,
            NodeContext context,
            out bool[,] layerMask,
            out int[,] layerIndexMap)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            var result = new string[width, height];
            layerMask = new bool[width, height];
            layerIndexMap = new int[width, height];
            var layers = _heightMapSettings.HeightLayers;

            if (layers == null || layers.Length == 0)
            {
                FillDefaultLayerIndices(layerIndexMap, -1);
                return result;
            }

            bool hasMaskLayer = _maskLayerIndex >= 0 && _maskLayerIndex < layers.Length;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float heightValue = heightMap[x, y];
                    int layerIndex = HeightLayerTileSelector.ResolveLayerIndex(layers, heightValue);

                    layerIndexMap[x, y] = layerIndex;
                    result[x, y] = layerIndex >= 0
                        ? HeightLayerTileSelector.SelectTileId(layers[layerIndex], x, y, context.Seed)
                        : string.Empty;
                    layerMask[x, y] = hasMaskLayer && layerIndex == _maskLayerIndex;
                }
            }

            return result;
        }

        private static void FillDefaultLayerIndices(int[,] layerIndexMap, int value)
        {
            int width = layerIndexMap.GetLength(0);
            int height = layerIndexMap.GetLength(1);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                layerIndexMap[x, y] = value;
        }
    }
}
