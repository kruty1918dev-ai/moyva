using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionInputService
    {
        private void CancelActivePointerDrags()
        {
            _isDraggingPendingPlacement = false;
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
                CancelActiveDrags();

            if (VerboseLogs)
                Debug.Log($"{LogTag} Active changed -> {_isActive}");
        }
    }
}
