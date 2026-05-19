using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
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

        private static readonly int[] TargetFrameRateOptions = { 30, 45, 60, 90, 120 };
        private static readonly int[] AntiAliasingOptions = { 0, 2, 4 };

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
            set { if (_playerNameInput != null) _playerNameInput.SetTextWithoutNotify(value ?? string.Empty); }
        }

        public float MasterVolume { get => GetSlider(_masterVolumeSlider); set => SetSlider(_masterVolumeSlider, value); }
        public float MusicVolume { get => GetSlider(_musicVolumeSlider); set => SetSlider(_musicVolumeSlider, value); }
        public float SfxVolume { get => GetSlider(_sfxVolumeSlider); set => SetSlider(_sfxVolumeSlider, value); }
        public float UiVolume { get => GetSlider(_uiVolumeSlider); set => SetSlider(_uiVolumeSlider, value); }

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
                    _renderScaleSlider.SetValueWithoutNotify(Mathf.Clamp(value, 0.42f, 1f));
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
            get => _dynamicRenderScaleToggle != null && _dynamicRenderScaleToggle.isOn;
            set { if (_dynamicRenderScaleToggle != null) _dynamicRenderScaleToggle.SetIsOnWithoutNotify(value); }
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
                    _lodBiasSlider.SetValueWithoutNotify(Mathf.Clamp(value, 0.4f, 2f));
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
            if (_dynamicRenderScaleToggle != null) _dynamicRenderScaleToggle.interactable = interactable;
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
            // 1: Захист від повторного биндингу підписок.
            if (_bound) return;
            // 2: Гарантуємо наявність runtime-контролів, якщо частина полів не виставлена в сцені.
            EnsureRuntimeControls();
            // 3: Налаштовуємо options/діапазони і підписуємо всі UI події.
            ConfigureGraphicsOptions();
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
            if (_dynamicRenderScaleToggle != null) _dynamicRenderScaleToggle.onValueChanged.AddListener(HandleDynamicRenderScale);
            if (_closeZoomOptimizationToggle != null) _closeZoomOptimizationToggle.onValueChanged.AddListener(HandleCloseZoomOptimization);
            if (_vSyncToggle != null) _vSyncToggle.onValueChanged.AddListener(HandleVSync);
            if (_shadowsToggle != null) _shadowsToggle.onValueChanged.AddListener(HandleShadows);
            if (_anisotropicFilteringToggle != null) _anisotropicFilteringToggle.onValueChanged.AddListener(HandleAnisotropicFiltering);
            if (_lodBiasSlider != null) _lodBiasSlider.onValueChanged.AddListener(HandleLodBias);
            if (_resetGraphicsButton != null) _resetGraphicsButton.onClick.AddListener(HandleResetGraphicsClicked);
            if (_deleteSavesButton != null) _deleteSavesButton.onClick.AddListener(HandleDeleteSavesClicked);
            _bound = true;
        }

        private void HandlePlayerNameChanged(string value) => OnPlayerNameChanged?.Invoke(value);
        private void HandleMaster(float v) => OnMasterVolumeChanged?.Invoke(v);
        private void HandleMusic(float v) => OnMusicVolumeChanged?.Invoke(v);
        private void HandleSfx(float v) => OnSfxVolumeChanged?.Invoke(v);
        private void HandleUi(float v) => OnUiVolumeChanged?.Invoke(v);
        private void HandleMute(bool v) => OnMutedChanged?.Invoke(v);
        private void HandleGraphicsProfile(int index) => OnGraphicsProfileChanged?.Invoke((GraphicsQualityProfile)Mathf.Clamp(index, 0, 4));
        private void HandleTargetFrameRate(int index) => OnTargetFrameRateChanged?.Invoke(TargetFrameRateOptions[Mathf.Clamp(index, 0, TargetFrameRateOptions.Length - 1)]);
        private void HandleRenderScale(float value) => OnRenderScaleChanged?.Invoke(Mathf.Clamp(value, 0.42f, 1f));
        private void HandleTextureQuality(int index) => OnTextureMipmapLimitChanged?.Invoke(Mathf.Clamp(index, 0, 3));
        private void HandleAntiAliasing(int index) => OnAntiAliasingChanged?.Invoke(AntiAliasingOptions[Mathf.Clamp(index, 0, AntiAliasingOptions.Length - 1)]);
        private void HandleDynamicRenderScale(bool v) => OnDynamicRenderScaleChanged?.Invoke(v);
        private void HandleCloseZoomOptimization(bool v) => OnCloseZoomOptimizationChanged?.Invoke(v);
        private void HandleVSync(bool v) => OnVSyncChanged?.Invoke(v);
        private void HandleShadows(bool v) => OnShadowsChanged?.Invoke(v);
        private void HandleAnisotropicFiltering(bool v) => OnAnisotropicFilteringChanged?.Invoke(v);
        private void HandleLodBias(float value) => OnLodBiasChanged?.Invoke(Mathf.Clamp(value, 0.4f, 2f));
        private void HandleResetGraphicsClicked() => OnResetGraphicsClicked?.Invoke();
        private void HandleDeleteSavesClicked() => OnDeleteSavesClicked?.Invoke();

        private void EnsureRuntimeControls()
        {
            var parent = _masterVolumeSlider != null && _masterVolumeSlider.transform.parent != null ? _masterVolumeSlider.transform.parent : transform;

            if (_masterVolumeSlider != null)
            {
                _musicVolumeSlider ??= CloneSlider(parent, _masterVolumeSlider, "Slider_MusicVolume", 0.7f);
                _sfxVolumeSlider ??= CloneSlider(parent, _masterVolumeSlider, "Slider_SfxVolume", 0.9f);
                _uiVolumeSlider ??= CloneSlider(parent, _masterVolumeSlider, "Slider_UiVolume", 0.9f);
            }

            _muteToggle ??= CreateMuteToggle(parent);
            EnsureGraphicsRuntimeControls(parent);
        }

        private void EnsureGraphicsRuntimeControls(Transform parent)
        {
            bool needsHeader = _graphicsProfileDropdown == null || _targetFrameRateDropdown == null || _renderScaleSlider == null;
            if (needsHeader)
                HomeMenuRuntimeUiFactory.CreateText(parent, "Label_GraphicsSettings", "Графіка", 18, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);

            _graphicsProfileDropdown ??= CreateLabeledDropdown(parent, "Dropdown_GraphicsProfile", "Профіль");
            _targetFrameRateDropdown ??= CreateLabeledDropdown(parent, "Dropdown_TargetFrameRate", "FPS");
            _renderScaleSlider ??= CreateLabeledSlider(parent, "Slider_RenderScale", "Масштаб", 0.42f, 1f, 0.75f);
            _textureQualityDropdown ??= CreateLabeledDropdown(parent, "Dropdown_TextureQuality", "Текстури");
            _antiAliasingDropdown ??= CreateLabeledDropdown(parent, "Dropdown_AntiAliasing", "Згладжування");
            _dynamicRenderScaleToggle ??= CreateLabeledToggle(parent, "Toggle_DynamicRenderScale", "Авто масштаб");
            _closeZoomOptimizationToggle ??= CreateLabeledToggle(parent, "Toggle_CloseZoomOptimization", "Оптимізація зуму");
            _vSyncToggle ??= CreateLabeledToggle(parent, "Toggle_VSync", "VSync");
            _shadowsToggle ??= CreateLabeledToggle(parent, "Toggle_Shadows", "Тіні");
            _anisotropicFilteringToggle ??= CreateLabeledToggle(parent, "Toggle_AnisotropicFiltering", "Anisotropic");
            _lodBiasSlider ??= CreateLabeledSlider(parent, "Slider_LodBias", "LOD", 0.4f, 2f, 0.85f);
            _resetGraphicsButton ??= HomeMenuRuntimeUiFactory.CreateButton(parent, "Button_ResetGraphics", "Скинути графіку", new Vector2(180f, 42f));
        }

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

        private static Slider CloneSlider(Transform parent, Slider source, string name, float value)
        {
            var clone = Instantiate(source.gameObject, parent, false);
            clone.name = name;
            clone.SetActive(true);
            var slider = clone.GetComponent<Slider>();
            slider.SetValueWithoutNotify(Mathf.Clamp01(value));
            return slider;
        }

        private static Toggle CreateMuteToggle(Transform parent)
        {
            var row = new GameObject("Toggle_Mute", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            var toggleObject = new GameObject("Control", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
            toggleObject.transform.SetParent(row.transform, false);
            var toggleRect = toggleObject.GetComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(28f, 28f);
            var background = toggleObject.GetComponent<Image>();
            background.color = new Color(0.18f, 0.19f, 0.24f, 1f);

            var checkmarkObject = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkmarkObject.transform.SetParent(toggleObject.transform, false);
            var checkmarkRect = checkmarkObject.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;
            var checkmark = checkmarkObject.GetComponent<Image>();
            checkmark.color = new Color(0.22f, 0.42f, 0.84f, 1f);

            var toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = checkmark;
            var label = HomeMenuRuntimeUiFactory.CreateText(row.transform, "Label", "Mute", 16, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            SetFlexible(label.gameObject, preferredWidth: 150f, preferredHeight: 34f);
            return toggle;
        }

        private static TMP_Dropdown CreateLabeledDropdown(Transform parent, string name, string label)
        {
            var row = CreateControlRow(parent, $"Row_{name}");
            var labelText = HomeMenuRuntimeUiFactory.CreateText(row.transform, "Label", label, 15, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            SetFlexible(labelText.gameObject, preferredWidth: 150f, preferredHeight: 34f);
            var dropdown = CreateRuntimeDropdown(row.transform, name);
            SetFlexible(dropdown.gameObject, preferredWidth: 210f, preferredHeight: 40f);
            return dropdown;
        }

        private static TMP_Dropdown CreateRuntimeDropdown(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_Dropdown));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(210f, 40f);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.15f, 0.16f, 0.21f, 1f);

            var caption = HomeMenuRuntimeUiFactory.CreateText(go.transform, "Caption", string.Empty, 15, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            var captionRect = caption.GetComponent<RectTransform>();
            Stretch(captionRect);
            captionRect.offsetMin = new Vector2(12f, 3f);
            captionRect.offsetMax = new Vector2(-34f, -3f);

            var arrow = HomeMenuRuntimeUiFactory.CreateText(go.transform, "Arrow", "v", 14, FontStyles.Bold, TextAlignmentOptions.Center);
            var arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1f, 0f);
            arrowRect.anchorMax = new Vector2(1f, 1f);
            arrowRect.pivot = new Vector2(1f, 0.5f);
            arrowRect.sizeDelta = new Vector2(30f, 0f);
            arrowRect.anchoredPosition = Vector2.zero;

            var template = CreateDropdownTemplate(go.transform, out var itemLabel);
            var dropdown = go.GetComponent<TMP_Dropdown>();
            dropdown.targetGraphic = image;
            dropdown.captionText = caption;
            dropdown.template = template;
            dropdown.itemText = itemLabel;
            return dropdown;
        }

        private static RectTransform CreateDropdownTemplate(Transform parent, out TextMeshProUGUI itemLabel)
        {
            var templateObject = new GameObject("Template", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
            templateObject.transform.SetParent(parent, false);
            templateObject.SetActive(false);
            var templateRect = templateObject.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = Vector2.zero;
            templateRect.sizeDelta = new Vector2(0f, 170f);
            templateObject.GetComponent<Image>().color = new Color(0.10f, 0.11f, 0.15f, 0.98f);

            var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportObject.transform.SetParent(templateObject.transform, false);
            var viewportRect = viewportObject.GetComponent<RectTransform>();
            Stretch(viewportRect);

            var contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewportObject.transform, false);
            var contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);
            var contentLayout = contentObject.GetComponent<VerticalLayoutGroup>();
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var itemObject = new GameObject("Item", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
            itemObject.transform.SetParent(contentObject.transform, false);
            var itemRect = itemObject.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0f, 34f);
            itemObject.GetComponent<Image>().color = new Color(0.15f, 0.16f, 0.21f, 1f);
            SetFlexible(itemObject, preferredHeight: 34f);

            itemLabel = HomeMenuRuntimeUiFactory.CreateText(itemObject.transform, "Item Label", "Option", 15, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            var itemLabelRect = itemLabel.GetComponent<RectTransform>();
            Stretch(itemLabelRect);
            itemLabelRect.offsetMin = new Vector2(12f, 2f);
            itemLabelRect.offsetMax = new Vector2(-12f, -2f);

            var toggle = itemObject.GetComponent<Toggle>();
            toggle.targetGraphic = itemObject.GetComponent<Image>();

            var scrollRect = templateObject.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            return templateRect;
        }

        private static Slider CreateLabeledSlider(Transform parent, string name, string label, float min, float max, float value)
        {
            var row = CreateControlRow(parent, $"Row_{name}");
            var labelText = HomeMenuRuntimeUiFactory.CreateText(row.transform, "Label", label, 15, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            SetFlexible(labelText.gameObject, preferredWidth: 150f, preferredHeight: 34f);

            var sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(row.transform, false);
            var sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(210f, 32f);

            var background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            background.transform.SetParent(sliderObject.transform, false);
            var backgroundRect = background.GetComponent<RectTransform>();
            Stretch(backgroundRect);
            backgroundRect.offsetMin = new Vector2(0f, 12f);
            backgroundRect.offsetMax = new Vector2(0f, -12f);
            background.GetComponent<Image>().color = new Color(0.15f, 0.16f, 0.21f, 1f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            Stretch(fillAreaRect);
            fillAreaRect.offsetMin = new Vector2(0f, 12f);
            fillAreaRect.offsetMax = new Vector2(0f, -12f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            Stretch(fillRect);
            fill.GetComponent<Image>().color = new Color(0.22f, 0.42f, 0.84f, 1f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(sliderObject.transform, false);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(22f, 22f);
            handle.GetComponent<Image>().color = new Color(0.94f, 0.95f, 0.98f, 1f);

            var slider = sliderObject.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.SetValueWithoutNotify(Mathf.Clamp(value, min, max));
            SetFlexible(sliderObject, preferredWidth: 210f, preferredHeight: 32f);
            return slider;
        }

        private static Toggle CreateLabeledToggle(Transform parent, string name, string label)
        {
            var row = CreateControlRow(parent, $"Row_{name}");
            var labelText = HomeMenuRuntimeUiFactory.CreateText(row.transform, "Label", label, 15, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            SetFlexible(labelText.gameObject, preferredWidth: 150f, preferredHeight: 34f);

            var toggleObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
            toggleObject.transform.SetParent(row.transform, false);
            var toggleRect = toggleObject.GetComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(28f, 28f);
            var background = toggleObject.GetComponent<Image>();
            background.color = new Color(0.18f, 0.19f, 0.24f, 1f);

            var checkmarkObject = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkmarkObject.transform.SetParent(toggleObject.transform, false);
            var checkmarkRect = checkmarkObject.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;
            checkmarkObject.GetComponent<Image>().color = new Color(0.22f, 0.42f, 0.84f, 1f);

            var toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = checkmarkObject.GetComponent<Image>();
            return toggle;
        }

        private static GameObject CreateControlRow(Transform parent, string name)
        {
            var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            SetFlexible(row, preferredHeight: 42f);
            return row;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetFlexible(GameObject target, float preferredWidth = -1f, float preferredHeight = -1f)
        {
            var element = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
            if (preferredWidth >= 0f)
                element.preferredWidth = preferredWidth;
            if (preferredHeight >= 0f)
                element.preferredHeight = preferredHeight;
        }
    }
}
