using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Shared.Graphics;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class GameSettingsPanelService : IGameSettingsPanelService, IInitializable, IDisposable
    {
        [InjectOptional] private IGameSettingsViewController _viewController;
        [Inject] private ILocalGameSettingsService _settingsService;
        [InjectOptional] private IConfirmationService _confirmationService;
        private IGraphicsSettingsService _graphicsSettingsService;

        [Inject]
        private void Construct([InjectOptional] IGraphicsSettingsService graphicsSettingsService)
        {
            _graphicsSettingsService = graphicsSettingsService;
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
                LabelText = "Підтвердження",
                MessageText = "Видалити всі локальні збереження?",
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
        }
    }
}