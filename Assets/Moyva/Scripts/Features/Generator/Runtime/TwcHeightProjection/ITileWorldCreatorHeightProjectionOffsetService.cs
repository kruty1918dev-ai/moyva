using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorHeightProjectionOffsetService
    {
        TileWorldCreatorHeightProjectionStats ApplyOffsets(
            TileWorldCreatorHeightProjectionState state,
            Transform root,
            int width,
            int height,
            float minX,
            float minZ);
    }
}
