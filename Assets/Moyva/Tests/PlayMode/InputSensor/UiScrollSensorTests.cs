using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ReactUnity.UGUI.Behaviours;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityHTML.Runtime;

namespace Kruty1918.Moyva.Tests.InputSensor.PlayMode
{
    /// <summary>
    /// Sensor-level scroll research for the MoyvaUI &lt;scroll&gt; element (the element
    /// behind the settings/controls pages). Events are queued through the real
    /// InputSystem → InputSystemUIInputModule → EventSystem → SmoothScrollRect chain,
    /// so the numbers reflect what a physical touchpad/mouse produces, not stubbed
    /// PointerEventData.
    ///
    /// Expected model: the scene module runs with scrollDeltaPerTick = 6 and the host
    /// markup uses sensitivity="24" (as in HomeMenuMoyvaUiMarkup). After the module
    /// conversion and SmoothScrollRect's *120 fix-up, one normalized wheel unit should
    /// move content by 6 × 24 = 144 px.
    /// </summary>
    public sealed class UiScrollSensorTests : InputTestFixture
    {
        private const float Sensitivity = 24f;
        private const float ExpectedPixelsPerUnit = 6f * Sensitivity;

        private Mouse _mouse;
        private EventSystem _eventSystem;
        private InputSystemUIInputModule _inputModule;
        private Canvas _canvas;
        private RectTransform _mountRoot;
        private UnityHtmlHost _host;
        private readonly List<GameObject> _objects = new();

        [SetUp]
        public void CreateScene()
        {
            _mouse = InputSystem.AddDevice<Mouse>();

            var eventSystemObject = Track(new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)));
            _eventSystem = eventSystemObject.GetComponent<EventSystem>();
            _inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            _inputModule.AssignDefaultActions();
            // Match the HomeMenu scene module: m_ScrollDeltaPerTick = 6.
            _inputModule.scrollDeltaPerTick = 6f;
            EmulateWindowsScrollUnits();

            var canvasObject = Track(new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)));
            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            _mountRoot = new GameObject("MountRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            _mountRoot.SetParent(canvasObject.transform, false);
            _mountRoot.anchorMin = _mountRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _mountRoot.sizeDelta = new Vector2(400f, 600f);
            _mountRoot.anchoredPosition = Vector2.zero;
            _objects.Add(_mountRoot.gameObject);

            _host = new UnityHtmlHost();
        }

        [TearDown]
        public void DestroyScene()
        {
            _host?.Dispose();
            foreach (var go in _objects)
                if (go != null)
                    Object.Destroy(go);
            _objects.Clear();
        }

        private GameObject Track(GameObject go)
        {
            _objects.Add(go);
            return go;
        }

        /// <summary>
        /// The test runtime reports scrollWheelDeltaPerTick = 1, while the native
        /// Windows runtime reports 120. The UI module divides incoming deltas by that
        /// value, so without this emulation every queued unit lands 120x too large.
        /// Setting 120 makes injected normalized deltas (1 = one notch) produce the
        /// same PointerEventData.scrollDelta as production.
        /// </summary>
        private static void EmulateWindowsScrollUnits()
        {
            var runtimeType = typeof(InputSystem).Assembly.GetType("UnityEngine.InputSystem.LowLevel.InputRuntime");
            var instance = runtimeType
                .GetField("s_Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .GetValue(null);
            Assert.That(instance.GetType().Name, Is.EqualTo("InputTestRuntime"),
                "Scroll emulation expects the Input System test runtime.");
            instance.GetType().GetProperty("scrollWheelDeltaPerTick").SetValue(instance, 120f);
        }

        private SmoothScrollRect MountScrollDocument(float smoothness = -1f)
        {
            var rows = new StringBuilder();
            for (var i = 0; i < 40; i++)
                rows.Append("<view className=\"row\"></view>");

            var smoothnessAttr = smoothness >= 0f ? $" smoothness=\"{smoothness.ToString(System.Globalization.CultureInfo.InvariantCulture)}\"" : string.Empty;
            var document = new UnityHtmlDocument(
                "<view className=\"shell\">" +
                $"<scroll className=\"nav\" direction=\"vertical\" sensitivity=\"{Sensitivity.ToString(System.Globalization.CultureInfo.InvariantCulture)}\"{smoothnessAttr}>" +
                rows + "</scroll></view>",
                ".shell { display: flex; flex-direction: column; width: 400px; height: 600px; }" +
                ".nav { display: flex; flex-direction: column; flex-grow: 1; min-height: 0; max-height: 100%; overflow: hidden; }" +
                ".row { width: 100%; height: 60px; flex-shrink: 0; }",
                "UiScrollSensorTests");

            var result = _host.Mount(_mountRoot, document);
            Assert.That(result.Succeeded, Is.True, result.ErrorMessage);

            var scroll = _mountRoot.GetComponentInChildren<SmoothScrollRect>(true);
            Assert.That(scroll, Is.Not.Null, "Mounted document must contain a SmoothScrollRect.");
            Assert.That(scroll.content.rect.height, Is.GreaterThan(scroll.viewport.rect.height),
                "Content must overflow the viewport for scrolling to engage.");
            return scroll;
        }

        private IEnumerator PointAtCenter(RectTransform element)
        {
            var corners = new Vector3[4];
            element.GetWorldCorners(corners);
            var center = RectTransformUtility.WorldToScreenPoint(null, (corners[0] + corners[2]) * 0.5f);
            Move(_mouse.position, center);
            yield return null;
        }

        /// <summary>
        /// Queue one frame worth of raw scroll deltas like a driver would, let the real
        /// input update + EventSystem dispatch run, then sample the resulting position.
        /// </summary>
        private IEnumerator RunScrollStream(
            IReadOnlyList<Vector2> stream,
            SmoothScrollRect scroll,
            List<float> appliedPerFrame)
        {
            appliedPerFrame.Clear();
            for (var i = 0; i < stream.Count; i++)
            {
                if (stream[i].sqrMagnitude > 0f)
                    InputSystem.QueueDeltaStateEvent(_mouse.scroll, stream[i]);
                yield return null;
                appliedPerFrame.Add(scroll.content.anchoredPosition.y);
            }
        }

        private static IEnumerator WaitForAnimationToSettle()
        {
            // Smooth-scroll animation lasts 0.12s of real time; in batch mode frames
            // are far shorter than 16ms, so wait on the clock, not the frame count.
            yield return new WaitForSecondsRealtime(0.4f);
        }

        private static float ExpectedTravel(IReadOnlyList<Vector2> stream)
        {
            var total = 0f;
            foreach (var delta in stream)
                total += Mathf.Abs(delta.y);
            return total * ExpectedPixelsPerUnit;
        }

        [UnityTest]
        public IEnumerator MouseWheel_SingleNotch_AppliesFullStepAfterAnimation()
        {
            var scroll = MountScrollDocument();
            yield return PointAtCenter((RectTransform)scroll.transform);

            var applied = new List<float>();
            var stream = SensorStreams.MouseWheelNotches(1, 1);
            yield return RunScrollStream(stream, scroll, applied);
            yield return WaitForAnimationToSettle();

            var moved = Mathf.Abs(scroll.content.anchoredPosition.y);
            TestContext.Out.WriteLine($"[sensor] single notch: moved {moved:F1}px, expected {ExpectedPixelsPerUnit:F1}px");
            Assert.That(moved, Is.EqualTo(ExpectedPixelsPerUnit).Within(1f),
                "One normalized wheel notch should move content 6*24=144px once the animation finishes.");
        }

        [UnityTest]
        public IEnumerator MouseWheel_RapidSpin_DeliversMostOfTheScrollDistance()
        {
            var scroll = MountScrollDocument();
            yield return PointAtCenter((RectTransform)scroll.transform);

            // Fast wheel spin: one notch every frame for 10 frames — what a flicked
            // wheel produces.
            var stream = SensorStreams.MouseWheelNotches(10, 1);
            var applied = new List<float>();
            yield return RunScrollStream(stream, scroll, applied);
            yield return WaitForAnimationToSettle();

            var expected = ExpectedTravel(stream);
            var moved = Mathf.Abs(scroll.content.anchoredPosition.y);
            var delivered = moved / expected;
            TestContext.Out.WriteLine($"[sensor] rapid wheel: moved {moved:F1}px of {expected:F1}px ({delivered:P0} delivered)");
            Assert.That(delivered, Is.GreaterThan(0.85f),
                $"Rapid wheel spin delivered only {delivered:P0} of the intended distance — in-flight smooth-scroll animation is truncated by every new scroll event.");
        }

        [UnityTest]
        public IEnumerator TouchpadScroll_ContinuousGesture_DeliversMostOfTheScrollDistance()
        {
            var scroll = MountScrollDocument();
            yield return PointAtCenter((RectTransform)scroll.transform);

            // Precision touchpad gesture: ~5 wheel-units spread over ~30 frames plus
            // an inertial tail — a single natural two-finger swipe.
            var stream = SensorStreams.TouchpadGesture(totalUnits: 5f, activeFrames: 30, tailFrames: 8, seed: 42);
            var applied = new List<float>();
            yield return RunScrollStream(stream, scroll, applied);
            yield return WaitForAnimationToSettle();

            var expected = ExpectedTravel(stream);
            var moved = Mathf.Abs(scroll.content.anchoredPosition.y);
            var delivered = moved / expected;
            TestContext.Out.WriteLine($"[sensor] touchpad gesture: moved {moved:F1}px of {expected:F1}px ({delivered:P0} delivered)");
            TestContext.Out.WriteLine($"[sensor] per-frame positions: {string.Join(", ", applied.ConvertAll(p => p.ToString("F1")))}");
            Assert.That(delivered, Is.GreaterThan(0.85f),
                $"Continuous touchpad scroll delivered only {delivered:P0} of the intended distance — this is the 'scroll freezes / doesn't obey' defect.");
        }

        [UnityTest]
        public IEnumerator TouchpadScroll_NoSmoothness_DeliversFullDistance()
        {
            // Control case: identical stream against the same element with the
            // smooth-scroll animation disabled (smoothness="0"). If the deficit
            // disappears, the animation restart is proven to be the loss mechanism.
            var scroll = MountScrollDocument(smoothness: 0f);
            yield return PointAtCenter((RectTransform)scroll.transform);

            var stream = SensorStreams.TouchpadGesture(totalUnits: 5f, activeFrames: 30, tailFrames: 0, seed: 42);
            var applied = new List<float>();
            yield return RunScrollStream(stream, scroll, applied);
            yield return WaitForAnimationToSettle();

            var expected = ExpectedTravel(stream);
            var moved = Mathf.Abs(scroll.content.anchoredPosition.y);
            var delivered = moved / expected;
            TestContext.Out.WriteLine($"[sensor] touchpad smoothness=0: moved {moved:F1}px of {expected:F1}px ({delivered:P0} delivered)");
            Assert.That(delivered, Is.GreaterThan(0.9f),
                "With smoothness=0 the same touchpad stream should land nearly 1:1.");
        }

        [UnityTest]
        public IEnumerator MouseDrag_TracksPointerOneToOne()
        {
            var scroll = MountScrollDocument();
            yield return PointAtCenter((RectTransform)scroll.transform);

            // Drag the list content up 200px with a held left button: real pointer
            // positions with sensor jitter, routed through the drag threshold (10px).
            // The view starts at the top, so a downward pull (scroll-up) would be
            // clamped — drag up to move content toward the bottom of the list.
            var corners = new Vector3[4];
            ((RectTransform)scroll.transform).GetWorldCorners(corners);
            var start = RectTransformUtility.WorldToScreenPoint(null, (corners[0] + corners[2]) * 0.5f);
            var positions = SensorStreams.DragPositions(start, new Vector2(0f, 200f), 30, seed: 11);

            Press(_mouse.leftButton);
            yield return null;
            var startY = scroll.content.anchoredPosition.y;
            foreach (var position in positions)
            {
                Move(_mouse.position, position);
                yield return null;
            }
            Release(_mouse.leftButton);
            yield return null;

            var moved = scroll.content.anchoredPosition.y - startY;
            TestContext.Out.WriteLine($"[sensor] drag: pointer +200px moved content {moved:F1}px");
            Assert.That(Mathf.Abs(moved - 200f), Is.LessThan(25f),
                "Content should track a drag nearly 1:1 (minus the drag threshold).");
        }
    }
}
