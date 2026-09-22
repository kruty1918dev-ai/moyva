using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Shared.Controls;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Localization;
using Kruty1918.UiFoundation;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>HomeMenuMoyvaUiViewController — class: головного меню Moyva UI виду контролера.</summary>
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
        IRoomListStatusView,
        ILobbyStatusView
    {
        private readonly HomeMenuMoyvaUiState _state;
        private readonly ILocalizationService _localization;
        private readonly IUiMotionService _uiMotion;
        private readonly IPlayerControlSettingsService _controlSettings;
        private readonly List<GameObject> _ownedObjects = new();
        private readonly List<GameSlotInfo> _slots = new();
        private readonly List<RoomInfo> _rooms = new();
        private readonly List<LobbyUserInfo> _lobbyUsers = new();
        private readonly List<KickPlayerInfo> _kickPlayers = new();
        private readonly BotDifficultyRegistry _botDifficulties;

        private readonly Button _createRoomNextButton;
        private readonly Button _worldCreateButton;
        private readonly Button _lobbyStartButton;
        private readonly Button _lobbyBackButton;
        private readonly Button _multiplayerCreateButton;
        private readonly Button _multiplayerJoinButton;

        /// <summary>Виконує HomeMenuMoyvaUiViewController.</summary>
        public HomeMenuMoyvaUiViewController(HomeMenuMoyvaUiState state,
            [Zenject.InjectOptional] Kruty1918.UIActions.API.IUiHotkeyService hotkeys = null,
            [Zenject.InjectOptional] IPlayerControlSettingsService controlSettings = null,
            [Zenject.InjectOptional] IInputDeviceContext devices = null,
            [Zenject.InjectOptional] ILocalizationService localization = null,
            [Zenject.InjectOptional] IUiMotionService uiMotion = null,
            [Zenject.InjectOptional] ILobbyFlowContext lobbyFlowContext = null)
        {
            _state = state;
            _localization = localization;
            _uiMotion = uiMotion;
            _controlSettings = controlSettings;
            FlowContext = lobbyFlowContext;
            Controls = new HomeMenuControlsEditor(state, this, hotkeys, controlSettings, devices);
            _botDifficulties = BotDifficultyRegistry.Load();
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
            SelectedBotDifficultyId = _botDifficulties.Select(null).id;
            SelectedMode = NetworkProviderType.Relay;
            SetDefaultGraphics();
        }

        /// <summary>Слоти списку збережень.</summary>
        public IReadOnlyList<GameSlotInfo> Slots => _slots;
        internal HomeMenuMoyvaUiState State => _state;
        /// <summary>Список кімнат лобі.</summary>
        public IReadOnlyList<RoomInfo> Rooms => _rooms;
        /// <summary>лобі Users — IReadOnlyList<LobbyUserInfo>.</summary>
        public IReadOnlyList<LobbyUserInfo> LobbyUsers => _lobbyUsers;
        /// <summary>викиду гравців — IReadOnlyList<KickPlayerInfo>.</summary>
        public IReadOnlyList<KickPlayerInfo> KickPlayers => _kickPlayers;
        /// <summary>створення кімнати заголовка — string.</summary>
        public string CreateRoomTitle { get; private set; } = "Create Lobby";
        /// <summary>створення кімнати секції заголовка — string.</summary>
        public string CreateRoomSectionTitle { get; private set; } = "Room Settings";
        /// <summary>створення кімнати наступного тексту — string.</summary>
        public string CreateRoomNextText { get; private set; } = "NEXT";
        /// <summary>запрошення код тексту — string.</summary>
        public string InviteCodeText { get; private set; } = "Invite Code: N/A";
        /// <summary>лобі Display назву — string.</summary>
        public string LobbyDisplayName { get; private set; } = "Lobby";
        /// <summary>викиду статусу — string.</summary>
        public string KickStatus { get; private set; } = string.Empty;
        /// <summary>викиду інтерактивності — bool.</summary>
        public bool KickInteractable { get; private set; } = true;
        /// <summary>налаштування інтерактивності — bool.</summary>
        public bool SettingsInteractable { get; private set; } = true;
        /// <summary>графіки налаштування інтерактивності — bool.</summary>
        public bool GraphicsSettingsInteractable { get; private set; } = true;
        // Busy-overlay read model. Mutated only by HomeMenuBusyOverlayService via
        // ApplyOverlayPresentation — the view never touches OverlayLoaderResult,
        // tasks or locks itself.
        /// <summary>накладання видимої — bool.</summary>
        public bool OverlayVisible { get; private set; }
        /// <summary>накладання прогресу — float.</summary>
        public float OverlayProgress { get; private set; }
        /// <summary>накладання Suffix — string.</summary>
        public string OverlaySuffix { get; private set; } = "%";
        /// <summary>накладання статусу — string.</summary>
        public string OverlayStatus { get; private set; } = string.Empty;
        /// <summary>Flow контексту — ILobbyFlowContext.</summary>
        public ILobbyFlowContext FlowContext { get; }
        /// <summary>лобі статусу — LobbyStatusInfo.</summary>
        public LobbyStatusInfo LobbyStatus { get; private set; }
        /// <summary>кімнати список стану — RoomListStatus.</summary>
        public RoomListStatus RoomListState { get; private set; } = RoomListStatus.Empty;
        /// <summary>кімнати список повідомлення — string.</summary>
        public string RoomListMessage { get; private set; } = "Open this panel to browse available rooms.";
        /// <summary>запрошення код значення — string.</summary>
        public string InviteCodeValue { get; private set; } = string.Empty;
        /// <summary>запрошення Copied — bool.</summary>
        public bool InviteCopied { get; private set; }
        /// <summary>створення кімнати Block причини — string.</summary>
        public string CreateRoomBlockReason { get; private set; } = string.Empty;
        /// <summary>Reduced руху — bool.</summary>
        public bool ReducedMotion => _uiMotion?.ReducedMotion ?? false;
        /// <summary>Confirmation видимої — bool.</summary>
        public bool ConfirmationVisible { get; private set; }
        /// <summary>поточного Confirmation — ConfirmationRequest?.</summary>
        public ConfirmationRequest? CurrentConfirmation { get; private set; }
        /// <summary>інформації видимої — bool.</summary>
        public bool InfoVisible { get; private set; }
        /// <summary>поточного інформації — InfoMessage.</summary>
        public InfoMessage CurrentInfo { get; private set; }
        /// <summary>пароль видимої — bool.</summary>
        public bool PasswordVisible { get; private set; }
        /// <summary>пароль кімнати Display назву — string.</summary>
        public string PasswordRoomDisplayName { get; private set; } = string.Empty;
        /// <summary>пароль помилки тексту — string.</summary>
        public string PasswordErrorText { get; private set; } = string.Empty;
        /// <summary>пароль значення — string.</summary>
        public string PasswordValue { get; private set; } = string.Empty;

        /// <summary>кімнати назву — string.</summary>
        public string RoomName { get; set; }
        /// <summary>пароль — string.</summary>
        public string Password { get; set; }
        /// <summary>Чи публічної — IsPublic.</summary>
        public bool IsPublic { get; set; }
        /// <summary>максимум гравців — int.</summary>
        public int MaxPlayers { get; set; }
        /// <summary>наступного кнопки — Button.</summary>
        public Button NextButton => _createRoomNextButton;

        /// <summary>світу назву — string.</summary>
        public string WorldName { get; set; }
        /// <summary>Сетер властивості.</summary>
        public int Seed { get; set; }
        /// <summary>розміру — WorldSize.</summary>
        public WorldSize Size { get; set; }
        /// <summary>карти Type — MapType.</summary>
        public MapType MapType { get; set; }
        /// <summary>складності — Difficulty.</summary>
        public Difficulty Difficulty { get; set; }
        /// <summary>вибраного Bot складності ID — string.</summary>
        public string SelectedBotDifficultyId { get; private set; }
        /// <summary>вибраного Bot складності індексу — int.</summary>
        public int SelectedBotDifficultyIndex
        {
            get
            {
                for (var i = 0; i < _botDifficulties.difficulties.Length; i++)
                    if (string.Equals(_botDifficulties.difficulties[i].id, SelectedBotDifficultyId, StringComparison.OrdinalIgnoreCase))
                        return i;
                return 0;
            }
        }
        /// <summary>Bot складності Options — string.</summary>
        public string BotDifficultyOptions => string.Join("|", _botDifficulties.difficulties.Select(d => d.displayName));
        /// <summary>створення світу кнопки — Button.</summary>
        public Button CreateWorldButton => _worldCreateButton;

        /// <summary>гравця назву — string.</summary>
        public string PlayerName { get; set; }
        /// <summary>Master гучність — float.</summary>
        public float MasterVolume { get; set; }
        /// <summary>музики гучність — float.</summary>
        public float MusicVolume { get; set; }
        /// <summary>Sfx гучність — float.</summary>
        public float SfxVolume { get; set; }
        /// <summary>UI гучність — float.</summary>
        public float UiVolume { get; set; }
        /// <summary>Ambience гучність — float.</summary>
        public float AmbienceVolume { get; set; }
        /// <summary>Чи заглушеної — IsMuted.</summary>
        public bool IsMuted { get; set; }
        /// <summary>графіки профілю — GraphicsQualityProfile.</summary>
        public GraphicsQualityProfile GraphicsProfile { get; set; }
        /// <summary>цілі кадру частоти — int.</summary>
        public int TargetFrameRate { get; set; }
        /// <summary>рендер масштаб — float.</summary>
        public float RenderScale { get; set; }
        /// <summary>динамічного рендер масштаб — bool.</summary>
        public bool DynamicRenderScale { get; set; }
        /// <summary>закриття зум оптимізації — bool.</summary>
        public bool CloseZoomOptimization { get; set; }
        /// <summary>текстур mipmap ліміту — int.</summary>
        public int TextureMipmapLimit { get; set; }
        /// <summary>Сетер властивості.</summary>
        public int AntiAliasing { get; set; }
        /// <summary>V синхронізації — bool.</summary>
        public bool VSync { get; set; }
        /// <summary>тіней — bool.</summary>
        public bool Shadows { get; set; }
        /// <summary>анізотропної Filtering — bool.</summary>
        public bool AnisotropicFiltering { get; set; }
        /// <summary>LOD зсув — float.</summary>
        public float LodBias { get; set; }
        /// <summary>миші чутливість — float.</summary>
        public float MouseSensitivity { get; private set; } = 1f;
        /// <summary>руху швидкість — float.</summary>
        public float MovementSpeed { get; private set; } = 1f;
        /// <summary>орбіти швидкість — float.</summary>
        public float OrbitSpeed { get; private set; } = 1f;
        /// <summary>зум швидкість — float.</summary>
        public float ZoomSpeed { get; private set; } = 1f;
        /// <summary>камери ефектів — bool.</summary>
        public bool CameraEffects { get; private set; } = true;
        /// <summary>камери тряски Intensity — float.</summary>
        public float CameraShakeIntensity { get; private set; } = 0.5f;
        /// <summary>плавного камери фокус — bool.</summary>
        public bool SmoothCameraFocus { get; private set; } = true;
        /// <summary>автоматичного камери фокус — bool.</summary>
        public bool AutomaticCameraFocus { get; private set; }
        /// <summary>Reduce камери руху — bool.</summary>
        public bool ReduceCameraMotion { get; private set; }
        /// <summary>зум Toward пальців — bool.</summary>
        public bool ZoomTowardFingers { get; private set; } = true;
        /// <summary>Гетер властивості.</summary>
        public HomeMenuControlsEditor Controls { get; }
        /// <summary>гравця керування Action.</summary>
        public IReadOnlyDictionary<PlayerControlAction, string> ControlBindings => _controlBindings;
        private readonly Dictionary<PlayerControlAction, string> _controlBindings = PlayerControlSettingsData.CreateDefault().Bindings;

        /// <summary>приєднання код — string.</summary>
        public string JoinCode { get; set; }
        /// <summary>приєднання інтерактивності — bool.</summary>
        public bool JoinInteractable { get; private set; } = true;
        /// <summary>початку гри кнопки — Button.</summary>
        public Button StartGameButton => _lobbyStartButton;
        /// <summary>заднього кнопки — Button.</summary>
        public Button BackButton => _lobbyBackButton;
        /// <summary>кнопки створення кімнати — Button.</summary>
        public Button ButtonCreateRoom { get => _multiplayerCreateButton; set { } }
        /// <summary>кнопки приєднання  кімнати — Button.</summary>
        public Button ButtonJoinToRoom { get => _multiplayerJoinButton; set { } }
        /// <summary>вибраного режим — NetworkProviderType.</summary>
        public NetworkProviderType SelectedMode { get; set; }
        /// <summary>Чи видимої — IsVisible.</summary>
        public bool IsVisible => InfoVisible || PasswordVisible;
        bool IPasswordPanelViewController.IsVisible => PasswordVisible;
        bool IInfoPanelViewController.IsVisible => InfoVisible;

        /// <summary>On слота вибраного.</summary>
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
        /// <summary>On Random Seed натискання.</summary>
        public event Action OnRandomSeedClicked;
        /// <summary>On налаштування Changed.</summary>
        public event Action OnSettingsChanged;
        /// <summary>On гравця назву Changed.</summary>
        public event Action<string> OnPlayerNameChanged;
        /// <summary>On Master гучність Changed.</summary>
        public event Action<float> OnMasterVolumeChanged;
        /// <summary>On музики гучність Changed.</summary>
        public event Action<float> OnMusicVolumeChanged;
        /// <summary>On Sfx гучність Changed.</summary>
        public event Action<float> OnSfxVolumeChanged;
        /// <summary>On UI гучність Changed.</summary>
        public event Action<float> OnUiVolumeChanged;
        /// <summary>On Ambience гучність Changed.</summary>
        public event Action<float> OnAmbienceVolumeChanged;
        /// <summary>On заглушеної Changed.</summary>
        public event Action<bool> OnMutedChanged;
        /// <summary>On графіки профілю Changed.</summary>
        public event Action<GraphicsQualityProfile> OnGraphicsProfileChanged;
        /// <summary>On цілі кадру частоти Changed.</summary>
        public event Action<int> OnTargetFrameRateChanged;
        /// <summary>On рендер масштаб Changed.</summary>
        public event Action<float> OnRenderScaleChanged;
        /// <summary>On динамічного рендер масштаб Changed.</summary>
        public event Action<bool> OnDynamicRenderScaleChanged;
        /// <summary>On закриття зум оптимізації Changed.</summary>
        public event Action<bool> OnCloseZoomOptimizationChanged;
        /// <summary>On текстур mipmap ліміту Changed.</summary>
        public event Action<int> OnTextureMipmapLimitChanged;
        /// <summary>Обробляє зміну налаштування згладжування.</summary>
        public event Action<int> OnAntiAliasingChanged;
        /// <summary>On V синхронізації Changed.</summary>
        public event Action<bool> OnVSyncChanged;
        /// <summary>On тіней Changed.</summary>
        public event Action<bool> OnShadowsChanged;
        /// <summary>On анізотропної Filtering Changed.</summary>
        public event Action<bool> OnAnisotropicFilteringChanged;
        /// <summary>On LOD зсув Changed.</summary>
        public event Action<float> OnLodBiasChanged;
        /// <summary>On скидання графіки натискання.</summary>
        public event Action OnResetGraphicsClicked;
        /// <summary>On Delete Saves натискання.</summary>
        public event Action OnDeleteSavesClicked;
        /// <summary>On миші чутливість Changed.</summary>
        public event Action<float> OnMouseSensitivityChanged;
        /// <summary>On руху швидкість Changed.</summary>
        public event Action<float> OnMovementSpeedChanged;
        /// <summary>On орбіти швидкість Changed.</summary>
        public event Action<float> OnOrbitSpeedChanged;
        /// <summary>On зум швидкість Changed.</summary>
        public event Action<float> OnZoomSpeedChanged;
        /// <summary>On камери ефектів Changed.</summary>
        public event Action<bool> OnCameraEffectsChanged;
        /// <summary>On камери тряски Intensity Changed.</summary>
        public event Action<float> OnCameraShakeIntensityChanged;
        /// <summary>On плавного камери фокус Changed.</summary>
        public event Action<bool> OnSmoothCameraFocusChanged;
        /// <summary>On автоматичного камери фокус Changed.</summary>
        public event Action<bool> OnAutomaticCameraFocusChanged;
        /// <summary>On Reduce камери руху Changed.</summary>
        public event Action<bool> OnReduceCameraMotionChanged;
        /// <summary>On зум Toward пальців Changed.</summary>
        public event Action<bool> OnZoomTowardFingersChanged;
        /// <summary>гравця керування Action.</summary>
        public event Action<PlayerControlAction, string> OnControlBindingChanged;
        /// <summary>On скидання Controls натискання.</summary>
        public event Action OnResetControlsClicked;
        /// <summary>Обробляє підтвердження інформаційного повідомлення.</summary>
        public event Action OnAcknowledged;
        /// <summary>On приєднання запитаного.</summary>
        public event Action OnJoinRequested;
        /// <summary>On приєднання код Changed.</summary>
        public event Action OnJoinCodeChanged;
        /// <summary>On список Rooms Refresh.</summary>
        public event Action OnListRoomsRefresh;
        /// <summary>On кімнати вибраного.</summary>
        public event Action<RoomInfo> OnRoomSelected;
        /// <summary>On закриття запитаного.</summary>
        public event Action OnCloseRequested;
        /// <summary>On Refresh запитаного.</summary>
        public event Action OnRefreshRequested;
        /// <summary>On викиду запитаного.</summary>
        public event Action<KickPlayerInfo> OnKickRequested;
        /// <summary>On режим Changed.</summary>
        public event Action<NetworkProviderType> OnModeChanged;
        /// <summary>On створення кімнати натискання.</summary>
        public event Action<NetworkProviderType> OnCreateRoomClicked;
        /// <summary>On приєднання кімнати натискання.</summary>
        public event Action<NetworkProviderType> OnJoinRoomClicked;
        /// <summary>Обробляє підтвердження дії користувачем.</summary>
        public event Action<string> OnConfirmed;
        /// <summary>On скасування.</summary>
        public event Action OnCancelled;
        /// <summary>Спрацьовує коли користувач обирає мову в Settings (index у SupportedLanguages).</summary>
        public event Action<int> OnLanguageSelected;

        /// <summary>Сетер властивості.</summary>
        public Action OnConfirme { get; set; }
        /// <summary>Сетер властивості.</summary>
        public Action OnCancled { get; set; }

        /// <summary>Ініціалізує компонент і підписує на події.</summary>
        public void Initialize()
        {
            if (_localization != null)
                _localization.LanguageChanged += OnLanguageChanged;
            if (_controlSettings != null)
                _controlSettings.OnSettingsChanged += OnControlSettingsChanged;
            _state.SetReducedMotion(ReducedMotion);
            _state.MarkDirty();
        }

        private void OnControlSettingsChanged(PlayerControlSettingsData data)
            => _state.SetReducedMotion(data.ReduceMotion);

        private void OnLanguageChanged() => _state.MarkDirty();

        /// <summary>Локалізує статичний текст; без сервісу повертає source.</summary>
        internal string T(string key) => _localization?.T(key) ?? key ?? string.Empty;

        /// <summary>Локалізує й форматує {0}..{n} плейсхолдери.</summary>
        internal string TF(string key, params object[] args) =>
            _localization?.TF(key, args) ?? key ?? string.Empty;

        /// <summary>Локалізує pipe-delimited options для &lt;select&gt; компонентів.</summary>
        internal string TOptions(string pipeDelimitedOptions)
        {
            if (string.IsNullOrEmpty(pipeDelimitedOptions) || _localization == null)
                return pipeDelimitedOptions ?? string.Empty;
            var parts = pipeDelimitedOptions.Split('|');
            for (var i = 0; i < parts.Length; i++)
                parts[i] = _localization.T(parts[i]);
            return string.Join("|", parts);
        }

        /// <summary>Pipe-delimited список мов для select у Settings.</summary>
        public string LanguageOptions
        {
            get
            {
                if (_localization == null) return string.Empty;
                return string.Join("|", _localization.SupportedLanguages.Select(l => l.DisplayName));
            }
        }

        /// <summary>Language індексу — int.</summary>
        public int LanguageIndex => _localization?.CurrentLanguageIndex ?? 0;

        /// <summary>Встановлює Language індексу.</summary>
        public void SetLanguageIndex(int index)
        {
            if (_localization == null ||
                index < 0 || index >= _localization.SupportedLanguages.Count ||
                index == _localization.CurrentLanguageIndex)
                return;
            OnLanguageSelected?.Invoke(index);
        }

        /// <summary>Звільняє ресурси та відписує від подій.</summary>
        public void Dispose()
        {
            if (_localization != null)
                _localization.LanguageChanged -= OnLanguageChanged;
            if (_controlSettings != null)
                _controlSettings.OnSettingsChanged -= OnControlSettingsChanged;
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

        /// <summary>
        /// Single mutation path for the busy-overlay read model; called by
        /// <see cref="HomeMenuBusyOverlayService"/> whenever the async side changes.
        /// </summary>
        internal void ApplyOverlayPresentation(bool visible, float progress, string suffix, string status)
        {
            OverlayVisible = visible;
            OverlayProgress = progress;
            OverlaySuffix = string.IsNullOrEmpty(suffix) ? "%" : suffix;
            OverlayStatus = status ?? string.Empty;
            _state.MarkDirty();
        }

        /// <summary>Встановлює кімнати список статусу.</summary>
        public void SetRoomListStatus(RoomListStatus status, string message)
        {
            RoomListState = status;
            RoomListMessage = message ?? string.Empty;
            _state.MarkDirty();
        }

        /// <summary>Встановлює лобі статусу.</summary>
        public void SetLobbyStatus(LobbyStatusInfo status)
        {
            LobbyStatus = status;
            _state.MarkDirty();
        }

        /// <summary>Встановлює Reduced руху.</summary>
        public void SetReducedMotion(bool reduced)
        {
            if (_uiMotion != null)
                _uiMotion.ReducedMotion = reduced;
            _state.SetReducedMotion(reduced);
            _state.MarkDirty();
        }

        /// <summary>Копіює запрошення код.</summary>
        public void CopyInviteCode()
        {
            if (string.IsNullOrWhiteSpace(InviteCodeValue))
                return;

            GUIUtility.systemCopyBuffer = InviteCodeValue;
            InviteCopied = true;
            _state.MarkDirty();
        }

        private static bool ShouldDestroyDeferred()
        {
#if UNITY_EDITOR
            return Application.isPlaying && UnityEditor.EditorApplication.isPlaying;
#else
            return Application.isPlaying;
#endif
        }

        /// <summary>Додає слота.</summary>
        public void AddSlot(GameSlotInfo slot)
        {
            RemoveSlot(slot.SlotName);
            _slots.Add(slot);
            _state.MarkDirty();
        }

        /// <summary>Видаляє слота.</summary>
        public void RemoveSlot(string slotName)
        {
            _slots.RemoveAll(s => string.Equals(s.SlotName, slotName, StringComparison.Ordinal));
            _state.MarkDirty();
        }

        /// <summary>Очищує Slots.</summary>
        public void ClearSlots()
        {
            _slots.Clear();
            _state.MarkDirty();
        }

        /// <summary>Оновлює Slots.</summary>
        public void RefreshSlots()
        {
            _slots.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));
            _state.MarkDirty();
        }

        /// <summary>Застосовує Presentation.</summary>
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

        /// <summary>Оновлює Refresh.</summary>
        public void Refresh(LocalGameSettings settings)
        {
            PlayerName = settings.PlayerName;
            MasterVolume = settings.MasterVolume;
            MusicVolume = settings.MusicVolume;
            SfxVolume = settings.SfxVolume;
            UiVolume = settings.UiVolume;
            AmbienceVolume = settings.AmbienceVolume;
            IsMuted = settings.IsMuted;
            _state.MarkDirty();
        }

        /// <summary>Оновлює графіки.</summary>
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

        /// <summary>Оновлює Controls.</summary>
        public void RefreshControls(PlayerControlSettingsData settings)
        {
            var normalized = settings.Normalized();
            MouseSensitivity = normalized.MouseSensitivity;
            MovementSpeed = normalized.MovementSpeed;
            OrbitSpeed = normalized.OrbitSpeed;
            ZoomSpeed = normalized.ZoomSpeed;
            CameraEffects = normalized.CameraEffects;
            CameraShakeIntensity = normalized.CameraShakeIntensity;
            SmoothCameraFocus = normalized.SmoothCameraFocus;
            AutomaticCameraFocus = normalized.AutomaticCameraFocus;
            ReduceCameraMotion = normalized.ReduceCameraMotion;
            ZoomTowardFingers = normalized.ZoomTowardFingers;
            _controlBindings.Clear();
            foreach (var pair in normalized.Bindings)
                _controlBindings[pair.Key] = pair.Value;
            _state.MarkDirty();
        }

        /// <summary>Встановлює інтерактивності.</summary>
        public void SetInteractable(bool interactable)
        {
            SettingsInteractable = interactable;
            _state.MarkDirty();
        }

        /// <summary>Встановлює графіки інтерактивності.</summary>
        public void SetGraphicsInteractable(bool interactable)
        {
            GraphicsSettingsInteractable = interactable;
            _state.MarkDirty();
        }

        /// <summary>Показує Show.</summary>
        public void Show(InfoMessage message)
        {
            CurrentInfo = message;
            InfoVisible = true;
            _state.MarkDirty();
        }

        /// <summary>Ховає Hide.</summary>
        public void Hide()
        {
            InfoVisible = false;
            PasswordVisible = false;
            _state.MarkDirty();
        }

        /// <summary>Додає кімнати  список.</summary>
        public void AddRoomToList(RoomInfo room)
        {
            var key = BuildRoomKey(room);
            _rooms.RemoveAll(r => string.Equals(BuildRoomKey(r), key, StringComparison.Ordinal));
            _rooms.Add(room);
            _state.MarkDirty();
        }

        /// <summary>Очищує кімнати список.</summary>
        public void ClearRoomList()
        {
            _rooms.Clear();
            _state.MarkDirty();
        }

        /// <summary>Оновлює кімнати список.</summary>
        public void RefreshRoomList()
        {
            OnListRoomsRefresh?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Встановлює приєднання інтерактивності.</summary>
        public void SetJoinInteractable(bool interactable)
        {
            JoinInteractable = interactable;
            _state.MarkDirty();
        }

        /// <summary>Встановлює гравців.</summary>
        public void SetPlayers(IReadOnlyList<KickPlayerInfo> players)
        {
            _kickPlayers.Clear();
            if (players != null)
                _kickPlayers.AddRange(players);
            _state.MarkDirty();
        }

        /// <summary>Очищує гравців.</summary>
        public void ClearPlayers()
        {
            _kickPlayers.Clear();
            _state.MarkDirty();
        }

        /// <summary>Встановлює статусу.</summary>
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

        /// <summary>Встановлює запрошення код.</summary>
        public void SetInviteCode(LobbyInviteCodePresentation presentation)
        {
            InviteCodeText = presentation.DisplayText;
            InviteCodeValue = presentation.Code;
            InviteCopied = false;
            if (!string.IsNullOrWhiteSpace(presentation.RoomName))
                LobbyDisplayName = presentation.RoomName;
            _state.MarkDirty();
        }

        /// <summary>Очищує лобі Invate код.</summary>
        public void ClearLobbyInvateCode()
        {
            InviteCodeText = "Invite Code: N/A";
            InviteCodeValue = string.Empty;
            InviteCopied = false;
            LobbyDisplayName = "Lobby";
            _state.MarkDirty();
        }

        /// <summary>Додає нового користувача.</summary>
        public void AddNewUser(LobbyUserInfo userInfo)
        {
            _lobbyUsers.RemoveAll(u => u.UserId == userInfo.UserId);
            _lobbyUsers.Add(userInfo);
            _state.MarkDirty();
        }

        /// <summary>Видаляє користувача.</summary>
        public void RemoveUser(int userId)
        {
            _lobbyUsers.RemoveAll(u => u.UserId == userId);
            _state.MarkDirty();
        }

        /// <summary>Очищує Users.</summary>
        public void ClearUsers()
        {
            _lobbyUsers.Clear();
            _state.MarkDirty();
        }

        /// <summary>Оновлює користувача список.</summary>
        public void RefreshUserList()
        {
            _lobbyUsers.Sort((a, b) => a.UserId.CompareTo(b.UserId));
            _state.MarkDirty();
        }

        /// <summary>Оновлює Refresh.</summary>
        public void Refresh() => _state.MarkDirty();

        /// <summary>Показує Show.</summary>
        public void Show(string roomDisplayName, string errorText)
        {
            PasswordRoomDisplayName = roomDisplayName ?? string.Empty;
            PasswordErrorText = errorText ?? string.Empty;
            PasswordValue = string.Empty;
            PasswordVisible = true;
            _state.MarkDirty();
        }

        /// <summary>Встановлює помилки тексту.</summary>
        public void SetErrorText(string errorText)
        {
            PasswordErrorText = errorText ?? string.Empty;
            _state.MarkDirty();
        }

        /// <summary>Намагається отримати Reqest.</summary>
        public bool TryGetReqest(out ConfirmationRequest? request)
        {
            request = CurrentConfirmation;
            return ConfirmationVisible && request.HasValue;
        }

        /// <summary>Показує Show.</summary>
        public void Show(ConfirmationRequest request)
        {
            CurrentConfirmation = request;
            OnConfirme = request.OnConfirm;
            OnCancled = request.OnCancel;
            ConfirmationVisible = true;
            _state.MarkDirty();
        }

        /// <summary>Виконує ForeceHide.</summary>
        public void ForeceHide()
        {
            ConfirmationVisible = false;
            CurrentConfirmation = null;
            _state.MarkDirty();
        }

        /// <summary>Виконує ClickCreateRoom.</summary>
        public void ClickCreateRoom() => InvokeButton(_createRoomNextButton, () => CreateRoomRequested?.Invoke());
        /// <summary>Виконує ClickCreateWorld.</summary>
        public void ClickCreateWorld() => InvokeButton(_worldCreateButton, () => CreateWorldRequested?.Invoke());
        /// <summary>Виконує ClickLobbyStart.</summary>
        public void ClickLobbyStart() => InvokeButton(_lobbyStartButton, null);
        /// <summary>Виконує ClickLobbyBack.</summary>
        public void ClickLobbyBack() => InvokeButton(_lobbyBackButton, null);
        /// <summary>Обирає слота.</summary>
        public void SelectSlot(int index)
        {
            if (index < 0 || index >= _slots.Count)
                return;

            OnSlotSelected?.Invoke(_slots[index]);
            _state.MarkDirty();
        }

        /// <summary>Обирає кімнати.</summary>
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

        /// <summary>Запитує приєднання.</summary>
        public void RequestJoin()
        {
            if (!JoinInteractable)
                return;

            OnJoinRequested?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Запитує кімнати Refresh.</summary>
        public void RequestRoomRefresh()
        {
            OnListRoomsRefresh?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Запитує викиду Refresh.</summary>
        public void RequestKickRefresh()
        {
            OnRefreshRequested?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Запитує викиду.</summary>
        public void RequestKick(int index)
        {
            if (!KickInteractable || index < 0 || index >= _kickPlayers.Count)
                return;

            OnKickRequested?.Invoke(_kickPlayers[index]);
            _state.MarkDirty();
        }

        /// <summary>Запитує викиду закриття.</summary>
        public void RequestKickClose()
        {
            OnCloseRequested?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Обирає мультиплеєра створення.</summary>
        public void ChooseMultiplayerCreate(NetworkProviderType provider)
        {
            SelectedMode = provider;
            OnModeChanged?.Invoke(provider);
            OnCreateRoomClicked?.Invoke(provider);
            _state.MarkDirty();
        }

        /// <summary>Обирає мультиплеєра приєднання.</summary>
        public void ChooseMultiplayerJoin(NetworkProviderType provider)
        {
            SelectedMode = provider;
            OnModeChanged?.Invoke(provider);
            OnJoinRoomClicked?.Invoke(provider);
            _state.MarkDirty();
        }

        /// <summary>Починає керування Interaction.</summary>
        public void BeginControlInteraction() => _state.BeginInteraction();

        /// <summary>Завершує керування Interaction.</summary>
        public void EndControlInteraction() => _state.EndInteraction();

        /// <summary>Встановлює налаштування секції.</summary>
        public void SetSettingsSection(HomeMenuSettingsSection section) => _state.SetSettingsSection(section);

        /// <summary>Встановлює Play Flow.</summary>
        public void SetPlayFlow(HomeMenuPlayFlow flow) => _state.SetPlayFlow(flow);

        /// <summary>Встановлює режим.</summary>
        public void SetMode(NetworkProviderType provider)
        {
            if (SelectedMode == provider)
                return;

            SelectedMode = provider;
            OnModeChanged?.Invoke(provider);
            _state.MarkDirty();
        }

        /// <summary>Встановлює світу розміру.</summary>
        public void SetWorldSize(WorldSize size)
        {
            Size = size;
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Встановлює карти Type.</summary>
        public void SetMapType(MapType mapType)
        {
            MapType = mapType;
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Встановлює складності.</summary>
        public void SetDifficulty(Difficulty difficulty)
        {
            Difficulty = difficulty;
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Встановлює Bot складності індексу.</summary>
        public void SetBotDifficultyIndex(int index)
        {
            if (_botDifficulties.difficulties == null || _botDifficulties.difficulties.Length == 0)
                return;

            index = Mathf.Clamp(index, 0, _botDifficulties.difficulties.Length - 1);
            SelectedBotDifficultyId = _botDifficulties.difficulties[index].id;
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Виконує RandomizeSeed.</summary>
        public void RandomizeSeed()
        {
            Seed = UnityEngine.Random.Range(100000, int.MaxValue);
            OnRandomSeedClicked?.Invoke();
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Перемикає кімнати Visibility.</summary>
        public void ToggleRoomVisibility()
        {
            IsPublic = !IsPublic;
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        /// <summary>Встановлює кімнати приватної.</summary>
        public void SetRoomPrivate(bool isPrivate)
        {
            var isPublic = !isPrivate;
            if (IsPublic == isPublic)
                return;

            IsPublic = isPublic;
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        /// <summary>Виконує IncreaseMaxPlayers.</summary>
        public void IncreaseMaxPlayers()
        {
            MaxPlayers = Mathf.Clamp(MaxPlayers + 1, 2, 8);
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        /// <summary>Виконує DecreaseMaxPlayers.</summary>
        public void DecreaseMaxPlayers()
        {
            MaxPlayers = Mathf.Clamp(MaxPlayers - 1, 2, 8);
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        /// <summary>Встановлює кімнати назву.</summary>
        public void SetRoomName(string value)
        {
            RoomName = NormalizeText(value, "Moyva Lobby");
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        /// <summary>Виконує PreviewRoomName.</summary>
        public void PreviewRoomName(string value)
        {
            RoomName = value ?? string.Empty;
            RefreshCreateRoomInteractable();
        }

        /// <summary>Встановлює кімнати пароль.</summary>
        public void SetRoomPassword(string value)
        {
            Password = value ?? string.Empty;
            RefreshCreateRoomInteractable();
            _state.MarkDirty();
        }

        /// <summary>Виконує PreviewRoomPassword.</summary>
        public void PreviewRoomPassword(string value)
        {
            Password = value ?? string.Empty;
            RefreshCreateRoomInteractable();
        }

        /// <summary>Встановлює світу назву.</summary>
        public void SetWorldName(string value)
        {
            WorldName = NormalizeText(value, "New World");
            OnSettingsChanged?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Виконує PreviewWorldName.</summary>
        public void PreviewWorldName(string value) => WorldName = value ?? string.Empty;

        /// <summary>Встановлює Seed.</summary>
        public void SetSeed(string value)
        {
            if (int.TryParse((value ?? string.Empty).Trim(), out var seed) && seed != 0)
            {
                Seed = seed;
                OnSettingsChanged?.Invoke();
                _state.MarkDirty();
            }
        }

        /// <summary>Встановлює приєднання код.</summary>
        public void SetJoinCode(string value)
        {
            JoinCode = (value ?? string.Empty).Trim();
            OnJoinCodeChanged?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Виконує PreviewJoinCode.</summary>
        public void PreviewJoinCode(string value) => JoinCode = value ?? string.Empty;

        /// <summary>Встановлює гравця назву.</summary>
        public void SetPlayerName(string value)
        {
            PlayerName = NormalizeText(value, "Player");
            OnPlayerNameChanged?.Invoke(PlayerName);
            _state.MarkDirty();
        }

        /// <summary>Виконує PreviewPlayerName.</summary>
        public void PreviewPlayerName(string value) => PlayerName = value ?? string.Empty;

        /// <summary>Виконує ChangePlayerName.</summary>
        public void ChangePlayerName()
        {
            PlayerName = string.IsNullOrWhiteSpace(PlayerName)
                ? $"Player-{UnityEngine.Random.Range(1000, 9999)}"
                : $"{PlayerName.Trim()}*";
            OnPlayerNameChanged?.Invoke(PlayerName);
            _state.MarkDirty();
        }

        /// <summary>Встановлює Master.</summary>
        public void SetMaster(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(MasterVolume, value)) return;
            MasterVolume = value;
            OnMasterVolumeChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Встановлює музики.</summary>
        public void SetMusic(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(MusicVolume, value)) return;
            MusicVolume = value;
            OnMusicVolumeChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Встановлює Sfx.</summary>
        public void SetSfx(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(SfxVolume, value)) return;
            SfxVolume = value;
            OnSfxVolumeChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Встановлює UI.</summary>
        public void SetUi(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(UiVolume, value)) return;
            UiVolume = value;
            OnUiVolumeChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Встановлює Ambience.</summary>
        public void SetAmbience(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(AmbienceVolume, value)) return;
            AmbienceVolume = value;
            OnAmbienceVolumeChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Перемикає заглушеної.</summary>
        public void ToggleMuted() => SetMuted(!IsMuted);
        /// <summary>Встановлює заглушеної.</summary>
        public void SetMuted(bool value)
        {
            if (IsMuted == value) return;
            IsMuted = value;
            OnMutedChanged?.Invoke(value);
            _state.MarkDirty();
        }
        /// <summary>Встановлює графіки профілю.</summary>
        public void SetGraphicsProfile(GraphicsQualityProfile profile)
        {
            if (GraphicsProfile == profile)
                return;

            GraphicsProfile = profile;
            OnGraphicsProfileChanged?.Invoke(profile);
            _state.MarkDirty();
        }
        /// <summary>Встановлює рендер масштаб.</summary>
        public void SetRenderScale(float value)
        {
            value = Mathf.Clamp(value, 0.42f, 1f);
            if (Mathf.Approximately(RenderScale, value)) return;
            RenderScale = value;
            OnRenderScaleChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Встановлює кадру частоти.</summary>
        public void SetFrameRate(int value)
        {
            value = Mathf.Clamp(value, 30, 360);
            if (TargetFrameRate == value) return;
            TargetFrameRate = value;
            OnTargetFrameRateChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Встановлює текстур mipmap ліміту.</summary>
        public void SetTextureMipmapLimit(int value)
        {
            value = Mathf.Clamp(value, 0, 3);
            if (TextureMipmapLimit == value) return;
            TextureMipmapLimit = value;
            OnTextureMipmapLimitChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Встановлює LOD зсув.</summary>
        public void SetLodBias(float value)
        {
            value = Mathf.Clamp(Mathf.Round(value * 10f) / 10f, 0.4f, 2f);
            if (Mathf.Approximately(LodBias, value)) return;
            LodBias = value;
            OnLodBiasChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Перемикає динамічного рендер масштаб.</summary>
        public void ToggleDynamicRenderScale() => SetDynamicRenderScale(!DynamicRenderScale);
        /// <summary>Встановлює динамічного рендер масштаб.</summary>
        public void SetDynamicRenderScale(bool value)
        {
            if (DynamicRenderScale == value) return;
            DynamicRenderScale = value;
            OnDynamicRenderScaleChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Перемикає закриття зум оптимізації.</summary>
        public void ToggleCloseZoomOptimization() => SetCloseZoomOptimization(!CloseZoomOptimization);
        /// <summary>Встановлює закриття зум оптимізації.</summary>
        public void SetCloseZoomOptimization(bool value)
        {
            if (CloseZoomOptimization == value) return;
            CloseZoomOptimization = value;
            OnCloseZoomOptimizationChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Перемикає V синхронізації.</summary>
        public void ToggleVSync() => SetVSync(!VSync);
        /// <summary>Встановлює V синхронізації.</summary>
        public void SetVSync(bool value)
        {
            if (VSync == value) return;
            VSync = value;
            OnVSyncChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Перемикає тіней.</summary>
        public void ToggleShadows() => SetShadows(!Shadows);
        /// <summary>Встановлює тіней.</summary>
        public void SetShadows(bool value)
        {
            if (Shadows == value) return;
            Shadows = value;
            OnShadowsChanged?.Invoke(value);
            _state.MarkDirty();
        }

        /// <summary>Перемикає анізотропної.</summary>
        public void ToggleAnisotropic() => SetAnisotropic(!AnisotropicFiltering);
        /// <summary>Встановлює анізотропної.</summary>
        public void SetAnisotropic(bool value)
        {
            if (AnisotropicFiltering == value) return;
            AnisotropicFiltering = value;
            OnAnisotropicFilteringChanged?.Invoke(value);
            _state.MarkDirty();
        }
        /// <summary>Коригує рендер масштаб.</summary>
        public void AdjustRenderScale(float delta) { RenderScale = Mathf.Clamp(RenderScale + delta, 0.42f, 1f); OnRenderScaleChanged?.Invoke(RenderScale); _state.MarkDirty(); }
        /// <summary>Коригує кадру частоти.</summary>
        public void AdjustFrameRate(int delta) { TargetFrameRate = Mathf.Clamp(TargetFrameRate + delta, 30, 360); OnTargetFrameRateChanged?.Invoke(TargetFrameRate); _state.MarkDirty(); }
        /// <summary>Коригує текстур mipmap ліміту.</summary>
        public void AdjustTextureMipmapLimit(int delta) { TextureMipmapLimit = Mathf.Clamp(TextureMipmapLimit + delta, 0, 3); OnTextureMipmapLimitChanged?.Invoke(TextureMipmapLimit); _state.MarkDirty(); }
        /// <summary>Встановлює Anti Aliasing.</summary>
        public void SetAntiAliasing(int value)
        {
            value = NormalizeAntiAliasing(value);
            if (AntiAliasing == value) return;
            AntiAliasing = value;
            OnAntiAliasingChanged?.Invoke(value);
            _state.MarkDirty();
        }
        /// <summary>Коригує LOD зсув.</summary>
        public void AdjustLodBias(float delta) { LodBias = Mathf.Clamp(Mathf.Round((LodBias + delta) * 10f) / 10f, 0.4f, 2f); OnLodBiasChanged?.Invoke(LodBias); _state.MarkDirty(); }
        /// <summary>Скидає графіки.</summary>
        public void ResetGraphics() { OnResetGraphicsClicked?.Invoke(); _state.MarkDirty(); }
        /// <summary>Виконує DeleteSaves.</summary>
        public void DeleteSaves() { OnDeleteSavesClicked?.Invoke(); _state.MarkDirty(); }
        /// <summary>Встановлює миші чутливість.</summary>
        public void SetMouseSensitivity(float value) { MouseSensitivity = Mathf.Clamp(value, 0.25f, 3f); OnMouseSensitivityChanged?.Invoke(MouseSensitivity); _state.MarkDirty(); }
        /// <summary>Встановлює руху швидкість.</summary>
        public void SetMovementSpeed(float value) { MovementSpeed = Mathf.Clamp(value, 0.25f, 3f); OnMovementSpeedChanged?.Invoke(MovementSpeed); _state.MarkDirty(); }
        /// <summary>Встановлює орбіти швидкість.</summary>
        public void SetOrbitSpeed(float value) { OrbitSpeed = Mathf.Clamp(value, 0.25f, 3f); OnOrbitSpeedChanged?.Invoke(OrbitSpeed); _state.MarkDirty(); }
        /// <summary>Встановлює зум швидкість.</summary>
        public void SetZoomSpeed(float value) { ZoomSpeed = Mathf.Clamp(value, 0.25f, 3f); OnZoomSpeedChanged?.Invoke(ZoomSpeed); _state.MarkDirty(); }
        /// <summary>Встановлює камери ефектів.</summary>
        public void SetCameraEffects(bool value) { CameraEffects = value; OnCameraEffectsChanged?.Invoke(value); _state.MarkDirty(); }
        /// <summary>Встановлює камери тряски Intensity.</summary>
        public void SetCameraShakeIntensity(float value) { CameraShakeIntensity = Mathf.Clamp01(value); OnCameraShakeIntensityChanged?.Invoke(CameraShakeIntensity); _state.MarkDirty(); }
        /// <summary>Встановлює плавного камери фокус.</summary>
        public void SetSmoothCameraFocus(bool value) { SmoothCameraFocus = value; OnSmoothCameraFocusChanged?.Invoke(value); _state.MarkDirty(); }
        /// <summary>Встановлює автоматичного камери фокус.</summary>
        public void SetAutomaticCameraFocus(bool value) { AutomaticCameraFocus = value; OnAutomaticCameraFocusChanged?.Invoke(value); _state.MarkDirty(); }
        /// <summary>Встановлює Reduce камери руху.</summary>
        public void SetReduceCameraMotion(bool value) { ReduceCameraMotion = value; OnReduceCameraMotionChanged?.Invoke(value); _state.MarkDirty(); }
        /// <summary>Встановлює зум Toward пальців.</summary>
        public void SetZoomTowardFingers(bool value) { ZoomTowardFingers = value; OnZoomTowardFingersChanged?.Invoke(value); _state.MarkDirty(); }
        /// <summary>Встановлює керування привʼязки.</summary>
        public void SetControlBinding(PlayerControlAction action, string controlPath) { OnControlBindingChanged?.Invoke(action, controlPath); _state.MarkDirty(); }
        /// <summary>Скидає Controls.</summary>
        public void ResetControls() { OnResetControlsClicked?.Invoke(); _state.MarkDirty(); }

        /// <summary>Підтверджує модального.</summary>
        public void ConfirmModal()
        {
            var confirm = OnConfirme;
            ForeceHide();
            confirm?.Invoke();
        }

        /// <summary>Скасовує модального.</summary>
        public void CancelModal()
        {
            var cancel = OnCancled;
            ForeceHide();
            cancel?.Invoke();
        }

        /// <summary>Підтверджує інформації.</summary>
        public void AcknowledgeInfo()
        {
            var callback = CurrentInfo.OnAcknowledged;
            InfoVisible = false;
            OnAcknowledged?.Invoke();
            callback?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Підтверджує пароль.</summary>
        public void ConfirmPassword()
        {
            PasswordVisible = false;
            OnConfirmed?.Invoke(PasswordValue ?? string.Empty);
            _state.MarkDirty();
        }

        /// <summary>Скасовує пароль.</summary>
        public void CancelPassword()
        {
            PasswordVisible = false;
            OnCancelled?.Invoke();
            _state.MarkDirty();
        }

        /// <summary>Сетер властивості.</summary>
        public void SetPasswordPreset(string value)
        {
            PasswordValue = value ?? string.Empty;
            _state.MarkDirty();
        }

        /// <summary>Встановлює пароль значення.</summary>
        public void SetPasswordValue(string value)
        {
            PasswordValue = value ?? string.Empty;
            _state.MarkDirty();
        }

        /// <summary>Виконує PreviewPasswordValue.</summary>
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
            AmbienceVolume = 0.85f;
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
            if (string.IsNullOrWhiteSpace(RoomName))
                CreateRoomBlockReason = "Enter a room name to continue.";
            else if (!IsPublic && string.IsNullOrEmpty(Password))
                CreateRoomBlockReason = "Private rooms need a password. Set one or make the room public.";
            else
                CreateRoomBlockReason = string.Empty;

            if (_createRoomNextButton != null)
                _createRoomNextButton.interactable = CreateRoomBlockReason.Length == 0;
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
