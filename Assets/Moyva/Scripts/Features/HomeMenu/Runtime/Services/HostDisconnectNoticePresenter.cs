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
                _infoPanelService?.Show(new InfoMessage("Хост покинув гру", message));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[HostDisconnectNoticePresenter] {e.Message}");
            }
        }

        private static string BuildMessage(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return "З'єднання з хостом втрачено. Гру повернуто до головного меню.";

            return $"З'єднання з хостом втрачено ({reason}). Гру повернуто до головного меню.";
        }
    }
}
