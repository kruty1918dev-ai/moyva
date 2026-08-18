using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using UnityEngine.InputSystem;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    /// <summary>
    /// Top-level Esc fallback. Higher-priority construction/modal contexts consume Esc first.
    /// </summary>
    internal sealed class GameplayPauseInputController : IInitializable, ITickable
    {
        private readonly IGameStateService _gameState;
        private readonly IGameModeService _gameMode;

        public GameplayPauseInputController(
            IGameStateService gameState,
            IGameModeService gameMode)
        {
            _gameState = gameState;
            _gameMode = gameMode;
        }

        public void Initialize()
        {
            if (_gameState.CurrentState == GameStateType.Idle)
                _gameState.StartGame();
        }

        public void Tick()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
                return;

            if (_gameState.CurrentState == GameStateType.Paused)
            {
                _gameState.ResumeGame();
                return;
            }

            if (_gameMode.CurrentMode == GameModeType.Construction)
                return;

            if (_gameState.CurrentState == GameStateType.Playing)
                _gameState.PauseGame();
        }
    }
}
