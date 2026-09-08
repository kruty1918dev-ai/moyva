using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Слухає команду <see cref="GameCommandType.StartGame"/> і ініціює локальний старт гри:
    /// заповнює <see cref="IGameplaySession"/> зі значень DTO + поточного лобі та викликає <see cref="IHomeMenuGameStarter"/>.
    /// Приймає старт лише від хоста поточного лобі та запускає локальний клієнт.
    /// </summary>
    internal sealed class GameStartListenerService : IInitializable, IDisposable
    {
        private const string Prefix = "[GameStartListener]";

        [Inject] private IGameplaySession _session = default;
        [InjectOptional] private IGameCommandSyncService _commandSync = default;
        [InjectOptional] private ILobbyService _lobbyService = default;
        [InjectOptional] private ISessionManager _sessionManager = default;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector = default;
        [InjectOptional] private IHomeMenuGameStarter _gameStarter = default;
        [InjectOptional] private IInfoPanelService _infoPanel = default;
        private CancellationTokenSource _lifecycleCts;
        private bool _startRequested;

        public void Initialize()
        {
            _lifecycleCts?.Cancel();
            _lifecycleCts?.Dispose();
            _lifecycleCts = new CancellationTokenSource();
            _startRequested = false;

            if (_commandSync == null) return;
            _commandSync.RegisterHandler(GameCommandType.StartGame, OnStartGameCommand);
        }

        public void Dispose()
        {
            _commandSync?.RegisterHandler(GameCommandType.StartGame, null);
            _lifecycleCts?.Cancel();
            _lifecycleCts?.Dispose();
            _lifecycleCts = null;
        }

        private void OnStartGameCommand(string senderId, byte[] payload)
        {
            if (_lifecycleCts == null || _lifecycleCts.IsCancellationRequested
                || _startRequested || _gameStarter == null
                || !TryAuthorizeHostStart(senderId, out string localId)
                || !WorldSettingsDto.TryFromBytes(payload, out var dto))
            {
                return;
            }

            try
            {
                _startRequested = true;
                var mode = _modeSelector?.CurrentMode ?? NetworkProviderType.Offline;
                var players = MultiplayerRoomLifecycle.ProjectGameplayPlayers(
                    _lobbyService?.Current,
                    localId,
                    localPlayerIsHost: false);

                _session.Apply(mode, dto, players, localId);
                GameLaunchContext.ConfigureMenuMultiplayerGame(
                    dto.WorldName,
                    dto.Seed,
                    dto.Size,
                    (int)dto.MapType,
                    (int)dto.Difficulty,
                    dto.MaxPlayers,
                    dto.IsPrivate,
                    dto.Width,
                    dto.Height,
                    isLocalPlayerHost: false,
                    localPlayerId: localId);

                var ct = _lifecycleCts.Token;
                MainThreadDispatcher.Enqueue(() =>
                {
                    if (ct.IsCancellationRequested)
                        return;
                    _ = StartClientGameAsync(ct);
                });
            }
            catch (Exception e)
            {
                _startRequested = false;
                Debug.LogError($"{Prefix} OnStartGameCommand error: {e}");
                _infoPanel?.Show(new InfoMessage("Start Failed", e.Message));
            }
        }

        private bool TryAuthorizeHostStart(string senderId, out string localId)
        {
            localId = (_lobbyService as ILobbyLocalIdentity)?.LocalPlayerId;
            if (string.IsNullOrWhiteSpace(localId))
                localId = _sessionManager?.LocalPlayerId;
            if (string.IsNullOrWhiteSpace(localId))
                localId = _session?.LocalPlayer.PlayerId;

            if (string.IsNullOrWhiteSpace(senderId)
                || string.IsNullOrWhiteSpace(localId)
                || string.Equals(senderId, localId, StringComparison.Ordinal)
                || _sessionManager?.IsLocalPlayerHost == true
                || _session?.IsHost == true)
            {
                return false;
            }

            var lobby = _lobbyService?.Current;
            if (lobby != null)
            {
                return lobby.State != LobbyState.Closed
                    && string.Equals(senderId, lobby.HostPlayerId, StringComparison.Ordinal)
                    && !string.Equals(localId, lobby.HostPlayerId, StringComparison.Ordinal);
            }

            var participants = _sessionManager?.Participants;
            if (participants != null)
            {
                foreach (var participant in participants)
                {
                    if (participant?.IsHost == true
                        && string.Equals(senderId, participant.Identity?.PlayerId, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task StartClientGameAsync(CancellationToken ct)
        {
            try
            {
                await _gameStarter.StartGameAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception e)
            {
                _startRequested = false;
                Debug.LogError($"{Prefix} Client start failed: {e}");
                _infoPanel?.Show(new InfoMessage("Start Failed", e.Message));
            }
        }

    }
}
