using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using Zenject;

namespace Kruty1918.Moyva.Tests.GameMode
{
    /// <summary>
    /// Юніт-тести для GameModePanelController.
    /// Перевіряють, що при зміні режиму відповідна панель показується,
    /// а всі інші приховуються — без необхідності змінювати контролер.
    /// </summary>
    [TestFixture]
    public class GameModePanelControllerTests : ZenjectUnitTestFixture
    {
        private SignalBus _signalBus;
        private FakePanel _normalPanel;
        private FakePanel _constructionPanel;
        private object _controllerInstance;

        public override void Setup()
        {
            base.Setup();

            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<GameModeChangedSignal>();

            _normalPanel = new FakePanel(GameModeType.Normal);
            _constructionPanel = new FakePanel(GameModeType.Construction);

            Container.Bind<IGameModePanel>().FromInstance(_normalPanel);
            Container.Bind<IGameModePanel>().FromInstance(_constructionPanel);

            var controllerType = typeof(IGameModeService).Assembly
                .GetType("Kruty1918.Moyva.GameMode.Runtime.GameModePanelController");
            Assert.NotNull(controllerType, "Не знайдено тип GameModePanelController у збірці GameMode.");

            _signalBus = Container.Resolve<SignalBus>();
            _controllerInstance = System.Activator.CreateInstance(controllerType,
                new List<IGameModePanel> { _normalPanel, _constructionPanel }, _signalBus);
            Assert.NotNull(_controllerInstance);

            InvokeLifecycle(_controllerInstance, "Initialize");
        }

        public override void Teardown()
        {
            if (_controllerInstance != null)
                InvokeLifecycle(_controllerInstance, "Dispose");
            base.Teardown();
        }

        // ─── Show / Hide ──────────────────────────────────────────────────────

        [Test]
        public void OnConstructionMode_ConstructionPanelShown_NormalPanelHidden()
        {
            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Construction });

            Assert.IsTrue(_constructionPanel.IsVisible);
            Assert.IsFalse(_normalPanel.IsVisible);
        }

        [Test]
        public void OnNormalMode_NormalPanelShown_ConstructionPanelHidden()
        {
            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Normal });

            Assert.IsTrue(_normalPanel.IsVisible);
            Assert.IsFalse(_constructionPanel.IsVisible);
        }

        // ─── Mode switch ──────────────────────────────────────────────────────

        [Test]
        public void ModeSwitchTwice_CorrectPanelActiveEachTime()
        {
            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Construction });
            Assert.IsTrue(_constructionPanel.IsVisible);
            Assert.IsFalse(_normalPanel.IsVisible);

            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Normal });
            Assert.IsTrue(_normalPanel.IsVisible);
            Assert.IsFalse(_constructionPanel.IsVisible);
        }

        // ─── Multiple panels for same mode ────────────────────────────────────

        [Test]
        public void MultiplePanelsForSameMode_AllShown()
        {
            var extraConstruction = new FakePanel(GameModeType.Construction);
            var controllerType = typeof(IGameModeService).Assembly
                .GetType("Kruty1918.Moyva.GameMode.Runtime.GameModePanelController");
            Assert.NotNull(controllerType);

            var allPanels = new List<IGameModePanel> { _normalPanel, _constructionPanel, extraConstruction };
            var ctrl = System.Activator.CreateInstance(controllerType, allPanels, _signalBus);
            Assert.NotNull(ctrl);

            InvokeLifecycle(ctrl, "Initialize");

            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Construction });

            Assert.IsTrue(_constructionPanel.IsVisible);
            Assert.IsTrue(extraConstruction.IsVisible);
            Assert.IsFalse(_normalPanel.IsVisible);

            InvokeLifecycle(ctrl, "Dispose");
        }

        // ─── No panels registered ─────────────────────────────────────────────

        [Test]
        public void NoPanels_SignalFired_NoException()
        {
            var controllerType = typeof(IGameModeService).Assembly
                .GetType("Kruty1918.Moyva.GameMode.Runtime.GameModePanelController");
            Assert.NotNull(controllerType);

            var emptyController = System.Activator.CreateInstance(controllerType,
                new List<IGameModePanel>(), _signalBus);
            Assert.NotNull(emptyController);

            InvokeLifecycle(emptyController, "Initialize");

            Assert.DoesNotThrow(() =>
                _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Construction }));

            InvokeLifecycle(emptyController, "Dispose");
        }

        private static void InvokeLifecycle(object instance, string methodName)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method, $"Метод '{methodName}' не знайдено у {instance.GetType().FullName}.");
            method.Invoke(instance, null);
        }

        // ─── Fake panel ───────────────────────────────────────────────────────

        private sealed class FakePanel : IGameModePanel
        {
            public GameModeType TargetMode { get; }
            public bool IsVisible { get; private set; }

            public FakePanel(GameModeType targetMode) => TargetMode = targetMode;

            public void Show() => IsVisible = true;
            public void Hide() => IsVisible = false;
        }
    }
}
