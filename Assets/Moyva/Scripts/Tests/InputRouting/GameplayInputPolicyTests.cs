using System;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.InputRouting.Runtime;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Tests.InputRouting
{
    public sealed class GameplayInputPolicyTests
    {
        private IGameplayInputPolicy _policy;
        private GameObject _eventSystemObject;
        private GameObject _canvasObject;

        [SetUp]
        public void SetUp()
        {
            _eventSystemObject = new GameObject(
                "TestEventSystem",
                typeof(EventSystem));
            EventSystem eventSystem = _eventSystemObject.GetComponent<EventSystem>();
            _policy = new GameplayInputPolicy(eventSystem);
            _canvasObject = new GameObject(
                "TestCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(GraphicRaycaster));
            _canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasObject != null)
                UnityEngine.Object.DestroyImmediate(_canvasObject);
            if (_eventSystemObject != null)
                UnityEngine.Object.DestroyImmediate(_eventSystemObject);
        }

        [Test]
        public void GlobalBlock_IsReleasedByLease()
        {
            Vector2 point = Vector2.zero;
            IDisposable lease = _policy.AcquireBlock(GameplayInputKind.All, this);

            Assert.IsFalse(_policy.CanProcess(GameplayInputKind.PointerZoom, point));
            Assert.IsFalse(_policy.CanProcess(GameplayInputKind.KeyboardNavigation, point));

            lease.Dispose();
            Assert.IsTrue(_policy.CanProcess(GameplayInputKind.PointerZoom, point));
        }

        [Test]
        public void FocusedTextField_BlocksKeyboardNavigation()
        {
            var inputObject = new GameObject("Input", typeof(RectTransform), typeof(TMP_InputField));
            inputObject.transform.SetParent(_canvasObject.transform, false);
            EventSystem eventSystem = _eventSystemObject.GetComponent<EventSystem>();
            eventSystem.SetSelectedGameObject(inputObject);

            Assert.IsFalse(_policy.CanProcess(GameplayInputKind.KeyboardNavigation, Vector2.zero));

            eventSystem.SetSelectedGameObject(null);
            UnityEngine.Object.DestroyImmediate(inputObject);
        }

    }
}
