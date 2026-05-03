using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed class GameSettingsViewController : MonoBehaviour, IGameSettingsViewController, IInitializable
    {
        [SerializeField] private TMP_InputField _playerNameInput;
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Slider _uiVolumeSlider;
        [SerializeField] private Toggle _muteToggle;
        [SerializeField] private Button _deleteSavesButton;

        private bool _bound;

        public event Action<string> OnPlayerNameChanged;
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;
        public event Action<float> OnUiVolumeChanged;
        public event Action<bool> OnMutedChanged;
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

        public void SetInteractable(bool interactable)
        {
            if (_playerNameInput != null) _playerNameInput.interactable = interactable;
            if (_masterVolumeSlider != null) _masterVolumeSlider.interactable = interactable;
            if (_musicVolumeSlider != null) _musicVolumeSlider.interactable = interactable;
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.interactable = interactable;
            if (_uiVolumeSlider != null) _uiVolumeSlider.interactable = interactable;
            if (_muteToggle != null) _muteToggle.interactable = interactable;
            if (_deleteSavesButton != null) _deleteSavesButton.interactable = interactable;
        }

        private void Bind()
        {
            if (_bound) return;
            EnsureRuntimeControls();
            if (_playerNameInput != null) _playerNameInput.onEndEdit.AddListener(HandlePlayerNameChanged);
            if (_masterVolumeSlider != null) _masterVolumeSlider.onValueChanged.AddListener(HandleMaster);
            if (_musicVolumeSlider != null) _musicVolumeSlider.onValueChanged.AddListener(HandleMusic);
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.onValueChanged.AddListener(HandleSfx);
            if (_uiVolumeSlider != null) _uiVolumeSlider.onValueChanged.AddListener(HandleUi);
            if (_muteToggle != null) _muteToggle.onValueChanged.AddListener(HandleMute);
            if (_deleteSavesButton != null) _deleteSavesButton.onClick.AddListener(HandleDeleteSavesClicked);
            _bound = true;
        }

        private void HandlePlayerNameChanged(string value) => OnPlayerNameChanged?.Invoke(value);
        private void HandleMaster(float v) => OnMasterVolumeChanged?.Invoke(v);
        private void HandleMusic(float v) => OnMusicVolumeChanged?.Invoke(v);
        private void HandleSfx(float v) => OnSfxVolumeChanged?.Invoke(v);
        private void HandleUi(float v) => OnUiVolumeChanged?.Invoke(v);
        private void HandleMute(bool v) => OnMutedChanged?.Invoke(v);
        private void HandleDeleteSavesClicked() => OnDeleteSavesClicked?.Invoke();

        private void EnsureRuntimeControls()
        {
            if (_masterVolumeSlider == null)
                return;

            var parent = _masterVolumeSlider.transform.parent != null ? _masterVolumeSlider.transform.parent : transform;
            _musicVolumeSlider ??= CloneSlider(parent, _masterVolumeSlider, "Slider_MusicVolume", 0.7f);
            _sfxVolumeSlider ??= CloneSlider(parent, _masterVolumeSlider, "Slider_SfxVolume", 0.9f);
            _uiVolumeSlider ??= CloneSlider(parent, _masterVolumeSlider, "Slider_UiVolume", 0.9f);
            _muteToggle ??= CreateMuteToggle(parent);
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
            HomeMenuRuntimeUiFactory.CreateText(row.transform, "Label", "Mute", 16, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            return toggle;
        }
    }
}
