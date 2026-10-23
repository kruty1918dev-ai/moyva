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
        // Roster authority model: the host's snapshot is authoritative for
        // clients (replace, not union-merge — unions can never remove members).
        // Client packets are presence heartbeats; the host consumes only the
        // sender's own player record from them so stale client views cannot
        // resurrect departed players.
        private bool MergeCurrentRoom(LobbyRoom incomingRoom, string senderId)
        {
            if (incomingRoom == null)
                return false;

            var localId = BuildLocalHostId();
            var now = DateTime.UtcNow;
            lock (_stateLock)
            {
                bool weAreHost = IsCurrentLocalHost();
                bool senderIsAuthoritativeHost = !string.IsNullOrWhiteSpace(senderId)
                    && string.Equals(senderId, incomingRoom.HostPlayerId, StringComparison.Ordinal);

                if (senderIsAuthoritativeHost)
                {
                    _lastHostSnapshotUtc = now;
                    if (weAreHost)
                        return false;

                    bool sameRoom = _current != null &&
                        string.Equals(_current.LobbyId, incomingRoom.LobbyId, StringComparison.Ordinal);
                    bool adoptStub = !sameRoom && _isDirectJoinStub && _current != null &&
                        !string.IsNullOrWhiteSpace(_current.RelayJoinCode) &&
                        string.Equals(_current.RelayJoinCode.Trim(), incomingRoom.RelayJoinCode,
                            StringComparison.OrdinalIgnoreCase);
                    if (!sameRoom && !adoptStub)
                        return false;

                    var adopted = AdoptHostSnapshot(incomingRoom, localId);
                    adopted = StripBannedPlayers(adopted);
                    if (HaveSameRoomState(_current, adopted))
                        return false;

                    _current = adopted;
                    _isDirectJoinStub = false;
                    return true;
                }

                // Client-authored presence: only the host consumes it, and only
                // the sender's own player record (heartbeats announce self).
                if (!weAreHost || _current == null || string.IsNullOrWhiteSpace(senderId))
                    return false;

                bool sameLobby = string.Equals(_current.LobbyId, incomingRoom.LobbyId, StringComparison.Ordinal);
                bool stubSender = string.Equals(incomingRoom.LobbyId, incomingRoom.RelayJoinCode,
                    StringComparison.OrdinalIgnoreCase);
                bool stubTargetsUs = stubSender &&
                    string.Equals(_current.RelayJoinCode, incomingRoom.RelayJoinCode,
                        StringComparison.OrdinalIgnoreCase);
                if (!sameLobby && !stubTargetsUs)
                    return false;

                if (IsBanned(_current, senderId))
                    return false;

                _presenceLastSeenUtc[senderId] = now;
                return UpsertPresencePlayer(senderId, incomingRoom);
            }
        }

        private LobbyRoom AdoptHostSnapshot(LobbyRoom incomingRoom, string localId)
        {
            var players = new List<LobbyPlayer>(incomingRoom.Players ?? Array.Empty<LobbyPlayer>());
            var selfKnown = false;
            foreach (var player in players)
            {
                if (player != null && string.Equals(player.PlayerId, localId, StringComparison.Ordinal))
                {
                    selfKnown = true;
                    break;
                }
            }

            if (!selfKnown)
            {
                var displayName = _localDisplayName;
                var localTicks = 0L;
                foreach (var player in _current?.Players ?? Array.Empty<LobbyPlayer>())
                {
                    if (player != null && string.Equals(player.PlayerId, localId, StringComparison.Ordinal))
                    {
                        if (!string.IsNullOrWhiteSpace(player.DisplayName))
                            displayName = player.DisplayName;
                        localTicks = player.LocalTimeTicks;
                        break;
                    }
                }

                players.Add(new LobbyPlayer(localId, displayName, isHost: false, localTicks));
            }

            return new LobbyRoom(incomingRoom.LobbyId, incomingRoom.LobbyCode, incomingRoom.Name,
                incomingRoom.MaxPlayers, incomingRoom.IsPrivate, incomingRoom.HostPlayerId,
                incomingRoom.RelayJoinCode, players, incomingRoom.PasswordHash, incomingRoom.State,
                incomingRoom.ReconnectRecords, incomingRoom.StartedWorldSettingsBytes,
                incomingRoom.BannedPlayerIds, incomingRoom.CapabilityFlags, incomingRoom.ConfigFingerprint);
        }

        private bool UpsertPresencePlayer(string senderId, LobbyRoom incomingRoom)
        {
            LobbyPlayer senderRecord = null;
            foreach (var player in incomingRoom.Players ?? Array.Empty<LobbyPlayer>())
            {
                if (player != null && string.Equals(player.PlayerId, senderId, StringComparison.Ordinal))
                {
                    senderRecord = player;
                    break;
                }
            }

            var players = new List<LobbyPlayer>(_current.Players ?? Array.Empty<LobbyPlayer>());
            var index = players.FindIndex(p =>
                p != null && string.Equals(p.PlayerId, senderId, StringComparison.Ordinal));
            if (index < 0)
            {
                players.Add(new LobbyPlayer(senderId,
                    string.IsNullOrWhiteSpace(senderRecord?.DisplayName) ? "Player" : senderRecord.DisplayName,
                    isHost: false, senderRecord?.LocalTimeTicks ?? 0));
            }
            else
            {
                var existing = players[index];
                var name = string.IsNullOrWhiteSpace(senderRecord?.DisplayName)
                    ? existing.DisplayName
                    : senderRecord.DisplayName;
                var ticks = senderRecord?.LocalTimeTicks ?? existing.LocalTimeTicks;
                if (!existing.IsHost
                    && string.Equals(existing.DisplayName, name, StringComparison.Ordinal)
                    && existing.LocalTimeTicks == ticks)
                {
                    return false;
                }

                players[index] = new LobbyPlayer(senderId, name, existing.IsHost, ticks);
            }

            _current = new LobbyRoom(_current.LobbyId, _current.LobbyCode, _current.Name, _current.MaxPlayers,
                _current.IsPrivate, _current.HostPlayerId, _current.RelayJoinCode, players,
                _current.PasswordHash, _current.State, _current.ReconnectRecords,
                _current.StartedWorldSettingsBytes, _current.BannedPlayerIds,
                _current.CapabilityFlags, _current.ConfigFingerprint);
            return true;
        }

        private static bool IsBanned(LobbyRoom room, string playerId)
        {
            var banned = room?.BannedPlayerIds;
            if (banned == null || banned.Count == 0 || string.IsNullOrWhiteSpace(playerId))
                return false;

            foreach (var id in banned)
            {
                if (string.Equals(id, playerId, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private void HandleLeavePayload(string roomId, string playerId)
        {
            byte[] advertiseBytes = null;
            lock (_stateLock)
            {
                if (_current == null || !IsCurrentLocalHost())
                    return;
                if (!string.Equals(_current.LobbyId, roomId, StringComparison.Ordinal))
                    return;

                var players = new List<LobbyPlayer>();
                var removed = false;
                foreach (var player in _current.Players ?? Array.Empty<LobbyPlayer>())
                {
                    if (player != null && string.Equals(player.PlayerId, playerId, StringComparison.Ordinal))
                    {
                        removed = true;
                        continue;
                    }
                    if (player != null)
                        players.Add(player);
                }

                if (!removed)
                    return;

                _presenceLastSeenUtc.Remove(playerId);
                _current = new LobbyRoom(_current.LobbyId, _current.LobbyCode, _current.Name, _current.MaxPlayers,
                    _current.IsPrivate, _current.HostPlayerId, _current.RelayJoinCode, players,
                    _current.PasswordHash, _current.State, _current.ReconnectRecords,
                    _current.StartedWorldSettingsBytes, _current.BannedPlayerIds,
                    _current.CapabilityFlags, _current.ConfigFingerprint);
                RememberDiscoveredRoom(_current);
                advertiseBytes = Encoding.UTF8.GetBytes(BuildPayload());
            }

            RaiseLobbyUpdated(_current);
            if (advertiseBytes != null)
                _ = SendHostDiscoveryPayloadAsync(advertiseBytes);
        }

        // Players that stop announcing themselves (crash, network loss) must
        // not linger forever: presence heartbeats run every BroadcastIntervalMs.
        private void ReapExpiredPresence()
        {
            bool changed = false;
            lock (_stateLock)
            {
                if (_current == null || !IsCurrentLocalHost())
                    return;

                var now = DateTime.UtcNow;
                var localId = BuildLocalHostId();
                var players = new List<LobbyPlayer>();
                foreach (var player in _current.Players ?? Array.Empty<LobbyPlayer>())
                {
                    if (player == null)
                        continue;

                    if (string.Equals(player.PlayerId, localId, StringComparison.Ordinal))
                    {
                        players.Add(player);
                        continue;
                    }

                    if (!_presenceLastSeenUtc.TryGetValue(player.PlayerId, out var lastSeen))
                    {
                        _presenceLastSeenUtc[player.PlayerId] = now;
                        players.Add(player);
                        continue;
                    }

                    if ((now - lastSeen).TotalMilliseconds > PresenceTimeoutMs)
                    {
                        _presenceLastSeenUtc.Remove(player.PlayerId);
                        changed = true;
                        continue;
                    }

                    players.Add(player);
                }

                if (!changed)
                    return;

                _current = new LobbyRoom(_current.LobbyId, _current.LobbyCode, _current.Name, _current.MaxPlayers,
                    _current.IsPrivate, _current.HostPlayerId, _current.RelayJoinCode, players,
                    _current.PasswordHash, _current.State, _current.ReconnectRecords,
                    _current.StartedWorldSettingsBytes, _current.BannedPlayerIds,
                    _current.CapabilityFlags, _current.ConfigFingerprint);
                RememberDiscoveredRoom(_current);
            }

            RaiseLobbyUpdated(_current);
        }

        // When the host disappears without a final advertise (crash/kill), the
        // last remaining signal is silence: close the lobby after the timeout.
        private void DetectHostSnapshotTimeout()
        {
            bool timedOut = false;
            lock (_stateLock)
            {
                if (_current == null || IsCurrentLocalHost())
                    return;
                if (_lastHostSnapshotUtc == DateTime.MinValue)
                    return;
                if ((DateTime.UtcNow - _lastHostSnapshotUtc).TotalMilliseconds <= HostSnapshotTimeoutMs)
                    return;

                _current = null;
                _isDirectJoinStub = false;
                _presenceLastSeenUtc.Clear();
                timedOut = true;
            }

            if (timedOut)
            {
                RaiseKickedFromLobby("lobby_closed");
                PublishState(LobbyState.Closed);
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

        private static bool TryChooseSuccessorHost(LobbyRoom room, string leavingPlayerId, out string successorHostId)
        {
            successorHostId = string.Empty;
            if (room?.Players == null)
                return false;

            var candidates = new List<LobbyPlayer>();
            foreach (var player in room.Players)
            {
                if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
                    continue;
                if (string.Equals(player.PlayerId, leavingPlayerId, StringComparison.Ordinal))
                    continue;
                candidates.Add(player);
            }

            if (candidates.Count == 0)
                return false;

            candidates.Sort((a, b) => string.CompareOrdinal(a.PlayerId, b.PlayerId));
            successorHostId = candidates[0].PlayerId;
            return true;
        }

        private static LobbyRoom RehostRoom(
            LobbyRoom room,
            string hostPlayerId,
            string relayJoinCode,
            string removePlayerId = null)
        {
            var players = new List<LobbyPlayer>();
            foreach (var player in room.Players ?? Array.Empty<LobbyPlayer>())
            {
                if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
                    continue;
                if (!string.IsNullOrWhiteSpace(removePlayerId) &&
                    string.Equals(player.PlayerId, removePlayerId, StringComparison.Ordinal))
                    continue;

                players.Add(new LobbyPlayer(
                    player.PlayerId,
                    player.DisplayName,
                    string.Equals(player.PlayerId, hostPlayerId, StringComparison.Ordinal),
                    player.LocalTimeTicks));
            }

            return new LobbyRoom(room.LobbyId, room.LobbyCode, room.Name, room.MaxPlayers, room.IsPrivate,
                hostPlayerId, relayJoinCode, players, room.PasswordHash, room.State,
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

        // Banned players never re-enter the merged roster: without this the
        // union merge would resurrect a kicked peer from its own broadcasts.
        private static LobbyRoom StripBannedPlayers(LobbyRoom room)
        {
            var banned = room?.BannedPlayerIds;
            if (room?.Players == null || banned == null || banned.Count == 0)
                return room;

            var bannedSet = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in banned)
                if (!string.IsNullOrWhiteSpace(id))
                    bannedSet.Add(id.Trim());
            if (bannedSet.Count == 0)
                return room;

            var players = new List<LobbyPlayer>(room.Players.Count);
            var removed = false;
            foreach (var player in room.Players)
            {
                if (player != null && bannedSet.Contains(player.PlayerId))
                {
                    removed = true;
                    continue;
                }
                players.Add(player);
            }

            if (!removed)
                return room;

            return new LobbyRoom(room.LobbyId, room.LobbyCode, room.Name, room.MaxPlayers, room.IsPrivate,
                room.HostPlayerId, room.RelayJoinCode, players, room.PasswordHash, room.State,
                room.ReconnectRecords, room.StartedWorldSettingsBytes, room.BannedPlayerIds,
                room.CapabilityFlags, room.ConfigFingerprint);
        }

        private bool DetectLocalKick(LobbyRoom incomingRoom)
        {
            if (incomingRoom == null || _current == null || IsCurrentLocalHost())
                return false;

            var banned = incomingRoom.BannedPlayerIds;
            if (banned == null || banned.Count == 0)
                return false;

            var localId = BuildLocalHostId();
            foreach (var id in banned)
            {
                if (string.Equals(id, localId, StringComparison.Ordinal))
                    return true;
            }
            return false;
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

            if (!string.Equals(first.HostPlayerId, second.HostPlayerId, StringComparison.Ordinal))
                return false;

            if (!string.Equals(first.RelayJoinCode, second.RelayJoinCode, StringComparison.Ordinal))
                return false;

            if (!HaveSameBytes(first.StartedWorldSettingsBytes, second.StartedWorldSettingsBytes))
                return false;

            if (first.Players.Count != second.Players.Count)
                return false;

            if (!HaveSameStrings(first.BannedPlayerIds, second.BannedPlayerIds))
                return false;

            var firstPlayers = new Dictionary<string, LobbyPlayer>(StringComparer.Ordinal);
            foreach (var player in first.Players)
            {
                if (player != null && !string.IsNullOrWhiteSpace(player.PlayerId))
                    firstPlayers[player.PlayerId] = player;
            }

            foreach (var player in second.Players)
            {
                if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
                    continue;
                if (!firstPlayers.TryGetValue(player.PlayerId, out var existing)
                    || existing.IsHost != player.IsHost
                    || !string.Equals(existing.DisplayName, player.DisplayName, StringComparison.Ordinal))
                {
                    return false;
                }
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

        private static bool HaveSameStrings(IReadOnlyList<string> first, IReadOnlyList<string> second)
        {
            var firstCount = first?.Count ?? 0;
            var secondCount = second?.Count ?? 0;
            if (firstCount != secondCount)
                return false;

            var set = new HashSet<string>(StringComparer.Ordinal);
            if (first != null)
                foreach (var value in first)
                    set.Add(value);

            if (second != null)
                foreach (var value in second)
                    if (!set.Contains(value))
                        return false;

            return true;
        }

    }
}
