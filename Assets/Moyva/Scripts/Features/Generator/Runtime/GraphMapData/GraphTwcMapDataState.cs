using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphTwcMapDataState : IGraphTwcMapDataState
    {
        private bool _hasLastBaseMapWorldBounds;
        private Bounds _lastBaseMapWorldBounds;

        public IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; private set; }
        public GraphLogicalTileMap LastLogicalMap { get; private set; }
        public float LastCellSize { get; private set; } = 1f;
        public bool TryGetLastBaseMapWorldBounds(out Bounds bounds)
        {
            bounds = _lastBaseMapWorldBounds;
            return _hasLastBaseMapWorldBounds;
        }

        public void Apply(GraphTwcMapGenerationResult result)
        {
            if (result == null)
                return;

            if (result.CompiledLayers != null)
                LastCompiledLayers = result.CompiledLayers;
            LastLogicalMap = result.LogicalMap;
            LastCellSize = result.CellSize > 0.0001f ? result.CellSize : 1f;
            _hasLastBaseMapWorldBounds = result.HasBaseMapWorldBounds;
            _lastBaseMapWorldBounds = result.BaseMapWorldBounds;
        }
    }
}
