using System;
using System.Collections.Generic;
using Kruty1918.Audio;
using UnityEngine;
using UnityEngine.Audio;

using Kruty1918.JsonConfig;
namespace Kruty1918.Moyva.Audio.Runtime
{
    /// <summary>
    /// Runtime registry for audio sounds. Loaded via Resources.Load("MoyvaAudioRegistry") by ProjectServicesInstaller.
    /// </summary>
[System.Serializable]
public sealed class AudioRegistrySO : JsonConfigObject, IAudioCatalog
    {
        [SerializeField] private AudioSoundDefinition[] _sounds = Array.Empty<AudioSoundDefinition>();
        [SerializeField] private AudioChannelDefinition[] _channels = Array.Empty<AudioChannelDefinition>();
        [SerializeField] private AudioBusGroupBinding[] _busGroups = Array.Empty<AudioBusGroupBinding>();
        [SerializeField, Min(1)] private int _defaultPoolSize = 12;
        [SerializeField] private bool _dontDestroyOnLoad = true;

        private Dictionary<string, AudioSoundDefinition> _byKey;
        private Dictionary<string, AudioChannelDefinition> _channelsByKey;
        private Dictionary<AudioBus, AudioMixerGroup> _groupByBus;

        public AudioSoundDefinition[] Sounds
        {
            get => _sounds;
            set
            {
                _sounds = value ?? Array.Empty<AudioSoundDefinition>();
                _byKey = null;
            }
        }

        public AudioChannelDefinition[] Channels
        {
            get => _channels;
            set
            {
                _channels = value ?? Array.Empty<AudioChannelDefinition>();
                _channelsByKey = null;
            }
        }

        public int DefaultPoolSize => Mathf.Max(1, _defaultPoolSize);
        public bool PersistAcrossScenes => _dontDestroyOnLoad;

        public bool TryGetChannel(string key, out AudioChannelDefinition channel)
        {
            channel = null;
            if (string.IsNullOrWhiteSpace(key))
                return false;

            EnsureChannelCache();
            return _channelsByKey.TryGetValue(key.Trim(), out channel) && channel != null;
        }

        /// <summary>Mixer-група за замовчуванням для bus (null — якщо не задано).</summary>
        public AudioMixerGroup GetBusGroup(AudioBus bus)
        {
            EnsureBusGroupCache();
            return _groupByBus.TryGetValue(bus, out var group) ? group : null;
        }

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

        private void EnsureChannelCache()
        {
            if (_channelsByKey != null)
                return;

            _channelsByKey = new Dictionary<string, AudioChannelDefinition>(StringComparer.OrdinalIgnoreCase);
            if (_channels == null)
                return;

            for (int i = 0; i < _channels.Length; i++)
            {
                var channel = _channels[i];
                if (channel == null || string.IsNullOrWhiteSpace(channel.Key))
                    continue;

                string key = channel.Key.Trim();
                if (!_channelsByKey.ContainsKey(key))
                    _channelsByKey.Add(key, channel);
            }
        }

        private void EnsureBusGroupCache()
        {
            if (_groupByBus != null)
                return;

            _groupByBus = new Dictionary<AudioBus, AudioMixerGroup>();
            if (_busGroups == null)
                return;

            for (int i = 0; i < _busGroups.Length; i++)
            {
                var binding = _busGroups[i];
                if (binding == null || binding.MixerGroup == null)
                    continue;

                _groupByBus[binding.Bus] = binding.MixerGroup;
            }
        }
    }
}
