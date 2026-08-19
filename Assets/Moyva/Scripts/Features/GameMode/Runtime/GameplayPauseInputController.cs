using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.UIActions.API;
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
        private readonly IUiEscapeRouter _escapeRouter;
        private readonly IUiActionRouter _actions;

        public GameplayPauseInputController(
            IGameStateService gameState,
            IUiEscapeRouter escapeRouter,
            IUiActionRouter actions)
        {
            _gameState = gameState;
            _escapeRouter = escapeRouter;
            _actions = actions;
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

            if (_escapeRouter.TryHandleEscape())
                return;

            _actions.Execute(
                _gameState.CurrentState == GameStateType.Paused
                    ? UiActionId.PauseClose
                    : UiActionId.PauseOpen,
                UiActionSource.Escape,
                "Gameplay");
        }
    }
}
