using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using Zenject;
using Kruty1918.Moyva.Shared.Connectivity;
using Kruty1918.Moyva.Shared.Graphics;

namespace Kruty1918.Moyva.Shared
{
    /// <summary>
    /// Installer for shared services (Connectivity, etc.).
    /// </summary>
    public sealed class SharedInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IConnectivityService>()
                .To<ConnectivityService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GraphicsSettingsService>()
                .AsSingle()
                .NonLazy();
        }

        // Helper for programmatic installation from other installers
        public static void Install(DiContainer container)
        {
            container.Bind<IConnectivityService>()
                .To<ConnectivityService>()
                .AsSingle();

            container.BindInterfacesAndSelfTo<GraphicsSettingsService>()
                .AsSingle()
                .NonLazy();
        }
    }
}

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
        void StopByKey(string key);
        void StopAll(AudioBus? bus = null);
        string[] GetKeys();
    }
}

namespace Kruty1918.Moyva.Audio.Runtime
{
    using Kruty1918.Moyva.Audio.API;

    public sealed class AudioService : IAudioService, IInitializable, ITickable, IDisposable
    {
        private const string DefaultRegistryResourcePath = "MoyvaAudioRegistry";
        private const string RootName = "MoyvaAudioPool";

        private readonly AudioRegistrySO _registry;
        private readonly Queue<AudioSource> _available = new Queue<AudioSource>();
        private readonly List<AudioSource> _active = new List<AudioSource>();
        private readonly Dictionary<AudioSource, string> _activeKeys = new Dictionary<AudioSource, string>();
        private readonly Dictionary<string, int> _activeCountByKey = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private GameObject _root;

        public AudioService([InjectOptional] AudioRegistrySO registry)
        {
            _registry = registry != null ? registry : Resources.Load<AudioRegistrySO>(DefaultRegistryResourcePath);
        }

        public void Initialize()
        {
            _root = new GameObject(RootName);
            if (_registry == null || _registry.PersistAcrossScenes)
                UnityEngine.Object.DontDestroyOnLoad(_root);

            int poolSize = _registry != null ? _registry.DefaultPoolSize : 12;
            if (_registry != null && _registry.Sounds != null)
            {
                for (int i = 0; i < _registry.Sounds.Length; i++)
                {
                    var sound = _registry.Sounds[i];
                    if (sound != null)
                        poolSize += Mathf.Max(0, sound.PoolWarmup);
                }
            }

            for (int i = 0; i < poolSize; i++)
                _available.Enqueue(CreateSource());

            if (_registry != null && _registry.VerboseLogs)
                Debug.Log($"[AudioService] Initialized with {_available.Count} pooled AudioSource objects.");
        }

        public void Tick()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var source = _active[i];
                if (source == null)
                {
                    _active.RemoveAt(i);
                    continue;
                }

                if (source.loop || source.isPlaying)
                    continue;

                Release(source);
            }
        }

        public void Dispose()
        {
            if (_root != null)
                UnityEngine.Object.Destroy(_root);

            _available.Clear();
            _active.Clear();
            _activeKeys.Clear();
            _activeCountByKey.Clear();
        }

        public bool TryGetSound(string key, out AudioSoundDefinition sound)
        {
            sound = null;
            return _registry != null && _registry.TryGet(key, out sound);
        }

        public AudioSoundDefinition GetSound(string key)
        {
            TryGetSound(key, out var sound);
            return sound;
        }

        public AudioHandle Play(string key)
            => Play(key, new AudioPlayOptions());

        public AudioHandle PlayAt(string key, Vector3 position, float volumeScale = 1f)
            => Play(key, new AudioPlayOptions(position, null, volumeScale));

        public AudioHandle Play(string key, AudioPlayOptions options)
        {
            if (!TryGetSound(key, out var sound))
            {
                Debug.LogWarning($"[AudioService] Sound key '{key}' not found.");
                return default;
            }

            var clip = ResolveClip(sound);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioService] Sound key '{key}' has no AudioClip.");
                return default;
            }

            if (!CanPlay(sound))
                return default;

            var source = GetConfiguredSourceInternal(sound, options.Parent);
            source.clip = clip;
            source.volume = Mathf.Clamp01(ResolveVolume(sound) * options.VolumeScale);
            source.pitch = Mathf.Clamp(ResolvePitch(sound) + options.PitchOffset, -3f, 3f);
            source.loop = options.LoopOverride ?? sound.Loop;

            if (options.Position.HasValue)
            {
                source.transform.position = options.Position.Value;
                source.spatialBlend = Mathf.Max(source.spatialBlend, 1f);
            }
            else if (options.Parent != null)
            {
                source.transform.localPosition = Vector3.zero;
            }
            else
            {
                source.transform.position = Vector3.zero;
            }

            RegisterActive(source, sound.Key);
            source.Play();
            return new AudioHandle(source);
        }

        public AudioSource GetConfiguredSource(string key, Transform parent = null)
        {
            if (!TryGetSound(key, out var sound))
                return null;

            return GetConfiguredSourceInternal(sound, parent);
        }

        public void StopByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var source = _active[i];
                if (source == null)
                    continue;

                if (_activeKeys.TryGetValue(source, out var sourceKey)
                    && string.Equals(sourceKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    source.Stop();
                    Release(source);
                }
            }
        }

        public void StopAll(AudioBus? bus = null)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var source = _active[i];
                if (source == null)
                    continue;

                if (bus.HasValue && _activeKeys.TryGetValue(source, out var key) && TryGetSound(key, out var sound) && sound.Bus != bus.Value)
                    continue;

                source.Stop();
                Release(source);
            }
        }

        public string[] GetKeys()
            => _registry != null ? _registry.GetKeys() : Array.Empty<string>();

        private AudioSource GetConfiguredSourceInternal(AudioSoundDefinition sound, Transform parent)
        {
            var source = _available.Count > 0 ? _available.Dequeue() : CreateSource();
            source.gameObject.SetActive(true);
            source.transform.SetParent(parent != null ? parent : _root.transform, false);
            ConfigureSource(source, sound);
            return source;
        }

        private AudioSource CreateSource()
        {
            var go = new GameObject("AudioSource");
            go.transform.SetParent(_root != null ? _root.transform : null, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.gameObject.SetActive(false);
            return source;
        }

        private void ConfigureSource(AudioSource source, AudioSoundDefinition sound)
        {
            source.outputAudioMixerGroup = sound.MixerGroup;
            source.priority = Mathf.Clamp(sound.Priority, 0, 256);
            source.spatialBlend = Mathf.Clamp01(sound.SpatialBlend);
            source.dopplerLevel = Mathf.Max(0f, sound.DopplerLevel);
            source.reverbZoneMix = Mathf.Clamp(sound.ReverbZoneMix, 0f, 1.1f);
            ApplyEffects(source.gameObject, sound.Effects);
        }

        private static void ApplyEffects(GameObject go, AudioEffectSettings effects)
        {
            if (effects == null)
                effects = new AudioEffectSettings();

            ConfigureLowPass(go, effects);
            ConfigureHighPass(go, effects);
            ConfigureEcho(go, effects);
            ConfigureReverb(go, effects);
            ConfigureDistortion(go, effects);
            ConfigureChorus(go, effects);
        }

        private static void ConfigureLowPass(GameObject go, AudioEffectSettings effects)
        {
            var filter = go.GetComponent<AudioLowPassFilter>();
            if (!effects.EnableLowPass)
            {
                if (filter != null) filter.enabled = false;
                return;
            }

            filter ??= go.AddComponent<AudioLowPassFilter>();
            filter.enabled = true;
            filter.cutoffFrequency = effects.LowPassCutoff;
            filter.lowpassResonanceQ = effects.LowPassResonance;
        }

        private static void ConfigureHighPass(GameObject go, AudioEffectSettings effects)
        {
            var filter = go.GetComponent<AudioHighPassFilter>();
            if (!effects.EnableHighPass)
            {
                if (filter != null) filter.enabled = false;
                return;
            }

            filter ??= go.AddComponent<AudioHighPassFilter>();
            filter.enabled = true;
            filter.cutoffFrequency = effects.HighPassCutoff;
            filter.highpassResonanceQ = effects.HighPassResonance;
        }

        private static void ConfigureEcho(GameObject go, AudioEffectSettings effects)
        {
            var filter = go.GetComponent<AudioEchoFilter>();
            if (!effects.EnableEcho)
            {
                if (filter != null) filter.enabled = false;
                return;
            }

            filter ??= go.AddComponent<AudioEchoFilter>();
            filter.enabled = true;
            filter.delay = effects.EchoDelay;
            filter.decayRatio = effects.EchoDecayRatio;
            filter.wetMix = effects.EchoWetMix;
            filter.dryMix = effects.EchoDryMix;
        }

        private static void ConfigureReverb(GameObject go, AudioEffectSettings effects)
        {
            var filter = go.GetComponent<AudioReverbFilter>();
            if (!effects.EnableReverb)
            {
                if (filter != null) filter.enabled = false;
                return;
            }

            filter ??= go.AddComponent<AudioReverbFilter>();
            filter.enabled = true;
            filter.reverbPreset = effects.ReverbPreset;
        }

        private static void ConfigureDistortion(GameObject go, AudioEffectSettings effects)
        {
            var filter = go.GetComponent<AudioDistortionFilter>();
            if (!effects.EnableDistortion)
            {
                if (filter != null) filter.enabled = false;
                return;
            }

            filter ??= go.AddComponent<AudioDistortionFilter>();
            filter.enabled = true;
            filter.distortionLevel = effects.DistortionLevel;
        }

        private static void ConfigureChorus(GameObject go, AudioEffectSettings effects)
        {
            var filter = go.GetComponent<AudioChorusFilter>();
            if (!effects.EnableChorus)
            {
                if (filter != null) filter.enabled = false;
                return;
            }

            filter ??= go.AddComponent<AudioChorusFilter>();
            filter.enabled = true;
            filter.dryMix = effects.ChorusDryMix;
            filter.wetMix1 = effects.ChorusWetMix1;
            filter.wetMix2 = effects.ChorusWetMix2;
            filter.wetMix3 = effects.ChorusWetMix3;
            filter.delay = effects.ChorusDelay;
            filter.rate = effects.ChorusRate;
            filter.depth = effects.ChorusDepth;
        }

        private bool CanPlay(AudioSoundDefinition sound)
        {
            int max = Mathf.Max(1, sound.MaxSimultaneous);
            return !_activeCountByKey.TryGetValue(sound.Key, out int count) || count < max;
        }

        private static AudioClip ResolveClip(AudioSoundDefinition sound)
        {
            if (sound.Variants != null && sound.Variants.Length > 0)
            {
                var valid = new List<AudioClip>();
                for (int i = 0; i < sound.Variants.Length; i++)
                    if (sound.Variants[i] != null)
                        valid.Add(sound.Variants[i]);

                if (valid.Count > 0)
                    return valid[UnityEngine.Random.Range(0, valid.Count)];
            }

            return sound.Clip;
        }

        private static float ResolveVolume(AudioSoundDefinition sound)
        {
            float delta = sound.VolumeRandom > 0f ? UnityEngine.Random.Range(-sound.VolumeRandom, sound.VolumeRandom) : 0f;
            return Mathf.Clamp01(sound.Volume + delta);
        }

        private static float ResolvePitch(AudioSoundDefinition sound)
        {
            float delta = sound.PitchRandom > 0f ? UnityEngine.Random.Range(-sound.PitchRandom, sound.PitchRandom) : 0f;
            return Mathf.Clamp(sound.Pitch + delta, -3f, 3f);
        }

        private void RegisterActive(AudioSource source, string key)
        {
            if (!_active.Contains(source))
                _active.Add(source);

            _activeKeys[source] = key;
            _activeCountByKey.TryGetValue(key, out int count);
            _activeCountByKey[key] = count + 1;
        }

        private void Release(AudioSource source)
        {
            if (source == null)
                return;

            _active.Remove(source);
            if (_activeKeys.TryGetValue(source, out var key))
            {
                _activeKeys.Remove(source);
                if (_activeCountByKey.TryGetValue(key, out int count))
                {
                    count--;
                    if (count <= 0) _activeCountByKey.Remove(key);
                    else _activeCountByKey[key] = count;
                }
            }

            source.Stop();
            source.clip = null;
            source.loop = false;
            source.transform.SetParent(_root != null ? _root.transform : null, false);
            source.gameObject.SetActive(false);
            _available.Enqueue(source);
        }
    }

    public static class AudioInstaller
    {
        private const string DefaultRegistryResourcePath = "MoyvaAudioRegistry";

        public static void Install(DiContainer container, AudioRegistrySO registry = null)
        {
            if (!container.HasBinding<AudioRegistrySO>())
            {
                registry ??= Resources.Load<AudioRegistrySO>(DefaultRegistryResourcePath);
                if (registry != null)
                    container.BindInstance(registry).AsSingle();
            }

            if (!container.HasBinding<IAudioService>())
                container.BindInterfacesAndSelfTo<AudioService>().AsSingle().NonLazy();
        }
    }
}

namespace Kruty1918.Moyva.Shared.Graphics
{
    public enum GraphicsQualityProfile
    {
        Auto = 0,
        Performance = 1,
        Balanced = 2,
        Quality = 3,
        Custom = 4
    }

    public struct GraphicsSettingsData
    {
        public GraphicsQualityProfile Profile;
        public int TargetFrameRate;
        public float RenderScale;
        public bool DynamicRenderScale;
        public bool CloseZoomOptimization;
        public int TextureMipmapLimit;
        public int AntiAliasing;
        public bool VSync;
        public bool Shadows;
        public bool AnisotropicFiltering;
        public float LodBias;

        public GraphicsSettingsData(
            GraphicsQualityProfile profile,
            int targetFrameRate,
            float renderScale,
            bool dynamicRenderScale,
            bool closeZoomOptimization,
            int textureMipmapLimit,
            int antiAliasing,
            bool vSync,
            bool shadows,
            bool anisotropicFiltering,
            float lodBias)
        {
            Profile = profile;
            TargetFrameRate = NormalizeFrameRate(targetFrameRate);
            RenderScale = Mathf.Clamp(renderScale, 0.42f, 1f);
            DynamicRenderScale = dynamicRenderScale;
            CloseZoomOptimization = closeZoomOptimization;
            TextureMipmapLimit = Mathf.Clamp(textureMipmapLimit, 0, 3);
            AntiAliasing = NormalizeAntiAliasing(antiAliasing);
            VSync = vSync;
            Shadows = shadows;
            AnisotropicFiltering = anisotropicFiltering;
            LodBias = Mathf.Clamp(lodBias, 0.4f, 2f);
        }

        public static GraphicsSettingsData CreateDefault()
        {
            return ForProfile(GraphicsQualityProfile.Auto, IsMobileRuntime());
        }

        public static GraphicsSettingsData ForProfile(GraphicsQualityProfile profile, bool isMobile)
        {
            switch (profile)
            {
                case GraphicsQualityProfile.Performance:
                    return new GraphicsSettingsData(profile, 60, isMobile ? 0.60f : 0.80f, true, true, isMobile ? 1 : 0, 0, false, false, false, 0.70f);
                case GraphicsQualityProfile.Quality:
                    return new GraphicsSettingsData(profile, 60, isMobile ? 0.90f : 1f, isMobile, true, 0, isMobile ? 0 : 2, false, !isMobile, true, 1.15f);
                case GraphicsQualityProfile.Balanced:
                    return new GraphicsSettingsData(profile, 60, isMobile ? 0.75f : 1f, true, true, 0, 0, false, false, true, 0.90f);
                default:
                    return isMobile
                        ? new GraphicsSettingsData(GraphicsQualityProfile.Auto, 60, 0.75f, true, true, 0, 0, false, false, true, 0.85f)
                        : new GraphicsSettingsData(GraphicsQualityProfile.Auto, 60, 1f, false, false, 0, 0, false, true, true, 1f);
            }
        }

        public GraphicsSettingsData WithProfile(GraphicsQualityProfile profile)
        {
            if (profile == GraphicsQualityProfile.Custom)
            {
                return new GraphicsSettingsData(
                    GraphicsQualityProfile.Custom,
                    TargetFrameRate,
                    RenderScale,
                    DynamicRenderScale,
                    CloseZoomOptimization,
                    TextureMipmapLimit,
                    AntiAliasing,
                    VSync,
                    Shadows,
                    AnisotropicFiltering,
                    LodBias);
            }

            return ForProfile(profile, IsMobileRuntime());
        }
        public GraphicsSettingsData WithTargetFrameRate(int value) => AsCustom(TargetFrameRate: NormalizeFrameRate(value));
        public GraphicsSettingsData WithRenderScale(float value) => AsCustom(RenderScale: Mathf.Clamp(value, 0.42f, 1f));
        public GraphicsSettingsData WithDynamicRenderScale(bool value) => AsCustom(DynamicRenderScale: value);
        public GraphicsSettingsData WithCloseZoomOptimization(bool value) => AsCustom(CloseZoomOptimization: value);
        public GraphicsSettingsData WithTextureMipmapLimit(int value) => AsCustom(TextureMipmapLimit: Mathf.Clamp(value, 0, 3));
        public GraphicsSettingsData WithAntiAliasing(int value) => AsCustom(AntiAliasing: NormalizeAntiAliasing(value));
        public GraphicsSettingsData WithVSync(bool value) => AsCustom(VSync: value);
        public GraphicsSettingsData WithShadows(bool value) => AsCustom(Shadows: value);
        public GraphicsSettingsData WithAnisotropicFiltering(bool value) => AsCustom(AnisotropicFiltering: value);
        public GraphicsSettingsData WithLodBias(float value) => AsCustom(LodBias: Mathf.Clamp(value, 0.4f, 2f));

        private GraphicsSettingsData AsCustom(
            int? TargetFrameRate = null,
            float? RenderScale = null,
            bool? DynamicRenderScale = null,
            bool? CloseZoomOptimization = null,
            int? TextureMipmapLimit = null,
            int? AntiAliasing = null,
            bool? VSync = null,
            bool? Shadows = null,
            bool? AnisotropicFiltering = null,
            float? LodBias = null)
        {
            return new GraphicsSettingsData(
                GraphicsQualityProfile.Custom,
                TargetFrameRate ?? this.TargetFrameRate,
                RenderScale ?? this.RenderScale,
                DynamicRenderScale ?? this.DynamicRenderScale,
                CloseZoomOptimization ?? this.CloseZoomOptimization,
                TextureMipmapLimit ?? this.TextureMipmapLimit,
                AntiAliasing ?? this.AntiAliasing,
                VSync ?? this.VSync,
                Shadows ?? this.Shadows,
                AnisotropicFiltering ?? this.AnisotropicFiltering,
                LodBias ?? this.LodBias);
        }

        private static bool IsMobileRuntime()
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return Application.isMobilePlatform;
#endif
        }

        private static int NormalizeFrameRate(int value)
        {
            if (value <= 0)
                return 60;

            return Mathf.Clamp(value, 30, 120);
        }

        private static int NormalizeAntiAliasing(int value)
        {
            if (value >= 4)
                return 4;

            if (value >= 2)
                return 2;

            return 0;
        }
    }

    public interface IGraphicsSettingsService
    {
        GraphicsSettingsData Settings { get; }
        event Action<GraphicsSettingsData> OnSettingsChanged;

        void SetProfile(GraphicsQualityProfile profile);
        void SetTargetFrameRate(int frameRate);
        void SetRenderScale(float renderScale);
        void SetDynamicRenderScale(bool enabled);
        void SetCloseZoomOptimization(bool enabled);
        void SetTextureMipmapLimit(int mipmapLimit);
        void SetAntiAliasing(int antiAliasing);
        void SetVSync(bool enabled);
        void SetShadows(bool enabled);
        void SetAnisotropicFiltering(bool enabled);
        void SetLodBias(float lodBias);
        void ResetToDefaults();
        void ApplyCurrentSettings();
    }

    internal sealed class GraphicsSettingsService : IGraphicsSettingsService, IInitializable
    {
        private const int Version = 1;
        private readonly string _filePath;
        private readonly GraphicsSettingsData _startupDefaults;
        private readonly DeveloperPixelOptimizationSettings _developerPixelOptimization;
        private RenderPipelineAsset _activeRenderPipeline;
        private PropertyInfo _renderScaleProperty;

        public GraphicsSettingsData Settings { get; private set; }
        public event Action<GraphicsSettingsData> OnSettingsChanged;

        public GraphicsSettingsService()
        {
            _filePath = Path.Combine(Application.persistentDataPath, MultiplayerClientScope.BuildScopedFileName("graphics_settings.dat"));
            _startupDefaults = GraphicsStartupDefaultsProvider.LoadDefaults();
            _developerPixelOptimization = GraphicsStartupDefaultsProvider.LoadDeveloperPixelOptimization();
            Settings = _startupDefaults;
        }

        public void Initialize()
        {
            Settings = LoadOrCreate();
            ApplyCurrentSettings();
            Save(Settings);
            OnSettingsChanged?.Invoke(Settings);
        }

        public void SetProfile(GraphicsQualityProfile profile) => Update(Settings.WithProfile(profile));
        public void SetTargetFrameRate(int frameRate) => Update(Settings.WithTargetFrameRate(frameRate));
        public void SetRenderScale(float renderScale) => Update(Settings.WithRenderScale(renderScale));
        public void SetDynamicRenderScale(bool enabled) => Update(Settings.WithDynamicRenderScale(enabled));
        public void SetCloseZoomOptimization(bool enabled) => Update(Settings.WithCloseZoomOptimization(enabled));
        public void SetTextureMipmapLimit(int mipmapLimit) => Update(Settings.WithTextureMipmapLimit(mipmapLimit));
        public void SetAntiAliasing(int antiAliasing) => Update(Settings.WithAntiAliasing(antiAliasing));
        public void SetVSync(bool enabled) => Update(Settings.WithVSync(enabled));
        public void SetShadows(bool enabled) => Update(Settings.WithShadows(enabled));
        public void SetAnisotropicFiltering(bool enabled) => Update(Settings.WithAnisotropicFiltering(enabled));
        public void SetLodBias(float lodBias) => Update(Settings.WithLodBias(lodBias));
        public void ResetToDefaults() => Update(_startupDefaults);

        public void ApplyCurrentSettings()
        {
            Apply(Settings);
        }

        private void Update(GraphicsSettingsData next)
        {
            if (AreEquivalent(Settings, next))
                return;

            Settings = next;
            ApplyCurrentSettings();
            Save(Settings);
            OnSettingsChanged?.Invoke(Settings);
        }

        private GraphicsSettingsData LoadOrCreate()
        {
            if (!File.Exists(_filePath))
                return _startupDefaults;

            try
            {
                using var stream = File.OpenRead(_filePath);
                using var reader = new BinaryReader(stream);
                int version = reader.ReadInt32();
                if (version != Version)
                    return _startupDefaults;

                return new GraphicsSettingsData(
                    (GraphicsQualityProfile)reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadSingle(),
                    reader.ReadBoolean(),
                    reader.ReadBoolean(),
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadBoolean(),
                    reader.ReadBoolean(),
                    reader.ReadBoolean(),
                    reader.ReadSingle());
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GraphicsSettings] Failed to load graphics settings: {e.Message}. Defaults will be used.");
                return _startupDefaults;
            }
        }

        private void Save(GraphicsSettingsData settings)
        {
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using var stream = File.Create(_filePath);
                using var writer = new BinaryWriter(stream);
                writer.Write(Version);
                writer.Write((int)settings.Profile);
                writer.Write(settings.TargetFrameRate);
                writer.Write(settings.RenderScale);
                writer.Write(settings.DynamicRenderScale);
                writer.Write(settings.CloseZoomOptimization);
                writer.Write(settings.TextureMipmapLimit);
                writer.Write(settings.AntiAliasing);
                writer.Write(settings.VSync);
                writer.Write(settings.Shadows);
                writer.Write(settings.AnisotropicFiltering);
                writer.Write(settings.LodBias);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GraphicsSettings] Failed to save graphics settings: {e.Message}");
            }
        }

        private void Apply(GraphicsSettingsData settings)
        {
            var effective = ApplyDeveloperPixelOverride(settings);

            QualitySettings.vSyncCount = effective.VSync ? 1 : 0;
            Application.targetFrameRate = effective.VSync ? -1 : effective.TargetFrameRate;
            OnDemandRendering.renderFrameInterval = 1;
            QualitySettings.antiAliasing = effective.AntiAliasing;
            QualitySettings.shadows = effective.Shadows ? ShadowQuality.HardOnly : ShadowQuality.Disable;
            QualitySettings.shadowDistance = effective.Shadows ? 20f : 0f;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.softParticles = false;
            QualitySettings.softVegetation = false;
            QualitySettings.globalTextureMipmapLimit = effective.TextureMipmapLimit;
            QualitySettings.anisotropicFiltering = effective.AnisotropicFiltering ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
            QualitySettings.lodBias = effective.LodBias;
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.resolutionScalingFixedDPIFactor = effective.RenderScale;
            TryApplyRenderScale(effective.RenderScale);
        }

        private GraphicsSettingsData ApplyDeveloperPixelOverride(GraphicsSettingsData settings)
        {
            if (!_developerPixelOptimization.Enabled)
                return settings;

            var next = settings.WithRenderScale(Mathf.Min(settings.RenderScale, _developerPixelOptimization.RenderScaleCap));
            next = next.WithTextureMipmapLimit(Mathf.Max(next.TextureMipmapLimit, _developerPixelOptimization.MinimumTextureMipmapLimit));

            if (_developerPixelOptimization.ForceDisableAntiAliasing)
                next = next.WithAntiAliasing(0);

            if (_developerPixelOptimization.ForceDisableAnisotropicFiltering)
                next = next.WithAnisotropicFiltering(false);

            return next;
        }

        private void TryApplyRenderScale(float renderScale)
        {
            var pipeline = QualitySettings.renderPipeline ?? GraphicsSettings.currentRenderPipeline;
            if (pipeline == null)
                return;

            if (_activeRenderPipeline != pipeline || _renderScaleProperty == null)
            {
                _activeRenderPipeline = pipeline;
                _renderScaleProperty = pipeline.GetType().GetProperty("renderScale", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (_renderScaleProperty == null || !_renderScaleProperty.CanWrite || _renderScaleProperty.PropertyType != typeof(float))
                return;

            _renderScaleProperty.SetValue(pipeline, Mathf.Clamp(renderScale, 0.42f, 1f));
        }

        private static bool AreEquivalent(GraphicsSettingsData left, GraphicsSettingsData right)
        {
            return left.Profile == right.Profile
                && left.TargetFrameRate == right.TargetFrameRate
                && Mathf.Approximately(left.RenderScale, right.RenderScale)
                && left.DynamicRenderScale == right.DynamicRenderScale
                && left.CloseZoomOptimization == right.CloseZoomOptimization
                && left.TextureMipmapLimit == right.TextureMipmapLimit
                && left.AntiAliasing == right.AntiAliasing
                && left.VSync == right.VSync
                && left.Shadows == right.Shadows
                && left.AnisotropicFiltering == right.AnisotropicFiltering
                && Mathf.Approximately(left.LodBias, right.LodBias);
        }
    }
}
