using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphTwcMapDataState : IGraphTwcMapDataState
    {
        private readonly IGraphTwcMapDataEnvironment _environment;
        private readonly IGraphTwcSeedService _seedService;
        private bool _hasLastBaseMapWorldBounds;
        private Bounds _lastBaseMapWorldBounds;

        public GraphTwcMapDataState(
            IGraphTwcMapDataEnvironment environment,
            IGraphTwcSeedService seedService)
        {
            _environment = environment;
            _seedService = seedService;
        }

        public IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; private set; }
        public float LastCellSize { get; private set; } = 1f;
        public string DiagnosticGraphName => _environment.Graph != null ? _environment.Graph.name : "null";
        public bool HasGraphAsset => _environment.Graph != null;
        public bool HasSharedMapSize => _environment.Graph?.SharedSettings != null && _environment.Graph.SharedSettings.HasMapSize;
        public Vector2Int DiagnosticSharedMapSize => _environment.Graph?.SharedSettings?.MapSize ?? Vector2Int.zero;
        public int DiagnosticSeed => _seedService.Resolve(_environment.Graph);
        public bool HasTileWorldCreatorManager => _environment.Manager != null;

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
            LastCellSize = result.CellSize > 0.0001f ? result.CellSize : 1f;
            _hasLastBaseMapWorldBounds = result.HasBaseMapWorldBounds;
            _lastBaseMapWorldBounds = result.BaseMapWorldBounds;
        }
    }
}
