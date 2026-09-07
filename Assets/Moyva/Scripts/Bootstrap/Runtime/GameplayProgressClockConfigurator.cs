using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Turns.API;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class GameplayProgressClockConfigurator : IInitializable
    {
        private const float DefaultSandboxRoundSeconds = 10f;

        private readonly IGameplayProgressClock _clock;

        public GameplayProgressClockConfigurator(IGameplayProgressClock clock)
        {
            _clock = clock;
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
                DefaultSandboxRoundSeconds,
                1f);
        }
    }
}
