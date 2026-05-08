using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.WorldCreation.API;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class LobbyPanelService : ILobbyPanelService, IInitializable, IDisposable
    {
        #region Поля і залежності
        // --- Інжектовані залежності (зв'язок із UI та мережевими сервісами)
        [Inject] private ILobbyPanelViewController _lobbyPanelViewController;
        [Inject(Id = "LobbyPanelName")] private string _lobbyPanelName;
        [Inject] private INavigation _navigation;

        [Inject] private ILobbyService _lobbyService;
        [Inject] private IJoinRoomPanelService _joinRoomPanelService;
        [Inject(Id = "JoinRoomPanelName")] private string _joinRoomPanelName;
        [InjectOptional] private ISessionManager _sessionManager;
        [InjectOptional] private IGameCommandSyncService _gameCommandSync;
        [InjectOptional] private IGameStateService _gameStateService;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;
        [InjectOptional] private IMultiplayerIdentityService _identityService;
        [InjectOptional] private ILocalGameSettingsService _localGameSettings;
        [InjectOptional] private IInfoPanelService _infoPanelService;
        [InjectOptional] private IWorldSetupViewController _worldSetupViewController;
        [InjectOptional] private IGameplaySession _gameplaySession;
        [InjectOptional] private IHomeMenuGameStarter _gameStarter;

        // --- Внутрішній стан
        private string _localPlayerId = string.Empty;
        private LobbyRoom _currentLobby;
        private bool _isStartingGame;
        #endregion

        #region Ініціалізація / життєвий цикл
        /// <summary>
        /// Налаштовує підписки на UI і сервіси, запускає початкове оновлення виду.
        /// </summary>
        public void Initialize()
        {
            // Підписка на кнопку Start
            if (_lobbyPanelViewController.StartGameButton != null)
            {
                _lobbyPanelViewController.StartGameButton.onClick.RemoveListener(OnStartGameClicked);
                _lobbyPanelViewController.StartGameButton.onClick.AddListener(OnStartGameClicked);
            }

            // Слухаємо оновлення лобі
            _lobbyService.LobbyUpdated -= OnLobbyUpdated;
            _lobbyService.LobbyUpdated += OnLobbyUpdated;
            _lobbyService.KickedFromLobby -= OnKickedFromLobby;
            _lobbyService.KickedFromLobby += OnKickedFromLobby;

            // Отримати локальний ідентифікатор гравця асинхронно (може виконувати sign-in під капотом)
            _ = ResolveLocalIdentityAsync();

            // Початкове оновлення UI
            Refresh();
        }

        /// <summary>
        /// Зняти підписки та очистити ресурси.
        /// </summary>
        public void Dispose()
        {
            if (_lobbyPanelViewController?.StartGameButton != null)
                _lobbyPanelViewController.StartGameButton.onClick.RemoveListener(OnStartGameClicked);
            if (_lobbyService != null)
            {
                _lobbyService.LobbyUpdated -= OnLobbyUpdated;
                _lobbyService.KickedFromLobby -= OnKickedFromLobby;
            }
        }

        /// <summary>
        /// Примусово оновити вид панелі на основі поточного стану лобі.
        /// </summary>
        public void Refresh()
        {
            UpdateViewFromLobby(_lobbyService.Current);
        }
        #endregion

        #region Оновлення UI
        /// <summary>
        /// Асинхронно отримати локальний Identity.PlayerId (якщо SessionManager ще не встановив його).
        /// </summary>
        private async Task ResolveLocalIdentityAsync()
        {
            try
            {
                if (_sessionManager != null && !string.IsNullOrEmpty(_sessionManager.LocalPlayerId))
                {
                    _localPlayerId = _sessionManager.LocalPlayerId;
                }
                else if (_identityService != null)
                {
                    var id = await _identityService.ResolveAsync(GetPlayerName());
                    _localPlayerId = id?.PlayerId ?? string.Empty;
                }

                // Після отримання ідентифікатора оновити UI
                MainThreadDispatcher.Enqueue(() => UpdateViewFromLobby(_lobbyService?.Current));
            }
            catch
            {
                _localPlayerId = string.Empty;
            }
        }

        /// <summary>
        /// Оновлює елементи панелі лобі: код запрошення, список гравців, стан кнопки Start.
        /// </summary>
        private void UpdateViewFromLobby(LobbyRoom lobby)
        {
            _currentLobby = lobby;
            _lobbyPanelViewController.ClearUsers();

            if (lobby == null)
            {
                _lobbyPanelViewController.ClearLobbyInvateCode();
                _lobbyPanelViewController.StartGameButton.interactable = false;
                return;
            }

            _lobbyPanelViewController.SetLobbyInvateCode(lobby.LobbyCode);

            int idx = 0;
            foreach (var p in lobby.Players)
            {
                _lobbyPanelViewController.AddNewUser(new LobbyUserInfo { UserName = p.DisplayName, UserId = idx++ });
            }
            _lobbyPanelViewController.RefreshUserList();

            bool isHost = IsHost(lobby);
            bool canStart = CanStartGame(lobby);
            if (_lobbyPanelViewController.StartGameButton != null)
                _lobbyPanelViewController.StartGameButton.interactable = !_isStartingGame && isHost && canStart;
        }
        #endregion

        #region Логіка хоста та старту гри
        /// <summary>
        /// Перевіряє, чи можна розпочати гру (достатня кількість гравців).
        /// </summary>
        private bool CanStartGame(LobbyRoom lobby)
        {
            if (lobby == null) return false;
            int minPlayers = Math.Min(2, lobby.MaxPlayers);
            return lobby.Players?.Count >= minPlayers;
        }

        /// <summary>
        /// Визначає, чи локальний гравець є хостом.
        /// Перевіряємо:
        /// 1) SessionManager flag
        /// 2) локальний PlayerId проти HostPlayerId у лобі
        /// 3) фолбек по Participants
        /// </summary>
        private bool IsHost(LobbyRoom lobby)
        {
            if (lobby == null) return false;

            // Якщо SessionManager вже знає, що ми хост — довіряємо йому
            if (_sessionManager != null && _sessionManager.IsLocalPlayerHost) return true;

            if (!string.IsNullOrEmpty(_localPlayerId))
                return lobby.HostPlayerId == _localPlayerId;

            // LAN/offline lobby ids can be generated locally and not match UGS identity.
            // In that case the host is the lobby player marked IsHost with our current display name.
            var localName = GetPlayerName();
            if (!string.IsNullOrWhiteSpace(localName) && lobby.Players != null)
            {
                foreach (var player in lobby.Players)
                {
                    if (player != null && player.IsHost && string.Equals(player.DisplayName, localName, StringComparison.Ordinal))
                        return true;
                }
            }

            // Фолбек: спробувати вивести з учасників сесії
            if (_sessionManager?.Participants != null)
            {
                foreach (var p in _sessionManager.Participants)
                {
                    if (p.IsHost && !string.IsNullOrEmpty(p.Identity?.PlayerId) && p.Identity.PlayerId == lobby.HostPlayerId)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Обробник натискання Start: блокуємо лобі, шлемо команду Start всім, і запускаємо гру локально.
        /// </summary>
        private async void OnStartGameClicked()
        {
            if (_isStartingGame)
                return;

            // Тільки хост може стартувати гру
            if (!IsHost(_currentLobby)) return;

            _isStartingGame = true;
            if (_lobbyPanelViewController.StartGameButton != null)
                _lobbyPanelViewController.StartGameButton.interactable = false;

            var worldSettings = BuildWorldSettingsDto();

            try
            {
                // Заблокувати лобі, щоб заборонити подальші приєднання
                if (_lobbyService != null)
                    await _lobbyService.LockAsync(true);
            }
            catch
            {
                // Ігноруємо помилки блокування (best-effort)
            }

            // Розіслати команду старту іншим гравцям
            try
            {
                _gameCommandSync?.SendCommand(GameCommandType.StartGame, worldSettings.ToBytes());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[LobbyPanelService] Failed to send StartGame command: {e.Message}");
            }

            // Локальний старт гри
            try
            {
                ApplyGameplaySession(worldSettings);
                _gameStateService?.StartGame();
                if (_gameStarter != null)
                    await _gameStarter.StartGameAsync();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[LobbyPanelService] Local StartGame failed: {e}");
                _infoPanelService?.Show(new InfoMessage("Помилка старту", e.Message));
            }
            finally
            {
                _isStartingGame = false;
                UpdateViewFromLobby(_currentLobby);
            }
        }

        private WorldSettingsDto BuildWorldSettingsDto()
        {
            var seed = _worldSetupViewController != null ? _worldSetupViewController.Seed : 0;
            var size = _worldSetupViewController != null ? (int)_worldSetupViewController.Size : (int)WorldSize.Medium;
            var mapType = _worldSetupViewController != null ? _worldSetupViewController.MapType : MapType.Continents;
            var difficulty = _worldSetupViewController != null ? _worldSetupViewController.Difficulty : Difficulty.Normal;
            var maxPlayers = _currentLobby != null ? _currentLobby.MaxPlayers : 4;
            var isPrivate = _currentLobby != null && _currentLobby.IsPrivate;
            return new WorldSettingsDto(seed, size, mapType, difficulty, maxPlayers, isPrivate);
        }

        private void ApplyGameplaySession(WorldSettingsDto worldSettings)
        {
            if (_gameplaySession == null)
                return;

            var localId = _sessionManager != null && !string.IsNullOrEmpty(_sessionManager.LocalPlayerId)
                ? _sessionManager.LocalPlayerId
                : _localPlayerId;
            var mode = _modeSelector?.CurrentMode ?? NetworkProviderType.Offline;
            _gameplaySession.Apply(mode, worldSettings, ProjectGameplayPlayers(_currentLobby, _sessionManager?.Participants, localId), localId);
        }

        private static IReadOnlyList<GameplayPlayer> ProjectGameplayPlayers(LobbyRoom lobby, IReadOnlyList<Participant> participants, string localPlayerId)
        {
            var result = new List<GameplayPlayer>();
            if (lobby?.Players != null)
            {
                foreach (var p in lobby.Players)
                {
                    if (p == null) continue;
                    var isLocal = !string.IsNullOrEmpty(localPlayerId) && string.Equals(p.PlayerId, localPlayerId, StringComparison.Ordinal);
                    result.Add(new GameplayPlayer(p.PlayerId, p.DisplayName, p.IsHost, isLocal));
                }
            }

            if (result.Count > 0 || participants == null)
                return result;

            foreach (var p in participants)
            {
                if (p?.Identity == null) continue;
                var isLocal = !string.IsNullOrEmpty(localPlayerId) && string.Equals(p.Identity.PlayerId, localPlayerId, StringComparison.Ordinal);
                result.Add(new GameplayPlayer(p.Identity.PlayerId, p.Identity.Nickname, p.IsHost, isLocal));
            }
            return result;
        }
        #endregion

        #region Обробники подій
        /// <summary>
        /// Викликається при оновленні лобі — просто оновлює вид.
        /// </summary>
        private void OnLobbyUpdated(LobbyRoom room)
        {
            MainThreadDispatcher.Enqueue(() => UpdateViewFromLobby(room));
        }

        /// <summary>
        /// Виклик коли гравця викидають з лобі — повертаємо користувача на попередню панель.
        /// </summary>
        private void OnKickedFromLobby(string reason)
        {
            MainThreadDispatcher.Enqueue(() => _ = ReturnToJoinPanelAfterKickAsync(reason));
        }

        private async Task ReturnToJoinPanelAfterKickAsync(string reason)
        {
            var targetPanelName = ResolveKickReturnPanelName();
            var providerType = _joinRoomPanelService?.LastJoinProviderType ?? NetworkProviderType.Relay;

            try
            {
                if (_modeSelector != null)
                    await _modeSelector.SetModeAsync(providerType);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[LobbyPanelService] Failed to restore multiplayer mode after kick: {e.Message}");
            }

            try
            {
                if (_joinRoomPanelService != null)
                    await _joinRoomPanelService.RefreshRoomListAsync();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[LobbyPanelService] Failed to refresh rooms after kick: {e.Message}");
            }

            await MainThreadDispatcher.EnqueueAsync(() =>
            {
                if (!string.IsNullOrWhiteSpace(targetPanelName))
                    _navigation.OpenForce(targetPanelName);
                else
                    _navigation.CloseLastForce();
            });

            try
            {
                _infoPanelService?.Show(new InfoMessage("Видалено з лобі", BuildLobbyExitMessage(reason)));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[LobbyPanelService] InfoPanel notify failed: {e.Message}");
            }
        }

        private string ResolveKickReturnPanelName()
        {
            if (!string.IsNullOrWhiteSpace(_joinRoomPanelService?.LastJoinPanelName))
                return _joinRoomPanelService.LastJoinPanelName;

            return _joinRoomPanelName;
        }

        private string GetPlayerName()
        {
            return string.IsNullOrWhiteSpace(_localGameSettings?.PlayerName)
                ? "Player"
                : _localGameSettings.PlayerName;
        }

        private static string BuildLobbyExitMessage(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return "Підключення до лобі втрачено. Оберіть іншу кімнату або спробуйте ще раз.";

            if (string.Equals(reason, "removed", StringComparison.OrdinalIgnoreCase))
                return "Лобі було видалено. Оберіть іншу кімнату або спробуйте ще раз.";

            if (string.Equals(reason, "lobby_closed", StringComparison.OrdinalIgnoreCase))
                return "Лобі закрито або гру вже розпочато. Оберіть іншу кімнату.";

            return $"Підключення до лобі втрачено: {reason}";
        }
        #endregion
    }
}