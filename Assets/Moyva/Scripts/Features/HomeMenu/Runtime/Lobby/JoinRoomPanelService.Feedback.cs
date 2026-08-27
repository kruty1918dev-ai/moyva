using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Shared.Common;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal partial class JoinRoomPanelService
    {
        private async Task ReturnToLobbyChooserWithMessageAsync(string joinPanelName, string title, string message, CancellationToken ct)
        {
            _passwordPanelService?.Cancel();

            try
            {
                if (_networkProvider != null)
                    await _networkProvider.LeaveSessionAsync(ct);
            }
            catch (Exception)
            {
            }

            try
            {
                if (_lobbyService != null)
                    await _lobbyService.LeaveAsync(ct);
            }
            catch (Exception)
            {
            }

            await MainThreadDispatcher.EnqueueAsync(() =>
            {
                _uiGateway?.OpenJoinPanelForce(joinPanelName);
            });

            _infoPanelService?.Show(new InfoMessage(title, string.IsNullOrWhiteSpace(message) ? "Action: Refresh the room list." : message));
        }

        private static string BuildJoinFailureMessage(Exception exception)
        {
            if (exception == null)
                return "Could not join the lobby. Try again.";

            var message = exception.Message;
            if (string.IsNullOrWhiteSpace(message))
                return "Could not join the lobby. Try again.";

            if (message.IndexOf("relay", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Could not get or use the room Relay code. Refresh the lobby list and try again.";

            if (message.IndexOf("lan", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Could not connect to the LAN session. Make sure the host is still in the room and try again.";

            return message;
        }

        private static string BuildJoinFailureMessage(DomainError error)
        {
            if (error.IsNone)
                return "Could not join the lobby. Try again.";

            if (error.Code == DomainErrorCode.WrongPassword)
                return "Wrong room password. Try again.";

            if (error.Code == DomainErrorCode.NotFound)
                return "Room was not found or is no longer available. Refresh the list and try again.";

            if (error.Code == DomainErrorCode.Validation)
                return string.IsNullOrWhiteSpace(error.Message)
                    ? "Invalid join data. Check the parameters and try again."
                    : error.Message;

            return string.IsNullOrWhiteSpace(error.Message)
                ? "Could not join the lobby. Try again."
                : error.Message;
        }

        private readonly struct ProbeResult
        {
            public bool RequiresPassword { get; }
            public string DisplayName { get; }
            public ProbeResult(bool requiresPassword, string displayName)
            {
                RequiresPassword = requiresPassword;
                DisplayName = displayName ?? string.Empty;
            }
        }

        /// <summary>
        /// Перевіряє за списком кімнат (без приєднання), чи потребує обрана кімната пароль.
        /// Це робиться через дані останнього QueryRoomsAsync — точно для LAN, для UGS — best-effort.
        /// </summary>
        private async Task<ProbeResult> TryProbeRoomForPasswordAsync(JoinRoomTarget target, CancellationToken ct)
        {
            try
            {
            var rooms = await _lobbyService.QueryRoomsAsync(ct);
                if (rooms != null)
                {
                    foreach (var r in rooms)
                    {
                        if (r == null) continue;
                        bool match = string.Equals(r.LobbyCode, target.Value, StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(r.LobbyId, target.Value, StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(r.RelayJoinCode, target.Value, StringComparison.OrdinalIgnoreCase);
                        if (match)
                            return new ProbeResult(r.HasPassword, r.Name);
                    }
                }
            }
            catch (Exception)
            {
            }
            return new ProbeResult(false, target.Value);
        }

        private string ResolveJoinOriginPanelName()
        {
            return _uiGateway?.ResolveJoinOriginPanelName(LastJoinPanelName) ?? LastJoinPanelName;
        }

        private void RememberJoinOrigin(string panelName, NetworkProviderType providerType)
        {
            if (!string.IsNullOrWhiteSpace(panelName))
                LastJoinPanelName = panelName;
            LastJoinProviderType = providerType;
        }

        private NetworkProviderType GetCurrentProviderType()
        {
            if (_switchableLobbyService != null)
                return _switchableLobbyService.CurrentProviderType;

            if (_lobbyService is SwitchableLobbyService switchableLobbyService)
                return switchableLobbyService.CurrentProviderType;

            if (_modeSelector != null)
                return _modeSelector.EffectiveMode;

            return NetworkProviderType.Relay;
        }

        private string GetPlayerName()
        {
            return string.IsNullOrWhiteSpace(_localGameSettings?.PlayerName)
                ? "Player"
                : _localGameSettings.PlayerName;
        }

    }
}
