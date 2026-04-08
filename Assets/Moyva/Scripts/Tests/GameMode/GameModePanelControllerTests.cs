using System.Collections.Generic;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.GameMode.Runtime;
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
        private GameModePanelController _controller;

        public override void Setup()
        {
            base.Setup();

            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<GameModeChangedSignal>();

            _normalPanel = new FakePanel(GameModeType.Normal);
            _constructionPanel = new FakePanel(GameModeType.Construction);

            Container.Bind<IGameModePanel>().FromInstance(_normalPanel);
            Container.Bind<IGameModePanel>().FromInstance(_constructionPanel);

            Container.BindInterfacesAndSelfTo<GameModePanelController>().AsSingle().NonLazy();

            _signalBus = Container.Resolve<SignalBus>();
            _controller = Container.Resolve<GameModePanelController>();
            _controller.Initialize();
        }

        public override void Teardown()
        {
            _controller.Dispose();
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
            Container.Bind<IGameModePanel>().FromInstance(extraConstruction);

            // Manually build a controller with all three panels
            var allPanels = new List<IGameModePanel>
            {
                _normalPanel,
                _constructionPanel,
                extraConstruction
            };
            var ctrl = new GameModePanelController(allPanels, _signalBus);
            ctrl.Initialize();

            _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Construction });

            Assert.IsTrue(_constructionPanel.IsVisible);
            Assert.IsTrue(extraConstruction.IsVisible);
            Assert.IsFalse(_normalPanel.IsVisible);

            ctrl.Dispose();
        }

        // ─── No panels registered ─────────────────────────────────────────────

        [Test]
        public void NoPanels_SignalFired_NoException()
        {
            var emptyController = new GameModePanelController(
                new List<IGameModePanel>(), _signalBus);
            emptyController.Initialize();

            Assert.DoesNotThrow(() =>
                _signalBus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Construction }));

            emptyController.Dispose();
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
