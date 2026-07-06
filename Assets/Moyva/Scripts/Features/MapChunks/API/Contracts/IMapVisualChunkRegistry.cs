using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IMapVisualChunkRegistry
    {
        void Clear();
        void ResetVisibilityState();
        void Register(Renderer renderer, IReadOnlyList<MapChunkCoord> chunks);
        void SetCameraVisible(IReadOnlyCollection<MapChunkCoord> visibleChunks);
        void SetFogFullyHidden(MapChunkCoord coord, bool hidden);
        void ApplyVisibility();
    }
}
