using System;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Shared.Audio;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    public sealed class InGamePauseMenuController : MonoBehaviour, IInitializable, IDisposable
    {
        [Header("Panels")]
        [SerializeField] private GameObject _rootPanel;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _exitConfirmPanel;

        [Header("Main Buttons")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        [Header("Settings Buttons")]
        [SerializeField] private Button _settingsBackButton;
        [SerializeField] private Button _graphicsLowButton;
        [SerializeField] private Button _graphicsBalancedButton;
        [SerializeField] private Button _graphicsHighButton;
        [SerializeField] private Button _graphicsResetButton;

        [Header("Audio Settings")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;

        [Header("Graphics Settings")]
        [SerializeField] private Slider _renderScaleSlider;
        [SerializeField] private Toggle _vSyncToggle;

        [Header("Confirm Buttons")]
        [SerializeField] private Button _exitConfirmYesButton;
        [SerializeField] private Button _exitConfirmNoButton;

        [Header("Scene")]
        [SerializeField] private string _homeMenuSceneName = "HomeMenu";

        private SignalBus _signalBus;
        private IGameStateService _gameStateService;
        private IAudioSettingsService _audioSettingsService;
        private IGraphicsSettingsService _graphicsSettingsService;
        private ISessionManager _sessionManager;
        private ISaveService _saveService;
        private bool _isMenuVisible;
        private bool _isSettingsVisible;
        private bool _isExitConfirmVisible;
        private bool _cachedCursorVisible;
        private CursorLockMode _cachedCursorLockMode;

        [Inject]
        private void Construct(
            SignalBus signalBus,
            IGameStateService gameStateService,
            [InjectOptional] IAudioSettingsService audioSettingsService,
            [InjectOptional] IGraphicsSettingsService graphicsSettingsService,
            [InjectOptional] ISessionManager sessionManager,
            [InjectOptional] ISaveService saveService)
        {
            _signalBus = signalBus;
            _gameStateService = gameStateService;
            _audioSettingsService = audioSettingsService;
            _graphicsSettingsService = graphicsSettingsService;
            _sessionManager = sessionManager;
            _saveService = saveService;
        }

        public void Initialize()
        {
            BindButton(_continueButton, OnContinueClicked);
            BindButton(_settingsButton, OnSettingsClicked);
            BindButton(_exitButton, OnExitClicked);
            BindButton(_settingsBackButton, OnSettingsBackClicked);
            BindButton(_graphicsLowButton, OnGraphicsLowClicked);
            BindButton(_graphicsBalancedButton, OnGraphicsBalancedClicked);
            BindButton(_graphicsHighButton, OnGraphicsHighClicked);
            BindButton(_graphicsResetButton, OnResetGraphicsClicked);
            BindButton(_exitConfirmYesButton, OnConfirmExitClicked);
            BindButton(_exitConfirmNoButton, OnCancelExitClicked);

            BindSlider(_masterVolumeSlider, OnMasterVolumeChanged);
            BindSlider(_musicVolumeSlider, OnMusicVolumeChanged);
            BindSlider(_sfxVolumeSlider, OnSfxVolumeChanged);
            BindSlider(_renderScaleSlider, OnRenderScaleChanged);

            BindToggle(_vSyncToggle, OnVSyncChanged);

            if (_signalBus != null)
                _signalBus.Subscribe<GamePausedSignal>(OnGamePausedChanged);

            if (_audioSettingsService != null)
            {
                _audioSettingsService.OnVolumesChanged -= OnAudioVolumesChanged;
                _audioSettingsService.OnVolumesChanged += OnAudioVolumesChanged;
                OnAudioVolumesChanged(_audioSettingsService.MasterVolume, _audioSettingsService.MusicVolume, _audioSettingsService.SfxVolume);
            }

            if (_graphicsSettingsService != null)
            {
                _graphicsSettingsService.OnSettingsChanged -= OnGraphicsSettingsChanged;
                _graphicsSettingsService.OnSettingsChanged += OnGraphicsSettingsChanged;
                OnGraphicsSettingsChanged(_graphicsSettingsService.Settings);
            }

            HideAllPanels();
            RestoreCursor();
        }

        public void Dispose()
        {
            UnbindButton(_continueButton, OnContinueClicked);
            UnbindButton(_settingsButton, OnSettingsClicked);
            UnbindButton(_exitButton, OnExitClicked);
            UnbindButton(_settingsBackButton, OnSettingsBackClicked);
            UnbindButton(_graphicsLowButton, OnGraphicsLowClicked);
            UnbindButton(_graphicsBalancedButton, OnGraphicsBalancedClicked);
            UnbindButton(_graphicsHighButton, OnGraphicsHighClicked);
            UnbindButton(_graphicsResetButton, OnResetGraphicsClicked);
            UnbindButton(_exitConfirmYesButton, OnConfirmExitClicked);
            UnbindButton(_exitConfirmNoButton, OnCancelExitClicked);

            UnbindSlider(_masterVolumeSlider, OnMasterVolumeChanged);
            UnbindSlider(_musicVolumeSlider, OnMusicVolumeChanged);
            UnbindSlider(_sfxVolumeSlider, OnSfxVolumeChanged);
            UnbindSlider(_renderScaleSlider, OnRenderScaleChanged);

            UnbindToggle(_vSyncToggle, OnVSyncChanged);

            if (_signalBus != null)
                _signalBus.TryUnsubscribe<GamePausedSignal>(OnGamePausedChanged);

            if (_audioSettingsService != null)
                _audioSettingsService.OnVolumesChanged -= OnAudioVolumesChanged;

            if (_graphicsSettingsService != null)
                _graphicsSettingsService.OnSettingsChanged -= OnGraphicsSettingsChanged;
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
                return;

            HandlePauseShortcut();
        }

        private void HandlePauseShortcut()
        {
            if (!_isMenuVisible)
            {
                OpenMenu();
                return;
            }

            if (_isExitConfirmVisible)
            {
                OnCancelExitClicked();
                return;
            }

            if (_isSettingsVisible)
            {
                ShowMainPanel();
                return;
            }

            OnContinueClicked();
        }

        private void OpenMenu()
        {
            CacheCursorState();
            SetRootVisible(true);
            ShowMainPanel();

            if (ShouldPauseGameplay())
                _gameStateService.PauseGame();
        }

        private void OnContinueClicked()
        {
            HideExitConfirm();
            HideSettingsPanel();
            CloseMenu();
        }

        private void OnSettingsClicked()
        {
            if (!_isMenuVisible)
                OpenMenu();

            ShowSettingsPanel();
        }

        private void OnSettingsBackClicked()
        {
            ShowMainPanel();
        }

        private void OnExitClicked()
        {
            if (HasOtherHumanParticipants())
            {
                ShowExitConfirm();
                return;
            }

            ExitToHomeMenu();
        }

        private void OnConfirmExitClicked()
        {
            HideExitConfirm();
            LeaveSessionAndExitAsync();
        }

        private void OnCancelExitClicked()
        {
            HideExitConfirm();
            ShowMainPanel();
        }

        private void OnGamePausedChanged(GamePausedSignal signal)
        {
            if (signal.IsPaused)
            {
                if (!_isMenuVisible)
                    CacheCursorState();

                SetRootVisible(true);
                ShowMainPanel();
            }
            else
            {
                HideAllPanels();
                RestoreCursor();
            }
        }

        private void OnAudioVolumesChanged(float masterVolume, float musicVolume, float sfxVolume)
        {
            SetSliderValue(_masterVolumeSlider, masterVolume);
            SetSliderValue(_musicVolumeSlider, musicVolume);
            SetSliderValue(_sfxVolumeSlider, sfxVolume);
        }

        private void OnGraphicsSettingsChanged(GraphicsSettingsData settings)
        {
            SetSliderValue(_renderScaleSlider, settings.RenderScale);
            SetToggleValue(_vSyncToggle, settings.VSync);
        }

        private void OnMasterVolumeChanged(float value)
        {
            _audioSettingsService?.SetMasterVolume(value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            _audioSettingsService?.SetMusicVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            _audioSettingsService?.SetSfxVolume(value);
        }

        private void OnRenderScaleChanged(float value)
        {
            _graphicsSettingsService?.SetRenderScale(value);
        }

        private void OnVSyncChanged(bool enabled)
        {
            _graphicsSettingsService?.SetVSync(enabled);
        }

        private void OnResetGraphicsClicked()
        {
            _graphicsSettingsService?.ResetToDefaults();
        }

        private void OnGraphicsLowClicked()
        {
            SetGraphicsProfile(GraphicsQualityProfile.Performance);
        }

        private void OnGraphicsBalancedClicked()
        {
            SetGraphicsProfile(GraphicsQualityProfile.Balanced);
        }

        private void OnGraphicsHighClicked()
        {
            SetGraphicsProfile(GraphicsQualityProfile.Quality);
        }

        private void SetGraphicsProfile(GraphicsQualityProfile profile)
        {
            _graphicsSettingsService?.SetProfile(profile);
        }

        private void ShowMainPanel()
        {
            _isMenuVisible = true;
            _isSettingsVisible = false;
            _isExitConfirmVisible = false;
            SetRootVisible(true);
            SetPanelState(_mainPanel, true);
            SetPanelState(_settingsPanel, false);
            SetPanelState(_exitConfirmPanel, false);
        }

        private void ShowSettingsPanel()
        {
            _isMenuVisible = true;
            _isSettingsVisible = true;
            _isExitConfirmVisible = false;
            SetRootVisible(true);
            SetPanelState(_mainPanel, false);
            SetPanelState(_settingsPanel, true);
            SetPanelState(_exitConfirmPanel, false);
        }

        private void ShowExitConfirm()
        {
            _isMenuVisible = true;
            _isSettingsVisible = false;
            _isExitConfirmVisible = true;
            SetRootVisible(true);
            SetPanelState(_mainPanel, false);
            SetPanelState(_settingsPanel, false);
            SetPanelState(_exitConfirmPanel, true);
        }

        private void HideExitConfirm()
        {
            _isExitConfirmVisible = false;
            SetPanelState(_exitConfirmPanel, false);
        }

        private void HideSettingsPanel()
        {
            _isSettingsVisible = false;
            SetPanelState(_settingsPanel, false);
        }

        private void HideAllPanels()
        {
            _isMenuVisible = false;
            _isSettingsVisible = false;
            _isExitConfirmVisible = false;
            SetPanelState(_rootPanel, false);
            SetPanelState(_mainPanel, false);
            SetPanelState(_settingsPanel, false);
            SetPanelState(_exitConfirmPanel, false);
        }

        private void CloseMenu()
        {
            _isMenuVisible = false;
            _isSettingsVisible = false;
            _isExitConfirmVisible = false;
            SetRootVisible(false);
            RestoreCursor();

            if (ShouldPauseGameplay())
                _gameStateService.ResumeGame();
        }

        private void SetRootVisible(bool visible)
        {
            if (_rootPanel != null)
                _rootPanel.SetActive(visible);
        }

        private static void SetPanelState(GameObject panel, bool visible)
        {
            if (panel != null)
                panel.SetActive(visible);
        }

        private static void SetSliderValue(Slider slider, float value)
        {
            if (slider != null)
                slider.SetValueWithoutNotify(value);
        }

        private static void SetToggleValue(Toggle toggle, bool value)
        {
            if (toggle != null)
                toggle.SetIsOnWithoutNotify(value);
        }

        private bool ShouldPauseGameplay()
        {
            return !HasOtherHumanParticipants();
        }

        private bool HasOtherHumanParticipants()
        {
            if (_sessionManager == null)
                return false;

            string localPlayerId = _sessionManager.LocalPlayerId;
            if (string.IsNullOrWhiteSpace(localPlayerId))
                return false;

            foreach (var participant in _sessionManager.Participants)
            {
                if (participant == null || participant.IsBot)
                    continue;

                string playerId = participant.Identity?.PlayerId;
                if (!string.IsNullOrWhiteSpace(playerId) && string.Equals(playerId, localPlayerId, StringComparison.Ordinal))
                    continue;

                return true;
            }

            return false;
        }

        private void ExitToHomeMenu()
        {
            if (ShouldPauseGameplay())
                _gameStateService.ResumeGame();

            _saveService?.Save();
            RestoreCursor();
            SceneManager.LoadScene(_homeMenuSceneName, LoadSceneMode.Single);
        }

        private async void LeaveSessionAndExitAsync()
        {
            if (ShouldPauseGameplay())
                _gameStateService.ResumeGame();

            try
            {
                if (_sessionManager != null)
                    await _sessionManager.LeaveSessionAsync();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[InGamePauseMenuController] LeaveSessionAsync failed: {exception.Message}");
            }

            RestoreCursor();
            SceneManager.LoadScene(_homeMenuSceneName, LoadSceneMode.Single);
        }

        private void CacheCursorState()
        {
            _cachedCursorVisible = Cursor.visible;
            _cachedCursorLockMode = Cursor.lockState;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void RestoreCursor()
        {
            Cursor.visible = _cachedCursorVisible;
            Cursor.lockState = _cachedCursorLockMode;
        }

        private static void BindButton(Button button, Action handler)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(handler);
            button.onClick.AddListener(handler);
        }

        private static void UnbindButton(Button button, Action handler)
        {
            if (button != null)
                button.onClick.RemoveListener(handler);
        }

        private static void BindSlider(Slider slider, Action<float> handler)
        {
            if (slider == null)
                return;

            slider.onValueChanged.RemoveListener(handler);
            slider.onValueChanged.AddListener(handler);
        }

        private static void UnbindSlider(Slider slider, Action<float> handler)
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(handler);
        }

        private static void BindToggle(Toggle toggle, Action<bool> handler)
        {
            if (toggle == null)
                return;

            toggle.onValueChanged.RemoveListener(handler);
            toggle.onValueChanged.AddListener(handler);
        }

        private static void UnbindToggle(Toggle toggle, Action<bool> handler)
        {
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(handler);
        }
    }
}