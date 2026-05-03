using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public interface IKickPlayerPanelViewController
    {
        event Action OnCloseRequested;
        event Action OnRefreshRequested;
        event Action<KickPlayerInfo> OnKickRequested;

        void SetPlayers(IReadOnlyList<KickPlayerInfo> players);
        void ClearPlayers();
        void SetStatus(string status);
        void SetInteractable(bool interactable);
    }
}