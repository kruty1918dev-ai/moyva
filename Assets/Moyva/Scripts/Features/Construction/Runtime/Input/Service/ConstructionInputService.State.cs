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
        private bool IsPointerOverInteractiveUI(Vector2 screenPosition, int pointerId)
        {
            if (!_blockInteractiveUi || _uiHitTester == null)
                return false;

            return _allowClicksThroughNonInteractiveUi
                ? _uiHitTester.IsPointerOverInteractiveUI(screenPosition, pointerId)
                : _uiHitTester.IsPointerOverAnyUI(screenPosition, pointerId);
        }

        // From ConstructionInputService.State.cs
        private void CancelActivePointerDrags()
        {
            _isDraggingPendingPlacement = false;
            ClearPendingPlacementSnapTarget();
            ClearTouchPendingDragCandidate();

            if (_isDraggingWallPath)
            {
                _wallHandleController.EndDrag();
                _wallDragPendingPositions.Clear();
            }

            _isDraggingWallPath = false;
        }

        private void CancelActiveDrags()
        {
            CancelActivePointerDrags();
            ClearTouchPlacementState();
            _touchTapTracker.Reset();
        }

        private void ClearTouchPlacementState()
        {
            ClearTouchPendingDragCandidate();
            ClearTouchPendingMoveSource();
            ClearTouchWallAnchor();
        }

        private void ClearTouchPendingDragCandidate()
        {
            _hasTouchPendingDragCandidate = false;
            _touchPendingDragCandidatePosition = default;
        }

        private void ClearTouchPendingMoveSource()
        {
            _hasTouchPendingMoveSource = false;
            _touchPendingMoveSourcePosition = default;
        }

        private void ClearTouchWallAnchor()
        {
            if (_hasTouchWallAnchor)
                _wallHandleController.EndDrag();

            _hasTouchWallAnchor = false;
            _touchWallAnchorPosition = default;

            if (!_isDraggingWallPath)
                _wallDragPendingPositions.Clear();
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _isActive = signal.NewMode == GameModeType.Construction;
            if (!_isActive)
            {
                ClearBuildGridHover();
                CancelActiveDrags();
            }
        }
    }
}
