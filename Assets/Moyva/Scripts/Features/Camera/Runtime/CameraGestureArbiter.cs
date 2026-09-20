using UnityEngine;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>CameraTouchMath — class: камери дотику Math.</summary>
    internal static class CameraTouchMath
    {
        /// <summary>Повертає DPI масштаб.</summary>
        public static float ResolveDpiScale(float screenDpi, float referenceDpi, float minScale, float maxScale)
            => screenDpi > 20f
                ? Mathf.Clamp(screenDpi / Mathf.Max(1f, referenceDpi), minScale, maxScale)
                : 1f;
    }

    /// <summary>CameraGestureSample — struct: камери жесту семплу.</summary>
    internal struct CameraGestureSample
    {
        /// <summary>Pan дельти у пікселях — Vector2.</summary>
        public Vector2 PanDeltaPixels;
        /// <summary>pinch дельти у пікселях — float.</summary>
        public float PinchDeltaPixels;
        /// <summary>twist у градусах — float.</summary>
        public float TwistDegrees;
        /// <summary>Чи Pan — HasPan.</summary>
        public bool HasPan;
        /// <summary>Чи pinch — HasPinch.</summary>
        public bool HasPinch;
        /// <summary>Чи twist — HasTwist.</summary>
        public bool HasTwist;
    }

    /// <summary>CameraGestureArbiter — class: камери жесту арбітра.</summary>
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

        /// <summary>Виконує CameraGestureArbiter.</summary>
        public CameraGestureArbiter(int settleFrames)
        {
            _settleFrames = Mathf.Max(0, settleFrames);
        }

        /// <summary>Кадри осідання, що лишились після завершення жесту.</summary>
        public bool SuppressSingleFingerPan => _settleFramesRemaining > 0;

        /// <summary>Повідомляє дотику кількості.</summary>
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

        /// <summary>Оновлює стан за тік осідання.</summary>
        public void TickSettle()
        {
            if (_settleFramesRemaining > 0)
                _settleFramesRemaining--;
        }

        /// <summary>Обчислює Two Finger.</summary>
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
