using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerRuntimeContextFactory
    {
        NodeContext Create(GraphAsset graph, int seed, Vector2Int mapSize, LayerMaskRegistry layerMaskRegistry = null);
        void RegisterServices(NodeContext context, GraphAsset graph);
    }

    internal sealed class GraphCompilerRuntimeContextFactory : IGraphCompilerRuntimeContextFactory
    {
        public NodeContext Create(GraphAsset graph, int seed, Vector2Int mapSize, LayerMaskRegistry layerMaskRegistry = null)
        {
            var context = new NodeContext(seed)
            {
                MapSize = mapSize
            };

            RegisterServices(context, graph);
            if (layerMaskRegistry != null)
                context.RegisterService(layerMaskRegistry);
            return context;
        }

        public void RegisterServices(NodeContext context, GraphAsset graph)
        {
            if (context == null || graph == null)
                return;

            var sharedSettings = graph.SharedSettings;
            if (sharedSettings != null)
            {
                context.ApplySharedSettings(sharedSettings);
                context.RegisterService(sharedSettings);
            }

            if (graph.TileRegistry != null)
                context.RegisterService(graph.TileRegistry);

            context.RegisterService<IGeneratorDataRegistry>(new GeneratorDataRegistry());
        }
    }
}
