using System;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Audio.Runtime;
using Kruty1918.Moyva.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap
{
    /// <summary>
    /// ProjectContext-level installer for services shared across all scenes.
    /// </summary>
    public sealed class ProjectServicesInstaller : MonoInstaller
    {
        [Header("Audio")]
        [Tooltip("Реєстр звуків. Якщо порожньо — завантажується з Resources/MoyvaAudioRegistry.")]
        [SerializeField] private AudioRegistrySO _audioRegistry;

        [Tooltip("Per-scene overrides для звуків. Якщо порожньо — завантажується з Resources/MoyvaSceneAudioOverrides.")]
        [SerializeField] private SceneAudioOverridesSO _sceneOverrides;

        [Tooltip("Профілі музики сцен. Може бути порожнім — тоді музика не налаштовується автоматично.")]
        [SerializeField] private SceneMusicProfileSO[] _musicProfiles;

        public override void InstallBindings()
        {
            SharedInstaller.Install(Container);

            AudioInstaller.Install(Container, _audioRegistry, _musicProfiles, _sceneOverrides);

            SaveSystemInstaller.Install(Container);

            MultiplayerInstaller.Install(Container);

            Container.BindInterfacesTo<MobileGraphicsAutoScaler>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ResponsiveCanvasScalerService>().AsSingle().NonLazy();
        }
    }

    internal sealed class ResponsiveCanvasScalerService : IInitializable, ITickable, IDisposable
    {
        private static readonly Vector2 LandscapeReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Vector2 PortraitReferenceResolution = new Vector2(1080f, 1920f);
        private const float AspectBlendRange = 0.35f;
        private const float DiscoveryIntervalSeconds = 2f;
        private const float FallbackDpi = 160f;

        private int _lastScreenWidth;
        private int _lastScreenHeight;
        private ScreenOrientation _lastOrientation;
        private float _nextDiscoveryAt;

        public void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplyToAllCanvases();
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void Tick()
        {
            if (!HasScreenChanged() && Time.unscaledTime < _nextDiscoveryAt)
                return;

            ApplyToAllCanvases();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyToAllCanvases();
        }

        private bool HasScreenChanged()
        {
            return _lastScreenWidth != Screen.width
                || _lastScreenHeight != Screen.height
                || _lastOrientation != Screen.orientation;
        }

        private void ApplyToAllCanvases()
        {
            _lastScreenWidth = Mathf.Max(1, Screen.width);
            _lastScreenHeight = Mathf.Max(1, Screen.height);
            _lastOrientation = Screen.orientation;
            _nextDiscoveryAt = Time.unscaledTime + DiscoveryIntervalSeconds;

            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
                ApplyToCanvas(canvases[i]);
        }

        private void ApplyToCanvas(Canvas canvas)
        {
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                return;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            Vector2 referenceResolution = ResolveReferenceResolution();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = ResolveMatchWidthOrHeight(referenceResolution);
            scaler.referencePixelsPerUnit = 100f;
            scaler.fallbackScreenDPI = FallbackDpi;
            scaler.defaultSpriteDPI = FallbackDpi;
        }

        private Vector2 ResolveReferenceResolution()
        {
            return _lastScreenHeight > _lastScreenWidth
                ? PortraitReferenceResolution
                : LandscapeReferenceResolution;
        }

        private float ResolveMatchWidthOrHeight(Vector2 referenceResolution)
        {
            float screenAspect = _lastScreenWidth / (float)_lastScreenHeight;
            float referenceAspect = referenceResolution.x / referenceResolution.y;
            float minAspect = referenceAspect - AspectBlendRange;
            float maxAspect = referenceAspect + AspectBlendRange;

            return Mathf.Clamp01(Mathf.InverseLerp(minAspect, maxAspect, screenAspect));
        }
    }
}
