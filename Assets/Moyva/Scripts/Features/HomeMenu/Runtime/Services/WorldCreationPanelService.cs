using System;
using System.Collections.Generic;
using System.Threading;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class WorldCreationPanelService : IWorldCreationPanelService, IInitializable, IDisposable
    {
        [Inject] private IWorldSetupViewController _viewController;
        [Inject] private INavigation _navigation;
        [Inject] private IGameplaySession _gameplaySession;
        [Inject] private IHomeMenuGameStarter _gameStarter;
        [InjectOptional] private ILobbyService _lobbyService;
        [InjectOptional] private ISaveService _saveService;
        [InjectOptional] private ILocalGameSettingsService _localSettings;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;
        [InjectOptional] private ISelectedGameModeService _selectedGameModeService;
        [InjectOptional] private IBotViewController _botViewController;
        [InjectOptional] private WorldCreationDefaultsSO _worldCreationDefaults;
        [InjectOptional] private IInfoPanelService _infoPanelService;
        [Inject(Id = "LobbyPanelName")] private string _lobbyPanelName;
        private WolrdCreationMode _mode;
        private bool _isStarting;
        private CancellationTokenSource _startCts;

        public void Initialize()
        {
            _viewController.OnButtonNextClicked -= OnCreteWorldClicked;
            _viewController.OnButtonNextClicked += OnCreteWorldClicked;
            _viewController.OnSettingsChanged -= Refresh;
            _viewController.OnSettingsChanged += Refresh;
            ApplyDefaultsToView();
            Refresh();
        }

        public void Dispose()
        {
            _viewController.OnButtonNextClicked -= OnCreteWorldClicked;
            _viewController.OnSettingsChanged -= Refresh;

            _startCts?.Cancel();
            _startCts?.Dispose();
            _startCts = null;
        }

        private async void OnCreteWorldClicked()
        {
            if (!CanProceed())
            {
                Refresh();
                return;
            }

            if (ShouldStoreMultiplayerDraft())
            {
                ApplyMultiplayerSessionDraft();
                _navigation.Open(_lobbyPanelName);
            }
            else if (_mode == WolrdCreationMode.Solo)
            {
                if (_isStarting)
                    return;

                _isStarting = true;
                _startCts?.Cancel();
                _startCts?.Dispose();
                _startCts = new CancellationTokenSource();
                var ct = _startCts.Token;
                try
                {
                    ApplySoloSession();
                    await _gameStarter.StartGameAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("[WorldCreationPanelService] Solo start canceled.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[WorldCreationPanelService] Solo start failed: {e}");
                    _infoPanelService?.Show(new InfoMessage("Помилка старту", e.Message));
                }
                finally
                {
                    _isStarting = false;
                    Refresh();
                }
            }
            else
            {
                throw new InvalidOperationException($"Unsupported world creation mode: {_mode}");
            }
        }

        public void Refresh()
        {
            if (_viewController.CreateWorldButton != null)
                _viewController.CreateWorldButton.interactable = CanProceed();
        }

        public void SetupMode(WolrdCreationMode mode)
        {
            _mode = mode;
            Refresh();
        }

        private bool CanProceed()
        {
            return !_isStarting
                && !string.IsNullOrWhiteSpace(_viewController.WorldName)
                && _viewController.Seed != 0;
        }

        private bool ShouldStoreMultiplayerDraft()
        {
            if (_mode == WolrdCreationMode.Multiplayer)
                return true;

            var currentLobby = _lobbyService?.Current;
            return currentLobby != null && !string.IsNullOrWhiteSpace(currentLobby.LobbyId ?? currentLobby.LobbyCode);
        }

        private void ApplySoloSession()
        {
            string localId = "local-player";
            string playerName = string.IsNullOrWhiteSpace(_localSettings?.PlayerName)
                ? "Player"
                : _localSettings.PlayerName;

            var players = new List<GameplayPlayer>
            {
                new GameplayPlayer(localId, playerName, isHost: true, isLocal: true)
            };

            int botCount = _selectedGameModeService != null
                && _selectedGameModeService.SelectedGameMode == Kruty1918.Moyva.HomeMenu.API.GameMode.Bot
                && _botViewController != null
                    ? Mathf.Max(0, _botViewController.BotCount)
                    : 0;

            var worldSettings = new WorldSettingsDto(
                _viewController.WorldName,
                _viewController.Seed,
                (int)_viewController.Size,
                ResolveWorldWidth(),
                ResolveWorldHeight(),
                _viewController.MapType,
                _viewController.Difficulty,
                maxPlayers: 1 + botCount,
                isPrivate: true);

            GameLaunchContext.ConfigureMenuNewGame(
                ResolveNewGameSlot(),
                worldSettings.WorldName,
                worldSettings.Seed,
                worldSettings.Size,
                (int)worldSettings.MapType,
                (int)worldSettings.Difficulty,
                worldSettings.MaxPlayers,
                worldSettings.IsPrivate,
                worldSettings.Width,
                worldSettings.Height);
            _gameplaySession.Apply(NetworkProviderType.Offline, worldSettings, players, localId);
        }

        private void ApplyMultiplayerSessionDraft()
        {
            string localId = ResolveLocalPlayerId();
            string playerName = string.IsNullOrWhiteSpace(_localSettings?.PlayerName)
                ? "Player"
                : _localSettings.PlayerName;
            var currentLobby = _lobbyService?.Current;
            int maxPlayers = currentLobby?.MaxPlayers > 0 ? currentLobby.MaxPlayers : 2;

            var worldSettings = new WorldSettingsDto(
                _viewController.WorldName,
                _viewController.Seed,
                (int)_viewController.Size,
                ResolveWorldWidth(),
                ResolveWorldHeight(),
                _viewController.MapType,
                _viewController.Difficulty,
                maxPlayers,
                currentLobby?.IsPrivate ?? true);

            var players = new List<GameplayPlayer>
            {
                new GameplayPlayer(localId, playerName, isHost: true, isLocal: true)
            };

            _gameplaySession.Apply(_modeSelector?.CurrentMode ?? NetworkProviderType.Relay, worldSettings, players, localId);
            GameLaunchContext.ConfigureMenuMultiplayerGame(
                worldSettings.WorldName,
                worldSettings.Seed,
                worldSettings.Size,
                (int)worldSettings.MapType,
                (int)worldSettings.Difficulty,
                worldSettings.MaxPlayers,
                worldSettings.IsPrivate,
                worldSettings.Width,
                worldSettings.Height);
        }

        private string ResolveLocalPlayerId()
        {
            var currentLobby = _lobbyService?.Current;
            if (currentLobby?.Players != null)
            {
                string playerName = string.IsNullOrWhiteSpace(_localSettings?.PlayerName)
                    ? "Player"
                    : _localSettings.PlayerName;

                foreach (var player in currentLobby.Players)
                {
                    if (player != null && player.IsHost && !string.IsNullOrWhiteSpace(player.PlayerId))
                        return player.PlayerId;
                }

                foreach (var player in currentLobby.Players)
                {
                    if (player != null &&
                        string.Equals(player.DisplayName, playerName, StringComparison.Ordinal) &&
                        !string.IsNullOrWhiteSpace(player.PlayerId))
                    {
                        return player.PlayerId;
                    }
                }
            }

            return "local-player";
        }

        private void ApplyDefaultsToView()
        {
            if (_worldCreationDefaults == null || _viewController == null)
                return;

            if (string.IsNullOrWhiteSpace(_viewController.WorldName))
                _viewController.WorldName = _worldCreationDefaults.BuildIndexedWorldName(CountExistingSaves());

            _viewController.Size = (WorldSize)Mathf.Clamp((int)_worldCreationDefaults.DefaultSizePreset, 0, 2);
            _viewController.MapType = MapDefaultMapType(_worldCreationDefaults.DefaultMapType);
            _viewController.Difficulty = MapDefaultDifficulty(_worldCreationDefaults.DefaultDifficulty);
        }

        private int ResolveWorldWidth()
        {
            return _worldCreationDefaults != null
                ? _worldCreationDefaults.ResolveWidth((WorldSizePreset)(int)_viewController.Size)
                : 0;
        }

        private int ResolveWorldHeight()
        {
            return _worldCreationDefaults != null
                ? _worldCreationDefaults.ResolveHeight((WorldSizePreset)(int)_viewController.Size)
                : 0;
        }

        private int CountExistingSaves()
        {
            if (_saveService == null)
                return 0;

            int count = 0;
            for (int slot = 0; slot <= 99; slot++)
            {
                if (_saveService.HasSave(slot))
                    count++;
            }

            return count;
        }

        private static MapType MapDefaultMapType(MapTypePreset preset)
        {
            return preset switch
            {
                MapTypePreset.Continental => MapType.Continents,
                MapTypePreset.Island => MapType.Islands,
                MapTypePreset.Mountain => MapType.Highlands,
                MapTypePreset.Plains => MapType.Pangaea,
                _ => MapType.Continents,
            };
        }

        private static Difficulty MapDefaultDifficulty(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => Difficulty.Easy,
                DifficultyLevel.Hard => Difficulty.Hard,
                DifficultyLevel.Brutal => Difficulty.Insane,
                _ => Difficulty.Normal,
            };
        }

        private int ResolveNewGameSlot()
        {
            if (_saveService == null)
                return 0;

            for (int slot = 0; slot <= 99; slot++)
            {
                if (!_saveService.HasSave(slot))
                    return slot;
            }

            return 0;
        }
    }
}