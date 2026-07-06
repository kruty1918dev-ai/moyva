using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct TileWorldCreatorWorldBuildResult
    {
        private readonly HashSet<string> _terrainIds;
        private readonly HashSet<string> _objectIds;
        private readonly HashSet<string> _buildingIds;

        public static TileWorldCreatorWorldBuildResult Disabled => new TileWorldCreatorWorldBuildResult(
            null,
            null,
            null,
            false,
            false,
            false,
            false,
            1f,
            false,
            default);

        public TileWorldCreatorWorldBuildResult(
            HashSet<string> terrainIds,
            HashSet<string> objectIds,
            HashSet<string> buildingIds,
            bool replaceTerrainVisuals,
            bool replaceObjectVisuals,
            bool replaceBuildingVisuals,
            bool suppressMoyvaLayerData,
            float cellSize,
            bool hasBaseMapWorldBounds,
            Bounds baseMapWorldBounds)
        {
            _terrainIds = terrainIds;
            _objectIds = objectIds;
            _buildingIds = buildingIds;
            ReplaceMappedTerrainVisuals = replaceTerrainVisuals;
            ReplaceMappedObjectVisuals = replaceObjectVisuals;
            ReplaceMappedBuildingVisuals = replaceBuildingVisuals;
            SuppressMoyvaLayerData = suppressMoyvaLayerData;
            CellSize = cellSize > 0.0001f ? cellSize : 1f;
            HasBaseMapWorldBounds = hasBaseMapWorldBounds;
            BaseMapWorldBounds = baseMapWorldBounds;
        }

        public bool ReplaceMappedTerrainVisuals { get; }
        public bool ReplaceMappedObjectVisuals { get; }
        public bool ReplaceMappedBuildingVisuals { get; }
        public bool SuppressMoyvaLayerData { get; }
        public float CellSize { get; }
        public bool HasBaseMapWorldBounds { get; }
        public Bounds BaseMapWorldBounds { get; }

        public bool ShouldReplaceTerrainVisual(string id)
            => ReplaceMappedTerrainVisuals && Contains(_terrainIds, id);

        public bool ShouldReplaceObjectVisual(string id)
            => ReplaceMappedObjectVisuals && Contains(_objectIds, id);

        public bool ShouldReplaceBuildingVisual(string id)
            => ReplaceMappedBuildingVisuals && Contains(_buildingIds, id);

        public static TileWorldCreatorWorldBuildResult FromPositions(
            TileWorldCreatorLayerPositionSet positions,
            TileWorldCreatorBuildOptions options,
            float cellSize,
            bool hasBounds,
            Bounds baseMapWorldBounds)
            => new TileWorldCreatorWorldBuildResult(
                positions.TerrainIds,
                positions.ObjectIds,
                positions.BuildingIds,
                options.ReplaceMappedTerrainVisuals,
                options.ReplaceMappedObjectVisuals,
                options.ReplaceMappedBuildingVisuals,
                options.SuppressMoyvaLayerDataWhenTerrainMapped && positions.TerrainIds.Count > 0,
                cellSize,
                hasBounds,
                baseMapWorldBounds);

        private static bool Contains(HashSet<string> ids, string id)
            => ids != null && !string.IsNullOrWhiteSpace(id) && ids.Contains(id);
    }
}
