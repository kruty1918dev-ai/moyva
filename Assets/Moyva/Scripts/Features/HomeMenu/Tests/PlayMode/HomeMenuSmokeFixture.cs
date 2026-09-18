using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Tests.HomeMenu.PlayMode
{
    /// <summary>
    /// Спільний PlayMode-стенд: завантажує реальну HomeMenu-сцену, чекає на маунт
    /// dynamic MoyvaUI, рендерить канвас у RenderTexture різних розмірів і збирає
    /// console output. Скріншоти пишуться у Temp/ai/smoke/.
    /// </summary>
    public abstract class HomeMenuSmokeFixture
    {
        private const string MenuSceneName = "HomeMenu";
        protected static readonly string ScreenshotDir =
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "ai", "smoke"));

        protected readonly List<string> Errors = new();
        protected readonly List<string> FatalErrors = new();
        protected readonly List<string> Warnings = new();

        // Internal — member types are internals of the production assembly exposed via InternalsVisibleTo.
        internal DiContainer Container { get; private set; }
        internal INavigation Nav { get; private set; }
        internal HomeMenuMoyvaUiViewController View { get; private set; }
        internal HomeMenuMoyvaUiState State { get; private set; }
        internal HomeMenuMoyvaUiBridge Bridge { get; private set; }
        internal HomeMenuMoyvaUiPresenter Presenter { get; private set; }
        internal HomeMenuMoyvaUiAnchor Anchor { get; private set; }

        private Camera _camera;
        private Canvas _canvas;
        private CanvasScaler _scaler;
        private RenderMode _origRenderMode;
        private CanvasScaler.ScaleMode _origScaleMode;
        private float _origScaleFactor;
        private float _origPlaneDistance;
        private Camera _origWorldCamera;
        private RenderTexture _rt;

        protected IEnumerator LoadMenu()
        {
            Directory.CreateDirectory(ScreenshotDir);
            Application.logMessageReceived += OnLog;
            LogAssert.ignoreFailingMessages = true;

            var op = SceneManager.LoadSceneAsync(MenuSceneName, LoadSceneMode.Single);
            while (op != null && !op.isDone)
                yield return null;

            // Zenject SceneContext installs during scene Awake; resolve on the next frame.
            yield return null;

            var sceneContext = UnityEngine.Object.FindFirstObjectByType<SceneContext>();
            Assert.IsNotNull(sceneContext, "SceneContext not found in HomeMenu scene.");
            Container = sceneContext.Container;
            Assert.IsNotNull(Container, "Zenject container is not initialized.");

            Nav = Container.Resolve<INavigation>();
            View = Container.Resolve<HomeMenuMoyvaUiViewController>();
            State = Container.Resolve<HomeMenuMoyvaUiState>();
            Presenter = Container.Resolve<HomeMenuMoyvaUiPresenter>();
            Bridge = Presenter.Bridge;
            Assert.IsNotNull(Bridge, "MoyvaUI bridge not available — presenter did not initialize.");

            var anchors = Container.ResolveAll<HomeMenuMoyvaUiAnchor>();
            foreach (var anchor in anchors)
            {
                if (anchor != null && anchor.MountRoot != null)
                {
                    Anchor = anchor;
                    break;
                }
            }
            Assert.IsNotNull(Anchor, "No HomeMenuMoyvaUiAnchor with a mount root in scene.");

            // Wait until the host mounts the document (anchor prepares + host builds the tree).
            yield return WaitUntil(
                () => State.IsMounted && Anchor.MountRoot.childCount > 0,
                TimeSpan.FromSeconds(20),
                "dynamic MoyvaUI did not mount");

            SetupCaptureRig();
            // Let the initial route settle.
            yield return WaitRealtime(0.6f);
        }

        [TearDown]
        public void SmokeTearDown()
        {
            Application.logMessageReceived -= OnLog;
            LogAssert.ignoreFailingMessages = false;
            TeardownCaptureRig();
        }

        private void SetupCaptureRig()
        {
            _canvas = Anchor.MountRoot.GetComponentInParent<Canvas>();
            Assert.IsNotNull(_canvas, "Mount root is not inside a Canvas.");
            _scaler = _canvas.GetComponent<CanvasScaler>();

            _origRenderMode = _canvas.renderMode;
            _origWorldCamera = _canvas.worldCamera;
            _origPlaneDistance = _canvas.planeDistance;
            _origScaleMode = _scaler != null ? _scaler.uiScaleMode : CanvasScaler.ScaleMode.ConstantPixelSize;
            _origScaleFactor = _scaler != null ? _scaler.scaleFactor : 1f;

            var cameraGo = new GameObject("SmokeCaptureCamera");
            _camera = cameraGo.AddComponent<Camera>();
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.043f, 0.047f, 0.07f);
            _camera.cullingMask = ~0;
            _camera.orthographic = true;
            _camera.nearClipPlane = -10f;
            _camera.farClipPlane = 10f;
            _camera.enabled = false; // manual Render() only

            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = _camera;
            _canvas.planeDistance = 2f; // default 100 sits beyond the rig's far clip plane
            // Disable the scaler: in batchmode it keeps ScaleWithScreenSize(ref 1280x720)
            // applied, pinning canvas units to 1280 regardless of the render-target size.
            if (_scaler != null)
                _scaler.enabled = false;
            _canvas.scaleFactor = 1f;
        }

        private void TeardownCaptureRig()
        {
            if (_camera != null)
            {
                _camera.targetTexture = null;
                UnityEngine.Object.Destroy(_camera.gameObject);
                _camera = null;
            }
            if (_canvas != null)
            {
                _canvas.renderMode = _origRenderMode;
                _canvas.worldCamera = _origWorldCamera;
                _canvas.planeDistance = _origPlaneDistance;
            }
            if (_scaler != null)
            {
                _scaler.enabled = true;
                _scaler.uiScaleMode = _origScaleMode;
                _scaler.scaleFactor = _origScaleFactor;
            }
            if (_rt != null)
            {
                _rt.Release();
                UnityEngine.Object.Destroy(_rt);
                _rt = null;
            }
        }

        /// <summary>Розмір канвасу = розмір RenderTexture камери (ScreenSpaceCamera mode).</summary>
        protected IEnumerator SetViewport(int width, int height)
        {
            if (_rt != null && (_rt.width != width || _rt.height != height))
            {
                _camera.targetTexture = null;
                _rt.Release();
                UnityEngine.Object.Destroy(_rt);
                _rt = null;
            }
            if (_rt == null)
            {
                _rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
                {
                    antiAliasing = 2,
                    name = $"smoke-{width}x{height}"
                };
                _rt.Create();
            }
            _camera.targetTexture = _rt;
            Canvas.ForceUpdateCanvases();
            // Anchor relayouts on size change; presenter re-renders on viewport-class change.
            yield return null;
            yield return null;
        }

        /// <summary>Дочікується спокійного стану після навігації та робить скріншот.</summary>
        protected IEnumerator Capture(string name, float settleSeconds = 0.45f)
        {
            Debug.Log($"[smoke-step] capture '{name}' settle");
            yield return WaitRealtime(settleSeconds);
            Debug.Log($"[smoke-step] capture '{name}' force-update");
            Canvas.ForceUpdateCanvases();
            yield return null;

            Debug.Log($"[smoke-step] capture '{name}' render");
            _camera.Render();
            RenderTexture.active = _rt;
            var tex = new Texture2D(_rt.width, _rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            var path = Path.Combine(ScreenshotDir, $"{name}.png");
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.Destroy(tex);
            Debug.Log($"[smoke-shot] {path}");
        }

        protected IEnumerator OpenRoute(string route, float settle = 0.5f)
        {
            Nav.Open(route);
            yield return WaitRealtime(settle);
        }

        protected IEnumerator CloseLast(float settle = 0.4f)
        {
            Nav.CloseLast();
            yield return WaitRealtime(settle);
        }

        protected static IEnumerator WaitUntil(Func<bool> condition, TimeSpan timeout, string what)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + timeout.TotalSeconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartupAsDouble > deadline)
                    Assert.Fail($"Timed out waiting for: {what}");
                yield return null;
            }
        }

        protected static IEnumerator WaitRealtime(float seconds)
        {
            var deadline = Time.realtimeSinceStartupAsDouble + seconds;
            while (Time.realtimeSinceStartupAsDouble < deadline)
                yield return null;
        }

        /// <summary>Діагностика горизонтального overflow: логує найширші RectTransform-и та їхній правий край.</summary>
        protected void LogOverflowingElements()
        {
            if (Anchor == null || Anchor.MountRoot == null) return;
            var rects = Anchor.MountRoot.GetComponentsInChildren<RectTransform>(true);
            var widest = new List<(RectTransform rt, float w, float right)>();
            var corners = new Vector3[4];
            foreach (var rt in rects)
            {
                rt.GetWorldCorners(corners);
                widest.Add((rt, rt.rect.width, corners[2].x));
            }
            widest.Sort((a, b) => b.w.CompareTo(a.w));
            foreach (var (rt, w, right) in widest.Take(18))
                TestContext.Out.WriteLine($"[smoke-overflow] w={w:F0} right={right:F1} : {GetPath(rt)}");
        }

        private static string GetPath(RectTransform rt)
        {
            var sb = new StringBuilder(rt.name);
            for (var p = rt.parent; p != null && p.GetComponentInParent<Canvas>() != p; p = p.parent)
                sb.Insert(0, p.name + "/");
            return sb.ToString();
        }

        /// <summary>Статуси, які вважаємо очікуваним шумом offline/batch середовища.</summary>
        private static readonly string[] ToleratedErrorFragments =
        {
            "multiplayer services", "Unity services", "Authentication", "Relay",
            "connectivity", "Multiplayer initialization", "ILobbyService",
            "Lobby service is unavailable", "timed out", "TimeoutException",
            "Vivox", "Couldn't resolve host", "network"
        };

        protected int UnexpectedErrorCount()
        {
            var count = 0;
            foreach (var error in Errors)
            {
                var tolerated = false;
                foreach (var fragment in ToleratedErrorFragments)
                {
                    if (error.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        tolerated = true;
                        break;
                    }
                }
                if (!tolerated)
                    count++;
            }
            return count;
        }

        protected void ReportLogSummary()
        {
            TestContext.Out.WriteLine($"[smoke-log] errors={Errors.Count} fatal={FatalErrors.Count} warnings={Warnings.Count} unexpectedErrors={UnexpectedErrorCount()}");
            foreach (var error in Errors)
                TestContext.Out.WriteLine($"[smoke-error] {error}");
            foreach (var fatal in FatalErrors)
                TestContext.Out.WriteLine($"[smoke-fatal] {fatal}");
        }

        /// <summary>Жорстке падіння тесту лише на Exception/Assert — LogError degraded paths репортимо.</summary>
        protected void AssertNoFatalLogs()
        {
            Assert.That(FatalErrors, Is.Empty,
                "Exceptions/asserts observed during the smoke pass:\n" + string.Join("\n", FatalErrors));
        }

        private void OnLog(string message, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Exception:
                case LogType.Assert:
                    FatalErrors.Add($"{type}: {message.Split('\n')[0]}");
                    break;
                case LogType.Error:
                    Errors.Add($"Error: {message.Split('\n')[0]}");
                    break;
                case LogType.Warning:
                    Warnings.Add(message.Split('\n')[0]);
                    break;
            }
        }
    }
}
