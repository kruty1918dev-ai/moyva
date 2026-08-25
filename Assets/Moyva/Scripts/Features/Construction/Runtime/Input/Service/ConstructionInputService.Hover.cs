using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.UIActions.API;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionInputService
    {
        private void UpdateBuildGridHover(ConstructionPointerSnapshot pointer)
        {
            if (pointer.ActivePointerCount > 1
                || IsPointerOverInteractiveUI(pointer.Position, pointer.PointerId)
                || !TryResolvePointerTile(pointer.Position, out Vector2Int tile))
            {
                ClearBuildGridHover();
                return;
            }

            string buildingId = IsPlacementSessionActive() && !_constructionService.IsDemolishMode
                ? _constructionService.GetSelectedBuildingId()
                : null;
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                if (!_buildGridState.SetHover(tile, ConstructionBuildGridTileVisualState.General))
                    return;

                PublishBuildGridHover(
                    tile,
                    null,
                    true,
                    true,
                    new[] { tile },
                    Array.Empty<Vector2Int>());
                return;
            }

            if (_buildGridState.HoverPosition == tile)
                return;

            ConstructionPlacementQueryResult detailed = _placementQuery.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    tile,
                    includeResources: true,
                    includeDetails: true,
                    attemptSource:
                        ConstructionPlacementAttemptSource.PointerHover,
                    allowUniquePreviewRelocation: true,
                    rotation: ResolveSelectedRotation()));
            ConstructionBuildGridTileVisualState visualState =
                ResolvePlacementVisualState(detailed);
            if (!_buildGridState.SetHover(tile, visualState))
                return;
            BuildHoverFootprintArrays(
                tile,
                detailed,
                out Vector2Int[] footprintPositions,
                out Vector2Int[] invalidPositions);
            PublishBuildGridHover(
                tile,
                buildingId,
                detailed.CanPreview,
                detailed.ResourcesValid,
                footprintPositions,
                invalidPositions);
        }

        private void ClearBuildGridHover()
        {
            if (_buildGridState == null || !_buildGridState.ClearHover())
                return;

            _signalBus.Fire(new BuildGridHoverChangedSignal
            {
                HasTile = false,
                FootprintPositions = Array.Empty<Vector2Int>(),
                InvalidFootprintPositions = Array.Empty<Vector2Int>(),
            });
            _buildGridDiagnostics?.LogHoverChanged(
                false,
                default,
                ConstructionBuildGridTileVisualState.Missing);
        }

        private void InvalidateBuildGridHover(BuildingSelectionChangedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(BuildingPreviewChangedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(BuildingPreviewMovedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(OnObjectsMapChangedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(GridTileChangedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(FogStateChangedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(SettlementResourceChangedSignal _) => InvalidatePlacementInteractionCaches();

        private void InvalidatePlacementInteractionCaches()
        {
            ClearBuildGridHover();
            ClearPlacementValidationCache();
        }

        private void ClearPlacementValidationCache()
        {
            _hasPlacementValidationCache = false;
            _cachedPlacementValidationPosition = default;
            _cachedPlacementValidationBuildingId = null;
            _cachedPlacementValidationIgnoredPendingPosition = null;
            _cachedPlacementValidationAllowed = false;
        }

        private void PublishBuildGridHover(
            Vector2Int tile,
            string buildingId,
            bool isPlacementValid,
            bool isAffordable,
            Vector2Int[] footprintPositions,
            Vector2Int[] invalidPositions)
        {
            _signalBus.Fire(new BuildGridHoverChangedSignal
            {
                HasTile = true,
                Position = tile,
                BuildingId = buildingId,
                IsPlacementValid = isPlacementValid,
                IsAffordable = isAffordable,
                RotationQuarterTurns =
                    (int)ResolveSelectedRotation(),
                FootprintPositions = footprintPositions,
                InvalidFootprintPositions = invalidPositions,
            });
            _buildGridDiagnostics?.LogHoverChanged(
                true,
                tile,
                _buildGridState.HoverVisualState);
        }

        private static void BuildHoverFootprintArrays(
            Vector2Int origin,
            ConstructionPlacementQueryResult result,
            out Vector2Int[] footprintPositions,
            out Vector2Int[] invalidPositions)
        {
            BuildingPlacementEvaluationResult evaluation = result.EvaluationResult;
            if (evaluation == null || evaluation.FootprintPositions.Count == 0)
            {
                footprintPositions = new[] { origin };
                invalidPositions = result.SpatialValid
                    ? Array.Empty<Vector2Int>()
                    : new[] { origin };
                return;
            }

            footprintPositions = new Vector2Int[evaluation.FootprintPositions.Count];
            for (int index = 0; index < footprintPositions.Length; index++)
                footprintPositions[index] = evaluation.FootprintPositions[index];

            if (!result.SpatialValid)
            {
                invalidPositions = (Vector2Int[])footprintPositions.Clone();
                return;
            }

            invalidPositions = Array.Empty<Vector2Int>();
        }

    }
}
