#if MOYVA_LEGACY_SCRIPTABLEOBJECT_TESTS
using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.GameMode.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.GameMode
{
    public sealed class ExitMatchCoordinatorTests
    {
        [Test]
        public async Task SoloExit_SavesResumesAndLoadsMenu()
        {
            var state = new FakeGameState(GameStateType.Paused);
            var save = new RecordingSaveHandler();
            var disconnect = new RecordingDisconnectHandler();
            var loader = new RecordingSceneLoader(state);
            var coordinator = Create(
                state,
                multiplayer: false,
                save,
                disconnect,
                loader);

            ExitMatchResult result = await coordinator.ExitToMenuAsync();

            Assert.That(result.Succeeded, Is.True);
            Assert.That(save.CallCount, Is.EqualTo(1));
            Assert.That(disconnect.CallCount, Is.Zero);
            Assert.That(state.ResumeCount, Is.EqualTo(1));
            Assert.That(loader.StatusWhenLoaded, Is.EqualTo(GameStateType.Playing));
        }

        [Test]
        public async Task MultiplayerExit_DisconnectsWithoutWritingSoloSave()
        {
            var state = new FakeGameState(GameStateType.Paused);
            var save = new RecordingSaveHandler();
            var disconnect = new RecordingDisconnectHandler();
            var coordinator = Create(
                state,
                multiplayer: true,
                save,
                disconnect,
                new RecordingSceneLoader(state));

            ExitMatchResult result = await coordinator.ExitToMenuAsync();

            Assert.That(result.Succeeded, Is.True);
            Assert.That(save.CallCount, Is.Zero);
            Assert.That(disconnect.CallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task ConcurrentExitRequests_ShareOneActiveFlow()
        {
            var state = new FakeGameState(GameStateType.Playing);
            var save = new BlockingSaveHandler();
            var loader = new RecordingSceneLoader(state);
            var coordinator = Create(
                state,
                multiplayer: false,
                save,
                new RecordingDisconnectHandler(),
                loader);

            Task<ExitMatchResult> first = coordinator.ExitToMenuAsync();
            Task<ExitMatchResult> second = coordinator.ExitToMenuAsync();

            Assert.That(second, Is.SameAs(first));
            Assert.That(coordinator.IsExiting, Is.True);

            save.Complete();
            ExitMatchResult result = await first;

            Assert.That(result.Succeeded, Is.True);
            Assert.That(save.CallCount, Is.EqualTo(1));
            Assert.That(loader.CallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task SceneLoadFailure_RestoresPreviousPauseState()
        {
            var state = new FakeGameState(GameStateType.Paused);
            var coordinator = Create(
                state,
                multiplayer: false,
                new RecordingSaveHandler(),
                new RecordingDisconnectHandler(),
                new FailingSceneLoader());

            ExitMatchResult result = await coordinator.ExitToMenuAsync();

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Error, Does.Contain("load failed"));
            Assert.That(state.CurrentState, Is.EqualTo(GameStateType.Paused));
            Assert.That(state.ResumeCount, Is.EqualTo(1));
            Assert.That(state.PauseCount, Is.EqualTo(1));
        }

        private static ExitMatchCoordinator Create(
            IGameStateService gameState,
            bool multiplayer,
            IExitMatchSaveHandler saveHandler,
            IExitMatchDisconnectHandler disconnectHandler,
            IExitMatchSceneLoader loader)
        {
            return new ExitMatchCoordinator(
                gameState,
                loader,
                new PauseModePolicy(multiplayer),
                saveHandler,
                disconnectHandler);
        }

        private sealed class PauseModePolicy : IGamePauseModePolicy
        {
            public PauseModePolicy(bool active)
            {
                IsMultiplayerSessionActive = active;
            }

            public bool IsMultiplayerSessionActive { get; }
        }

        private sealed class FakeGameState : IGameStateService
        {
            public FakeGameState(GameStateType state)
            {
                CurrentState = state;
            }

            public GameStateType CurrentState { get; private set; }
            public int PauseCount { get; private set; }
            public int ResumeCount { get; private set; }

            public void StartGame() => CurrentState = GameStateType.Playing;

            public void PauseGame()
            {
                PauseCount++;
                CurrentState = GameStateType.Paused;
            }

            public void ResumeGame()
            {
                ResumeCount++;
                CurrentState = GameStateType.Playing;
            }

            public void EndGame(string winnerId)
                => CurrentState = GameStateType.GameOver;
        }

        private class RecordingSaveHandler : IExitMatchSaveHandler
        {
            public int CallCount { get; protected set; }

            public virtual Task SaveBeforeExitAsync(
                CancellationToken cancellationToken)
            {
                CallCount++;
                return Task.CompletedTask;
            }
        }

        private sealed class BlockingSaveHandler : RecordingSaveHandler
        {
            private readonly TaskCompletionSource<bool> _completion =
                new TaskCompletionSource<bool>();

            public override Task SaveBeforeExitAsync(
                CancellationToken cancellationToken)
            {
                CallCount++;
                return _completion.Task;
            }

            public void Complete() => _completion.TrySetResult(true);
        }

        private sealed class RecordingDisconnectHandler
            : IExitMatchDisconnectHandler
        {
            public int CallCount { get; private set; }

            public Task DisconnectBeforeExitAsync(
                CancellationToken cancellationToken)
            {
                CallCount++;
                return Task.CompletedTask;
            }
        }

        private sealed class RecordingSceneLoader : IExitMatchSceneLoader
        {
            private readonly IGameStateService _state;

            public RecordingSceneLoader(IGameStateService state)
            {
                _state = state;
            }

            public int CallCount { get; private set; }
            public GameStateType StateWhenLoaded { get; private set; }

            public Task LoadHomeMenuAsync(CancellationToken cancellationToken)
            {
                CallCount++;
                StateWhenLoaded = _state.CurrentState;
                return Task.CompletedTask;
            }
        }

        private sealed class FailingSceneLoader : IExitMatchSceneLoader
        {
            public Task LoadHomeMenuAsync(CancellationToken cancellationToken)
                => throw new InvalidOperationException("load failed");
        }
    }
}

#endif
