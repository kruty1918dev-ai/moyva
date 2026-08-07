using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Moyva/Map Chunks/Map Chunk Scene Settings")]
    public sealed class MapChunkSceneSettings : MonoBehaviour, IMapChunkSettingsProvider
    {
        [Header("Chunk Layout")]
        [Tooltip("Розмір чанка у grid-тайлах: 16 означає 16x16 тайлів. Якщо розмір карти не кратний chunkSize, +X/+Z край автоматично обрізається до останнього повного чанка.")]
        [SerializeField, Min(1)] private int chunkSize = 16;

        [Header("Camera Culling")]
        [SerializeField] private bool enableCameraCulling = true;
        [SerializeField, Min(0.02f)] private float cameraCullingIntervalSeconds = 0.08f;
        [SerializeField, Min(0f)] private float cameraCullingPaddingCells = 1f;

        [Header("Visual Discovery")]
        [SerializeField] private bool enableVisualChunkDiscovery = true;
        [SerializeField] private bool enableVisualChunkPartitioning = true;

        [Tooltip(
            "Повторно сканує renderers протягом startup-вікна. " +
            "Для chunk-first генерації зазвичай не потрібно.")]
        [SerializeField]
        private bool repeatVisualChunkDiscoveryDuringStartup;

        [Tooltip(
            "Повторно перепризначає renderer parents протягом startup-вікна. " +
            "Для повністю зібраного світу залиш вимкненим.")]
        [SerializeField]
        private bool repeatVisualChunkPartitioningDuringStartup;

        [SerializeField, Min(0.05f)] private float visualDiscoveryIntervalSeconds = 0.75f;
        [SerializeField, Min(0.1f)] private float visualPartitionDurationSeconds = 2f;
        [SerializeField] private LayerMask visualDiscoveryLayerMask = ~0;
        [SerializeField] private string[] ignoredRendererNameTokens = { "Fog", "Canvas", "UI", "Camera", "Light" };

        public int ChunkSize => Mathf.Max(1, chunkSize);
        public bool EnableCameraCulling => enableCameraCulling;
        public float CameraCullingIntervalSeconds => Mathf.Max(0.02f, cameraCullingIntervalSeconds);
        public float CameraCullingPaddingCells => Mathf.Max(0f, cameraCullingPaddingCells);
        public bool EnableVisualChunkDiscovery => enableVisualChunkDiscovery;
        public bool EnableVisualChunkPartitioning => enableVisualChunkPartitioning;
        public bool RepeatVisualChunkDiscoveryDuringStartup =>
            repeatVisualChunkDiscoveryDuringStartup;
        public bool RepeatVisualChunkPartitioningDuringStartup =>
            repeatVisualChunkPartitioningDuringStartup;
        public float VisualDiscoveryIntervalSeconds => Mathf.Max(0.05f, visualDiscoveryIntervalSeconds);
        public float VisualPartitionDurationSeconds => Mathf.Max(0.1f, visualPartitionDurationSeconds);
        public LayerMask VisualDiscoveryLayerMask => visualDiscoveryLayerMask;
        public IReadOnlyList<string> IgnoredRendererNameTokens => ignoredRendererNameTokens;
    }
}
