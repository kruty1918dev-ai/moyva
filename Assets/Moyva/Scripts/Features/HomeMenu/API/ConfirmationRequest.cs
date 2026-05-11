using System;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
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
        public string LobbyId;
        public string HostDisplayName;
        public NetworkProviderType ProviderType;
        public int CurrentPlayers;
        public int MaxPlayers;
        /// <summary>True, якщо кімната захищена паролем.</summary>
        public bool HasPassword;
        /// <summary>True, якщо кімната позначена як приватна (не видна у списку).</summary>
        public bool IsPrivate;
        public RoomCapabilityFlags CapabilityFlags;

        public bool HasJoinCode => !string.IsNullOrWhiteSpace(JoinCode);
        public bool HasLobbyId => !string.IsNullOrWhiteSpace(LobbyId);

        public string DisplayKey
        {
            get
            {
                if (HasJoinCode) return JoinCode.Trim();
                if (HasLobbyId) return LobbyId.Trim();
                return string.Empty;
            }
        }

        public string DisplayIdentifier
        {
            get
            {
                if (HasJoinCode) return JoinCode.Trim();
                if (HasLobbyId) return $"LobbyId: {LobbyId.Trim()}";
                return "Код недоступний";
            }
        }

        public bool IsJoinable => HasJoinCode || HasLobbyId;

        public string HostOrRoomDisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(HostDisplayName)) return HostDisplayName.Trim();
                if (!string.IsNullOrWhiteSpace(RoomName)) return RoomName.Trim();
                return "Room";
            }
        }

        public string ProviderLabel
        {
            get
            {
                switch (ProviderType)
                {
                    case NetworkProviderType.Lan:
                        return "Local";
                    case NetworkProviderType.Relay:
                        return "Global";
                    case NetworkProviderType.WebSocket:
                        return "Global";
                    case NetworkProviderType.Offline:
                        return "Offline";
                    default:
                        return ProviderType.ToString();
                }
            }
        }
    }

    public enum WorldSize
    {
        Small,
        Medium,
        Large
    }
}