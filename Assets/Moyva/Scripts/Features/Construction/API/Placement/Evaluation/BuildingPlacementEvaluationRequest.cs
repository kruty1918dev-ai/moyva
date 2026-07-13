using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public sealed class BuildingPlacementEvaluationRequest
    {
        public IBuildingRegistry BuildingRegistry;
        public string BuildingId;
        public Vector2Int Position;
        public Vector2Int? IgnoredPendingPosition;
        public Vector2Int? IgnoredOccupiedPosition;
        public int MinSpacing;
        public int TownHallBuildRadius;
        public Func<Vector2Int, bool> IsOccupied;
        public Func<Vector2Int, string> GetOccupantId;
        public Func<Vector2Int, Vector2Int?> GetOccupantOrigin;
        public Func<Vector2Int, bool> IsFogBlocked;
        public Func<Vector2Int, bool> IsTerrainBlocked;
        public Func<Vector2Int, int?> GetTerrainLevel;
        public Func<Vector2Int, string> GetTileId;
        public IReadOnlyList<BuildingPlacementSimulationEntry> PendingPlacements;
        public bool SkipInfluenceRules;
    }
}
