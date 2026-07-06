using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcMapDataDiagnostics
    {
        IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; }
        float LastCellSize { get; }
        string DiagnosticGraphName { get; }
        bool HasGraphAsset { get; }
        bool HasSharedMapSize { get; }
        Vector2Int DiagnosticSharedMapSize { get; }
        int DiagnosticSeed { get; }
        bool HasTileWorldCreatorManager { get; }
        bool TryGetLastBaseMapWorldBounds(out Bounds bounds);
    }
}
