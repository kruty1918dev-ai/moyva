using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public static partial class BuildingPlacementEvaluator
    {
        private static bool HasInfluenceCenterCoveringPosition(
            BuildingPlacementEvaluationRequest request,
            Vector2Int position,
            BuildingDefinition candidate,
            out BuildingPlacementSimulationEntry? coveringCenter)
        {
            coveringCenter = null;
            int candidateLimit = ResolveCandidateProximityLimit(candidate);

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
                    if (!IsSameOwnerOrUnknown(
                            request.OwnerId,
                            placed.OwnerId))
                    {
                        continue;
                    }

                    BuildingDefinition definition =
                        request.BuildingRegistry.GetById(
                            placed.BuildingId);
                    if (!IsInfluenceCenter(definition))
                        continue;

                    int allowedRadius = ResolveCoverageRadius(
                        definition,
                        candidateLimit,
                        request.TownHallBuildRadius);
                    if (allowedRadius <= 0
                        || GetChebyshevDistance(
                            placed.Position,
                            position) > allowedRadius)
                    {
                        continue;
                    }

                    coveringCenter = placed;
                    return true;
                }
            }
            else
            {
                int searchRadius =
                    ResolvePlacedCenterSearchRadius(
                        request,
                        candidateLimit);
                for (int offsetX = -searchRadius;
                     offsetX <= searchRadius;
                     offsetX++)
                {
                    for (int offsetY = -searchRadius;
                         offsetY <= searchRadius;
                         offsetY++)
                    {
                        var centerPosition = new Vector2Int(
                            position.x + offsetX,
                            position.y + offsetY);
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
                        if (!IsOwnedByPlacementOwner(
                                request,
                                resolvedOrigin))
                        {
                            continue;
                        }

                        BuildingDefinition definition =
                            request.BuildingRegistry.GetById(
                                occupantId);
                        if (!IsInfluenceCenter(definition))
                            continue;

                        int allowedRadius =
                            ResolveCoverageRadius(
                                definition,
                                candidateLimit,
                                request.TownHallBuildRadius);
                        if (allowedRadius <= 0)
                            continue;

                        if (GetChebyshevDistance(
                                resolvedOrigin,
                                position) <= allowedRadius)
                        {
                            coveringCenter =
                                new BuildingPlacementSimulationEntry(
                                    resolvedOrigin,
                                    occupantId,
                                    request.GetOccupantOwnerId?.Invoke(
                                        resolvedOrigin));
                            return true;
                        }
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
                if (!IsSameOwnerOrUnknown(
                        request.OwnerId,
                        pending.OwnerId))
                {
                    continue;
                }

                BuildingDefinition pendingDefinition =
                    request.BuildingRegistry.GetById(
                        pending.BuildingId);
                if (!IsInfluenceCenter(pendingDefinition))
                    continue;

                int allowedRadius = ResolveCoverageRadius(
                    pendingDefinition,
                    candidateLimit,
                    request.TownHallBuildRadius);
                if (allowedRadius <= 0)
                    continue;

                if (GetChebyshevDistance(
                        pending.Position,
                        position) <= allowedRadius)
                {
                    coveringCenter = pending;
                    return true;
                }
            }

            return false;
        }

        private static bool HasInfluenceCenterTooClose(
            BuildingPlacementEvaluationRequest request,
            Vector2Int candidatePosition,
            BuildingDefinition candidate,
            out BuildingPlacementOverlap overlap)
        {
            overlap = default;
            int candidateMinimum =
                BuildingDefinitionCapabilities
                    .GetMinimumSettlementCenterDistance(candidate);

            IReadOnlyList<BuildingPlacementSimulationEntry> placed =
                request.PlacedBuildings;
            if (placed != null)
            {
                for (int index = 0; index < placed.Count; index++)
                {
                    BuildingPlacementSimulationEntry entry = placed[index];
                    if (entry.Position == request.IgnoredOccupiedPosition)
                        continue;

                    BuildingDefinition existing =
                        request.BuildingRegistry.GetById(entry.BuildingId);
                    if (!IsInfluenceCenter(existing))
                        continue;

                    int requiredDistance = Mathf.Max(
                        candidateMinimum,
                        BuildingDefinitionCapabilities
                            .GetMinimumSettlementCenterDistance(existing));
                    if (requiredDistance <= 0)
                        continue;

                    if (GetChebyshevDistance(
                            entry.Position,
                            candidatePosition)
                        < requiredDistance)
                    {
                        overlap = new BuildingPlacementOverlap(
                            entry.Position,
                            entry.BuildingId,
                            requiredDistance);
                        return true;
                    }
                }
            }
            else
            {
                // Compatibility fallback for tests/external callers that do not
                // provide the Pass-2 placed-building snapshot. Runtime service
                // does provide it, so gameplay avoids this radius^2 path.
                int searchRadius = candidateMinimum;
                BuildingDefinition[] definitions =
                    request.BuildingRegistry?.GetAll()
                    ?? Array.Empty<BuildingDefinition>();
                for (int index = 0; index < definitions.Length; index++)
                {
                    BuildingDefinition definition = definitions[index];
                    if (IsInfluenceCenter(definition))
                    {
                        searchRadius = Mathf.Max(
                            searchRadius,
                            BuildingDefinitionCapabilities
                                .GetMinimumSettlementCenterDistance(definition));
                    }
                }

                for (int offsetX = -searchRadius;
                     offsetX <= searchRadius;
                     offsetX++)
                {
                    for (int offsetY = -searchRadius;
                         offsetY <= searchRadius;
                         offsetY++)
                    {
                        Vector2Int probe = new Vector2Int(
                            candidatePosition.x + offsetX,
                            candidatePosition.y + offsetY);
                        string occupantId = GetOccupantId(request, probe);
                        if (string.IsNullOrWhiteSpace(occupantId))
                            continue;

                        Vector2Int origin = ResolveOccupantOrigin(request, probe);
                        if (origin == request.IgnoredOccupiedPosition)
                            continue;

                        BuildingDefinition existing =
                            request.BuildingRegistry.GetById(occupantId);
                        if (!IsInfluenceCenter(existing))
                            continue;

                        int requiredDistance = Mathf.Max(
                            candidateMinimum,
                            BuildingDefinitionCapabilities
                                .GetMinimumSettlementCenterDistance(existing));
                        if (requiredDistance > 0
                            && GetChebyshevDistance(origin, candidatePosition)
                                < requiredDistance)
                        {
                            overlap = new BuildingPlacementOverlap(
                                origin,
                                occupantId,
                                requiredDistance);
                            return true;
                        }
                    }
                }
            }

            IReadOnlyList<BuildingPlacementSimulationEntry> pending =
                request.PendingPlacements;
            if (pending == null)
                return false;

            for (int index = 0; index < pending.Count; index++)
            {
                BuildingPlacementSimulationEntry entry = pending[index];
                if (entry.Position == request.IgnoredPendingPosition)
                    continue;

                BuildingDefinition existing =
                    request.BuildingRegistry.GetById(entry.BuildingId);
                if (!IsInfluenceCenter(existing))
                    continue;

                int requiredDistance = Mathf.Max(
                    candidateMinimum,
                    BuildingDefinitionCapabilities
                        .GetMinimumSettlementCenterDistance(existing));
                if (requiredDistance <= 0)
                    continue;

                if (GetChebyshevDistance(
                        entry.Position,
                        candidatePosition)
                    < requiredDistance)
                {
                    overlap = new BuildingPlacementOverlap(
                        entry.Position,
                        entry.BuildingId,
                        requiredDistance);
                    return true;
                }
            }

            return false;
        }

    }
}
