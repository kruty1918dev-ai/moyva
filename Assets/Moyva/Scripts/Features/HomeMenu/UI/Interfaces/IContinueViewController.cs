using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    /// <summary>
    /// Контракт UI-контролера панелі Continue (вибір слоту збереження).
    /// Залежності: використовується ContinuePanelService.
    /// </summary>
    public interface IContinueViewController
    {
        /// <summary>Додати слот у список UI.</summary>
        void AddSlot(GameSlotInfo slot);

        /// <summary>Видалити слот зі списку за назвою.</summary>
        void RemoveSlot(string slotName);

        /// <summary>Очистити всі слоти в UI.</summary>
        void ClearSlots();

        /// <summary>Оновити відображення списку слотів.</summary>
        void RefreshSlots();

        /// <summary>Подія вибору слоту користувачем.</summary>
        event Action<GameSlotInfo> OnSlotSelected;
    }
}