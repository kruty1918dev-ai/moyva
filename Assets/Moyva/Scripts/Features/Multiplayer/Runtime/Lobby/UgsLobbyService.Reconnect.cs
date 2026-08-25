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
        private void LogRuntimeContext(string action)
        {
            var profile = MultiplayerClientScope.IsDefault ? "default" : MultiplayerClientScope.ScopeId;
            var playerId = AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn
                ? AuthenticationService.Instance.PlayerId
                : "<not-signed-in>";
            _logger.Trace($"[UgsLobby] {action} context: profile={profile}, playerId={playerId}, services={UnityServices.State}, signedIn={(AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)}.");
        }

        private async Task PublishReconnectRecordsForRemovedPlayersAsync(LobbyRoom previous, LobbyRoom current, CancellationToken ct)
        {
            if (previous == null || current == null || current.State != LobbyState.Started || _lobby == null)
                return;

            var activeIds = new HashSet<string>(StringComparer.Ordinal);
            var activeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var player in current.Players)
            {
                activeIds.Add(player.PlayerId);
                activeNames.Add(player.DisplayName ?? string.Empty);
            }

            var records = new List<LobbyReconnectRecord>();
            foreach (var record in current.ReconnectRecords)
            {
                if (!activeNames.Contains(record.DisplayName ?? string.Empty))
                    records.Add(record);
            }

            bool changed = records.Count != current.ReconnectRecords.Count;
            long hostTicks = DateTime.UtcNow.Ticks;
            foreach (var player in previous.Players)
            {
                if (player.IsHost || activeIds.Contains(player.PlayerId))
                    continue;

                long playerTicks = player.LocalTimeTicks > 0 ? player.LocalTimeTicks : DateTime.Now.Ticks;
                records.Add(new LobbyReconnectRecord(player.DisplayName, playerTicks, hostTicks));
                changed = true;
            }

            if (!changed)
                return;

            var update = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { ReconnectRecordsDataKey, new DataObject(DataObject.VisibilityOptions.Member, EncodeReconnectRecords(records)) },
                }
            };

            _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, update).ConfigureAwait(false);
            _current = Project(_lobby);
        }

        private async Task UpdateLocalPlayerTimeAsync()
        {
            var lobby = _lobby;
            var lobbyService = LobbyService.Instance;
            var authenticationService = AuthenticationService.Instance;
            if (lobby == null || lobbyService == null || authenticationService == null || string.IsNullOrEmpty(authenticationService.PlayerId))
                return;

            var update = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { LocalTimeTicksDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, DateTime.Now.Ticks.ToString()) },
                }
            };

            await lobbyService.UpdatePlayerAsync(lobby.Id, authenticationService.PlayerId, update).ConfigureAwait(false);
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

        private static string EncodeReconnectRecords(IReadOnlyList<LobbyReconnectRecord> records)
        {
            if (records == null || records.Count == 0)
                return string.Empty;

            var parts = new List<string>(records.Count);
            for (int index = 0; index < records.Count; index++)
            {
                var record = records[index];
                string name = Convert.ToBase64String(Encoding.UTF8.GetBytes(record.DisplayName ?? string.Empty));
                parts.Add($"{name},{record.PlayerLocalTicksAtDisconnect},{record.HostUtcTicksAtDisconnect}");
            }

            return string.Join(";", parts);
        }

        private static IReadOnlyList<LobbyReconnectRecord> DecodeReconnectRecords(string encoded)
        {
            if (string.IsNullOrWhiteSpace(encoded))
                return Array.Empty<LobbyReconnectRecord>();

            var records = new List<LobbyReconnectRecord>();
            var entries = encoded.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int index = 0; index < entries.Length; index++)
            {
                var fields = entries[index].Split(',');
                if (fields.Length != 3 || !long.TryParse(fields[1], out long playerTicks) || !long.TryParse(fields[2], out long hostTicks))
                    continue;

                try
                {
                    string name = Encoding.UTF8.GetString(Convert.FromBase64String(fields[0]));
                    records.Add(new LobbyReconnectRecord(name, playerTicks, hostTicks));
                }
                catch { }
            }

            return records;
        }

        private void CloseLobbyState(string reason)
        {
            StopLoops();
            _lobby = null;
            _current = null;
            _isHost = false;
            KickedFromLobby?.Invoke(reason);
            PublishState(LobbyState.Closed);
        }

        private static bool IsUnauthorized(LobbyServiceException exception)
        {
            if (exception == null)
                return false;

            var message = exception.Message ?? string.Empty;
            return message.Contains("401", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            StopLoops();
            _operationLock.Dispose();
        }
    }
}
