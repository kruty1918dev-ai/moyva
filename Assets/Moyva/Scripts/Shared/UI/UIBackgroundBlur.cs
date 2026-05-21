using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kruty1918.Moyva.Shared.UI
{
    /// <summary>
    /// Blurs UI background content behind this Graphic in both Edit and Play mode.
    /// </summary>
    [AddComponentMenu("UI/Effects/UI Background Blur")]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public class UIBackgroundBlur : MonoBehaviour, IMaterialModifier
    {
        private const string ShaderName = "Moyva/UI/BackgroundBlur";

        [Header("Blur")]
        [SerializeField, Range(0f, 20f)] private float _blurStrength = 6f;
        [SerializeField, Range(1, 8)] private int _downsample = 2;

        [Header("Fade")]
        [SerializeField, Min(0f)] private float _fadeInDuration = 0.18f;
        [SerializeField, Min(0f)] private float _fadeOutDuration = 0.16f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Render")]
        [SerializeField] private Shader _shader;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs;

        private Graphic _graphic;
        private Material _runtimeMaterial;

        private float _currentOpacity = 1f;
        private float _targetOpacity = 1f;
        private float _fadeDuration;
        private float _fadeElapsed;
        private float _fadeStartOpacity;
        private bool _isFading;
        private bool _deactivateAfterFadeOut;
        private bool _loggedMissingShader;
        private bool _loggedNoCaptureTexture;
        private bool _loggedCaptureTextureRestored;

        private int _lastScreenWidth = -1;
        private int _lastScreenHeight = -1;
        private double _lastEditorTime;

        private static readonly int BlurSizeId = Shader.PropertyToID("_BlurSize");
        private static readonly int DownsampleId = Shader.PropertyToID("_Downsample");
        private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        private static readonly int BackgroundTexId = Shader.PropertyToID("_BackgroundTex");
        private static readonly int BackgroundAvailableId = Shader.PropertyToID("_BackgroundAvailable");

        internal int CaptureDownsample => _downsample;
        internal bool IsReadyForCapture => isActiveAndEnabled && _graphic != null && _graphic.enabled && _graphic.canvas != null;
        internal bool NeedsContinuousCapture => _isFading;
        internal Graphic Graphic => _graphic;
        internal bool DebugLogsEnabled => _enableDebugLogs;

        public float BlurStrength => _blurStrength;
        public int Downsample => _downsample;

        private void Reset()
        {
            _graphic = GetComponent<Graphic>();
            AssignDefaultShader();
            _currentOpacity = 1f;
            _targetOpacity = 1f;
        }

        private void OnEnable()
        {
            EnsureGraphic();
            AssignDefaultShader();

            CaptureService.Register(this);
            LogInfo($"Enabled. blur={_blurStrength:0.##}, downsample={_downsample}, fadeIn={_fadeInDuration:0.###}, fadeOut={_fadeOutDuration:0.###}");

            if (_fadeInDuration > 0f)
            {
                _currentOpacity = 0f;
                BeginFade(1f, _fadeInDuration, false);
            }
            else
            {
                _currentOpacity = 1f;
                _targetOpacity = 1f;
                _isFading = false;
            }

            Refresh();
            MarkGraphicDirty();
        }

        private void OnDisable()
        {
            CaptureService.Unregister(this);
            _isFading = false;
            _deactivateAfterFadeOut = false;
            LogInfo("Disabled.");
            ReleaseMaterial();
        }

        private void OnDestroy()
        {
            CaptureService.Unregister(this);
            ReleaseMaterial();
        }

        private void OnValidate()
        {
            _blurStrength = Mathf.Clamp(_blurStrength, 0f, 20f);
            _downsample = Mathf.Clamp(_downsample, 1, 8);
            _fadeInDuration = Mathf.Max(0f, _fadeInDuration);
            _fadeOutDuration = Mathf.Max(0f, _fadeOutDuration);
            if (_fadeCurve == null)
                _fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            EnsureGraphic();
            AssignDefaultShader();
            Refresh();
            MarkGraphicDirty();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
                return;

            Refresh();
            MarkGraphicDirty();
        }

        private void Update()
        {
            if (_shader == null)
                AssignDefaultShader();

            bool screenChanged = _lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height;
            if (screenChanged)
            {
                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;
                Refresh();
            }

            if (_isFading)
            {
                TickFade(GetDeltaTime());
            }
        }

        public void Show()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            _deactivateAfterFadeOut = false;
            BeginFade(1f, _fadeInDuration, false);
            LogInfo("Show() called.");
        }

        public void Hide()
        {
            if (!isActiveAndEnabled)
                return;

            BeginFade(0f, _fadeOutDuration, true);
            LogInfo("Hide() called.");
        }

        public void SetBlur(float value)
        {
            float clamped = Mathf.Clamp(value, 0f, 20f);
            if (!Mathf.Approximately(_blurStrength, clamped))
                LogInfo($"SetBlur({value:0.###}) -> {clamped:0.###}");

            _blurStrength = clamped;
            Refresh();
            MarkGraphicDirty();
        }

        public void SetPixelation(int value)
        {
            int clamped = Mathf.Clamp(value, 1, 8);
            if (_downsample != clamped)
                LogInfo($"SetPixelation({value}) -> {clamped}");

            _downsample = clamped;
            Refresh();
            MarkGraphicDirty();
        }

        public void Refresh()
        {
            CaptureService.MarkDirty();
            MarkGraphicDirty();
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!isActiveAndEnabled || baseMaterial == null)
                return baseMaterial;

            Shader targetShader = ResolveShader();
            if (targetShader == null)
                return baseMaterial;

            EnsureMaterial(baseMaterial, targetShader);
            ApplyMaterialParameters();
            return _runtimeMaterial;
        }

        private void EnsureGraphic()
        {
            if (_graphic == null)
                _graphic = GetComponent<Graphic>();
        }

        private Shader ResolveShader()
        {
            AssignDefaultShader();

            if (_shader == null)
            {
                if (!_loggedMissingShader)
                {
                    LogWarn($"Shader '{ShaderName}' not found. Blur will be disabled until the shader is available.");
                    _loggedMissingShader = true;
                }
            }
            else
            {
                _loggedMissingShader = false;
            }

            return _shader;
        }

        private void AssignDefaultShader()
        {
            if (_shader == null)
                _shader = Shader.Find(ShaderName);
        }

        private void EnsureMaterial(Material baseMaterial, Shader targetShader)
        {
            if (_runtimeMaterial == null || _runtimeMaterial.shader != targetShader)
            {
                ReleaseMaterial();
                _runtimeMaterial = new Material(targetShader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            _runtimeMaterial.CopyPropertiesFromMaterial(baseMaterial);
            _runtimeMaterial.shader = targetShader;
        }

        private void ApplyMaterialParameters()
        {
            if (_runtimeMaterial == null)
                return;

            float effectiveBlur = _blurStrength * Mathf.Clamp01(_currentOpacity);
            _runtimeMaterial.SetFloat(BlurSizeId, effectiveBlur);
            _runtimeMaterial.SetFloat(DownsampleId, _downsample);
            _runtimeMaterial.SetFloat(OpacityId, _currentOpacity);

            if (CaptureService.TryGetCaptureTexture(out Texture captureTexture))
            {
                _runtimeMaterial.SetTexture(BackgroundTexId, captureTexture);
                _runtimeMaterial.SetFloat(BackgroundAvailableId, 1f);

                if (_loggedNoCaptureTexture)
                {
                    if (!_loggedCaptureTextureRestored)
                    {
                        LogInfo("Background capture texture restored.");
                        _loggedCaptureTextureRestored = true;
                    }

                    _loggedNoCaptureTexture = false;
                }
            }
            else
            {
                _runtimeMaterial.SetTexture(BackgroundTexId, Texture2D.blackTexture);
                _runtimeMaterial.SetFloat(BackgroundAvailableId, 0f);

                if (!_loggedNoCaptureTexture)
                {
                    LogWarn("No background capture texture available. Using fallback black texture.");
                    _loggedNoCaptureTexture = true;
                    _loggedCaptureTextureRestored = false;
                }
            }
        }

        private void TickFade(float deltaTime)
        {
            if (_fadeDuration <= 0f)
            {
                _currentOpacity = _targetOpacity;
                CompleteFade();
                return;
            }

            _fadeElapsed += Mathf.Max(0f, deltaTime);
            float t = Mathf.Clamp01(_fadeElapsed / _fadeDuration);
            float curved = _fadeCurve != null ? _fadeCurve.Evaluate(t) : t;
            _currentOpacity = Mathf.Lerp(_fadeStartOpacity, _targetOpacity, curved);

            Refresh();
            MarkGraphicDirty();

            if (t >= 1f)
                CompleteFade();
        }

        private void BeginFade(float targetOpacity, float duration, bool deactivateAfter)
        {
            _fadeStartOpacity = _currentOpacity;
            _targetOpacity = Mathf.Clamp01(targetOpacity);
            _fadeDuration = Mathf.Max(0f, duration);
            _fadeElapsed = 0f;
            _deactivateAfterFadeOut = deactivateAfter;
            _isFading = _fadeDuration > 0f && !Mathf.Approximately(_fadeStartOpacity, _targetOpacity);

            if (!_isFading)
            {
                _currentOpacity = _targetOpacity;
                CompleteFade();
                return;
            }

            CaptureService.MarkDirty();
            MarkGraphicDirty();
        }

        private void CompleteFade()
        {
            _isFading = false;
            _currentOpacity = _targetOpacity;
            Refresh();
            MarkGraphicDirty();

            if (_deactivateAfterFadeOut && _currentOpacity <= 0.0001f)
            {
                _deactivateAfterFadeOut = false;
                gameObject.SetActive(false);
            }
        }

        private void MarkGraphicDirty()
        {
            if (_graphic != null)
                _graphic.SetMaterialDirty();
        }

        private void LogInfo(string message)
        {
            if (!_enableDebugLogs)
                return;

            Debug.Log($"[UIBackgroundBlur:{name}] {message}", this);
        }

        private void LogWarn(string message)
        {
            if (!_enableDebugLogs)
                return;

            Debug.LogWarning($"[UIBackgroundBlur:{name}] {message}", this);
        }

        private float GetDeltaTime()
        {
            if (Application.isPlaying)
                return Time.unscaledDeltaTime;

#if UNITY_EDITOR
            double now = EditorApplication.timeSinceStartup;
            if (_lastEditorTime <= 0.0)
            {
                _lastEditorTime = now;
                return 0f;
            }

            float dt = (float)(now - _lastEditorTime);
            _lastEditorTime = now;
            return Mathf.Clamp(dt, 0f, 0.1f);
#else
            return 0f;
#endif
        }

        private void ReleaseMaterial()
        {
            if (_runtimeMaterial == null)
                return;

            if (Application.isPlaying)
                Destroy(_runtimeMaterial);
            else
                DestroyImmediate(_runtimeMaterial);

            _runtimeMaterial = null;
        }

        private static class CaptureService
        {
            private struct CanvasState
            {
                public Canvas Canvas;
                public RenderMode RenderMode;
                public Camera WorldCamera;
                public float PlaneDistance;
            }

            private struct GraphicState
            {
                public Graphic Graphic;
                public bool Enabled;
            }

            private static readonly HashSet<UIBackgroundBlur> Instances = new();
            private static readonly List<CanvasState> CanvasStates = new();
            private static readonly List<GraphicState> GraphicStates = new();

            private static Camera _captureCamera;
            private static RenderTexture _captureTexture;
            private static bool _isSubscribed;
            private static bool _isCapturing;
            private static bool _isDirty = true;
            private static bool _hadCaptureTexture;
            private static int _lastCaptureFrame = -1;
            private static int _lastCaptureWidth;
            private static int _lastCaptureHeight;
            private static int _lastCaptureDownsample = -1;
            private static bool _loggedNoSourceCamera;
            private static bool _loggedRenderRequestFallback;

            public static void Register(UIBackgroundBlur instance)
            {
                if (instance == null)
                    return;

                Instances.Add(instance);
                EnsureSubscribed();
                _isDirty = true;
            }

            public static void Unregister(UIBackgroundBlur instance)
            {
                if (instance == null)
                    return;

                Instances.Remove(instance);
                RemoveInvalidInstances();
                _isDirty = true;

                if (Instances.Count == 0)
                    Cleanup();
            }

            public static void MarkDirty()
            {
                _isDirty = true;
            }

            public static bool TryGetCaptureTexture(out Texture texture)
            {
                texture = _captureTexture;
                return _captureTexture != null;
            }

            private static void EnsureSubscribed()
            {
                if (_isSubscribed)
                    return;

                Canvas.preWillRenderCanvases += OnWillRenderCanvases;
                Canvas.willRenderCanvases += OnWillRenderCanvases;
                _isSubscribed = true;
            }

            private static void OnWillRenderCanvases()
            {
                if (_isCapturing)
                    return;

                RemoveInvalidInstances();
                if (Instances.Count == 0)
                {
                    Cleanup();
                    return;
                }

                bool hasAnimatedInstance = HasAnimatedInstances();
                if (!_isDirty && !hasAnimatedInstance)
                    return;

                if (_lastCaptureFrame == Time.frameCount && _lastCaptureFrame >= 0)
                    return;

                int screenWidth = Mathf.Max(1, Screen.width);
                int screenHeight = Mathf.Max(1, Screen.height);
                if (screenWidth <= 1 || screenHeight <= 1)
                    return;

                int downsample = ResolveCaptureDownsample();
                bool sizeChanged = screenWidth != _lastCaptureWidth || screenHeight != _lastCaptureHeight || downsample != _lastCaptureDownsample;

                Camera sourceCamera = ResolveSourceCamera();
                if (sourceCamera == null)
                {
                    if (!_loggedNoSourceCamera)
                    {
                        LogWarnForDebugInstances("No active source camera found. Capture will use internal fallback camera settings.");
                        _loggedNoSourceCamera = true;
                    }
                }
                else
                {
                    if (_loggedNoSourceCamera)
                    {
                        LogInfoForDebugInstances($"Source camera restored: {sourceCamera.name}.");
                        _loggedNoSourceCamera = false;
                    }
                }

                bool captureTextureChanged = EnsureCaptureResources(sourceCamera, screenWidth, screenHeight, downsample);
                if (_captureCamera == null || _captureTexture == null)
                    return;

                _isCapturing = true;
                try
                {
                    PrepareCanvases();
                    PrepareGraphics();
                    Canvas.ForceUpdateCanvases();
                    RenderCaptureCamera();
                    _lastCaptureFrame = Time.frameCount;
                    _isDirty = false;
                    _lastCaptureWidth = screenWidth;
                    _lastCaptureHeight = screenHeight;
                    _lastCaptureDownsample = downsample;

                    if (!_hadCaptureTexture || captureTextureChanged || sizeChanged || hasAnimatedInstance)
                        MarkAllGraphicsDirty();

                    _hadCaptureTexture = true;
                }
                finally
                {
                    RestoreGraphics();
                    RestoreCanvases();
                    Canvas.ForceUpdateCanvases();
                    _isCapturing = false;
                }
            }

            private static bool HasAnimatedInstances()
            {
                foreach (UIBackgroundBlur instance in Instances)
                {
                    if (instance != null && instance.NeedsContinuousCapture)
                        return true;
                }

                return false;
            }

            private static int ResolveCaptureDownsample()
            {
                int downsample = 8;

                foreach (UIBackgroundBlur instance in Instances)
                {
                    if (instance == null || !instance.IsReadyForCapture)
                        continue;

                    downsample = Mathf.Min(downsample, Mathf.Clamp(instance.CaptureDownsample, 1, 8));
                }

                return Mathf.Clamp(downsample, 1, 8);
            }

            private static bool EnsureCaptureResources(Camera sourceCamera, int screenWidth, int screenHeight, int downsample)
            {
                EnsureCaptureCamera();
                if (_captureCamera == null)
                    return false;

                ConfigureCaptureCamera(sourceCamera);

                int targetWidth = Mathf.Max(1, Mathf.CeilToInt(screenWidth / (float)downsample));
                int targetHeight = Mathf.Max(1, Mathf.CeilToInt(screenHeight / (float)downsample));
                if (_captureTexture != null && _captureTexture.width == targetWidth && _captureTexture.height == targetHeight && _captureTexture.depth > 0)
                    return false;

                ReleaseCaptureTexture();

                // Unity RenderGraph requests require depth-stencil on the destination RT.
                var descriptor = new RenderTextureDescriptor(targetWidth, targetHeight)
                {
                    graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm,
                    depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt,
                    msaaSamples = 1,
                    useMipMap = false,
                    autoGenerateMips = false,
                    sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear
                };

                _captureTexture = new RenderTexture(descriptor)
                {
                    name = "Moyva UI Background Blur Capture",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
                _captureTexture.Create();
                _captureCamera.targetTexture = _captureTexture;
                LogInfoForDebugInstances($"Created capture texture {targetWidth}x{targetHeight} (downsample={downsample}).");
                return true;
            }

            private static void EnsureCaptureCamera()
            {
                if (_captureCamera != null)
                    return;

                var gameObject = new GameObject("Moyva UI Background Blur Capture Camera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                _captureCamera = gameObject.AddComponent<Camera>();
                _captureCamera.enabled = false;
            }

            private static void ConfigureCaptureCamera(Camera sourceCamera)
            {
                if (sourceCamera != null)
                {
                    _captureCamera.CopyFrom(sourceCamera);
                    _captureCamera.transform.SetPositionAndRotation(sourceCamera.transform.position, sourceCamera.transform.rotation);
                    _captureCamera.cullingMask = sourceCamera.cullingMask | (1 << 5);
                }
                else
                {
                    _captureCamera.clearFlags = CameraClearFlags.SolidColor;
                    _captureCamera.backgroundColor = Color.clear;
                    _captureCamera.cullingMask = -1;
                    _captureCamera.orthographic = true;
                    _captureCamera.orthographicSize = 5f;
                    _captureCamera.nearClipPlane = 0.01f;
                    _captureCamera.farClipPlane = 1000f;
                }

                _captureCamera.enabled = false;
                _captureCamera.forceIntoRenderTexture = true;
                _captureCamera.allowMSAA = false;
                _captureCamera.allowHDR = false;
                _captureCamera.useOcclusionCulling = false;
                _captureCamera.cameraType = CameraType.Game;
                _captureCamera.targetTexture = _captureTexture;
            }

            private static void RenderCaptureCamera()
            {
                if (_captureCamera == null || _captureTexture == null)
                    return;

                _captureCamera.targetTexture = _captureTexture;

                var request = new RenderPipeline.StandardRequest
                {
                    destination = _captureTexture,
                    mipLevel = 0,
                    face = CubemapFace.Unknown,
                    slice = 0
                };

                if (RenderPipeline.SupportsRenderRequest(_captureCamera, request))
                {
                    RenderPipeline.SubmitRenderRequest(_captureCamera, request);
                    _loggedRenderRequestFallback = false;
                    return;
                }

                if (!_loggedRenderRequestFallback)
                {
                    LogWarnForDebugInstances("RenderRequest is not supported; using Camera.Render() fallback.");
                    _loggedRenderRequestFallback = true;
                }

                _captureCamera.Render();
            }

            private static Camera ResolveSourceCamera()
            {
                Camera mainCamera = Camera.main;
                if (IsValidSourceCamera(mainCamera))
                    return mainCamera;

                Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                Camera bestCamera = null;
                float bestDepth = float.NegativeInfinity;

                foreach (Camera camera in cameras)
                {
                    if (!IsValidSourceCamera(camera))
                        continue;

                    if (camera.depth > bestDepth)
                    {
                        bestDepth = camera.depth;
                        bestCamera = camera;
                    }
                }

                return bestCamera;
            }

            private static bool IsValidSourceCamera(Camera camera)
            {
                return camera != null
                    && camera != _captureCamera
                    && camera.isActiveAndEnabled
                    && camera.gameObject.scene.IsValid()
                    && camera.cameraType != CameraType.Preview
                    && camera.targetTexture == null;
            }

            private static void PrepareCanvases()
            {
                CanvasStates.Clear();
                Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

                foreach (Canvas canvas in canvases)
                {
                    if (canvas == null || !canvas.isRootCanvas || !canvas.enabled || !canvas.gameObject.activeInHierarchy || !canvas.gameObject.scene.IsValid())
                        continue;

                    CanvasStates.Add(new CanvasState
                    {
                        Canvas = canvas,
                        RenderMode = canvas.renderMode,
                        WorldCamera = canvas.worldCamera,
                        PlaneDistance = canvas.planeDistance
                    });

                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;

                    if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    {
                        canvas.worldCamera = _captureCamera;
                        if (canvas.planeDistance <= 0f)
                            canvas.planeDistance = 100f;
                    }
                }
            }

            private static void RestoreCanvases()
            {
                for (int i = CanvasStates.Count - 1; i >= 0; i--)
                {
                    CanvasState state = CanvasStates[i];
                    if (state.Canvas == null)
                        continue;

                    state.Canvas.renderMode = state.RenderMode;
                    state.Canvas.worldCamera = state.WorldCamera;
                    state.Canvas.planeDistance = state.PlaneDistance;
                }

                CanvasStates.Clear();
            }

            private static void PrepareGraphics()
            {
                GraphicStates.Clear();

                foreach (UIBackgroundBlur instance in Instances)
                {
                    if (instance == null || !instance.IsReadyForCapture)
                        continue;

                    Graphic graphic = instance.Graphic;
                    if (graphic == null || !graphic.enabled)
                        continue;

                    GraphicStates.Add(new GraphicState
                    {
                        Graphic = graphic,
                        Enabled = true
                    });

                    graphic.enabled = false;
                }
            }

            private static void RestoreGraphics()
            {
                for (int i = GraphicStates.Count - 1; i >= 0; i--)
                {
                    GraphicState state = GraphicStates[i];
                    if (state.Graphic != null)
                        state.Graphic.enabled = state.Enabled;
                }

                GraphicStates.Clear();
            }

            private static void RemoveInvalidInstances()
            {
                Instances.RemoveWhere(instance => instance == null || !instance.gameObject.scene.IsValid());
            }

            private static void MarkAllGraphicsDirty()
            {
                foreach (UIBackgroundBlur instance in Instances)
                {
                    if (instance != null)
                        instance.MarkGraphicDirty();
                }
            }

            private static void LogInfoForDebugInstances(string message)
            {
                foreach (UIBackgroundBlur instance in Instances)
                {
                    if (instance != null && instance.DebugLogsEnabled)
                    {
                        Debug.Log($"[UIBackgroundBlur:CaptureService] {message}", instance);
                        return;
                    }
                }
            }

            private static void LogWarnForDebugInstances(string message)
            {
                foreach (UIBackgroundBlur instance in Instances)
                {
                    if (instance != null && instance.DebugLogsEnabled)
                    {
                        Debug.LogWarning($"[UIBackgroundBlur:CaptureService] {message}", instance);
                        return;
                    }
                }
            }

            private static void Cleanup()
            {
                if (_isSubscribed)
                {
                    Canvas.preWillRenderCanvases -= OnWillRenderCanvases;
                    Canvas.willRenderCanvases -= OnWillRenderCanvases;
                    _isSubscribed = false;
                }

                ReleaseCaptureTexture();
                ReleaseCaptureCamera();
                CanvasStates.Clear();
                GraphicStates.Clear();
                _isCapturing = false;
                _isDirty = true;
                _lastCaptureFrame = -1;
                _lastCaptureWidth = 0;
                _lastCaptureHeight = 0;
                _lastCaptureDownsample = -1;
                _loggedNoSourceCamera = false;
                _loggedRenderRequestFallback = false;

                if (_hadCaptureTexture)
                    MarkAllGraphicsDirty();

                _hadCaptureTexture = false;
            }

            private static void ReleaseCaptureTexture()
            {
                if (_captureTexture == null)
                    return;

                LogInfoForDebugInstances("Releasing capture texture.");

                if (Application.isPlaying)
                    Object.Destroy(_captureTexture);
                else
                    Object.DestroyImmediate(_captureTexture);

                _captureTexture = null;
            }

            private static void ReleaseCaptureCamera()
            {
                if (_captureCamera == null)
                    return;

                LogInfoForDebugInstances("Releasing capture camera.");

                GameObject cameraObject = _captureCamera.gameObject;
                if (Application.isPlaying)
                    Object.Destroy(cameraObject);
                else
                    Object.DestroyImmediate(cameraObject);

                _captureCamera = null;
            }
        }
    }
}
