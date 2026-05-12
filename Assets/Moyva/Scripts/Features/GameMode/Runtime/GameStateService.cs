using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    /// <summary>
    /// Керує станом ігрового циклу: старт, пауза, відновлення, завершення.
    /// Надсилає відповідні сигнали через SignalBus.
    /// </summary>
    internal sealed class GameStateService : IGameStateService
    {
        public GameStateType CurrentState { get; private set; } = GameStateType.Idle;

        private readonly SignalBus _signalBus;
        private float _cachedTimeScale = 1f;
        private bool _isPaused;

        [Inject]
        public GameStateService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void StartGame()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            CurrentState = GameStateType.Playing;
            _signalBus.Fire(new GameStartedSignal());
        }

        public void PauseGame()
        {
            if (CurrentState != GameStateType.Playing || _isPaused)
                return;

            CurrentState = GameStateType.Paused;
            _isPaused = true;
            _cachedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _signalBus.Fire(new GamePausedSignal { IsPaused = true });
        }

        public void ResumeGame()
        {
            if (CurrentState != GameStateType.Paused || !_isPaused)
                return;

            CurrentState = GameStateType.Playing;
            _isPaused = false;
            Time.timeScale = _cachedTimeScale > 0f ? _cachedTimeScale : 1f;
            _signalBus.Fire(new GamePausedSignal { IsPaused = false });
        }

        public void EndGame(string winnerId)
        {
            _isPaused = false;
            Time.timeScale = 1f;
            CurrentState = GameStateType.GameOver;
            _signalBus.Fire(new GameEndedSignal { WinnerId = winnerId });
        }
    }
}
