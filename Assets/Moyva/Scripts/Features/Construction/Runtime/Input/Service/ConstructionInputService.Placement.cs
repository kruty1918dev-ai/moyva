using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionInputService
    {
        private void HandlePointerSelection(Vector2 screenPos, int pointerId, bool allowDragStart, bool selectionOnRelease, bool skipUiCheck = false)
        {
            if (!skipUiCheck && IsPointerOverInteractiveUI(screenPos, pointerId))
            {
                return;
            }

            if (!TryResolvePointerTile(screenPos, out Vector2Int tilePos))
            {
                return;
            }

            if (!_gridService.TryGetTileData(tilePos, out _))
            {
                return;
            }

            if (_constructionService.IsDemolishMode && IsPlacementSessionInactive())
            {
                ClearTouchPlacementState();
                bool result = _constructionService.TryDemolishAt(tilePos);
                return;
            }

            if (IsPlacementSessionActive())
            {
                HandlePlacementSelection(tilePos, allowDragStart, selectionOnRelease);
                return;
            }
        }

        private void HandlePlacementSelection(Vector2Int tilePos, bool allowDragStart, bool selectionOnRelease)
        {
            string selectedBuildingId = _constructionService.GetSelectedBuildingId();
            bool wallMode = !string.IsNullOrWhiteSpace(selectedBuildingId) && _wallTopologyService.IsWall(selectedBuildingId);

            if (selectionOnRelease)
            {
                HandleReleaseSelectionPlacement(tilePos, selectedBuildingId, wallMode);
                return;
            }

            if (wallMode && _objectsMapService.TryGetOccupant(tilePos, out var occupantId) && _wallTopologyService.IsWallOrGate(occupantId))
            {
                _wallHandleController.Show(tilePos);
                _isDraggingWallPath = allowDragStart;
                _wallDragStartPosition = tilePos;
                _lastWallDragTile = tilePos;
                _wallDragPendingPositions.Clear();
                _wallDragPendingPositions.Add(tilePos);

                return;
            }

            if (_constructionService.HasPendingPlacementAt(tilePos))
            {
                bool gateMode = !string.IsNullOrWhiteSpace(selectedBuildingId)
                    && _wallTopologyService.IsGate(selectedBuildingId);
                if (gateMode)
                {
                    if (!IsPointerPlacementAllowed(tilePos, selectedBuildingId))
                        return;

                    bool placed = _constructionService.TryPreviewAt(tilePos);
                    return;
                }

                _isDraggingPendingPlacement = allowDragStart && _enableMousePendingPreviewDrag;
                _draggedPlacementPosition = tilePos;
                ClearPendingPlacementSnapTarget();

                return;
            }

            if (wallMode)
            {
                if (!IsPointerPlacementAllowed(tilePos, selectedBuildingId))
                    return;

                bool placed = _constructionService.TryPreviewAt(tilePos);
                if (placed && allowDragStart)
                {
                    _isDraggingWallPath = true;
                    _wallDragStartPosition = tilePos;
                    _lastWallDragTile = tilePos;
                    _wallDragPendingPositions.Clear();
                    _wallDragPendingPositions.Add(tilePos);
                }

                return;
            }

            bool result = _constructionService.TryPreviewAt(tilePos);

            if (result && allowDragStart && _enableMousePendingPreviewDrag)
            {
                _isDraggingPendingPlacement = true;
                _draggedPlacementPosition = tilePos;
                ClearPendingPlacementSnapTarget();
            }
        }

        private bool IsPointerPlacementAllowed(
            Vector2Int position,
            string buildingId)
        {
            if (_placementQuery == null || string.IsNullOrWhiteSpace(buildingId))
                return false;

            ConstructionPlacementQueryResult result =
                _placementQuery.EvaluatePlacement(
                    new ConstructionPlacementQueryRequest(
                        buildingId,
                        position,
                        includeResources: true,
                        includeDetails: true,
                        ownerId: _constructionService.GetActiveOwner(),
                        attemptSource:
                            ConstructionPlacementAttemptSource.PointerClick,
                        allowUniquePreviewRelocation: true,
                        rotation: ResolveSelectedRotation()));
            if (result.CanPreview)
                return true;
            _signalBus.Fire(
                new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = buildingId,
                    PreviewState = BuildingPreviewState.Blocked,
                });
            return false;
        }
    }
}
