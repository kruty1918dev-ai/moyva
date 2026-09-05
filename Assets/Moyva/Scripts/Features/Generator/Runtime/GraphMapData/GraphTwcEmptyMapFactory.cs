namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcEmptyMapFactory
    {
        GraphTwcMapGenerationResult Create(int width, int height);
    }

    internal sealed class GraphTwcEmptyMapFactory : IGraphTwcEmptyMapFactory
    {
        public GraphTwcMapGenerationResult Create(int width, int height)
        {
            var biomeMap = new string[width, height];
            var heightMap = new float[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    biomeMap[x, y] = GeneratedWorldDataDefaults.FallbackTileId;
                    heightMap[x, y] = GeneratedWorldDataDefaults.FallbackLandHeight;
                }
            }

            return new GraphTwcMapGenerationResult
            {
                BiomeMap = biomeMap,
                ObjectMap = new string[width, height],
                HeightMap = heightMap,
                BuildingMap = new string[width, height]
            };
        }
    }
}
