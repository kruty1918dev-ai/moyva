using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    /// <summary>
    /// On HomeMenu scene startup, drains any pending host-disconnected notice
    /// produced by <see cref="HostDisconnectNotice"/> (set by SessionManager
    /// when the user was kicked while inside the gameplay scene) and surfaces
    /// it through <see cref="IInfoPanelService"/>.
    /// </summary>
    internal sealed class HostDisconnectNoticePresenter : IInitializable
    {
        [Inject(Optional = true)] private IInfoPanelService _infoPanelService = null;

        public void Initialize()
        {
            try
            {
                if (!HostDisconnectNotice.TryConsume(out var reason))
                    return;

                var message = BuildMessage(reason);
                _infoPanelService?.Show(new InfoMessage("Host Left the Game", message));
            }
            catch (Exception)
            {
            }
        }

        private static string BuildMessage(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return "Connection to the host was lost. The game returned to the main menu.";

            return $"Connection to the host was lost ({reason}). The game returned to the main menu.";
        }
    }
}
