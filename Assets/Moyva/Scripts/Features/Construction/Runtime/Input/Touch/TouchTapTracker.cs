using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class TouchTapTracker
    {
        private int _trackedTouchId = -1;
        private Vector2 _touchStartScreenPosition;
        private float _touchStartTime;
        private bool _touchStartedOverUi;
        private bool _touchMovedBeyondTap;
        private bool _multiTouchObserved;

        public bool IsTracking => _trackedTouchId >= 0;

        public int TrackedTouchId => _trackedTouchId;

        public bool HasMovedBeyondTap => _touchMovedBeyondTap;

        public void Begin(int touchId, Vector2 startScreenPosition, float startTime, bool startedOverUi, bool multiTouchObserved)
        {
            _trackedTouchId = touchId;
            _touchStartScreenPosition = startScreenPosition;
            _touchStartTime = startTime;
            _touchStartedOverUi = startedOverUi;
            _touchMovedBeyondTap = false;
            _multiTouchObserved = multiTouchObserved;
        }

        public void UpdateMove(Vector2 currentPosition, float maxMovePixels)
        {
            if (!IsTracking || _touchMovedBeyondTap)
                return;

            if ((currentPosition - _touchStartScreenPosition).sqrMagnitude > maxMovePixels * maxMovePixels)
                _touchMovedBeyondTap = true;
        }

        public bool Complete(float releaseTime, float maxTapDurationSeconds)
        {
            if (!IsTracking)
                return false;

            return !_touchStartedOverUi
                && !_touchMovedBeyondTap
                && !_multiTouchObserved
                && releaseTime - _touchStartTime <= maxTapDurationSeconds;
        }

        public void Reset()
        {
            _trackedTouchId = -1;
            _touchStartScreenPosition = Vector2.zero;
            _touchStartTime = 0f;
            _touchStartedOverUi = false;
            _touchMovedBeyondTap = false;
            _multiTouchObserved = false;
        }
    }
}
