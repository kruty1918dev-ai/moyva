using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    public sealed partial class LanLobbyService
    {
        private static bool TryParsePayload(string payload, out LobbyRoom room, out string joinCode)
        {
            room = null;
            joinCode = string.Empty;

            var parts = payload?.Split('|');
            if (parts == null || parts.Length < 6)
                return false;

            if (!string.Equals(parts[0], PayloadProtocol, StringComparison.Ordinal))
                return false;

            var roomId = parts[1];
            if (string.IsNullOrWhiteSpace(roomId))
                return false;

            var name = parts[2];
            var max = int.TryParse(parts[3], out var parsedMax) ? parsedMax : 4;
            var ip = parts[4];
            var port = parts[5];
            if (!IPAddress.TryParse(ip, out var parsedIp) || parsedIp.AddressFamily != AddressFamily.InterNetwork)
                return false;

            if (!int.TryParse(port, out var parsedPort) || parsedPort <= 0 || parsedPort > 65535)
                return false;

            var hostId = parts.Length >= 7 && !string.IsNullOrWhiteSpace(parts[6]) ? parts[6] : roomId;
            var hostName = parts.Length >= 8 ? parts[7] : string.Empty;
            var players = parts.Length >= 9 ? DeserializePlayers(parts[8]) : new List<LobbyPlayer>();
            if (players.Count == 0 && !string.IsNullOrWhiteSpace(hostName))
                players.Add(new LobbyPlayer(hostId, hostName.Trim(), isHost: true));

            var isPrivate = parts.Length >= 10 && parts[9] == "1";
            var passwordHash = parts.Length >= 11 ? parts[10] : string.Empty;
            var state = LobbyState.Open;
            if (parts.Length >= 12 && int.TryParse(parts[11], out var rawState) && Enum.IsDefined(typeof(LobbyState), rawState))
                state = (LobbyState)rawState;
            var worldSettings = parts.Length >= 13 ? DecodeBytes(parts[12]) : Array.Empty<byte>();
            var configFingerprint = parts.Length >= 14 ? parts[13] : string.Empty;

            joinCode = $"lan:{ip}:{port}";
            var lobbyCode = roomId.Length >= 8 ? roomId.Substring(0, 8) : roomId;
            room = new LobbyRoom(roomId, lobbyCode, name, max, isPrivate, hostId, joinCode, players, passwordHash, state,
                startedWorldSettingsBytes: worldSettings,
                configFingerprint: configFingerprint);
            return true;
        }

        private void PublishState(LobbyState state)
        {
            if (_state == state) return;
            _state = state;
            StateChanged?.Invoke(state);
        }

        private static bool IsDiscoveryQuery(string payload)
        {
            var parts = payload?.Split('|');
            return parts != null && parts.Length >= 2 &&
                   string.Equals(parts[0], PayloadProtocol, StringComparison.Ordinal) &&
                   string.Equals(parts[1], DiscoveryQuery, StringComparison.Ordinal);
        }

        private static string SerializePlayers(LobbyRoom room)
        {
            if (room?.Players == null || room.Players.Count == 0)
                return string.Empty;

            var parts = new List<string>(room.Players.Count);
            foreach (var player in room.Players)
            {
                if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
                    continue;

                parts.Add($"{EncodeToken(player.PlayerId)}~{EncodeToken(player.DisplayName)}~{(player.IsHost ? "1" : "0")}");
            }

            return string.Join(",", parts);
        }

        private static List<LobbyPlayer> DeserializePlayers(string value)
        {
            var players = new List<LobbyPlayer>();
            if (string.IsNullOrWhiteSpace(value))
                return players;

            var items = value.Split(',');
            foreach (var item in items)
            {
                var parts = item.Split('~');
                if (parts.Length < 3)
                    continue;

                var playerId = DecodeToken(parts[0]);
                if (string.IsNullOrWhiteSpace(playerId))
                    continue;

                players.Add(new LobbyPlayer(playerId, DecodeToken(parts[1]), parts[2] == "1"));
            }

            return players;
        }

        private static string EncodeToken(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private static string DecodeToken(string value)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(value ?? string.Empty));
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string EncodeBytes(byte[] bytes)
        {
            return bytes == null || bytes.Length == 0 ? string.Empty : Convert.ToBase64String(bytes);
        }

        private static byte[] DecodeBytes(string encoded)
        {
            if (string.IsNullOrWhiteSpace(encoded))
                return Array.Empty<byte>();

            try { return Convert.FromBase64String(encoded); }
            catch { return Array.Empty<byte>(); }
        }

    }
}
