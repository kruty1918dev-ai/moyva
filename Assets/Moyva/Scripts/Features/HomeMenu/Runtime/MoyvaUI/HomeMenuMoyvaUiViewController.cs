using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Networking;
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

        public HomeMenuMoyvaUiViewController(HomeMenuMoyvaUiState state)
        {
            _state = state;
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
        public IReadOnlyList<RoomInfo> Rooms => _rooms;
        public IReadOnlyList<LobbyUserInfo> LobbyUsers => _lobbyUsers;
        public IReadOnlyList<KickPlayerInfo> KickPlayers => _kickPlayers;
        public string CreateRoomTitle { get; private set; } = "Create Lobby";
        public string CreateRoomSectionTitle { get; private set; } = "Room Settings";
        public string CreateRoomNextText { get; private set; } = "Create Lobby";
        public string InviteCodeText { get; private set; } = "Invite Code: N/A";
        public string KickStatus { get; private set; } = string.Empty;
        public bool KickInteractable { get; private set; } = true;
        public bool SettingsInteractable { get; private set; } = true;
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
        public event Action OnButtonNextClicked;
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

        public void SetInteractable(bool interactable)
        {
            SettingsInteractable = interactable;
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
            _state.MarkDirty();
        }

        public void ClearLobbyInvateCode()
        {
            InviteCodeText = "Invite Code: N/A";
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

        public void ClickCreateRoom() => InvokeButton(_createRoomNextButton, null);
        public void ClickCreateWorld() => InvokeButton(_worldCreateButton, null);
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
            _state.MarkDirty();
        }

        public void IncreaseMaxPlayers()
        {
            MaxPlayers = Mathf.Clamp(MaxPlayers + 1, 1, 8);
            _state.MarkDirty();
        }

        public void DecreaseMaxPlayers()
        {
            MaxPlayers = Mathf.Clamp(MaxPlayers - 1, 1, 8);
            _state.MarkDirty();
        }

        public void ChangePlayerName()
        {
            PlayerName = string.IsNullOrWhiteSpace(PlayerName)
                ? $"Player-{UnityEngine.Random.Range(1000, 9999)}"
                : $"{PlayerName.Trim()}*";
            OnPlayerNameChanged?.Invoke(PlayerName);
            _state.MarkDirty();
        }

        public void SetMaster(float value) { MasterVolume = Mathf.Clamp01(value); OnMasterVolumeChanged?.Invoke(MasterVolume); _state.MarkDirty(); }
        public void SetMusic(float value) { MusicVolume = Mathf.Clamp01(value); OnMusicVolumeChanged?.Invoke(MusicVolume); _state.MarkDirty(); }
        public void SetSfx(float value) { SfxVolume = Mathf.Clamp01(value); OnSfxVolumeChanged?.Invoke(SfxVolume); _state.MarkDirty(); }
        public void SetUi(float value) { UiVolume = Mathf.Clamp01(value); OnUiVolumeChanged?.Invoke(UiVolume); _state.MarkDirty(); }
        public void ToggleMuted() { IsMuted = !IsMuted; OnMutedChanged?.Invoke(IsMuted); _state.MarkDirty(); }
        public void SetGraphicsProfile(GraphicsQualityProfile profile) { GraphicsProfile = profile; OnGraphicsProfileChanged?.Invoke(profile); _state.MarkDirty(); }
        public void ToggleVSync() { VSync = !VSync; OnVSyncChanged?.Invoke(VSync); _state.MarkDirty(); }
        public void ToggleShadows() { Shadows = !Shadows; OnShadowsChanged?.Invoke(Shadows); _state.MarkDirty(); }
        public void ToggleAnisotropic() { AnisotropicFiltering = !AnisotropicFiltering; OnAnisotropicFilteringChanged?.Invoke(AnisotropicFiltering); _state.MarkDirty(); }
        public void AdjustRenderScale(float delta) { RenderScale = Mathf.Clamp(RenderScale + delta, 0.42f, 1f); OnRenderScaleChanged?.Invoke(RenderScale); _state.MarkDirty(); }
        public void AdjustFrameRate(int delta) { TargetFrameRate = Mathf.Clamp(TargetFrameRate + delta, 30, 360); OnTargetFrameRateChanged?.Invoke(TargetFrameRate); _state.MarkDirty(); }
        public void ResetGraphics() { OnResetGraphicsClicked?.Invoke(); _state.MarkDirty(); }
        public void DeleteSaves() { OnDeleteSavesClicked?.Invoke(); _state.MarkDirty(); }

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

        private void InvokeButton(Button button, Action fallback)
        {
            if (button != null && button.interactable)
                button.onClick.Invoke();
            else
                fallback?.Invoke();

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
            AnisotropicFiltering = true;
            Shadows = true;
            LodBias = 1f;
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
