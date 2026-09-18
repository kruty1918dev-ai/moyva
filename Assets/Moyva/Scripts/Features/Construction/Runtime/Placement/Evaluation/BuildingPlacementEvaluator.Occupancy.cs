using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public static partial class BuildingPlacementEvaluator
    {
        private static bool IsFootprintOccupied(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(
                    definition,
                    request.Position,
                    index,
                    request.Rotation);
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
            int spacing = ResolveSpacing(request, definition);
            if (spacing <= 0)
                return false;

            int footprintCount = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int cellIndex = 0; cellIndex < footprintCount; cellIndex++)
            {
                Vector2Int footprintCell = BuildingFootprintUtility.GetOccupiedCell(
                    definition,
                    request.Position,
                    cellIndex,
                    request.Rotation);
                for (int offsetX = -spacing; offsetX <= spacing; offsetX++)
                {
                    for (int offsetY = -spacing; offsetY <= spacing; offsetY++)
                    {
                        if (offsetX == 0 && offsetY == 0)
                            continue;

                        var neighbor = new Vector2Int(footprintCell.x + offsetX, footprintCell.y + offsetY);
                        if (BuildingFootprintUtility.Contains(
                                definition,
                                request.Position,
                                neighbor,
                                request.Rotation))
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

        private static int ResolveSpacing(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition)
        {
            if (!BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out SpacingPlacementRuleModule module))
            {
                return Mathf.Max(0, request.MinSpacing);
            }

            return module.MergeMode switch
            {
                PlacementRuleMergeMode.Disabled => 0,
                PlacementRuleMergeMode.Override =>
                    Mathf.Max(0, module.MinimumSpacing),
                _ => Mathf.Max(0, request.MinSpacing),
            };
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
                if (!BuildingFootprintUtility.Contains(
                        pendingDefinition,
                        pending.Position,
                        position,
                        pending.Rotation))
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

        private static bool IsOwnedByPlacementOwner(
            BuildingPlacementEvaluationRequest request,
            Vector2Int origin)
        {
            if (request.GetOccupantOwnerId == null)
                return true;

            return IsSameOwnerOrUnknown(
                request.OwnerId,
                request.GetOccupantOwnerId(origin));
        }

        private static bool IsSameOwnerOrUnknown(
            string placementOwnerId,
            string existingOwnerId)
        {
            if (string.IsNullOrWhiteSpace(placementOwnerId)
                || string.IsNullOrWhiteSpace(existingOwnerId))
            {
                return true;
            }

            return string.Equals(
                placementOwnerId.Trim(),
                existingOwnerId.Trim(),
                StringComparison.Ordinal);
        }
    }
}
