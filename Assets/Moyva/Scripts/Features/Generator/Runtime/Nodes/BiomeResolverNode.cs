using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Biome Resolver", "Generators", "Перетворює базову карту висот у біомну карту за правилами висоти та вологості. Це основна нода, яка вирішує, де буде трава, ліс, болото, суха земля чи інші типи поверхні.")]
    public sealed class BiomeResolverNode : NodeBase
    {
        [Header("Biome Settings")]
        [Tooltip("ScriptableObject із повним набором правил біомів. Саме тут зберігаються діапазони висоти, вологості та fallback-тайл для резолву клітинок.")]
        [SerializeField] private DataBiomesSettings _biomesSettings;

        public override string Title => "Biome Resolver";
        public override string Category => "Generators";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("BaseTileMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("No input HeightMap provided.");

            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            var baseTileMap = inputs[1] as string[,] ?? new string[width, height];

            // Keep preview generation alive in editor even if biome config is not assigned yet.
            if (_biomesSettings == null)
                return NodeOutput.Warning("DataBiomesSettings not assigned. Using BaseTileMap as fallback.",
                    (string[,])baseTileMap.Clone());

            string[,] result = ResolveWithLocalSettings(heightMap, (string[,])baseTileMap.Clone(), context);

            return result != null
                ? NodeOutput.Success(result)
                : NodeOutput.Warning("BiomeResolver returned null. Using BaseTileMap as fallback.",
                    (string[,])baseTileMap.Clone());
        }

        private string[,] ResolveWithLocalSettings(float[,] heightMap, string[,] baseTileMap, NodeContext context)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            var result = baseTileMap;
            string defaultTile = string.IsNullOrWhiteSpace(_biomesSettings.DefaultTileID)
                ? "grass"
                : _biomesSettings.DefaultTileID;
            float[,] moistureMap = GenerateMoistureMap(width, height, context);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string selectedTile = SelectBiome(heightMap[x, y], moistureMap[x, y]);

                    if (!string.IsNullOrEmpty(selectedTile))
                        result[x, y] = selectedTile;
                    else if (string.IsNullOrEmpty(result[x, y]))
                        result[x, y] = defaultTile;
                }
            }

            return result;
        }

        private string SelectBiome(float heightValue, float moisture)
        {
            if (_biomesSettings.Biomes == null)
                return null;

            foreach (var biome in _biomesSettings.Biomes)
            {
                if (heightValue >= biome.MinHeight && heightValue <= biome.MaxHeight &&
                    moisture >= biome.MinMoisture && moisture <= biome.MaxMoisture)
                    return biome.TileID;
            }

            return null;
        }

        private float[,] GenerateMoistureMap(int width, int height, NodeContext context)
        {
            var map = new float[width, height];
            float scale = Mathf.Max(0.0001f, _biomesSettings.MoistureScale);
            var rng = context.CreateRandom($"{NodeId}:BiomeMoisture");
            float offsetX = (float)rng.NextDouble() * 9999f;
            float offsetY = (float)rng.NextDouble() * 9999f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float sampleX = (float)x / width * scale + offsetX;
                    float sampleY = (float)y / height * scale + offsetY;
                    map[x, y] = Mathf.PerlinNoise(sampleX, sampleY);
                }
            }

            return map;
        }
    }
}
