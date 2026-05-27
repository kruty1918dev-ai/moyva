using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Shared.Graphics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Головний контролер екрану налаштувань гри (аудіо, графіка, перформанс, сервісні дії).
    /// Працює як адаптер між UI-елементами й подіями API HomeMenu.
    /// </summary>
    public sealed class GameSettingsViewController : MonoBehaviour, IGameSettingsViewController, IInitializable
    {
        [SerializeField] private TMP_InputField _playerNameInput;
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Slider _uiVolumeSlider;
        [SerializeField] private Toggle _muteToggle;
        [SerializeField] private TMP_Dropdown _graphicsProfileDropdown;
        [SerializeField] private TMP_Dropdown _targetFrameRateDropdown;
        [SerializeField] private Slider _renderScaleSlider;
        [SerializeField] private TMP_Dropdown _textureQualityDropdown;
        [SerializeField] private TMP_Dropdown _antiAliasingDropdown;
        [SerializeField] private Toggle _dynamicRenderScaleToggle;
        [SerializeField] private Toggle _closeZoomOptimizationToggle;
        [SerializeField] private Toggle _vSyncToggle;
        [SerializeField] private Toggle _shadowsToggle;
        [SerializeField] private Toggle _anisotropicFilteringToggle;
        [SerializeField] private Slider _lodBiasSlider;
        [SerializeField] private Button _resetGraphicsButton;
        [SerializeField] private Button _deleteSavesButton;

        [Header("Slider Value Text")]
        [SerializeField] private TMP_Text _masterVolumeValueText;
        [SerializeField] private TMP_Text _musicVolumeValueText;
        [SerializeField] private TMP_Text _sfxVolumeValueText;
        [SerializeField] private TMP_Text _uiVolumeValueText;
        [SerializeField] private TMP_Text _renderScaleValueText;
        [SerializeField] private TMP_Text _lodBiasValueText;

        [Header("Slider Value Format")]
        [SerializeField] private string _volumeSliderValueFormat = "{0:0%}";
        [SerializeField] private string _renderScaleValueFormat = "{0:0%}";
        [SerializeField] private string _lodBiasValueFormat = "{0:0.##}";

        private static readonly int[] TargetFrameRateOptions = { 30, 45, 60, 90, 120 };
        private static readonly int[] AntiAliasingOptions = { 0, 2, 4 };
        private const float DefaultInputFontSize = 14f;

        private bool _bound;

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

        public string PlayerName
        {
            get => _playerNameInput != null ? _playerNameInput.text : string.Empty;
            set
            {
                if (_playerNameInput != null)
                    _playerNameInput.SetTextWithoutNotify(value ?? string.Empty);
            }
        }

        public float MasterVolume
        {
            get => GetSlider(_masterVolumeSlider);
            set
            {
                SetSlider(_masterVolumeSlider, value);
                UpdateSliderValueText(_masterVolumeSlider, _masterVolumeValueText, _volumeSliderValueFormat);
            }
        }

        public float MusicVolume
        {
            get => GetSlider(_musicVolumeSlider);
            set
            {
                SetSlider(_musicVolumeSlider, value);
                UpdateSliderValueText(_musicVolumeSlider, _musicVolumeValueText, _volumeSliderValueFormat);
            }
        }

        public float SfxVolume
        {
            get => GetSlider(_sfxVolumeSlider);
            set
            {
                SetSlider(_sfxVolumeSlider, value);
                UpdateSliderValueText(_sfxVolumeSlider, _sfxVolumeValueText, _volumeSliderValueFormat);
            }
        }

        public float UiVolume
        {
            get => GetSlider(_uiVolumeSlider);
            set
            {
                SetSlider(_uiVolumeSlider, value);
                UpdateSliderValueText(_uiVolumeSlider, _uiVolumeValueText, _volumeSliderValueFormat);
            }
        }

        public GraphicsQualityProfile GraphicsProfile
        {
            get => _graphicsProfileDropdown != null ? (GraphicsQualityProfile)Mathf.Clamp(_graphicsProfileDropdown.value, 0, 4) : GraphicsQualityProfile.Auto;
            set => SetDropdown(_graphicsProfileDropdown, Mathf.Clamp((int)value, 0, 4));
        }

        public int TargetFrameRate
        {
            get => GetMappedDropdownValue(_targetFrameRateDropdown, TargetFrameRateOptions, 60);
            set => SetDropdown(_targetFrameRateDropdown, FindOptionIndex(TargetFrameRateOptions, value));
        }

        public float RenderScale
        {
            get => _renderScaleSlider != null ? _renderScaleSlider.value : 1f;
            set
            {
                if (_renderScaleSlider != null)
                {
                    _renderScaleSlider.SetValueWithoutNotify(Mathf.Clamp(value, 0.42f, 1f));
                    UpdateSliderValueText(_renderScaleSlider, _renderScaleValueText, _renderScaleValueFormat);
                }
            }
        }

        public int TextureMipmapLimit
        {
            get => _textureQualityDropdown != null ? Mathf.Clamp(_textureQualityDropdown.value, 0, 3) : 0;
            set => SetDropdown(_textureQualityDropdown, Mathf.Clamp(value, 0, 3));
        }

        public int AntiAliasing
        {
            get => GetMappedDropdownValue(_antiAliasingDropdown, AntiAliasingOptions, 0);
            set => SetDropdown(_antiAliasingDropdown, FindOptionIndex(AntiAliasingOptions, value));
        }

        public bool DynamicRenderScale
        {
            get => false;
            set
            {
                if (_dynamicRenderScaleToggle != null)
                    _dynamicRenderScaleToggle.SetIsOnWithoutNotify(false);
            }
        }

        public bool CloseZoomOptimization
        {
            get => _closeZoomOptimizationToggle != null && _closeZoomOptimizationToggle.isOn;
            set { if (_closeZoomOptimizationToggle != null) _closeZoomOptimizationToggle.SetIsOnWithoutNotify(value); }
        }

        public bool VSync
        {
            get => _vSyncToggle != null && _vSyncToggle.isOn;
            set { if (_vSyncToggle != null) _vSyncToggle.SetIsOnWithoutNotify(value); }
        }

        public bool Shadows
        {
            get => _shadowsToggle != null && _shadowsToggle.isOn;
            set { if (_shadowsToggle != null) _shadowsToggle.SetIsOnWithoutNotify(value); }
        }

        public bool AnisotropicFiltering
        {
            get => _anisotropicFilteringToggle != null && _anisotropicFilteringToggle.isOn;
            set { if (_anisotropicFilteringToggle != null) _anisotropicFilteringToggle.SetIsOnWithoutNotify(value); }
        }

        public float LodBias
        {
            get => _lodBiasSlider != null ? _lodBiasSlider.value : 1f;
            set
            {
                if (_lodBiasSlider != null)
                {
                    _lodBiasSlider.SetValueWithoutNotify(Mathf.Clamp(value, 0.4f, 2f));
                    UpdateSliderValueText(_lodBiasSlider, _lodBiasValueText, _lodBiasValueFormat);
                }
            }
        }

        public bool IsMuted
        {
            get => _muteToggle != null && _muteToggle.isOn;
            set { if (_muteToggle != null) _muteToggle.SetIsOnWithoutNotify(value); }
        }

        private static float GetSlider(Slider s) => s != null ? s.value : 1f;
        private static void SetSlider(Slider s, float v) { if (s != null) s.SetValueWithoutNotify(Mathf.Clamp01(v)); }

        private void Awake() { Bind(); }
        public void Initialize() { Bind(); }

        private void OnDestroy()
        {
            if (!_bound) return;
            if (_playerNameInput != null) _playerNameInput.onEndEdit.RemoveListener(HandlePlayerNameChanged);
            if (_masterVolumeSlider != null) _masterVolumeSlider.onValueChanged.RemoveListener(HandleMaster);
            if (_musicVolumeSlider != null) _musicVolumeSlider.onValueChanged.RemoveListener(HandleMusic);
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.onValueChanged.RemoveListener(HandleSfx);
            if (_uiVolumeSlider != null) _uiVolumeSlider.onValueChanged.RemoveListener(HandleUi);
            if (_muteToggle != null) _muteToggle.onValueChanged.RemoveListener(HandleMute);
            if (_graphicsProfileDropdown != null) _graphicsProfileDropdown.onValueChanged.RemoveListener(HandleGraphicsProfile);
            if (_targetFrameRateDropdown != null) _targetFrameRateDropdown.onValueChanged.RemoveListener(HandleTargetFrameRate);
            if (_renderScaleSlider != null) _renderScaleSlider.onValueChanged.RemoveListener(HandleRenderScale);
            if (_textureQualityDropdown != null) _textureQualityDropdown.onValueChanged.RemoveListener(HandleTextureQuality);
            if (_antiAliasingDropdown != null) _antiAliasingDropdown.onValueChanged.RemoveListener(HandleAntiAliasing);
            if (_dynamicRenderScaleToggle != null) _dynamicRenderScaleToggle.onValueChanged.RemoveListener(HandleDynamicRenderScale);
            if (_closeZoomOptimizationToggle != null) _closeZoomOptimizationToggle.onValueChanged.RemoveListener(HandleCloseZoomOptimization);
            if (_vSyncToggle != null) _vSyncToggle.onValueChanged.RemoveListener(HandleVSync);
            if (_shadowsToggle != null) _shadowsToggle.onValueChanged.RemoveListener(HandleShadows);
            if (_anisotropicFilteringToggle != null) _anisotropicFilteringToggle.onValueChanged.RemoveListener(HandleAnisotropicFiltering);
            if (_lodBiasSlider != null) _lodBiasSlider.onValueChanged.RemoveListener(HandleLodBias);
            if (_resetGraphicsButton != null) _resetGraphicsButton.onClick.RemoveListener(HandleResetGraphicsClicked);
            if (_deleteSavesButton != null) _deleteSavesButton.onClick.RemoveListener(HandleDeleteSavesClicked);
            _bound = false;
        }

        public void Refresh(LocalGameSettings settings)
        {
            PlayerName = settings.PlayerName;
            MasterVolume = settings.MasterVolume;
            MusicVolume = settings.MusicVolume;
            SfxVolume = settings.SfxVolume;
            UiVolume = settings.UiVolume;
            IsMuted = settings.IsMuted;
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
        }

        public void SetInteractable(bool interactable)
        {
            if (_playerNameInput != null) _playerNameInput.interactable = interactable;
            if (_masterVolumeSlider != null) _masterVolumeSlider.interactable = interactable;
            if (_musicVolumeSlider != null) _musicVolumeSlider.interactable = interactable;
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.interactable = interactable;
            if (_uiVolumeSlider != null) _uiVolumeSlider.interactable = interactable;
            if (_muteToggle != null) _muteToggle.interactable = interactable;
            if (_graphicsProfileDropdown != null) _graphicsProfileDropdown.interactable = interactable;
            if (_targetFrameRateDropdown != null) _targetFrameRateDropdown.interactable = interactable;
            if (_renderScaleSlider != null) _renderScaleSlider.interactable = interactable;
            if (_textureQualityDropdown != null) _textureQualityDropdown.interactable = interactable;
            if (_antiAliasingDropdown != null) _antiAliasingDropdown.interactable = interactable;
            if (_dynamicRenderScaleToggle != null) _dynamicRenderScaleToggle.interactable = false;
            if (_closeZoomOptimizationToggle != null) _closeZoomOptimizationToggle.interactable = interactable;
            if (_vSyncToggle != null) _vSyncToggle.interactable = interactable;
            if (_shadowsToggle != null) _shadowsToggle.interactable = interactable;
            if (_anisotropicFilteringToggle != null) _anisotropicFilteringToggle.interactable = interactable;
            if (_lodBiasSlider != null) _lodBiasSlider.interactable = interactable;
            if (_resetGraphicsButton != null) _resetGraphicsButton.interactable = interactable;
            if (_deleteSavesButton != null) _deleteSavesButton.interactable = interactable;
        }

        private void Bind()
        {
            if (_bound) return;
            ConfigureGraphicsOptions();
            RefreshSliderValueTexts();
            if (_playerNameInput != null) _playerNameInput.onEndEdit.AddListener(HandlePlayerNameChanged);
            if (_masterVolumeSlider != null) _masterVolumeSlider.onValueChanged.AddListener(HandleMaster);
            if (_musicVolumeSlider != null) _musicVolumeSlider.onValueChanged.AddListener(HandleMusic);
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.onValueChanged.AddListener(HandleSfx);
            if (_uiVolumeSlider != null) _uiVolumeSlider.onValueChanged.AddListener(HandleUi);
            if (_muteToggle != null) _muteToggle.onValueChanged.AddListener(HandleMute);
            if (_graphicsProfileDropdown != null) _graphicsProfileDropdown.onValueChanged.AddListener(HandleGraphicsProfile);
            if (_targetFrameRateDropdown != null) _targetFrameRateDropdown.onValueChanged.AddListener(HandleTargetFrameRate);
            if (_renderScaleSlider != null) _renderScaleSlider.onValueChanged.AddListener(HandleRenderScale);
            if (_textureQualityDropdown != null) _textureQualityDropdown.onValueChanged.AddListener(HandleTextureQuality);
            if (_antiAliasingDropdown != null) _antiAliasingDropdown.onValueChanged.AddListener(HandleAntiAliasing);
            if (_dynamicRenderScaleToggle != null)
            {
                _dynamicRenderScaleToggle.SetIsOnWithoutNotify(false);
                _dynamicRenderScaleToggle.interactable = false;
            }
            if (_closeZoomOptimizationToggle != null) _closeZoomOptimizationToggle.onValueChanged.AddListener(HandleCloseZoomOptimization);
            if (_vSyncToggle != null) _vSyncToggle.onValueChanged.AddListener(HandleVSync);
            if (_shadowsToggle != null) _shadowsToggle.onValueChanged.AddListener(HandleShadows);
            if (_anisotropicFilteringToggle != null) _anisotropicFilteringToggle.onValueChanged.AddListener(HandleAnisotropicFiltering);
            if (_lodBiasSlider != null) _lodBiasSlider.onValueChanged.AddListener(HandleLodBias);
            if (_resetGraphicsButton != null) _resetGraphicsButton.onClick.AddListener(HandleResetGraphicsClicked);
            if (_deleteSavesButton != null) _deleteSavesButton.onClick.AddListener(HandleDeleteSavesClicked);
            _bound = true;
        }

        private void HandlePlayerNameChanged(string value)
        {
            OnPlayerNameChanged?.Invoke(value ?? string.Empty);
        }
        private void HandleMaster(float v)
        {
            UpdateSliderValueText(_masterVolumeValueText, v, _volumeSliderValueFormat);
            OnMasterVolumeChanged?.Invoke(v);
        }

        private void HandleMusic(float v)
        {
            UpdateSliderValueText(_musicVolumeValueText, v, _volumeSliderValueFormat);
            OnMusicVolumeChanged?.Invoke(v);
        }

        private void HandleSfx(float v)
        {
            UpdateSliderValueText(_sfxVolumeValueText, v, _volumeSliderValueFormat);
            OnSfxVolumeChanged?.Invoke(v);
        }

        private void HandleUi(float v)
        {
            UpdateSliderValueText(_uiVolumeValueText, v, _volumeSliderValueFormat);
            OnUiVolumeChanged?.Invoke(v);
        }
        private void HandleMute(bool v) => OnMutedChanged?.Invoke(v);
        private void HandleGraphicsProfile(int index) => OnGraphicsProfileChanged?.Invoke((GraphicsQualityProfile)Mathf.Clamp(index, 0, 4));
        private void HandleTargetFrameRate(int index) => OnTargetFrameRateChanged?.Invoke(TargetFrameRateOptions[Mathf.Clamp(index, 0, TargetFrameRateOptions.Length - 1)]);
        private void HandleRenderScale(float value)
        {
            float clampedValue = Mathf.Clamp(value, 0.42f, 1f);
            UpdateSliderValueText(_renderScaleValueText, clampedValue, _renderScaleValueFormat);
            OnRenderScaleChanged?.Invoke(clampedValue);
        }
        private void HandleTextureQuality(int index) => OnTextureMipmapLimitChanged?.Invoke(Mathf.Clamp(index, 0, 3));
        private void HandleAntiAliasing(int index) => OnAntiAliasingChanged?.Invoke(AntiAliasingOptions[Mathf.Clamp(index, 0, AntiAliasingOptions.Length - 1)]);
        private void HandleDynamicRenderScale(bool v) => OnDynamicRenderScaleChanged?.Invoke(false);
        private void HandleCloseZoomOptimization(bool v) => OnCloseZoomOptimizationChanged?.Invoke(v);
        private void HandleVSync(bool v) => OnVSyncChanged?.Invoke(v);
        private void HandleShadows(bool v) => OnShadowsChanged?.Invoke(v);
        private void HandleAnisotropicFiltering(bool v) => OnAnisotropicFilteringChanged?.Invoke(v);
        private void HandleLodBias(float value)
        {
            float clampedValue = Mathf.Clamp(value, 0.4f, 2f);
            UpdateSliderValueText(_lodBiasValueText, clampedValue, _lodBiasValueFormat);
            OnLodBiasChanged?.Invoke(clampedValue);
        }
        private void HandleResetGraphicsClicked() => OnResetGraphicsClicked?.Invoke();
        private void HandleDeleteSavesClicked() => OnDeleteSavesClicked?.Invoke();

        private void ConfigureGraphicsOptions()
        {
            FillDropdown(_graphicsProfileDropdown, "Auto", "Performance", "Balanced", "Quality", "Custom");
            FillDropdown(_targetFrameRateDropdown, "30", "45", "60", "90", "120");
            FillDropdown(_textureQualityDropdown, "Full", "1/2", "1/4", "1/8");
            FillDropdown(_antiAliasingDropdown, "Off", "2x", "4x");

            if (_renderScaleSlider != null)
            {
                _renderScaleSlider.minValue = 0.42f;
                _renderScaleSlider.maxValue = 1f;
                _renderScaleSlider.wholeNumbers = false;
            }

            if (_lodBiasSlider != null)
            {
                _lodBiasSlider.minValue = 0.4f;
                _lodBiasSlider.maxValue = 2f;
                _lodBiasSlider.wholeNumbers = false;
            }
        }

        private static void FillDropdown(TMP_Dropdown dropdown, params string[] labels)
        {
            if (dropdown == null)
                return;

            dropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>(labels.Length);
            for (int i = 0; i < labels.Length; i++)
                options.Add(new TMP_Dropdown.OptionData(labels[i]));

            dropdown.AddOptions(options);
        }

        private static void SetDropdown(TMP_Dropdown dropdown, int index)
        {
            if (dropdown == null)
                return;

            int maxIndex = Mathf.Max(0, dropdown.options.Count - 1);
            dropdown.SetValueWithoutNotify(Mathf.Clamp(index, 0, maxIndex));
            dropdown.RefreshShownValue();
        }

        private static int GetMappedDropdownValue(TMP_Dropdown dropdown, int[] values, int fallback)
        {
            if (dropdown == null || values == null || values.Length == 0)
                return fallback;

            int index = Mathf.Clamp(dropdown.value, 0, values.Length - 1);
            return values[index];
        }

        private static int FindOptionIndex(int[] values, int value)
        {
            if (values == null || values.Length == 0)
                return 0;

            int closestIndex = 0;
            int closestDelta = Mathf.Abs(values[0] - value);
            for (int i = 1; i < values.Length; i++)
            {
                int delta = Mathf.Abs(values[i] - value);
                if (delta >= closestDelta)
                    continue;

                closestIndex = i;
                closestDelta = delta;
            }

            return closestIndex;
        }

        private void RefreshSliderValueTexts()
        {
            UpdateSliderValueText(_masterVolumeSlider, _masterVolumeValueText, _volumeSliderValueFormat);
            UpdateSliderValueText(_musicVolumeSlider, _musicVolumeValueText, _volumeSliderValueFormat);
            UpdateSliderValueText(_sfxVolumeSlider, _sfxVolumeValueText, _volumeSliderValueFormat);
            UpdateSliderValueText(_uiVolumeSlider, _uiVolumeValueText, _volumeSliderValueFormat);
            UpdateSliderValueText(_renderScaleSlider, _renderScaleValueText, _renderScaleValueFormat);
            UpdateSliderValueText(_lodBiasSlider, _lodBiasValueText, _lodBiasValueFormat);
        }

        private static void UpdateSliderValueText(Slider slider, TMP_Text valueText, string format)
        {
            if (slider == null)
                return;

            UpdateSliderValueText(valueText, slider.value, format);
        }

        private static void UpdateSliderValueText(TMP_Text valueText, float value, string format)
        {
            if (valueText == null)
                return;

            valueText.text = FormatSliderValue(value, format);
            valueText.ForceMeshUpdate();
        }

        private static string FormatSliderValue(float value, string format)
        {
            string effectiveFormat = string.IsNullOrWhiteSpace(format) ? "{0:0.##}" : format;
            try
            {
                return string.Format(effectiveFormat, value);
            }
            catch (FormatException)
            {
                return value.ToString("0.##");
            }
        }
    }
}
