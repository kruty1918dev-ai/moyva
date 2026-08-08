using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Extension point for independently registered placement policies.
    /// Return null when the rule allows placement, or a blocker to stop the
    /// pipeline. Evaluators are invoked in registration order.
    /// </summary>
    public interface IBuildingPlacementRuleEvaluator
    {
        BuildingPlacementBlocker Evaluate(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition);
    }

    public sealed class BuildingPlacementEvaluationRequest
    {
        public IBuildingRegistry BuildingRegistry;
        public string BuildingId;
        public string OwnerId;
        public Vector2Int Position;
        public Vector2Int? IgnoredPendingPosition;
        public Vector2Int? IgnoredOccupiedPosition;
        public int MinSpacing;
        public int TownHallBuildRadius;
        public Func<Vector2Int, bool> IsOccupied;
        public Func<Vector2Int, bool> TileExists;
        public Func<Vector2Int, string> GetOccupantId;
        public Func<Vector2Int, Vector2Int?> GetOccupantOrigin;
        public Func<Vector2Int, string> GetOccupantOwnerId;
        public Func<Vector2Int, bool> IsFogBlocked;
        public Func<Vector2Int, FogStateType?> GetFogState;
        public Func<Vector2Int, bool> IsTerrainBlocked;
        public Func<Vector2Int, int?> GetTerrainLevel;
        public Func<Vector2Int, string> GetTileId;
        public Func<Vector2Int, string, bool> HasTerrainTag;
        public IReadOnlyList<BuildingPlacementSimulationEntry> PendingPlacements;
        public IReadOnlyList<BuildingPlacementSimulationEntry> PlacedBuildings;
        public HashSet<Vector2Int> TileMatchWorkspace;
        public bool? HasInfluenceCenterDefinitions;
        public int MaxInfluenceRadius = -1;
        public IReadOnlyList<IBuildingPlacementRuleEvaluator> RuleEvaluators;
        public bool SkipInfluenceRules;
    }
}
