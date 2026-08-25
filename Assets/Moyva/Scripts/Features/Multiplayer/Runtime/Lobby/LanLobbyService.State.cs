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
        private bool MergeCurrentRoom(LobbyRoom incomingRoom)
        {
            if (incomingRoom == null)
                return false;

            lock (_stateLock)
            {
                if (_current == null || !string.Equals(_current.LobbyId, incomingRoom.LobbyId, StringComparison.Ordinal))
                    return false;

                var merged = MergeRooms(_current, incomingRoom);
                if (HaveSamePlayers(_current, merged))
                    return false;

                _current = merged;
                return true;
            }
        }

        private static LobbyRoom AddLocalPlayer(LobbyRoom room, string displayName)
        {
            var localId = BuildLocalHostId();
            var players = new List<LobbyPlayer>(room.Players ?? Array.Empty<LobbyPlayer>());
            var exists = false;
            foreach (var player in players)
            {
                if (string.Equals(player.PlayerId, localId, StringComparison.Ordinal))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
                players.Add(new LobbyPlayer(localId, string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName.Trim(), isHost: false));

            return new LobbyRoom(room.LobbyId, room.LobbyCode, room.Name, room.MaxPlayers, room.IsPrivate,
                room.HostPlayerId, room.RelayJoinCode, players, room.PasswordHash, room.State,
                room.ReconnectRecords, room.StartedWorldSettingsBytes);
        }

        private static bool MatchesJoinInput(LobbyRoom room, string value)
        {
            if (room == null || string.IsNullOrWhiteSpace(value))
                return false;

            return string.Equals(room.LobbyCode, value, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(room.LobbyId, value, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(room.RelayJoinCode, value, StringComparison.OrdinalIgnoreCase);
        }

        private LobbyRoom FindDiscoveredRoom(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            lock (_stateLock)
            {
                return _discoveredRooms.TryGetValue(value.Trim(), out var room) ? room : null;
            }
        }

        private void RememberDiscoveredRoom(LobbyRoom room)
        {
            if (room == null)
                return;

            lock (_stateLock)
            {
                AddDiscoveredKey(room.LobbyId, room);
                AddDiscoveredKey(room.LobbyCode, room);
                AddDiscoveredKey(room.RelayJoinCode, room);
            }
        }

        private void AddDiscoveredKey(string key, LobbyRoom room)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _discoveredRooms[key.Trim()] = room;
        }

        private static bool IsLanJoinCode(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Trim().StartsWith("lan:", StringComparison.OrdinalIgnoreCase);
        }

        private static LobbyRoom CreateDirectJoinRoom(string joinCode, string displayName)
        {
            var localId = BuildLocalHostId();
            var players = new List<LobbyPlayer>
            {
                new LobbyPlayer(localId, string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName.Trim(), isHost: false)
            };

            return new LobbyRoom(joinCode.Trim(), joinCode.Trim(), "LAN Room", 4, false,
                string.Empty, joinCode.Trim(), players, state: LobbyState.Open);
        }

        private static LobbyRoom MergeRooms(LobbyRoom first, LobbyRoom second)
        {
            var playersById = new Dictionary<string, LobbyPlayer>(StringComparer.Ordinal);
            AddPlayers(playersById, first?.Players);
            AddPlayers(playersById, second?.Players);

            var lobbyId = !string.IsNullOrWhiteSpace(first?.LobbyId) ? first.LobbyId : second?.LobbyId;
            var lobbyCode = !string.IsNullOrWhiteSpace(first?.LobbyCode) ? first.LobbyCode : second?.LobbyCode;
            var name = !string.IsNullOrWhiteSpace(first?.Name) ? first.Name : second?.Name;
            var maxPlayers = first?.MaxPlayers > 0 ? first.MaxPlayers : (second?.MaxPlayers ?? 4);
            var hostPlayerId = !string.IsNullOrWhiteSpace(first?.HostPlayerId) ? first.HostPlayerId : second?.HostPlayerId;
            var relayJoinCode = !string.IsNullOrWhiteSpace(first?.RelayJoinCode) ? first.RelayJoinCode : second?.RelayJoinCode;
            var passwordHash = !string.IsNullOrEmpty(first?.PasswordHash) ? first.PasswordHash : second?.PasswordHash;
            var state = first?.State ?? second?.State ?? LobbyState.Open;
            var reconnectRecords = (first?.ReconnectRecords?.Count ?? 0) > 0 ? first.ReconnectRecords : second?.ReconnectRecords;
            var worldSettings = (first?.StartedWorldSettingsBytes?.Length ?? 0) > 0 ? first.StartedWorldSettingsBytes : second?.StartedWorldSettingsBytes;

            return new LobbyRoom(lobbyId, lobbyCode, name, maxPlayers, first?.IsPrivate ?? second?.IsPrivate ?? false,
                hostPlayerId, relayJoinCode, new List<LobbyPlayer>(playersById.Values), passwordHash, state,
                reconnectRecords, worldSettings);
        }

        private static void AddPlayers(Dictionary<string, LobbyPlayer> playersById, IReadOnlyList<LobbyPlayer> players)
        {
            if (players == null)
                return;

            foreach (var player in players)
            {
                if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
                    continue;

                playersById[player.PlayerId] = player;
            }
        }

        private static bool HaveSamePlayers(LobbyRoom first, LobbyRoom second)
        {
            if (first?.Players == null || second?.Players == null)
                return first?.Players == second?.Players;

            if (first.Players.Count != second.Players.Count)
                return false;

            var firstPlayers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var player in first.Players)
                firstPlayers.Add(player.PlayerId);

            foreach (var player in second.Players)
            {
                if (!firstPlayers.Contains(player.PlayerId))
                    return false;
            }

            return true;
        }

    }
}
