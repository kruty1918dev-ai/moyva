using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.UIActions.API;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    internal sealed class GameModeUiActionHandler :
        IUiActionHandler,
        IInitializable,
        System.IDisposable
    {
        private readonly IGameStateService _gameState;
        private readonly IGameModeService _gameMode;
        private readonly IUiContextStack _contexts;
        private readonly SignalBus _signalBus;
        private System.IDisposable _pauseContext;

        public GameModeUiActionHandler(
            IGameStateService gameState,
            IGameModeService gameMode,
            IUiContextStack contexts,
            SignalBus signalBus)
        {
            _gameState = gameState;
            _gameMode = gameMode;
            _contexts = contexts;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _pauseContext = _contexts.Push(new UiContextRegistration(
                "PauseModal",
                UiContextLayer.Modal,
                100,
                () => _gameState.CurrentState == GameStateType.Paused,
                UiActionId.PauseClose,
                blocksLowerHotkeys: true,
                allowedHotkeyActionIds: new[] { UiActionId.PauseClose }));
        }

        public void Dispose()
        {
            _pauseContext?.Dispose();
        }

        public bool CanHandle(string actionId)
        {
            return actionId == UiActionId.BuildOpen
                || actionId == UiActionId.BuildClose
                || actionId == UiActionId.BuildToggle
                || actionId == UiActionId.PauseOpen
                || actionId == UiActionId.PauseClose;
        }

        public UiActionResult Handle(UiActionRequest request)
        {
            switch (request.ActionId)
            {
                case UiActionId.BuildOpen:
                    return RequestMode(GameModeType.Construction);
                case UiActionId.BuildClose:
                    return RequestMode(GameModeType.Normal);
                case UiActionId.BuildToggle:
                    return RequestMode(
                        _gameMode.CurrentMode == GameModeType.Construction
                            ? GameModeType.Normal
                            : GameModeType.Construction);
                case UiActionId.PauseOpen:
                    if (_gameState.CurrentState == GameStateType.Paused)
                        return UiActionResult.Ignored();
                    if (_gameMode.CurrentMode != GameModeType.Normal)
                        return UiActionResult.Rejected(UiActionReasonCode.WrongContext);
                    _gameState.PauseGame();
                    return UiActionResult.Performed();
                case UiActionId.PauseClose:
                    if (_gameState.CurrentState != GameStateType.Paused)
                        return UiActionResult.Ignored();
                    _gameState.ResumeGame();
                    return UiActionResult.Performed();
                default:
                    return UiActionResult.Ignored(UiActionReasonCode.ActionUnavailable);
            }
        }

        private UiActionResult RequestMode(GameModeType requestedMode)
        {
            if (_gameMode.CurrentMode == requestedMode)
                return UiActionResult.Ignored();

            _signalBus.Fire(new GameModeChangeRequestedSignal
            {
                RequestedMode = requestedMode,
            });
            return UiActionResult.Performed();
        }
    }
}
