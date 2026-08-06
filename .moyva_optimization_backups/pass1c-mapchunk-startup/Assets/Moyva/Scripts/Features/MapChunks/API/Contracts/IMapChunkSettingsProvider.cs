using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IMapChunkSettingsProvider
    {
        int ChunkSize { get; }
        bool EnableCameraCulling { get; }
        float CameraCullingIntervalSeconds { get; }
        float CameraCullingPaddingCells { get; }
        bool EnableVisualChunkDiscovery { get; }
        bool EnableVisualChunkPartitioning { get; }
        float VisualDiscoveryIntervalSeconds { get; }
        float VisualPartitionDurationSeconds { get; }
        LayerMask VisualDiscoveryLayerMask { get; }
        IReadOnlyList<string> IgnoredRendererNameTokens { get; }
    }
}
