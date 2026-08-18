using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.GameMode.API;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    internal sealed class ExitMatchCoordinator : IExitMatchCoordinator
    {
        private readonly IGameStateService _gameState;
        private readonly IGamePauseModePolicy _pauseModePolicy;
        private readonly IExitMatchSaveHandler _saveHandler;
        private readonly IExitMatchDisconnectHandler _disconnectHandler;
        private readonly IExitMatchSceneLoader _sceneLoader;
        private readonly object _gate = new object();

        private Task<ExitMatchResult> _activeExit;

        public ExitMatchCoordinator(
            IGameStateService gameState,
            IExitMatchSceneLoader sceneLoader,
            [InjectOptional] IGamePauseModePolicy pauseModePolicy = null,
            [InjectOptional] IExitMatchSaveHandler saveHandler = null,
            [InjectOptional] IExitMatchDisconnectHandler disconnectHandler = null)
        {
            _gameState = gameState;
            _sceneLoader = sceneLoader;
            _pauseModePolicy = pauseModePolicy;
            _saveHandler = saveHandler;
            _disconnectHandler = disconnectHandler;
        }

        public bool IsExiting
        {
            get
            {
                lock (_gate)
                    return _activeExit != null && !_activeExit.IsCompleted;
            }
        }

        public Task<ExitMatchResult> ExitToMenuAsync(
            CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                if (_activeExit != null && !_activeExit.IsCompleted)
                    return _activeExit;

                _activeExit = ExecuteAsync(cancellationToken);
                return _activeExit;
            }
        }

        private async Task<ExitMatchResult> ExecuteAsync(
            CancellationToken cancellationToken)
        {
            bool wasPaused = _gameState.CurrentState == GameStateType.Paused;
            bool resumedForTransition = false;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                bool multiplayer = _pauseModePolicy?.IsMultiplayerSessionActive
                    ?? false;

                if (multiplayer)
                {
                    if (_disconnectHandler != null)
                    {
                        await _disconnectHandler.DisconnectBeforeExitAsync(
                            cancellationToken);
                    }
                }
                else if (_saveHandler != null)
                {
                    await _saveHandler.SaveBeforeExitAsync(cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (wasPaused)
                {
                    _gameState.ResumeGame();
                    resumedForTransition = true;
                }

                await _sceneLoader.LoadHomeMenuAsync(cancellationToken);
                return ExitMatchResult.Success();
            }
            catch (OperationCanceledException)
            {
                RestorePauseAfterFailedTransition(wasPaused, resumedForTransition);
                return ExitMatchResult.Cancellation();
            }
            catch (Exception exception)
            {
                RestorePauseAfterFailedTransition(wasPaused, resumedForTransition);
                return ExitMatchResult.Failure(exception.Message);
            }
        }

        private void RestorePauseAfterFailedTransition(
            bool wasPaused,
            bool resumedForTransition)
        {
            if (wasPaused
                && resumedForTransition
                && _gameState.CurrentState == GameStateType.Playing)
            {
                _gameState.PauseGame();
            }
        }
    }

    internal sealed class HomeMenuSceneLoader : IExitMatchSceneLoader
    {
        private const string HomeMenuSceneName = "HomeMenu";

        public Task LoadHomeMenuAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AsyncOperation operation = SceneManager.LoadSceneAsync(
                HomeMenuSceneName,
                LoadSceneMode.Single);

            if (operation == null)
                throw new InvalidOperationException(
                    $"Scene '{HomeMenuSceneName}' could not be loaded.");

            if (operation.isDone)
                return Task.CompletedTask;

            var completion = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            operation.completed += _ => completion.TrySetResult(true);
            return completion.Task;
        }
    }
}
