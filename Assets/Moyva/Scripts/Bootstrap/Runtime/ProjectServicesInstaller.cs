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
        public override void InstallBindings()
        {
            // Install shared services (connectivity, health checks, etc.) first
            SharedInstaller.Install(Container);

            // Project-level audio framework: registry lookup, pooled sources,
            // and automatic per-sound effect application.
            AudioInstaller.Install(Container);

            // Save system installer belongs to the SaveSystem library and is
            // safe to install here because it's a project-level orchestration.
            SaveSystemInstaller.Install(Container);

            // The composition root (ProjectServicesInstaller) is responsible for
            // orchestrating which feature libraries are installed at startup.
            // It's acceptable for the bootstrap to invoke a library's installer
            // so that the library can register its own dependencies in the
            // project's DI container. Call the multiplayer install helper to
            // ensure minimal switchable wrappers (ILobbyService / INetworkProvider)
            // are available synchronously for UI services during startup.
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
