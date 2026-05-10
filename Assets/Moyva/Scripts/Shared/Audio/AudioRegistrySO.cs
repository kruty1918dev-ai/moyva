using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Audio.API;
using UnityEngine;

namespace Kruty1918.Moyva.Audio.Runtime
{
    /// <summary>
    /// Runtime registry for audio sounds. Loaded via Resources.Load("MoyvaAudioRegistry") by ProjectServicesInstaller.
    /// </summary>
    [CreateAssetMenu(fileName = "MoyvaAudioRegistry", menuName = "Moyva/Audio/Audio Registry")]
    public sealed class AudioRegistrySO : ScriptableObject
    {
        [SerializeField] private AudioSoundDefinition[] _sounds = Array.Empty<AudioSoundDefinition>();
        [SerializeField, Min(1)] private int _defaultPoolSize = 12;
        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] private bool _verboseLogs;

        private Dictionary<string, AudioSoundDefinition> _byKey;

        public AudioSoundDefinition[] Sounds
        {
            get => _sounds;
            set
            {
                _sounds = value ?? Array.Empty<AudioSoundDefinition>();
                _byKey = null;
            }
        }

        public int DefaultPoolSize => Mathf.Max(1, _defaultPoolSize);
        public bool PersistAcrossScenes => _dontDestroyOnLoad;
        public bool VerboseLogs => _verboseLogs;

        public bool TryGet(string key, out AudioSoundDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(key))
                return false;

            EnsureCache();
            return _byKey.TryGetValue(key.Trim(), out definition) && definition != null;
        }

        public string[] GetKeys()
        {
            EnsureCache();
            var keys = new string[_byKey.Count];
            _byKey.Keys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);
            return keys;
        }

        public void RebuildCache()
        {
            _byKey = null;
            EnsureCache();
        }

        private void OnValidate()
        {
            _byKey = null;
        }

        private void EnsureCache()
        {
            if (_byKey != null)
                return;

            _byKey = new Dictionary<string, AudioSoundDefinition>(StringComparer.OrdinalIgnoreCase);
            if (_sounds == null)
                return;

            for (int i = 0; i < _sounds.Length; i++)
            {
                var sound = _sounds[i];
                if (sound == null || string.IsNullOrWhiteSpace(sound.Key))
                    continue;

                string key = sound.Key.Trim();
                if (!_byKey.ContainsKey(key))
                    _byKey.Add(key, sound);
            }
        }
    }
}
