using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    public sealed partial class UgsLobbyService
    {
        private static LobbyRoom Project(Lobby l)
        {
            if (l == null)
                return new LobbyRoom(string.Empty, string.Empty, string.Empty, 0, false, string.Empty, string.Empty, new List<LobbyPlayer>());

            string relayCode = string.Empty;
            if (l.Data != null && l.Data.TryGetValue(RelayCodeDataKey, out var dataObj) && dataObj != null)
            {
                relayCode = dataObj.Value ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(relayCode) && !RelayJoinCodeUtility.IsValid(relayCode))
                    relayCode = string.Empty;
            }

            string passwordHash = string.Empty;
            if (l.Data != null && l.Data.TryGetValue(PasswordHashDataKey, out var pwdObj) && pwdObj != null)
                passwordHash = pwdObj.Value ?? string.Empty;

            byte[] worldSettingsBytes = Array.Empty<byte>();
            if (l.Data != null && l.Data.TryGetValue(WorldSettingsDataKey, out var worldObj) && worldObj != null)
                worldSettingsBytes = DecodeBytes(worldObj.Value);

            IReadOnlyList<LobbyReconnectRecord> reconnectRecords = Array.Empty<LobbyReconnectRecord>();
            if (l.Data != null && l.Data.TryGetValue(ReconnectRecordsDataKey, out var reconnectObj) && reconnectObj != null)
                reconnectRecords = DecodeReconnectRecords(reconnectObj.Value);

            string configFingerprint = string.Empty;
            if (l.Data != null && l.Data.TryGetValue(ConfigFingerprintDataKey, out var fingerprintObj) && fingerprintObj != null)
                configFingerprint = fingerprintObj.Value ?? string.Empty;

            var players = new List<LobbyPlayer>(l.Players?.Count ?? 0);
            if (l.Players != null)
            {
                foreach (var p in l.Players)
                {
                    string name = p.Id;
                    if (p.Data != null && p.Data.TryGetValue("name", out var nm) && nm != null)
                        name = nm.Value;
                    long localTicks = 0;
                    if (p.Data != null && p.Data.TryGetValue(LocalTimeTicksDataKey, out var ticksObj) && ticksObj != null)
                        long.TryParse(ticksObj.Value, out localTicks);
                    players.Add(new LobbyPlayer(p.Id, name, isHost: p.Id == l.HostId, localTicks));
                }
            }

            var state = ResolveLobbyState(l);
            return new LobbyRoom(l.Id, l.LobbyCode, l.Name, l.MaxPlayers, l.IsPrivate,
                l.HostId, relayCode, players, passwordHash, state, reconnectRecords,
                worldSettingsBytes, configFingerprint: configFingerprint);
        }

        private static LobbyState ResolveLobbyState(Lobby lobby)
        {
            if (lobby == null)
                return LobbyState.Closed;

            if (lobby.Data != null && lobby.Data.TryGetValue(StateDataKey, out var stateObj) && stateObj != null &&
                Enum.TryParse(stateObj.Value, out LobbyState state))
            {
                return state;
            }

            return lobby.IsLocked ? LobbyState.Started : LobbyState.Open;
        }

        private void PublishState(LobbyState state)
        {
            if (_state == state) return;
            _state = state;
            StateChanged?.Invoke(state);
        }

        private static bool IsMoyvaRelayLobby(Lobby lobby)
        {
            if (lobby?.Data == null)
                return false;

            return HasDataValue(lobby, ProjectDataKey, ProjectDataValue) &&
                   HasDataValue(lobby, ProviderDataKey, ProviderDataValue);
        }

        private static bool HasDataValue(Lobby lobby, string key, string expectedValue)
        {
            return lobby.Data.TryGetValue(key, out var dataObject) &&
                   dataObject != null &&
                   string.Equals(dataObject.Value, expectedValue, StringComparison.Ordinal);
        }

    }
}
