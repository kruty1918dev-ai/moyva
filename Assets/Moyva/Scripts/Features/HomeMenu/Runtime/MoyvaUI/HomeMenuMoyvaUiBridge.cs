using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
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

        internal HomeMenuMoyvaUiBridge(
            INavigation navigation,
            IConfirmationService confirmationService,
            HomeMenuMoyvaUiViewController view)
        {
            _navigation = navigation;
            _confirmationService = confirmationService;
            _view = view;
        }

        public void Play() => Open(PlayModePanel);
        public void Continue() => Open(ContinuePanel);
        public void Multiplayer() => Open(MultiplayerPanel);
        public void Settings() => Open(SettingsPanel);
        public void WorldSetup() => Open(WorldSetupPanel);
        public void Back() => _navigation?.CloseLast();
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

        public void CreateRoom() => _view?.ClickCreateRoom();
        public void TogglePublic() => _view?.ToggleRoomVisibility();
        public void MaxPlayersMinus() => _view?.DecreaseMaxPlayers();
        public void MaxPlayersPlus() => _view?.IncreaseMaxPlayers();

        public void CreateWorld() => _view?.ClickCreateWorld();
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

        public void StartGame() => _view?.ClickLobbyStart();
        public void LeaveLobby() => _view?.ClickLobbyBack();
        public void RefreshRooms() => _view?.RequestRoomRefresh();
        public void JoinTypedRoom() => _view?.RequestJoin();
        public void SelectSlot(int index) => _view?.SelectSlot(index);
        public void SelectRoom(int index) => _view?.SelectRoom(index);
        public void RefreshKickPlayers() => _view?.RequestKickRefresh();
        public void CloseKickPlayers() => _view?.RequestKickClose();
        public void KickPlayer(int index) => _view?.RequestKick(index);

        public void ChangePlayerName() => _view?.ChangePlayerName();
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
        public void GraphicsAuto() => _view?.SetGraphicsProfile(GraphicsQualityProfile.Auto);
        public void GraphicsPerformance() => _view?.SetGraphicsProfile(GraphicsQualityProfile.Performance);
        public void GraphicsBalanced() => _view?.SetGraphicsProfile(GraphicsQualityProfile.Balanced);
        public void GraphicsQuality() => _view?.SetGraphicsProfile(GraphicsQualityProfile.Quality);
        public void RenderScaleDown() => _view?.AdjustRenderScale(-0.1f);
        public void RenderScaleUp() => _view?.AdjustRenderScale(0.1f);
        public void FrameRateDown() => _view?.AdjustFrameRate(-30);
        public void FrameRateUp() => _view?.AdjustFrameRate(30);
        public void ToggleVSync() => _view?.ToggleVSync();
        public void ToggleShadows() => _view?.ToggleShadows();
        public void ToggleAnisotropic() => _view?.ToggleAnisotropic();
        public void ResetGraphics() => _view?.ResetGraphics();
        public void DeleteSaves() => _view?.DeleteSaves();

        public void Confirm() => _view?.ConfirmModal();
        public void Cancel() => _view?.CancelModal();
        public void AcknowledgeInfo() => _view?.AcknowledgeInfo();
        public void PasswordEmpty() => _view?.SetPasswordPreset(string.Empty);
        public void PasswordDemo() => _view?.SetPasswordPreset("moyva");
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
    }
}
