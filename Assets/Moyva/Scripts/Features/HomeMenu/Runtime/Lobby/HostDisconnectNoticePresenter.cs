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
        [Zenject.InjectOptional] private Kruty1918.Moyva.Shared.Localization.ILocalizationService _loca;
        private string T(string key) => _loca?.T(key) ?? key ?? string.Empty;
        private string TF(string key, params object[] args) => _loca?.TF(key, args) ?? key ?? string.Empty;

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

        private string BuildMessage(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return T("Connection to the host was lost. The game returned to the main menu.");

            return TF("Connection to the host was lost ({0}). The game returned to the main menu.", reason);
        }
    }
}
