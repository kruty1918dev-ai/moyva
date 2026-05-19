using System;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контракт UI-контролера вибору мережевого режиму multiplayer.
    /// Залежності: використовується MultiplayerModePanelService.
    /// </summary>
    public interface IMultiplayerModeViewController
    {
        /// <summary>Поточний обраний мережевий провайдер.</summary>
        NetworkProviderType SelectedMode { get; set; }

        /// <summary>Подія зміни мережевого режиму.</summary>
        event Action<NetworkProviderType> OnModeChanged;

        /// <summary>Оновити UI відповідно до поточного стану.</summary>
        void Refresh();
    }
}