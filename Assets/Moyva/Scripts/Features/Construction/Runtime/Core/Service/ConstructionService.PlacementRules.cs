// AI-context consolidation: related partials live together by responsibility.
// No gameplay behavior is intentionally changed by this file organization.
using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using System.Collections.Generic;

// ---- Consolidated from ConstructionService.FogTerrain.cs ----
namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private void ApplyBuildingFogReveal(
            string buildingId,
            Vector2Int position)
        {
            if (_fogOfWarService == null
                || _buildingRegistry == null)
            {
                return;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(buildingId);

            bool hasFogModule =
                BuildingDefinitionCapabilities.TryGetFogReveal(
                    definition,
                    out FogRevealBuildingModule fogReveal);
            int defenseBonus =
                BuildingDefinitionCapabilities
                    .GetDefenseVisionRevealBonus(definition);

            if (!hasFogModule && defenseBonus <= 0)
                return;

            int baseRadius = hasFogModule
                ? Mathf.Max(0, fogReveal.RevealRadius)
                : 0;
            int radius =
                Mathf.Max(0, baseRadius + defenseBonus);
            string areaId =
                GetBuildingFogVisionAreaId(position);

            if (radius <= 0)
            {
                _fogOfWarService.UnregisterUnit(areaId);
                return;
            }

            if (!hasFogModule)
            {
                _fogOfWarService.RegisterFixedVisionArea(
                    areaId,
                    position,
                    radius,
                    FogRevealShape.PixelCircle);
                return;
            }

            if (fogReveal.RevealWhileActive)
            {
                _fogOfWarService.RegisterFixedVisionArea(
                    areaId,
                    position,
                    radius,
                    fogReveal.Shape);
                return;
            }

            _fogOfWarService.UnregisterUnit(areaId);

            if (fogReveal.RevealOnBuilt)
            {
                _fogOfWarService.RevealArea(
                    position,
                    radius,
                    fogReveal.Shape,
                    keepVisible: false,
                    areaId);
            }
        }

        private static string GetBuildingFogVisionAreaId(Vector2Int position)
            => $"building:{position.x}:{position.y}";

    }
}

// ---- Consolidated from ConstructionService.Footprint.cs ----
namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private ConstructionRotation ResolvePlacedRotation(
            Vector2Int origin)
            => _placedRotationByOrigin.TryGetValue(
                origin,
                out ConstructionRotation rotation)
                ? rotation
                : ConstructionRotation.Degrees0;

        private bool TryResolveGateReplacement(
            Vector2Int position,
            string gateBuildingId,
            out Vector2Int replacedOrigin,
            out string replacedBuildingId)
        {
            replacedOrigin = position;
            replacedBuildingId = null;
            BuildingDefinition candidate =
                _placementBuildingRegistry?.GetById(gateBuildingId);
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    candidate,
                    out ReplacementPlacementRuleModule replacementModule))
            {
                if (replacementModule.MergeMode
                    == PlacementRuleMergeMode.Disabled)
                {
                    return false;
                }

                if (replacementModule.MergeMode
                    == PlacementRuleMergeMode.Override)
                {
                    if (!_objectsMapService.TryGetOccupant(
                            position,
                            out replacedBuildingId)
                        || !CanReplaceBuilding(
                            replacedBuildingId,
                            replacementModule))
                    {
                        replacedBuildingId = null;
                        return false;
                    }

                    replacedOrigin = _footprints.ResolveOrigin(position);
                    return true;
                }
            }

            // Compatibility adapter for definitions not migrated to an explicit
            // replacement module yet.
            if (_wallTopologyService == null
                || _wallGateReplacementValidator == null
                || !_wallTopologyService.IsGate(gateBuildingId)
                || !_wallGateReplacementValidator.CanReplaceWallWithGate(
                    position,
                    gateBuildingId,
                    out replacedBuildingId))
            {
                return false;
            }

            replacedOrigin = _footprints.ResolveOrigin(position);
            if (string.IsNullOrWhiteSpace(replacedBuildingId)
                && _objectsMapService.TryGetOccupant(position, out string occupantId))
            {
                replacedBuildingId = occupantId;
            }

            return !string.IsNullOrWhiteSpace(replacedBuildingId);
        }

        private bool RequiresReplacement(
            string buildingId,
            out ReplacementPlacementRuleModule module)
        {
            BuildingDefinition definition =
                _placementBuildingRegistry?.GetById(buildingId);
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out module))
            {
                return module.MergeMode
                    == PlacementRuleMergeMode.Override;
            }

            module = null;
            return _wallTopologyService != null
                && _wallTopologyService.IsGate(buildingId);
        }

        private static bool RequiresSameOwner(
            ReplacementPlacementRuleModule module)
            => module?.MergeMode == PlacementRuleMergeMode.Override
                ? module.RequireSameOwner
                : true;

        private bool IsReplacementSatisfiedByPendingMarker(
            string replacementBuildingId,
            string replacedPendingBuildingId,
            ReplacementPlacementRuleModule module)
        {
            if (string.IsNullOrWhiteSpace(replacedPendingBuildingId))
                return false;

            if (module?.MergeMode == PlacementRuleMergeMode.Disabled)
                return false;

            if (module?.MergeMode == PlacementRuleMergeMode.Override)
            {
                return CanReplaceBuilding(
                    replacedPendingBuildingId,
                    module);
            }

            // Compatibility adapter for unmigrated gates only.
            return _wallTopologyService != null
                && _wallTopologyService.IsGate(replacementBuildingId)
                && _wallTopologyService.IsWall(
                    replacedPendingBuildingId);
        }

        private bool CanReplaceBuilding(
            string replacedBuildingId,
            ReplacementPlacementRuleModule module)
        {
            if (module == null
                || string.IsNullOrWhiteSpace(replacedBuildingId))
            {
                return false;
            }

            if (ContainsId(
                    module.ReplaceableBuildingIds,
                    replacedBuildingId))
            {
                return true;
            }

            BuildingDefinition replaced =
                _placementBuildingRegistry?.GetById(replacedBuildingId);
            if (replaced == null
                || module.ReplaceableBuildingTags == null)
            {
                return false;
            }

            for (int index = 0;
                 index < module.ReplaceableBuildingTags.Length;
                 index++)
            {
                string tag = module.ReplaceableBuildingTags[index];
                if (ContainsTag(replaced.Tags, tag)
                    || ContainsTag(replaced.RuntimeTags, tag))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsId(
            System.Collections.Generic.IReadOnlyList<string> values,
            string expected)
        {
            if (values == null || string.IsNullOrWhiteSpace(expected))
                return false;

            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(
                        values[index]?.Trim(),
                        expected.Trim(),
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private void RestoreBuildingFootprintOrLog(Vector2Int origin, string buildingId, string context)
        {
            if (_footprints.TryRegister(
                    origin,
                    buildingId,
                    ResolvePlacedRotation(origin)))
                return;

            Debug.LogError(
                $"[MoyvaBuildGridDiag] footprint-rollback-failed context='{context}' building='{buildingId}' origin={origin}");
        }
    }
}

// ---- Consolidated from ConstructionService.Influence.cs ----
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

// ---- Consolidated from ConstructionService.Validation.cs ----
namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private bool IsBlockedBySpacing(Vector2Int position, Vector2Int? ignoredPendingPosition)
        {
            try
            {
                if (_minSpacing <= 0)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] IsBlockedBySpacing({position}): _minSpacing <= 0, spacing-перевірка відключена");
                    return false;
                }

                if (_objectsMapService == null)
                {
                    Debug.LogError("[Construction] IsBlockedBySpacing: _objectsMapService == null");
                    return false;
                }

                if (_pendingPositions == null)
                {
                    Debug.LogError("[Construction] IsBlockedBySpacing: _pendingPositions == null");
                    return false;
                }

                for (int dx = -_minSpacing; dx <= _minSpacing; dx++)
                {
                    for (int dy = -_minSpacing; dy <= _minSpacing; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        var neighbor = new Vector2Int(position.x + dx, position.y + dy);
                        bool blockedByPending = _pendingPositions.Contains(neighbor) && neighbor != ignoredPendingPosition;
                        bool isOccupied = _objectsMapService.IsOccupied(neighbor);
                        if (isOccupied || blockedByPending)
                        {
                            if (VerboseLogs)
                                Debug.Log($"[Construction] IsBlockedBySpacing({position}): BLOCKED біля {neighbor} (occupied={isOccupied}, pending={blockedByPending})");
                            return true;
                        }
                    }
                }

                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedBySpacing({position}): OK (spacing={_minSpacing})");

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в IsBlockedBySpacing({position}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        private bool CanPlaceAt(
            Vector2Int position,
            Vector2Int? ignoredPendingPosition,
            string buildingId,
            out bool tileOccupied,
            out bool spacingBlocked,
            out bool fogBlocked,
            out bool influenceZoneBlocked,
            out bool terrainBlocked,
            Vector2Int? ignoredOccupiedPosition = null,
            string satisfiedReplacementBuildingId = null,
            ConstructionRotation rotation =
                ConstructionRotation.Degrees0)
        {
            var fastQuery = new ConstructionPlacementQueryRequest(
                buildingId,
                position,
                ignoredPendingPosition,
                ignoredOccupiedPosition,
                includeResources: false,
                includeDetails: false,
                ownerId: _activeOwnerId,
                attemptSource:
                    ConstructionPlacementAttemptSource.Confirm,
                allowUniquePreviewRelocation: false,
                satisfiedReplacementBuildingId:
                    satisfiedReplacementBuildingId,
                rotation: rotation);

            ConstructionPlacementQueryResult fastResult =
                EvaluatePlacement(fastQuery);

            bool fastAccepted =
                fastResult.AvailabilityValid
                && fastResult.SpatialValid
                && fastResult.AuthorityValid;

            if (fastAccepted)
            {
                tileOccupied = false;
                spacingBlocked = false;
                fogBlocked = false;
                influenceZoneBlocked = false;
                terrainBlocked = false;

                LogPlacementAttempt(
                    fastResult,
                    emitRejectedAction: false);
                return true;
            }

            var detailedQuery = new ConstructionPlacementQueryRequest(
                buildingId,
                position,
                ignoredPendingPosition,
                ignoredOccupiedPosition,
                includeResources: false,
                includeDetails: true,
                ownerId: _activeOwnerId,
                attemptSource:
                    ConstructionPlacementAttemptSource.Confirm,
                allowUniquePreviewRelocation: false,
                satisfiedReplacementBuildingId:
                    satisfiedReplacementBuildingId,
                rotation: rotation);

            ConstructionPlacementQueryResult result =
                EvaluatePlacement(detailedQuery);

            BuildingPlacementEvaluationResult evaluation =
                result.EvaluationResult;

            tileOccupied = evaluation?.TileOccupied
                ?? (!result.IsSpatiallyValid
                    && _objectsMapService.IsOccupied(position));
            spacingBlocked =
                evaluation?.SpacingBlocked ?? false;
            fogBlocked =
                evaluation?.FogBlocked ?? false;
            influenceZoneBlocked =
                evaluation?.InfluenceZoneBlocked ?? false;
            terrainBlocked = evaluation?.TerrainBlocked
                ?? (!result.IsSpatiallyValid
                    && _placementEnvironmentRules.IsBlockedByTerrain(position, out _));

            LogPlacementAttempt(
                result,
                emitRejectedAction: !result.CanCommit);

            return result.AvailabilityValid
                && result.SpatialValid
                && result.AuthorityValid;
        }

        private string GetObjectOccupantId(Vector2Int position, Vector2Int? ignoredOccupiedPosition = null)
        {
            if (ignoredOccupiedPosition.HasValue && position == ignoredOccupiedPosition.Value)
                return null;

            return _objectsMapService.TryGetOccupant(position, out var occupantId)
                ? occupantId
                : null;
        }

        private IReadOnlyList<BuildingPlacementSimulationEntry> BuildPlacementSimulationEntries()
        {
            if (_placementSimulationSnapshotVersion == _pendingPlacementsVersion)
                return _placementSimulationSnapshot;

            _placementSimulationSnapshotVersion = _pendingPlacementsVersion;
            _placementSimulationSnapshot.Clear();
            for (int index = 0; index < _pendingPlacements.Count; index++)
            {
                var placement = _pendingPlacements[index];
                _placementSimulationSnapshot.Add(
                    new BuildingPlacementSimulationEntry(
                        placement.Position,
                        placement.BuildingId,
                        NormalizeOwnerId(_activeOwnerId),
                        placement.Rotation));
            }

            return _placementSimulationSnapshot;
        }

        private IReadOnlyList<BuildingPlacementSimulationEntry>
            BuildPlacedBuildingSimulationEntries()
        {
            _placedBuildingSimulationSnapshot.Clear();

            foreach (var pair in _factionPlacedBuildings)
            {
                _placedBuildingSimulationSnapshot.Add(
                    new BuildingPlacementSimulationEntry(
                        pair.Key,
                        pair.Value.BuildingId,
                        NormalizeOwnerId(pair.Value.FactionId),
                        ResolvePlacedRotation(pair.Key)));
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                _placedBuildingSimulationSnapshot.Add(
                    new BuildingPlacementSimulationEntry(
                        pair.Key,
                        pair.Value,
                        NormalizeOwnerId(_activeOwnerId),
                        ResolvePlacedRotation(pair.Key)));
            }

            return _placedBuildingSimulationSnapshot;
        }
    }
}
