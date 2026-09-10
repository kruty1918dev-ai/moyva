using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Shared.Controls;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class HomeMenuMoyvaUiViewController :
        IInitializable,
        IDisposable,
        IContinueViewController,
        ICreateRoomViewController,
        IGameSettingsViewController,
        IInfoPanelViewController,
        IJoinRoomViewController,
        IKickPlayerPanelViewController,
        ILobbyPanelViewController,
        IMultiplayerModeViewController,
        IMultiplayerViewController,
        IPasswordPanelViewController,
        IWorldSetupViewController,
        IConfiremationPanel,
        IOverlayLoader
    {
        private readonly HomeMenuMoyvaUiState _state;
        private readonly List<GameObject> _ownedObjects = new();
        private readonly List<GameSlotInfo> _slots = new();
        private readonly List<RoomInfo> _rooms = new();
        private readonly List<LobbyUserInfo> _lobbyUsers = new();
        private readonly List<KickPlayerInfo> _kickPlayers = new();

        private readonly Button _createRoomNextButton;
        private readonly Button _worldCreateButton;
        private readonly Button _lobbyStartButton;
        private readonly Button _lobbyBackButton;
        private readonly Button _multiplayerCreateButton;
        private readonly Button _multiplayerJoinButton;

        private TaskCompletionSource<bool> _overlayCompletionSource;
        private OverlayLoaderResult _overlayResult;
        private int _overlayLockCount;

        public HomeMenuMoyvaUiViewController(HomeMenuMoyvaUiState state,
            [Zenject.InjectOptional] Kruty1918.Moyva.UIActions.API.IUiHotkeyService hotkeys = null)
        {
            _state = state;
            Controls = new HomeMenuControlsEditor(state, this, hotkeys);
            _createRoomNextButton = CreateHiddenButton("MoyvaUI_CreateRoom_Next");
            _worldCreateButton = CreateHiddenButton("MoyvaUI_World_Create");
            _lobbyStartButton = CreateHiddenButton("MoyvaUI_Lobby_Start");
            _lobbyBackButton = CreateHiddenButton("MoyvaUI_Lobby_Back");
            _multiplayerCreateButton = CreateHiddenButton("MoyvaUI_Multiplayer_Create");
            _multiplayerJoinButton = CreateHiddenButton("MoyvaUI_Multiplayer_Join");

            RoomName = "Moyva Lobby";
            IsPublic = true;
            MaxPlayers = 4;
            WorldName = "New World";
            Seed = UnityEngine.Random.Range(100000, int.MaxValue);
            Size = WorldSize.Medium;
            MapType = MapType.Continents;
            Difficulty = Difficulty.Normal;
            SelectedMode = NetworkProviderType.Relay;
            SetDefaultGraphics();
        }

        public IReadOnlyList<GameSlotInfo> Slots => _slots;
        internal HomeMenuMoyvaUiState State => _state;
        public IReadOnlyList<RoomInfo> Rooms => _rooms;
        public IReadOnlyList<LobbyUserInfo> LobbyUsers => _lobbyUsers;
        public IReadOnlyList<KickPlayerInfo> KickPlayers => _kickPlayers;
        public string CreateRoomTitle { get; private set; } = "Create Lobby";
        public string CreateRoomSectionTitle { get; private set; } = "Room Settings";
        public string CreateRoomNextText { get; private set; } = "NEXT";
        public string InviteCodeText { get; private set; } = "Invite Code: N/A";
        public string LobbyDisplayName { get; private set; } = "Lobby";
        public string KickStatus { get; private set; } = string.Empty;
        public bool KickInteractable { get; private set; } = true;
        public bool SettingsInteractable { get; private set; } = true;
        public bool GraphicsSettingsInteractable { get; private set; } = true;
        public bool OverlayVisible => _overlayResult != null && _overlayResult.IsLoading;
        public float OverlayProgress => _overlayResult?.Progress ?? 0f;
        public string OverlaySuffix { get; private set; } = "%";
        public bool ConfirmationVisible { get; private set; }
        public ConfirmationRequest? CurrentConfirmation { get; private set; }
        public bool InfoVisible { get; private set; }
        public InfoMessage CurrentInfo { get; private set; }
        public bool PasswordVisible { get; private set; }
        public string PasswordRoomDisplayName { get; private set; } = string.Empty;
        public string PasswordErrorText { get; private set; } = string.Empty;
        public string PasswordValue { get; private set; } = string.Empty;

        public string RoomName { get; set; }
        public string Password { get; set; }
        public bool IsPublic { get; set; }
        public int MaxPlayers { get; set; }
        public Button NextButton => _createRoomNextButton;

        public string WorldName { get; set; }
        public int Seed { get; set; }
        public WorldSize Size { get; set; }
        public MapType MapType { get; set; }
        public Difficulty Difficulty { get; set; }
        public Button CreateWorldButton => _worldCreateButton;

        public string PlayerName { get; set; }
        public float MasterVolume { get; set; }
        public float MusicVolume { get; set; }
        public float SfxVolume { get; set; }
        public float UiVolume { get; set; }
        public bool IsMuted { get; set; }
        public GraphicsQualityProfile GraphicsProfile { get; set; }
        public int TargetFrameRate { get; set; }
        public float RenderScale { get; set; }
        public bool DynamicRenderScale { get; set; }
        public bool CloseZoomOptimization { get; set; }
        public int TextureMipmapLimit { get; set; }
        public int AntiAliasing { get; set; }
        public bool VSync { get; set; }
        public bool Shadows { get; set; }
        public bool AnisotropicFiltering { get; set; }
        public float LodBias { get; set; }
        public float MouseSensitivity { get; private set; } = 1f;
        public float MovementSpeed { get; private set; } = 1f;
        public float OrbitSpeed { get; private set; } = 1f;
        public float ZoomSpeed { get; private set; } = 1f;
        public HomeMenuControlsEditor Controls { get; }
        public IReadOnlyDictionary<PlayerControlAction, string> ControlBindings => _controlBindings;
        private readonly Dictionary<PlayerControlAction, string> _controlBindings = PlayerControlSettingsData.CreateDefault().Bindings;

        public string JoinCode { get; set; }
        public bool JoinInteractable { get; private set; } = true;
        public Button StartGameButton => _lobbyStartButton;
        public Button BackButton => _lobbyBackButton;
        public Button ButtonCreateRoom { get => _multiplayerCreateButton; set { } }
        public Button ButtonJoinToRoom { get => _multiplayerJoinButton; set { } }
        public NetworkProviderType SelectedMode { get; set; }
        public bool IsVisible => InfoVisible || PasswordVisible;
        bool IPasswordPanelViewController.IsVisible => PasswordVisible;
        bool IInfoPanelViewController.IsVisible => InfoVisible;

        public event Action<GameSlotInfo> OnSlotSelected;
        private event Action CreateRoomRequested;
        private event Action CreateWorldRequested;
        event Action ICreateRoomViewController.OnButtonNextClicked
        {
            add => CreateRoomRequested += value;
            remove => CreateRoomRequested -= value;
        }
        event Action IWorldSetupViewController.OnButtonNextClicked
        {
            add => CreateWorldRequested += value;
            remove => CreateWorldRequested -= value;
        }
        public event Action OnRandomSeedClicked;
        public event Action OnSettingsChanged;
        public event Action<string> OnPlayerNameChanged;
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;
        public event Action<float> OnUiVolumeChanged;
        public event Action<bool> OnMutedChanged;
        public event Action<GraphicsQualityProfile> OnGraphicsProfileChanged;
        public event Action<int> OnTargetFrameRateChanged;
        public event Action<float> OnRenderScaleChanged;
        public event Action<bool> OnDynamicRenderScaleChanged;
        public event Action<bool> OnCloseZoomOptimizationChanged;
        public event Action<int> OnTextureMipmapLimitChanged;
        public event Action<int> OnAntiAliasingChanged;
        public event Action<bool> OnVSyncChanged;
        public event Action<bool> OnShadowsChanged;
        public event Action<bool> OnAnisotropicFilteringChanged;
        public event Action<float> OnLodBiasChanged;
        public event Action OnResetGraphicsClicked;
        public event Action OnDeleteSavesClicked;
        public event Action<float> OnMouseSensitivityChanged;
        public event Action<float> OnMovementSpeedChanged;
        public event Action<float> OnOrbitSpeedChanged;
        public event Action<float> OnZoomSpeedChanged;
        public event Action<PlayerControlAction, string> OnControlBindingChanged;
        public event Action OnResetControlsClicked;
        public event Action OnAcknowledged;
        public event Action OnJoinRequested;
        public event Action OnJoinCodeChanged;
        public event Action OnListRoomsRefresh;
        public event Action<RoomInfo> OnRoomSelected;
        public event Action OnCloseRequested;
        public event Action OnRefreshRequested;
        public event Action<KickPlayerInfo> OnKickRequested;
        public event Action<NetworkProviderType> OnModeChanged;
        public event Action<NetworkProviderType> OnCreateRoomClicked;
        public event Action<NetworkProviderType> OnJoinRoomClicked;
        public event Action<string> OnConfirmed;
        public event Action OnCancelled;

        public Action OnConfirme { get; set; }
        public Action OnCancled { get; set; }

        public void Initialize() => _state.MarkDirty();

        public void Dispose()
        {
            StopOverlay(true);
            for (int i = 0; i < _ownedObjects.Count; i++)
            {
                if (_ownedObjects[i] == null)
                    continue;

                if (ShouldDestroyDeferred())
                    UnityEngine.Object.Destroy(_ownedObjects[i]);
                else
                    UnityEngine.Object.DestroyImmediate(_ownedObjects[i]);
            }

            _ownedObjects.Clear();
        }

        private static bool ShouldDestroyDeferred()
        {
#if UNITY_EDITOR
            return Application.isPlaying && UnityEditor.EditorApplication.isPlaying;
#else
            return Application.isPlaying;
#endif
        }

        public void AddSlot(GameSlotInfo slot)
        {
            RemoveSlot(slot.SlotName);
            _slots.Add(slot);
            _state.MarkDirty();
        }

        public void RemoveSlot(string slotName)
        {
            _slots.RemoveAll(s => string.Equals(s.SlotName, slotName, StringComparison.Ordinal));
            _state.MarkDirty();
        }

        public void ClearSlots()
        {
            _slots.Clear();
            _state.MarkDirty();
        }

        public void RefreshSlots()
        {
            _slots.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));
            _state.MarkDirty();
        }

        public void ApplyPresentation(CreateRoomPanelPresentation presentation)
        {
            if (!string.IsNullOrWhiteSpace(presentation.Title))
                CreateRoomTitle = presentation.Title;
            if (!string.IsNullOrWhiteSpace(presentation.SectionTitle))
                CreateRoomSectionTitle = presentation.SectionTitle;
            if (!string.IsNullOrWhiteSpace(presentation.NextButtonText))
                CreateRoomNextText = presentation.NextButtonText;
            _state.MarkDirty();
        }

        public void Refresh(LocalGameSettings settings)
        {
            PlayerName = settings.PlayerName;
            MasterVolume = settings.MasterVolume;
            MusicVolume = settings.MusicVolume;
            SfxVolume = settings.SfxVolume;
            UiVolume = settings.UiVolume;
            IsMuted = settings.IsMuted;
            _state.MarkDirty();
        }

        public void RefreshGraphics(GraphicsSettingsData settings)
        {
            GraphicsProfile = settings.Profile;
            TargetFrameRate = settings.TargetFrameRate;
            RenderScale = settings.RenderScale;
            DynamicRenderScale = settings.DynamicRenderScale;
            CloseZoomOptimization = settings.CloseZoomOptimization;
            TextureMipmapLimit = settings.TextureMipmapLimit;
            AntiAliasing = settings.AntiAliasing;
            VSync = settings.VSync;
            Shadows = settings.Shadows;
            AnisotropicFiltering = settings.AnisotropicFiltering;
            LodBias = settings.LodBias;
            _state.MarkDirty();
        }

        public void RefreshControls(PlayerControlSettingsData settings)
        {
            var normalized = settings.Normalized();
            MouseSensitivity = normalized.MouseSensitivity;
            MovementSpeed = normalized.MovementSpeed;
            OrbitSpeed = normalized.OrbitSpeed;
            ZoomSpeed = normalized.ZoomSpeed;
            _controlBindings.Clear();
            foreach (var pair in normalized.Bindings)
                _controlBindings[pair.Key] = pair.Value;
            _state.MarkDirty();
        }

        public void SetInteractable(bool interactable)
        {
            SettingsInteractable = interactable;
            _state.MarkDirty();
        }

        public void SetGraphicsInteractable(bool interactable)
        {
            GraphicsSettingsInteractable = interactable;
            _state.MarkDirty();
        }

        public void Show(InfoMessage message)
        {
            CurrentInfo = message;
            InfoVisible = true;
            _state.MarkDirty();
        }

        public void Hide()
        {
            InfoVisible = false;
            PasswordVisible = false;
            _state.MarkDirty();
        }

        public void AddRoomToList(RoomInfo room)
        {
            var key = BuildRoomKey(room);
            _rooms.RemoveAll(r => string.Equals(BuildRoomKey(r), key, StringComparison.Ordinal));
            _rooms.Add(room);
            _state.MarkDirty();
        }

        public void ClearRoomList()
        {
            _rooms.Clear();
            _state.MarkDirty();
        }

        public void RefreshRoomList()
        {
            OnListRoomsRefresh?.Invoke();
            _state.MarkDirty();
        }

        public void SetJoinInteractable(bool interactable)
        {
            JoinInteractable = interactable;
            _state.MarkDirty();
        }

        public void SetPlayers(IReadOnlyList<KickPlayerInfo> players)
        {
            _kickPlayers.Clear();
            if (players != null)
                _kickPlayers.AddRange(players);
            _state.MarkDirty();
        }

        public void ClearPlayers()
        {
            _kickPlayers.Clear();
            _state.MarkDirty();
        }

        public void SetStatus(string status)
        {
            KickStatus = status ?? string.Empty;
            _state.MarkDirty();
        }

        void IKickPlayerPanelViewController.SetInteractable(bool interactable)
        {
            KickInteractable = interactable;
            _state.MarkDirty();
        }

        public void SetInviteCode(LobbyInviteCodePresentation presentation)
        {
            InviteCodeText = presentation.DisplayText;
            if (!string.IsNullOrWhiteSpace(presentation.RoomName))
                LobbyDisplayName = presentation.RoomName;
            _state.MarkDirty();
        }

        public void ClearLobbyInvateCode()
        {
            InviteCodeText = "Invite Code: N/A";
            LobbyDisplayName = "Lobby";
            _state.MarkDirty();
        }

        public void AddNewUser(LobbyUserInfo userInfo)
        {
            _lobbyUsers.RemoveAll(u => u.UserId == userInfo.UserId);
            _lobbyUsers.Add(userInfo);
            _state.MarkDirty();
        }

        public void RemoveUser(int userId)
        {
            _lobbyUsers.RemoveAll(u => u.UserId == userId);
            _state.MarkDirty();
        }

        public void ClearUsers()
        {
            _lobbyUsers.Clear();
            _state.MarkDirty();
        }

        public void RefreshUserList()
        {
            _lobbyUsers.Sort((a, b) => a.UserId.CompareTo(b.UserId));
            _state.MarkDirty();
        }

        public void Refresh() => _state.MarkDirty();

        public void Show(string roomDisplayName, string errorText)
        {
            PasswordRoomDisplayName = roomDisplayName ?? string.Empty;
            PasswordErrorText = errorText ?? string.Empty;
            PasswordValue = string.Empty;
            PasswordVisible = true;
            _state.MarkDirty();
        }

        public void SetErrorText(string errorText)
        {
            PasswordErrorText = errorText ?? string.Empty;
            _state.MarkDirty();
        }

        public bool TryGetReqest(out ConfirmationRequest? request)
        {
            request = CurrentConfirmation;
            return ConfirmationVisible && request.HasValue;
        }

        public void Show(ConfirmationRequest request)
        {
            CurrentConfirmation = request;
            OnConfirme = request.OnConfirm;
            OnCancled = request.OnCancel;
            ConfirmationVisible = true;
            _state.MarkDirty();
        }

        public void ForeceHide()
        {
            ConfirmationVisible = false;
            CurrentConfirmation = null;
            _state.MarkDirty();
        }

        public OverlayLoaderResult LoadOverlay(float value, float maxValue = 100, string sufix = "%")
        {
            _overlayResult?.SetLoading(false, _overlayResult.Progress);
            _overlayCompletionSource?.TrySetResult(true);
            _overlayCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            OverlaySuffix = string.IsNullOrEmpty(sufix) ? "%" : sufix;
            var progress = maxValue <= 0f ? 0f : Mathf.Clamp01(value / maxValue) * 100f;
            _overlayResult = OverlayLoaderResult.Start(
                async () => await _overlayCompletionSource.Task.ConfigureAwait(false),
                null,
                _ => StopOverlay(true));
            _overlayResult.SetLoading(true, progress);
            _state.MarkDirty();
            return _overlayResult;
        }

        public void UpdateOverlay(float value, float maxValue = 100, string sufix = "%")
        {
            if (_overlayResult == null || !_overlayResult.IsLoading)
                return;

            OverlaySuffix = string.IsNullOrEmpty(sufix) ? "%" : sufix;
            var progress = maxValue <= 0f ? 0f : Mathf.Clamp01(value / maxValue) * 100f;
            _overlayResult.SetLoading(true, progress);
            _state.MarkDirty();
        }

        public void StopOverlay(bool forceImmediate = false)
        {
            if (_overlayLockCount > 0 && !forceImmediate)
                return;

            _overlayCompletionSource?.TrySetResult(true);
            _overlayCompletionSource = null;
            _overlayResult?.SetLoading(false, _overlayResult.Progress);
            _overlayResult = null;
            _state.MarkDirty();
        }

        public void LockOverlay() => _overlayLockCount++;

        public void UnlockOverlay() => _overlayLockCount = Math.Max(0, _overlayLockCount - 1);

        public void ClickCreateRoom() => InvokeButton(_createRoomNextButton, () => CreateRoomRequested?.Invoke());
        public void ClickCreateWorld() => InvokeButton(_worldCreateButton, () => CreateWorldRequested?.Invoke());
        public void ClickLobbyStart() => InvokeButton(_lobbyStartButton, null);
        public void ClickLobbyBack() => InvokeButton(_lobbyBackButton, null);
        public void SelectSlot(int index)
        {
            if (index < 0 || index >= _slots.Count)
                return;

            OnSlotSelected?.Invoke(_slots[index]);
            _state.MarkDirty();
        }

        public void SelectRoom(int index)
        {
            if (index < 0 || index >= _rooms.Count)
                return;

            var room = _rooms[index];
            JoinCode = room.HasJoinCode ? room.JoinCode : string.Empty;
            OnJoinCodeChanged?.Invoke();
            OnRoomSelected?.Invoke(room);
            _state.MarkDirty();
        }

        public void RequestJoin()
        {
            if (!JoinInteractable)
                return;

            OnJoinRequested?.Invoke();
            _state.MarkDirty();
        }

        public void RequestRoomRefresh()
        {
            OnListRoomsRefresh?.Invoke();
            _state.MarkDirty();
        }

        public void RequestKickRefresh()
        {
            OnRefreshRequested?.Invoke();
            _state.MarkDirty();
        }

        public void RequestKick(int index)
        {
            if (!KickInteractable || index < 0 || index >= _kickPlayers.Count)
                return;

            OnKickRequested?.Invoke(_kickPlayers[index]);
            _state.MarkDirty();
        }

        public void RequestKickClose()
        {
            OnCloseRequested?.Invoke();
            _state.MarkDirty();
        }

        public void ChooseMultiplayerCreate(NetworkProviderType provider)
        {
            SelectedMode = provider;
            OnModeChanged?.Invoke(provider);
            OnCreateRoomClicked?.Invoke(provider);
            _state.MarkDirty();
        }

        public void ChooseMultiplayerJoin(NetworkProviderType provider)
        {
            SelectedMode = provider;
            OnModeChanged?.Invoke(provider);
            OnJoinRoomClicked?.Invoke(provider);
            _state.MarkDirty();
        }

        public void BeginControlInteraction() => _state.BeginInteraction();

        public void EndControlInteraction() => _state.EndInteraction();

        public void SetSettingsSection(HomeMenuSettingsSection section) => _state.SetSettingsSection(section);

        public void SetPlayFlow(HomeMenuPlayFlow flow) => _state.SetPlayFlow(flow);

        public void SetMode(NetworkProviderType provider)
        {
            if (SelectedMode == provider)
                return;

            SelectedMode = provider;
            OnModeChanged?.Invoke(provider);
            _state.MarkDirty();
        }

        public void SetWorldSize(WorldSize size)
        {
            Size = size;
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        public void SetMapType(MapType mapType)
        {
            MapType = mapType;
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        public void SetDifficulty(Difficulty difficulty)
        {
            Difficulty = difficulty;
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        public void RandomizeSeed()
        {
            Seed = UnityEngine.Random.Range(100000, int.MaxValue);
            OnRandomSeedClicked?.Invoke();
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        public void ToggleRoomVisibility()
        {
            IsPublic = !IsPublic;
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        public void SetRoomPrivate(bool isPrivate)
        {
            var isPublic = !isPrivate;
            if (IsPublic == isPublic)
                return;

            IsPublic = isPublic;
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        public void IncreaseMaxPlayers()
        {
            MaxPlayers = Mathf.Clamp(MaxPlayers + 1, 2, 8);
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        public void DecreaseMaxPlayers()
        {
            MaxPlayers = Mathf.Clamp(MaxPlayers - 1, 2, 8);
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        public void SetRoomName(string value)
        {
            RoomName = NormalizeText(value, "Moyva Lobby");
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        public void PreviewRoomName(string value)
        {
            RoomName = value ?? string.Empty;
            RefreshCreateRoomInteractable();
        }

        public void SetRoomPassword(string value)
        {
            Password = value ?? string.Empty;
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        public void PreviewRoomPassword(string value)
        {
            Password = value ?? string.Empty;
            RefreshCreateRoomInteractable();
        }

        public void SetWorldName(string value)
        {
            WorldName = NormalizeText(value, "New World");
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        public void PreviewWorldName(string value) => WorldName = value ?? string.Empty;

        public void SetSeed(string value)
        {
            if (int.TryParse((value ?? string.Empty).Trim(), out var seed) && seed != 0)
            {
                Seed = seed;
                OnSettingsChanged?.Invoke();
                _state.MarkDirty();
            }
        }

        public void SetJoinCode(string value)
        {
            JoinCode = (value ?? string.Empty).Trim();
            OnJoinCodeChanged?.Invoke();
            _state.MarkDirty();
        }

        public void PreviewJoinCode(string value) => JoinCode = value ?? string.Empty;

        public void SetPlayerName(string value)
        {
            PlayerName = NormalizeText(value, "Player");
            OnPlayerNameChanged?.Invoke(PlayerName);
            _state.MarkDirty();
        }

        public void PreviewPlayerName(string value) => PlayerName = value ?? string.Empty;

        public void ChangePlayerName()
        {
            PlayerName = string.IsNullOrWhiteSpace(PlayerName)
                ? $"Player-{UnityEngine.Random.Range(1000, 9999)}"
                : $"{PlayerName.Trim()}*";
            OnPlayerNameChanged?.Invoke(PlayerName);
            _state.MarkDirty();
        }

        public void SetMaster(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(MasterVolume, value)) return;
            MasterVolume = value;
            OnMasterVolumeChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void SetMusic(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(MusicVolume, value)) return;
            MusicVolume = value;
            OnMusicVolumeChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void SetSfx(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(SfxVolume, value)) return;
            SfxVolume = value;
            OnSfxVolumeChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void SetUi(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(UiVolume, value)) return;
            UiVolume = value;
            OnUiVolumeChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void ToggleMuted() => SetMuted(!IsMuted);
        public void SetMuted(bool value)
        {
            if (IsMuted == value) return;
            IsMuted = value;
            OnMutedChanged?.Invoke(value);
            _state.MarkDirty();
        }
        public void SetGraphicsProfile(GraphicsQualityProfile profile)
        {
            if (GraphicsProfile == profile)
                return;

            GraphicsProfile = profile;
            OnGraphicsProfileChanged?.Invoke(profile);
            _state.MarkDirty();
        }
        public void SetRenderScale(float value)
        {
            value = Mathf.Clamp(value, 0.42f, 1f);
            if (Mathf.Approximately(RenderScale, value)) return;
            RenderScale = value;
            OnRenderScaleChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void SetFrameRate(int value)
        {
            value = Mathf.Clamp(value, 30, 360);
            if (TargetFrameRate == value) return;
            TargetFrameRate = value;
            OnTargetFrameRateChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void SetTextureMipmapLimit(int value)
        {
            value = Mathf.Clamp(value, 0, 3);
            if (TextureMipmapLimit == value) return;
            TextureMipmapLimit = value;
            OnTextureMipmapLimitChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void SetLodBias(float value)
        {
            value = Mathf.Clamp(Mathf.Round(value * 10f) / 10f, 0.4f, 2f);
            if (Mathf.Approximately(LodBias, value)) return;
            LodBias = value;
            OnLodBiasChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void ToggleDynamicRenderScale() => SetDynamicRenderScale(!DynamicRenderScale);
        public void SetDynamicRenderScale(bool value)
        {
            if (DynamicRenderScale == value) return;
            DynamicRenderScale = value;
            OnDynamicRenderScaleChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void ToggleCloseZoomOptimization() => SetCloseZoomOptimization(!CloseZoomOptimization);
        public void SetCloseZoomOptimization(bool value)
        {
            if (CloseZoomOptimization == value) return;
            CloseZoomOptimization = value;
            OnCloseZoomOptimizationChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void ToggleVSync() => SetVSync(!VSync);
        public void SetVSync(bool value)
        {
            if (VSync == value) return;
            VSync = value;
            OnVSyncChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void ToggleShadows() => SetShadows(!Shadows);
        public void SetShadows(bool value)
        {
            if (Shadows == value) return;
            Shadows = value;
            OnShadowsChanged?.Invoke(value);
            _state.MarkDirty();
        }

        public void ToggleAnisotropic() => SetAnisotropic(!AnisotropicFiltering);
        public void SetAnisotropic(bool value)
        {
            if (AnisotropicFiltering == value) return;
            AnisotropicFiltering = value;
            OnAnisotropicFilteringChanged?.Invoke(value);
            _state.MarkDirty();
        }
        public void AdjustRenderScale(float delta) { RenderScale = Mathf.Clamp(RenderScale + delta, 0.42f, 1f); OnRenderScaleChanged?.Invoke(RenderScale); _state.MarkDirty(); }
        public void AdjustFrameRate(int delta) { TargetFrameRate = Mathf.Clamp(TargetFrameRate + delta, 30, 360); OnTargetFrameRateChanged?.Invoke(TargetFrameRate); _state.MarkDirty(); }
        public void AdjustTextureMipmapLimit(int delta) { TextureMipmapLimit = Mathf.Clamp(TextureMipmapLimit + delta, 0, 3); OnTextureMipmapLimitChanged?.Invoke(TextureMipmapLimit); _state.MarkDirty(); }
        public void SetAntiAliasing(int value)
        {
            value = NormalizeAntiAliasing(value);
            if (AntiAliasing == value) return;
            AntiAliasing = value;
            OnAntiAliasingChanged?.Invoke(value);
            _state.MarkDirty();
        }
        public void AdjustLodBias(float delta) { LodBias = Mathf.Clamp(Mathf.Round((LodBias + delta) * 10f) / 10f, 0.4f, 2f); OnLodBiasChanged?.Invoke(LodBias); _state.MarkDirty(); }
        public void ResetGraphics() { OnResetGraphicsClicked?.Invoke(); _state.MarkDirty(); }
        public void DeleteSaves() { OnDeleteSavesClicked?.Invoke(); _state.MarkDirty(); }
        public void SetMouseSensitivity(float value) { MouseSensitivity = Mathf.Clamp(value, 0.25f, 3f); OnMouseSensitivityChanged?.Invoke(MouseSensitivity); _state.MarkDirty(); }
        public void SetMovementSpeed(float value) { MovementSpeed = Mathf.Clamp(value, 0.25f, 3f); OnMovementSpeedChanged?.Invoke(MovementSpeed); _state.MarkDirty(); }
        public void SetOrbitSpeed(float value) { OrbitSpeed = Mathf.Clamp(value, 0.25f, 3f); OnOrbitSpeedChanged?.Invoke(OrbitSpeed); _state.MarkDirty(); }
        public void SetZoomSpeed(float value) { ZoomSpeed = Mathf.Clamp(value, 0.25f, 3f); OnZoomSpeedChanged?.Invoke(ZoomSpeed); _state.MarkDirty(); }
        public void SetControlBinding(PlayerControlAction action, string controlPath) { OnControlBindingChanged?.Invoke(action, controlPath); _state.MarkDirty(); }
        public void ResetControls() { OnResetControlsClicked?.Invoke(); _state.MarkDirty(); }

        public void ConfirmModal()
        {
            var confirm = OnConfirme;
            ForeceHide();
            confirm?.Invoke();
        }

        public void CancelModal()
        {
            var cancel = OnCancled;
            ForeceHide();
            cancel?.Invoke();
        }

        public void AcknowledgeInfo()
        {
            var callback = CurrentInfo.OnAcknowledged;
            InfoVisible = false;
            OnAcknowledged?.Invoke();
            callback?.Invoke();
            _state.MarkDirty();
        }

        public void ConfirmPassword()
        {
            PasswordVisible = false;
            OnConfirmed?.Invoke(PasswordValue ?? string.Empty);
            _state.MarkDirty();
        }

        public void CancelPassword()
        {
            PasswordVisible = false;
            OnCancelled?.Invoke();
            _state.MarkDirty();
        }

        public void SetPasswordPreset(string value)
        {
            PasswordValue = value ?? string.Empty;
            _state.MarkDirty();
        }

        public void SetPasswordValue(string value)
        {
            PasswordValue = value ?? string.Empty;
            _state.MarkDirty();
        }

        public void PreviewPasswordValue(string value) => PasswordValue = value ?? string.Empty;

        private void InvokeButton(Button button, Action fallback)
        {
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
                fallback?.Invoke();
            }

            _state.MarkDirty();
        }

        private Button CreateHiddenButton(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            var image = go.GetComponent<Image>();
            image.color = Color.clear;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            _ownedObjects.Add(go);
            return button;
        }

        private void SetDefaultGraphics()
        {
            PlayerName = "Player";
            MasterVolume = 1f;
            MusicVolume = 0.7f;
            SfxVolume = 0.9f;
            UiVolume = 0.9f;
            GraphicsProfile = GraphicsQualityProfile.Auto;
            TargetFrameRate = 60;
            RenderScale = 1f;
            DynamicRenderScale = false;
            CloseZoomOptimization = false;
            TextureMipmapLimit = 0;
            AntiAliasing = 0;
            VSync = false;
            AnisotropicFiltering = true;
            Shadows = true;
            LodBias = 1f;
        }

        private void RefreshCreateRoomInteractable()
        {
            if (_createRoomNextButton != null)
                _createRoomNextButton.interactable = !string.IsNullOrWhiteSpace(RoomName) &&
                    (IsPublic || !string.IsNullOrEmpty(Password));
        }

        private static string NormalizeText(string value, string fallback)
        {
            var normalized = (value ?? string.Empty).Trim();
            return string.IsNullOrWhiteSpace(normalized) ? fallback : normalized;
        }

        private static int NormalizeAntiAliasing(int value)
        {
            if (value >= 4)
                return 4;

            if (value >= 2)
                return 2;

            return 0;
        }

        private static string BuildRoomKey(RoomInfo room)
        {
            var key = room.DisplayKey;
            if (string.IsNullOrWhiteSpace(key))
                key = $"{room.RoomName}:{room.MaxPlayers}";
            return $"{room.ProviderType}:{key}";
        }
    }
}
