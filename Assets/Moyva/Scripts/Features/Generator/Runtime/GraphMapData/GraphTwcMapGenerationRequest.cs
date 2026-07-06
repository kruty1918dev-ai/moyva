using System.Collections.Generic;
using GiantGrey.TileWorldCreator.Components;
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
            IReadOnlyList<CompiledLayerMap> lastCompiledLayers)
        {
            Graph = graph;
            Manager = manager;
            Width = width;
            Height = height;
            LastCompiledLayers = lastCompiledLayers;
        }

        public GraphAsset Graph { get; }
        public TileWorldCreatorManager Manager { get; }
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; }
    }
}
