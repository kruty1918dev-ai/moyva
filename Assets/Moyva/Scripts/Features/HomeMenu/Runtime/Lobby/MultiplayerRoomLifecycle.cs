using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Lobbies;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    internal static class MultiplayerRoomLifecycle
    {
        public static IReadOnlyList<GameplayPlayer> ProjectGameplayPlayers(LobbyRoom lobby, string localPlayerId)
        {
            var players = new List<GameplayPlayer>();
            var usedIds = new HashSet<string>(StringComparer.Ordinal);

            if (lobby?.Players != null)
            {
                foreach (var player in lobby.Players)
                {
                    var playerId = string.IsNullOrWhiteSpace(player.PlayerId)
                        ? $"player-{players.Count:00}"
                        : player.PlayerId;

                    players.Add(new GameplayPlayer(
                        playerId,
                        string.IsNullOrWhiteSpace(player.DisplayName) ? playerId : player.DisplayName,
                        player.IsHost,
                        string.Equals(playerId, localPlayerId, StringComparison.Ordinal)));
                    usedIds.Add(playerId);
                }
            }

            int targetCount = Math.Max(players.Count, lobby?.MaxPlayers ?? players.Count);
            for (int index = players.Count; index < targetCount; index++)
            {
                var botId = $"bot-{index:00}";
                while (usedIds.Contains(botId))
                    botId = $"bot-{index:00}-{usedIds.Count:00}";

                players.Add(new GameplayPlayer(botId, $"Бот {index + 1}", false, false));
                usedIds.Add(botId);
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
                return "Кімната недоступна.";

            if (lobby.State == LobbyState.Closed)
                return "Кімната вже закрита.";

            bool reconnectAllowed = IsReconnectAllowed(lobby, playerName, reconnectToleranceSeconds);
            if (lobby.State == LobbyState.Started && !reconnectAllowed)
                return "Гра вже запущена. Приєднання доступне лише для перепідключення.";

            int playerCount = lobby.Players?.Count ?? 0;
            if (lobby.State == LobbyState.Open && lobby.MaxPlayers > 0 && playerCount >= lobby.MaxPlayers)
                return "Кімната вже заповнена.";

            if (string.IsNullOrWhiteSpace(lobby.RelayJoinCode) && string.IsNullOrWhiteSpace(lobby.LobbyId) && string.IsNullOrWhiteSpace(lobby.LobbyCode))
                return "Кімната не має коду для підключення.";

            return null;
        }

        private static string NormalizeName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
