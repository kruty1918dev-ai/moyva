using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    internal sealed class GameModeService : IGameModeService
    {
        private readonly SignalBus _signalBus;

        public GameModeType CurrentMode { get; private set; } = GameModeType.Normal;

        [Inject]
        public GameModeService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void SetMode(GameModeType newMode)
        {
            if (newMode == CurrentMode)
                return;

            CurrentMode = newMode;
            _signalBus.Fire(new GameModeChangedSignal { NewMode = newMode });
        }
    }
}
