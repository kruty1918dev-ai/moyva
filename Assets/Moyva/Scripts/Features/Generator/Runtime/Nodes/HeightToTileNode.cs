using Kruty1918.Moyva.Generator.API;
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

        public override string Title => "Height To Tile";
        public override string Category => "Converters";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("TileMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("No input HeightMap provided.");
            if (_heightMapSettings == null)
                return NodeOutput.Error("HeightMapSettings not assigned.");

            string[,] result = GenerateFromLocalSettings(heightMap);

            return result != null
                ? NodeOutput.Success(result)
                : NodeOutput.Error("VirtualHeightMapGenerator returned null.");
        }

        private string[,] GenerateFromLocalSettings(float[,] heightMap)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            var result = new string[width, height];
            var layers = _heightMapSettings.HeightLayers;

            if (layers == null || layers.Length == 0)
                return result;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float heightValue = heightMap[x, y];
                    for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
                    {
                        var layer = layers[layerIndex];
                        if (heightValue >= layer.MinHeight && heightValue <= layer.MaxHeight)
                        {
                            result[x, y] = layer.TileID;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(result[x, y]))
                        result[x, y] = layers[^1].TileID;
                }
            }

            return result;
        }
    }
}
