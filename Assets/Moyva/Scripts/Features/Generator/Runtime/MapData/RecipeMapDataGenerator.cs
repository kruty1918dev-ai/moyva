using System;
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Thin map-data entry point for recipe-driven TileWorldCreator generation.
    /// Generation details live in <see cref="IMapGenerationPipeline"/>.
    /// </summary>
    internal sealed class RecipeMapDataGenerator : IMapDataGenerator
    {
        private readonly IMapGenerationEnvironment _environment;
        private readonly IMapGenerationPipeline _pipeline;
        private readonly IMapGenerationState _state;

        public RecipeMapDataGenerator(
            IMapGenerationEnvironment environment,
            IMapGenerationPipeline pipeline,
            IMapGenerationState state)
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
            var request = new MapGenerationRequest(
                _environment.Recipe,
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
