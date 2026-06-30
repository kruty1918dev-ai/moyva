using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    [Obsolete("Use IFogVisualUpdater for TWC dual-grid fog volume visuals.")]
    public interface IFogTextureUpdater
    {
        void Initialize(int width, int height, Material fogMaterial);
        void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles);
        void RebuildFullTexture(IFogOfWarService fogService);
    }
}
