using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.SaveSystem;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcSeedService
    {
        int Resolve(GraphAsset graph);
    }

    internal sealed class GraphTwcSeedService : IGraphTwcSeedService
    {
        public int Resolve(GraphAsset graph)
        {
            if (GameLaunchContext.TryGetSeed(out int launchSeed))
                return GlobalSeed.Normalize(launchSeed);

            if (graph?.Nodes == null)
                return GlobalSeed.DefaultSeed;

            foreach (var node in graph.Nodes)
            {
                if (node is ISeedProvider seedProvider)
                    return GlobalSeed.Normalize(seedProvider.Seed);
            }

            return GlobalSeed.DefaultSeed;
        }
    }
}
