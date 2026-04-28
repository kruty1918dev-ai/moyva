using System;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public struct ConfirmationRequest
    {
        public string LabelText;
        public string MessageText;
        public System.Action OnConfirm;
        public System.Action OnCancel;
    }

    public enum BotDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum BotStrategy
    {
        Random,
        Defensive,
        Aggressive
    }

    public struct GameSlotInfo
    {
        public string SlotName;
        public int SlotIndex;
        public DateTime LastModified;
    }


    public struct RoomInfo
    {
        public string RoomName;
        public string JoinCode;
        public int CurrentPlayers;
        public int MaxPlayers;
    }

    public enum WorldSize
    {
        Small,
        Medium,
        Large
    }
}