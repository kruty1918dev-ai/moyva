using Kruty1918.Moyva.SaveSystem;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    internal sealed class BotDiagnosticsLaunchObserver : IInitializable
    {
        private readonly IBotDiagnosticsLogger _log;

        public BotDiagnosticsLaunchObserver(IBotDiagnosticsLogger log)
        {
            _log = log;
        }

        public void Initialize()
        {
            _log.Info(
                BotDiagnosticCategory.Session,
                "LAUNCH.CONTEXT",
                "Gameplay launch context captured for BotAI diagnostics.",
                details:
                    $"mode={GameLaunchContext.Mode}; source={GameLaunchContext.Source}; " +
                    $"hasWorldSettings={GameLaunchContext.HasWorldSettings}; " +
                    $"maxPlayers={GameLaunchContext.MaxPlayers}; " +
                    $"saveSlot={GameLaunchContext.SaveSlot}; " +
                    $"autoLoad={GameLaunchContext.IsAutoLoadEnabled()}");

            if (GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest &&
                GameLaunchContext.MaxPlayers < 2)
            {
                _log.Error(
                    BotDiagnosticCategory.Session,
                    "LAUNCH.DIRECT_TOO_FEW_PLAYERS",
                    "Direct Gameplay launch is configured with fewer than two participants.",
                    reason:
                        "A default Human + Bot session requires MaxPlayers >= 2.",
                    details:
                        $"mode={GameLaunchContext.Mode}; maxPlayers={GameLaunchContext.MaxPlayers}");
            }
        }
    }
}
