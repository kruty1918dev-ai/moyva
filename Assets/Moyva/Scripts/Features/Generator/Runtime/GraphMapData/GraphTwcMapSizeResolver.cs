using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcMapSizeResolver
    {
        Vector2Int Resolve(GraphAsset graph, int requestedWidth, int requestedHeight);
    }

    internal sealed class GraphTwcMapSizeResolver : IGraphTwcMapSizeResolver
    {
        public Vector2Int Resolve(GraphAsset graph, int requestedWidth, int requestedHeight)
        {
            if (GameLaunchContext.TryGetWorldDimensions(out int launchWidth, out int launchHeight))
                return Clamp(launchWidth, launchHeight);

            var shared = graph?.SharedSettings;
            if (shared != null && shared.HasMapSize)
                return Clamp(shared.MapWidth, shared.MapHeight);

            return Clamp(requestedWidth, requestedHeight);
        }

        private static Vector2Int Clamp(int width, int height)
        {
            return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
        }
    }
}
