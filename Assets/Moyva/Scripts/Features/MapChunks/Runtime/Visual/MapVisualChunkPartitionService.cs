using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapVisualChunkPartitionService :
        IMapVisualChunkPartitionService,
        IInitializable,
        ITickable,
        IDisposable
    {
        private const string AuditPrefix =
            "[MOYVA_CHUNK_PARTITION]";

        private readonly SignalBus _signalBus;
        private readonly IMapChunkSettingsProvider _settings;
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapVisualChunkRootService _roots;
        private readonly IMapVisualRendererCollector _collector;
        private readonly IMapVisualRendererFilter _filter;
        private readonly List<Renderer> _renderers = new(512);
        private readonly List<MapChunkCoord> _chunks = new(16);

        private float _partitionUntil;
        private float _nextPartitionAt;
        private bool _requested;
        private int _lastRequestFrame = -1;

        public MapVisualChunkPartitionService(
            SignalBus signalBus,
            IMapChunkSettingsProvider settings,
            IMapChunkLayoutService layout,
            IMapVisualChunkRootService roots,
            IMapVisualRendererCollector collector,
            IMapVisualRendererFilter filter)
        {
            _signalBus = signalBus;
            _settings = settings;
            _layout = layout;
            _roots = roots;
            _collector = collector;
            _filter = filter;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldBuiltSignal>(
                OnWorldBuilt);

            _signalBus.Subscribe<WorldGeneratedDataSignal>(
                OnWorldGenerated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(
                OnWorldBuilt);

            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(
                OnWorldGenerated);
        }

        public void Tick()
        {
            if (!_settings.EnableVisualChunkPartitioning
                || !_layout.IsConfigured)
            {
                return;
            }

            if (!_requested
                && Time.unscaledTime > _partitionUntil)
            {
                return;
            }

            if (Time.unscaledTime < _nextPartitionAt)
                return;

            PartitionOnce();
            _requested = false;

            if (_settings
                .RepeatVisualChunkPartitioningDuringStartup)
            {
                _nextPartitionAt =
                    Time.unscaledTime
                    + _settings.VisualDiscoveryIntervalSeconds;
            }
            else
            {
                _partitionUntil =
                    float.NegativeInfinity;

                _nextPartitionAt =
                    float.PositiveInfinity;
            }
        }

        public void RequestPartition()
        {
            if (_lastRequestFrame == Time.frameCount)
                return;

            _lastRequestFrame =
                Time.frameCount;

            _requested =
                true;

            _partitionUntil =
                _settings
                    .RepeatVisualChunkPartitioningDuringStartup
                    ? Time.unscaledTime
                      + _settings.VisualPartitionDurationSeconds
                    : Time.unscaledTime;

            _nextPartitionAt =
                0f;
        }

        private void OnWorldBuilt(
            WorldBuiltSignal _)
        {
            RequestPartition();
        }

        private void OnWorldGenerated(
            WorldGeneratedDataSignal _)
        {
            RequestPartition();
        }

        private void PartitionOnce()
        {
            _collector.CollectScene(
                _renderers);

            int moved = 0;
            int multiChunk = 0;
            int oversized = 0;
            int noChunk = 0;

            for (int i = 0;
                 i < _renderers.Count;
                 i++)
            {
                Renderer renderer =
                    _renderers[i];

                if (!_filter.CanPartition(
                        renderer,
                        _roots))
                {
                    continue;
                }

                int count =
                    _layout.GetChunksOverlapping(
                        renderer.bounds,
                        _chunks);

                if (count != 1)
                {
                    if (count > 1)
                    {
                        multiChunk++;

                        LogPartitionDetail(
                            "MULTI_CHUNK",
                            renderer,
                            _chunks,
                            default,
                            false);
                    }
                    else
                    {
                        noChunk++;

                        LogPartitionDetail(
                            "NO_CHUNK",
                            renderer,
                            _chunks,
                            default,
                            false);
                    }

                    continue;
                }

                MapChunkCoord targetCoord =
                    _chunks[0];

                if (!MapChunkBoundsContainment
                    .ContainsRendererXZ(
                        _layout,
                        renderer.bounds,
                        targetCoord))
                {
                    oversized++;

                    LogPartitionDetail(
                        "OVERSIZED",
                        renderer,
                        _chunks,
                        targetCoord,
                        true);

                    continue;
                }

                Transform root =
                    _roots.GetOrCreateRoot(
                        targetCoord);

                if (renderer.transform.parent == root)
                    continue;

                LogPartitionDetail(
                    "MOVE",
                    renderer,
                    _chunks,
                    targetCoord,
                    true);

                renderer.transform.SetParent(
                    root,
                    true);

                moved++;
            }

            if (moved > 0
                || multiChunk > 0
                || oversized > 0
                || noChunk > 0)
            {
                Debug.Log(
                    $"[MoyvaMapChunks] Visual partition pass " +
                    $"moved={moved}, " +
                    $"multiChunk={multiChunk}, " +
                    $"oversized={oversized}, " +
                    $"noChunk={noChunk}, " +
                    $"scanned={_renderers.Count}.");
            }

            Debug.Log(
                $"{AuditPrefix} COMPLETE " +
                $"moved={moved} " +
                $"multiChunk={multiChunk} " +
                $"oversized={oversized} " +
                $"noChunk={noChunk} " +
                $"scanned={_renderers.Count}");

            _renderers.Clear();
        }

        private static void LogPartitionDetail(
            string kind,
            Renderer renderer,
            IReadOnlyList<MapChunkCoord> overlaps,
            MapChunkCoord target,
            bool hasTarget)
        {
            if (renderer == null)
                return;

            Transform parent =
                renderer.transform.parent;

            Debug.LogWarning(
                $"{AuditPrefix} {kind} " +
                $"renderer='{BuildPath(renderer.transform)}' " +
                $"parent='" +
                $"{(parent != null ? BuildPath(parent) : "<root>")}' " +
                $"generatedTerrain={IsGeneratedTerrain(renderer)} " +
                $"overlaps={FormatChunks(overlaps)} " +
                $"target=" +
                $"{(hasTarget ? target.ToString() : "<none>")} " +
                $"bounds={FormatBounds(renderer.bounds)}");
        }

        private static bool IsGeneratedTerrain(
            Renderer renderer)
        {
            return renderer != null
                && string.Equals(
                    renderer.gameObject.name,
                    "TerrainMesh",
                    StringComparison.Ordinal);
        }

        private static string BuildPath(
            Transform transform)
        {
            if (transform == null)
                return "<null>";

            var builder =
                new StringBuilder(
                    transform.name);

            Transform current =
                transform.parent;

            while (current != null)
            {
                builder.Insert(
                    0,
                    '/');

                builder.Insert(
                    0,
                    current.name);

                current =
                    current.parent;
            }

            return builder.ToString();
        }

        private static string FormatChunks(
            IReadOnlyList<MapChunkCoord> chunks)
        {
            if (chunks == null
                || chunks.Count == 0)
            {
                return "[]";
            }

            var builder =
                new StringBuilder("[");

            for (int i = 0;
                 i < chunks.Count;
                 i++)
            {
                if (i > 0)
                    builder.Append(',');

                builder.Append(
                    chunks[i]);
            }

            builder.Append(']');
            return builder.ToString();
        }

        private static string FormatBounds(
            Bounds bounds)
        {
            return
                $"center=({F(bounds.center.x)}," +
                $"{F(bounds.center.y)}," +
                $"{F(bounds.center.z)})/" +
                $"size=({F(bounds.size.x)}," +
                $"{F(bounds.size.y)}," +
                $"{F(bounds.size.z)})";
        }

        private static string F(
            float value)
        {
            return value.ToString(
                "0.###",
                CultureInfo.InvariantCulture);
        }
    }
}
