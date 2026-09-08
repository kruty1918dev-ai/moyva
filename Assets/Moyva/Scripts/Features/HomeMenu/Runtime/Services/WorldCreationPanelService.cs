using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Core;
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
        [InjectOptional] private INetworkProvider _networkProvider;
        [InjectOptional] private ISaveService _saveService;
        [InjectOptional] private ISessionManager _sessionManager;
        [InjectOptional] private ILocalGameSettingsService _localSettings;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;
        [InjectOptional] private ILobbyFlowContext _lobbyFlowContext;
        [InjectOptional] private WorldCreationDefaultsSO _worldCreationDefaults;
        [InjectOptional] private IHomeMenuGameStarter _gameStarter;
        [InjectOptional] private IOverlayLoader _overlayLoader;
        [InjectOptional] private IGameStateService _gameStateService;
        [InjectOptional] private IInfoPanelService _infoPanelService;
        [InjectOptional] private HomeMenuMoyvaUiState _moyvaUiState;
        [Inject(Id = "LobbyPanelName")] private string _lobbyPanelName;
        private bool _isStartingLocalGame;
        private CancellationTokenSource _startCts;

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
            _startCts?.Cancel();
            _startCts?.Dispose();
            _startCts = null;
        }

        private async void OnCreteWorldClicked()
        {
            if (_isStartingLocalGame)
                return;

            if (!CanProceed())
            {
                Refresh();
                return;
            }

            int saveSlot = 0;
            if (IsSoloFlow() && !TryFindNewSaveSlot(out saveSlot))
            {
                _infoPanelService?.Show(new InfoMessage("Cannot start game",
                    _saveService == null ? "The save service is not available. Return to the main menu and try again."
                    : "No save slot is available. Delete an old save before starting a new world."));
                return;
            }

            if (ShouldCreateMultiplayerLobby())
            {
                _isStartingLocalGame = true;
                Refresh();
                _startCts?.Cancel();
                _startCts?.Dispose();
                _startCts = new CancellationTokenSource();

                try
                {
                    _overlayLoader?.LoadOverlay(0f, 100f, "%");
                    var room = await CreateMultiplayerLobbyAsync(_startCts.Token);
                    if (room == null)
                        return;

                    ApplyGameplaySessionDraft(saveSlot);
                    _navigation.Open(_lobbyPanelName);
                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[WorldCreationPanelService] Multiplayer lobby creation failed: {e}");
                    _infoPanelService?.Show(new InfoMessage("Room Error", e.Message));
                    await CleanupFailedLobbyCreationAsync();
                    return;
                }
                finally
                {
                    _isStartingLocalGame = false;
                    try { _overlayLoader?.StopOverlay(true); } catch { }
                    Refresh();
                }
            }

            ApplyGameplaySessionDraft(saveSlot);
            if (ShouldContinueToLobby())
                _navigation.Open(_lobbyPanelName);
            else
                await StartLocalGameAsync();
        }

        private void Refresh()
        {
            if (_viewController.CreateWorldButton != null)
                _viewController.CreateWorldButton.interactable = CanProceed();
        }

        private bool CanProceed()
            => !string.IsNullOrWhiteSpace(_viewController.WorldName)
                && _viewController.Seed != 0;

        private bool TryFindNewSaveSlot(out int slot)
        {
            if (_saveService != null)
            {
                for (slot = 0; slot <= 99; slot++)
                    if (!_saveService.HasSave(slot)) return true;
            }

            slot = -1;
            return false;
        }

        private void ApplyGameplaySessionDraft(int saveSlot)
        {
            string localId = ResolveLocalPlayerId();
            string playerName = string.IsNullOrWhiteSpace(_localSettings?.PlayerName)
                ? "Player"
                : _localSettings.PlayerName;
            bool soloFlow = IsSoloFlow();
            var currentLobby = _lobbyService?.Current;
            int maxPlayers = soloFlow
                ? 1
                : currentLobby != null && currentLobby.MaxPlayers > 0
                    ? currentLobby.MaxPlayers
                    : 2;

            var worldSettings = new WorldSettingsDto(
                _viewController.WorldName,
                _viewController.Seed,
                (int)_viewController.Size,
                ResolveWorldWidth(),
                ResolveWorldHeight(),
                _viewController.MapType,
                _viewController.Difficulty,
                maxPlayers,
                soloFlow || (currentLobby?.IsPrivate ?? true));

            var players = new List<GameplayPlayer>
            {
                new GameplayPlayer(localId, playerName, isHost: true, isLocal: true)
            };

            _gameplaySession.Apply(ResolveProvider(), worldSettings, players, localId);
            if (soloFlow)
            {
                GameLaunchContext.ConfigureMenuNewGame(
                    saveSlot,
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
            else
            {
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
        }

        private string ResolveLocalPlayerId()
        {
            if (!string.IsNullOrWhiteSpace(_sessionManager?.LocalPlayerId))
                return _sessionManager.LocalPlayerId.Trim();

            if (GameLaunchContext.HasLocalPlayerRole
                && !string.IsNullOrWhiteSpace(GameLaunchContext.LocalPlayerId))
            {
                return GameLaunchContext.LocalPlayerId.Trim();
            }

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
            if (IsSoloFlow())
                return NetworkProviderType.Offline;

            if (_lobbyFlowContext != null && _lobbyFlowContext.FlowKind != LobbyFlowKind.None)
                return _lobbyFlowContext.Provider;

            if (_lobbyService?.Current != null)
                return _modeSelector?.CurrentMode ?? NetworkProviderType.Relay;

            return NetworkProviderType.Offline;
        }

        private bool ShouldContinueToLobby()
        {
            if (IsSoloFlow())
                return false;

            if (_lobbyService?.Current != null)
                return true;

            return _lobbyFlowContext != null && _lobbyFlowContext.FlowKind != LobbyFlowKind.None;
        }

        private bool ShouldCreateMultiplayerLobby()
        {
            return !IsSoloFlow()
                && _lobbyService?.Current == null
                && _lobbyFlowContext != null
                && _lobbyFlowContext.FlowKind == LobbyFlowKind.Create
                && _lobbyFlowContext.HasRoomDraft;
        }

        private bool IsSoloFlow()
        {
            if (_moyvaUiState != null)
                return _moyvaUiState.PlayFlow == HomeMenuPlayFlow.Solo;

            if (_lobbyFlowContext != null && _lobbyFlowContext.FlowKind != LobbyFlowKind.None)
                return false;

            return _lobbyService?.Current == null;
        }

        private async Task StartLocalGameAsync()
        {
            if (_isStartingLocalGame)
                return;

            if (_gameStarter == null)
            {
                Debug.LogError("[WorldCreationPanelService] Cannot start local game: IHomeMenuGameStarter is not available.");
                return;
            }

            _isStartingLocalGame = true;
            _startCts?.Cancel();
            _startCts?.Dispose();
            _startCts = new CancellationTokenSource();

            try
            {
                _overlayLoader?.LoadOverlay(0f, 100f, "%");
                _gameStateService?.StartGame();
                await _gameStarter.StartGameAsync(_startCts.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError($"[WorldCreationPanelService] Local game start failed: {e}");
            }
            finally
            {
                _isStartingLocalGame = false;
                try { _overlayLoader?.StopOverlay(true); } catch { }
            }
        }

        private async Task<LobbyRoom> CreateMultiplayerLobbyAsync(CancellationToken ct)
        {
            if (_lobbyService == null)
                throw new InvalidOperationException("Lobby service is unavailable.");

            await ApplySelectedProviderAsync(ct);

            var providerType = ResolveProvider();
            string roomName = string.IsNullOrWhiteSpace(_lobbyFlowContext?.RoomName)
                ? "Moyva Lobby"
                : _lobbyFlowContext.RoomName.Trim();

            string transportJoinCode = await StartNetworkHostAsync(providerType, roomName, ct);
            if (transportJoinCode == null)
                return null;

            var options = new CreateRoomOptions(
                roomName,
                Mathf.Clamp(_lobbyFlowContext?.MaxPlayers ?? 4, 2, 8),
                isPrivate: !(_lobbyFlowContext?.IsPublic ?? true),
                displayName: ResolvePlayerName(),
                password: _lobbyFlowContext?.Password,
                relayJoinCode: transportJoinCode);

            LobbyRoom room = await _lobbyService.CreateRoomAsync(options, ct);
            if (room == null)
                throw new InvalidOperationException("Could not create lobby: service returned an empty result.");

            if (!string.IsNullOrWhiteSpace(transportJoinCode))
                await _lobbyService.SetRelayJoinCodeAsync(transportJoinCode, ct);

            _lobbyFlowContext?.ClearRoomDraft();
            return room;
        }

        private async Task ApplySelectedProviderAsync(CancellationToken ct)
        {
            if (_modeSelector != null)
                await _modeSelector.SetModeAsync(ResolveProvider());
        }

        private async Task<string> StartNetworkHostAsync(NetworkProviderType providerType, string roomName, CancellationToken ct)
        {
            if (_networkProvider == null)
            {
                if (providerType == NetworkProviderType.Relay)
                    throw new InvalidOperationException("Global Relay transport is unavailable.");

                return string.Empty;
            }

            var effectiveNetworkType = ResolveEffectiveNetworkProviderType();
            if (providerType == NetworkProviderType.Relay && effectiveNetworkType != NetworkProviderType.Relay)
                throw new InvalidOperationException($"Global Relay transport is unavailable: the active network provider is {effectiveNetworkType}.");

            var hostSessionId = providerType == NetworkProviderType.Relay
                ? string.Empty
                : roomName;
            var result = await _networkProvider.HostSessionAsync(hostSessionId, ct);
            if (result == null || !result.Success)
                throw new InvalidOperationException(result?.ErrorMessage ?? "Could not start the network session.");

            var joinCode = result.SessionId?.Trim() ?? string.Empty;
            if (providerType == NetworkProviderType.Relay && !RelayJoinCodeUtility.IsValid(joinCode))
                throw new InvalidOperationException($"Relay returned an invalid join code '{joinCode}'.");

            if (providerType == NetworkProviderType.Lan && !IsLanJoinCode(joinCode))
                throw new InvalidOperationException($"LAN transport returned an invalid join code '{joinCode}'. Expected lan:<ip>:<port>.");

            return joinCode;
        }

        private static bool IsLanJoinCode(string joinCode)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
                return false;

            var parts = joinCode.Trim().Split(':');
            return parts.Length >= 3
                && string.Equals(parts[0], "lan", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(parts[1])
                && int.TryParse(parts[2], out var port)
                && port > 0
                && port <= 65535;
        }

        private NetworkProviderType ResolveEffectiveNetworkProviderType()
        {
            return _networkProvider is SwitchableNetworkProvider switchableNetworkProvider
                ? switchableNetworkProvider.CurrentType
                : _networkProvider is RelayNetworkProvider ? NetworkProviderType.Relay
                : _networkProvider is LanNetworkProvider ? NetworkProviderType.Lan
                : _networkProvider is WebSocketNetworkProvider ? NetworkProviderType.WebSocket
                : _networkProvider is OfflineNetworkProvider ? NetworkProviderType.Offline
                : ResolveProvider();
        }

        private async Task CleanupFailedLobbyCreationAsync()
        {
            try { if (_lobbyService != null) await _lobbyService.LeaveAsync(); } catch { }
            try { if (_networkProvider != null) await _networkProvider.LeaveSessionAsync(); } catch { }
        }

        private string ResolvePlayerName()
        {
            return string.IsNullOrWhiteSpace(_localSettings?.PlayerName)
                ? "Player"
                : _localSettings.PlayerName;
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
