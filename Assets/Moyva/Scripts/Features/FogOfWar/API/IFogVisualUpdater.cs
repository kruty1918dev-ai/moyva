using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public interface IFogVisualUpdater
    {
        void Initialize(int width, int height, FogWorldVisualContext context);
        void SetWorldContext(FogWorldVisualContext context);
        void PreviewRevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible);
        void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles);
        void RebuildFullVisual(IFogOfWarService fogService);
    }
}
