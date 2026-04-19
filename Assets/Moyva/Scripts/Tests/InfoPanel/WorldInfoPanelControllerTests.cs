using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Tests.InfoPanel
{
    [TestFixture]
    public sealed class WorldInfoPanelControllerTests : ZenjectUnitTestFixture
    {
        private SignalBus _signalBus;
        private GameObject _panelRoot;
        private Component _title;
        private Component _subtitle;
        private Component _content;
        private Button _closeButton;
        private object _controller;
        private MethodInfo _initializeMethod;
        private MethodInfo _disposeMethod;

        public override void Setup()
        {
            base.Setup();
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<WorldInfoPanelRequestedSignal>();
            Container.DeclareSignal<WorldInfoPanelClosedSignal>();

            _signalBus = Container.Resolve<SignalBus>();

            var tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            Assert.NotNull(tmpType, "Не знайдено TMPro.TextMeshProUGUI.");

            var controllerType = Type.GetType("Kruty1918.Moyva.InfoPanel.UI.WorldInfoPanelController, Kruty1918.Moyva.InfoPanel");
            Assert.NotNull(controllerType, "Не знайдено WorldInfoPanelController у збірці Kruty1918.Moyva.InfoPanel.");

            _panelRoot = new GameObject("WorldInfoPanelRoot");
            _panelRoot.SetActive(true);

            _title = new GameObject("TitleText").AddComponent(tmpType);
            _subtitle = new GameObject("SubtitleText").AddComponent(tmpType);
            _content = new GameObject("ContentText").AddComponent(tmpType);
            _closeButton = new GameObject("CloseButton").AddComponent<Button>();

            _controller = Activator.CreateInstance(
                controllerType,
                _signalBus,
                _panelRoot,
                _title,
                _subtitle,
                _content,
                _closeButton);
            Assert.NotNull(_controller);

            _initializeMethod = controllerType.GetMethod("Initialize");
            _disposeMethod = controllerType.GetMethod("Dispose");
            Assert.NotNull(_initializeMethod);
            Assert.NotNull(_disposeMethod);

            _initializeMethod.Invoke(_controller, null);
        }

        public override void Teardown()
        {
            _disposeMethod.Invoke(_controller, null);
            UnityEngine.Object.DestroyImmediate(_panelRoot);
            UnityEngine.Object.DestroyImmediate(_title.gameObject);
            UnityEngine.Object.DestroyImmediate(_subtitle.gameObject);
            UnityEngine.Object.DestroyImmediate(_content.gameObject);
            UnityEngine.Object.DestroyImmediate(_closeButton.gameObject);
            base.Teardown();
        }

        [Test]
        public void Initialize_HidesPanel()
        {
            Assert.IsFalse(_panelRoot.activeSelf);
        }

        [Test]
        public void RequestedSignal_ShowsPanelAndUpdatesText()
        {
            _signalBus.Fire(new WorldInfoPanelRequestedSignal
            {
                Title = "Заголовок",
                Subtitle = "Підзаголовок",
                Content = "Контент"
            });

            Assert.IsTrue(_panelRoot.activeSelf);
            Assert.AreEqual("Заголовок", ReadText(_title));
            Assert.AreEqual("Підзаголовок", ReadText(_subtitle));
            Assert.AreEqual("Контент", ReadText(_content));
        }

        [Test]
        public void CloseButton_HidesPanel_AndFiresClosedSignal()
        {
            bool closedRaised = false;
            _signalBus.Subscribe<WorldInfoPanelClosedSignal>(_ => closedRaised = true);

            _signalBus.Fire(new WorldInfoPanelRequestedSignal
            {
                Title = "T",
                Subtitle = "S",
                Content = "C"
            });

            _closeButton.onClick.Invoke();

            Assert.IsFalse(_panelRoot.activeSelf);
            Assert.IsTrue(closedRaised);
        }

        private static string ReadText(Component textComponent)
        {
            var property = textComponent.GetType().GetProperty("text");
            Assert.NotNull(property);
            return property.GetValue(textComponent) as string;
        }
    }
}
