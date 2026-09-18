using System;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Converts stable placement reason codes to short player-facing source text (localized at render).
    /// Detailed spatial blockers remain available in BuildingPlacementEvaluationResult.
    /// </summary>
    public static class ConstructionPlacementReasonText
    {
        public static string Resolve(string reasonCode, string fallback = null)
        {
            switch ((reasonCode ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "allowed":
                    return null;
                case "resources":
                    return "Not enough resources.";
                case "authority":
                    return "Only the owner can perform this action.";
                case "occupied-tile":
                    return "The tile is already occupied.";
                case "spacing":
                    return "Too close to another building.";
                case "fog":
                    return "Scout this area first.";
                case "influence-required":
                    return "A settlement influence zone is required.";
                case "influence-overlap":
                    return "Settlement zones cannot overlap.";
                case "terrain":
                    return "Unsuitable terrain for this building.";
                case "adjacency":
                    return "Adjacent tile requirements are not met.";
                case "prerequisite":
                    return "Meet the building requirements first.";
                case "configuration":
                case "building-id-empty":
                    return "The building is configured incorrectly.";
                case "spatial-rules":
                    return "Building is not allowed here.";
                case "resource-context-deferred":
                    return "Cost will be checked after choosing a location.";
                default:
                    return string.IsNullOrWhiteSpace(fallback)
                        ? "Construction requirements are not met."
                        : fallback;
            }
        }

        public static string Resolve(BuildingPlacementBlockerKind kind, string fallback = null)
        {
            string reasonCode = kind switch
            {
                BuildingPlacementBlockerKind.OccupiedTile => "occupied-tile",
                BuildingPlacementBlockerKind.Spacing => "spacing",
                BuildingPlacementBlockerKind.Fog => "fog",
                BuildingPlacementBlockerKind.InfluenceRequired => "influence-required",
                BuildingPlacementBlockerKind.InfluenceOverlap => "influence-overlap",
                BuildingPlacementBlockerKind.Configuration => "configuration",
                BuildingPlacementBlockerKind.Terrain => "terrain",
                BuildingPlacementBlockerKind.Adjacency => "adjacency",
                BuildingPlacementBlockerKind.Prerequisite => "prerequisite",
                _ => null,
            };
            return Resolve(reasonCode, fallback);
        }
    }
}
