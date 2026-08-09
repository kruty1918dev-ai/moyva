using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private bool IsBlockedByInfluenceZone(Vector2Int position, string buildingId, Vector2Int? ignoredPendingPosition)
        {
            if (_placementRulesProvider != null && !_placementRulesProvider.EnableInfluenceZoneRules)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone({position}, {buildingId}): profile disabled influence rule");
                return false;
            }

            if (string.IsNullOrWhiteSpace(buildingId))
            {
                Debug.LogWarning("[Construction] IsBlockedByInfluenceZone: buildingId порожній");
                return false;
            }

            if (_buildingRegistry == null)
            {
                Debug.LogError("[Construction] IsBlockedByInfluenceZone: _buildingRegistry == null");
                return false;
            }

            var candidate = _buildingRegistry.GetById(buildingId);
            if (candidate == null)
            {
                Debug.LogWarning($"[Construction] IsBlockedByInfluenceZone: будівля '{buildingId}' не знайдена у реєстрі");
                return false;
            }

            bool anyInfluenceCenterDefined =
                HasAnyInfluenceCenterDefinition();
            if (!anyInfluenceCenterDefined)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] IsBlockedByInfluenceZone: RuleDisabled - немає центру поселення з SettlementCenterBuildingModule у реєстрі");
                return false;
            }

            int ruleRadius = IsInfluenceCenter(candidate)
                ? ResolveInfluenceRadius(candidate)
                : ResolveMaxInfluenceRadius();
            if (ruleRadius <= 0)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone: ruleRadius <= 0 ({ruleRadius}) - правило відключено");
                return false;
            }

            bool hasInfluenceCenterInRange = HasInfluenceCenterCoveringPosition(position, candidate, ignoredPendingPosition);
            if (VerboseLogs)
                Debug.Log($"[Construction] IsBlockedByInfluenceZone({position}, {buildingId}): ruleRadius={ruleRadius}, hasInfluenceCenterInRange={hasInfluenceCenterInRange}");

            bool requireInfluenceCenterInRange;
            bool blockWhenInfluenceCenterExists;

            if (candidate.UseCustomTownHallRules)
            {
                requireInfluenceCenterInRange = candidate.RequireTownHallInRange;
                blockWhenInfluenceCenterExists = candidate.BlockIfTownHallAlreadyInRange;

                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone: CustomRules - require={requireInfluenceCenterInRange}, blockWhenExists={blockWhenInfluenceCenterExists}");
            }
            else
            {
                bool isInfluenceCenter = IsInfluenceCenter(candidate);
                requireInfluenceCenterInRange = !isInfluenceCenter;
                blockWhenInfluenceCenterExists = isInfluenceCenter;

                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone: DefaultRules - isInfluenceCenter={isInfluenceCenter}, require={requireInfluenceCenterInRange}, blockWhenExists={blockWhenInfluenceCenterExists}");
            }

            if (requireInfluenceCenterInRange && !hasInfluenceCenterInRange)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone: BLOCKED - потрібен центр поселення у радіусі {ruleRadius}");
                return true;
            }

            int candidateInfluenceRadius = ResolveInfluenceRadius(candidate);
            if (blockWhenInfluenceCenterExists && HasOverlappingInfluenceCenter(position, candidateInfluenceRadius, ignoredPendingPosition, out var overlapPosition, out var overlapBuildingId, out var overlapRadius))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone: BLOCKED - зона '{buildingId}' radius={candidateInfluenceRadius} перетинається з '{overlapBuildingId}' на {overlapPosition} radius={overlapRadius}");
                return true;
            }

            if (VerboseLogs)
                Debug.Log("[Construction] IsBlockedByInfluenceZone: ALLOWED");

            return false;
        }

        private bool HasInfluenceCenterCoveringPosition(Vector2Int position, BuildingDefinition candidate, Vector2Int? ignoredPendingPosition)
        {
            int candidateLimit = ResolveCandidateProximityLimit(candidate);
            if (HasPlacedInfluenceCenter(position, ignoredPendingPosition, candidateLimit))
                return true;

            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                var pending = _pendingPlacements[i];
                if (pending.Position == ignoredPendingPosition)
                    continue;

                var pendingDef = _buildingRegistry.GetById(pending.BuildingId);
                if (!IsInfluenceCenter(pendingDef))
                    continue;

                int allowedRadius = ResolveCoverageRadius(pendingDef, candidateLimit);
                if (allowedRadius > 0 && GetChebyshevDistance(pending.Position, position) <= allowedRadius)
                    return true;
            }

            return false;
        }

        private bool HasPlacedInfluenceCenter(Vector2Int position, Vector2Int? ignoredPendingPosition, int candidateLimit)
        {
            foreach (var pair in _factionPlacedBuildings)
            {
                if (pair.Key == ignoredPendingPosition)
                    continue;

                BuildingDefinition definition =
                    _buildingRegistry.GetById(pair.Value.BuildingId);
                if (!IsInfluenceCenter(definition))
                    continue;

                int allowedRadius =
                    ResolveCoverageRadius(definition, candidateLimit);
                if (allowedRadius > 0
                    && GetChebyshevDistance(pair.Key, position)
                        <= allowedRadius)
                {
                    return true;
                }
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (pair.Key == ignoredPendingPosition)
                    continue;

                BuildingDefinition definition =
                    _buildingRegistry.GetById(pair.Value);
                if (!IsInfluenceCenter(definition))
                    continue;

                int allowedRadius =
                    ResolveCoverageRadius(definition, candidateLimit);
                if (allowedRadius > 0
                    && GetChebyshevDistance(pair.Key, position)
                        <= allowedRadius)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasOverlappingInfluenceCenter(
            Vector2Int candidatePosition,
            int candidateRadius,
            Vector2Int? ignoredPendingPosition,
            out Vector2Int overlappingPosition,
            out string overlappingBuildingId,
            out int overlappingRadius)
        {
            overlappingPosition = default;
            overlappingBuildingId = null;
            overlappingRadius = 0;

            if (candidateRadius <= 0)
                return false;

            foreach (var pair in _factionPlacedBuildings)
            {
                if (pair.Key == ignoredPendingPosition)
                    continue;

                BuildingDefinition definition =
                    _buildingRegistry.GetById(pair.Value.BuildingId);
                if (!IsInfluenceCenter(definition))
                    continue;

                int existingRadius = ResolveInfluenceRadius(definition);
                if (existingRadius <= 0
                    || GetChebyshevDistance(pair.Key, candidatePosition)
                        > candidateRadius + existingRadius)
                {
                    continue;
                }

                overlappingPosition = pair.Key;
                overlappingBuildingId = pair.Value.BuildingId;
                overlappingRadius = existingRadius;
                return true;
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (pair.Key == ignoredPendingPosition)
                    continue;

                BuildingDefinition definition =
                    _buildingRegistry.GetById(pair.Value);
                if (!IsInfluenceCenter(definition))
                    continue;

                int existingRadius = ResolveInfluenceRadius(definition);
                if (existingRadius <= 0
                    || GetChebyshevDistance(pair.Key, candidatePosition)
                        > candidateRadius + existingRadius)
                {
                    continue;
                }

                overlappingPosition = pair.Key;
                overlappingBuildingId = pair.Value;
                overlappingRadius = existingRadius;
                return true;
            }

            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                var pending = _pendingPlacements[i];
                if (pending.Position == ignoredPendingPosition)
                    continue;

                var pendingDef = _buildingRegistry.GetById(pending.BuildingId);
                if (!IsInfluenceCenter(pendingDef))
                    continue;

                int existingRadius = ResolveInfluenceRadius(pendingDef);
                if (existingRadius <= 0)
                    continue;

                if (GetChebyshevDistance(
                        pending.Position,
                        candidatePosition)
                    <= candidateRadius + existingRadius)
                {
                    overlappingPosition = pending.Position;
                    overlappingBuildingId = pending.BuildingId;
                    overlappingRadius = existingRadius;
                    return true;
                }
            }

            return false;
        }

private int ResolveCoverageRadius(BuildingDefinition centerDefinition, int candidateLimit)
        {
            int sourceRadius = ResolveInfluenceRadius(centerDefinition);
            if (sourceRadius <= 0)
                return 0;

            return candidateLimit > 0
                ? Mathf.Min(sourceRadius, candidateLimit)
                : sourceRadius;
        }

        private int ResolvePlacedCenterSearchRadius(int candidateLimit)
        {
            int maxRadius = ResolveMaxInfluenceRadius();
            return candidateLimit > 0
                ? Mathf.Min(maxRadius, candidateLimit)
                : maxRadius;
        }

        private bool HasAnyInfluenceCenterDefinition()
        {
            if (_cachedInfluenceCenterDefinitionState >= 0)
                return _cachedInfluenceCenterDefinitionState == 1;

            bool found = false;
            BuildingDefinition[] definitions =
                _buildingRegistry?.GetAll()
                ?? System.Array.Empty<BuildingDefinition>();
            for (int index = 0; index < definitions.Length; index++)
            {
                if (!IsInfluenceCenter(definitions[index]))
                    continue;

                found = true;
                break;
            }

            _cachedInfluenceCenterDefinitionState = found ? 1 : 0;
            return found;
        }

        private int ResolveMaxInfluenceRadius()
        {
            if (_cachedMaxInfluenceRadius >= 0)
                return _cachedMaxInfluenceRadius;

            int maxRadius = _townHallBuildRadius;
            BuildingDefinition[] definitions =
                _buildingRegistry?.GetAll()
                ?? System.Array.Empty<BuildingDefinition>();
            for (int i = 0; i < definitions.Length; i++)
            {
                BuildingDefinition definition = definitions[i];
                if (!IsInfluenceCenter(definition))
                    continue;

                maxRadius = Mathf.Max(
                    maxRadius,
                    ResolveInfluenceRadius(definition));
            }

            _cachedMaxInfluenceRadius = Mathf.Max(0, maxRadius);
            return _cachedMaxInfluenceRadius;
        }

        private int ResolveCandidateProximityLimit(BuildingDefinition candidate)
        {
            return candidate != null && candidate.TownHallProximityRadiusOverride > 0
                ? candidate.TownHallProximityRadiusOverride
                : 0;
        }

        private int ResolveInfluenceRadius(BuildingDefinition definition)
        {
            return BuildingDefinitionCapabilities.GetInfluenceRadius(definition, _townHallBuildRadius);
        }

        private static int GetChebyshevDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        private static bool IsInfluenceCenter(BuildingDefinition definition)
        {
            return BuildingDefinitionCapabilities.IsSettlementCenter(
                definition);
        }
    }
}
