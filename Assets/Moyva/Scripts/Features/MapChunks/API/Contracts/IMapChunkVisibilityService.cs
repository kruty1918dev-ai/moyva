using System.Collections.Generic;

namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IMapChunkVisibilityService
    {
        void ResetVisibility();
        void SetCameraVisible(IReadOnlyCollection<MapChunkCoord> visibleChunks);
        void RefreshFogCoverage();
    }
}
