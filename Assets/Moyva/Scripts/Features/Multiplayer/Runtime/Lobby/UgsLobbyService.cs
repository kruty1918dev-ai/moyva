// UgsLobbyService — Unity Gaming Services Lobby backend.
//
// SETUP:
//   1. Install package  com.unity.services.lobbies  via Package Manager.
//   2. Add the scripting define  MOYVA_UGS_LOBBY  in  Player Settings -> Scripting Define Symbols.
//   3. Enable Lobby + Authentication + Relay in the Unity Dashboard.
//
// Without MOYVA_UGS_LOBBY the service compiles as a no-op stub and every call
// returns a graceful failure (null / empty), so higher layers can fall back.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;
#if MOYVA_UGS_LOBBY
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
#endif

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// Real UGS Lobby implementation. Creates/joins lobbies, stores the Relay join code
    /// in lobby data, runs a heartbeat loop for the host and a poll loop for all peers.
    /// </summary>
    public sealed class UgsLobbyService : ILobbyService, IDisposable
    {
        private const string RelayCodeDataKey = "relayJoinCode";
        private const float HeartbeatSeconds = 15f;
        private const float PollSeconds = 1.5f;

        private readonly IMultiplayerLogger _logger;
        private LobbyRoom _current;
        private bool _isHost;
        private CancellationTokenSource _loopCts;

        public LobbyRoom Current => _current;

        public event Action<LobbyRoom> LobbyUpdated;
        public event Action<string> KickedFromLobby;

        public UgsLobbyService(IMultiplayerLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

#if MOYVA_UGS_LOBBY
        private Lobby _lobby;

        public async Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default)
        {
            await EnsureServicesReadyAsync();

            var createOptions = new CreateLobbyOptions
            {
                IsPrivate = options.IsPrivate,
                Player = BuildLocalPlayer(options.DisplayName),
                Data = new Dictionary<string, DataObject>
                {
                    { RelayCodeDataKey, new DataObject(DataObject.VisibilityOptions.Member, string.Empty) },
                }
            };

            _lobby = await LobbyService.Instance.CreateLobbyAsync(options.Name, options.MaxPlayers, createOptions);
            _isHost = true;
            _current = Project(_lobby);
            LobbyUpdated?.Invoke(_current);

            StartLoops();
            _logger.Info($"[UgsLobby] Created '{options.Name}' id={_lobby.Id} code={_lobby.LobbyCode}");
            return _current;
        }

        public async Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default)
        {
            await EnsureServicesReadyAsync();

            var opts = new JoinLobbyByCodeOptions { Player = BuildLocalPlayer(displayName) };
            _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, opts);
            _isHost = false;
            _current = Project(_lobby);
            LobbyUpdated?.Invoke(_current);

            StartLoops();
            _logger.Info($"[UgsLobby] Joined by code '{lobbyCode}' id={_lobby.Id}");
            return _current;
        }

        public async Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default)
        {
            await EnsureServicesReadyAsync();

            var opts = new JoinLobbyByIdOptions { Player = BuildLocalPlayer(displayName) };
            _lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, opts);
            _isHost = false;
            _current = Project(_lobby);
            LobbyUpdated?.Invoke(_current);

            StartLoops();
            _logger.Info($"[UgsLobby] Joined by id '{lobbyId}'");
            return _current;
        }

        public async Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default)
        {
            await EnsureServicesReadyAsync();

            var query = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ),
                }
            };

            var result = await LobbyService.Instance.QueryLobbiesAsync(query);
            var list = new List<LobbyRoom>(result.Results.Count);
            foreach (var l in result.Results)
                list.Add(Project(l));
            return list;
        }

        public async Task LeaveAsync(CancellationToken ct = default)
        {
            StopLoops();
            if (_lobby == null) return;

            try
            {
                if (_isHost)
                    await LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
                else
                    await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (Exception e)
            {
                _logger.Warn($"[UgsLobby] LeaveAsync: {e.Message}");
            }
            finally
            {
                _lobby = null;
                _current = null;
                _isHost = false;
            }
        }

        public async Task KickAsync(string playerId, CancellationToken ct = default)
        {
            if (_lobby == null || !_isHost) return;
            await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, playerId);
        }

        public async Task SetRelayJoinCodeAsync(string relayJoinCode, CancellationToken ct = default)
        {
            if (_lobby == null || !_isHost) return;

            var updateOpts = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { RelayCodeDataKey, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode ?? string.Empty) },
                }
            };

            _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, updateOpts);
            _current = Project(_lobby);
            LobbyUpdated?.Invoke(_current);
        }

        public async Task LockAsync(bool locked, CancellationToken ct = default)
        {
            if (_lobby == null || !_isHost) return;
            var opts = new UpdateLobbyOptions { IsLocked = locked };
            _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, opts);
            _current = Project(_lobby);
            LobbyUpdated?.Invoke(_current);
        }

        // ── Internals ────────────────────────────────────────────────────────

        private static async Task EnsureServicesReadyAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private static Player BuildLocalPlayer(string displayName)
        {
            return new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, displayName ?? "Player") },
                });
        }

        private static LobbyRoom Project(Lobby l)
        {
            string relayCode = string.Empty;
            if (l.Data != null && l.Data.TryGetValue(RelayCodeDataKey, out var dataObj) && dataObj != null)
                relayCode = dataObj.Value ?? string.Empty;

            var players = new List<LobbyPlayer>(l.Players?.Count ?? 0);
            if (l.Players != null)
            {
                foreach (var p in l.Players)
                {
                    string name = p.Id;
                    if (p.Data != null && p.Data.TryGetValue("name", out var nm) && nm != null)
                        name = nm.Value;
                    players.Add(new LobbyPlayer(p.Id, name, isHost: p.Id == l.HostId));
                }
            }

            return new LobbyRoom(l.Id, l.LobbyCode, l.Name, l.MaxPlayers, l.IsPrivate,
                l.HostId, relayCode, players);
        }

        private void StartLoops()
        {
            StopLoops();
            _loopCts = new CancellationTokenSource();
            _ = PollLoop(_loopCts.Token);
            if (_isHost)
                _ = HeartbeatLoop(_loopCts.Token);
        }

        private void StopLoops()
        {
            if (_loopCts != null)
            {
                try { _loopCts.Cancel(); } catch { /* ignore */ }
                _loopCts.Dispose();
                _loopCts = null;
            }
        }

        private async Task HeartbeatLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _lobby != null && _isHost)
            {
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(_lobby.Id);
                }
                catch (Exception e)
                {
                    _logger.Warn($"[UgsLobby] Heartbeat failed: {e.Message}");
                }

                try { await Task.Delay(TimeSpan.FromSeconds(HeartbeatSeconds), ct); }
                catch (OperationCanceledException) { return; }
            }
        }

        private async Task PollLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _lobby != null)
            {
                try
                {
                    var refreshed = await LobbyService.Instance.GetLobbyAsync(_lobby.Id);
                    if (refreshed != null)
                    {
                        _lobby = refreshed;
                        _current = Project(_lobby);
                        LobbyUpdated?.Invoke(_current);

                        bool stillIn = false;
                        string myId = AuthenticationService.Instance.PlayerId;
                        foreach (var p in _current.Players)
                        {
                            if (p.PlayerId == myId) { stillIn = true; break; }
                        }
                        if (!stillIn)
                        {
                            _logger.Warn("[UgsLobby] Kicked / removed from lobby.");
                            KickedFromLobby?.Invoke("removed");
                            StopLoops();
                            _lobby = null;
                            _current = null;
                            return;
                        }
                    }
                }
                catch (LobbyServiceException e) when ((int)e.Reason == (int)LobbyExceptionReason.LobbyNotFound)
                {
                    _logger.Warn("[UgsLobby] Lobby no longer exists.");
                    KickedFromLobby?.Invoke("lobby_closed");
                    StopLoops();
                    _lobby = null;
                    _current = null;
                    return;
                }
                catch (Exception e)
                {
                    _logger.Warn($"[UgsLobby] Poll failed: {e.Message}");
                }

                try { await Task.Delay(TimeSpan.FromSeconds(PollSeconds), ct); }
                catch (OperationCanceledException) { return; }
            }
        }

        public void Dispose() => StopLoops();
#else
        // ── Stub: compiles without the UGS Lobby package ─────────────────────

        public Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default)
        {
            _logger.Warn("[UgsLobby] com.unity.services.lobbies not installed. Enable MOYVA_UGS_LOBBY.");
            return Task.FromResult<LobbyRoom>(null);
        }

        public Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default)
            => Task.FromResult<LobbyRoom>(null);

        public Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default)
            => Task.FromResult<LobbyRoom>(null);

        public Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default)
        {
            IReadOnlyList<LobbyRoom> empty = Array.Empty<LobbyRoom>();
            return Task.FromResult(empty);
        }

        public Task LeaveAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task KickAsync(string playerId, CancellationToken ct = default) => Task.CompletedTask;
        public Task SetRelayJoinCodeAsync(string relayJoinCode, CancellationToken ct = default) => Task.CompletedTask;
        public Task LockAsync(bool locked, CancellationToken ct = default) => Task.CompletedTask;

        public void Dispose() { }
#endif
    }
}
