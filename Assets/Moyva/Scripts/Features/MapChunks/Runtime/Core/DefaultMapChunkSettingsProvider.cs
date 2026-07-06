using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class DefaultMapChunkSettingsProvider : IMapChunkSettingsProvider
    {
        private static readonly string[] IgnoredNames = { "Fog", "Canvas", "UI", "Camera", "Light" };

        public int ChunkSize => 16;
        public bool EnableCameraCulling => true;
        public float CameraCullingIntervalSeconds => 0.08f;
        public float CameraCullingPaddingCells => 1f;
        public bool EnableVisualChunkDiscovery => true;
        public bool EnableVisualChunkPartitioning => true;
        public float VisualDiscoveryIntervalSeconds => 0.75f;
        public float VisualPartitionDurationSeconds => 2f;
        public LayerMask VisualDiscoveryLayerMask => ~0;
        public IReadOnlyList<string> IgnoredRendererNameTokens => IgnoredNames;
    }
}
