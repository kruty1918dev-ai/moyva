using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Owns settlement/influence-zone placement policy.
    ///
    /// It reads building definitions plus caller-supplied pending/placed
    /// placement snapshots. It does not own construction mutation state.
    /// </summary>
    internal sealed class ConstructionInfluencePolicy
    {
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly int _defaultInfluenceRadius;
        private readonly IConstructionPlacementRulesProvider
            _placementRulesProvider;

        private int _cachedMaxInfluenceRadius = -1;
        private int _cachedInfluenceCenterDefinitionState = -1;

        public ConstructionInfluencePolicy(
            IBuildingRegistry buildingRegistry,
            int defaultInfluenceRadius,
            IConstructionPlacementRulesProvider placementRulesProvider)
        {
            _buildingRegistry = buildingRegistry;
            _defaultInfluenceRadius =
                Mathf.Max(0, defaultInfluenceRadius);
            _placementRulesProvider = placementRulesProvider;
        }

        public bool IsBlocked(
            Vector2Int position,
            string buildingId,
            Vector2Int? ignoredPendingPosition,
            IReadOnlyList<BuildingPlacementSimulationEntry>
                pendingPlacements,
            IReadOnlyList<BuildingPlacementSimulationEntry>
                placedBuildings)
        {
            if (_placementRulesProvider != null
                && !_placementRulesProvider.EnableInfluenceZoneRules)
                return false;

            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            if (_buildingRegistry == null)
            {
                Debug.LogError(
                    "[Construction] IsBlockedByInfluenceZone: _buildingRegistry == null");
                return false;
            }

            BuildingDefinition candidate =
                _buildingRegistry.GetById(buildingId);
            if (candidate == null)
                return false;

            if (!HasAnyInfluenceCenterDefinition())
                return false;

            int ruleRadius = IsInfluenceCenter(candidate)
                ? ResolveInfluenceRadius(candidate)
                : ResolveMaxInfluenceRadius();

            if (ruleRadius <= 0)
                return false;

            bool hasInfluenceCenterInRange =
                HasInfluenceCenterCoveringPosition(
                    position,
                    candidate,
                    ignoredPendingPosition,
                    pendingPlacements,
                    placedBuildings);

            bool requireInfluenceCenterInRange;
            bool blockWhenInfluenceCenterExists;

            if (candidate.UseCustomTownHallRules)
            {
                requireInfluenceCenterInRange =
                    candidate.RequireTownHallInRange;
                blockWhenInfluenceCenterExists =
                    candidate.BlockIfTownHallAlreadyInRange;
            }
            else
            {
                bool isInfluenceCenter =
                    IsInfluenceCenter(candidate);
                requireInfluenceCenterInRange =
                    !isInfluenceCenter;
                blockWhenInfluenceCenterExists =
                    isInfluenceCenter;
            }

            if (requireInfluenceCenterInRange
                && !hasInfluenceCenterInRange)
                return true;

            int candidateInfluenceRadius =
                ResolveInfluenceRadius(candidate);

            if (blockWhenInfluenceCenterExists
                && HasOverlappingInfluenceCenter(
                    position,
                    candidateInfluenceRadius,
                    ignoredPendingPosition,
                    pendingPlacements,
                    placedBuildings,
                    out _,
                    out _,
                    out _))
                return true;

            return false;
        }

        public bool HasAnyInfluenceCenterDefinition()
        {
            if (_cachedInfluenceCenterDefinitionState >= 0)
                return _cachedInfluenceCenterDefinitionState == 1;

            bool found = false;
            BuildingDefinition[] definitions =
                _buildingRegistry?.GetAll()
                ?? Array.Empty<BuildingDefinition>();

            for (int index = 0;
                 index < definitions.Length;
                 index++)
            {
                if (!IsInfluenceCenter(definitions[index]))
                    continue;

                found = true;
                break;
            }

            _cachedInfluenceCenterDefinitionState =
                found ? 1 : 0;
            return found;
        }

        public int ResolveMaxInfluenceRadius()
        {
            if (_cachedMaxInfluenceRadius >= 0)
                return _cachedMaxInfluenceRadius;

            int maxRadius = _defaultInfluenceRadius;
            BuildingDefinition[] definitions =
                _buildingRegistry?.GetAll()
                ?? Array.Empty<BuildingDefinition>();

            for (int index = 0;
                 index < definitions.Length;
                 index++)
            {
                BuildingDefinition definition =
                    definitions[index];

                if (!IsInfluenceCenter(definition))
                    continue;

                maxRadius = Mathf.Max(
                    maxRadius,
                    ResolveInfluenceRadius(definition));
            }

            _cachedMaxInfluenceRadius =
                Mathf.Max(0, maxRadius);
            return _cachedMaxInfluenceRadius;
        }

        private bool HasInfluenceCenterCoveringPosition(
            Vector2Int position,
            BuildingDefinition candidate,
            Vector2Int? ignoredPendingPosition,
            IReadOnlyList<BuildingPlacementSimulationEntry>
                pendingPlacements,
            IReadOnlyList<BuildingPlacementSimulationEntry>
                placedBuildings)
        {
            int candidateLimit =
                ResolveCandidateProximityLimit(candidate);

            if (HasPlacedInfluenceCenter(
                    position,
                    ignoredPendingPosition,
                    candidateLimit,
                    placedBuildings))
            {
                return true;
            }

            if (pendingPlacements == null)
                return false;

            for (int index = 0;
                 index < pendingPlacements.Count;
                 index++)
            {
                BuildingPlacementSimulationEntry pending =
                    pendingPlacements[index];

                if (pending.Position == ignoredPendingPosition)
                    continue;

                BuildingDefinition pendingDefinition =
                    _buildingRegistry.GetById(
                        pending.BuildingId);

                if (!IsInfluenceCenter(pendingDefinition))
                    continue;

                int allowedRadius =
                    ResolveCoverageRadius(
                        pendingDefinition,
                        candidateLimit);

                if (allowedRadius > 0
                    && GetChebyshevDistance(
                        pending.Position,
                        position) <= allowedRadius)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasPlacedInfluenceCenter(
            Vector2Int position,
            Vector2Int? ignoredPosition,
            int candidateLimit,
            IReadOnlyList<BuildingPlacementSimulationEntry>
                placedBuildings)
        {
            if (placedBuildings == null)
                return false;

            for (int index = 0;
                 index < placedBuildings.Count;
                 index++)
            {
                BuildingPlacementSimulationEntry placed =
                    placedBuildings[index];

                if (placed.Position == ignoredPosition)
                    continue;

                BuildingDefinition definition =
                    _buildingRegistry.GetById(
                        placed.BuildingId);

                if (!IsInfluenceCenter(definition))
                    continue;

                int allowedRadius =
                    ResolveCoverageRadius(
                        definition,
                        candidateLimit);

                if (allowedRadius > 0
                    && GetChebyshevDistance(
                        placed.Position,
                        position) <= allowedRadius)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasOverlappingInfluenceCenter(
            Vector2Int candidatePosition,
            int candidateRadius,
            Vector2Int? ignoredPosition,
            IReadOnlyList<BuildingPlacementSimulationEntry>
                pendingPlacements,
            IReadOnlyList<BuildingPlacementSimulationEntry>
                placedBuildings,
            out Vector2Int overlappingPosition,
            out string overlappingBuildingId,
            out int overlappingRadius)
        {
            overlappingPosition = default;
            overlappingBuildingId = null;
            overlappingRadius = 0;

            if (candidateRadius <= 0)
                return false;

            if (TryFindOverlap(
                    candidatePosition,
                    candidateRadius,
                    ignoredPosition,
                    placedBuildings,
                    out overlappingPosition,
                    out overlappingBuildingId,
                    out overlappingRadius))
            {
                return true;
            }

            return TryFindOverlap(
                candidatePosition,
                candidateRadius,
                ignoredPosition,
                pendingPlacements,
                out overlappingPosition,
                out overlappingBuildingId,
                out overlappingRadius);
        }

        private bool TryFindOverlap(
            Vector2Int candidatePosition,
            int candidateRadius,
            Vector2Int? ignoredPosition,
            IReadOnlyList<BuildingPlacementSimulationEntry>
                placements,
            out Vector2Int overlappingPosition,
            out string overlappingBuildingId,
            out int overlappingRadius)
        {
            overlappingPosition = default;
            overlappingBuildingId = null;
            overlappingRadius = 0;

            if (placements == null)
                return false;

            for (int index = 0;
                 index < placements.Count;
                 index++)
            {
                BuildingPlacementSimulationEntry placement =
                    placements[index];

                if (placement.Position == ignoredPosition)
                    continue;

                BuildingDefinition definition =
                    _buildingRegistry.GetById(
                        placement.BuildingId);

                if (!IsInfluenceCenter(definition))
                    continue;

                int existingRadius =
                    ResolveInfluenceRadius(definition);

                if (existingRadius <= 0
                    || GetChebyshevDistance(
                        placement.Position,
                        candidatePosition)
                        > candidateRadius + existingRadius)
                {
                    continue;
                }

                overlappingPosition = placement.Position;
                overlappingBuildingId =
                    placement.BuildingId;
                overlappingRadius = existingRadius;
                return true;
            }

            return false;
        }

        private int ResolveCoverageRadius(
            BuildingDefinition centerDefinition,
            int candidateLimit)
        {
            int sourceRadius =
                ResolveInfluenceRadius(centerDefinition);

            if (sourceRadius <= 0)
                return 0;

            return candidateLimit > 0
                ? Mathf.Min(sourceRadius, candidateLimit)
                : sourceRadius;
        }

        private int ResolveCandidateProximityLimit(
            BuildingDefinition candidate)
        {
            return candidate != null
                   && candidate.TownHallProximityRadiusOverride > 0
                ? candidate.TownHallProximityRadiusOverride
                : 0;
        }

        private int ResolveInfluenceRadius(
            BuildingDefinition definition)
        {
            return BuildingDefinitionCapabilities
                .GetInfluenceRadius(
                    definition,
                    _defaultInfluenceRadius);
        }

        private static int GetChebyshevDistance(
            Vector2Int a,
            Vector2Int b)
        {
            return Mathf.Max(
                Mathf.Abs(a.x - b.x),
                Mathf.Abs(a.y - b.y));
        }

        private static bool IsInfluenceCenter(
            BuildingDefinition definition)
        {
            return BuildingDefinitionCapabilities
                .IsSettlementCenter(definition);
        }
    }
}
