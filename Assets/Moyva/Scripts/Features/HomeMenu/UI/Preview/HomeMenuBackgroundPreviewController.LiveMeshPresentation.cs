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
        private void ConfigureLivePreviewCamera(Bounds worldBounds, MoyvaProjectSettingsSO projectSettings, int previewLayer)
        {
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
            _livePreviewCamera.targetTexture = null;
            _livePreviewCamera.allowHDR = false;
            _livePreviewCamera.allowMSAA = false;

            var fogOverride = cameraObject.AddComponent<LivePreviewCameraFogOverride>();
            fogOverride.DisableFog = projectSettings.HomeMenuPreviewDisableFog;

            Quaternion cameraRotation = Quaternion.Euler(projectSettings.ResolvePreviewCameraEuler());
            _livePreviewCamera.transform.rotation = cameraRotation;
            bool usePerspective = projectSettings.ResolveUsePerspectivePreviewCamera();
            _livePreviewCamera.orthographic = !usePerspective;

            float aspect = ResolvePreviewAspect();
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

        private float ResolvePreviewAspect()
        {
            return Screen.width / Mathf.Max(1f, Screen.height);
        }

        private void DestroyLiveMeshPreview()
        {
            if (_livePreviewRoot != null)
                Destroy(_livePreviewRoot);
            if (_livePreviewCamera != null)
                Destroy(_livePreviewCamera.gameObject);
            if (_livePreviewLight != null)
                Destroy(_livePreviewLight.gameObject);

            for (int i = 0; i < _livePreviewMeshes.Count; i++)
            {
                if (_livePreviewMeshes[i] != null)
                    Destroy(_livePreviewMeshes[i]);
            }

            _livePreviewMeshes.Clear();
            _livePreviewRoot = null;
            _livePreviewCamera = null;
            _livePreviewLight = null;
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
