using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Shared.Common;
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
        [InjectOptional] private WorldCreationDefaultsSO _worldCreationDefaults;
        [InjectOptional] private IHomeMenuGameStarter _gameStarter;
        [InjectOptional] private IOverlayLoader _overlayLoader;
        [InjectOptional] private IConfirmationService _confirmationService;
        [InjectOptional] private ILobbyFlowContext _lobbyFlowContext;

        // --- Внутрішній стан
        private string _localPlayerId = string.Empty;
        private LobbyRoom _currentLobby;
        private bool _isStartingGame;
        private bool _isLeavingLobby;
        private bool _leavePromptOpen;
        private CancellationTokenSource _startGameCts;
        private readonly MultiplayerActionRateLimiter _rateLimiter = new MultiplayerActionRateLimiter();
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

            // Підписка на кнопку Back — показуємо модальне вікно підтвердження виходу
            if (_lobbyPanelViewController.BackButton != null)
            {
                _lobbyPanelViewController.BackButton.onClick.RemoveListener(OnBackClicked);
                _lobbyPanelViewController.BackButton.onClick.AddListener(OnBackClicked);
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
            if (_lobbyPanelViewController?.BackButton != null)
                _lobbyPanelViewController.BackButton.onClick.RemoveListener(OnBackClicked);
            if (_lobbyService != null)
            {
                _lobbyService.LobbyUpdated -= OnLobbyUpdated;
                _lobbyService.KickedFromLobby -= OnKickedFromLobby;
            }

            _startGameCts?.Cancel();
            _startGameCts?.Dispose();
            _startGameCts = null;
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
                if (_lobbyPanelViewController.StartGameButton != null)
                    _lobbyPanelViewController.StartGameButton.interactable = false;
                return;
            }

            _lobbyPanelViewController.SetInviteCode(LobbyInviteCodeResolver.Resolve(lobby, ResolveProvider()));

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
            if (lobby.State != LobbyState.Open) return false;
            return CountConnectedPlayers(lobby) >= 2;
        }

        private static bool CanStartLocallyWithoutCommandSync(LobbyRoom lobby)
        {
            return false;
        }

        private static int CountConnectedPlayers(LobbyRoom lobby)
        {
            if (lobby?.Players == null)
                return 0;

            var count = 0;
            foreach (var player in lobby.Players)
            {
                if (player != null && !string.IsNullOrWhiteSpace(player.PlayerId))
                    count++;
            }

            return count;
        }

        private int CountOtherConnectedPlayers(LobbyRoom lobby)
        {
            if (lobby?.Players == null)
                return 0;

            var localId = ResolveLocalPlayerId(lobby);
            if (string.IsNullOrWhiteSpace(localId))
                return Math.Max(0, CountConnectedPlayers(lobby) - 1);

            var count = 0;
            foreach (var player in lobby.Players)
            {
                if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
                    continue;

                if (!string.Equals(player.PlayerId, localId, StringComparison.Ordinal))
                    count++;
            }

            return count;
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

            if (_sessionManager != null && _sessionManager.IsLocalPlayerHost)
                return true;

            var localId = ResolveLocalPlayerId(lobby);
            return !string.IsNullOrWhiteSpace(localId)
                && string.Equals(lobby.HostPlayerId, localId, StringComparison.Ordinal);
        }

        private string ResolveLocalPlayerId(LobbyRoom lobby)
        {
            var localId = (_lobbyService as ILobbyLocalIdentity)?.LocalPlayerId;
            if (string.IsNullOrWhiteSpace(localId))
                localId = _sessionManager?.LocalPlayerId;
            if (string.IsNullOrWhiteSpace(localId))
                localId = _localPlayerId;
            if (string.IsNullOrWhiteSpace(localId) && _sessionManager != null && _sessionManager.IsLocalPlayerHost)
                localId = lobby?.HostPlayerId;

            return localId?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Обробник натискання Start: блокуємо лобі, шлемо команду Start всім, і запускаємо гру локально.
        /// </summary>
        private async void OnStartGameClicked()
        {
            if (!_rateLimiter.Allow("start-game", TimeSpan.FromSeconds(1)))
            {
                _infoPanelService?.Show(new InfoMessage("Please Wait", "Start is being requested too often."));
                return;
            }

            var readiness = MultiplayerPreflightChecks.ValidateSessionReadiness(
                hasLobbyService: _lobbyService != null,
                hasGameStarter: _gameStarter != null,
                hasCommandSync: _gameCommandSync != null || CanStartLocallyWithoutCommandSync(_currentLobby));
            if (readiness.IsFailure)
            {
                var readinessError = MultiplayerUserFacingError.FromDomainError(readiness.Error, MoyvaId.NewTraceId());
                _infoPanelService?.Show(new InfoMessage("Session readiness", readinessError.BuildDisplayMessage()));
                return;
            }

            if (_isStartingGame)
                return;

            // Тільки хост може стартувати гру
            if (!IsHost(_currentLobby)) return;
            if (!CanStartGame(_currentLobby))
            {
                _infoPanelService?.Show(new InfoMessage("Start Unavailable", "At least two players must be in the room to start the game."));
                UpdateViewFromLobby(_currentLobby);
                return;
            }

            _isStartingGame = true;
            _startGameCts?.Cancel();
            _startGameCts?.Dispose();
            _startGameCts = new CancellationTokenSource();
            var ct = _startGameCts.Token;
            if (_lobbyPanelViewController.StartGameButton != null)
                _lobbyPanelViewController.StartGameButton.interactable = false;

            try { _overlayLoader?.LoadOverlay(0f, 100f, "%"); } catch { }

            try
            {
                var worldSettings = BuildWorldSettingsDto();
                var worldSettingsBytes = worldSettings.ToBytes();
                await _lobbyService.LockAsync(true, worldSettingsBytes, ct);
                _gameCommandSync?.SendCommand(GameCommandType.StartGame, worldSettingsBytes);

                var localPlayerId = ApplyGameplaySession(worldSettings);
                GameLaunchContext.ConfigureMenuMultiplayerGame(
                    worldSettings.WorldName,
                    worldSettings.Seed,
                    worldSettings.Size,
                    (int)worldSettings.MapType,
                    (int)worldSettings.Difficulty,
                    worldSettings.MaxPlayers,
                    worldSettings.IsPrivate,
                    worldSettings.Width,
                    worldSettings.Height,
                    isLocalPlayerHost: _gameplaySession?.IsHost ?? IsHost(_currentLobby),
                    localPlayerId: localPlayerId);
                _gameStateService?.StartGame();
                if (_gameStarter != null)
                    await _gameStarter.StartGameAsync(ct);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[LobbyPanelService] Local StartGame failed: {e}");
                _infoPanelService?.Show(new InfoMessage("Start Failed", e.Message));
            }
            finally
            {
                _isStartingGame = false;
                try { _overlayLoader?.StopOverlay(true); } catch { }
                UpdateViewFromLobby(_currentLobby);
            }
        }

        private WorldSettingsDto BuildWorldSettingsDto()
        {
            if (TryGetDraftWorldSettings(out var draft))
                return draft;

            var seed = _worldSetupViewController != null ? _worldSetupViewController.Seed : 0;
            var worldName = _worldSetupViewController != null ? _worldSetupViewController.WorldName : "New World";
            var size = _worldSetupViewController != null ? (int)_worldSetupViewController.Size : (int)WorldSize.Medium;
            var mapType = _worldSetupViewController != null ? _worldSetupViewController.MapType : MapType.Continents;
            var difficulty = _worldSetupViewController != null ? _worldSetupViewController.Difficulty : Difficulty.Normal;
            var maxPlayers = _currentLobby != null ? _currentLobby.MaxPlayers : 4;
            var isPrivate = _currentLobby != null && _currentLobby.IsPrivate;
            int width = _worldCreationDefaults != null
                ? _worldCreationDefaults.ResolveWidth((WorldSizePreset)size)
                : 0;
            int height = _worldCreationDefaults != null
                ? _worldCreationDefaults.ResolveHeight((WorldSizePreset)size)
                : 0;
            return new WorldSettingsDto(worldName, seed, size, width, height, mapType, difficulty, maxPlayers, isPrivate);
        }

        private bool TryGetDraftWorldSettings(out WorldSettingsDto worldSettings)
        {
            worldSettings = default;
            var draft = _gameplaySession?.WorldSettings ?? default;
            if (string.IsNullOrWhiteSpace(draft.WorldName) || draft.Seed == 0)
                return false;

            int maxPlayers = _currentLobby != null ? _currentLobby.MaxPlayers : draft.MaxPlayers;
            bool isPrivate = _currentLobby != null ? _currentLobby.IsPrivate : draft.IsPrivate;
            worldSettings = new WorldSettingsDto(
                draft.WorldName,
                draft.Seed,
                draft.Size,
                draft.Width,
                draft.Height,
                draft.MapType,
                draft.Difficulty,
                maxPlayers,
                isPrivate);
            return true;
        }

        private string ApplyGameplaySession(WorldSettingsDto worldSettings)
        {
            if (_gameplaySession == null)
                return ResolveGameplayLocalPlayerId(_currentLobby);

            var localId = ResolveGameplayLocalPlayerId(_currentLobby);
            var mode = ResolveProvider();
            bool isLocalHost = IsHost(_currentLobby);
            _gameplaySession.Apply(
                mode,
                worldSettings,
                MultiplayerRoomLifecycle.ProjectGameplayPlayers(
                    _currentLobby,
                    localId,
                    isLocalHost),
                localId);
            return localId;
        }

        private string ResolveGameplayLocalPlayerId(LobbyRoom lobby)
        {
            var localId = (_lobbyService as ILobbyLocalIdentity)?.LocalPlayerId;
            if (string.IsNullOrWhiteSpace(localId))
                localId = _sessionManager?.LocalPlayerId;
            if (string.IsNullOrWhiteSpace(localId))
                localId = _localPlayerId;
            return localId?.Trim() ?? string.Empty;
        }

        private NetworkProviderType ResolveProvider()
        {
            if (_lobbyFlowContext != null && _lobbyFlowContext.FlowKind != LobbyFlowKind.None)
                return _lobbyFlowContext.Provider;

            return _modeSelector?.CurrentMode ?? NetworkProviderType.Offline;
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
            catch (Exception)
            {
            }

            try
            {
                if (_joinRoomPanelService != null)
                    await _joinRoomPanelService.RefreshRoomListAsync();
            }
            catch (Exception)
            {
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
                _infoPanelService?.Show(new InfoMessage("Removed from Lobby", BuildLobbyExitMessage(reason)));
            }
            catch (Exception)
            {
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
                return "Connection to the lobby was lost. Choose another room or try again.";

            if (string.Equals(reason, "removed", StringComparison.OrdinalIgnoreCase))
                return "The lobby was removed. Choose another room or try again.";

            if (string.Equals(reason, "lobby_closed", StringComparison.OrdinalIgnoreCase))
                return "The lobby is closed or the game has already started. Choose another room.";

            return $"Connection to the lobby was lost: {reason}";
        }

        /// <summary>
        /// Обробка кнопки Back у лобі — показуємо confirmation і виходимо з лобі при підтвердженні.
        /// </summary>
        private void OnBackClicked()
        {
            if (!TryLeaveBeforeNavigation(() => _navigation.OpenLast()))
                _navigation.OpenLast();
        }

        public bool TryLeaveBeforeNavigation(Action continuation)
        {
            if (_isStartingGame || _lobbyService?.Current == null
                || !string.Equals(_navigation.CurrentMenu, _lobbyPanelName, StringComparison.Ordinal))
                return false;
            if (_isLeavingLobby || _leavePromptOpen)
                return true;
            if (_confirmationService == null)
            {
                _infoPanelService?.Show(new InfoMessage("Leave unavailable", "The confirmation dialog is unavailable."));
                return true;
            }
            _leavePromptOpen = true;
            var lobby = _currentLobby ?? _lobbyService.Current;
            bool host = IsHost(lobby);
            _confirmationService.Show(new ConfirmationRequest
            {
                LabelText = "Leave Lobby?",
                MessageText = BuildLeaveLobbyConfirmationMessage(lobby, host),
                OnConfirm = () =>
                {
                    _leavePromptOpen = false;
                    _ = LeaveLobbyAndNavigateBackAsync(continuation);
                },
                OnCancel = () => _leavePromptOpen = false,
            });
            return true;
        }

        private string BuildLeaveLobbyConfirmationMessage(LobbyRoom lobby, bool host)
        {
            if (!host)
                return "Leave this lobby and disconnect from its participants?";

            return CountOtherConnectedPlayers(lobby) > 0
                ? "Leave the lobby? Host will transfer to another player before you disconnect."
                : "Leave the lobby? The lobby will close because no other players are connected.";
        }

        private async Task LeaveLobbyAndNavigateBackAsync(Action continuation)
        {
            if (_isLeavingLobby)
                return;
            _isLeavingLobby = true;
            try
            {
                if (_sessionManager != null)
                    await _sessionManager.LeaveSessionAsync();
                else if (_lobbyService != null)
                    await _lobbyService.LeaveAsync();
                await MainThreadDispatcher.EnqueueAsync(() =>
                {
                    _currentLobby = null;
                    continuation?.Invoke();
                });
            }
            catch (Exception exception)
            {
                await MainThreadDispatcher.EnqueueAsync(() =>
                    _infoPanelService?.Show(new InfoMessage("Could not leave lobby", exception.Message)));
            }
            finally
            {
                _isLeavingLobby = false;
            }
        }
        #endregion
    }
}
