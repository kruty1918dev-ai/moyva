using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.GameMode.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.UIActions.API;
using NUnit.Framework;
using Zenject;

namespace Kruty1918.Moyva.Tests.GameMode
{
    /// <summary>
    /// Mode-exit veto contract: IGameModeTransitionPolicy implementations can
    /// block Construction -> Normal (initial castle not placed yet) for every
    /// exit route — ClosePanel, mode toggle and direct signal requests routed
    /// through GameModeChangeRequestRouter.
    /// </summary>
    public class GameModeUiActionHandlerTests
    {
        private SignalBus _bus;
        private FakeGameStateService _gameState;
        private FakeGameModeService _gameMode;
        private FakeUiContextStack _contexts;
        private FakeTransitionPolicy _policy;
        private GameModeUiActionHandler _handler;
        private int _modeRequests;

        [SetUp]
        public void SetUp()
        {
            _modeRequests = 0;
            var container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<GameModeChangeRequestedSignal>().OptionalSubscriber();
            _bus = container.Resolve<SignalBus>();
            _bus.Subscribe<GameModeChangeRequestedSignal>(_ => _modeRequests++);

            _gameState = new FakeGameStateService();
            _gameMode = new FakeGameModeService();
            _contexts = new FakeUiContextStack();
            _policy = new FakeTransitionPolicy();
            _handler = new GameModeUiActionHandler(
                _gameState, _gameMode, _contexts, _bus,
                new List<IGameModeTransitionPolicy> { _policy });
        }

        [TearDown]
        public void TearDown()
        {
            _handler?.Dispose();
            _handler = null;
        }

        [Test]
        public void Close_WhenPolicyBlocksExit_ReturnsRejectedWithReason()
        {
            _gameMode.CurrentMode = GameModeType.Construction;
            _policy.BlockReason = "Place your first castle before leaving construction mode.";

            UiActionResult result = _handler.Execute(Request(UiActionIds.Construction.Close));

            Assert.AreEqual(UiActionStatus.Rejected, result.Status);
            Assert.AreEqual(_policy.BlockReason, result.Details);
            Assert.AreEqual(0, _modeRequests, "Rejected transition must not fire a mode request.");
        }

        [Test]
        public void Toggle_FromConstruction_WhenPolicyBlocksExit_ReturnsRejected()
        {
            _gameMode.CurrentMode = GameModeType.Construction;
            _policy.BlockReason = "blocked";

            UiActionResult result = _handler.Execute(Request(UiActionIds.Construction.Toggle));

            Assert.AreEqual(UiActionStatus.Rejected, result.Status);
            Assert.AreEqual(0, _modeRequests);
        }

        [Test]
        public void Close_WhenPolicyAllows_PerformsAndFiresRequest()
        {
            _gameMode.CurrentMode = GameModeType.Construction;
            _policy.BlockReason = null;

            UiActionResult result = _handler.Execute(Request(UiActionIds.Construction.Close));

            Assert.AreEqual(UiActionStatus.Performed, result.Status);
            Assert.AreEqual(1, _modeRequests);
            Assert.IsTrue(_policy.Consulted, "Policy must be consulted before firing the request.");
        }

        [Test]
        public void Close_WithoutPolicies_Performs()
        {
            _gameMode.CurrentMode = GameModeType.Construction;
            _handler = new GameModeUiActionHandler(_gameState, _gameMode, _contexts, _bus);

            UiActionResult result = _handler.Execute(Request(UiActionIds.Construction.Close));

            Assert.AreEqual(UiActionStatus.Performed, result.Status);
            Assert.AreEqual(1, _modeRequests);
        }

        [Test]
        public void Router_WhenPolicyBlocks_DoesNotChangeMode()
        {
            _gameMode.CurrentMode = GameModeType.Construction;
            _policy.BlockReason = "blocked";
            var router = new GameModeChangeRequestRouter(
                _bus, _gameMode, new List<IGameModeTransitionPolicy> { _policy });
            router.Initialize();
            try
            {
                _bus.Fire(new GameModeChangeRequestedSignal { RequestedMode = GameModeType.Normal });
            }
            finally
            {
                router.Dispose();
            }

            Assert.AreEqual(0, _gameMode.SetModeCalls, "Blocked transition must not reach SetMode.");
            Assert.AreEqual(GameModeType.Construction, _gameMode.CurrentMode);
        }

        [Test]
        public void Router_WhenPolicyAllows_AppliesMode()
        {
            _gameMode.CurrentMode = GameModeType.Construction;
            var router = new GameModeChangeRequestRouter(
                _bus, _gameMode, new List<IGameModeTransitionPolicy> { _policy });
            router.Initialize();
            try
            {
                _bus.Fire(new GameModeChangeRequestedSignal { RequestedMode = GameModeType.Normal });
            }
            finally
            {
                router.Dispose();
            }

            Assert.AreEqual(1, _gameMode.SetModeCalls);
        }

        private static UiActionRequest Request(UiActionId actionId)
            => new(actionId, UiActionSource.Programmatic, "Tests");

        private sealed class FakeGameStateService : IGameStateService
        {
            public GameStateType CurrentState { get; set; } = GameStateType.Playing;
            public void StartGame() { }
            public void PauseGame() => CurrentState = GameStateType.Paused;
            public void ResumeGame() => CurrentState = GameStateType.Playing;
            public void EndGame(string winnerId) { }
        }

        private sealed class FakeGameModeService : IGameModeService
        {
            public GameModeType CurrentMode { get; set; } = GameModeType.Normal;
            public int SetModeCalls;

            public void SetMode(GameModeType newMode)
            {
                SetModeCalls++;
                CurrentMode = newMode;
            }
        }

        private sealed class FakeUiContextStack : IUiContextStack
        {
            public string ActiveContextId => null;
            public IReadOnlyList<UiContextRegistration> ActiveContexts { get; }
                = new List<UiContextRegistration>();

            public IDisposable Push(UiContextRegistration registration)
                => new PushHandle();

            public bool IsActionAllowedByContext(UiActionId actionId) => true;

            private sealed class PushHandle : IDisposable
            {
                public void Dispose() { }
            }
        }

        private sealed class FakeTransitionPolicy : IGameModeTransitionPolicy
        {
            public string BlockReason;
            public bool Consulted;

            public bool TryGetTransitionBlockReason(
                GameModeType currentMode,
                GameModeType requestedMode,
                out string reason)
            {
                Consulted = true;
                reason = BlockReason;
                return BlockReason != null;
            }
        }
    }
}
