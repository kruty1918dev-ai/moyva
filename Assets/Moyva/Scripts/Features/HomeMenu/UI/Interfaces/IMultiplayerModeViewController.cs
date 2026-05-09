using System;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public interface IMultiplayerModeViewController
    {
        NetworkProviderType SelectedMode { get; set; }
        event Action<NetworkProviderType> OnModeChanged;
        void Refresh();
    }
}