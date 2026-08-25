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
        public async Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default)
        {
            await EnsureServicesReadyAsync();
            LogRuntimeContext("CreateRoomAsync");

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var initialRelayJoinCode = options.RelayJoinCode?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(initialRelayJoinCode) && !RelayJoinCodeUtility.IsValid(initialRelayJoinCode))
                throw new ArgumentException($"Invalid Relay join code '{initialRelayJoinCode}'.", nameof(options));

            await _operationLock.WaitAsync(ct);
            try
            {
                if (_lobby != null && _current != null)
                {
                    return _current;
                }

                var createOptions = new CreateLobbyOptions
                {
                    IsPrivate = options.IsPrivate,
                    Player = BuildLocalPlayer(options.DisplayName),
                    Data = new Dictionary<string, DataObject>
                    {
                        { RelayCodeDataKey, new DataObject(DataObject.VisibilityOptions.Member, initialRelayJoinCode) },
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
                catch (Exception)
                {
                    throw;
                }
                if (_lobby == null)
                {
                    return null;
                }

                _isHost = true;
                _current = Project(_lobby);
                LobbyUpdated?.Invoke(_current);
                PublishState(_current.State);

                StartLoops();
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
            LogRuntimeContext("JoinByCodeAsync");
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
                    return null;
                }

                _isHost = false;
                _current = Project(_lobby);
                LobbyUpdated?.Invoke(_current);
                PublishState(_current.State);

                StartLoops();
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
                try { await LeaveAsync(ct).ConfigureAwait(false); } catch { }
                throw new WrongPasswordException();
            }

            return room;
        }

        public async Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default)
        {
            await EnsureServicesReadyAsync();

            await _operationLock.WaitAsync(ct);
            LogRuntimeContext("JoinByIdAsync");
            try
            {
                if (_current != null && string.Equals(_current.LobbyId, lobbyId, StringComparison.Ordinal))
                {
                    return _current;
                }

                var opts = new JoinLobbyByIdOptions { Player = BuildLocalPlayer(displayName) };
                if (LobbyService.Instance == null)
                    throw new InvalidOperationException("[UgsLobby] Unity Lobbies LobbyService instance is unavailable; make sure the Unity Services Lobbies package is present and initialized.");
                _lobby = await AwaitWithTimeoutAsync(
                    LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, opts),
                    TimeSpan.FromSeconds(JoinRequestTimeoutSeconds),
                    ct,
                    $"JoinLobbyByIdAsync('{lobbyId}')");
                if (_lobby == null)
                {
                    return null;
                }
                _isHost = false;
                _current = Project(_lobby);
                LobbyUpdated?.Invoke(_current);
                PublishState(_current.State);

                StartLoops();
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

    }
}
