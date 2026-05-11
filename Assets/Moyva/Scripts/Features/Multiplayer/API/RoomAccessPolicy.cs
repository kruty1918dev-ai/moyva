using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Lobbies;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    public interface IRoomAccessPolicyService
    {
        bool CanKick(LobbyRoom room, string actorPlayerId, string targetPlayerId);
        bool CanBan(LobbyRoom room, string actorPlayerId, string targetPlayerId);
        bool CanJoin(LobbyRoom room, string playerId, out string reason);
    }

    public sealed class RoomAccessPolicyService : IRoomAccessPolicyService
    {
        public bool CanKick(LobbyRoom room, string actorPlayerId, string targetPlayerId)
        {
            if (room == null || string.IsNullOrWhiteSpace(actorPlayerId) || string.IsNullOrWhiteSpace(targetPlayerId))
                return false;

            if (!string.Equals(room.HostPlayerId, actorPlayerId, StringComparison.Ordinal))
                return false;

            if (string.Equals(actorPlayerId, targetPlayerId, StringComparison.Ordinal))
                return false;

            return !string.Equals(room.HostPlayerId, targetPlayerId, StringComparison.Ordinal);
        }

        public bool CanBan(LobbyRoom room, string actorPlayerId, string targetPlayerId)
        {
            return CanKick(room, actorPlayerId, targetPlayerId);
        }

        public bool CanJoin(LobbyRoom room, string playerId, out string reason)
        {
            reason = string.Empty;
            if (room == null)
            {
                reason = "Кімната недоступна.";
                return false;
            }

            if (room.BannedPlayerIds != null)
            {
                for (int index = 0; index < room.BannedPlayerIds.Count; index++)
                {
                    if (string.Equals(room.BannedPlayerIds[index], playerId, StringComparison.Ordinal))
                    {
                        reason = "Вас заблоковано у цій кімнаті.";
                        return false;
                    }
                }
            }

            return true;
        }
    }
}