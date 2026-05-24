using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.SaveSystem;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IBootstrapStarterPackDecisionService
    {
        bool IsMultiplayerClient();
        bool ShouldGrantForCurrentLaunch();
    }

    internal sealed class BootstrapStarterPackDecisionService : IBootstrapStarterPackDecisionService
    {
    #pragma warning disable CS0649
        [InjectOptional] private ISessionManager _sessionManager;
    #pragma warning restore CS0649

        public bool IsMultiplayerClient()
        {
            if (_sessionManager == null)
                return false;
            if (_sessionManager.Participants == null || _sessionManager.Participants.Count <= 1)
                return false;
            return !_sessionManager.IsLocalPlayerHost;
        }

        public bool ShouldGrantForCurrentLaunch()
        {
            switch (GameLaunchContext.Mode)
            {
                case GameLaunchMode.MenuLoadGame:
                case GameLaunchMode.MenuJoinGame:
                    return false;
                case GameLaunchMode.DirectGameplayTest:
                case GameLaunchMode.MenuNewGame:
                case GameLaunchMode.MenuMultiplayerGame:
                case GameLaunchMode.Unknown:
                default:
                    return true;
            }
        }
    }
}