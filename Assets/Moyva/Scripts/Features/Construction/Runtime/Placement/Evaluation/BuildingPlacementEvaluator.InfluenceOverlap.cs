using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public static partial class BuildingPlacementEvaluator
    {
        private static bool HasOverlappingInfluenceCenter(
            BuildingPlacementEvaluationRequest request,
            Vector2Int candidatePosition,
            int candidateRadius,
            out BuildingPlacementOverlap overlap)
        {
            overlap = default;
            if (candidateRadius <= 0)
                return false;

            IReadOnlyList<BuildingPlacementSimulationEntry> placedBuildings =
                request.PlacedBuildings;
            if (placedBuildings != null)
            {
                for (int index = 0; index < placedBuildings.Count; index++)
                {
                    BuildingPlacementSimulationEntry placed =
                        placedBuildings[index];
                    if (placed.Position == request.IgnoredOccupiedPosition)
                        continue;

                    BuildingDefinition definition =
                        request.BuildingRegistry.GetById(
                            placed.BuildingId);
                    if (!IsInfluenceCenter(definition))
                        continue;

                    int existingRadius = ResolveInfluenceRadius(
                        definition,
                        Mathf.Max(
                            0,
                            request.TownHallBuildRadius));
                    if (existingRadius <= 0
                        || GetChebyshevDistance(
                            placed.Position,
                            candidatePosition)
                            > candidateRadius + existingRadius)
                    {
                        continue;
                    }

                    overlap = new BuildingPlacementOverlap(
                        placed.Position,
                        placed.BuildingId,
                        existingRadius);
                    return true;
                }
            }
            else
            {
                int searchRadius =
                    candidateRadius
                    + ResolveMaxInfluenceRadius(request);
                for (int offsetX = -searchRadius;
                     offsetX <= searchRadius;
                     offsetX++)
                {
                    for (int offsetY = -searchRadius;
                         offsetY <= searchRadius;
                         offsetY++)
                    {
                        var centerPosition = new Vector2Int(
                            candidatePosition.x + offsetX,
                            candidatePosition.y + offsetY);
                        if (centerPosition
                            == request.IgnoredPendingPosition)
                        {
                            continue;
                        }

                        string occupantId =
                            GetOccupantId(
                                request,
                                centerPosition);
                        if (string.IsNullOrWhiteSpace(occupantId))
                            continue;

                        Vector2Int resolvedOrigin =
                            ResolveOccupantOrigin(
                                request,
                                centerPosition);
                        BuildingDefinition definition =
                            request.BuildingRegistry.GetById(
                                occupantId);
                        if (!IsInfluenceCenter(definition))
                            continue;

                        int existingRadius = ResolveInfluenceRadius(
                            definition,
                            Mathf.Max(
                                0,
                                request.TownHallBuildRadius));
                        if (existingRadius <= 0)
                            continue;

                        if (GetChebyshevDistance(
                                resolvedOrigin,
                                candidatePosition)
                            > candidateRadius + existingRadius)
                        {
                            continue;
                        }

                        overlap = new BuildingPlacementOverlap(
                            resolvedOrigin,
                            occupantId,
                            existingRadius);
                        return true;
                    }
                }
            }

            IReadOnlyList<BuildingPlacementSimulationEntry>
                pendingPlacements = request.PendingPlacements;
            if (pendingPlacements == null)
                return false;

            for (int index = 0;
                 index < pendingPlacements.Count;
                 index++)
            {
                BuildingPlacementSimulationEntry pending =
                    pendingPlacements[index];
                if (pending.Position == request.IgnoredPendingPosition)
                    continue;

                BuildingDefinition pendingDefinition =
                    request.BuildingRegistry.GetById(
                        pending.BuildingId);
                if (!IsInfluenceCenter(pendingDefinition))
                    continue;

                int existingRadius = ResolveInfluenceRadius(
                    pendingDefinition,
                    Mathf.Max(
                        0,
                        request.TownHallBuildRadius));
                if (existingRadius <= 0)
                    continue;

                if (GetChebyshevDistance(
                        pending.Position,
                        candidatePosition)
                    <= candidateRadius + existingRadius)
                {
                    overlap = new BuildingPlacementOverlap(
                        pending.Position,
                        pending.BuildingId,
                        existingRadius);
                    return true;
                }
            }

            return false;
        }

        private static int ResolveCoverageRadius(
            BuildingDefinition centerDefinition,
            int candidateLimit,
            int fallbackRadius)
        {
            int sourceRadius = ResolveInfluenceRadius(centerDefinition, Mathf.Max(0, fallbackRadius));
            if (sourceRadius <= 0)
                return 0;

            return candidateLimit > 0
                ? Mathf.Min(sourceRadius, candidateLimit)
                : sourceRadius;
        }

        private static int ResolvePlacedCenterSearchRadius(BuildingPlacementEvaluationRequest request, int candidateLimit)
        {
            int maxRadius = ResolveMaxInfluenceRadius(request);
            return candidateLimit > 0
                ? Mathf.Min(maxRadius, candidateLimit)
                : maxRadius;
        }

        private static int ResolveMaxInfluenceRadius(BuildingPlacementEvaluationRequest request)
        {
            if (request.MaxInfluenceRadius >= 0)
                return request.MaxInfluenceRadius;

            int maxRadius = 0;
            var definitions = request.BuildingRegistry.GetAll() ?? Array.Empty<BuildingDefinition>();
            for (int index = 0; index < definitions.Length; index++)
            {
                var definition = definitions[index];
                if (!IsInfluenceCenter(definition))
                    continue;

                maxRadius = Mathf.Max(maxRadius, ResolveInfluenceRadius(definition, request.TownHallBuildRadius));
            }

            return Mathf.Max(0, maxRadius);
        }

        private static int ResolveCandidateProximityLimit(BuildingDefinition candidate)
        {
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    candidate,
                    out SettlementInfluenceRequirementBuildingModule module)
                && module.MergeMode == PlacementRuleMergeMode.Override
                && module.MaximumDistanceToCenter > 0)
            {
                return module.MaximumDistanceToCenter;
            }

            return candidate != null && candidate.TownHallProximityRadiusOverride > 0
                ? candidate.TownHallProximityRadiusOverride
                : 0;
        }

        private readonly struct BuildingPlacementOverlap
        {
            public BuildingPlacementOverlap(Vector2Int position, string buildingId, int radius)
            {
                Position = position;
                BuildingId = buildingId;
                Radius = radius;
            }

            public Vector2Int Position { get; }
            public string BuildingId { get; }
            public int Radius { get; }
        }
    }
}
