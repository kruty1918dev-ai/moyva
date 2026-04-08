using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.GameMode.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using Zenject;

namespace Kruty1918.Moyva.Tests.GameMode
{
    [TestFixture]
    public class GameModeChangeRequestRouterTests : ZenjectUnitTestFixture
    {
        private SignalBus _signalBus;
        private FakeGameModeService _gameModeService;
        private GameModeChangeRequestRouter _router;

        public override void Setup()
        {
            base.Setup();

            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<GameModeChangeRequestedSignal>();

            _gameModeService = new FakeGameModeService();
            _signalBus = Container.Resolve<SignalBus>();
            _router = new GameModeChangeRequestRouter(_signalBus, _gameModeService);
            _router.Initialize();
        }

        public override void Teardown()
        {
            _router.Dispose();
            base.Teardown();
        }

        [Test]
        public void GameModeChangeRequestedSignal_ShouldCallSetMode()
        {
            _signalBus.Fire(new GameModeChangeRequestedSignal
            {
                RequestedMode = GameModeType.Construction
            });

            Assert.AreEqual(1, _gameModeService.SetModeCalls);
            Assert.AreEqual(GameModeType.Construction, _gameModeService.LastRequestedMode);
        }

        [Test]
        public void Dispose_ShouldUnsubscribeFromSignal()
        {
            _router.Dispose();

            _signalBus.Fire(new GameModeChangeRequestedSignal
            {
                RequestedMode = GameModeType.Normal
            });

            Assert.AreEqual(0, _gameModeService.SetModeCalls);
        }

        private sealed class FakeGameModeService : IGameModeService
        {
            public GameModeType CurrentMode { get; private set; } = GameModeType.Normal;
            public int SetModeCalls { get; private set; }
            public GameModeType LastRequestedMode { get; private set; } = GameModeType.Normal;

            public void SetMode(GameModeType newMode)
            {
                SetModeCalls++;
                LastRequestedMode = newMode;
                CurrentMode = newMode;
            }
        }
    }
}
