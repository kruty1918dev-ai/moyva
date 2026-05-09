using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class GameSettingsPanelService : IGameSettingsPanelService, IInitializable, IDisposable
    {
        [InjectOptional] private IGameSettingsViewController _viewController;
        [Inject] private ILocalGameSettingsService _settingsService;
        [InjectOptional] private IConfirmationService _confirmationService;

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
            _viewController.OnDeleteSavesClicked -= OnDeleteSavesClicked;
            _viewController.OnDeleteSavesClicked += OnDeleteSavesClicked;
            _settingsService.OnSettingsChanged -= OnSettingsChanged;
            _settingsService.OnSettingsChanged += OnSettingsChanged;
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
                _viewController.OnDeleteSavesClicked -= OnDeleteSavesClicked;
            }

            if (_settingsService != null)
                _settingsService.OnSettingsChanged -= OnSettingsChanged;
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
    }
}