using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTileTransformCollector
    {
        int Collect(Transform root, List<TileWorldCreatorTileTransformSample> buffer, HashSet<EntityId> collectedIds, out int skippedSideWallRenderers);
    }
}
