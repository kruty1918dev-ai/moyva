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
        public async Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default)
        {
            await EnsureServicesReadyAsync();
            LogRuntimeContext("QueryRoomsAsync");

            if (LobbyService.Instance == null)
            {
                return Array.Empty<LobbyRoom>();
            }

            var query = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ),
                    new QueryFilter(QueryFilter.FieldOptions.S1, ProjectDataValue, QueryFilter.OpOptions.EQ),
                    new QueryFilter(QueryFilter.FieldOptions.S2, ProviderDataValue, QueryFilter.OpOptions.EQ),
                }
            };

            try
            {
                var result = await LobbyService.Instance.QueryLobbiesAsync(query);
                var list = new List<LobbyRoom>(result.Results.Count);
                foreach (var l in result.Results)
                {
                    if (!IsMoyvaRelayLobby(l))
                        continue;

                    list.Add(Project(l));
                }

                return list;
            }
            catch (LobbyServiceException e) when (e.Message != null && e.Message.Contains("Too Many Requests"))
            {
                return Array.Empty<LobbyRoom>();
            }
            catch (Exception)
            {
                return Array.Empty<LobbyRoom>();
            }
        }

        public async Task LeaveAsync(CancellationToken ct = default)
        {
            StopLoops();
            if (_lobby == null) return;
            bool left = false;

            try
            {
                string localPlayerId = AuthenticationService.Instance.PlayerId;
                if (_isHost && TryChooseSuccessorHost(_lobby, localPlayerId, out var successorHostId))
                {
                    await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, new UpdateLobbyOptions
                    {
                        HostId = successorHostId,
                        Data = new Dictionary<string, DataObject>
                        {
                            { RelayCodeDataKey, new DataObject(DataObject.VisibilityOptions.Member, string.Empty) },
                        }
                    });
                    await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, localPlayerId);
                }
                else if (_isHost)
                {
                    await LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
                }
                else
                {
                    await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, localPlayerId);
                }
                left = true;
            }
            catch
            {
                StartLoops();
                throw;
            }
            finally
            {
                if (left)
                {
                    _lobby = null;
                    _current = null;
                    _isHost = false;
                    PublishState(LobbyState.Closed);
                }
            }
        }

        public async Task<bool> TryTransferHostAsync(string newHostPlayerId, string relayJoinCode, CancellationToken ct = default)
        {
            if (_lobby == null || string.IsNullOrWhiteSpace(newHostPlayerId))
                return false;

            var update = new UpdateLobbyOptions
            {
                HostId = newHostPlayerId.Trim(),
                Data = new Dictionary<string, DataObject>
                {
                    { RelayCodeDataKey, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode?.Trim() ?? string.Empty) },
                }
            };

            _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, update);
            _current = Project(_lobby);
            _isHost = string.Equals(_current.HostPlayerId, AuthenticationService.Instance.PlayerId, StringComparison.Ordinal);
            LobbyUpdated?.Invoke(_current);
            PublishState(_current.State);
            if (_isHost)
                StartLoops();
            return true;
        }

        private static bool TryChooseSuccessorHost(Lobby lobby, string leavingPlayerId, out string successorHostId)
        {
            successorHostId = string.Empty;
            if (lobby?.Players == null)
                return false;

            var candidates = new List<Player>();
            foreach (var player in lobby.Players)
            {
                if (player == null || string.IsNullOrWhiteSpace(player.Id))
                    continue;
                if (string.Equals(player.Id, leavingPlayerId, StringComparison.Ordinal))
                    continue;
                candidates.Add(player);
            }

            if (candidates.Count == 0)
                return false;

            candidates.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            successorHostId = candidates[0].Id;
            return true;
        }

        public async Task KickAsync(string playerId, CancellationToken ct = default)
        {
            if (_lobby == null || !_isHost || string.IsNullOrWhiteSpace(playerId)) return;

            await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, playerId);
            ct.ThrowIfCancellationRequested();

            _lobby = await LobbyService.Instance.GetLobbyAsync(_lobby.Id);
            _current = Project(_lobby);
            LobbyUpdated?.Invoke(_current);
        }

        public async Task SetRelayJoinCodeAsync(string relayJoinCode, CancellationToken ct = default)
        {
            if (_lobby == null || !_isHost) return;

            var normalizedRelayJoinCode = relayJoinCode?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(normalizedRelayJoinCode) && !RelayJoinCodeUtility.IsValid(normalizedRelayJoinCode))
            {
                var message = $"[UgsLobby] Refusing to publish invalid Relay join code '{normalizedRelayJoinCode}' for lobby '{_lobby.Id}'.";
                throw new ArgumentException(message, nameof(relayJoinCode));
            }

            var updateOpts = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { RelayCodeDataKey, new DataObject(DataObject.VisibilityOptions.Member, normalizedRelayJoinCode) },
                }
            };

            _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, updateOpts);
            _current = Project(_lobby);
            LobbyUpdated?.Invoke(_current);
            PublishState(_current.State);
        }

        public async Task LockAsync(bool locked, byte[] startedWorldSettingsBytes = null, CancellationToken ct = default)
        {
            if (_lobby == null || !_isHost) return;
            var state = locked ? LobbyState.Started : LobbyState.Open;
            var data = new Dictionary<string, DataObject>
            {
                { StateDataKey, new DataObject(DataObject.VisibilityOptions.Public, state.ToString()) },
                { WorldSettingsDataKey, new DataObject(DataObject.VisibilityOptions.Member, locked ? EncodeBytes(startedWorldSettingsBytes) : string.Empty) },
            };

            var opts = new UpdateLobbyOptions
            {
                IsLocked = false,
                Data = data,
            };
            _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, opts);
            _current = Project(_lobby);
            LobbyUpdated?.Invoke(_current);
            PublishState(state);
        }

        // ── Internals ────────────────────────────────────────────────────────

    }
}
