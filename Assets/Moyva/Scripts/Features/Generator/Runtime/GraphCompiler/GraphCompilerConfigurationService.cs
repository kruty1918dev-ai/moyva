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
        private static readonly Vector2Int DefaultMapSize = new Vector2Int(50, 50);

        public Vector2Int ResolveRequestedSize(GraphAsset graph, Vector2Int? mapSizeOverride)
        {
            if (mapSizeOverride.HasValue)
                return Normalize(mapSizeOverride.Value);

            var sharedSize = graph?.SharedSettings?.MapSize ?? DefaultMapSize;
            return Normalize(sharedSize);
        }

        public void Apply(GraphAsset graph, Configuration config, int seed, Vector2Int? mapSizeOverride)
        {
            graph.EnsureLayerGraphStates();
            graph.EnsureDefaultLayer();

            Vector2Int size = ResolveRequestedSize(graph, mapSizeOverride);
            config.width = size.x;
            config.height = size.y;
            config.useGlobalRandomSeed = true;
            config.globalRandomSeed = seed == 0 ? 1 : seed;
            GraphCompilerLayerAssetUtility.EnsureBlueprintRootFolder(config);
        }

        private static Vector2Int Normalize(Vector2Int size)
        {
            return new Vector2Int(
                size.x > 0 ? size.x : DefaultMapSize.x,
                size.y > 0 ? size.y : DefaultMapSize.y);
        }
    }
}
