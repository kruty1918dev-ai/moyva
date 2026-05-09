// UgsLobbyService — Unity Gaming Services Lobby backend.
//
// SETUP:
//   1. Install package com.unity.services.multiplayer via Package Manager (contains Lobbies).
//   2. Enable Lobby + Authentication + Relay in the Unity Dashboard.
//
// Note: if the Lobbies package is not installed the service may behave as a no-op.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// Real UGS Lobby implementation. Creates/joins lobbies, stores the Relay join code
    /// in lobby data, runs a heartbeat loop for the host and a poll loop for all peers.
    /// </summary>
    public sealed class UgsLobbyService : ILobbyService, IDisposable
    {
        private const string RelayCodeDataKey = "relayJoinCode";
        private const string ProjectDataKey = "moyvaProject";
        private const string ProjectDataValue = "moyva";
        private const string ProviderDataKey = "moyvaProvider";
        private const string ProviderDataValue = "relay";
        private const string PasswordHashDataKey = "moyvaPasswordHash";
        private const string StateDataKey = "moyvaState";
        private const string WorldSettingsDataKey = "moyvaWorldSettings";
        private const string ReconnectRecordsDataKey = "moyvaReconnectRecords";
        private const string LocalTimeTicksDataKey = "localTimeTicks";
        private const float HeartbeatSeconds = 15f;
        private const float PollSeconds = 5f;
        private const float PollBackoffSeconds = 20f;
        private const float JoinRequestTimeoutSeconds = 20f;

        private readonly IMultiplayerLogger _logger;
        private readonly SemaphoreSlim _operationLock = new SemaphoreSlim(1, 1);
        private LobbyRoom _current;
        private LobbyState _state = LobbyState.Closed;
        private bool _isHost;
        private CancellationTokenSource _loopCts;

        private static readonly object ServicesReadyLock = new object();
        private static Task _servicesReadyTask;
        private static Task _signInTask;

        public LobbyRoom Current => _current;
        public LobbyState State => _state;

        public event Action<LobbyRoom> LobbyUpdated;
        public event Action<string> KickedFromLobby;
        public event Action<LobbyState> StateChanged;

        public UgsLobbyService(IMultiplayerLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private Lobby _lobby;

        public async Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default)
        {
            await EnsureServicesReadyAsync();

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            await _operationLock.WaitAsync(ct);
            try
            {
                if (_lobby != null && _current != null)
                {
                    _logger.Warn($"[UgsLobby] CreateRoomAsync ignored because local player is already in lobby '{_current.Name}' id={_current.LobbyId}.");
                    return _current;
                }

                var createOptions = new CreateLobbyOptions
                {
                    IsPrivate = options.IsPrivate,
                    Player = BuildLocalPlayer(options.DisplayName),
                    Data = new Dictionary<string, DataObject>
                    {
                        { RelayCodeDataKey, new DataObject(DataObject.VisibilityOptions.Member, string.Empty) },
                        { ProjectDataKey, new DataObject(DataObject.VisibilityOptions.Public, ProjectDataValue, DataObject.IndexOptions.S1) },
                        { ProviderDataKey, new DataObject(DataObject.VisibilityOptions.Public, ProviderDataValue, DataObject.IndexOptions.S2) },
                        { PasswordHashDataKey, new DataObject(DataObject.VisibilityOptions.Public, LobbyPasswordHasher.Hash(options.Password)) },
                        { StateDataKey, new DataObject(DataObject.VisibilityOptions.Public, LobbyState.Open.ToString()) },
                        { WorldSettingsDataKey, new DataObject(DataObject.VisibilityOptions.Member, string.Empty) },
                        { ReconnectRecordsDataKey, new DataObject(DataObject.VisibilityOptions.Member, string.Empty) },
                    }
                };

                if (LobbyService.Instance == null)
                    throw new InvalidOperationException("[UgsLobby] Unity Lobbies LobbyService instance is unavailable; make sure the Unity Services Lobbies package is present and initialized.");

                try
                {
                    _lobby = await LobbyService.Instance.CreateLobbyAsync(options.Name, options.MaxPlayers, createOptions);
                }
                catch (Exception e)
                {
                    _logger.Warn($"[UgsLobby] CreateLobbyAsync failed: {e}");
                    if (e is NullReferenceException)
                        _logger.Warn("[UgsLobby] CreateLobbyAsync null reference during UGS lobby creation.");
                    throw;
                }
                if (_lobby == null)
                {
                    _logger.Warn("[UgsLobby] CreateLobbyAsync returned null.");
                    return null;
                }

                _isHost = true;
                _current = Project(_lobby);
                LobbyUpdated?.Invoke(_current);
                PublishState(_current.State);

                StartLoops();
                _logger.Info($"[UgsLobby] Created '{options.Name}' id={_lobby.Id} code={_lobby.LobbyCode}");
                return _current;
            }
            finally
            {
                _operationLock.Release();
            }
        }

        public async Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default)
        {
            await EnsureServicesReadyAsync();

            await _operationLock.WaitAsync(ct);
            try
            {
                if (_current != null && string.Equals(_current.LobbyCode, lobbyCode, StringComparison.OrdinalIgnoreCase))
                    return _current;

                var opts = new JoinLobbyByCodeOptions { Player = BuildLocalPlayer(displayName) };
                if (LobbyService.Instance == null)
                    throw new InvalidOperationException("[UgsLobby] Unity Lobbies LobbyService instance is unavailable; make sure the Unity Services Lobbies package is present and initialized.");

                try
                {
                    _lobby = await AwaitWithTimeoutAsync(
                        LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, opts),
                        TimeSpan.FromSeconds(JoinRequestTimeoutSeconds),
                        ct,
                        $"JoinLobbyByCodeAsync('{lobbyCode}')");
                }
                catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.Conflict || e.Reason == LobbyExceptionReason.LobbyConflict)
                {
                    _logger.Warn($"[UgsLobby] JoinByCodeAsync: already member of lobby '{lobbyCode}'; returning current lobby.");
                    if (_current != null)
                        return _current;

                    if (_lobby != null)
                    {
                        _current = Project(_lobby);
                        return _current;
                    }

                    throw;
                }

                if (_lobby == null)
                {
                    _logger.Warn("[UgsLobby] JoinLobbyByCodeAsync returned null.");
                    return null;
                }

                _isHost = false;
                _current = Project(_lobby);
                LobbyUpdated?.Invoke(_current);
                PublishState(_current.State);

                StartLoops();
                _logger.Info($"[UgsLobby] Joined by code '{lobbyCode}' id={_lobby.Id}");
                return _current;
            }
            finally
            {
                _operationLock.Release();
            }
        }

        public async Task<LobbyRoom> JoinByCodeWithPasswordAsync(string lobbyCode, string displayName, string password, CancellationToken ct = default)
        {
            // Спершу приєднуємось, потім звіряємо хеш паролю з даних кімнати.
            // У разі невідповідності — виходимо з лобі та кидаємо WrongPasswordException.
            var room = await JoinByCodeAsync(lobbyCode, displayName, ct).ConfigureAwait(false);
            if (room == null)
                return null;

            if (room.HasPassword && !LobbyPasswordHasher.Verify(password, room.PasswordHash))
            {
                _logger.Warn($"[UgsLobby] JoinByCodeWithPasswordAsync: невірний пароль для '{lobbyCode}'.");
                try { await LeaveAsync(ct).ConfigureAwait(false); } catch { }
                throw new WrongPasswordException();
            }

            return room;
        }

        public async Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default)
        {
            await EnsureServicesReadyAsync();

            await _operationLock.WaitAsync(ct);
            try
            {
                if (_current != null && string.Equals(_current.LobbyId, lobbyId, StringComparison.Ordinal))
                {
                    UnityEngine.Debug.Log($"[UgsLobby] JoinByIdAsync: early return — already in lobby '{lobbyId}'.");
                    return _current;
                }

                var opts = new JoinLobbyByIdOptions { Player = BuildLocalPlayer(displayName) };
                if (LobbyService.Instance == null)
                    throw new InvalidOperationException("[UgsLobby] Unity Lobbies LobbyService instance is unavailable; make sure the Unity Services Lobbies package is present and initialized.");

                UnityEngine.Debug.Log($"[UgsLobby] JoinByIdAsync: calling LobbyService.Instance.JoinLobbyByIdAsync('{lobbyId}')...");
                _lobby = await AwaitWithTimeoutAsync(
                    LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, opts),
                    TimeSpan.FromSeconds(JoinRequestTimeoutSeconds),
                    ct,
                    $"JoinLobbyByIdAsync('{lobbyId}')");
                if (_lobby == null)
                {
                    UnityEngine.Debug.LogWarning($"[UgsLobby] JoinByIdAsync: LobbyService.Instance.JoinLobbyByIdAsync('{lobbyId}') returned null.");
                    _logger.Warn("[UgsLobby] JoinLobbyByIdAsync returned null.");
                    return null;
                }

                UnityEngine.Debug.Log($"[UgsLobby] JoinByIdAsync: joined lobby '{lobbyId}' successfully (players={_lobby.Players?.Count ?? 0}).");
                _isHost = false;
                _current = Project(_lobby);
                LobbyUpdated?.Invoke(_current);
                PublishState(_current.State);

                StartLoops();
                _logger.Info($"[UgsLobby] Joined by id '{lobbyId}'");
                return _current;
            }
            finally
            {
                _operationLock.Release();
            }
        }

        private static async Task<T> AwaitWithTimeoutAsync<T>(Task<T> operation, TimeSpan timeout, CancellationToken ct, string operationName)
        {
            var timeoutTask = Task.Delay(timeout, ct);
            var completed = await Task.WhenAny(operation, timeoutTask).ConfigureAwait(false);
            if (completed == operation)
                return await operation.ConfigureAwait(false);

            ct.ThrowIfCancellationRequested();
            throw new TimeoutException($"[UgsLobby] {operationName} timed out after {timeout.TotalSeconds:0.#}s.");
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
                _logger.Warn("[UgsLobby] QueryRoomsAsync rate limited, returning empty room list.");
                return Array.Empty<LobbyRoom>();
            }
            catch (Exception e)
            {
                _logger.Warn($"[UgsLobby] QueryRoomsAsync failed: {e.Message}");
                return Array.Empty<LobbyRoom>();
            }
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
                PublishState(LobbyState.Closed);
            }
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

        private static async Task EnsureServicesReadyAsync()
        {
            if (_servicesReadyTask == null || !_servicesReadyTask.IsCompletedSuccessfully)
            {
                lock (ServicesReadyLock)
                {
                    if (_servicesReadyTask == null || !_servicesReadyTask.IsCompletedSuccessfully)
                    {
                        _servicesReadyTask = InitializeServicesAsync();
                    }
                }
            }

            await _servicesReadyTask;

            if (AuthenticationService.Instance == null)
                throw new InvalidOperationException("[UgsLobby] AuthenticationService instance is unavailable after Unity Services initialization.");

            MultiplayerClientScope.ApplyAuthenticationProfileIfNeeded();

            if (AuthenticationService.Instance.IsSignedIn)
                return;

            if (_signInTask == null || !_signInTask.IsCompletedSuccessfully)
            {
                lock (ServicesReadyLock)
                {
                    if (_signInTask == null || !_signInTask.IsCompletedSuccessfully)
                    {
                        _signInTask = SignInAnonymouslyOnceAsync();
                    }
                }
            }

            await _signInTask;

            if (!AuthenticationService.Instance.IsSignedIn || string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId))
            {
                throw new InvalidOperationException("[UgsLobby] Authentication failed after sign-in; PlayerId is unavailable.");
            }
        }

        private static async Task InitializeServicesAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return;

            await UnityServices.InitializeAsync();
        }

        private static async Task SignInAnonymouslyOnceAsync()
        {
            if (AuthenticationService.Instance == null)
                throw new InvalidOperationException("[UgsLobby] AuthenticationService instance is unavailable during sign-in.");

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException e) when (e.ErrorCode == AuthenticationErrorCodes.ClientInvalidUserState)
            {
                if (AuthenticationService.Instance.IsSignedIn)
                    return;

                await WaitForSignInCompletionAsync();

                if (AuthenticationService.Instance.IsSignedIn)
                    return;

                throw;
            }
        }

        private static async Task WaitForSignInCompletionAsync()
        {
            const int pollingDelayMs = 100;
            const int timeoutMs = 5000;
            var waitedMs = 0;

            while (waitedMs < timeoutMs)
            {
                if (AuthenticationService.Instance.IsSignedIn)
                    return;

                await Task.Delay(pollingDelayMs);
                waitedMs += pollingDelayMs;
            }

            if (!AuthenticationService.Instance.IsSignedIn || string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId))
            {
                throw new InvalidOperationException("[UgsLobby] Authentication failed: user is not signed in or PlayerId is unavailable.");
            }
        }

        private static Player BuildLocalPlayer(string displayName)
        {
            if (AuthenticationService.Instance == null)
                throw new InvalidOperationException("[UgsLobby] AuthenticationService instance is unavailable.");

            if (!AuthenticationService.Instance.IsSignedIn || string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId))
                throw new InvalidOperationException("[UgsLobby] Cannot build local player: authentication is not completed or PlayerId is missing.");

            return new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, displayName ?? "Player") },
                    { LocalTimeTicksDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, DateTime.Now.Ticks.ToString()) },
                });
        }

        private static LobbyRoom Project(Lobby l)
        {
            if (l == null)
                return new LobbyRoom(string.Empty, string.Empty, string.Empty, 0, false, string.Empty, string.Empty, new List<LobbyPlayer>());

            string relayCode = string.Empty;
            if (l.Data != null && l.Data.TryGetValue(RelayCodeDataKey, out var dataObj) && dataObj != null)
                relayCode = dataObj.Value ?? string.Empty;

            string passwordHash = string.Empty;
            if (l.Data != null && l.Data.TryGetValue(PasswordHashDataKey, out var pwdObj) && pwdObj != null)
                passwordHash = pwdObj.Value ?? string.Empty;

            byte[] worldSettingsBytes = Array.Empty<byte>();
            if (l.Data != null && l.Data.TryGetValue(WorldSettingsDataKey, out var worldObj) && worldObj != null)
                worldSettingsBytes = DecodeBytes(worldObj.Value);

            IReadOnlyList<LobbyReconnectRecord> reconnectRecords = Array.Empty<LobbyReconnectRecord>();
            if (l.Data != null && l.Data.TryGetValue(ReconnectRecordsDataKey, out var reconnectObj) && reconnectObj != null)
                reconnectRecords = DecodeReconnectRecords(reconnectObj.Value);

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
                l.HostId, relayCode, players, passwordHash, state, reconnectRecords, worldSettingsBytes);
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
                catch (LobbyServiceException e) when (IsUnauthorized(e))
                {
                    _logger.Warn("[UgsLobby] Heartbeat unauthorized. Closing local lobby state.");
                    CloseLobbyState("unauthorized");
                    return;
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
                    await UpdateLocalPlayerTimeAsync();
                    var refreshed = await LobbyService.Instance.GetLobbyAsync(_lobby.Id);
                    if (refreshed != null)
                    {
                        var previous = _current;
                        _lobby = refreshed;
                        _current = Project(_lobby);
                        if (_isHost)
                            await PublishReconnectRecordsForRemovedPlayersAsync(previous, _current, ct);

                        LobbyUpdated?.Invoke(_current);
                        PublishState(_current.State);

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
                            PublishState(LobbyState.Closed);
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
                    PublishState(LobbyState.Closed);
                    return;
                }
                catch (LobbyServiceException e) when (IsUnauthorized(e))
                {
                    _logger.Warn("[UgsLobby] Poll unauthorized. Closing local lobby state.");
                    CloseLobbyState("unauthorized");
                    return;
                }
                catch (Exception e)
                {
                    _logger.Warn($"[UgsLobby] Poll failed: {e.Message}");
                    var delaySeconds = e.Message.Contains("Too Many Requests") ? PollBackoffSeconds : PollSeconds;
                    try { await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct); }
                    catch (OperationCanceledException) { return; }
                    continue;
                }

                try { await Task.Delay(TimeSpan.FromSeconds(PollSeconds), ct); }
                catch (OperationCanceledException) { return; }
            }
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
            if (_lobby == null || LobbyService.Instance == null || AuthenticationService.Instance == null || string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId))
                return;

            var update = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { LocalTimeTicksDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, DateTime.Now.Ticks.ToString()) },
                }
            };

            await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, AuthenticationService.Instance.PlayerId, update).ConfigureAwait(false);
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
