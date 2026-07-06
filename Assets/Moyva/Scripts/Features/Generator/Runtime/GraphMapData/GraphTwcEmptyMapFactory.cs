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
            return new GraphTwcMapGenerationResult
            {
                BiomeMap = new string[width, height],
                ObjectMap = new string[width, height],
                HeightMap = new float[width, height],
                BuildingMap = new string[width, height]
            };
        }
    }
}
