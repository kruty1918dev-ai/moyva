using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal static class JoinRoomDomainLogic
    {
        public static string GetPostJoinBlockReason(LobbyRoom room, string playerName, float reconnectToleranceSeconds)
        {
            if (room == null)
                return "Кімната недоступна.";

            if (room.State == LobbyState.Closed)
                return "Кімната вже закрита.";

            if (room.State == LobbyState.Started &&
                !MultiplayerRoomLifecycle.IsReconnectAllowed(room, playerName, reconnectToleranceSeconds))
            {
                return "Гра вже запущена. Приєднання доступне лише для перепідключення з тим самим ніком і коректним локальним часом.";
            }

            return null;
        }

        public static string ResolveLocalPlayerId(LobbyRoom room, string playerName)
        {
            if (room?.Players == null)
                return string.Empty;

            foreach (var player in room.Players)
            {
                if (!string.IsNullOrWhiteSpace(player.PlayerId) &&
                    string.Equals(player.DisplayName, playerName, StringComparison.OrdinalIgnoreCase))
                {
                    return player.PlayerId;
                }
            }

            foreach (var player in room.Players)
            {
                if (!string.IsNullOrWhiteSpace(player.PlayerId) && !player.IsHost)
                    return player.PlayerId;
            }

            return string.Empty;
        }

        public static List<RoomInfo> ProjectRoomInfos(IReadOnlyList<LobbyRoom> rooms, NetworkProviderType providerType)
        {
            var result = new List<RoomInfo>();
            if (rooms == null)
                return result;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var room in rooms)
            {
                if (room == null)
                    continue;

                // Private rooms are hidden from discovery list and can be joined by direct code only.
                if (room.IsPrivate)
                    continue;

                if (room.State != LobbyState.Open)
                    continue;

                int currentPlayers = room.Players?.Count ?? 0;
                if (room.MaxPlayers > 0 && currentPlayers >= room.MaxPlayers)
                    continue;

                var key = BuildRoomKey(room, providerType);
                if (string.IsNullOrWhiteSpace(key) || !seen.Add(key))
                    continue;

                result.Add(new RoomInfo
                {
                    RoomName = room.Name,
                    JoinCode = room.LobbyCode,
                    LobbyId = room.LobbyId,
                    HostDisplayName = ResolveHostDisplayName(room),
                    ProviderType = providerType,
                    CurrentPlayers = currentPlayers,
                    MaxPlayers = room.MaxPlayers,
                    HasPassword = room.HasPassword,
                    IsPrivate = room.IsPrivate,
                    CapabilityFlags = room.CapabilityFlags,
                });
            }

            return result;
        }

        private static string BuildRoomKey(LobbyRoom room, NetworkProviderType providerType)
        {
            if (!string.IsNullOrWhiteSpace(room.LobbyId))
                return $"{providerType}:id:{room.LobbyId.Trim()}";

            if (!string.IsNullOrWhiteSpace(room.LobbyCode))
                return $"{providerType}:code:{room.LobbyCode.Trim()}";

            if (!string.IsNullOrWhiteSpace(room.RelayJoinCode))
                return $"{providerType}:relay:{room.RelayJoinCode.Trim()}";

            return string.Empty;
        }

        private static string ResolveHostDisplayName(LobbyRoom room)
        {
            if (room?.Players == null)
                return string.Empty;

            foreach (var player in room.Players)
            {
                if (player != null && player.IsHost && !string.IsNullOrWhiteSpace(player.DisplayName))
                    return player.DisplayName.Trim();
            }

            foreach (var player in room.Players)
            {
                if (player != null &&
                    string.Equals(player.PlayerId, room.HostPlayerId, StringComparison.Ordinal) &&
                    !string.IsNullOrWhiteSpace(player.DisplayName))
                {
                    return player.DisplayName.Trim();
                }
            }

            foreach (var player in room.Players)
            {
                if (player != null && !string.IsNullOrWhiteSpace(player.DisplayName))
                    return player.DisplayName.Trim();
            }

            return string.Empty;
        }
    }
}
