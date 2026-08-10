using System.Collections;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.InputRouting.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Tests.InputRoutingPlayMode
{
    public sealed class GameplayInputPointerPolicyPlayModeTests
    {
        private IGameplayInputPolicy _policy;
        private GameObject _eventSystemObject;
        private GameObject _canvasObject;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _eventSystemObject = new GameObject(
                "TestEventSystem",
                typeof(EventSystem));
            EventSystem eventSystem =
                _eventSystemObject.GetComponent<EventSystem>();
            _policy = new GameplayInputPolicy(eventSystem);

            _canvasObject = new GameObject(
                "TestCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(GraphicRaycaster));
            _canvasObject.GetComponent<Canvas>().renderMode =
                RenderMode.ScreenSpaceOverlay;
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasObject != null)
                Object.Destroy(_canvasObject);
            if (_eventSystemObject != null)
                Object.Destroy(_eventSystemObject);
        }

        [UnityTest]
        public IEnumerator ExplicitBlocker_BlocksPointerKinds_ButNotKeyboard()
        {
            CreateFullScreenBlocker();
            yield return null;
            Vector2 center = new(Screen.width * 0.5f, Screen.height * 0.5f);

            Assert.IsFalse(_policy.CanProcess(
                GameplayInputKind.PointerZoom,
                center));
            Assert.IsFalse(_policy.CanProcess(
                GameplayInputKind.Placement,
                center));
            Assert.IsTrue(_policy.CanProcess(
                GameplayInputKind.KeyboardNavigation,
                center));
        }

        [UnityTest]
        public IEnumerator PointerCapture_KeepsInitialOwnership_UntilRelease()
        {
            Vector2 outside = new(-1000f, -1000f);
            Assert.IsTrue(_policy.TryBeginPointerCapture(
                GameplayInputKind.PointerPan,
                outside));

            CreateFullScreenBlocker();
            yield return null;
            Vector2 center = new(Screen.width * 0.5f, Screen.height * 0.5f);
            Assert.IsTrue(_policy.CanProcess(
                GameplayInputKind.PointerPan,
                center));

            _policy.EndPointerCapture(GameplayInputKind.PointerPan);
            Assert.IsFalse(_policy.CanProcess(
                GameplayInputKind.PointerPan,
                center));
        }

        private void CreateFullScreenBlocker()
        {
            var blocker = new GameObject(
                "Blocker",
                typeof(RectTransform),
                typeof(Image),
                typeof(GameplayInputBlocker));
            blocker.transform.SetParent(_canvasObject.transform, false);

            RectTransform rect = blocker.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            blocker.GetComponent<Image>().raycastTarget = true;
            Canvas.ForceUpdateCanvases();
        }
    }
}
