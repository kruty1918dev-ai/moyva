using System;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{

    public static class BuildingPlacementEvaluator
    {
        public static BuildingPlacementEvaluationResult Evaluate(BuildingPlacementEvaluationRequest request)
        {
            var result = new BuildingPlacementEvaluationResult();
            if (request == null)
            {
                result.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Configuration,
                    Message = "Placement request is null.",
                });
                return result;
            }

            result.TileOccupied = IsTileOccupied(request, request.Position);
            if (result.TileOccupied)
            {
                result.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.OccupiedTile,
                    Message = "Тайл уже зайнятий будівлею або pending-preview.",
                    Position = request.Position,
                    BuildingId = GetOccupantId(request, request.Position),
                });
                return result;
            }

            result.SpacingBlocked = IsBlockedBySpacing(request, result);
            if (result.SpacingBlocked)
                return result;

            result.FogBlocked = IsBlockedByFog(request, result);
            if (result.FogBlocked)
                return result;

            result.InfluenceZoneBlocked = IsBlockedByInfluenceZone(request, result);
            return result;
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

        private static bool IsTileOccupied(BuildingPlacementEvaluationRequest request, Vector2Int position)
        {
            if (request.IsOccupied != null && request.IsOccupied(position))
                return true;

            return HasPendingAt(request, position, request.IgnoredPendingPosition, out _);
        }

        private static bool IsBlockedBySpacing(BuildingPlacementEvaluationRequest request, BuildingPlacementEvaluationResult result)
        {
            int spacing = Mathf.Max(0, request.MinSpacing);
            if (spacing <= 0)
                return false;

            for (int offsetX = -spacing; offsetX <= spacing; offsetX++)
            {
                for (int offsetY = -spacing; offsetY <= spacing; offsetY++)
                {
                    if (offsetX == 0 && offsetY == 0)
                        continue;

                    var neighbor = new Vector2Int(request.Position.x + offsetX, request.Position.y + offsetY);
                    bool occupied = request.IsOccupied != null && request.IsOccupied(neighbor);
                    bool pending = HasPendingAt(request, neighbor, request.IgnoredPendingPosition, out var pendingEntry);
                    if (!occupied && !pending)
                        continue;

                    result.AddBlocker(new BuildingPlacementBlocker
                    {
                        Kind = BuildingPlacementBlockerKind.Spacing,
                        Message = $"Порушено мінімальний відступ {spacing}: поруч є {(pending ? "pending-preview" : "зайнятий тайл") }.",
                        Position = neighbor,
                        BuildingId = pending ? pendingEntry.BuildingId : GetOccupantId(request, neighbor),
                        Radius = spacing,
                    });
                    return true;
                }
            }

            return false;
        }

        private static bool IsBlockedByFog(BuildingPlacementEvaluationRequest request, BuildingPlacementEvaluationResult result)
        {
            var definition = request.BuildingRegistry?.GetById(request.BuildingId);
            if (definition != null && definition.CanPlaceInFog)
                return false;

            if (request.IsFogBlocked == null || !request.IsFogBlocked(request.Position))
                return false;

            result.AddBlocker(new BuildingPlacementBlocker
            {
                Kind = BuildingPlacementBlockerKind.Fog,
                Message = "Тайл не є видимим у Fog of War. Будівництво дозволене тільки на Visible.",
                Position = request.Position,
            });
            return true;
        }

        private static bool IsBlockedByInfluenceZone(BuildingPlacementEvaluationRequest request, BuildingPlacementEvaluationResult result)
        {
            if (string.IsNullOrWhiteSpace(request.BuildingId))
            {
                result.AddNote("BuildingId порожній, influence-перевірку пропущено.");
                return false;
            }

            if (request.BuildingRegistry == null)
            {
                result.AddNote("BuildingRegistry не заданий, influence-перевірку пропущено.");
                return false;
            }

            var candidate = request.BuildingRegistry.GetById(request.BuildingId);
            if (candidate == null)
            {
                result.AddNote($"Будівлю '{request.BuildingId}' не знайдено у реєстрі, influence-перевірку пропущено.");
                return false;
            }

            if (!AnyInfluenceCenterDefined(request))
            {
                result.AddNote("У реєстрі немає ратуші або замку, тому правило зони поселення вимкнене.");
                return false;
            }

            int ruleRadius = IsInfluenceCenter(candidate)
                ? ResolveInfluenceRadius(candidate, Mathf.Max(0, request.TownHallBuildRadius))
                : ResolveMaxInfluenceRadius(request);
            if (ruleRadius <= 0)
            {
                result.AddNote("Радіус influence-правила дорівнює 0, перевірку пропущено.");
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
                result.AddBlocker(new BuildingPlacementBlocker
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
                result.AddNote($"Позицію покриває центр '{coveringCenter.Value.BuildingId}' на {coveringCenter.Value.Position}.");
            }

            int candidateRadius = ResolveInfluenceRadius(candidate, Mathf.Max(0, request.TownHallBuildRadius));
            if (blockWhenInfluenceCenterExists
                && HasOverlappingInfluenceCenter(request, request.Position, candidateRadius, out var overlap))
            {
                result.AddBlocker(new BuildingPlacementBlocker
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

                    var definition = request.BuildingRegistry.GetById(occupantId);
                    if (!IsInfluenceCenter(definition))
                        continue;

                    int allowedRadius = ResolveCoverageRadius(definition, candidateLimit, request.TownHallBuildRadius);
                    if (allowedRadius <= 0)
                        continue;

                    if (GetChebyshevDistance(centerPosition, position) <= allowedRadius)
                    {
                        coveringCenter = new BuildingPlacementSimulationEntry(centerPosition, occupantId);
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

                    var definition = request.BuildingRegistry.GetById(occupantId);
                    if (!IsInfluenceCenter(definition))
                        continue;

                    int existingRadius = ResolveInfluenceRadius(definition, Mathf.Max(0, request.TownHallBuildRadius));
                    if (existingRadius <= 0)
                        continue;

                    if (GetChebyshevDistance(centerPosition, candidatePosition) > candidateRadius + existingRadius)
                        continue;

                    overlap = new BuildingPlacementOverlap(centerPosition, occupantId, existingRadius);
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
                if (pending.Position != position || pending.Position == ignoredPendingPosition)
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
