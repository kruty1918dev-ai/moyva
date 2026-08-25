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
            catch (Exception e)
            {
            }

            try
            {
                if (_lobbyService != null)
                    await _lobbyService.LeaveAsync(ct);
            }
            catch (Exception e)
            {
            }

            await MainThreadDispatcher.EnqueueAsync(() =>
            {
                _uiGateway?.OpenJoinPanelForce(joinPanelName);
            });

            _infoPanelService?.Show(new InfoMessage(title, string.IsNullOrWhiteSpace(message) ? "Дія: Оновіть список кімнат." : message));
        }

        private static string BuildJoinFailureMessage(Exception exception)
        {
            if (exception == null)
                return "Не вдалося приєднатися до лобі. Спробуйте ще раз.";

            var message = exception.Message;
            if (string.IsNullOrWhiteSpace(message))
                return "Не вдалося приєднатися до лобі. Спробуйте ще раз.";

            if (message.IndexOf("relay", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Не вдалося отримати або використати Relay-код кімнати. Оновіть список лобі й спробуйте ще раз.";

            if (message.IndexOf("lan", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Не вдалося підключитися до LAN-сесії. Переконайтеся, що хост ще в кімнаті, і спробуйте ще раз.";

            return message;
        }

        private static string BuildJoinFailureMessage(DomainError error)
        {
            if (error.IsNone)
                return "Не вдалося приєднатися до лобі. Спробуйте ще раз.";

            if (error.Code == DomainErrorCode.WrongPassword)
                return "Невірний пароль кімнати. Спробуйте ще раз.";

            if (error.Code == DomainErrorCode.NotFound)
                return "Кімнату не знайдено або вона вже недоступна. Оновіть список і спробуйте ще раз.";

            if (error.Code == DomainErrorCode.Validation)
                return string.IsNullOrWhiteSpace(error.Message)
                    ? "Невалідні дані для приєднання. Перевірте параметри й повторіть спробу."
                    : error.Message;

            return string.IsNullOrWhiteSpace(error.Message)
                ? "Не вдалося приєднатися до лобі. Спробуйте ще раз."
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
            catch (Exception e)
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
