using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public static partial class BuildingPlacementEvaluator
    {
        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        public static BuildingPlacementEvaluationResult Evaluate(BuildingPlacementEvaluationRequest request)
        {
            var result = new BuildingPlacementEvaluationResult();
            EvaluateCore(request, result);
            return result;
        }

        public static bool CanPlace(BuildingPlacementEvaluationRequest request)
        {
            return EvaluateCore(request, result: null);
        }

        private static bool EvaluateCore(
            BuildingPlacementEvaluationRequest request,
            BuildingPlacementEvaluationResult result)
        {
            if (request == null)
            {
                if (result != null)
                    result.ConfigurationBlocked = true;
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Configuration,
                    Message = "Placement request is null.",
                });
                return false;
            }

            BuildingDefinition definition = request.BuildingRegistry?.GetById(request.BuildingId);
            if (definition == null)
            {
                if (result != null)
                    result.ConfigurationBlocked = true;
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Configuration,
                    Message = $"Building definition '{request.BuildingId}' is missing.",
                    Position = request.Position,
                    BuildingId = request.BuildingId,
                });
                return false;
            }

            if (!BuildingFootprintUtility.TryValidate(definition, out string footprintConfigurationReason))
            {
                if (result != null)
                    result.ConfigurationBlocked = true;
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Configuration,
                    Message = footprintConfigurationReason,
                    Position = request.Position,
                    BuildingId = request.BuildingId,
                });
                return false;
            }

            int footprintCellCount = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            if (result != null)
            {
                for (int index = 0; index < footprintCellCount; index++)
                    result.AddFootprintPosition(BuildingFootprintUtility.GetOccupiedCell(
                        definition,
                        request.Position,
                        index,
                        request.Rotation));
            }

            bool footprintOutsideMap =
                IsFootprintOutsideMap(request, definition, result);
            if (result != null)
                result.TerrainBlocked = footprintOutsideMap;
            if (footprintOutsideMap)
                return false;

            bool tileOccupied = IsFootprintOccupied(request, definition, result);
            if (result != null)
                result.TileOccupied = tileOccupied;
            if (tileOccupied)
                return false;

            bool terrainBlocked = IsBlockedByResolvedTerrain(
                    request,
                    definition,
                    result)
                || IsBlockedByFootprintSlope(request, definition, result);
            if (result != null)
                result.TerrainBlocked = terrainBlocked;
            if (terrainBlocked)
                return false;

            bool adjacencyBlocked =
                IsBlockedByRequiredNeighborOffsets(
                    request,
                    definition,
                    result)
                || IsBlockedByTileRequirements(
                    request,
                    definition,
                    result);
            if (result != null)
                result.AdjacencyBlocked = adjacencyBlocked;
            if (adjacencyBlocked)
                return false;

            bool spacingBlocked = IsBlockedBySpacing(request, definition, result);
            if (result != null)
                result.SpacingBlocked = spacingBlocked;
            if (spacingBlocked)
                return false;

            bool fogBlocked = IsBlockedByFog(request, definition, result);
            if (result != null)
                result.FogBlocked = fogBlocked;
            if (fogBlocked)
                return false;

            bool influenceBlocked =
                IsBlockedByInfluenceZone(request, result);
            if (result != null)
                result.InfluenceZoneBlocked = influenceBlocked;
            if (influenceBlocked)
                return false;

            return !IsBlockedByRegisteredEvaluators(
                request,
                definition,
                result);
        }

        public static int ResolveInfluenceRadius(BuildingDefinition definition, int fallbackRadius)
        {
            return BuildingDefinitionCapabilities.GetInfluenceRadius(definition, fallbackRadius);
        }

        public static int GetChebyshevDistance(Vector2Int first, Vector2Int second)
        {
            return Mathf.Max(Mathf.Abs(first.x - second.x), Mathf.Abs(first.y - second.y));
        }

        public static bool IsInfluenceCenter(BuildingDefinition definition)
        {
            if (definition == null)
                return false;

            if (BuildingDefinitionCapabilities.HasEnabledModule<
                    SettlementCenterBuildingModule>(definition))
            {
                return true;
            }

            if (definition.PlacementRules != null)
                return definition.PlacementRules.CreatesSettlementInfluence;

            return false;
        }

        private static bool IsBlockedByRegisteredEvaluators(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            IReadOnlyList<IBuildingPlacementRuleEvaluator> evaluators =
                request.RuleEvaluators;
            if (evaluators == null)
                return false;

            for (int index = 0; index < evaluators.Count; index++)
            {
                IBuildingPlacementRuleEvaluator evaluator =
                    evaluators[index];
                if (evaluator == null)
                    continue;

                BuildingPlacementBlocker blocker;
                try
                {
                    blocker = evaluator.Evaluate(
                        request,
                        definition);
                }
                catch (Exception exception)
                {
                    blocker = new BuildingPlacementBlocker
                    {
                        Kind = BuildingPlacementBlockerKind.Configuration,
                        Message =
                            $"Placement evaluator '{evaluator.GetType().Name}' failed: {exception.Message}",
                        Position = request.Position,
                        BuildingId = request.BuildingId,
                    };
                }

                if (blocker == null)
                    continue;

                MarkBlocked(result, blocker.Kind);
                result?.AddBlocker(blocker);
                return true;
            }

            return false;
        }

        private static void MarkBlocked(
            BuildingPlacementEvaluationResult result,
            BuildingPlacementBlockerKind kind)
        {
            if (result == null)
                return;

            switch (kind)
            {
                case BuildingPlacementBlockerKind.OccupiedTile:
                    result.TileOccupied = true;
                    break;
                case BuildingPlacementBlockerKind.Spacing:
                    result.SpacingBlocked = true;
                    break;
                case BuildingPlacementBlockerKind.Fog:
                    result.FogBlocked = true;
                    break;
                case BuildingPlacementBlockerKind.InfluenceRequired:
                case BuildingPlacementBlockerKind.InfluenceOverlap:
                    result.InfluenceZoneBlocked = true;
                    break;
                case BuildingPlacementBlockerKind.Terrain:
                    result.TerrainBlocked = true;
                    break;
                case BuildingPlacementBlockerKind.Adjacency:
                    result.AdjacencyBlocked = true;
                    break;
                case BuildingPlacementBlockerKind.Configuration:
                case BuildingPlacementBlockerKind.Prerequisite:
                default:
                    result.ConfigurationBlocked = true;
                    break;
            }
        }

    }
}
