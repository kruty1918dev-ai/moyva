using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct GraphTwcMapGenerationRequest
    {
        public GraphTwcMapGenerationRequest(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            int width,
            int height,
            IReadOnlyList<CompiledLayerMap> lastCompiledLayers,
            int? seedOverride = null)
        {
            Graph = graph;
            Manager = manager;
            Width = width;
            Height = height;
            LastCompiledLayers = lastCompiledLayers;
            SeedOverride = seedOverride;
        }

        public GraphAsset Graph { get; }
        public int? SeedOverride { get; }
        public TileWorldCreatorManager Manager { get; }
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; }
    }
}
