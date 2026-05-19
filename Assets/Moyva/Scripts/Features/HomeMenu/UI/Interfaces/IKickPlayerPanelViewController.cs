using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контракт UI-панелі керування виключенням гравців з лобі.
    /// Залежності: використовується KickPlayerPanelService.
    /// </summary>
    public interface IKickPlayerPanelViewController
    {
        /// <summary>Подія закриття панелі.</summary>
        event Action OnCloseRequested;

        /// <summary>Подія запиту оновлення списку гравців.</summary>
        event Action OnRefreshRequested;

        /// <summary>Подія запиту виключення конкретного гравця.</summary>
        event Action<KickPlayerInfo> OnKickRequested;

        /// <summary>Встановити поточний список гравців у UI.</summary>
        void SetPlayers(IReadOnlyList<KickPlayerInfo> players);

        /// <summary>Очистити список гравців у UI.</summary>
        void ClearPlayers();

        /// <summary>Встановити текст статусу операції.</summary>
        void SetStatus(string status);

        /// <summary>Увімкнути/вимкнути взаємодію з панеллю.</summary>
        void SetInteractable(bool interactable);
    }
}