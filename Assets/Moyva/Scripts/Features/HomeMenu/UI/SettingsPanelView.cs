using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Панель налаштувань: 4 слайдери гучності, перемикач mute,
    /// кнопка видалення даних користувача, грід соціальних посилань, кнопка "назад".
    /// </summary>
    public sealed class SettingsPanelView : MonoBehaviour
    {
        [Header("Аудіо")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider uiSlider;
        [SerializeField] private Toggle muteToggle;
        [SerializeField] private TMP_Text masterValueLabel;
        [SerializeField] private TMP_Text musicValueLabel;
        [SerializeField] private TMP_Text sfxValueLabel;
        [SerializeField] private TMP_Text uiValueLabel;

        [Header("Дані користувача")]
        [SerializeField] private Button deleteDataButton;
        [SerializeField] private TMP_Text deleteDataStatus;

        [Header("Соціальні посилання")]
        [SerializeField] private RectTransform socialLinksRoot;
        [SerializeField] private SocialLinkButtonView socialLinkButtonPrefab;

        [Header("Навігація")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button resetDefaultsButton;

        private IAudioSettingsService _audio;
        private IUserDataService _userData;
        private ISocialLinksService _social;
        private IHomeMenuFlow _flow;
        private SignalBus _signalBus;

        private bool _suppressEvents;

        [Inject]
        internal void Construct(
            IAudioSettingsService audio,
            IUserDataService userData,
            ISocialLinksService social,
            IHomeMenuFlow flow,
            SignalBus signalBus)
        {
            _audio     = audio     ?? throw new ArgumentNullException(nameof(audio));
            _userData  = userData  ?? throw new ArgumentNullException(nameof(userData));
            _social    = social    ?? throw new ArgumentNullException(nameof(social));
            _flow      = flow      ?? throw new ArgumentNullException(nameof(flow));
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
        }

        private void Start()
        {
            BuildSocialLinks();
            RefreshFromService();

            masterSlider?.onValueChanged.AddListener(v => OnVolumeChanged(AudioChannel.Master, v));
            musicSlider?.onValueChanged.AddListener(v => OnVolumeChanged(AudioChannel.Music, v));
            sfxSlider?.onValueChanged.AddListener(v => OnVolumeChanged(AudioChannel.Sfx, v));
            uiSlider?.onValueChanged.AddListener(v => OnVolumeChanged(AudioChannel.Ui, v));
            muteToggle?.onValueChanged.AddListener(OnMuteChanged);
            deleteDataButton?.onClick.AddListener(OnDeleteDataClicked);
            backButton?.onClick.AddListener(OnBackClicked);
            resetDefaultsButton?.onClick.AddListener(OnResetDefaultsClicked);

            _audio.VolumeChanged += HandleVolumeExternal;
            _audio.MuteChanged   += HandleMuteExternal;
        }

        private void OnDestroy()
        {
            if (_audio != null)
            {
                _audio.VolumeChanged -= HandleVolumeExternal;
                _audio.MuteChanged   -= HandleMuteExternal;
            }
        }

        // ── UI → Service ─────────────────────────────────────────────────

        private void OnVolumeChanged(AudioChannel channel, float value)
        {
            if (_suppressEvents) return;
            _audio.SetVolume(channel, value);
            UpdateVolumeLabel(channel, value);
        }

        private void OnMuteChanged(bool muted)
        {
            if (_suppressEvents) return;
            _audio.SetMuted(muted);
        }

        private void OnDeleteDataClicked()
        {
            _flow.ShowConfirm(
                title:   "Видалити всі дані?",
                message: "Всі збереження та налаштування буде безповоротно видалено.",
                onConfirm: () =>
                {
                    int removed = _userData.DeleteAllUserData();
                    _signalBus.TryFire(new UserDataClearedSignal { DeletedCount = removed });
                    if (deleteDataStatus != null)
                        deleteDataStatus.text = removed > 0
                            ? $"Видалено записів: {removed}"
                            : "Даних для видалення не знайдено";
                    RefreshFromService();
                });
        }

        private void OnBackClicked()            => _flow.ShowMain();
        private void OnResetDefaultsClicked()   => _audio.ResetToDefaults();

        // ── Service → UI ─────────────────────────────────────────────────

        private void HandleVolumeExternal(AudioChannel channel, float value)
        {
            _suppressEvents = true;
            try
            {
                switch (channel)
                {
                    case AudioChannel.Master: if (masterSlider != null) masterSlider.value = value; break;
                    case AudioChannel.Music:  if (musicSlider  != null) musicSlider.value  = value; break;
                    case AudioChannel.Sfx:    if (sfxSlider    != null) sfxSlider.value    = value; break;
                    case AudioChannel.Ui:     if (uiSlider     != null) uiSlider.value     = value; break;
                }
                UpdateVolumeLabel(channel, value);
            }
            finally { _suppressEvents = false; }
        }

        private void HandleMuteExternal(bool muted)
        {
            _suppressEvents = true;
            try { if (muteToggle != null) muteToggle.isOn = muted; }
            finally { _suppressEvents = false; }
        }

        private void RefreshFromService()
        {
            _suppressEvents = true;
            try
            {
                if (masterSlider != null) masterSlider.value = _audio.GetVolume(AudioChannel.Master);
                if (musicSlider  != null) musicSlider.value  = _audio.GetVolume(AudioChannel.Music);
                if (sfxSlider    != null) sfxSlider.value    = _audio.GetVolume(AudioChannel.Sfx);
                if (uiSlider     != null) uiSlider.value     = _audio.GetVolume(AudioChannel.Ui);
                if (muteToggle   != null) muteToggle.isOn    = _audio.IsMuted;

                UpdateVolumeLabel(AudioChannel.Master, _audio.GetVolume(AudioChannel.Master));
                UpdateVolumeLabel(AudioChannel.Music,  _audio.GetVolume(AudioChannel.Music));
                UpdateVolumeLabel(AudioChannel.Sfx,    _audio.GetVolume(AudioChannel.Sfx));
                UpdateVolumeLabel(AudioChannel.Ui,     _audio.GetVolume(AudioChannel.Ui));
            }
            finally { _suppressEvents = false; }
        }

        private void UpdateVolumeLabel(AudioChannel channel, float value)
        {
            string text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
            switch (channel)
            {
                case AudioChannel.Master: if (masterValueLabel != null) masterValueLabel.text = text; break;
                case AudioChannel.Music:  if (musicValueLabel  != null) musicValueLabel.text  = text; break;
                case AudioChannel.Sfx:    if (sfxValueLabel    != null) sfxValueLabel.text    = text; break;
                case AudioChannel.Ui:     if (uiValueLabel     != null) uiValueLabel.text     = text; break;
            }
        }

        private void BuildSocialLinks()
        {
            if (socialLinksRoot == null || socialLinkButtonPrefab == null) return;
            // Очищаємо попередньо створені (наприклад при повторному входженні у сцену).
            for (int i = socialLinksRoot.childCount - 1; i >= 0; i--)
                Destroy(socialLinksRoot.GetChild(i).gameObject);

            foreach (var entry in _social.Links)
            {
                var view = Instantiate(socialLinkButtonPrefab, socialLinksRoot);
                view.Bind(entry, _social);
            }
        }
    }
}
