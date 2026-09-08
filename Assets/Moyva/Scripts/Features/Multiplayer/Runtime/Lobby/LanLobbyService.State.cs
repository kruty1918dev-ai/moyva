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

                var merged = MergeRooms(_current, incomingRoom, preferSecondState: !IsCurrentLocalHost());
                if (HaveSameRoomState(_current, merged))
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
                room.ReconnectRecords, room.StartedWorldSettingsBytes, room.BannedPlayerIds,
                room.CapabilityFlags, room.ConfigFingerprint);
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
                PruneExpiredDiscoveredRooms(DateTime.UtcNow);
                return _discoveredRooms.TryGetValue(value.Trim(), out var entry) ? entry.Room : null;
            }
        }

        private void RememberDiscoveredRoom(LobbyRoom room)
        {
            if (room == null)
                return;

            lock (_stateLock)
            {
                var now = DateTime.UtcNow;
                PruneExpiredDiscoveredRooms(now);

                var merged = room;
                foreach (var key in BuildDiscoveryKeys(room))
                {
                    if (_discoveredRooms.TryGetValue(key, out var existing))
                        merged = MergeRooms(existing.Room, merged, preferSecondState: true);
                }

                var entry = new DiscoveredRoomEntry(merged, now);
                AddDiscoveredKey(merged.LobbyId, entry);
                AddDiscoveredKey(merged.LobbyCode, entry);
                AddDiscoveredKey(merged.RelayJoinCode, entry);
            }
        }

        private void AddDiscoveredKey(string key, DiscoveredRoomEntry entry)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _discoveredRooms[key.Trim()] = entry;
        }

        private static bool IsLanJoinCode(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Trim().StartsWith("lan:", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsShortLanLobbyCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var trimmed = value.Trim();
            if (trimmed.Length != ShortLobbyCodeLength)
                return false;

            for (var i = 0; i < trimmed.Length; i++)
            {
                if (!char.IsDigit(trimmed[i]))
                    return false;
            }

            return true;
        }

        internal static string BuildShortLanLobbyCode(string roomId)
        {
            var source = string.IsNullOrWhiteSpace(roomId)
                ? Guid.NewGuid().ToString("N")
                : roomId.Trim();

            unchecked
            {
                var hash = 17;
                for (var i = 0; i < source.Length; i++)
                    hash = hash * 31 + source[i];

                var value = (int)((uint)hash % 1000000);
                return value.ToString("D6");
            }
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

        private IReadOnlyList<LobbyRoom> SnapshotDiscoveredRooms()
        {
            lock (_stateLock)
            {
                var now = DateTime.UtcNow;
                PruneExpiredDiscoveredRooms(now);

                var rooms = new List<LobbyRoom>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in _discoveredRooms.Values)
                {
                    var room = entry?.Room;
                    if (room == null)
                        continue;

                    var key = !string.IsNullOrWhiteSpace(room.LobbyId)
                        ? room.LobbyId
                        : (!string.IsNullOrWhiteSpace(room.RelayJoinCode) ? room.RelayJoinCode : room.LobbyCode);
                    if (string.IsNullOrWhiteSpace(key) || !seen.Add(key))
                        continue;

                    rooms.Add(room);
                }

                return rooms;
            }
        }

        private void PruneExpiredDiscoveredRooms(DateTime nowUtc)
        {
            var expired = new List<string>();
            foreach (var pair in _discoveredRooms)
            {
                if (pair.Value == null ||
                    (nowUtc - pair.Value.LastSeenUtc).TotalMilliseconds > DiscoveredRoomTtlMs)
                {
                    expired.Add(pair.Key);
                }
            }

            for (var i = 0; i < expired.Count; i++)
                _discoveredRooms.Remove(expired[i]);
        }

        private static IEnumerable<string> BuildDiscoveryKeys(LobbyRoom room)
        {
            if (room == null)
                yield break;

            if (!string.IsNullOrWhiteSpace(room.LobbyId))
                yield return room.LobbyId.Trim();
            if (!string.IsNullOrWhiteSpace(room.LobbyCode))
                yield return room.LobbyCode.Trim();
            if (!string.IsNullOrWhiteSpace(room.RelayJoinCode))
                yield return room.RelayJoinCode.Trim();
        }

        private bool IsCurrentLocalHost()
        {
            var current = _current;
            return current != null &&
                   string.Equals(current.HostPlayerId, BuildLocalHostId(), StringComparison.Ordinal);
        }

        private static LobbyRoom MergeRooms(LobbyRoom first, LobbyRoom second)
            => MergeRooms(first, second, preferSecondState: false);

        private static LobbyRoom MergeRooms(LobbyRoom first, LobbyRoom second, bool preferSecondState)
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
            var state = preferSecondState
                ? second?.State ?? first?.State ?? LobbyState.Open
                : first?.State ?? second?.State ?? LobbyState.Open;
            var reconnectRecords = preferSecondState && (second?.ReconnectRecords?.Count ?? 0) > 0
                ? second.ReconnectRecords
                : ((first?.ReconnectRecords?.Count ?? 0) > 0 ? first.ReconnectRecords : second?.ReconnectRecords);
            var worldSettings = preferSecondState && (second?.StartedWorldSettingsBytes?.Length ?? 0) > 0
                ? second.StartedWorldSettingsBytes
                : ((first?.StartedWorldSettingsBytes?.Length ?? 0) > 0 ? first.StartedWorldSettingsBytes : second?.StartedWorldSettingsBytes);
            var bannedPlayerIds = (first?.BannedPlayerIds?.Count ?? 0) > 0 ? first.BannedPlayerIds : second?.BannedPlayerIds;
            var capabilityFlags = first?.CapabilityFlags ?? second?.CapabilityFlags ?? RoomCapabilityFlags.None;
            var configFingerprint = !string.IsNullOrWhiteSpace(first?.ConfigFingerprint)
                ? first.ConfigFingerprint
                : second?.ConfigFingerprint;

            return new LobbyRoom(lobbyId, lobbyCode, name, maxPlayers, first?.IsPrivate ?? second?.IsPrivate ?? false,
                hostPlayerId, relayJoinCode, new List<LobbyPlayer>(playersById.Values), passwordHash, state,
                reconnectRecords, worldSettings, bannedPlayerIds, capabilityFlags, configFingerprint);
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

        private static bool HaveSameRoomState(LobbyRoom first, LobbyRoom second)
        {
            if (first?.Players == null || second?.Players == null)
                return first?.Players == second?.Players;

            if (first.State != second.State)
                return false;

            if (!string.Equals(first.RelayJoinCode, second.RelayJoinCode, StringComparison.Ordinal))
                return false;

            if (!HaveSameBytes(first.StartedWorldSettingsBytes, second.StartedWorldSettingsBytes))
                return false;

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

        private static bool HaveSameBytes(byte[] first, byte[] second)
        {
            var firstLength = first?.Length ?? 0;
            var secondLength = second?.Length ?? 0;
            if (firstLength != secondLength)
                return false;

            for (var i = 0; i < firstLength; i++)
            {
                if (first[i] != second[i])
                    return false;
            }

            return true;
        }

    }
}
