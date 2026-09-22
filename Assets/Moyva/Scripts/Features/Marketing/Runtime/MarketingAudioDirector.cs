using System.Collections;
using System.Collections.Generic;
using Kruty1918.Audio;
using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Trailer audio: music bed + ambience + sparse accent SFX + bus-level
    /// ducking. Uses only the canonical IAudioService; no parallel audio stack.
    /// </summary>
    public sealed class MarketingAudioDirector
    {
        private readonly IAudioService _audio;
        private readonly MonoBehaviour _host;
        private readonly List<string> _usedKeys = new List<string>();
        private float _musicBaseVolume = 1f;
        private Coroutine _duckRoutine;

        public MarketingAudioDirector(IAudioService audio, MonoBehaviour host)
        {
            _audio = audio;
            _host = host;
            if (_audio != null)
                _musicBaseVolume = _audio.GetBusVolume(AudioBus.Music);
        }

        public IReadOnlyList<string> UsedKeys => _usedKeys;

        public void PlayMusic(string key)
        {
            if (_audio == null || string.IsNullOrEmpty(key)) return;
            if (!_audio.TryGetSound(key, out var def) || def == null) return;
            _audio.StopAll(AudioBus.Music);
            var h = _audio.Play(key, new AudioPlayOptions(loopOverride: true));
            if (h.IsValid) _usedKeys.Add(key);
        }

        public void PlayAmbience(string key)
        {
            if (_audio == null || string.IsNullOrEmpty(key)) return;
            if (!_audio.TryGetSound(key, out var def) || def == null) return;
            var h = _audio.Play(key, new AudioPlayOptions(loopOverride: true, volumeScale: 0.6f));
            if (h.IsValid) _usedKeys.Add(key);
        }

        /// <summary>Sparse accent SFX — called on meaningful events only
        /// (impact, reveal, end card). Never per-cut whoosh spam.</summary>
        public void Accent(string key, float volumeScale = 1f)
        {
            if (_audio == null || string.IsNullOrEmpty(key)) return;
            var h = _audio.Play(key, new AudioPlayOptions(volumeScale: volumeScale));
            if (h.IsValid) _usedKeys.Add(key);
        }

        /// <summary>Short music duck for impacts / end-card resolution.</summary>
        public void DuckMusic(float seconds = 0.8f, float to = 0.35f)
        {
            if (_audio == null || _host == null) return;
            if (_duckRoutine != null) _host.StopCoroutine(_duckRoutine);
            _duckRoutine = _host.StartCoroutine(DuckRoutine(seconds, to));
        }

        private IEnumerator DuckRoutine(float seconds, float to)
        {
            _audio.SetBusVolume(AudioBus.Music, _musicBaseVolume * to);
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            // Smooth return
            float t = 0f;
            float from = _musicBaseVolume * to;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                _audio.SetBusVolume(AudioBus.Music, Mathf.Lerp(from, _musicBaseVolume, t / 0.5f));
                yield return null;
            }
            _audio.SetBusVolume(AudioBus.Music, _musicBaseVolume);
            _duckRoutine = null;
        }

        public void StopAll()
        {
            if (_audio == null) return;
            _audio.StopAll(AudioBus.Music);
            _audio.StopAll(AudioBus.Ambience);
            _audio.SetBusVolume(AudioBus.Music, _musicBaseVolume);
        }

        /// <summary>Pick a default music key from the index (Music-bus loop).</summary>
        public static string AutoMusicKey(ContentIndexSnapshot index)
            => index != null && index.musicKeys != null && index.musicKeys.Count > 0
                ? index.musicKeys[0] : string.Empty;

        public static string AutoAmbienceKey(ContentIndexSnapshot index)
            => index != null && index.ambienceKeys != null && index.ambienceKeys.Count > 0
                ? index.ambienceKeys[0] : string.Empty;
    }
}
