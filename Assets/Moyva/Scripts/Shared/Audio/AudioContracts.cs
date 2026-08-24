using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Kruty1918.Moyva.Audio.API
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AudioKeyAttribute : PropertyAttribute { }

    public enum AudioBus
    {
        Master = 0,
        Music = 1,
        Sfx = 2,
        Ui = 3,
        Ambience = 4,
    }

    [Serializable]
    public sealed class AudioEffectSettings
    {
        public bool EnableLowPass;
        [Range(10f, 22000f)] public float LowPassCutoff = 5000f;
        [Range(1f, 10f)] public float LowPassResonance = 1f;

        public bool EnableHighPass;
        [Range(10f, 22000f)] public float HighPassCutoff = 120f;
        [Range(1f, 10f)] public float HighPassResonance = 1f;

        public bool EnableEcho;
        [Range(10f, 5000f)] public float EchoDelay = 220f;
        [Range(0f, 1f)] public float EchoDecayRatio = 0.35f;
        [Range(0f, 1f)] public float EchoWetMix = 0.28f;
        [Range(0f, 1f)] public float EchoDryMix = 1f;

        public bool EnableReverb;
        public AudioReverbPreset ReverbPreset = AudioReverbPreset.Room;

        public bool EnableDistortion;
        [Range(0f, 1f)] public float DistortionLevel = 0.18f;

        public bool EnableChorus;
        [Range(0f, 1f)] public float ChorusDryMix = 0.5f;
        [Range(0f, 1f)] public float ChorusWetMix1 = 0.5f;
        [Range(0f, 1f)] public float ChorusWetMix2 = 0.5f;
        [Range(0f, 1f)] public float ChorusWetMix3 = 0.5f;
        [Range(0f, 20f)] public float ChorusDelay = 40f;
        [Range(0f, 20f)] public float ChorusRate = 0.8f;
        [Range(0f, 1f)] public float ChorusDepth = 0.03f;
    }

    [Serializable]
    public sealed class AudioSoundDefinition
    {
        [Tooltip("Унікальний ключ звуку. Використовується у коді: audio.Play(\"ui-click\").")]
        public string Key;

        [Tooltip("Основний AudioClip. Якщо задано Variants, система випадково обере один із них.")]
        public AudioClip Clip;

        [Tooltip("Опційні варіанти цього звуку для менш повторюваного SFX.")]
        public AudioClip[] Variants = Array.Empty<AudioClip>();

        public AudioBus Bus = AudioBus.Sfx;
        public AudioMixerGroup MixerGroup;

        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0f, 1f)] public float VolumeRandom = 0f;
        [Range(-3f, 3f)] public float Pitch = 1f;
        [Range(0f, 1f)] public float PitchRandom = 0f;
        [Range(0f, 1f)] public float SpatialBlend = 0f;
        [Range(0f, 5f)] public float DopplerLevel = 0f;
        [Range(0f, 1.1f)] public float ReverbZoneMix = 1f;
        [Range(0, 256)] public int Priority = 128;
        public bool Loop;
        [Min(0)] public int PoolWarmup = 1;
        [Min(1)] public int MaxSimultaneous = 8;
        public AudioEffectSettings Effects = new AudioEffectSettings();
    }

    public readonly struct AudioPlayOptions
    {
        public static AudioPlayOptions Default => new AudioPlayOptions(volumeScale: 1f);

        public readonly Vector3? Position;
        public readonly Transform Parent;
        public readonly float VolumeScale;
        public readonly float PitchOffset;
        public readonly bool? LoopOverride;

        public AudioPlayOptions(Vector3? position = null, Transform parent = null, float volumeScale = 1f, float pitchOffset = 0f, bool? loopOverride = null)
        {
            Position = position;
            Parent = parent;
            VolumeScale = Mathf.Max(0f, volumeScale);
            PitchOffset = pitchOffset;
            LoopOverride = loopOverride;
        }
    }

    public readonly struct AudioHandle
    {
        private readonly AudioSource _source;

        public AudioHandle(AudioSource source)
        {
            _source = source;
        }

        public bool IsValid => _source != null;
        public bool IsPlaying => _source != null && _source.isPlaying;
        public AudioSource Source => _source;

        public void Stop()
        {
            if (_source != null)
                _source.Stop();
        }
    }

    public interface IAudioService
    {
        bool TryGetSound(string key, out AudioSoundDefinition sound);
        AudioSoundDefinition GetSound(string key);
        AudioHandle Play(string key);
        AudioHandle Play(string key, AudioPlayOptions options);
        AudioHandle PlayAt(string key, Vector3 position, float volumeScale = 1f);
        AudioSource GetConfiguredSource(string key, Transform parent = null);
        void SetBusVolume(AudioBus bus, float volume);
        float GetBusVolume(AudioBus bus);
        void StopByKey(string key);
        void StopAll(AudioBus? bus = null);
        string[] GetKeys();
    }
}

