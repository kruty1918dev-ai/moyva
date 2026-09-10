using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Shared.Controls;
using Kruty1918.Moyva.Shared.Graphics;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class GameSettingsPanelService : IGameSettingsPanelService, IInitializable, IDisposable
    {
        [InjectOptional] private IGameSettingsViewController _viewController;
        [Inject] private ILocalGameSettingsService _settingsService;
        [InjectOptional] private IConfirmationService _confirmationService;
        [InjectOptional] private HomeMenuMoyvaUiViewController _moyvaUiViewController;
        [InjectOptional] private ILobbyService _lobbyService;
        private IGraphicsSettingsService _graphicsSettingsService;
        private IPlayerControlSettingsService _controlSettingsService;

        [Inject]
        private void Construct(
            [InjectOptional] IGraphicsSettingsService graphicsSettingsService,
            [InjectOptional] IPlayerControlSettingsService controlSettingsService)
        {
            _graphicsSettingsService = graphicsSettingsService;
            _controlSettingsService = controlSettingsService;
        }

        public void Initialize()
        {
            if (_viewController == null)
                return;

            _viewController.OnPlayerNameChanged -= OnPlayerNameChanged;
            _viewController.OnPlayerNameChanged += OnPlayerNameChanged;
            _viewController.OnMasterVolumeChanged -= OnMasterVolumeChanged;
            _viewController.OnMasterVolumeChanged += OnMasterVolumeChanged;
            _viewController.OnMusicVolumeChanged -= OnMusicVolumeChanged;
            _viewController.OnMusicVolumeChanged += OnMusicVolumeChanged;
            _viewController.OnSfxVolumeChanged -= OnSfxVolumeChanged;
            _viewController.OnSfxVolumeChanged += OnSfxVolumeChanged;
            _viewController.OnUiVolumeChanged -= OnUiVolumeChanged;
            _viewController.OnUiVolumeChanged += OnUiVolumeChanged;
            _viewController.OnMutedChanged -= OnMutedChanged;
            _viewController.OnMutedChanged += OnMutedChanged;
            _viewController.OnGraphicsProfileChanged -= OnGraphicsProfileChanged;
            _viewController.OnGraphicsProfileChanged += OnGraphicsProfileChanged;
            _viewController.OnTargetFrameRateChanged -= OnTargetFrameRateChanged;
            _viewController.OnTargetFrameRateChanged += OnTargetFrameRateChanged;
            _viewController.OnRenderScaleChanged -= OnRenderScaleChanged;
            _viewController.OnRenderScaleChanged += OnRenderScaleChanged;
            _viewController.OnCloseZoomOptimizationChanged -= OnCloseZoomOptimizationChanged;
            _viewController.OnCloseZoomOptimizationChanged += OnCloseZoomOptimizationChanged;
            _viewController.OnTextureMipmapLimitChanged -= OnTextureMipmapLimitChanged;
            _viewController.OnTextureMipmapLimitChanged += OnTextureMipmapLimitChanged;
            _viewController.OnAntiAliasingChanged -= OnAntiAliasingChanged;
            _viewController.OnAntiAliasingChanged += OnAntiAliasingChanged;
            _viewController.OnVSyncChanged -= OnVSyncChanged;
            _viewController.OnVSyncChanged += OnVSyncChanged;
            _viewController.OnShadowsChanged -= OnShadowsChanged;
            _viewController.OnShadowsChanged += OnShadowsChanged;
            _viewController.OnAnisotropicFilteringChanged -= OnAnisotropicFilteringChanged;
            _viewController.OnAnisotropicFilteringChanged += OnAnisotropicFilteringChanged;
            _viewController.OnLodBiasChanged -= OnLodBiasChanged;
            _viewController.OnLodBiasChanged += OnLodBiasChanged;
            _viewController.OnResetGraphicsClicked -= OnResetGraphicsClicked;
            _viewController.OnResetGraphicsClicked += OnResetGraphicsClicked;
            _viewController.OnDeleteSavesClicked -= OnDeleteSavesClicked;
            _viewController.OnDeleteSavesClicked += OnDeleteSavesClicked;
            _settingsService.OnSettingsChanged -= OnSettingsChanged;
            _settingsService.OnSettingsChanged += OnSettingsChanged;
            if (_graphicsSettingsService != null)
            {
                _graphicsSettingsService.OnSettingsChanged -= OnGraphicsSettingsChanged;
                _graphicsSettingsService.OnSettingsChanged += OnGraphicsSettingsChanged;
                _viewController.RefreshGraphics(_graphicsSettingsService.Settings);
            }
            if (_controlSettingsService != null && _moyvaUiViewController != null)
            {
                _controlSettingsService.OnSettingsChanged -= OnControlSettingsChanged;
                _controlSettingsService.OnSettingsChanged += OnControlSettingsChanged;
                _moyvaUiViewController.OnMouseSensitivityChanged -= OnMouseSensitivityChanged;
                _moyvaUiViewController.OnMouseSensitivityChanged += OnMouseSensitivityChanged;
                _moyvaUiViewController.OnMovementSpeedChanged -= OnMovementSpeedChanged;
                _moyvaUiViewController.OnMovementSpeedChanged += OnMovementSpeedChanged;
                _moyvaUiViewController.OnOrbitSpeedChanged -= OnOrbitSpeedChanged;
                _moyvaUiViewController.OnOrbitSpeedChanged += OnOrbitSpeedChanged;
                _moyvaUiViewController.OnZoomSpeedChanged -= OnZoomSpeedChanged;
                _moyvaUiViewController.OnZoomSpeedChanged += OnZoomSpeedChanged;
                _moyvaUiViewController.OnControlBindingChanged -= OnControlBindingChanged;
                _moyvaUiViewController.OnControlBindingChanged += OnControlBindingChanged;
                _moyvaUiViewController.OnResetControlsClicked -= OnResetControlsClicked;
                _moyvaUiViewController.OnResetControlsClicked += OnResetControlsClicked;
                _moyvaUiViewController.RefreshControls(_controlSettingsService.Settings);
            }
            ApplySettingsPolicy();
            _viewController.Refresh(_settingsService.Settings);
        }

        public void Dispose()
        {
            if (_viewController != null)
            {
                _viewController.OnPlayerNameChanged -= OnPlayerNameChanged;
                _viewController.OnMasterVolumeChanged -= OnMasterVolumeChanged;
                _viewController.OnMusicVolumeChanged -= OnMusicVolumeChanged;
                _viewController.OnSfxVolumeChanged -= OnSfxVolumeChanged;
                _viewController.OnUiVolumeChanged -= OnUiVolumeChanged;
                _viewController.OnMutedChanged -= OnMutedChanged;
                _viewController.OnGraphicsProfileChanged -= OnGraphicsProfileChanged;
                _viewController.OnTargetFrameRateChanged -= OnTargetFrameRateChanged;
                _viewController.OnRenderScaleChanged -= OnRenderScaleChanged;
                _viewController.OnCloseZoomOptimizationChanged -= OnCloseZoomOptimizationChanged;
                _viewController.OnTextureMipmapLimitChanged -= OnTextureMipmapLimitChanged;
                _viewController.OnAntiAliasingChanged -= OnAntiAliasingChanged;
                _viewController.OnVSyncChanged -= OnVSyncChanged;
                _viewController.OnShadowsChanged -= OnShadowsChanged;
                _viewController.OnAnisotropicFilteringChanged -= OnAnisotropicFilteringChanged;
                _viewController.OnLodBiasChanged -= OnLodBiasChanged;
                _viewController.OnResetGraphicsClicked -= OnResetGraphicsClicked;
                _viewController.OnDeleteSavesClicked -= OnDeleteSavesClicked;
            }

            if (_settingsService != null)
                _settingsService.OnSettingsChanged -= OnSettingsChanged;

            if (_graphicsSettingsService != null)
                _graphicsSettingsService.OnSettingsChanged -= OnGraphicsSettingsChanged;

            if (_controlSettingsService != null)
                _controlSettingsService.OnSettingsChanged -= OnControlSettingsChanged;

            if (_moyvaUiViewController != null)
            {
                _moyvaUiViewController.OnMouseSensitivityChanged -= OnMouseSensitivityChanged;
                _moyvaUiViewController.OnMovementSpeedChanged -= OnMovementSpeedChanged;
                _moyvaUiViewController.OnOrbitSpeedChanged -= OnOrbitSpeedChanged;
                _moyvaUiViewController.OnZoomSpeedChanged -= OnZoomSpeedChanged;
                _moyvaUiViewController.OnControlBindingChanged -= OnControlBindingChanged;
                _moyvaUiViewController.OnResetControlsClicked -= OnResetControlsClicked;
            }
        }

        private void OnPlayerNameChanged(string playerName)
        {
            _settingsService.SetPlayerName(playerName);
            _viewController.Refresh(_settingsService.Settings);
        }

        private void OnMasterVolumeChanged(float volume)
        {
            _settingsService.SetMasterVolume(volume);
        }

        private void OnMusicVolumeChanged(float volume)
        {
            _settingsService.SetMusicVolume(volume);
        }

        private void OnSfxVolumeChanged(float volume)
        {
            _settingsService.SetSfxVolume(volume);
        }

        private void OnUiVolumeChanged(float volume)
        {
            _settingsService.SetUiVolume(volume);
        }

        private void OnMutedChanged(bool isMuted)
        {
            _settingsService.SetMuted(isMuted);
        }

        private void OnGraphicsProfileChanged(GraphicsQualityProfile profile)
        {
            _graphicsSettingsService?.SetProfile(profile);
        }

        private void OnTargetFrameRateChanged(int frameRate)
        {
            _graphicsSettingsService?.SetTargetFrameRate(frameRate);
        }

        private void OnRenderScaleChanged(float renderScale)
        {
            _graphicsSettingsService?.SetRenderScale(renderScale);
        }

        private void OnCloseZoomOptimizationChanged(bool enabled)
        {
            _graphicsSettingsService?.SetCloseZoomOptimization(enabled);
        }

        private void OnTextureMipmapLimitChanged(int mipmapLimit)
        {
            _graphicsSettingsService?.SetTextureMipmapLimit(mipmapLimit);
        }

        private void OnAntiAliasingChanged(int antiAliasing)
        {
            _graphicsSettingsService?.SetAntiAliasing(antiAliasing);
        }

        private void OnVSyncChanged(bool enabled)
        {
            _graphicsSettingsService?.SetVSync(enabled);
        }

        private void OnShadowsChanged(bool enabled)
        {
            _graphicsSettingsService?.SetShadows(enabled);
        }

        private void OnAnisotropicFilteringChanged(bool enabled)
        {
            _graphicsSettingsService?.SetAnisotropicFiltering(enabled);
        }

        private void OnLodBiasChanged(float lodBias)
        {
            _graphicsSettingsService?.SetLodBias(lodBias);
        }

        private void OnResetGraphicsClicked()
        {
            _graphicsSettingsService?.ResetToDefaults();
        }

        private void OnDeleteSavesClicked()
        {
            if (_confirmationService == null)
            {
                _settingsService.DeleteAllSaves();
                return;
            }

            _confirmationService.Show(new ConfirmationRequest
            {
                LabelText = "Confirmation",
                MessageText = "Delete all local saves?",
                OnConfirm = _settingsService.DeleteAllSaves,
                OnCancel = () => { }
            });
        }

        private void OnSettingsChanged(LocalGameSettings settings)
        {
            _viewController?.Refresh(settings);
        }

        private void OnGraphicsSettingsChanged(GraphicsSettingsData settings)
        {
            _viewController?.RefreshGraphics(settings);
            ApplySettingsPolicy();
        }

        private void OnMouseSensitivityChanged(float value) => _controlSettingsService?.SetMouseSensitivity(value);
        private void OnMovementSpeedChanged(float value) => _controlSettingsService?.SetMovementSpeed(value);
        private void OnOrbitSpeedChanged(float value) => _controlSettingsService?.SetOrbitSpeed(value);
        private void OnZoomSpeedChanged(float value) => _controlSettingsService?.SetZoomSpeed(value);

        private void OnControlBindingChanged(PlayerControlAction action, string controlPath)
        {
            if (_controlSettingsService == null)
                return;

            if (!_controlSettingsService.TrySetBinding(action, controlPath, out var conflictingAction))
            {
                _confirmationService?.Show(new ConfirmationRequest
                {
                    LabelText = "Control conflict",
                    MessageText = $"This key is already assigned to {conflictingAction}.",
                    OnConfirm = () => { }
                });
            }
        }

        private void OnResetControlsClicked() => _controlSettingsService?.ResetToDefaults();

        private void OnControlSettingsChanged(PlayerControlSettingsData settings)
        {
            _moyvaUiViewController?.RefreshControls(settings);
        }

        private void ApplySettingsPolicy()
        {
            bool humanMultiplayer = _lobbyService?.Current != null &&
                                    _lobbyService.Current.Players != null &&
                                    _lobbyService.Current.Players.Count > 1;
            _moyvaUiViewController?.SetGraphicsInteractable(!humanMultiplayer);
        }
    }
}
