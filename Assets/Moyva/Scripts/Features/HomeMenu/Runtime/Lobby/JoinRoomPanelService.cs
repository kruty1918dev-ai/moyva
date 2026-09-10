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
    internal partial class JoinRoomPanelService : IJoinRoomPanelService, IInitializable, IDisposable
    {
        [Inject] private IJoinRoomViewController _viewController;
        [InjectOptional] private ILobbyService _lobbyService;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;
        [InjectOptional] private SwitchableLobbyService _switchableLobbyService;
        [InjectOptional] private IOverlayLoader _loader;
        [Inject] private IJoinRoomUiGateway _uiGateway;
        [InjectOptional] private ILocalGameSettingsService _localGameSettings;
        [InjectOptional] private IConfirmationService _confirmationService;
        [InjectOptional] private IPasswordPanelService _passwordPanelService;
        [InjectOptional] private IInfoPanelService _infoPanelService;
        [InjectOptional] private INetworkProvider _networkProvider;
        [InjectOptional] private SwitchableNetworkProvider _switchableNetworkProvider;
        [InjectOptional] private IConfigStore _configStore;
        [InjectOptional] private IGameplaySession _gameplaySession;
        [InjectOptional] private IHomeMenuGameStarter _gameStarter;
        [InjectOptional] private IServiceModeProfileProvider _serviceModeProfileProvider;
        [InjectOptional] private IMultiplayerState _multiplayerState;
        [InjectOptional] private IRoomAccessPolicyService _roomAccessPolicy;

        private JoinRoomTransportAdapter _transportAdapter;

        private CancellationTokenSource _roomsCts;
        private CancellationTokenSource _joinCts;
        private Action _onJoinRequested;
        private Action _onListRefreshRequested;
        private Action _onJoinCodeChangedCallback;
        private Action<RoomInfo> _onRoomSelectedCallback;
        private Action<NetworkProviderType> _onModeChangedCallback;
        private Action<LobbyState> _onLobbyStateChangedCallback;
        private bool _isJoining;
        private bool _joinedRoomClosed;
        private JoinPipelineState _joinState = JoinPipelineState.Idle;
        private readonly MultiplayerActionRateLimiter _actionRateLimiter = new MultiplayerActionRateLimiter();
        private readonly MultiplayerIdempotencyGuard _idempotencyGuard = new MultiplayerIdempotencyGuard();
        private string _activeJoinOperationKey;

        public string LastJoinPanelName { get; private set; }
        public NetworkProviderType LastJoinProviderType { get; private set; } = NetworkProviderType.Relay;

        public void Dispose()
        {
            try { if (_onJoinRequested != null) _viewController.OnJoinRequested -= _onJoinRequested; } catch { }
            try { if (_onListRefreshRequested != null) _viewController.OnListRoomsRefresh -= _onListRefreshRequested; } catch { }
            try { if (_onJoinCodeChangedCallback != null) _viewController.OnJoinCodeChanged -= _onJoinCodeChangedCallback; } catch { }
            try { if (_onRoomSelectedCallback != null) _viewController.OnRoomSelected -= _onRoomSelectedCallback; } catch { }
            try { if (_modeSelector != null && _onModeChangedCallback != null) _modeSelector.OnModeChanged -= _onModeChangedCallback; } catch { }
            try { if (_lobbyService != null && _onLobbyStateChangedCallback != null) _lobbyService.StateChanged -= _onLobbyStateChangedCallback; } catch { }

            _roomsCts?.Cancel();
            _roomsCts?.Dispose();
            _joinCts?.Cancel();
            _joinCts?.Dispose();
        }

        public void Initialize()
        {
            if (_onJoinRequested != null)
                _viewController.OnJoinRequested -= _onJoinRequested;
            _onJoinRequested = OnJoinClicked;
            _viewController.OnJoinRequested += _onJoinRequested;

            if (_onJoinCodeChangedCallback != null)
                _viewController.OnJoinCodeChanged -= _onJoinCodeChangedCallback;
            _onJoinCodeChangedCallback = () => OnJoinCodeChanged(_viewController.JoinCode);
            _viewController.OnJoinCodeChanged += _onJoinCodeChangedCallback;

            if (_onRoomSelectedCallback != null)
                _viewController.OnRoomSelected -= _onRoomSelectedCallback;
            _onRoomSelectedCallback = room => OnRoomSelected(room);
            _viewController.OnRoomSelected += _onRoomSelectedCallback;

            if (_onListRefreshRequested != null)
                _viewController.OnListRoomsRefresh -= _onListRefreshRequested;
            _onListRefreshRequested = () => _ = RefreshRoomListAsync();
            _viewController.OnListRoomsRefresh += _onListRefreshRequested;

            if (_modeSelector != null)
            {
                if (_onModeChangedCallback != null)
                    _modeSelector.OnModeChanged -= _onModeChangedCallback;
                _onModeChangedCallback = _ => OnProviderModeChanged();
                _modeSelector.OnModeChanged += _onModeChangedCallback;
            }

            if (_lobbyService != null)
            {
                if (_onLobbyStateChangedCallback != null)
                    _lobbyService.StateChanged -= _onLobbyStateChangedCallback;
                _onLobbyStateChangedCallback = OnLobbyStateChanged;
                _lobbyService.StateChanged += _onLobbyStateChangedCallback;
            }

            Refresh();
        }

        public void Refresh()
        {
            OnJoinCodeChanged(_viewController.JoinCode);
        }

        public void RefreshRoomList()
        {
            if (_isJoining)
                return;

            _ = RefreshRoomListAsync();
        }

        public Task<bool> PrepareForOpenAsync(CancellationToken ct = default)
        {
            return RefreshRoomListAsync(ct);
        }

        private JoinRoomTransportAdapter TransportAdapter => _transportAdapter ??=
            new JoinRoomTransportAdapter(_lobbyService, _networkProvider, _switchableNetworkProvider, GetCurrentProviderType, _serviceModeProfileProvider);

    }
}
