using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Kruty1918.Moyva.Audio.Runtime
{
    /// <summary>
    /// Per-scene override для окремого звуку.
    /// SceneName = "" означає глобальний override (всі сцени).
    /// </summary>
    [Serializable]
    public sealed class SoundSceneOverride
    {
        [HideInInspector] public string SceneName;
        [HideInInspector] public string SoundKey;

        public bool OverrideVolume;
        [Range(0f, 1f)] public float Volume = 1f;

        public bool OverridePitch;
        [Range(-3f, 3f)] public float Pitch = 1f;

        public bool OverrideMixerGroup;
        public AudioMixerGroup MixerGroup;

        public bool OverrideLoop;
        public bool Loop;

        public bool OverrideSpatialBlend;
        [Range(0f, 1f)] public float SpatialBlend;

        public bool OverridePriority;
        [Range(0, 256)] public int Priority = 128;

        /// <summary>
        /// Автоматично відтворити цей звук при завантаженні сцени.
        /// Не вимагає окремого OverrideXxx — просто встановіть true.
        /// </summary>
        public bool PlayOnAwake;
    }

    /// <summary>
    /// Реєстр per-scene overrides для звуків.
    /// Runtime завантажує з Resources/MoyvaSceneAudioOverrides.
    /// </summary>
    [CreateAssetMenu(fileName = "MoyvaSceneAudioOverrides", menuName = "Moyva/Audio/Scene Audio Overrides")]
    public sealed class SceneAudioOverridesSO : ScriptableObject
    {
        [SerializeField] private List<SoundSceneOverride> _overrides = new List<SoundSceneOverride>();

        public IReadOnlyList<SoundSceneOverride> Overrides => _overrides;

        /// <summary>
        /// Знайти override для (scene, sound). Scene-specific перемагає global (SceneName="").
        /// </summary>
        public bool TryGet(string sceneName, string soundKey, out SoundSceneOverride result)
        {
            SoundSceneOverride globalFallback = null;
            for (int i = 0; i < _overrides.Count; i++)
            {
                var o = _overrides[i];
                if (o == null) continue;
                if (!string.Equals(o.SoundKey, soundKey, StringComparison.OrdinalIgnoreCase)) continue;

                if (string.Equals(o.SceneName, sceneName, StringComparison.OrdinalIgnoreCase))
                {
                    result = o;
                    return true;
                }
                if (string.IsNullOrEmpty(o.SceneName))
                    globalFallback = o;
            }
            result = globalFallback;
            return globalFallback != null;
        }

        public bool HasOverride(string sceneName, string soundKey)
        {
            for (int i = 0; i < _overrides.Count; i++)
            {
                var o = _overrides[i];
                if (o != null
                    && string.Equals(o.SoundKey, soundKey, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(o.SceneName, sceneName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public SoundSceneOverride GetOrCreate(string sceneName, string soundKey)
        {
            for (int i = 0; i < _overrides.Count; i++)
            {
                var o = _overrides[i];
                if (o != null
                    && string.Equals(o.SoundKey, soundKey, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(o.SceneName, sceneName, StringComparison.OrdinalIgnoreCase))
                    return o;
            }
            var n = new SoundSceneOverride { SceneName = sceneName, SoundKey = soundKey };
            _overrides.Add(n);
            return n;
        }

        public void RemoveOverride(string sceneName, string soundKey)
        {
            _overrides.RemoveAll(o => o != null
                && string.Equals(o.SoundKey, soundKey, StringComparison.OrdinalIgnoreCase)
                && string.Equals(o.SceneName, sceneName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
