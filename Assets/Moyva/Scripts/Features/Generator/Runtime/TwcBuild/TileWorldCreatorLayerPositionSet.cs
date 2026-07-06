using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorLayerPositionSet
    {
        public Dictionary<string, HashSet<Vector2>> TerrainPositions { get; } = new();
        public Dictionary<string, HashSet<Vector2>> ObjectPositions { get; } = new();
        public Dictionary<string, HashSet<Vector2>> BuildingPositions { get; } = new();
        public HashSet<string> TerrainIds { get; } = new();
        public HashSet<string> ObjectIds { get; } = new();
        public HashSet<string> BuildingIds { get; } = new();

        public bool HasAnyMappedLayer =>
            TerrainPositions.Count > 0 || ObjectPositions.Count > 0 || BuildingPositions.Count > 0;
    }
}
