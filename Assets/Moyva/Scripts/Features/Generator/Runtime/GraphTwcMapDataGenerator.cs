using System;
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Thin map-data entry point for graph-driven TileWorldCreator generation.
    /// Generation details live in <see cref="IGraphTwcMapGenerationPipeline"/>.
    /// </summary>
    internal sealed class GraphTwcMapDataGenerator : IMapDataGenerator
    {
        private readonly IGraphTwcMapDataEnvironment _environment;
        private readonly IGraphTwcMapGenerationPipeline _pipeline;
        private readonly IGraphTwcMapDataState _state;

        public GraphTwcMapDataGenerator(
            IGraphTwcMapDataEnvironment environment,
            IGraphTwcMapGenerationPipeline pipeline,
            IGraphTwcMapDataState state)
        {
            _environment = environment;
            _pipeline = pipeline;
            _state = state;
        }

        public void GenerateMapData(
            int width,
            int height,
            Action<string[,], string[,], float[,], string[,]> onComplete)
        {
            var request = new GraphTwcMapGenerationRequest(
                _environment.Graph,
                _environment.Manager,
                width,
                height,
                _state.LastCompiledLayers);
            var result = _pipeline.Generate(request);
            _state.Apply(result);
            onComplete?.Invoke(
                result.BiomeMap,
                result.ObjectMap,
                result.HeightMap,
                result.BuildingMap);
        }
    }
}
