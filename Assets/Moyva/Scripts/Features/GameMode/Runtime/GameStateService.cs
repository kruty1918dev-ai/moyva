using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
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

        [Inject]
        public GameStateService(SignalBus signalBus)
        {
            _signalBus = signalBus;
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
            _signalBus.Fire(new GamePausedSignal { IsPaused = true });
        }

        public void ResumeGame()
        {
            if (CurrentState != GameStateType.Paused)
                return;

            CurrentState = GameStateType.Playing;
            _signalBus.Fire(new GamePausedSignal { IsPaused = false });
        }

        public void EndGame(string winnerId)
        {
            CurrentState = GameStateType.GameOver;
            _signalBus.Fire(new GameEndedSignal { WinnerId = winnerId });
        }
    }
}
