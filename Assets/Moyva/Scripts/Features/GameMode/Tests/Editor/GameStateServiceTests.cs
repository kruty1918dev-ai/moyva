using System;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.GameMode.Runtime;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.GameMode
{
    /// <summary>
    /// Pause/resume contract of GameStateService: time scale ownership in
    /// single-player vs multiplayer, input blocking, signal emission and
    /// cleanup on Dispose — all pinned against a real SignalBus.
    /// </summary>
    public class GameStateServiceTests
    {
        private SignalBus _bus;
        private FakePausePolicy _pausePolicy;
        private FakeInputPolicy _inputPolicy;
        private GameStateService _service;

        private int _startedCount;
        private int _pausedCount;
        private int _resumedCount;
        private string _endedWinner = "<unset>";

        [SetUp]
        public void SetUp()
        {
            _bus = CreateSignalBus();
            _bus.Subscribe<GameStartedSignal>(() => _startedCount++);
            _bus.Subscribe<GamePausedSignal>(s =>
            {
                if (s.IsPaused) _pausedCount++;
                else _resumedCount++;
            });
            _bus.Subscribe<GameEndedSignal>(s => _endedWinner = s.WinnerId);

            _pausePolicy = new FakePausePolicy();
            _inputPolicy = new FakeInputPolicy();
            _service = new GameStateService(_bus, _pausePolicy, _inputPolicy);
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            _service = null;
            Time.timeScale = 1f;
        }

        [Test]
        public void Pause_SinglePlayer_ZeroesTimeScaleAndBlocksInput()
        {
            _service.StartGame();
            Time.timeScale = 1f;

            _service.PauseGame();

            Assert.AreEqual(GameStateType.Paused, _service.CurrentState);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.AreEqual(1, _inputPolicy.BlocksAcquired);
            Assert.AreEqual(1, _pausedCount);
        }

        [Test]
        public void Resume_SinglePlayer_RestoresTimeScaleAndReleasesBlock()
        {
            _service.StartGame();
            _service.PauseGame();

            _service.ResumeGame();

            Assert.AreEqual(GameStateType.Playing, _service.CurrentState);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(0, _inputPolicy.ActiveBlocks);
            Assert.AreEqual(1, _resumedCount);
        }

        [Test]
        public void Pause_Multiplayer_DoesNotTouchTimeScale()
        {
            _pausePolicy.IsMultiplayerSessionActive = true;
            _service.StartGame();
            Time.timeScale = 1f;

            _service.PauseGame();

            Assert.AreEqual(GameStateType.Paused, _service.CurrentState);
            Assert.AreEqual(1f, Time.timeScale, "timeScale must not change in multiplayer");
        }

        [Test]
        public void Pause_WhenNotPlaying_IsIgnored()
        {
            _service.PauseGame(); // Idle

            Assert.AreEqual(GameStateType.Idle, _service.CurrentState);
            Assert.AreEqual(0, _pausedCount);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void Resume_WhenNotPaused_IsIgnored()
        {
            _service.StartGame();
            _service.ResumeGame();

            Assert.AreEqual(0, _resumedCount);
        }

        [Test]
        public void Pause_PreservesPriorTimeScale()
        {
            _service.StartGame();
            Time.timeScale = 0.5f;

            _service.PauseGame();
            _service.ResumeGame();

            Assert.AreEqual(0.5f, Time.timeScale);
        }

        [Test]
        public void Dispose_WhilePaused_RestoresTimeScaleAndReleasesBlock()
        {
            _service.StartGame();
            _service.PauseGame();

            _service.Dispose();
            _service = null;

            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(0, _inputPolicy.ActiveBlocks);
        }

        [Test]
        public void StartGame_FiresStartedSignalAndClearsWinner()
        {
            _service.EndGame("red");
            _service.StartGame();

            Assert.AreEqual(GameStateType.Playing, _service.CurrentState);
            Assert.AreEqual(string.Empty, _service.WinnerId);
            Assert.AreEqual(1, _startedCount);
        }

        [Test]
        public void EndGame_FiresEndedWithWinnerAndBlocksInput()
        {
            _service.StartGame();
            _service.EndGame("blue");

            Assert.IsTrue(_service.IsGameOver);
            Assert.AreEqual("blue", _endedWinner);
            Assert.AreEqual(1, _inputPolicy.BlocksAcquired);
        }

        [Test]
        public void EndGame_SameWinnerTwice_FiresOnce()
        {
            _service.StartGame();
            _service.EndGame("blue");
            _endedWinner = "<unset>";

            _service.EndGame("blue");

            Assert.AreEqual("<unset>", _endedWinner, "duplicate EndGame must not re-fire");
        }

        private static SignalBus CreateSignalBus()
        {
            var container = new DiContainer();
            SignalBusInstaller.Install(container);
            container.DeclareSignal<GameStartedSignal>().OptionalSubscriber();
            container.DeclareSignal<GamePausedSignal>().OptionalSubscriber();
            container.DeclareSignal<GameEndedSignal>().OptionalSubscriber();
            return container.Resolve<SignalBus>();
        }

        private sealed class FakePausePolicy : IGamePauseModePolicy
        {
            public bool IsMultiplayerSessionActive { get; set; }
        }

        private sealed class FakeInputPolicy : IGameplayInputPolicy
        {
            public int BlocksAcquired;
            public int ActiveBlocks;

            public bool CanProcess(GameplayInputKind inputKind, Vector2 screenPosition, int pointerId = -1)
                => true;

            public bool IsPointerOverUi(Vector2 screenPosition, int pointerId = -1, bool interactiveOnly = true)
                => false;

            public bool TryBeginPointerCapture(GameplayInputKind inputKind, Vector2 screenPosition, int pointerId = -1)
                => true;

            public void EndPointerCapture(GameplayInputKind inputKind, int pointerId = -1) { }

            public IDisposable AcquireBlock(GameplayInputKind inputMask, object owner)
            {
                BlocksAcquired++;
                ActiveBlocks++;
                return new Block(this);
            }

            private sealed class Block : IDisposable
            {
                private readonly FakeInputPolicy _policy;
                private bool _disposed;

                public Block(FakeInputPolicy policy) => _policy = policy;

                public void Dispose()
                {
                    if (_disposed) return;
                    _disposed = true;
                    _policy.ActiveBlocks--;
                }
            }
        }
    }
}
