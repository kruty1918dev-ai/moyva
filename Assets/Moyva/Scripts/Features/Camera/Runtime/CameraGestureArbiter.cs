using UnityEngine;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal static class CameraTouchMath
    {
        /// <summary>
        /// Scales pixel-based gesture thresholds for the device DPI.
        /// Returns 1 when the platform does not report a meaningful DPI.
        /// </summary>
        public static float ResolveDpiScale(float screenDpi, float referenceDpi, float minScale, float maxScale)
            => screenDpi > 20f
                ? Mathf.Clamp(screenDpi / Mathf.Max(1f, referenceDpi), minScale, maxScale)
                : 1f;
    }

    /// <summary>Result of two-finger gesture arbitration for one frame.</summary>
    internal struct CameraGestureSample
    {
        /// <summary>Pan delta in pixels beyond the drag dead zone.</summary>
        public Vector2 PanDeltaPixels;
        /// <summary>Pinch delta in pixels beyond the pinch dead zone.</summary>
        public float PinchDeltaPixels;
        /// <summary>Twist in degrees beyond the per-frame twist threshold.</summary>
        public float TwistDegrees;
        public bool HasPan;
        public bool HasPinch;
        public bool HasTwist;
    }

    /// <summary>
    /// Arbiters competing two-finger gestures (pan / pinch / twist) per
    /// continuous touch gesture. Dominance latches for the whole gesture:
    /// once the accumulated pinch exceeds its dominance threshold twist is
    /// suppressed (no accidental rotation while zooming); once accumulated
    /// twist exceeds its threshold pinch is suppressed. Also suppresses
    /// one-finger pan for a few frames after a multi-touch gesture ends so
    /// releasing the second finger does not cause a jump.
    /// </summary>
    internal sealed class CameraGestureArbiter
    {
        private const float PerFrameTwistThresholdDegrees = 0.5f;

        private readonly int _settleFrames;
        private int _lastTouchCount;
        private int _settleFramesRemaining;
        private float _pinchAccumulated;
        private float _twistAccumulated;
        private bool _pinchDominant;
        private bool _twistDominant;

        public CameraGestureArbiter(int settleFrames)
        {
            _settleFrames = Mathf.Max(0, settleFrames);
        }

        /// <summary>True while one-finger pan should be ignored after a gesture.</summary>
        public bool SuppressSingleFingerPan => _settleFramesRemaining > 0;

        /// <summary>Call once per frame with the active pressed-touch count.</summary>
        public void NotifyTouchCount(int touchCount)
        {
            if (touchCount == _lastTouchCount)
                return;

            if (touchCount < _lastTouchCount)
                _settleFramesRemaining = _settleFrames;

            if (touchCount < 2)
            {
                _pinchAccumulated = 0f;
                _twistAccumulated = 0f;
                _pinchDominant = false;
                _twistDominant = false;
            }

            _lastTouchCount = touchCount;
        }

        /// <summary>Call once per frame to age the post-gesture settle window.</summary>
        public void TickSettle()
        {
            if (_settleFramesRemaining > 0)
                _settleFramesRemaining--;
        }

        public CameraGestureSample EvaluateTwoFinger(
            Vector2 centerDeltaPixels,
            float pinchDeltaPixels,
            float twistDegrees,
            float dragDeadZonePixels,
            float pinchDeadZonePixels,
            float pinchDominancePixels,
            float twistDominanceDegrees)
        {
            _pinchAccumulated += pinchDeltaPixels;
            _twistAccumulated += twistDegrees;

            if (!_twistDominant && Mathf.Abs(_pinchAccumulated) >= pinchDominancePixels)
                _pinchDominant = true;
            if (!_pinchDominant && Mathf.Abs(_twistAccumulated) >= twistDominanceDegrees)
                _twistDominant = true;

            var sample = new CameraGestureSample();

            // Pinch is the dominant gesture: while pinching, twist is locked
            // out and explicit pan is suppressed (focal-point zoom already
            // keeps the map under the fingers).
            if (Mathf.Abs(pinchDeltaPixels) > pinchDeadZonePixels || _pinchDominant)
            {
                sample.PinchDeltaPixels = Mathf.Abs(pinchDeltaPixels) > pinchDeadZonePixels
                    ? pinchDeltaPixels
                    : 0f;
                sample.HasPinch = sample.PinchDeltaPixels != 0f;
                return sample;
            }

            if (!_pinchDominant && Mathf.Abs(twistDegrees) > PerFrameTwistThresholdDegrees)
            {
                sample.TwistDegrees = twistDegrees;
                sample.HasTwist = true;
                return sample;
            }

            if (centerDeltaPixels.sqrMagnitude > dragDeadZonePixels * dragDeadZonePixels)
            {
                sample.PanDeltaPixels = centerDeltaPixels;
                sample.HasPan = true;
            }

            return sample;
        }
    }
}
