using System;
using System.Globalization;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Shared.Controls;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    public sealed class HomeMenuMoyvaUiBridge
    {
        private const string PlayModePanel = "PlayModePanel";
        private const string ContinuePanel = "ContinuePanel";
        private const string MultiplayerPanel = "SelectMultiplayerType";
        private const string SettingsPanel = "SettingsPanel";
        private const string WorldSetupPanel = "WorldSetupPanel";

        private readonly INavigation _navigation;
        private readonly IConfirmationService _confirmationService;
        private readonly HomeMenuMoyvaUiViewController _view;
        private readonly ILobbyFlowContext _lobbyFlowContext;
        private readonly HomeMenuMoyvaUiState _state;

        private static readonly int[] FrameRateOptions = { 30, 45, 60, 90, 120, 144, 240 };

        internal HomeMenuMoyvaUiBridge(
            INavigation navigation,
            IConfirmationService confirmationService,
            HomeMenuMoyvaUiViewController view,
            ILobbyFlowContext lobbyFlowContext = null,
            HomeMenuMoyvaUiState state = null)
        {
            _navigation = navigation;
            _confirmationService = confirmationService;
            _view = view;
            _lobbyFlowContext = lobbyFlowContext;
            _state = state ?? view?.State;
        }

        public void Play() => Open(PlayModePanel);
        public void Continue() => Open(ContinuePanel);
        public void Solo()
        {
            _lobbyFlowContext?.Set(NetworkProviderType.Offline, LobbyFlowKind.None);
            _state?.SetPlayFlow(HomeMenuPlayFlow.Solo);
            Open(WorldSetupPanel);
        }
        public void Multiplayer()
        {
            _state?.SetPlayFlow(HomeMenuPlayFlow.Multiplayer);
            Open(MultiplayerPanel);
        }
        public void Settings() => Open(SettingsPanel);
        public void WorldSetup() => Open(WorldSetupPanel);
        public void Back()
        {
            if (_view != null && _view.Controls.IsCapturing) { _view.Controls.CancelCapture(); return; }
            _navigation?.CloseLast();
        }
        public void BackForce() => _navigation?.CloseLastForce();

        public void Exit()
        {
            if (_confirmationService == null)
            {
                Debug.LogError("[HomeMenuMoyvaUiBridge] Confirmation service is not available.");
                return;
            }

            _confirmationService.Show(new ConfirmationRequest
            {
                LabelText = "Exit Game",
                MessageText = "Are you sure you want to quit?",
                OnConfirm = Application.Quit
            });
        }

        public void CreateLan() => _view?.ChooseMultiplayerCreate(NetworkProviderType.Lan);
        public void CreateGlobal() => _view?.ChooseMultiplayerCreate(NetworkProviderType.Relay);
        public void JoinLan() => _view?.ChooseMultiplayerJoin(NetworkProviderType.Lan);
        public void JoinGlobal() => _view?.ChooseMultiplayerJoin(NetworkProviderType.Relay);
        public void SetModeLan() => _view?.SetMode(NetworkProviderType.Lan);
        public void SetModeGlobal() => _view?.SetMode(NetworkProviderType.Relay);
        public void SetNetworkMode(object value) => _view?.SetMode(ToInt(value, 0) == 1 ? NetworkProviderType.Lan : NetworkProviderType.Relay);
        public void CreateSelected() => _view?.ChooseMultiplayerCreate(NormalizeMultiplayerMode(_view.SelectedMode));
        public void JoinSelected() => _view?.ChooseMultiplayerJoin(NormalizeMultiplayerMode(_view.SelectedMode));

        public void ShowGeneralSettings() => _state?.SetSettingsSection(HomeMenuSettingsSection.General);
        public void ShowAudioSettings() => _state?.SetSettingsSection(HomeMenuSettingsSection.Audio);
        public void ShowGraphicsSettings() => _state?.SetSettingsSection(HomeMenuSettingsSection.Graphics);
        public void ShowControlsSettings() => _state?.SetSettingsSection(HomeMenuSettingsSection.Controls);

        public void BeginControlInteraction() => _state?.BeginInteraction();
        public void EndControlInteraction() => _state?.EndInteraction();

        public void CreateRoom() => _view?.ClickCreateRoom();
        public void SetRoomName(string value) => _view?.SetRoomName(value);
        public void SetRoomPassword(string value) => _view?.SetRoomPassword(value);
        public void PreviewRoomName(string value) => _view?.PreviewRoomName(value);
        public void PreviewRoomPassword(string value) => _view?.PreviewRoomPassword(value);
        public void CommitRoomName(string value) => Commit(() => _view?.SetRoomName(value));
        public void CommitRoomPassword(string value) => Commit(() => _view?.SetRoomPassword(value));
        public void TogglePublic() => _view?.ToggleRoomVisibility();
        public void SetPrivate(object value) => _view?.SetRoomPrivate(ToBool(value));
        public void MaxPlayersMinus() => _view?.DecreaseMaxPlayers();
        public void MaxPlayersPlus() => _view?.IncreaseMaxPlayers();

        public void CreateWorld() => _view?.ClickCreateWorld();
        public void SetWorldName(string value) => _view?.SetWorldName(value);
        public void SetSeed(string value) => _view?.SetSeed(value);
        public void PreviewWorldName(string value) => _view?.PreviewWorldName(value);
        public void CommitWorldName(string value) => Commit(() => _view?.SetWorldName(value));
        public void CommitSeed(string value) => Commit(() => _view?.SetSeed(value));
        public void RandomSeed() => _view?.RandomizeSeed();
        public void WorldSmall() => _view?.SetWorldSize(WorldSize.Small);
        public void WorldMedium() => _view?.SetWorldSize(WorldSize.Medium);
        public void WorldLarge() => _view?.SetWorldSize(WorldSize.Large);
        public void MapContinents() => _view?.SetMapType(MapType.Continents);
        public void MapPangaea() => _view?.SetMapType(MapType.Pangaea);
        public void MapIslands() => _view?.SetMapType(MapType.Islands);
        public void MapHighlands() => _view?.SetMapType(MapType.Highlands);
        public void DifficultyEasy() => _view?.SetDifficulty(Difficulty.Easy);
        public void DifficultyNormal() => _view?.SetDifficulty(Difficulty.Normal);
        public void DifficultyHard() => _view?.SetDifficulty(Difficulty.Hard);
        public void DifficultyInsane() => _view?.SetDifficulty(Difficulty.Insane);
        public void SetWorldSizeValue(object value) => _view?.SetWorldSize((WorldSize)Mathf.Clamp(ToInt(value, 1), 0, 2));
        public void SetMapTypeValue(object value) => _view?.SetMapType((MapType)Mathf.Clamp(ToInt(value, 0), 0, 5));
        public void SetDifficultyValue(object value) => _view?.SetDifficulty((Difficulty)Mathf.Clamp(ToInt(value, 1), 0, 3));

        public void StartGame() => _view?.ClickLobbyStart();
        public void LeaveLobby() => _view?.ClickLobbyBack();
        public void RefreshRooms() => _view?.RequestRoomRefresh();
        public void JoinTypedRoom() => _view?.RequestJoin();
        public void SetJoinCode(string value) => _view?.SetJoinCode(value);
        public void PreviewJoinCode(string value) => _view?.PreviewJoinCode(value);
        public void CommitJoinCode(string value) => Commit(() => _view?.SetJoinCode(value));
        public void SelectSlot(int index) => _view?.SelectSlot(index);
        public void SelectRoom(int index) => _view?.SelectRoom(index);
        public void RefreshKickPlayers() => _view?.RequestKickRefresh();
        public void CloseKickPlayers() => _view?.RequestKickClose();
        public void KickPlayer(int index) => _view?.RequestKick(index);

        public void ChangePlayerName() => _view?.ChangePlayerName();
        public void SetPlayerName(string value) => _view?.SetPlayerName(value);
        public void PreviewPlayerName(string value) => _view?.PreviewPlayerName(value);
        public void CommitPlayerName(string value) => Commit(() => _view?.SetPlayerName(value));
        public void SetMasterValue(object value) => _view?.SetMaster(ToFloat(value, _view.MasterVolume));
        public void SetMusicValue(object value) => _view?.SetMusic(ToFloat(value, _view.MusicVolume));
        public void SetSfxValue(object value) => _view?.SetSfx(ToFloat(value, _view.SfxVolume));
        public void SetUiValue(object value) => _view?.SetUi(ToFloat(value, _view.UiVolume));
        public void CommitMasterValue(object value) => Commit(() => SetMasterValue(value));
        public void CommitMusicValue(object value) => Commit(() => SetMusicValue(value));
        public void CommitSfxValue(object value) => Commit(() => SetSfxValue(value));
        public void CommitUiValue(object value) => Commit(() => SetUiValue(value));
        public void MasterLow() => _view?.SetMaster(0.35f);
        public void MasterMid() => _view?.SetMaster(0.7f);
        public void MasterHigh() => _view?.SetMaster(1f);
        public void MusicLow() => _view?.SetMusic(0.35f);
        public void MusicHigh() => _view?.SetMusic(0.9f);
        public void SfxLow() => _view?.SetSfx(0.35f);
        public void SfxHigh() => _view?.SetSfx(0.9f);
        public void UiLow() => _view?.SetUi(0.35f);
        public void UiHigh() => _view?.SetUi(0.9f);
        public void ToggleMuted() => _view?.ToggleMuted();
        public void SetMuted(object value) => _view?.SetMuted(ToBool(value));
        public void GraphicsAuto() => _view?.SetGraphicsProfile(GraphicsQualityProfile.Auto);
        public void GraphicsPerformance() => _view?.SetGraphicsProfile(GraphicsQualityProfile.Performance);
        public void GraphicsBalanced() => _view?.SetGraphicsProfile(GraphicsQualityProfile.Balanced);
        public void GraphicsQuality() => _view?.SetGraphicsProfile(GraphicsQualityProfile.Quality);
        public void SetGraphicsProfileValue(object value) => _view?.SetGraphicsProfile(
            (GraphicsQualityProfile)Mathf.Clamp(ToInt(value, 0), 0, 4));
        public void SetRenderScaleValue(object value) => _view?.SetRenderScale(ToFloat(value, _view.RenderScale));
        public void SetFrameRateValue(object value) => _view?.SetFrameRate(ToInt(value, _view.TargetFrameRate));
        public void SetMipmapValue(object value) => _view?.SetTextureMipmapLimit(ToInt(value, _view.TextureMipmapLimit));
        public void SetAntiAliasingSliderValue(object value) => _view?.SetAntiAliasing(SliderToAntiAliasing(ToInt(value, 0)));
        public void SetLodBiasValue(object value) => _view?.SetLodBias(ToFloat(value, _view.LodBias));
        public void CommitRenderScaleValue(object value) => Commit(() => SetRenderScaleValue(value));
        public void CommitLodBiasValue(object value) => Commit(() => SetLodBiasValue(value));
        public void SetFrameRateOption(object value)
        {
            var index = Mathf.Clamp(ToInt(value, 2), 0, FrameRateOptions.Length - 1);
            _view?.SetFrameRate(FrameRateOptions[index]);
        }
        public void SetTextureQualityOption(object value) => _view?.SetTextureMipmapLimit(Mathf.Clamp(ToInt(value, 0), 0, 3));
        public void SetAntiAliasingOption(object value) => _view?.SetAntiAliasing(SliderToAntiAliasing(ToInt(value, 0)));
        public void RenderScaleDown() => _view?.AdjustRenderScale(-0.1f);
        public void RenderScaleUp() => _view?.AdjustRenderScale(0.1f);
        public void FrameRateDown() => _view?.AdjustFrameRate(-30);
        public void FrameRateUp() => _view?.AdjustFrameRate(30);
        public void ToggleDynamicRenderScale() => _view?.ToggleDynamicRenderScale();
        public void ToggleCloseZoomOptimization() => _view?.ToggleCloseZoomOptimization();
        public void MipmapDown() => _view?.AdjustTextureMipmapLimit(-1);
        public void MipmapUp() => _view?.AdjustTextureMipmapLimit(1);
        public void AntiAliasingOff() => _view?.SetAntiAliasing(0);
        public void AntiAliasing2x() => _view?.SetAntiAliasing(2);
        public void AntiAliasing4x() => _view?.SetAntiAliasing(4);
        public void ToggleVSync() => _view?.ToggleVSync();
        public void ToggleShadows() => _view?.ToggleShadows();
        public void ToggleAnisotropic() => _view?.ToggleAnisotropic();
        public void SetVSync(object value) => _view?.SetVSync(ToBool(value));
        public void SetShadows(object value) => _view?.SetShadows(ToBool(value));
        public void SetAnisotropic(object value) => _view?.SetAnisotropic(ToBool(value));
        public void LodBiasDown() => _view?.AdjustLodBias(-0.1f);
        public void LodBiasUp() => _view?.AdjustLodBias(0.1f);
        public void ResetGraphics() => _view?.ResetGraphics();
        public void DeleteSaves() => _view?.DeleteSaves();
        public void CommitMouseSensitivityValue(object value) => Commit(() => _view?.SetMouseSensitivity(ToFloat(value, _view.MouseSensitivity)));
        public void CommitMovementSpeedValue(object value) => Commit(() => _view?.SetMovementSpeed(ToFloat(value, _view.MovementSpeed)));
        public void CommitOrbitSpeedValue(object value) => Commit(() => _view?.SetOrbitSpeed(ToFloat(value, _view.OrbitSpeed)));
        public void CommitZoomSpeedValue(object value) => Commit(() => _view?.SetZoomSpeed(ToFloat(value, _view.ZoomSpeed)));
        public void ResetControls()
        {
            _view?.Controls.CancelCapture();
            _confirmationService?.Show(new ConfirmationRequest
            {
                LabelText = "Restore camera controls?",
                MessageText = "This resets camera shortcuts and sensitivity to their defaults.",
                OnConfirm = () => { _view?.ResetControls(); _view?.Controls.ResetSelection(); }
            });
        }
        public void SelectControlKey(string key) => _view?.Controls.SelectKey(key);
        public void ToggleControlModifier(int modifier) => _view?.Controls.ToggleModifier(modifier);
        public void EditControlAction(int action) => _view?.Controls.SelectAction(action, true);
        public void SelectControlAction(object action) => _view?.Controls.SelectAction(ToInt(action, 0), false);
        public void RecordControlShortcut() => _view?.Controls.StartCapture();
        public void CancelControlCapture() => _view?.Controls.CancelCapture();
        public void ApplyControlShortcut() => _view?.Controls.Apply();
        public void RebindControl(int actionIndex, string controlPath)
        {
            if (!Enum.IsDefined(typeof(PlayerControlAction), actionIndex))
                return;

            _view?.SetControlBinding((PlayerControlAction)actionIndex, controlPath);
        }

        public void Confirm() => _view?.ConfirmModal();
        public void Cancel() => _view?.CancelModal();
        public void AcknowledgeInfo() => _view?.AcknowledgeInfo();
        public void PasswordEmpty() => _view?.SetPasswordPreset(string.Empty);
        public void PasswordDemo() => _view?.SetPasswordPreset("moyva");
        public void SetPasswordValue(string value) => _view?.SetPasswordValue(value);
        public void PreviewPasswordValue(string value) => _view?.PreviewPasswordValue(value);
        public void CommitPasswordValue(string value) => Commit(() => _view?.SetPasswordValue(value));
        public void ConfirmPassword() => _view?.ConfirmPassword();
        public void CancelPassword() => _view?.CancelPassword();

        private void Open(string panelName)
        {
            if (_navigation == null)
            {
                Debug.LogError($"[HomeMenuMoyvaUiBridge] Navigation service is not available for '{panelName}'.");
                return;
            }

            _navigation.Open(panelName);
        }

        private void Commit(Action action)
        {
            try
            {
                action?.Invoke();
            }
            finally
            {
                _state?.EndInteraction();
            }
        }

        private static float ToFloat(object value, float fallback)
        {
            switch (value)
            {
                case null:
                    return fallback;
                case float floatValue:
                    return floatValue;
                case double doubleValue:
                    return (float)doubleValue;
                case int intValue:
                    return intValue;
                case long longValue:
                    return longValue;
                case string stringValue:
                    if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariantValue))
                        return invariantValue;
                    if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.CurrentCulture, out var currentValue))
                        return currentValue;
                    return fallback;
                case IConvertible convertible:
                    try
                    {
                        return convertible.ToSingle(CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        return fallback;
                    }
                default:
                    return fallback;
            }
        }

        private static int ToInt(object value, int fallback)
        {
            return Mathf.RoundToInt(ToFloat(value, fallback));
        }

        private static bool ToBool(object value)
        {
            if (value is bool boolValue)
                return boolValue;

            return value != null && bool.TryParse(value.ToString(), out var parsed) && parsed;
        }

        private static NetworkProviderType NormalizeMultiplayerMode(NetworkProviderType mode)
        {
            return mode == NetworkProviderType.Lan ? NetworkProviderType.Lan : NetworkProviderType.Relay;
        }

        private static int SliderToAntiAliasing(int value)
        {
            if (value >= 2)
                return 4;

            if (value >= 1)
                return 2;

            return 0;
        }
    }
}
