using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    [CreateAssetMenu(fileName = "MapChunkSettings", menuName = "Moyva/Map/Chunk Settings")]
    public sealed class MapChunkSettingsSO : ScriptableObject, IMapChunkSettingsProvider
    {
        [Tooltip("Розмір чанка у grid-тайлах: 16 означає 16x16 тайлів. Крайові чанки можуть бути менші, якщо розмір мапи не кратний.")]
        [Min(1)] public int ChunkSize = 16;
        public bool EnableCameraCulling = true;
        [Min(0.02f)] public float CameraCullingIntervalSeconds = 0.08f;
        [Min(0f)] public float CameraCullingPaddingCells = 1f;
        public bool EnableVisualChunkDiscovery = true;
        public bool EnableVisualChunkPartitioning = true;
        [Min(0.05f)] public float VisualDiscoveryIntervalSeconds = 0.75f;
        [Min(0.1f)] public float VisualPartitionDurationSeconds = 2f;
        public LayerMask VisualDiscoveryLayerMask = ~0;
        public string[] IgnoredRendererNameTokens = { "Fog", "Canvas", "UI", "Camera", "Light" };

        int IMapChunkSettingsProvider.ChunkSize => Mathf.Max(1, ChunkSize);
        bool IMapChunkSettingsProvider.EnableCameraCulling => EnableCameraCulling;
        float IMapChunkSettingsProvider.CameraCullingIntervalSeconds => Mathf.Max(0.02f, CameraCullingIntervalSeconds);
        float IMapChunkSettingsProvider.CameraCullingPaddingCells => Mathf.Max(0f, CameraCullingPaddingCells);
        bool IMapChunkSettingsProvider.EnableVisualChunkDiscovery => EnableVisualChunkDiscovery;
        bool IMapChunkSettingsProvider.EnableVisualChunkPartitioning => EnableVisualChunkPartitioning;
        float IMapChunkSettingsProvider.VisualDiscoveryIntervalSeconds => Mathf.Max(0.05f, VisualDiscoveryIntervalSeconds);
        float IMapChunkSettingsProvider.VisualPartitionDurationSeconds => Mathf.Max(0.1f, VisualPartitionDurationSeconds);
        LayerMask IMapChunkSettingsProvider.VisualDiscoveryLayerMask => VisualDiscoveryLayerMask;
        IReadOnlyList<string> IMapChunkSettingsProvider.IgnoredRendererNameTokens => IgnoredRendererNameTokens;
    }
}
