using System;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using UnityEngine.Audio;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Production-реалізація <see cref="IAudioSettingsService"/>.
    /// Значення зберігаються у <see cref="PlayerPrefs"/> і, за наявності,
    /// застосовуються до <see cref="AudioMixer"/> із прив'язками
    /// <see cref="AudioMixerBindingsSO"/>.
    ///
    /// Лінійний діапазон [0..1] конвертується у dB за формулою
    /// <c>20 * log10(clamp(v, eps, 1))</c>.
    /// </summary>
    internal sealed class AudioSettingsService : IAudioSettingsService
    {
        private const string PrefPrefix = "Moyva.Audio";
        private const string PrefMuted  = PrefPrefix + ".Muted";
        private const float  MinLinear  = 0.0001f;

        private readonly AudioMixerBindingsSO _bindings;

        private float _master, _music, _sfx, _ui;
        private bool  _muted;

        public event Action<AudioChannel, float> VolumeChanged;
        public event Action<bool> MuteChanged;

        public AudioSettingsService(AudioMixerBindingsSO bindings)
        {
            _bindings = bindings != null
                ? bindings
                : throw new ArgumentNullException(nameof(bindings));
        }

        /// <inheritdoc/>
        public bool IsMuted => _muted;

        /// <inheritdoc/>
        public float GetVolume(AudioChannel channel) => channel switch
        {
            AudioChannel.Master => _master,
            AudioChannel.Music  => _music,
            AudioChannel.Sfx    => _sfx,
            AudioChannel.Ui     => _ui,
            _                   => 0f
        };

        /// <inheritdoc/>
        public void SetVolume(AudioChannel channel, float value)
        {
            value = Mathf.Clamp01(value);
            switch (channel)
            {
                case AudioChannel.Master: _master = value; break;
                case AudioChannel.Music:  _music  = value; break;
                case AudioChannel.Sfx:    _sfx    = value; break;
                case AudioChannel.Ui:     _ui     = value; break;
                default:                  return;
            }

            ApplyToMixer(channel, value);
            PlayerPrefs.SetFloat(KeyFor(channel), value);
            VolumeChanged?.Invoke(channel, value);
        }

        /// <inheritdoc/>
        public void SetMuted(bool muted)
        {
            if (_muted == muted) return;
            _muted = muted;
            ApplyToMixer(AudioChannel.Master, muted ? 0f : _master);
            PlayerPrefs.SetInt(PrefMuted, muted ? 1 : 0);
            MuteChanged?.Invoke(muted);
        }

        /// <inheritdoc/>
        public void Save()
        {
            PlayerPrefs.SetFloat(KeyFor(AudioChannel.Master), _master);
            PlayerPrefs.SetFloat(KeyFor(AudioChannel.Music),  _music);
            PlayerPrefs.SetFloat(KeyFor(AudioChannel.Sfx),    _sfx);
            PlayerPrefs.SetFloat(KeyFor(AudioChannel.Ui),     _ui);
            PlayerPrefs.SetInt(PrefMuted, _muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <inheritdoc/>
        public void Load()
        {
            _master = PlayerPrefs.GetFloat(KeyFor(AudioChannel.Master), _bindings.DefaultMaster);
            _music  = PlayerPrefs.GetFloat(KeyFor(AudioChannel.Music),  _bindings.DefaultMusic);
            _sfx    = PlayerPrefs.GetFloat(KeyFor(AudioChannel.Sfx),    _bindings.DefaultSfx);
            _ui     = PlayerPrefs.GetFloat(KeyFor(AudioChannel.Ui),     _bindings.DefaultUi);
            _muted  = PlayerPrefs.GetInt(PrefMuted, 0) == 1;

            ApplyToMixer(AudioChannel.Master, _muted ? 0f : _master);
            ApplyToMixer(AudioChannel.Music,  _music);
            ApplyToMixer(AudioChannel.Sfx,    _sfx);
            ApplyToMixer(AudioChannel.Ui,     _ui);

            VolumeChanged?.Invoke(AudioChannel.Master, _master);
            VolumeChanged?.Invoke(AudioChannel.Music,  _music);
            VolumeChanged?.Invoke(AudioChannel.Sfx,    _sfx);
            VolumeChanged?.Invoke(AudioChannel.Ui,     _ui);
            MuteChanged?.Invoke(_muted);
        }

        /// <inheritdoc/>
        public void ResetToDefaults()
        {
            SetVolume(AudioChannel.Master, _bindings.DefaultMaster);
            SetVolume(AudioChannel.Music,  _bindings.DefaultMusic);
            SetVolume(AudioChannel.Sfx,    _bindings.DefaultSfx);
            SetVolume(AudioChannel.Ui,     _bindings.DefaultUi);
            SetMuted(false);
            Save();
        }

        // ─────────────────────────────────────────────────────────────────

        private void ApplyToMixer(AudioChannel channel, float linear)
        {
            var mixer = _bindings.Mixer;
            if (mixer == null) return;

            string param = ParameterFor(channel);
            if (string.IsNullOrEmpty(param)) return;

            float db = LinearToDb(linear);
            if (!mixer.SetFloat(param, db))
                Debug.LogWarning($"[AudioSettingsService] AudioMixer не має exposed параметра '{param}'.");
        }

        private string ParameterFor(AudioChannel channel) => channel switch
        {
            AudioChannel.Master => _bindings.MasterParameter,
            AudioChannel.Music  => _bindings.MusicParameter,
            AudioChannel.Sfx    => _bindings.SfxParameter,
            AudioChannel.Ui     => _bindings.UiParameter,
            _                   => null
        };

        private static string KeyFor(AudioChannel channel) => $"{PrefPrefix}.{channel}";

        /// <summary>Лінійне [0..1] → dB. 0 → -80dB, 1 → 0dB.</summary>
        internal static float LinearToDb(float linear)
        {
            return Mathf.Log10(Mathf.Max(linear, MinLinear)) * 20f;
        }
    }
}
