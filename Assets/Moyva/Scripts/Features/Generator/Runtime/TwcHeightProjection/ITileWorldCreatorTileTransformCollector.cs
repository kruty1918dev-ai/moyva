using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTileTransformCollector
    {
        int Collect(Transform root, List<TileWorldCreatorTileTransformSample> buffer, HashSet<int> collectedIds, out int skippedSideWallRenderers);
    }
}
