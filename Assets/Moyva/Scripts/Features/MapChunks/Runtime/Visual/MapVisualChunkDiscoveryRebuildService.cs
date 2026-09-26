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
        private readonly IMapVisualChunkRootService _roots;
        private readonly List<Renderer> _renderers = new(512);
        private readonly List<MapChunkCoord> _chunks = new(16);
        private readonly List<MapChunkCoord> _singleChunk = new(1);

        public MapVisualChunkDiscoveryRebuildService(
            IMapChunkLayoutService layout,
            IMapVisualChunkRegistry registry,
            IMapVisualRendererCollector collector,
            IMapVisualRendererFilter filter,
            [Zenject.InjectOptional] IMapVisualChunkRootService roots = null)
        {
            _layout = layout;
            _registry = registry;
            _collector = collector;
            _filter = filter;
            _roots = roots;
        }

        public void Rebuild()
        {
            _registry.Clear();
            _collector.CollectPreferredRoots(_renderers);
            if (_renderers.Count == 0)
                _collector.CollectScene(_renderers);

            int registered = RegisterRenderers();
            _registry.ApplyVisibility();
            _renderers.Clear();
        }

        /*
         * Renderers already parented under a MapChunk_X_Y root (terrain
         * meshes, spawned props) belong to exactly that chunk. Registering
         * them by full bounds overlap would let a wide canopy stay visible
         * while its supporting chunk is culled or fog-hidden — the floating-
         * decor symptom. Bounds overlap remains only for loose scene
         * renderers outside the chunk hierarchy.
         */
        private int RegisterRenderers()
        {
            int registered = 0;
            for (int i = 0; i < _renderers.Count; i++)
            {
                var renderer = _renderers[i];
                if (!_filter.CanRegister(renderer))
                    continue;

                if (_roots != null
                    && _roots.TryGetOwnedChunk(renderer.transform, out MapChunkCoord owned))
                {
                    _singleChunk.Clear();
                    _singleChunk.Add(owned);
                    _registry.Register(renderer, _singleChunk);
                    registered++;
                    continue;
                }

                if (_layout.GetChunksOverlapping(renderer.bounds, _chunks) <= 0)
                    continue;

                _registry.Register(renderer, _chunks);
                registered++;
            }

            return registered;
        }
    }
}
