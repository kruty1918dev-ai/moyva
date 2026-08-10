using System;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    /// <summary>
    /// Керує станом ігрового циклу: старт, пауза, відновлення, завершення.
    /// Надсилає відповідні сигнали через SignalBus.
    /// </summary>
    internal sealed class GameStateService : IGameStateService, IDisposable
    {
        public GameStateType CurrentState { get; private set; } = GameStateType.Idle;

        private readonly SignalBus _signalBus;
        private readonly IGamePauseModePolicy _pauseModePolicy;
        private readonly IGameplayInputPolicy _inputPolicy;
        private IDisposable _inputBlock;
        private float _timeScaleBeforePause = 1f;
        private bool _ownsSimulationPause;

        [Inject]
        public GameStateService(
            SignalBus signalBus,
            [InjectOptional] IGamePauseModePolicy pauseModePolicy = null,
            [InjectOptional] IGameplayInputPolicy inputPolicy = null)
        {
            _signalBus = signalBus;
            _pauseModePolicy = pauseModePolicy;
            _inputPolicy = inputPolicy;
        }

        public void StartGame()
        {
            CurrentState = GameStateType.Playing;
            _signalBus.Fire(new GameStartedSignal());
        }

        public void PauseGame()
        {
            if (CurrentState != GameStateType.Playing)
                return;

            CurrentState = GameStateType.Paused;
            _inputBlock = _inputPolicy?.AcquireBlock(
                GameplayInputKind.All,
                this);

            if (!(_pauseModePolicy?.IsMultiplayerSessionActive ?? false))
            {
                _timeScaleBeforePause = Time.timeScale > 0f
                    ? Time.timeScale
                    : 1f;
                Time.timeScale = 0f;
                _ownsSimulationPause = true;
            }
            _signalBus.Fire(new GamePausedSignal { IsPaused = true });
        }

        public void ResumeGame()
        {
            if (CurrentState != GameStateType.Paused)
                return;

            CurrentState = GameStateType.Playing;
            ReleasePauseState();
            _signalBus.Fire(new GamePausedSignal { IsPaused = false });
        }

        public void EndGame(string winnerId)
        {
            ReleasePauseState();
            CurrentState = GameStateType.GameOver;
            _signalBus.Fire(new GameEndedSignal { WinnerId = winnerId });
        }

        public void Dispose() => ReleasePauseState();

        private void ReleasePauseState()
        {
            _inputBlock?.Dispose();
            _inputBlock = null;

            if (_ownsSimulationPause)
                Time.timeScale = Mathf.Max(0.0001f, _timeScaleBeforePause);

            _ownsSimulationPause = false;
        }
    }
}
