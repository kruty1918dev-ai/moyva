using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public interface IContinueViewController
    {
        void AddSlot(GameSlotInfo slot);
        void RemoveSlot(string slotName);
        void ClearSlots();
        void RefreshSlots();
        event Action<GameSlotInfo> OnSlotSelected; // pass slot 
    }
}