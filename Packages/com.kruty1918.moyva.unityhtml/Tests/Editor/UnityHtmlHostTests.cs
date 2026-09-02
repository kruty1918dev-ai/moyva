using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReactUnity.UGUI.Behaviours;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityHTML.Runtime;

namespace UnityHTML.Tests
{
    public sealed class UnityHtmlHostTests
    {
        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [TearDown]
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = false;
        }

        [Test]
        public void Mount_RendersHtmlCssAndControls()
        {
            var rootObject = CreateRoot();
            using var host = new UnityHtmlHost();

            try
            {
                var document = new UnityHtmlDocument(
                    "<view className='shell'><text>UnityHTML ready</text><button><text>Click</text></button><img /></view>",
                    ".shell { display: flex; flex-direction: row; width: 400px; height: 100px; gap: 8px; background-color: rgb(12, 34, 56); }",
                    "UnityHtmlHostTests");

                var result = host.Mount(rootObject.GetComponent<RectTransform>(), document);

                Assert.That(result.Succeeded, Is.True, result.ErrorMessage);
                Assert.That(rootObject.GetComponentInChildren<TextMeshProUGUI>(true), Is.Not.Null);
                Assert.That(rootObject.GetComponentInChildren<Button>(true), Is.Not.Null);
                Assert.That(rootObject.GetComponentInChildren<Image>(true), Is.Not.Null);
                Assert.That(rootObject.GetComponentsInChildren<RectTransform>(true).Any(rect =>
                    Mathf.Abs(rect.rect.width - 400f) < 0.5f &&
                    Mathf.Abs(rect.rect.height - 100f) < 0.5f), Is.True, "Class stylesheet layout was not applied.");
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void Mount_ExposesGlobalsToJsBridge()
        {
            var rootObject = CreateRoot();
            using var host = new UnityHtmlHost();
            var bridge = new TestBridge();

            try
            {
                var document = new UnityHtmlDocument(
                    "<button onClick='Globals.testBridge.Trigger()'><text>Run</text></button>",
                    null,
                    "UnityHtmlGlobals");

                var result = host.Mount(rootObject.GetComponent<RectTransform>(), document, new Dictionary<string, object>
                {
                    ["testBridge"] = bridge
                });

                Assert.That(result.Succeeded, Is.True, result.ErrorMessage);
                rootObject.GetComponentInChildren<Button>(true).onClick.Invoke();
                Assert.That(bridge.TriggerCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void Mount_StripsUtf8BomFromTextAssetHtml()
        {
            var rootObject = CreateRoot();
            using var host = new UnityHtmlHost();

            try
            {
                var document = new UnityHtmlDocument(
                    "\uFEFF<view className='shell'><text>UnityHTML ready</text></view>",
                    "\uFEFF.shell { display: flex; width: 400px; height: 100px; }",
                    "UnityHtmlBom");

                var result = host.Mount(rootObject.GetComponent<RectTransform>(), document);

                Assert.That(result.Succeeded, Is.True, result.ErrorMessage);
                Assert.That(rootObject.GetComponentInChildren<TextMeshProUGUI>(true), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void Mount_ConfiguresInputFieldsForReadableEditing()
        {
            var rootObject = CreateRoot();
            using var host = new UnityHtmlHost();

            try
            {
                var document = new UnityHtmlDocument(
                    "<input className='menu-input' value='New World' placeholder='World'></input>",
                    ".menu-input { width: 320px; height: 44px; color: white; }",
                    "UnityHtmlInput");

                var result = host.Mount(rootObject.GetComponent<RectTransform>(), document);
                var input = rootObject.GetComponentInChildren<TMP_InputField>(true);

                Assert.That(result.Succeeded, Is.True, result.ErrorMessage);
                Assert.That(input, Is.Not.Null);
                Assert.That(input.interactable, Is.True);
                Assert.That(input.targetGraphic, Is.Not.Null);
                Assert.That(input.targetGraphic.raycastTarget, Is.True);
                Assert.That(input.textComponent.color.a, Is.GreaterThan(0.95f));
                Assert.That(input.textComponent.alignment, Is.EqualTo(TextAlignmentOptions.MidlineLeft));
                Assert.That(input.customCaretColor, Is.True);
                Assert.That(input.textViewport.offsetMin.x, Is.EqualTo(12f).Within(0.01f));
                Assert.That(input.textViewport.offsetMax.x, Is.EqualTo(-12f).Within(0.01f));
                Assert.That(input.textComponent.rectTransform.anchorMin, Is.EqualTo(Vector2.zero));
                Assert.That(input.textComponent.rectTransform.anchorMax, Is.EqualTo(Vector2.one));
                Assert.That(input.GetComponent<ScrollEventBubbling>().Bubble, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void Mount_RendersSliderAndExposesValueChanged()
        {
            var rootObject = CreateRoot();
            using var host = new UnityHtmlHost();
            var bridge = new SliderBridge();

            try
            {
                var document = new UnityHtmlDocument(
                    "<slider value='0.25' minValue='0' maxValue='1' onValueChanged='Globals.sliderBridge.SetValue(event)'></slider>",
                    ".menu-slider { width: 320px; height: 32px; }",
                    "UnityHtmlSlider");

                var result = host.Mount(rootObject.GetComponent<RectTransform>(), document, new Dictionary<string, object>
                {
                    ["sliderBridge"] = bridge
                });

                var slider = rootObject.GetComponentInChildren<Slider>(true);
                Assert.That(result.Succeeded, Is.True, result.ErrorMessage);
                Assert.That(slider, Is.Not.Null);
                Assert.That(slider.value, Is.EqualTo(0.25f).Within(0.001f));

                slider.value = 0.75f;
                Assert.That(bridge.Value, Is.EqualTo(0.75f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void Mount_SliderCommitsOnReleaseAndLocksParentScroll()
        {
            var rootObject = CreateRoot();
            var scrollRect = rootObject.AddComponent<ScrollRect>();
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            using var host = new UnityHtmlHost();
            var bridge = new SliderBridge();

            try
            {
                var document = new UnityHtmlDocument(
                    "<slider className='menu-slider' value='0.25' minValue='0' maxValue='1' format='percent' onBeginChange='Globals.sliderBridge.Begin(event)' onEndChange='Globals.sliderBridge.End(event)'></slider>",
                    ".menu-slider { width: 320px; height: 34px; }",
                    "UnityHtmlSliderCommit");

                var result = host.Mount(rootObject.GetComponent<RectTransform>(), document, new Dictionary<string, object>
                {
                    ["sliderBridge"] = bridge
                });
                var slider = rootObject.GetComponentInChildren<Slider>(true);
                var pointer = new PointerEventData(eventSystemObject.GetComponent<EventSystem>());

                Assert.That(result.Succeeded, Is.True, result.ErrorMessage);
                Assert.That(slider, Is.Not.Null);
                ExecuteEvents.Execute(slider.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                Assert.That(scrollRect.vertical, Is.False);
                Assert.That(bridge.BeginCount, Is.EqualTo(1));

                slider.value = 0.8f;
                Assert.That(rootObject.GetComponentsInChildren<TextMeshProUGUI>(true).Any(text => text.text == "80%"), Is.True);

                ExecuteEvents.Execute(slider.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                Assert.That(scrollRect.vertical, Is.True);
                Assert.That(bridge.EndCount, Is.EqualTo(1));
                Assert.That(bridge.Value, Is.EqualTo(0.8f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(eventSystemObject);
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void Mount_RendersSelectOptionsAndExposesChangedIndex()
        {
            var rootObject = CreateRoot();
            using var host = new UnityHtmlHost();
            var bridge = new SelectBridge();

            try
            {
                var document = new UnityHtmlDocument(
                    "<select className='menu-select' options='One|Two|Three' value='1' onChange='Globals.selectBridge.SetValue(event)'></select>",
                    ".menu-select { width: 320px; height: 42px; }",
                    "UnityHtmlSelect");

                var result = host.Mount(rootObject.GetComponent<RectTransform>(), document, new Dictionary<string, object>
                {
                    ["selectBridge"] = bridge
                });
                var dropdown = rootObject.GetComponentInChildren<TMP_Dropdown>(true);

                Assert.That(result.Succeeded, Is.True, result.ErrorMessage);
                Assert.That(dropdown, Is.Not.Null);
                Assert.That(dropdown.options.Select(option => option.text), Is.EqualTo(new[] { "One", "Two", "Three" }));
                Assert.That(dropdown.value, Is.EqualTo(1));
                Assert.That(dropdown.captionText.text, Is.EqualTo("Two"));

                dropdown.value = 2;
                Assert.That(bridge.Value, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void RemountUnmountAndDispose_DoNotLeaveRenderedObjects()
        {
            var rootObject = CreateRoot();
            var root = rootObject.GetComponent<RectTransform>();
            using var host = new UnityHtmlHost();

            try
            {
                var first = host.Mount(root, new UnityHtmlDocument("<text>First</text>", null, "First"));
                var second = host.Mount(root, new UnityHtmlDocument("<text>Second</text>", null, "Second"));

                Assert.That(first.Succeeded, Is.True, first.ErrorMessage);
                Assert.That(second.Succeeded, Is.True, second.ErrorMessage);
                Assert.That(rootObject.GetComponentsInChildren<TextMeshProUGUI>(true), Has.Length.EqualTo(1));
                Assert.That(rootObject.GetComponentInChildren<TextMeshProUGUI>(true).text, Does.Contain("Second"));

                host.Unmount();
                Assert.That(root.childCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void Mount_WithScrollDirection_DoesNotLeakDetachedElements()
        {
            var rootObject = CreateRoot();
            using var host = new UnityHtmlHost();
            var existingDetachedIds = Object.FindObjectsByType<ReactElement>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(element => element != null && element.transform.parent == null)
                .Select(element => element.GetInstanceID())
                .ToHashSet();

            try
            {
                host.Mount(
                    rootObject.GetComponent<RectTransform>(),
                    new UnityHtmlDocument(
                        "<scroll direction='vertical'><text>Scrollable</text></scroll>",
                        null,
                        "UnityHtmlDetachedScroll"));

                var leaked = Object.FindObjectsByType<ReactElement>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None)
                    .Where(element =>
                        element != null &&
                        element.transform.parent == null &&
                        !existingDetachedIds.Contains(element.GetInstanceID()))
                    .ToArray();

                Assert.That(leaked, Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void Mount_WhenDocumentMissing_ReturnsFailure()
        {
            using var host = new UnityHtmlHost();
            var result = host.Mount(null, default);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Empty);
        }

        [Test]
        public void Mount_WhenHtmlAssetMissing_ReturnsFailure()
        {
            var rootObject = CreateRoot();
            using var host = new UnityHtmlHost();

            try
            {
                var result = host.Mount(rootObject.GetComponent<RectTransform>(), UnityHtmlDocument.FromTextAssets(null));

                Assert.That(result.Succeeded, Is.False);
                Assert.That(result.ErrorMessage, Does.Contain("empty or missing"));
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void Mount_WhenHtmlCannotRender_ReturnsFailureAndClearsRoot()
        {
            var rootObject = CreateRoot();
            var root = rootObject.GetComponent<RectTransform>();
            using var host = new UnityHtmlHost();

            try
            {
                var result = host.Mount(
                    root,
                    new UnityHtmlDocument("<text unknown-render-property='bad'>Broken</text>", null, "BrokenHtml"));

                Assert.That(result.Succeeded, Is.False);
                Assert.That(result.ErrorMessage, Is.Not.Empty);
                Assert.That(root.childCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        private static GameObject CreateRoot()
        {
            var rootObject = new GameObject("UnityHTML Host Test Root", typeof(RectTransform), typeof(Canvas));
            var root = rootObject.GetComponent<RectTransform>();
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 800f);
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 600f);
            return rootObject;
        }

        private sealed class TestBridge
        {
            public int TriggerCount { get; private set; }

            public void Trigger() => TriggerCount++;
        }

        private sealed class SliderBridge
        {
            public float Value { get; private set; }
            public int BeginCount { get; private set; }
            public int EndCount { get; private set; }

            public void SetValue(float value) => Value = value;
            public void Begin(float value) => BeginCount++;
            public void End(float value)
            {
                Value = value;
                EndCount++;
            }
        }

        private sealed class SelectBridge
        {
            public int Value { get; private set; } = -1;

            public void SetValue(int value) => Value = value;
        }
    }
}
