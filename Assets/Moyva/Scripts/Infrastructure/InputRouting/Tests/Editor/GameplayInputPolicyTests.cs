using System.Collections.Generic;
using System.Reflection;
using Kruty1918.InputRouting.API;
using Kruty1918.Moyva.InputRouting.Runtime;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Tests.InputRouting
{
    /// <summary>
    /// P069 — modal/non-modal input rules: a GameplayInputBlocker region must
    /// stop pointer gameplay input (placement, zoom, primary) while keyboard
    /// navigation is governed by text focus, and free map space stays usable.
    /// Real GraphicRaycaster hits cannot occur in EditMode (graphic.depth is
    /// only assigned by a canvas render pass), so a purpose-built raycaster
    /// feeds hits into the real EventSystem.RaycastAll pipeline.
    /// </summary>
    public sealed class GameplayInputPolicyTests
    {
        private GameObject _eventSystemObject;
        private EventSystem _eventSystem;
        private RegionRaycaster _raycaster;
        private GameplayInputPolicy _policy;

        [SetUp]
        public void SetUp()
        {
            _eventSystemObject = new GameObject(
                "EventSystem", typeof(EventSystem), typeof(RegionRaycaster));
            _eventSystem = _eventSystemObject.GetComponent<EventSystem>();
            _raycaster = _eventSystemObject.GetComponent<RegionRaycaster>();
            // EditMode never runs OnEnable; the raycaster registry only fills
            // there, so invoke the lifecycle explicitly.
            ForceEnable(_raycaster);

            _policy = new GameplayInputPolicy(_eventSystem);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_eventSystemObject);
        }

        private static void ForceEnable(Component component)
        {
            for (var type = component.GetType(); type != null; type = type.BaseType)
            {
                var method = type.GetMethod(
                    "OnEnable",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (method != null)
                {
                    method.Invoke(component, null);
                    return;
                }
            }
        }

        /// <summary>
        /// Reports every registered RectTransform region as hit when the
        /// pointer position falls inside its rect. Stands in for uGUI's
        /// GraphicRaycaster, which cannot hit in edit mode.
        /// </summary>
        private sealed class RegionRaycaster : BaseRaycaster
        {
            public readonly List<RectTransform> Regions = new();

            public override Camera eventCamera => null;

            public override void Raycast(
                PointerEventData eventData, List<RaycastResult> resultAppendList)
            {
                foreach (var region in Regions)
                {
                    if (region == null)
                        continue;

                    if (RectTransformUtility.RectangleContainsScreenPoint(
                            region, eventData.position, null))
                        resultAppendList.Add(new RaycastResult
                        {
                            gameObject = region.gameObject,
                            module = this,
                        });
                }
            }
        }

        private RectTransform AddRegion(Rect rect)
        {
            var regionObject = new GameObject("Region", typeof(RectTransform));
            var rectTransform = (RectTransform)regionObject.transform;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.position = new Vector3(
                rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f, 0f);
            rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
            _raycaster.Regions.Add(rectTransform);
            return rectTransform;
        }

        private RectTransform AddShield(Rect rect, GameplayInputKind mask)
        {
            var rectTransform = AddRegion(rect);
            rectTransform.gameObject.AddComponent<GameplayInputBlocker>()
                .GetType()
                .GetField("_blockedInput", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(rectTransform.GetComponent<GameplayInputBlocker>(), mask);
            return rectTransform;
        }

        [Test]
        public void PointerOverBlocker_DeniesPointerKinds_NotKeyboard()
        {
            var shield = AddShield(new Rect(0, 0, 400, 600), GameplayInputKind.AllPointer);
            var inside = new Vector2(200f, 300f);
            var outside = new Vector2(600f, 300f);

            Assert.IsFalse(_policy.CanProcess(GameplayInputKind.PrimaryPointer, inside),
                "Primary pointer inside a blocking region must not reach the map.");
            Assert.IsFalse(_policy.CanProcess(GameplayInputKind.PointerZoom, inside),
                "Scroll over a panel region must not zoom the camera.");
            Assert.IsFalse(_policy.CanProcess(GameplayInputKind.Placement, inside),
                "Placement must be blocked under a shield.");
            Assert.IsTrue(_policy.CanProcess(GameplayInputKind.KeyboardNavigation, inside),
                "Keyboard navigation is not a pointer kind — the shield must not eat it.");

            Assert.IsTrue(_policy.CanProcess(GameplayInputKind.PrimaryPointer, outside),
                "Pointer input outside any UI region must reach the map.");
            Assert.IsTrue(_policy.CanProcess(GameplayInputKind.PointerZoom, outside),
                "Scroll outside UI must zoom the camera.");
        }

        [Test]
        public void PointerOverSelectable_DeniesPointerInput()
        {
            var region = AddRegion(new Rect(0, 0, 200, 60));
            region.gameObject.AddComponent<Button>();
            var inside = new Vector2(100f, 30f);
            var outside = new Vector2(500f, 400f);

            Assert.IsFalse(_policy.CanProcess(GameplayInputKind.PrimaryPointer, inside),
                "Pointer over an interactive control must not reach the map.");
            Assert.IsTrue(_policy.CanProcess(GameplayInputKind.PrimaryPointer, outside),
                "Pointer over empty space must still reach the map.");
        }

        [Test]
        public void IsPointerOverUi_InteractiveOnly_SkipsPlainRegions()
        {
            AddRegion(new Rect(0, 0, 200, 60));
            var inside = new Vector2(100f, 30f);

            Assert.IsTrue(_policy.IsPointerOverUi(inside, interactiveOnly: false),
                "A raycastable region is still UI — over-any-UI must report it.");
            Assert.IsFalse(_policy.IsPointerOverUi(inside, interactiveOnly: true),
                "A plain graphic consumes no input — over-interactive-UI must not claim it.");
        }

        [Test]
        public void PointerCapture_KeepsGestureOwner_UntilReleased()
        {
            var region = AddRegion(new Rect(0, 0, 200, 60));
            region.gameObject.AddComponent<Button>();
            var inside = new Vector2(100f, 30f);
            var outside = new Vector2(500f, 400f);

            // A pan that starts on free map space keeps working even when the
            // pointer later drifts over UI; a pan that starts on UI stays dead.
            Assert.IsTrue(_policy.TryBeginPointerCapture(
                GameplayInputKind.PointerPan, outside));
            Assert.IsTrue(_policy.CanProcess(GameplayInputKind.PointerPan, inside),
                "A captured gesture keeps its begin-decision while over UI.");

            _policy.EndPointerCapture(GameplayInputKind.PointerPan);
            Assert.IsFalse(_policy.TryBeginPointerCapture(
                GameplayInputKind.PointerPan, inside),
                "A gesture that starts over UI must not reach the map.");
            Assert.IsFalse(_policy.CanProcess(GameplayInputKind.PointerPan, inside),
                "Capture must remember the denial while the pointer is down.");
        }

        [Test]
        public void FocusedTextInput_SuppressesKeyboardNavigation()
        {
            var inputObject = new GameObject("Input", typeof(RectTransform), typeof(TMP_InputField));
            _eventSystem.SetSelectedGameObject(inputObject);

            Assert.IsFalse(_policy.CanProcess(GameplayInputKind.KeyboardNavigation, Vector2.zero),
                "Gameplay hotkeys must stay dead while a text field is focused.");

            _eventSystem.SetSelectedGameObject(null);
            Assert.IsTrue(_policy.CanProcess(GameplayInputKind.KeyboardNavigation, Vector2.zero),
                "Closing/blurring the field must hand keyboard control back to the game.");
        }

        [Test]
        public void AcquireBlock_MasksOffKinds_UntilDisposed()
        {
            using (var block = _policy.AcquireBlock(
                       GameplayInputKind.Placement | GameplayInputKind.PointerZoom, this))
            {
                Assert.IsFalse(_policy.CanProcess(GameplayInputKind.Placement, Vector2.zero));
                Assert.IsFalse(_policy.CanProcess(GameplayInputKind.PointerZoom, Vector2.zero));
                Assert.IsTrue(_policy.CanProcess(GameplayInputKind.PrimaryPointer, Vector2.zero),
                    "A mask block must leave unlisted kinds untouched.");
            }

            Assert.IsTrue(_policy.CanProcess(GameplayInputKind.Placement, Vector2.zero),
                "Disposing the lease must restore the input kind.");
        }
    }
}
