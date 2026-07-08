using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IMapVisualChunkRegistry
    {
        void Clear();
        void ResetVisibilityState();
        void Register(Renderer renderer, IReadOnlyList<MapChunkCoord> chunks);
        int CameraVisibilityVersion { get; }
        void SetCameraVisible(IReadOnlyCollection<MapChunkCoord> visibleChunks);
        bool IsCameraVisible(MapChunkCoord coord);
        void SetFogFullyHidden(MapChunkCoord coord, bool hidden);
        void ApplyVisibility();
    }
}
