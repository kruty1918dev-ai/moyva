using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Turns.API;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class GameplayProgressClockConfigurator : IInitializable
    {
        private readonly IGameplayProgressClock _clock;
        private readonly BootstrapGameSettings _settings;

        public GameplayProgressClockConfigurator(
            IGameplayProgressClock clock,
            BootstrapGameSettings settings)
        {
            _clock = clock;
            _settings = settings ?? new BootstrapGameSettings();
        }

        public void Initialize()
        {
            GameLaunchContext.EnsureNotExpired();
            bool sandbox =
                GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest
                || GameLaunchContext.Source == GameLaunchSource.DirectGameplayTest
                || (GameLaunchContext.HasWorldSettings && GameLaunchContext.MaxPlayers <= 1);

            _clock.Configure(
                sandbox ? GameplayProgressMode.SandboxRealtime : GameplayProgressMode.TurnBased,
                _settings.SandboxProgress?.RoundSeconds ?? 10f,
                _settings.SandboxProgress?.InitialSpeed ?? 1f);
        }
    }
}
