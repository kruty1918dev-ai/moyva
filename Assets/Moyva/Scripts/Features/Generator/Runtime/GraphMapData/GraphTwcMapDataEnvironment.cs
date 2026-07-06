using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphTwcMapDataEnvironment : IGraphTwcMapDataEnvironment
    {
        public GraphTwcMapDataEnvironment(GraphAsset graph, TileWorldCreatorManager manager)
        {
            Graph = graph;
            Manager = manager;
        }

        public GraphAsset Graph { get; }
        public TileWorldCreatorManager Manager { get; }
    }
}
