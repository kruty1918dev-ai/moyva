using System;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private long _nextPlacementAttemptId;

        private ConstructionPlacementQueryRequest ResolveEffectivePlacementRequest(
            ConstructionPlacementQueryRequest request,
            string ownerId)
        {
            if (!request.AllowUniquePreviewRelocation
                || request.IgnoredPendingPosition.HasValue
                || request.IgnoredOccupiedPosition.HasValue
                || string.IsNullOrWhiteSpace(request.BuildingId))
            {
                return request;
            }

            BuildingDefinition definition =
                _placementBuildingRegistry?.GetById(request.BuildingId);
            BuildingPlacementUniquenessScope scope =
                BuildingDefinitionCapabilities.GetPlacementUniquenessScope(definition);
            if (scope == BuildingPlacementUniquenessScope.None)
                return request;

            if (TryFindPendingPlacementByBuildingId(request.BuildingId, out int pendingIndex))
            {
                PendingPlacement pending = _pendingPlacements[pendingIndex];
                return request.WithIgnoredPositions(
                    pending.Position,
                    pending.OriginalPosition);
            }

            return TryFindPlacedBuildingPosition(
                    request.BuildingId,
                    ownerId,
                    scope,
                    out Vector2Int originalPosition)
                ? request.WithIgnoredPositions(null, originalPosition)
                : request;
        }

        private ConstructionPlacementQueryResult CreatePlacementQueryResult(
            bool isSpatiallyValid,
            bool resourcesValid,
            bool isGateReplacement,
            string reason,
            BuildingPlacementEvaluationResult evaluationResult,
            ConstructionPlacementQueryRequest request,
            string ownerId,
            BuildingPerPlayerLimitEvaluation limitEvaluation,
            string explicitReasonCode = null)
        {
            if (!request.IncludeDetails
                && !ShouldCaptureDiagnostic(request.AttemptSource))
            {
                return new ConstructionPlacementQueryResult(
                    isSpatiallyValid,
                    resourcesValid,
                    isGateReplacement,
                    reason,
                    evaluationResult);
            }

            Vector2Int contextPosition = request.Position;
            BuildingPlacementBlocker firstBlocker =
                evaluationResult != null
                && evaluationResult.Blockers.Count > 0
                    ? evaluationResult.Blockers[0]
                    : null;
            if (firstBlocker != null && firstBlocker.Position.HasValue)
                contextPosition = firstBlocker.Position.Value;

            string terrainReason = null;
            if (!isSpatiallyValid)
                IsBlockedByTerrain(contextPosition, out terrainReason);

            string fogState = null;
            if (_fogOfWarService != null)
            {
                try
                {
                    fogState = _fogOfWarService
                        .GetFogState(contextPosition)
                        .ToString();
                }
                catch (Exception ex)
                {
                    fogState = $"error:{ex.GetType().Name}";
                }
            }

            int? terrainLevel =
                GetTerrainLevelForPlacementQuery(contextPosition);
            string tileId = GetTileId(contextPosition);
            string reasonCode =
                ConstructionPlacementDiagnosticFormatter.ResolveReasonCode(
                    evaluationResult,
                    isSpatiallyValid,
                    resourcesValid,
                    explicitReasonCode);
            var diagnostic = new ConstructionPlacementDiagnostic(
                ++_nextPlacementAttemptId,
                request.AttemptSource,
                request.BuildingId,
                request.Position,
                contextPosition,
                ownerId,
                isSpatiallyValid,
                resourcesValid,
                isGateReplacement,
                reasonCode,
                reason,
                tileId,
                terrainLevel,
                terrainReason,
                fogState,
                limitEvaluation.Limit,
                limitEvaluation.ExistingCount,
                limitEvaluation.PendingCount,
                request.IgnoredPendingPosition,
                request.IgnoredOccupiedPosition,
                evaluationResult?.Blockers);

            return new ConstructionPlacementQueryResult(
                isSpatiallyValid,
                resourcesValid,
                isGateReplacement,
                reason,
                evaluationResult,
                diagnostic);
        }

        private void LogPlacementAttempt(
            ConstructionPlacementQueryResult result,
            bool emitRejectedAction)
        {
            ConstructionPlacementDiagnostic diagnostic = result.Diagnostic;
            if (diagnostic == null)
                return;

            if (!diagnostic.IsValid)
            {
                if (emitRejectedAction || ShouldAlwaysLogSource(diagnostic.Source))
                    Debug.LogWarning(ConstructionPlacementDiagnosticFormatter.FormatSingleLine(diagnostic));
                return;
            }

            if (VerboseLogs && ShouldAlwaysLogSource(diagnostic.Source))
                Debug.Log(ConstructionPlacementDiagnosticFormatter.FormatSingleLine(diagnostic));
        }

        private void LogSyntheticPlacementRejection(
            ConstructionPlacementAttemptSource source,
            string buildingId,
            Vector2Int position,
            string ownerId,
            string reasonCode,
            string reason)
        {
            int? terrainLevel = GetTerrainLevelForPlacementQuery(position);
            string terrainReason = null;
            IsBlockedByTerrain(position, out terrainReason);

            string fogState = null;
            if (_fogOfWarService != null)
            {
                try
                {
                    fogState = _fogOfWarService.GetFogState(position).ToString();
                }
                catch (Exception ex)
                {
                    fogState = $"error:{ex.GetType().Name}";
                }
            }

            var diagnostic = new ConstructionPlacementDiagnostic(
                ++_nextPlacementAttemptId,
                source,
                buildingId,
                position,
                position,
                ownerId,
                isSpatiallyValid: false,
                resourcesValid: false,
                isGateReplacement: false,
                reasonCode,
                reason,
                GetTileId(position),
                terrainLevel,
                terrainReason,
                fogState,
                perPlayerLimit: 0,
                existingOwnedCount: 0,
                pendingOwnedCount: 0,
                ignoredPendingPosition: null,
                ignoredOccupiedPosition: null,
                blockers: null);
            Debug.LogWarning(ConstructionPlacementDiagnosticFormatter.FormatSingleLine(diagnostic));
        }

        private static bool ShouldCaptureDiagnostic(
            ConstructionPlacementAttemptSource source)
        {
            switch (source)
            {
                case ConstructionPlacementAttemptSource.PointerClick:
                case ConstructionPlacementAttemptSource.PreviewMove:
                case ConstructionPlacementAttemptSource.Confirm:
                case ConstructionPlacementAttemptSource.DirectPlace:
                case ConstructionPlacementAttemptSource.WallPath:
                case ConstructionPlacementAttemptSource.NetworkRequest:
                    return true;
                default:
                    return false;
            }
        }

        private static bool ShouldAlwaysLogSource(
            ConstructionPlacementAttemptSource source)
        {
            switch (source)
            {
                case ConstructionPlacementAttemptSource.PointerClick:
                case ConstructionPlacementAttemptSource.PreviewMove:
                case ConstructionPlacementAttemptSource.Confirm:
                case ConstructionPlacementAttemptSource.DirectPlace:
                case ConstructionPlacementAttemptSource.WallPath:
                case ConstructionPlacementAttemptSource.NetworkRequest:
                    return true;
                default:
                    return false;
            }
        }
    }
}
