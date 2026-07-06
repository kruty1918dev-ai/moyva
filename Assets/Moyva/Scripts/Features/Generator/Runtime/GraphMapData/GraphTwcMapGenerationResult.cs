using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphTwcMapGenerationResult
    {
        public string[,] BiomeMap;
        public string[,] ObjectMap;
        public float[,] HeightMap;
        public string[,] BuildingMap;
        public IReadOnlyList<CompiledLayerMap> CompiledLayers;
        public float CellSize = 1f;
        public bool HasBaseMapWorldBounds;
        public Bounds BaseMapWorldBounds;
    }
}
