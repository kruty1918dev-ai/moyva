using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Lobbies;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    internal static class MultiplayerRoomLifecycle
    {
        public static IReadOnlyList<GameplayPlayer> ProjectGameplayPlayers(
            LobbyRoom lobby,
            string localPlayerId,
            bool localPlayerIsHost = false)
        {
            var players = new List<GameplayPlayer>();
            bool hasLocalPlayer = false;

            if (lobby?.Players != null)
            {
                foreach (var player in lobby.Players)
                {
                    var playerId = string.IsNullOrWhiteSpace(player.PlayerId)
                        ? $"player-{players.Count:00}"
                        : player.PlayerId;

                    bool isLocal =
                        string.Equals(
                            playerId,
                            localPlayerId,
                            StringComparison.Ordinal);
                    hasLocalPlayer |= isLocal;

                    players.Add(new GameplayPlayer(
                        playerId,
                        string.IsNullOrWhiteSpace(player.DisplayName) ? playerId : player.DisplayName,
                        player.IsHost,
                        isLocal));
                }
            }

            if (!hasLocalPlayer
                && !string.IsNullOrWhiteSpace(localPlayerId))
            {
                string normalizedLocalPlayerId = localPlayerId.Trim();
                players.Add(new GameplayPlayer(
                    normalizedLocalPlayerId,
                    normalizedLocalPlayerId,
                    localPlayerIsHost,
                    isLocal: true));
            }

            return players;
        }

        public static bool IsReconnectAllowed(LobbyRoom lobby, string playerName, float toleranceSeconds)
        {
            if (lobby?.ReconnectRecords == null || lobby.ReconnectRecords.Count == 0)
                return false;

            var normalizedName = NormalizeName(playerName);
            if (string.IsNullOrEmpty(normalizedName))
                return false;

            long nowLocalTicks = DateTime.Now.Ticks;
            long nowHostTicks = DateTime.UtcNow.Ticks;
            long toleranceTicks = TimeSpan.FromSeconds(Math.Max(0f, toleranceSeconds)).Ticks;

            foreach (var record in lobby.ReconnectRecords)
            {
                if (!string.Equals(NormalizeName(record.DisplayName), normalizedName, StringComparison.OrdinalIgnoreCase))
                    continue;

                long playerElapsed = Math.Max(0, nowLocalTicks - record.PlayerLocalTicksAtDisconnect);
                long hostElapsed = Math.Max(0, nowHostTicks - record.HostUtcTicksAtDisconnect);
                if (Math.Abs(playerElapsed - hostElapsed) <= toleranceTicks)
                    return true;
            }

            return false;
        }

        public static string GetJoinBlockReason(LobbyRoom lobby, string playerName, float reconnectToleranceSeconds)
        {
            if (lobby == null)
                return "Room is unavailable.";

            if (lobby.State == LobbyState.Closed)
                return "Room is already closed.";

            bool reconnectAllowed = IsReconnectAllowed(lobby, playerName, reconnectToleranceSeconds);
            if (lobby.State == LobbyState.Started && !reconnectAllowed)
                return "Game has already started. Joining is only available for reconnect.";

            int playerCount = lobby.Players?.Count ?? 0;
            if (lobby.State == LobbyState.Open && lobby.MaxPlayers > 0 && playerCount >= lobby.MaxPlayers)
                return "Room is full.";

            if (string.IsNullOrWhiteSpace(lobby.RelayJoinCode) && string.IsNullOrWhiteSpace(lobby.LobbyId) && string.IsNullOrWhiteSpace(lobby.LobbyCode))
                return "Room has no join code.";

            return null;
        }

        private static string NormalizeName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
