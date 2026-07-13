using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridTileCollector
    {
        void Collect(
            List<ConstructionBuildGridOverlayEntry> results,
            Func<Vector2Int, ConstructionBuildGridTileVisualState> resolveVisualState,
            out ConstructionBuildGridCollectionStats stats);
    }
}
