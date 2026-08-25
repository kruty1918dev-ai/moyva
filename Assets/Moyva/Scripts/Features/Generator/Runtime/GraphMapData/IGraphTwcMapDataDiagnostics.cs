using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcMapDataDiagnostics
    {
        IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; }
        GraphLogicalTileMap LastLogicalMap { get; }
        float LastCellSize { get; }
        bool TryGetLastBaseMapWorldBounds(out Bounds bounds);
    }
}
