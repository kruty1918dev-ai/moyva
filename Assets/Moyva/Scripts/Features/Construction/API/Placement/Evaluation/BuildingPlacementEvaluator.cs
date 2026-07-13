using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{

    public static class BuildingPlacementEvaluator
    {
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
                    result.AddFootprintPosition(BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index));
            }

            bool tileOccupied = IsFootprintOccupied(request, definition, result);
            if (result != null)
                result.TileOccupied = tileOccupied;
            if (tileOccupied)
                return false;

            bool terrainBlocked = IsBlockedByTerrain(request, definition, result)
                || IsBlockedByRequiredTerrain(request, definition, result)
                || IsBlockedByFootprintSlope(request, definition, result);
            if (result != null)
                result.TerrainBlocked = terrainBlocked;
            if (terrainBlocked)
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

            bool influenceBlocked = !request.SkipInfluenceRules && IsBlockedByInfluenceZone(request, result);
            if (result != null)
                result.InfluenceZoneBlocked = influenceBlocked;
            return !influenceBlocked;
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
            return BuildingDefinitionCapabilities.IsTownHall(definition)
                || BuildingDefinitionCapabilities.IsCastle(definition);
        }

        private static bool IsBlockedByRequiredTerrain(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            IReadOnlyList<string> requiredTerrainIds = definition?.RequiredTerrainIds;
            if (requiredTerrainIds == null || requiredTerrainIds.Count == 0)
                return false;

            if (request.GetTileId == null)
            {
                result?.AddNote("GetTileId не заданий, terrain-вимогу будівлі пропущено.");
                return false;
            }

            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index);
                string tileId = request.GetTileId(position);
                if (ContainsTileId(requiredTerrainIds, tileId))
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Terrain,
                    Message = $"Будівлю '{request.BuildingId}' не можна ставити на tile '{tileId}'.",
                    Position = position,
                    BuildingId = request.BuildingId,
                });
                return true;
            }

            return false;
        }

        private static bool IsBlockedByTerrain(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            if (request.IsTerrainBlocked == null)
                return false;

            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index);
                if (!request.IsTerrainBlocked(position))
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Terrain,
                    Message = "Footprint contains a terrain cell blocked for construction.",
                    Position = position,
                    BuildingId = request.BuildingId,
                });
                return true;
            }

            return false;
        }

        private static bool IsBlockedByFootprintSlope(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            if (definition?.Footprint == null
                || !definition.Footprint.RequiresFlatGround
                || request.GetTerrainLevel == null)
            {
                return false;
            }

            int? expectedLevel = null;
            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index);
                int? level = request.GetTerrainLevel(position);
                if (!level.HasValue)
                    continue;

                if (!expectedLevel.HasValue)
                {
                    expectedLevel = level;
                    continue;
                }

                if (expectedLevel.Value == level.Value)
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Terrain,
                    Message = $"Footprint requires flat ground, but terrain levels differ ({expectedLevel.Value} and {level.Value}).",
                    Position = position,
                    BuildingId = request.BuildingId,
                });
                return true;
            }

            return false;
        }

        private static bool ContainsTileId(IReadOnlyList<string> tileIds, string tileId)
        {
            if (tileIds == null || tileIds.Count == 0 || string.IsNullOrWhiteSpace(tileId))
                return false;

            for (int i = 0; i < tileIds.Count; i++)
            {
                string candidate = tileIds[i];
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;

                if (string.Equals(candidate.Trim(), tileId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool IsFootprintOccupied(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index);
                bool occupied = request.IsOccupied != null && request.IsOccupied(position);
                bool pending = HasPendingAt(request, position, request.IgnoredPendingPosition, out var pendingEntry);
                if (!occupied && !pending)
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.OccupiedTile,
                    Message = "Footprint cell is already occupied by a building or pending preview.",
                    Position = position,
                    BuildingId = pending ? pendingEntry.BuildingId : GetOccupantId(request, position),
                });
                return true;
            }

            return false;
        }

        private static bool IsBlockedBySpacing(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            int spacing = Mathf.Max(0, request.MinSpacing);
            if (spacing <= 0)
                return false;

            int footprintCount = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int cellIndex = 0; cellIndex < footprintCount; cellIndex++)
            {
                Vector2Int footprintCell = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, cellIndex);
                for (int offsetX = -spacing; offsetX <= spacing; offsetX++)
                {
                    for (int offsetY = -spacing; offsetY <= spacing; offsetY++)
                    {
                        if (offsetX == 0 && offsetY == 0)
                            continue;

                        var neighbor = new Vector2Int(footprintCell.x + offsetX, footprintCell.y + offsetY);
                        if (BuildingFootprintUtility.Contains(definition, request.Position, neighbor))
                            continue;

                        bool occupied = request.IsOccupied != null && request.IsOccupied(neighbor);
                        bool pending = HasPendingAt(request, neighbor, request.IgnoredPendingPosition, out var pendingEntry);
                        if (!occupied && !pending)
                            continue;

                        result?.AddBlocker(new BuildingPlacementBlocker
                        {
                            Kind = BuildingPlacementBlockerKind.Spacing,
                            Message = $"Порушено мінімальний відступ {spacing}: поруч є {(pending ? "pending-preview" : "зайнятий тайл")}.",
                            Position = neighbor,
                            BuildingId = pending ? pendingEntry.BuildingId : GetOccupantId(request, neighbor),
                            Radius = spacing,
                        });
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsBlockedByFog(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            if (definition != null && definition.CanPlaceInFog)
                return false;

            if (request.IsFogBlocked == null)
                return false;

            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index);
                if (!request.IsFogBlocked(position))
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Fog,
                    Message = "Тайл не є видимим у Fog of War. Будівництво дозволене тільки на Visible.",
                    Position = position,
                });
                return true;
            }

            return false;
        }

        private static bool IsBlockedByInfluenceZone(BuildingPlacementEvaluationRequest request, BuildingPlacementEvaluationResult result)
        {
            if (string.IsNullOrWhiteSpace(request.BuildingId))
            {
                result?.AddNote("BuildingId порожній, influence-перевірку пропущено.");
                return false;
            }

            if (request.BuildingRegistry == null)
            {
                result?.AddNote("BuildingRegistry не заданий, influence-перевірку пропущено.");
                return false;
            }

            var candidate = request.BuildingRegistry.GetById(request.BuildingId);
            if (candidate == null)
            {
                result?.AddNote($"Будівлю '{request.BuildingId}' не знайдено у реєстрі, influence-перевірку пропущено.");
                return false;
            }

            if (!AnyInfluenceCenterDefined(request))
            {
                result?.AddNote("У реєстрі немає ратуші або замку, тому правило зони поселення вимкнене.");
                return false;
            }

            int ruleRadius = IsInfluenceCenter(candidate)
                ? ResolveInfluenceRadius(candidate, Mathf.Max(0, request.TownHallBuildRadius))
                : ResolveMaxInfluenceRadius(request);
            if (ruleRadius <= 0)
            {
                result?.AddNote("Радіус influence-правила дорівнює 0, перевірку пропущено.");
                return false;
            }

            bool requireInfluenceCenterInRange;
            bool blockWhenInfluenceCenterExists;
            if (candidate.UseCustomTownHallRules)
            {
                requireInfluenceCenterInRange = candidate.RequireTownHallInRange;
                blockWhenInfluenceCenterExists = candidate.BlockIfTownHallAlreadyInRange;
            }
            else
            {
                bool candidateIsCenter = IsInfluenceCenter(candidate);
                requireInfluenceCenterInRange = !candidateIsCenter;
                blockWhenInfluenceCenterExists = candidateIsCenter;
            }

            bool hasInfluenceCenterInRange = HasInfluenceCenterCoveringPosition(request, request.Position, candidate, out var coveringCenter);
            if (requireInfluenceCenterInRange && !hasInfluenceCenterInRange)
            {
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.InfluenceRequired,
                    Message = $"Потрібна ратуша або замок у радіусі {ruleRadius}.",
                    Position = request.Position,
                    Radius = ruleRadius,
                });
                return true;
            }

            if (coveringCenter.HasValue)
            {
                result?.AddNote($"Позицію покриває центр '{coveringCenter.Value.BuildingId}' на {coveringCenter.Value.Position}.");
            }

            int candidateRadius = ResolveInfluenceRadius(candidate, Mathf.Max(0, request.TownHallBuildRadius));
            if (blockWhenInfluenceCenterExists
                && HasOverlappingInfluenceCenter(request, request.Position, candidateRadius, out var overlap))
            {
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.InfluenceOverlap,
                    Message = $"Зона центру перетинається з '{overlap.BuildingId}' на {overlap.Position}.",
                    Position = overlap.Position,
                    BuildingId = overlap.BuildingId,
                    Radius = overlap.Radius,
                });
                return true;
            }

            return false;
        }

        private static bool AnyInfluenceCenterDefined(BuildingPlacementEvaluationRequest request)
        {
            var definitions = request.BuildingRegistry.GetAll() ?? Array.Empty<BuildingDefinition>();
            for (int index = 0; index < definitions.Length; index++)
            {
                if (IsInfluenceCenter(definitions[index]))
                    return true;
            }

            return false;
        }

        private static bool HasInfluenceCenterCoveringPosition(
            BuildingPlacementEvaluationRequest request,
            Vector2Int position,
            BuildingDefinition candidate,
            out BuildingPlacementSimulationEntry? coveringCenter)
        {
            coveringCenter = null;
            int candidateLimit = ResolveCandidateProximityLimit(candidate);

            int searchRadius = ResolvePlacedCenterSearchRadius(request, candidateLimit);
            for (int offsetX = -searchRadius; offsetX <= searchRadius; offsetX++)
            {
                for (int offsetY = -searchRadius; offsetY <= searchRadius; offsetY++)
                {
                    var centerPosition = new Vector2Int(position.x + offsetX, position.y + offsetY);
                    if (centerPosition == request.IgnoredPendingPosition)
                        continue;

                    string occupantId = GetOccupantId(request, centerPosition);
                    if (string.IsNullOrWhiteSpace(occupantId))
                        continue;

                    Vector2Int resolvedOrigin = ResolveOccupantOrigin(request, centerPosition);

                    var definition = request.BuildingRegistry.GetById(occupantId);
                    if (!IsInfluenceCenter(definition))
                        continue;

                    int allowedRadius = ResolveCoverageRadius(definition, candidateLimit, request.TownHallBuildRadius);
                    if (allowedRadius <= 0)
                        continue;

                    if (GetChebyshevDistance(resolvedOrigin, position) <= allowedRadius)
                    {
                        coveringCenter = new BuildingPlacementSimulationEntry(resolvedOrigin, occupantId);
                        return true;
                    }
                }
            }

            var pendingPlacements = request.PendingPlacements;
            if (pendingPlacements == null)
                return false;

            for (int index = 0; index < pendingPlacements.Count; index++)
            {
                var pending = pendingPlacements[index];
                if (pending.Position == request.IgnoredPendingPosition)
                    continue;

                var pendingDefinition = request.BuildingRegistry.GetById(pending.BuildingId);
                if (!IsInfluenceCenter(pendingDefinition))
                    continue;

                int allowedRadius = ResolveCoverageRadius(pendingDefinition, candidateLimit, request.TownHallBuildRadius);
                if (allowedRadius <= 0)
                    continue;

                if (GetChebyshevDistance(pending.Position, position) <= allowedRadius)
                {
                    coveringCenter = pending;
                    return true;
                }
            }

            return false;
        }

        private static bool HasOverlappingInfluenceCenter(
            BuildingPlacementEvaluationRequest request,
            Vector2Int candidatePosition,
            int candidateRadius,
            out BuildingPlacementOverlap overlap)
        {
            overlap = default;
            if (candidateRadius <= 0)
                return false;

            int searchRadius = candidateRadius + ResolveMaxInfluenceRadius(request);
            for (int offsetX = -searchRadius; offsetX <= searchRadius; offsetX++)
            {
                for (int offsetY = -searchRadius; offsetY <= searchRadius; offsetY++)
                {
                    var centerPosition = new Vector2Int(candidatePosition.x + offsetX, candidatePosition.y + offsetY);
                    if (centerPosition == request.IgnoredPendingPosition)
                        continue;

                    string occupantId = GetOccupantId(request, centerPosition);
                    if (string.IsNullOrWhiteSpace(occupantId))
                        continue;

                    Vector2Int resolvedOrigin = ResolveOccupantOrigin(request, centerPosition);

                    var definition = request.BuildingRegistry.GetById(occupantId);
                    if (!IsInfluenceCenter(definition))
                        continue;

                    int existingRadius = ResolveInfluenceRadius(definition, Mathf.Max(0, request.TownHallBuildRadius));
                    if (existingRadius <= 0)
                        continue;

                    if (GetChebyshevDistance(resolvedOrigin, candidatePosition) > candidateRadius + existingRadius)
                        continue;

                    overlap = new BuildingPlacementOverlap(resolvedOrigin, occupantId, existingRadius);
                    return true;
                }
            }

            var pendingPlacements = request.PendingPlacements;
            if (pendingPlacements == null)
                return false;

            for (int index = 0; index < pendingPlacements.Count; index++)
            {
                var pending = pendingPlacements[index];
                if (pending.Position == request.IgnoredPendingPosition)
                    continue;

                var pendingDefinition = request.BuildingRegistry.GetById(pending.BuildingId);
                if (!IsInfluenceCenter(pendingDefinition))
                    continue;

                int existingRadius = ResolveInfluenceRadius(pendingDefinition, Mathf.Max(0, request.TownHallBuildRadius));
                if (existingRadius <= 0)
                    continue;

                if (GetChebyshevDistance(pending.Position, candidatePosition) <= candidateRadius + existingRadius)
                {
                    overlap = new BuildingPlacementOverlap(pending.Position, pending.BuildingId, existingRadius);
                    return true;
                }
            }

            return false;
        }

        private static int ResolveCoverageRadius(BuildingDefinition centerDefinition, int candidateLimit, int fallbackRadius)
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
            int maxRadius = Mathf.Max(0, request.TownHallBuildRadius);
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
            return candidate != null && candidate.TownHallProximityRadiusOverride > 0
                ? candidate.TownHallProximityRadiusOverride
                : 0;
        }

        private static bool HasPendingAt(
            BuildingPlacementEvaluationRequest request,
            Vector2Int position,
            Vector2Int? ignoredPendingPosition,
            out BuildingPlacementSimulationEntry pendingEntry)
        {
            pendingEntry = default;
            var pendingPlacements = request.PendingPlacements;
            if (pendingPlacements == null)
                return false;

            for (int index = 0; index < pendingPlacements.Count; index++)
            {
                var pending = pendingPlacements[index];
                if (pending.Position == ignoredPendingPosition)
                    continue;

                BuildingDefinition pendingDefinition = request.BuildingRegistry?.GetById(pending.BuildingId);
                if (!BuildingFootprintUtility.Contains(pendingDefinition, pending.Position, position))
                    continue;

                pendingEntry = pending;
                return true;
            }

            return false;
        }

        private static string GetOccupantId(BuildingPlacementEvaluationRequest request, Vector2Int position)
        {
            return request.GetOccupantId != null ? request.GetOccupantId(position) : null;
        }

        private static Vector2Int ResolveOccupantOrigin(BuildingPlacementEvaluationRequest request, Vector2Int position)
        {
            Vector2Int? origin = request.GetOccupantOrigin?.Invoke(position);
            return origin ?? position;
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
