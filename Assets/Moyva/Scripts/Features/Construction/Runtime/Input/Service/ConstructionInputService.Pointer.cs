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
        private void HandlePointerRelease(ConstructionPointerSnapshot pointer)
        {
            if (!pointer.WasReleasedThisFrame)
                return;

            if (_isDraggingPendingPlacement)
            {
                if (VerboseLogs)
                    Debug.Log($"{LogTag} Drag ended at {_draggedPlacementPosition}.");

                SnapPendingPlacementToPointerTile(pointer.Position);
                PublishPendingPlacementDragVisual(pointer.Position, _draggedPlacementPosition, snapToGrid: true);
                _isDraggingPendingPlacement = false;
            }

            if (_isDraggingWallPath)
            {
                _isDraggingWallPath = false;
                _wallDragPendingPositions.Clear();
                _wallHandleController.EndDrag();
                EndWallUndoBatch();

                if (VerboseLogs)
                    Debug.Log($"{LogTag} Wall drag ended at {_lastWallDragTile}.");
            }
        }

        private bool TryResolvePointerTile(Vector2 screenPosition, out Vector2Int tilePosition)
        {
            tilePosition = _screenToGrid.ScreenToGrid(screenPosition);
            return _gridService != null && _gridService.TryGetTileData(tilePosition, out _);
        }

        private Camera ResolveCamera()
        {
            if (_cachedCamera != null && _cachedCamera.isActiveAndEnabled)
                return _cachedCamera;

            _cachedCamera = Camera.main;
            return _cachedCamera != null && _cachedCamera.isActiveAndEnabled
                ? _cachedCamera
                : null;
        }

        private bool TryHandleWallPathDrag(ConstructionPointerSnapshot pointer)
        {
            if (!_isDraggingWallPath || !pointer.IsPressed)
                return false;

            if (IsPointerOverInteractiveUI(pointer.Position, pointer.PointerId))
                return true;

            if (!TryResolvePointerTile(pointer.Position, out Vector2Int dragTilePos))
                return true;

            if (dragTilePos != _lastWallDragTile)
                PreviewWallPathTo(dragTilePos);

            return true;
        }

        private bool TryHandlePendingPlacementDrag(ConstructionPointerSnapshot pointer)
        {
            if (!_isDraggingPendingPlacement || !pointer.IsPressed)
                return false;

            if (IsPointerOverInteractiveUI(pointer.Position, pointer.PointerId))
                return true;

            PublishPendingPlacementDragVisual(pointer.Position, _draggedPlacementPosition, snapToGrid: false);
            return true;
        }

        private void SnapPendingPlacementToPointerTile(Vector2 screenPosition)
        {
            if (TryResolvePendingPlacementSnapTarget(screenPosition, out Vector2Int snappedPosition)
                && TryMoveDraggedPlacementTo(snappedPosition))
            {
                if (VerboseLogs && snappedPosition != _draggedPlacementPosition)
                    Debug.Log($"{LogTag} Drag snapped preview: {_draggedPlacementPosition} -> {snappedPosition}");

                _draggedPlacementPosition = snappedPosition;
            }

            ClearPendingPlacementSnapTarget();
        }

        private bool TryMoveDraggedPlacementTo(Vector2Int targetPosition)
        {
            if (!_gridService.TryGetTileData(targetPosition, out _))
                return false;

            if (targetPosition == _draggedPlacementPosition)
                return true;

            return TryResolveDraggedPlacementBuildingId(out string buildingId)
                && IsBuildGridPlacementAllowed(targetPosition, buildingId, _draggedPlacementPosition)
                && _constructionService.TryMovePendingPlacement(_draggedPlacementPosition, targetPosition);
        }

        private bool IsBuildGridPlacementAllowed(Vector2Int position, string buildingId, Vector2Int? ignoredPendingPosition = null)
        {
            if (_placementQuery == null || string.IsNullOrWhiteSpace(buildingId))
                return false;

            if (_hasPlacementValidationCache
                && _cachedPlacementValidationPosition == position
                && string.Equals(
                    _cachedPlacementValidationBuildingId,
                    buildingId,
                    StringComparison.Ordinal)
                && _cachedPlacementValidationIgnoredPendingPosition == ignoredPendingPosition)
            {
                return _cachedPlacementValidationAllowed;
            }

            bool allowed = _placementQuery.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    position,
                    ignoredPendingPosition,
                    includeResources: true,
                    attemptSource:
                        ConstructionPlacementAttemptSource.DragValidation,
                    allowUniquePreviewRelocation: true,
                    rotation: ResolvePlacementRotation(
                        ignoredPendingPosition))).CanPreview;

            _hasPlacementValidationCache = true;
            _cachedPlacementValidationPosition = position;
            _cachedPlacementValidationBuildingId = buildingId;
            _cachedPlacementValidationIgnoredPendingPosition = ignoredPendingPosition;
            _cachedPlacementValidationAllowed = allowed;
            return allowed;
        }

        private ConstructionRotation ResolveSelectedRotation()
            => _constructionService
                is IConstructionRotationService rotationService
                ? rotationService.SelectedRotation
                : ConstructionRotation.Degrees0;

        private ConstructionRotation ResolvePlacementRotation(
            Vector2Int? pendingPosition)
        {
            if (pendingPosition.HasValue
                && _constructionService
                    is IConstructionRotationService rotationService
                && rotationService.TryGetPendingRotation(
                    pendingPosition.Value,
                    out ConstructionRotation pendingRotation))
            {
                return pendingRotation;
            }

            return ResolveSelectedRotation();
        }

        private static ConstructionBuildGridTileVisualState ResolvePlacementVisualState(
            ConstructionPlacementQueryResult result)
        {
            if (!result.CanSelect)
                return ConstructionBuildGridTileVisualState.General;
            if (!result.SpatialValid)
                return ConstructionBuildGridTileVisualState.Invalid;
            return result.ResourcesValid
                ? ConstructionBuildGridTileVisualState.Valid
                : ConstructionBuildGridTileVisualState.Unaffordable;
        }

        private void PublishPendingPlacementDragVisual(Vector2 screenPosition, Vector2Int tilePosition, bool snapToGrid)
        {
            // DragPlacementFix: use the same surface-aware screen mapping as
            // normal construction hover. Grid-plane world projection is only
            // used to obtain smooth XZ cursor following after the target tile
            // has already been resolved.
            string buildingId = _constructionService.GetSelectedBuildingId();
            if (string.IsNullOrWhiteSpace(buildingId)
                && !_constructionService.TryGetPendingBuildingIdAt(tilePosition, out buildingId))
            {
                return;
            }

            Vector2Int snapTargetPosition = tilePosition;
            bool hasSnapTarget = !snapToGrid
                && TryResolveActualPointerTile(
                    screenPosition,
                    out snapTargetPosition);

            Vector2Int pointerSurfaceTile = hasSnapTarget
                ? snapTargetPosition
                : tilePosition;

            Vector3 worldPosition = snapToGrid
                ? Vector3.zero
                : ResolvePointerWorldOnConstructionPlane(
                    screenPosition,
                    pointerSurfaceTile);

            bool isSnapTargetValid = hasSnapTarget
                && IsBuildGridPlacementAllowed(
                    snapTargetPosition,
                    buildingId,
                    tilePosition);

            CachePendingPlacementSnapTarget(
                isSnapTargetValid,
                snapTargetPosition);

            _signalBus.Fire(new BuildingPreviewDragVisualSignal
            {
                Position = tilePosition,
                BuildingId = buildingId,
                WorldPosition = worldPosition,
                SnapToGrid = snapToGrid,
                HasSnapTarget = hasSnapTarget,
                SnapTargetPosition = hasSnapTarget
                    ? snapTargetPosition
                    : tilePosition,
                IsSnapTargetValid = isSnapTargetValid,
            });
        }


        private bool TryResolveActualPointerTile(
            Vector2 screenPosition,
            out Vector2Int tile)
        {
            // DragPlacementFix: ScreenToGridConverter already resolves the
            // nearest generated terrain surface. Do not convert a point from
            // the lower base grid plane back into a cell.
            tile = _screenToGrid.ScreenToGrid(screenPosition);
            return _gridService != null
                && _gridService.TryGetTileData(tile, out _);
        }


        private bool TryResolvePendingPlacementSnapTarget(Vector2 screenPosition, out Vector2Int tile)
        {
            if (_hasPendingPlacementSnapTarget)
            {
                tile = _pendingPlacementSnapTarget;
                return true;
            }

            if (!TryResolveDraggedPlacementBuildingId(out string buildingId))
            {
                tile = default;
                return false;
            }

            // DragPlacementFix: release uses exactly the same terrain-aware
            // tile mapping that was shown while dragging.
            return TryResolveActualPointerTile(screenPosition, out tile)
                && IsBuildGridPlacementAllowed(
                    tile,
                    buildingId,
                    _draggedPlacementPosition);
        }


        private void CachePendingPlacementSnapTarget(bool hasSnapTarget, Vector2Int snapTargetPosition)
        {
            _hasPendingPlacementSnapTarget = hasSnapTarget;
            _pendingPlacementSnapTarget = hasSnapTarget ? snapTargetPosition : default;
        }

        private void ClearPendingPlacementSnapTarget()
        {
            _hasPendingPlacementSnapTarget = false;
            _pendingPlacementSnapTarget = default;
        }

        private bool TryResolveDraggedPlacementBuildingId(out string buildingId)
        {
            buildingId = _constructionService.GetSelectedBuildingId();
            if (!string.IsNullOrWhiteSpace(buildingId))
                return true;

            return _constructionService.TryGetPendingBuildingIdAt(_draggedPlacementPosition, out buildingId);
        }

        private Vector2Int ResolvePointerGridTile(Vector3 pointerWorld, Vector2 screenPosition)
        {
            if (_gridGeometry != null && _gridGeometry.TryGetCellAtWorld(pointerWorld, out Vector2Int tile))
                return tile;

            return _screenToGrid.ScreenToGrid(screenPosition);
        }

        private Vector3 ResolvePointerWorldOnConstructionPlane(Vector2 screenPosition, Vector2Int fallbackTile)
        {
            Camera camera = ResolveCamera();

            float planeY;
            if (_terrainAlignment != null)
            {
                // DragPlacementFix: project the pointer onto the visible
                // terrain surface of the resolved target tile, not the base
                // construction plane underneath elevated terrain.
                planeY = _terrainAlignment
                    .ResolveWorldPosition(fallbackTile, 0f)
                    .y;
            }
            else
            {
                planeY = _gridGeometry != null
                    && _gridGeometry.TryGetGridPlaneY(out float gridPlaneY)
                        ? gridPlaneY
                        : PointerFollowPlaneFallbackY;
            }

            if (camera == null)
                return new Vector3(
                    fallbackTile.x,
                    planeY,
                    fallbackTile.y);

            Ray ray = camera.ScreenPointToRay(screenPosition);
            Plane plane = new(
                Vector3.up,
                new Vector3(0f, planeY, 0f));

            return plane.Raycast(ray, out float distance)
                ? ray.GetPoint(distance)
                : new Vector3(
                    fallbackTile.x,
                    planeY,
                    fallbackTile.y);
        }



        private ConstructionPointerSnapshot ReadPointerSnapshot() => _pointerInputSource.ReadPointerSnapshot();
    }
}
