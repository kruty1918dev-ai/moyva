using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerConfigurationService
    {
        Vector2Int ResolveRequestedSize(GraphAsset graph, Vector2Int? mapSizeOverride);
        void Apply(GraphAsset graph, Configuration config, int seed, Vector2Int? mapSizeOverride);
    }

    internal sealed class GraphCompilerConfigurationService : IGraphCompilerConfigurationService
    {
        public Vector2Int ResolveRequestedSize(GraphAsset graph, Vector2Int? mapSizeOverride)
        {
            return mapSizeOverride ?? graph?.SharedSettings?.MapSize ?? new Vector2Int(50, 50);
        }

        public void Apply(GraphAsset graph, Configuration config, int seed, Vector2Int? mapSizeOverride)
        {
            graph.EnsureLayerGraphStates();
            graph.EnsureDefaultLayer();

            Vector2Int size = ResolveRequestedSize(graph, mapSizeOverride);
            config.width = size.x > 0 ? size.x : 50;
            config.height = size.y > 0 ? size.y : 50;
            config.useGlobalRandomSeed = true;
            config.globalRandomSeed = seed == 0 ? 1 : seed;
            GraphCompilerLayerAssetUtility.EnsureBlueprintRootFolder(config);
        }
    }
}
