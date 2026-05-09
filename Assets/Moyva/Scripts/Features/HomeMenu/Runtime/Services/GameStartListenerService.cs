using System;
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
    /// Виконується на хості і клієнтах.
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

        public void Initialize()
        {
            if (_commandSync == null) return;
            _commandSync.RegisterHandler(GameCommandType.StartGame, OnStartGameCommand);
            Debug.Log($"{Prefix} Registered handler for StartGame.");
        }

        public void Dispose()
        {
            // IGameCommandSyncService не має Unregister — лишаємо в зареєстрованому стані до перезавантаження сесії.
        }

        private void OnStartGameCommand(string senderId, byte[] payload)
        {
            try
            {
                Debug.Log($"{Prefix} StartGame received from '{senderId}', payload={payload?.Length ?? 0} bytes.");

                if (!WorldSettingsDto.TryFromBytes(payload, out var dto))
                {
                    Debug.LogWarning($"{Prefix} Empty or invalid WorldSettings payload — using defaults.");
                    dto = new WorldSettingsDto(0, 1, MapType.Continents, Difficulty.Normal, 4, false);
                }

                var mode = _modeSelector?.CurrentMode ?? NetworkProviderType.Offline;
                var localId = _sessionManager?.LocalPlayerId ?? string.Empty;
                var players = MultiplayerRoomLifecycle.ProjectGameplayPlayers(_lobbyService?.Current, localId);

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
                    dto.Height);

                MainThreadDispatcher.Enqueue(() =>
                {
                    if (_gameStarter == null)
                    {
                        Debug.LogWarning($"{Prefix} IHomeMenuGameStarter не підключений — гра не запущена.");
                        return;
                    }
                    _ = _gameStarter.StartGameAsync();
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"{Prefix} OnStartGameCommand error: {e}");
                _infoPanel?.Show(new InfoMessage("Помилка старту", e.Message));
            }
        }

    }
}
