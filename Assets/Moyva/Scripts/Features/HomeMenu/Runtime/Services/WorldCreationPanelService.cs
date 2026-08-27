using System;
using System.Collections.Generic;
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
    internal sealed class WorldCreationPanelService : IInitializable, IDisposable
    {
        [Inject] private IWorldSetupViewController _viewController;
        [Inject] private INavigation _navigation;
        [Inject] private IGameplaySession _gameplaySession;
        [InjectOptional] private ILobbyService _lobbyService;
        [InjectOptional] private ISaveService _saveService;
        [InjectOptional] private ILocalGameSettingsService _localSettings;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;
        [InjectOptional] private ILobbyFlowContext _lobbyFlowContext;
        [InjectOptional] private WorldCreationDefaultsSO _worldCreationDefaults;
        [Inject(Id = "LobbyPanelName")] private string _lobbyPanelName;

        /// <summary>Підписує multiplayer world setup на UI та застосовує JSON defaults.</summary>
        public void Initialize()
        {
            _viewController.OnButtonNextClicked -= OnCreteWorldClicked;
            _viewController.OnButtonNextClicked += OnCreteWorldClicked;
            _viewController.OnSettingsChanged -= Refresh;
            _viewController.OnSettingsChanged += Refresh;
            ApplyDefaultsToView();
            Refresh();
        }

        /// <summary>Відписує обробники UI під час закриття меню.</summary>
        public void Dispose()
        {
            _viewController.OnButtonNextClicked -= OnCreteWorldClicked;
            _viewController.OnSettingsChanged -= Refresh;
        }

        private void OnCreteWorldClicked()
        {
            if (!CanProceed())
            {
                Refresh();
                return;
            }

            ApplyMultiplayerSessionDraft();
            _navigation.Open(_lobbyPanelName);
        }

        private void Refresh()
        {
            if (_viewController.CreateWorldButton != null)
                _viewController.CreateWorldButton.interactable = CanProceed();
        }

        private bool CanProceed()
            => !string.IsNullOrWhiteSpace(_viewController.WorldName)
                && _viewController.Seed != 0;

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

            _gameplaySession.Apply(ResolveProvider(), worldSettings, players, localId);
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
                isLocalPlayerHost: true,
                localPlayerId: localId);
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

        private NetworkProviderType ResolveProvider()
        {
            if (_lobbyFlowContext != null && _lobbyFlowContext.FlowKind != LobbyFlowKind.None)
                return _lobbyFlowContext.Provider;

            return _modeSelector?.CurrentMode ?? NetworkProviderType.Relay;
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

    }
}
