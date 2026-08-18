using System;
using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapVisualChunkPartitionService : IMapVisualChunkPartitionService, IInitializable, ITickable, IDisposable
    {
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
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public void Tick()
        {
            if (!_settings.EnableVisualChunkPartitioning || !_layout.IsConfigured)
                return;

            if (!_requested && Time.unscaledTime > _partitionUntil)
                return;

            if (Time.unscaledTime < _nextPartitionAt)
                return;

            PartitionOnce();
            _requested = false;
            _nextPartitionAt = Time.unscaledTime + _settings.VisualDiscoveryIntervalSeconds;
        }

        public void RequestPartition()
        {
            _requested = true;
            _partitionUntil = Time.unscaledTime + _settings.VisualPartitionDurationSeconds;
            _nextPartitionAt = 0f;
        }

        private void OnWorldBuilt(WorldBuiltSignal _) => RequestPartition();
        private void OnWorldGenerated(WorldGeneratedDataSignal _) => RequestPartition();

        private void PartitionOnce()
        {
            _collector.CollectScene(_renderers);

            int moved = 0;
            int multiChunk = 0;
            int oversized = 0;
            for (int i = 0; i < _renderers.Count; i++)
            {
                var renderer = _renderers[i];
                if (!_filter.CanPartition(renderer, _roots))
                    continue;

                int count = _layout.GetChunksOverlapping(renderer.bounds, _chunks);
                if (count != 1)
                {
                    if (count > 1)
                        multiChunk++;
                    continue;
                }

                if (!MapChunkBoundsContainment.ContainsRendererXZ(_layout, renderer.bounds, _chunks[0]))
                {
                    oversized++;
                    continue;
                }

                Transform root = _roots.GetOrCreateRoot(_chunks[0]);
                if (renderer.transform.parent == root)
                    continue;

                renderer.transform.SetParent(root, true);
                moved++;
            }

            Debug.Log($"[MoyvaMapChunks] Visual partition pass moved={moved}, multiChunk={multiChunk}, oversized={oversized}, scanned={_renderers.Count}.");
        }
    }
}
