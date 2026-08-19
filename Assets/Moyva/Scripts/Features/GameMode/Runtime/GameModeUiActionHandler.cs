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
                UiActionIds.Pause.Close,
                blocksLowerHotkeys: true,
                allowedHotkeyActionIds: new[] { UiActionIds.Pause.Close }));
        }

        public void Dispose()
        {
            _pauseContext?.Dispose();
        }

        public System.Collections.Generic.IReadOnlyCollection<UiActionId> ActionIds { get; } =
            new[]
            {
                UiActionIds.Construction.Open,
                UiActionIds.Construction.Close,
                UiActionIds.Construction.Toggle,
                UiActionIds.Pause.Open,
                UiActionIds.Pause.Close,
            };

        public UiActionResult Execute(in UiActionRequest request)
        {
            if (request.ActionId == UiActionIds.Construction.Open)
                return RequestMode(GameModeType.Construction);

            if (request.ActionId == UiActionIds.Construction.Close)
                return RequestMode(GameModeType.Normal);

            if (request.ActionId == UiActionIds.Construction.Toggle)
                return RequestMode(
                    _gameMode.CurrentMode == GameModeType.Construction
                        ? GameModeType.Normal
                        : GameModeType.Construction);

            if (request.ActionId == UiActionIds.Pause.Open)
            {
                if (_gameState.CurrentState == GameStateType.Paused)
                    return UiActionResult.Ignored(UiActionReason.AlreadyOpen);
                if (_gameMode.CurrentMode != GameModeType.Normal)
                    return UiActionResult.Rejected(UiActionReason.WrongContext);
                _gameState.PauseGame();
                return UiActionResult.Performed();
            }

            if (request.ActionId == UiActionIds.Pause.Close)
            {
                if (_gameState.CurrentState != GameStateType.Paused)
                    return UiActionResult.Ignored(UiActionReason.AlreadyClosed);
                _gameState.ResumeGame();
                return UiActionResult.Performed();
            }

            return UiActionResult.Ignored(UiActionReason.ActionUnavailable);
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
