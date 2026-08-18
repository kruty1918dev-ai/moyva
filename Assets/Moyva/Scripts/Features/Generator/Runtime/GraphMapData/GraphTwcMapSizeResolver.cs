using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.MapChunks.API;
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
            Vector2Int requested;
            var shared = graph?.SharedSettings;
            if (shared != null && shared.HasMapSize)
                requested = Clamp(shared.MapWidth, shared.MapHeight);
            else if (GameLaunchContext.TryGetWorldDimensions(out int launchWidth, out int launchHeight))
                requested = Clamp(launchWidth, launchHeight);
            else
                requested = Clamp(requestedWidth, requestedHeight);

            Vector2Int effective = MapChunkSizePolicy.CropMapSize(requested.x, requested.y);
            if (effective != requested)
            {
                Debug.Log(
                    "[MOYVA_CHUNK_SIZE] MAP_REQUEST_CROPPED " +
                    $"requested={requested.x}x{requested.y} " +
                    $"effective={effective.x}x{effective.y} " +
                    $"chunkSize={MapChunkSizePolicy.ChunkSize} fullChunksOnly=1");
            }
            return effective;
        }

        private static Vector2Int Clamp(int width, int height)
        {
            return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
        }
    }
}
