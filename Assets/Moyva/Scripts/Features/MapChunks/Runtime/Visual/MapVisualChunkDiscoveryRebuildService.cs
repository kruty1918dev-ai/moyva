using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapVisualChunkDiscoveryRebuildService : IMapVisualChunkDiscoveryRebuildService
    {
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapVisualChunkRegistry _registry;
        private readonly IMapVisualRendererCollector _collector;
        private readonly IMapVisualRendererFilter _filter;
        private readonly List<Renderer> _renderers = new(512);
        private readonly List<MapChunkCoord> _chunks = new(16);

        public MapVisualChunkDiscoveryRebuildService(
            IMapChunkLayoutService layout,
            IMapVisualChunkRegistry registry,
            IMapVisualRendererCollector collector,
            IMapVisualRendererFilter filter)
        {
            _layout = layout;
            _registry = registry;
            _collector = collector;
            _filter = filter;
        }

        public void Rebuild()
        {
            _registry.Clear();
            _collector.CollectPreferredRoots(_renderers);
            if (_renderers.Count == 0)
                _collector.CollectScene(_renderers);

            int registered = RegisterRenderers();
            Debug.Log($"[MoyvaMapChunks] Visual discovery registered renderers={registered}, scanned={_renderers.Count}.");
            _registry.ApplyVisibility();
            _renderers.Clear();
        }

        private int RegisterRenderers()
        {
            int registered = 0;
            for (int i = 0; i < _renderers.Count; i++)
            {
                var renderer = _renderers[i];
                if (!_filter.CanRegister(renderer))
                    continue;

                if (_layout.GetChunksOverlapping(renderer.bounds, _chunks) <= 0)
                    continue;

                _registry.Register(renderer, _chunks);
                registered++;
            }

            return registered;
        }
    }
}
