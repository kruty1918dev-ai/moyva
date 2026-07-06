using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using Zenject;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapChunkVisibilityService : IMapChunkVisibilityService
    {
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapVisualChunkRegistry _registry;
        private readonly IMapFogChunkCoverageService _fogCoverage;

        public MapChunkVisibilityService(
            IMapChunkLayoutService layout,
            IMapVisualChunkRegistry registry,
            [InjectOptional] IMapFogChunkCoverageService fogCoverage = null)
        {
            _layout = layout;
            _registry = registry;
            _fogCoverage = fogCoverage;
        }

        public void ResetVisibility()
        {
            _registry.Clear();
            _registry.ResetVisibilityState();
        }

        public void SetCameraVisible(IReadOnlyCollection<MapChunkCoord> visibleChunks)
        {
            _registry.SetCameraVisible(visibleChunks);
        }

        public void RefreshFogCoverage()
        {
            if (!_layout.IsConfigured || _fogCoverage == null)
                return;

            foreach (var descriptor in _layout.Chunks)
                _registry.SetFogFullyHidden(descriptor.Coord, _fogCoverage.IsChunkFullyHidden(descriptor));

            _registry.ApplyVisibility();
        }
    }
}
