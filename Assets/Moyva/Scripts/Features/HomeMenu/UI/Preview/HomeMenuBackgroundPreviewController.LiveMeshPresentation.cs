using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Clouds.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed partial class HomeMenuBackgroundPreviewController
    {
        private const int MaxLivePreviewTextureWidth = 1920;
        private const int MaxLivePreviewTextureHeight = 1080;

        private bool ConfigureLivePreviewCamera(
            Bounds worldBounds,
            MoyvaProjectSettingsSO projectSettings,
            int previewLayer,
            out string failureReason)
        {
            failureReason = string.Empty;
            if (_targetImage == null)
            {
                failureReason = "The target RawImage is missing.";
                return false;
            }

            var cameraObject = new GameObject("HomeMenuLiveMeshPreviewCamera")
            {
                hideFlags = HideFlags.DontSave
            };
            _livePreviewCamera = cameraObject.AddComponent<Camera>();
            _livePreviewCamera.enabled = true;
            _livePreviewCamera.clearFlags = CameraClearFlags.SolidColor;
            _livePreviewCamera.backgroundColor = projectSettings.HomeMenuPreviewBackgroundColor;
            _livePreviewCamera.cullingMask = 1 << previewLayer;
            _livePreviewCamera.depth = projectSettings.HomeMenuPreviewCameraDepth;
            _livePreviewCamera.rect = new Rect(0f, 0f, 1f, 1f);
            _livePreviewCamera.allowHDR = false;
            _livePreviewCamera.forceIntoRenderTexture = true;

            if (!EnsureLivePreviewRenderTexture(out failureReason, out _))
                return false;

            var fogOverride = cameraObject.AddComponent<LivePreviewCameraFogOverride>();
            fogOverride.DisableFog = projectSettings.HomeMenuPreviewDisableFog;

            _livePreviewWorldBounds = worldBounds;
            _livePreviewProjectSettings = projectSettings;
            _hasLivePreviewWorldBounds = true;
            ApplyLivePreviewCameraFraming(worldBounds, projectSettings);
            return true;
        }

        private void ApplyLivePreviewCameraFraming(Bounds worldBounds, MoyvaProjectSettingsSO projectSettings)
        {
            if (_livePreviewCamera == null || projectSettings == null)
                return;

            Quaternion cameraRotation = Quaternion.Euler(projectSettings.ResolvePreviewCameraEuler());
            _livePreviewCamera.transform.rotation = cameraRotation;
            bool usePerspective = projectSettings.ResolveUsePerspectivePreviewCamera();
            _livePreviewCamera.orthographic = !usePerspective;

            float aspect = _livePreviewRenderTextureSize.y > 0
                ? _livePreviewRenderTextureSize.x / (float)_livePreviewRenderTextureSize.y
                : Screen.width / Mathf.Max(1f, Screen.height);
            Vector3 viewExtents = ResolveViewExtents(worldBounds, cameraRotation);
            float padding = Mathf.Max(0.01f, projectSettings.HomeMenuPreviewCameraPadding);
            float distance;
            if (usePerspective)
            {
                float fieldOfView = projectSettings.ResolvePreviewPerspectiveFieldOfView();
                _livePreviewCamera.fieldOfView = fieldOfView;
                float halfHeight = Mathf.Max(viewExtents.y, viewExtents.x / Mathf.Max(0.01f, aspect)) * padding;
                distance = halfHeight / Mathf.Tan(Mathf.Max(0.01f, fieldOfView * Mathf.Deg2Rad * 0.5f)) + viewExtents.z;
            }
            else
            {
                _livePreviewCamera.orthographicSize = Mathf.Max(viewExtents.y, viewExtents.x / Mathf.Max(0.01f, aspect)) * padding;
                distance = worldBounds.size.magnitude * 1.75f + 2f;
            }

            distance = Mathf.Max(1f, distance);
            _livePreviewCamera.transform.position = worldBounds.center - _livePreviewCamera.transform.forward * distance;
            _livePreviewCamera.nearClipPlane = 0.01f;
            _livePreviewCamera.farClipPlane = Mathf.Max(distance + worldBounds.size.magnitude * 2f + 10f, 50f);
        }

        private void ConfigureLivePreviewLight(MoyvaProjectSettingsSO projectSettings, int previewLayer)
        {
            var lightObject = new GameObject("HomeMenuLiveMeshPreviewLight")
            {
                hideFlags = HideFlags.DontSave
            };
            _livePreviewLight = lightObject.AddComponent<Light>();
            _livePreviewLight.type = LightType.Directional;
            _livePreviewLight.intensity = projectSettings.ResolvePreviewLightIntensity();
            _livePreviewLight.color = projectSettings.Project3DLightColor;
            _livePreviewLight.cullingMask = 1 << previewLayer;
            _livePreviewLight.shadows = projectSettings.HomeMenuPreviewCastShadows ? LightShadows.Soft : LightShadows.None;
            _livePreviewLight.transform.rotation = Quaternion.Euler(projectSettings.PreviewLightEuler);
        }

        private bool EnsureLivePreviewRenderTexture(out string failureReason, out bool recreated)
        {
            failureReason = string.Empty;
            recreated = false;

            if (_livePreviewCamera == null)
            {
                failureReason = "The live preview camera is missing.";
                return false;
            }

            if (_targetImage == null)
            {
                failureReason = "The target RawImage is missing.";
                return false;
            }

            Vector2Int targetSize = ResolveLivePreviewTextureSize();
            int requestedMsaa = NormalizeMsaaSampleCount(QualitySettings.antiAliasing);
            var descriptor = new RenderTextureDescriptor(
                targetSize.x,
                targetSize.y,
                RenderTextureFormat.ARGB32,
                24)
            {
                dimension = TextureDimension.Tex2D,
                volumeDepth = 1,
                msaaSamples = requestedMsaa,
                useMipMap = false,
                autoGenerateMips = false,
                enableRandomWrite = false,
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear
            };

            descriptor.msaaSamples = Mathf.Max(1, SystemInfo.GetRenderTextureSupportedMSAASampleCount(descriptor));
            int supportedMsaa = descriptor.msaaSamples;
            if (_livePreviewRenderTexture != null
                && _livePreviewRenderTexture.IsCreated()
                && _livePreviewRenderTextureSize == targetSize
                && _livePreviewRenderTextureMsaa == supportedMsaa)
            {
                PresentLivePreviewTexture();
                return true;
            }

            RenderTexture replacement = null;
            try
            {
                replacement = new RenderTexture(descriptor)
                {
                    name = $"HomeMenuLivePreview_{targetSize.x}x{targetSize.y}",
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                replacement.Create();
            }
            catch (Exception exception)
            {
                ReleaseRenderTexture(replacement);
                failureReason = $"RenderTexture creation failed: {exception.Message}";
                return false;
            }

            if (!replacement.IsCreated())
            {
                ReleaseRenderTexture(replacement);
                failureReason = $"RenderTexture {targetSize.x}x{targetSize.y} could not be created.";
                return false;
            }

            RenderTexture previous = _livePreviewRenderTexture;
            _livePreviewRenderTexture = replacement;
            _livePreviewRenderTextureSize = targetSize;
            _livePreviewRenderTextureMsaa = supportedMsaa;
            PresentLivePreviewTexture();
            ReleaseRenderTexture(previous);
            recreated = true;
            return true;
        }

        private void PresentLivePreviewTexture()
        {
            if (_livePreviewCamera == null || _livePreviewRenderTexture == null || _targetImage == null)
                return;

            _livePreviewCamera.targetTexture = _livePreviewRenderTexture;
            _livePreviewCamera.aspect = _livePreviewRenderTextureSize.x
                / Mathf.Max(1f, _livePreviewRenderTextureSize.y);
            _livePreviewCamera.allowMSAA = _livePreviewRenderTextureMsaa > 1;
            _targetImage.texture = _livePreviewRenderTexture;
            _targetImage.color = Color.white;
            _targetImage.enabled = true;
            ApplyCoverUv();
        }

        private Vector2Int ResolveLivePreviewTextureSize()
        {
            float width = Screen.width;
            float height = Screen.height;

            if (_targetImage != null)
            {
                RectTransform targetRect = _targetImage.rectTransform;
                targetRect.GetWorldCorners(_targetWorldCorners);
                Canvas canvas = _targetImage.canvas;
                Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;

                Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                for (int i = 0; i < _targetWorldCorners.Length; i++)
                {
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, _targetWorldCorners[i]);
                    min = Vector2.Min(min, screenPoint);
                    max = Vector2.Max(max, screenPoint);
                }

                Vector2 pixelSize = max - min;
                if (pixelSize.x > 1f && pixelSize.y > 1f)
                {
                    width = pixelSize.x;
                    height = pixelSize.y;
                }
                else
                {
                    Rect rect = targetRect.rect;
                    if (rect.width > 1f && rect.height > 1f)
                    {
                        width = rect.width;
                        height = rect.height;
                    }
                }
            }

            return CalculateLivePreviewTextureSize(
                Mathf.Max(1, Mathf.RoundToInt(width)),
                Mathf.Max(1, Mathf.RoundToInt(height)));
        }

        internal static Vector2Int CalculateLivePreviewTextureSize(int width, int height)
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            float scale = Mathf.Min(
                1f,
                Mathf.Min(
                    MaxLivePreviewTextureWidth / (float)width,
                    MaxLivePreviewTextureHeight / (float)height));
            return new Vector2Int(
                Mathf.Max(1, Mathf.RoundToInt(width * scale)),
                Mathf.Max(1, Mathf.RoundToInt(height * scale)));
        }

        internal static int NormalizeMsaaSampleCount(int antiAliasing)
        {
            if (antiAliasing >= 8)
                return 8;
            if (antiAliasing >= 4)
                return 4;
            if (antiAliasing >= 2)
                return 2;
            return 1;
        }

        private void DestroyLiveMeshPreview()
        {
            DisposeLivePreviewRenderTexture();

            if (_livePreviewRoot != null)
                DestroyPreviewObject(_livePreviewRoot);
            if (_livePreviewCamera != null)
                DestroyPreviewObject(_livePreviewCamera.gameObject);
            if (_livePreviewLight != null)
                DestroyPreviewObject(_livePreviewLight.gameObject);

            for (int i = 0; i < _livePreviewMeshes.Count; i++)
            {
                if (_livePreviewMeshes[i] != null)
                    DestroyPreviewObject(_livePreviewMeshes[i]);
            }

            _livePreviewMeshes.Clear();
            _livePreviewRoot = null;
            _livePreviewCamera = null;
            _livePreviewLight = null;
            _livePreviewWorldBounds = default;
            _livePreviewProjectSettings = null;
            _hasLivePreviewWorldBounds = false;
        }

        private void DisposeLivePreviewRenderTexture()
        {
            if (_livePreviewCamera != null && ReferenceEquals(_livePreviewCamera.targetTexture, _livePreviewRenderTexture))
                _livePreviewCamera.targetTexture = null;
            if (_targetImage != null && ReferenceEquals(_targetImage.texture, _livePreviewRenderTexture))
                _targetImage.texture = null;

            ReleaseRenderTexture(_livePreviewRenderTexture);
            _livePreviewRenderTexture = null;
            _livePreviewRenderTextureSize = Vector2Int.zero;
            _livePreviewRenderTextureMsaa = 1;
        }

        private static void ReleaseRenderTexture(RenderTexture texture)
        {
            if (texture == null)
                return;

            if (texture.IsCreated())
                texture.Release();

            DestroyPreviewObject(texture);
        }

        private void FallBackFromLivePreview(string failureReason)
        {
            _livePreviewUnavailable = true;
            LogLivePreviewFallbackOnce(failureReason);
            DestroyLiveMeshPreview();
            RegeneratePreview();
        }

        private void LogLivePreviewFallbackOnce(string failureReason)
        {
            if (_loggedLivePreviewFallback)
                return;

            _loggedLivePreviewFallback = true;
            string reason = string.IsNullOrWhiteSpace(failureReason)
                ? "The live mesh preview could not be initialized."
                : failureReason;
            Debug.LogWarning(
                $"[HomeMenuBackgroundPreviewController] {reason} Falling back to the texture preview.",
                this);
        }

        private void SetTexturePreviewVisible(bool visible)
        {
            if (_targetImage != null)
                _targetImage.enabled = visible;
        }

        private static Vector3 ResolveViewExtents(Bounds bounds, Quaternion cameraRotation)
        {
            Quaternion worldToView = Quaternion.Inverse(cameraRotation);
            Vector3 extents = bounds.extents;
            float maxX = 0.01f;
            float maxY = 0.01f;
            float maxZ = 0.01f;

            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = new Vector3(extents.x * x, extents.y * y, extents.z * z);
                Vector3 view = worldToView * corner;
                maxX = Mathf.Max(maxX, Mathf.Abs(view.x));
                maxY = Mathf.Max(maxY, Mathf.Abs(view.y));
                maxZ = Mathf.Max(maxZ, Mathf.Abs(view.z));
            }

            return new Vector3(maxX, maxY, maxZ);
        }

        private static Bounds TransformBounds(Matrix4x4 matrix, Bounds source)
        {
            Vector3 center = matrix.MultiplyPoint3x4(source.center);
            Vector3 extents = source.extents;
            Vector3 axisX = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
            extents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
            return new Bounds(center, extents * 2f);
        }

        private static void EncapsulateBounds(ref Bounds bounds, ref bool hasBounds, Bounds addition)
        {
            if (!hasBounds)
            {
                bounds = addition;
                hasBounds = true;
                return;
            }

            bounds.Encapsulate(addition);
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x)
                && !float.IsNaN(value.y) && !float.IsInfinity(value.y)
                && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

    }
}
