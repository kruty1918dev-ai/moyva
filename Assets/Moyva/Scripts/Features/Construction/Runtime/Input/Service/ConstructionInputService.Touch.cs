using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionInputService
    {
        private bool TryHandleReleaseSelectionInput(ConstructionPointerSnapshot pointer)
        {
            if (!pointer.SelectOnRelease)
                return false;

            if (pointer.ActivePointerCount > 1)
            {
                if (!_enableMultiTouchCancel)
                    return true;

                if (VerboseLogs)
                    Debug.Log($"{LogTag} Multi-touch detected. Active drags cancelled.");

                CancelActivePointerDrags();
                _touchTapTracker.Reset();
                return true;
            }

            if (!_touchTapTracker.IsTracking && pointer.WasPressedThisFrame)
            {
                BeginReleaseSelectionTracking(pointer);
                return true;
            }

            if (pointer.PointerId != _touchTapTracker.TrackedTouchId)
                return pointer.ActivePointerCount > 0;

            if (pointer.IsPressed)
            {
                if (TryHandleReleaseSelectionPendingPlacementDrag(pointer))
                    return true;

                _touchTapTracker.UpdateMove(pointer.Position, _touchTapMaxMovePixels);
                return true;
            }

            if (pointer.WasReleasedThisFrame)
            {
                CompleteReleaseSelectionTracking(pointer.Position, pointer.PointerId);
                return true;
            }

            if (_touchTapTracker.IsTracking)
                _touchTapTracker.Reset();

            return false;
        }

        private void BeginReleaseSelectionTracking(ConstructionPointerSnapshot pointer)
        {
            Vector2 startScreenPosition = pointer.Position;
            bool startedOverUi = IsPointerOverInteractiveUI(startScreenPosition, pointer.PointerId);
            bool multiTouchObserved = pointer.ActivePointerCount > 1;
            _touchTapTracker.Begin(pointer.PointerId, startScreenPosition, Time.unscaledTime, startedOverUi, multiTouchObserved);
            TryBeginReleaseSelectionPendingPlacementDragCandidate(startScreenPosition, startedOverUi);

            if (VerboseLogs)
                Debug.Log($"{LogTag} Touch tracking started. touchId={pointer.PointerId}, startedOverUi={startedOverUi}.");
        }

        private void CompleteReleaseSelectionTracking(Vector2 releaseScreenPosition, int touchId)
        {
            if (_isDraggingPendingPlacement)
            {
                if (VerboseLogs)
                    Debug.Log($"{LogTag} Touch drag ended at {_draggedPlacementPosition}. touchId={touchId}");

                SnapPendingPlacementToPointerTile(releaseScreenPosition);
                PublishPendingPlacementDragVisual(releaseScreenPosition, _draggedPlacementPosition, snapToGrid: true);
                _isDraggingPendingPlacement = false;
                ClearTouchPendingDragCandidate();
                _touchTapTracker.Reset();
                return;
            }

            bool isTap = _touchTapTracker.Complete(Time.unscaledTime, _touchTapMaxDurationSeconds);

            if (VerboseLogs)
                Debug.Log($"{LogTag} Touch tracking completed. touchId={touchId}, isTap={isTap}.");

            if (isTap)
                HandlePointerSelection(releaseScreenPosition, touchId, allowDragStart: false, selectionOnRelease: true);

            ClearTouchPendingDragCandidate();
            _touchTapTracker.Reset();
        }

        private void TryBeginReleaseSelectionPendingPlacementDragCandidate(Vector2 startScreenPosition, bool startedOverUi)
        {
            ClearTouchPendingDragCandidate();

            if (!_enableTouchPendingPreviewDrag || startedOverUi || IsPlacementSessionInactive())
                return;

            string selectedBuildingId = _constructionService.GetSelectedBuildingId();
            bool wallMode = !string.IsNullOrWhiteSpace(selectedBuildingId) && _wallTopologyService.IsWall(selectedBuildingId);
            if (wallMode)
                return;

            if (!TryResolvePointerTile(startScreenPosition, out Vector2Int startTile))
                return;

            if (!_constructionService.HasPendingPlacementAt(startTile))
                return;

            _hasTouchPendingDragCandidate = true;
            _touchPendingDragCandidatePosition = startTile;

            if (VerboseLogs)
                Debug.Log($"{LogTag} Touch drag candidate started at {startTile}.");
        }

        private bool TryHandleReleaseSelectionPendingPlacementDrag(ConstructionPointerSnapshot pointer)
        {
            if (!_hasTouchPendingDragCandidate && !_isDraggingPendingPlacement)
                return false;

            Vector2 screenPosition = pointer.Position;
            if (IsPointerOverInteractiveUI(screenPosition, pointer.PointerId))
                return true;

            _touchTapTracker.UpdateMove(screenPosition, _touchTapMaxMovePixels);
            if (!_isDraggingPendingPlacement)
            {
                if (!_touchTapTracker.HasMovedBeyondTap)
                    return true;

                _isDraggingPendingPlacement = true;
                _draggedPlacementPosition = _touchPendingDragCandidatePosition;
                ClearPendingPlacementSnapTarget();

                if (VerboseLogs)
                    Debug.Log($"{LogTag} Touch drag started at {_draggedPlacementPosition}.");
            }

            PublishPendingPlacementDragVisual(screenPosition, _draggedPlacementPosition, snapToGrid: false);
            return true;
        }

        private void HandleReleaseSelectionPlacement(Vector2Int tilePos, string selectedBuildingId, bool wallMode)
        {
            bool gateMode = !string.IsNullOrWhiteSpace(selectedBuildingId)
                && _wallTopologyService.IsGate(selectedBuildingId);

            if (gateMode)
            {
                ClearTouchPendingMoveSource();
                ClearTouchWallAnchor();
                if (!IsBuildGridPlacementAllowed(tilePos, selectedBuildingId))
                {
                    if (VerboseLogs)
                        Debug.Log($"{LogTag} Touch gate placement rejected by build grid at {tilePos}");
                    return;
                }

                bool gatePlaced = _constructionService.TryPreviewAt(tilePos);
                if (VerboseLogs)
                    Debug.Log($"{LogTag} Touch gate placement at {tilePos} => {gatePlaced}");
                return;
            }

            if (wallMode)
            {
                ClearTouchPendingMoveSource();
                HandleTouchWallSelection(tilePos);
                return;
            }

            ClearTouchWallAnchor();

            if (_hasTouchPendingMoveSource)
            {
                if (_constructionService.HasPendingPlacementAt(tilePos))
                {
                    _touchPendingMoveSourcePosition = tilePos;
                    if (VerboseLogs)
                        Debug.Log($"{LogTag} Touch move source changed to {tilePos}.");
                    return;
                }

                if (!_constructionService.TryGetPendingBuildingIdAt(_touchPendingMoveSourcePosition, out string movingBuildingId)
                    || !IsBuildGridPlacementAllowed(tilePos, movingBuildingId, _touchPendingMoveSourcePosition))
                {
                    if (VerboseLogs)
                        Debug.Log($"{LogTag} Touch move rejected by build grid: {_touchPendingMoveSourcePosition} -> {tilePos}");
                    return;
                }

                bool moved = _constructionService.TryMovePendingPlacement(_touchPendingMoveSourcePosition, tilePos);
                if (VerboseLogs)
                    Debug.Log($"{LogTag} Touch move preview {_touchPendingMoveSourcePosition} -> {tilePos} => {moved}");

                if (moved)
                    ClearTouchPendingMoveSource();

                return;
            }

            if (_constructionService.HasPendingPlacementAt(tilePos))
            {
                _hasTouchPendingMoveSource = true;
                _touchPendingMoveSourcePosition = tilePos;

                if (VerboseLogs)
                    Debug.Log($"{LogTag} Touch move source selected at {tilePos}.");

                return;
            }

            if (!IsBuildGridPlacementAllowed(tilePos, selectedBuildingId))
            {
                if (VerboseLogs)
                    Debug.Log($"{LogTag} Touch placement rejected by build grid at {tilePos}");
                return;
            }

            bool placed = _constructionService.TryPreviewAt(tilePos);
            if (VerboseLogs)
                Debug.Log($"{LogTag} Touch preview at {tilePos} => {placed}");
        }

        private void HandleTouchWallSelection(Vector2Int tilePos)
        {
            if (_hasTouchWallAnchor)
            {
                if (tilePos != _lastWallDragTile)
                    PreviewWallPathTo(tilePos);

                if (VerboseLogs)
                    Debug.Log($"{LogTag} Touch wall path completed: {_touchWallAnchorPosition} -> {tilePos}");

                ClearTouchWallAnchor();
                return;
            }

            bool startsFromExistingWall = _objectsMapService.TryGetOccupant(tilePos, out var occupantId)
                && _wallTopologyService.IsWallOrGate(occupantId);
            bool startsFromPendingWall = _constructionService.TryGetPendingBuildingIdAt(tilePos, out var pendingId)
                && _wallTopologyService.IsWallOrGate(pendingId);

            if (startsFromExistingWall)
            {
                _wallHandleController.Show(tilePos);
            }
            else if (!startsFromPendingWall)
            {
                string selectedBuildingId = _constructionService.GetSelectedBuildingId();
                if (!IsBuildGridPlacementAllowed(tilePos, selectedBuildingId))
                {
                    if (VerboseLogs)
                        Debug.Log($"{LogTag} Touch wall anchor at {tilePos} rejected by build grid.");
                    return;
                }

                bool placed = _constructionService.TryPreviewAt(tilePos);
                if (!placed)
                {
                    if (VerboseLogs)
                        Debug.Log($"{LogTag} Touch wall anchor at {tilePos} rejected.");
                    return;
                }
            }

            _hasTouchWallAnchor = true;
            _touchWallAnchorPosition = tilePos;
            _wallDragStartPosition = tilePos;
            _lastWallDragTile = tilePos;
            _wallDragPendingPositions.Clear();
            _wallDragPendingPositions.Add(tilePos);

            if (VerboseLogs)
                Debug.Log($"{LogTag} Touch wall anchor set at {tilePos}.");
        }
    }
}
