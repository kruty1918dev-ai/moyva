using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Kruty1918.Moyva.Visuals
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class PlanarWaterReflection : MonoBehaviour
    {
        private static readonly int WaterReflectionTextureId = Shader.PropertyToID("_WaterReflectionTexture");
        private static readonly int ReflectionVPId = Shader.PropertyToID("_ReflectionVP");
        private static readonly int ReflectionVerticalFlipId = Shader.PropertyToID("_ReflectionVerticalFlip");
        private static bool _isRenderingReflection;

        [Tooltip("Plane used for mirroring the camera. Leave empty to use this transform.")]
        public Transform waterPlane;

        [Tooltip("Layers rendered into the water reflection. The Water layer is always excluded at render time.")]
        public LayerMask reflectionMask = ~0;

        [Tooltip("Reflection texture size relative to the source camera size.")]
        [Range(0.1f, 1f)] public float textureScale = 0.5f;

        [Tooltip("Small offset for the reflection clip plane to avoid artifacts at the water surface.")]
        [Min(0f)] public float clipPlaneOffset = 0.07f;

        [Tooltip("When enabled, the reflection camera uses the source camera skybox/background.")]
        public bool reflectSkybox = true;

        [Tooltip("Turn this on if the reflection texture appears vertically inverted on the current graphics API.")]
        public bool verticalFlip;

        [Tooltip("Water materials that should receive the reflection texture. Materials on this renderer are also updated.")]
        public Material[] targetWaterMaterials;

        private Camera _reflectionCamera;
        private RenderTexture _reflectionTexture;
        private UniversalAdditionalCameraData _reflectionCameraData;
        private MaterialPropertyBlock _propertyBlock;
        private int _textureWidth;
        private int _textureHeight;
        private Matrix4x4 _lastReflectionVP = Matrix4x4.identity;

        private void OnEnable()
        {
            if (waterPlane == null)
                waterPlane = transform;

            RenderPipelineManager.beginCameraRendering += RenderReflection;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= RenderReflection;
            ReleaseResources();
        }

        private void OnValidate()
        {
            textureScale = Mathf.Clamp(textureScale, 0.1f, 1f);
            clipPlaneOffset = Mathf.Max(0f, clipPlaneOffset);

            if (waterPlane == null)
                waterPlane = transform;
        }

        private void RenderReflection(ScriptableRenderContext context, Camera sourceCamera)
        {
            if (!isActiveAndEnabled || _isRenderingReflection || sourceCamera == null)
                return;

            if (sourceCamera.cameraType == CameraType.Reflection || sourceCamera.cameraType == CameraType.Preview)
                return;

            Transform plane = waterPlane != null ? waterPlane : transform;
            EnsureResources(sourceCamera);
            if (_reflectionCamera == null || _reflectionTexture == null)
                return;

            _isRenderingReflection = true;
            bool previousInvertCulling = GL.invertCulling;

            try
            {
                CopyCameraSettings(sourceCamera, _reflectionCamera);

                Vector3 planePosition = plane.position;
                Vector3 planeNormal = plane.up;
                float planeDistance = -Vector3.Dot(planeNormal, planePosition) - clipPlaneOffset;
                var reflectionPlane = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, planeDistance);

                Matrix4x4 reflectionMatrix = Matrix4x4.zero;
                CalculateReflectionMatrix(ref reflectionMatrix, reflectionPlane);

                Vector3 reflectedPosition = reflectionMatrix.MultiplyPoint(sourceCamera.transform.position);
                Vector3 reflectedForward = reflectionMatrix.MultiplyVector(sourceCamera.transform.forward);
                Vector3 reflectedUp = reflectionMatrix.MultiplyVector(sourceCamera.transform.up);

                _reflectionCamera.transform.SetPositionAndRotation(
                    reflectedPosition,
                    Quaternion.LookRotation(reflectedForward, reflectedUp));

                _reflectionCamera.worldToCameraMatrix = sourceCamera.worldToCameraMatrix * reflectionMatrix;

                Vector4 clipPlane = CameraSpacePlane(_reflectionCamera, planePosition, planeNormal, 1f);
                _reflectionCamera.projectionMatrix = sourceCamera.CalculateObliqueMatrix(clipPlane);
                _reflectionCamera.cullingMask = GetReflectionCullingMask();
                _reflectionCamera.targetTexture = _reflectionTexture;

                Matrix4x4 gpuProjection = GL.GetGPUProjectionMatrix(_reflectionCamera.projectionMatrix, true);
                _lastReflectionVP = gpuProjection * _reflectionCamera.worldToCameraMatrix;
                ApplyReflectionTexture();

                GL.invertCulling = true;
#pragma warning disable CS0618
                UniversalRenderPipeline.RenderSingleCamera(context, _reflectionCamera);
#pragma warning restore CS0618
            }
            finally
            {
                GL.invertCulling = previousInvertCulling;
                _isRenderingReflection = false;
            }
        }

        private void EnsureResources(Camera sourceCamera)
        {
            int width = Mathf.Max(1, Mathf.RoundToInt(sourceCamera.pixelWidth * textureScale));
            int height = Mathf.Max(1, Mathf.RoundToInt(sourceCamera.pixelHeight * textureScale));

            if (_reflectionTexture == null || _textureWidth != width || _textureHeight != height)
            {
                ReleaseTexture();

                RenderTextureFormat format = sourceCamera.allowHDR
                    ? RenderTextureFormat.DefaultHDR
                    : RenderTextureFormat.Default;
                _reflectionTexture = new RenderTexture(width, height, 16, format)
                {
                    name = "_WaterReflectionTexture",
                    hideFlags = HideFlags.HideAndDontSave,
                    antiAliasing = 1,
                    useMipMap = false,
                    autoGenerateMips = false,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };

                _textureWidth = width;
                _textureHeight = height;
                ApplyReflectionTexture();
            }

            if (_reflectionCamera == null)
            {
                var cameraObject = new GameObject("Planar Water Reflection Camera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _reflectionCamera = cameraObject.AddComponent<Camera>();
                _reflectionCamera.enabled = false;
                _reflectionCameraData = _reflectionCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
            else if (_reflectionCameraData == null)
            {
                _reflectionCamera.TryGetComponent(out _reflectionCameraData);
            }
        }

        private void CopyCameraSettings(Camera source, Camera destination)
        {
            destination.CopyFrom(source);
            destination.enabled = false;
            destination.targetTexture = _reflectionTexture;
            destination.depth = source.depth - 1f;
            destination.useOcclusionCulling = false;
            destination.allowMSAA = false;

            if (_reflectionCameraData != null)
            {
                _reflectionCameraData.renderType = CameraRenderType.Base;
                _reflectionCameraData.renderPostProcessing = false;
                _reflectionCameraData.requiresDepthTexture = false;
                _reflectionCameraData.requiresColorTexture = false;
                _reflectionCameraData.antialiasing = AntialiasingMode.None;
            }

            if (!reflectSkybox)
            {
                destination.clearFlags = CameraClearFlags.SolidColor;
                destination.backgroundColor = Color.clear;
            }
        }

        private int GetReflectionCullingMask()
        {
            int mask = reflectionMask.value;
            int waterLayer = LayerMask.NameToLayer("Water");
            if (waterLayer >= 0)
                mask &= ~(1 << waterLayer);

            return mask;
        }

        private void ApplyReflectionTexture()
        {
            if (_reflectionTexture == null)
                return;

            Shader.SetGlobalTexture(WaterReflectionTextureId, _reflectionTexture);
            Shader.SetGlobalMatrix(ReflectionVPId, _lastReflectionVP);
            Shader.SetGlobalFloat(ReflectionVerticalFlipId, verticalFlip ? 1f : 0f);
            ApplyReflectionTextureToMaterials(targetWaterMaterials);

            if (TryGetComponent(out Renderer waterRenderer))
            {
                _propertyBlock ??= new MaterialPropertyBlock();
                waterRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetTexture(WaterReflectionTextureId, _reflectionTexture);
                _propertyBlock.SetMatrix(ReflectionVPId, _lastReflectionVP);
                _propertyBlock.SetFloat(ReflectionVerticalFlipId, verticalFlip ? 1f : 0f);
                waterRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private void ApplyReflectionTextureToMaterials(Material[] materials)
        {
            if (materials == null)
                return;

            foreach (Material material in materials)
            {
                if (material == null)
                    continue;

                if (material.HasProperty(WaterReflectionTextureId))
                    material.SetTexture(WaterReflectionTextureId, _reflectionTexture);
                if (material.HasProperty(ReflectionVPId))
                    material.SetMatrix(ReflectionVPId, _lastReflectionVP);
                if (material.HasProperty(ReflectionVerticalFlipId))
                    material.SetFloat(ReflectionVerticalFlipId, verticalFlip ? 1f : 0f);
            }
        }

        private void ReleaseResources()
        {
            ReleaseTexture();

            if (_reflectionCamera != null)
            {
                if (Application.isPlaying)
                    Destroy(_reflectionCamera.gameObject);
                else
                    DestroyImmediate(_reflectionCamera.gameObject);

                _reflectionCamera = null;
                _reflectionCameraData = null;
            }
        }

        private void ReleaseTexture()
        {
            if (_reflectionTexture == null)
                return;

            _reflectionTexture.Release();
            if (Application.isPlaying)
                Destroy(_reflectionTexture);
            else
                DestroyImmediate(_reflectionTexture);

            _reflectionTexture = null;
            _textureWidth = 0;
            _textureHeight = 0;
        }

        private Vector4 CameraSpacePlane(Camera camera, Vector3 position, Vector3 normal, float sideSign)
        {
            Vector3 offsetPosition = position + normal * clipPlaneOffset;
            Matrix4x4 worldToCamera = camera.worldToCameraMatrix;
            Vector3 cameraPosition = worldToCamera.MultiplyPoint(offsetPosition);
            Vector3 cameraNormal = worldToCamera.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(
                cameraNormal.x,
                cameraNormal.y,
                cameraNormal.z,
                -Vector3.Dot(cameraPosition, cameraNormal));
        }

        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMatrix, Vector4 plane)
        {
            reflectionMatrix.m00 = 1f - 2f * plane[0] * plane[0];
            reflectionMatrix.m01 = -2f * plane[0] * plane[1];
            reflectionMatrix.m02 = -2f * plane[0] * plane[2];
            reflectionMatrix.m03 = -2f * plane[3] * plane[0];

            reflectionMatrix.m10 = -2f * plane[1] * plane[0];
            reflectionMatrix.m11 = 1f - 2f * plane[1] * plane[1];
            reflectionMatrix.m12 = -2f * plane[1] * plane[2];
            reflectionMatrix.m13 = -2f * plane[3] * plane[1];

            reflectionMatrix.m20 = -2f * plane[2] * plane[0];
            reflectionMatrix.m21 = -2f * plane[2] * plane[1];
            reflectionMatrix.m22 = 1f - 2f * plane[2] * plane[2];
            reflectionMatrix.m23 = -2f * plane[3] * plane[2];

            reflectionMatrix.m30 = 0f;
            reflectionMatrix.m31 = 0f;
            reflectionMatrix.m32 = 0f;
            reflectionMatrix.m33 = 1f;
        }
    }
}
