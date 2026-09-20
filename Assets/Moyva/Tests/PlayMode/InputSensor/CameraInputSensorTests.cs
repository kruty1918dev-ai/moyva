using System.Collections;
using System.Collections.Generic;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Camera.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Kruty1918.Moyva.Tests.InputSensor.PlayMode
{
    /// <summary>
    /// Sensor-level research for the camera input path. Real queued device events
    /// (mouse wheel, middle-button drags, touchscreen drags) drive
    /// CameraPlayerController.Tick() — no direct MoveCamera/ZoomCamera calls.
    ///
    /// Scroll units are post-normalization device units (one wheel notch = 1.0),
    /// matching what Windows reports under ScrollDeltaBehavior.UniformAcrossAllPlatforms
    /// — the project's effective setting (no InputSettings asset overrides the
    /// default).
    /// </summary>
    public sealed class CameraInputSensorTests : InputTestFixture
    {
        private Mouse _mouse;
        private Keyboard _keyboard;
        private Touchscreen _touchscreen;
        private GameObject _cameraObject;
        private UnityEngine.Camera _camera;
        private CameraSettingsSO _settings;
        private CameraMovement _movement;
        private CameraZoom _zoom;
        private CameraPlayerController _controller;

        [SetUp]
        public void CreateRig()
        {
            _mouse = InputSystem.AddDevice<Mouse>();
            _keyboard = InputSystem.AddDevice<Keyboard>();

            _cameraObject = new GameObject("SensorCamera");
            _camera = _cameraObject.AddComponent<UnityEngine.Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 20f;
            _camera.transform.position = new Vector3(0f, 0f, -10f);

            _settings = new CameraSettingsSO();
            _settings.controlProfile.zoomSpeed = 5f; // value from camerasettings.json

            _movement = new CameraMovement(_camera, _settings);
            _movement.Initialize();
            _zoom = new CameraZoom(_camera, _settings, _movement);
            _zoom.Initialize();
            _controller = new CameraPlayerController(_movement, _zoom, _settings, null);
        }

        [TearDown]
        public void DestroyRig()
        {
            _controller?.Dispose();
            if (_cameraObject != null)
                Object.Destroy(_cameraObject);
        }

        /// <summary>One sensor frame: events land during the input update, then the controller ticks.</summary>
        private IEnumerator PumpFrame()
        {
            yield return null;
            _controller.Tick();
            _movement.LateTick();
            _zoom.LateTick();
        }

        private void SettleSmoothing(int iterations = 600)
        {
            for (var i = 0; i < iterations; i++)
            {
                _movement.LateTick();
                _zoom.LateTick();
            }
        }

        [UnityTest]
        public IEnumerator MouseWheel_UnderNormalizedDeltas_ZoomIsMuchWeakerThanDesigned()
        {
            Assert.That(Application.isFocused, Is.True,
                "CameraPlayerController drops all input when the application is not focused.");

            var stream = SensorStreams.MouseWheelNotches(8, 1);
            var startSize = _camera.orthographicSize;
            foreach (var delta in stream)
            {
                InputSystem.QueueDeltaStateEvent(_mouse.scroll, delta);
                yield return PumpFrame();
            }
            SettleSmoothing();

            var moved = _camera.orthographicSize - startSize;
            // Controller divides mouse.scroll by 120 — a constant designed for raw
            // platform deltas (120 per notch). With normalized deltas (1 per notch)
            // each notch moves zoom by only 1/120 * zoomSpeed.
            var expectedPerNotch = 5f / 120f;
            TestContext.Out.WriteLine($"[sensor] camera wheel zoom: 8 notches moved orthoSize {moved:F3} " +
                $"(expected {8f * expectedPerNotch:F3} under Uniform normalization, {8f * 5f:F1} if raw 120-unit deltas)");
            Assert.That(Mathf.Abs(moved), Is.EqualTo(8f * expectedPerNotch).Within(0.05f),
                "Wheel zoom responds in normalized units — the /120 constant makes it 120x weaker than the raw-delta design.");
        }

        [UnityTest]
        public IEnumerator TouchpadScrollStream_CameraZoomStillResponds()
        {
            Assert.That(Application.isFocused, Is.True);

            var stream = SensorStreams.TouchpadGesture(totalUnits: 5f, activeFrames: 30, tailFrames: 0, seed: 7);
            var startSize = _camera.orthographicSize;
            foreach (var delta in stream)
            {
                InputSystem.QueueDeltaStateEvent(_mouse.scroll, delta);
                yield return PumpFrame();
            }
            SettleSmoothing();

            var moved = _camera.orthographicSize - startSize;
            var expected = 5f * 5f / 120f;
            TestContext.Out.WriteLine($"[sensor] camera touchpad zoom: gesture moved orthoSize {moved:F3} (expected ~{expected:F3})");
            Assert.That(Mathf.Abs(moved), Is.EqualTo(expected).Within(0.05f),
                "Continuous small deltas accumulate into the zoom target — no loss on the camera path.");
        }

        [UnityTest]
        public IEnumerator MiddleMouseDrag_PansCameraByWorldMappedDelta()
        {
            Assert.That(Application.isFocused, Is.True);

            var start = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Move(_mouse.position, start);
            yield return PumpFrame();

            Press(_mouse.middleButton);
            yield return PumpFrame(); // capture begins

            var positions = SensorStreams.DragPositions(start, new Vector2(200f, 0f), 30, seed: 3);
            var startPosition = _camera.transform.position;
            foreach (var position in positions)
            {
                Move(_mouse.position, position);
                yield return PumpFrame();
            }
            Release(_mouse.middleButton);
            yield return PumpFrame();
            SettleSmoothing();

            var worldDelta = _camera.transform.position - startPosition;
            var worldPerPixel = 2f * _camera.orthographicSize / Screen.height;
            var expected = 200f * worldPerPixel;
            TestContext.Out.WriteLine($"[sensor] camera pan: 200px drag moved camera {worldDelta.magnitude:F2} world units (expected ~{expected:F2})");
            Assert.That(worldDelta.magnitude, Is.EqualTo(expected).Within(expected * 0.3f),
                "Pan drag should move the camera roughly 1:1 with the pointer's world-mapped distance.");
        }

        [UnityTest]
        public IEnumerator TouchDrag_MovesCameraImmediately()
        {
            Assert.That(Application.isFocused, Is.True);
            _touchscreen = InputSystem.AddDevice<Touchscreen>();

            var start = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            BeginTouch(1, start);
            yield return PumpFrame();

            var positions = SensorStreams.DragPositions(start, new Vector2(150f, 0f), 25, seed: 5);
            var deltas = SensorStreams.DeltasFrom(positions);
            var startPosition = _camera.transform.position;
            for (var i = 1; i < positions.Count; i++)
            {
                MoveTouch(1, positions[i], deltas[i]);
                yield return PumpFrame();
            }
            EndTouch(1, positions[positions.Count - 1]);
            yield return PumpFrame();

            var worldDelta = _camera.transform.position - startPosition;
            TestContext.Out.WriteLine($"[sensor] touch drag: 150px moved camera {worldDelta.magnitude:F2} world units immediately");
            Assert.That(worldDelta.magnitude, Is.GreaterThan(0.5f),
                "Single-finger touch drag should drive MoveCameraImmediate — no smoothing lag.");
        }

        [UnityTest]
        public IEnumerator TwoFingerPinch_ZoomsCameraByScale()
        {
            Assert.That(Application.isFocused, Is.True);
            _touchscreen = InputSystem.AddDevice<Touchscreen>();

            var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            BeginTouch(1, center + new Vector2(-60f, 0f));
            BeginTouch(2, center + new Vector2(60f, 0f));
            yield return PumpFrame();

            var startSize = _camera.orthographicSize;
            // Pinch out: fingers separate 60 -> 110 px each side over 15 frames.
            for (var i = 1; i <= 15; i++)
            {
                var offset = 60f + 50f * i / 15f;
                var prevOffset = 60f + 50f * (i - 1) / 15f;
                var delta = offset - prevOffset;
                MoveTouch(1, center + new Vector2(-offset, 0f), new Vector2(-delta, 0f));
                MoveTouch(2, center + new Vector2(offset, 0f), new Vector2(delta, 0f));
                yield return PumpFrame();
            }
            EndTouch(1, center + new Vector2(-110f, 0f));
            EndTouch(2, center + new Vector2(110f, 0f));
            yield return PumpFrame();
            SettleSmoothing();

            var moved = _camera.orthographicSize - startSize;
            TestContext.Out.WriteLine($"[sensor] pinch out: orthoSize moved {moved:F3} from {startSize:F1}");
            Assert.That(moved, Is.LessThan(0f),
                "Pinch-out (fingers separate) should zoom in — orthoSize shrinks.");
        }
    }
}
