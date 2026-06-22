using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class PlanarReflectionRenderer : MonoBehaviour
{
    [Header("References")]
    public Camera SourceCamera;
    public Material WaterMaterial;

    [Header("Reflection Background")]
    public bool TransparentBackground = true;
    public Color BackgroundColor = new Color(0f, 0f, 0f, 0f);

    [Header("Reflection Render Texture")]
    [Min(64)] public int TextureSize = 1024;
    public LayerMask ReflectionMask = ~0;
    public FilterMode TextureFilterMode = FilterMode.Bilinear;

    [Header("Water Plane")]
    public Transform WaterPlane;
    [Tooltip("Used only when Water Plane is empty.")]
    public float WaterHeight = 0f;
    [Tooltip("Small offset that prevents z-fighting/clipping artifacts at the water line.")]
    public float ClipPlaneOffset = 0.07f;
    public bool UseObliqueClipPlane = true;

    [Header("Shader Projection")]
    [Tooltip("Turn this on if the reflection texture appears vertically inverted.")]
    public bool VerticalFlip = false;

    [Header("Debug")]
    public bool RenderInEditMode = true;

    private Camera _reflectionCamera;
    private RenderTexture _reflectionTexture;
    private int _currentTextureSize;
    private bool _isRendering;

    private static readonly int WaterReflectionTextureId = Shader.PropertyToID("_WaterReflectionTexture");
    private static readonly int ReflectionVPId = Shader.PropertyToID("_ReflectionVP");
    private static readonly int ReflectionVerticalFlipId = Shader.PropertyToID("_ReflectionVerticalFlip");

    private void OnEnable()
    {
        TryFindMaterial();
        CreateResources();
    }

    private void OnDisable()
    {
        ReleaseResources();
    }

    private void OnValidate()
    {
        TextureSize = Mathf.Max(64, TextureSize);
        ClipPlaneOffset = Mathf.Max(0f, ClipPlaneOffset);
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying && !RenderInEditMode)
        {
            return;
        }

        Camera cam = SourceCamera != null ? SourceCamera : Camera.main;
        if (cam == null || WaterMaterial == null)
        {
            TryFindMaterial();
            return;
        }

        RenderReflection(cam);
    }

    private void TryFindMaterial()
    {
        if (WaterMaterial != null)
        {
            return;
        }

        Renderer waterRenderer = GetComponent<Renderer>();
        if (waterRenderer != null)
        {
            WaterMaterial = waterRenderer.sharedMaterial;
        }
    }

    private void CreateResources()
    {
        if (_reflectionTexture == null || _currentTextureSize != TextureSize)
        {
            if (_reflectionTexture != null)
            {
                DestroyResource(_reflectionTexture);
            }

            _currentTextureSize = TextureSize;
            _reflectionTexture = new RenderTexture(TextureSize, TextureSize, 24, RenderTextureFormat.ARGB32)
            {
                name = "Planar Reflection Texture",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = TextureFilterMode,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = false,
                autoGenerateMips = false
            };
            _reflectionTexture.Create();
        }

        if (_reflectionCamera == null)
        {
            GameObject cameraObject = new GameObject("Planar Reflection Camera");
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            _reflectionCamera = cameraObject.AddComponent<Camera>();
            _reflectionCamera.enabled = false;
        }
    }

    private void ReleaseResources()
    {
        if (_reflectionTexture != null)
        {
            DestroyResource(_reflectionTexture);
            _reflectionTexture = null;
        }

        if (_reflectionCamera != null)
        {
            DestroyResource(_reflectionCamera.gameObject);
            _reflectionCamera = null;
        }
    }

    private static void DestroyResource(Object obj)
    {
        if (obj == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(obj);
        }
        else
        {
            DestroyImmediate(obj);
        }
    }

    private void RenderReflection(Camera sourceCamera)
    {
        if (_isRendering)
        {
            return;
        }

        CreateResources();

        _isRendering = true;

        Vector3 planePosition;
        Vector3 planeNormal;

        if (WaterPlane != null)
        {
            planePosition = WaterPlane.position;
            planeNormal = WaterPlane.up.normalized;
        }
        else
        {
            planePosition = new Vector3(0f, WaterHeight, 0f);
            planeNormal = Vector3.up;
        }

        // Plane equation: ax + by + cz + d = 0.
        float d = -Vector3.Dot(planeNormal, planePosition) - ClipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, d);

        Matrix4x4 reflectionMatrix = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflectionMatrix, reflectionPlane);

        _reflectionCamera.CopyFrom(sourceCamera);
        if (TransparentBackground)
        {
            _reflectionCamera.clearFlags = CameraClearFlags.SolidColor;
            _reflectionCamera.backgroundColor = BackgroundColor;
        }
        _reflectionCamera.enabled = false;
        _reflectionCamera.targetTexture = _reflectionTexture;
        _reflectionCamera.cullingMask = ReflectionMask;
        _reflectionCamera.useOcclusionCulling = false;

        // Mirror the camera through the water plane.
        _reflectionCamera.worldToCameraMatrix = sourceCamera.worldToCameraMatrix * reflectionMatrix;

        // Keep the transform in sync for editor preview/debugging.
        Vector3 reflectedPosition = reflectionMatrix.MultiplyPoint(sourceCamera.transform.position);
        Vector3 reflectedForward = Vector3.Reflect(sourceCamera.transform.forward, planeNormal);
        Vector3 reflectedUp = Vector3.Reflect(sourceCamera.transform.up, planeNormal);
        _reflectionCamera.transform.SetPositionAndRotation(
            reflectedPosition,
            Quaternion.LookRotation(reflectedForward, reflectedUp)
        );

        _reflectionCamera.projectionMatrix = sourceCamera.projectionMatrix;

        if (UseObliqueClipPlane)
        {
            Vector4 clipPlane = CameraSpacePlane(_reflectionCamera, planePosition, planeNormal, 1.0f, ClipPlaneOffset);
            _reflectionCamera.projectionMatrix = _reflectionCamera.CalculateObliqueMatrix(clipPlane);
        }

        bool oldInvertCulling = GL.invertCulling;

        try
        {
            GL.invertCulling = !oldInvertCulling;
            _reflectionCamera.Render();
        }
        finally
        {
            GL.invertCulling = oldInvertCulling;
        }

        Matrix4x4 gpuProjection = GL.GetGPUProjectionMatrix(_reflectionCamera.projectionMatrix, true);
        Matrix4x4 reflectionVP = gpuProjection * _reflectionCamera.worldToCameraMatrix;

        WaterMaterial.SetTexture(WaterReflectionTextureId, _reflectionTexture);
        WaterMaterial.SetMatrix(ReflectionVPId, reflectionVP);
        WaterMaterial.SetFloat(ReflectionVerticalFlipId, VerticalFlip ? 1f : 0f);

        Shader.SetGlobalTexture(WaterReflectionTextureId, _reflectionTexture);
        Shader.SetGlobalMatrix(ReflectionVPId, reflectionVP);
        Shader.SetGlobalFloat(ReflectionVerticalFlipId, VerticalFlip ? 1f : 0f);

        _isRendering = false;
    }

    private static Vector4 CameraSpacePlane(Camera camera, Vector3 position, Vector3 normal, float sideSign, float clipPlaneOffset)
    {
        Vector3 offsetPosition = position + normal * clipPlaneOffset;
        Matrix4x4 worldToCamera = camera.worldToCameraMatrix;

        Vector3 cameraPosition = worldToCamera.MultiplyPoint(offsetPosition);
        Vector3 cameraNormal = worldToCamera.MultiplyVector(normal).normalized * sideSign;

        return new Vector4(
            cameraNormal.x,
            cameraNormal.y,
            cameraNormal.z,
            -Vector3.Dot(cameraPosition, cameraNormal)
        );
    }

    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMatrix, Vector4 plane)
    {
        reflectionMatrix.m00 = 1F - 2F * plane[0] * plane[0];
        reflectionMatrix.m01 = -2F * plane[0] * plane[1];
        reflectionMatrix.m02 = -2F * plane[0] * plane[2];
        reflectionMatrix.m03 = -2F * plane[3] * plane[0];

        reflectionMatrix.m10 = -2F * plane[1] * plane[0];
        reflectionMatrix.m11 = 1F - 2F * plane[1] * plane[1];
        reflectionMatrix.m12 = -2F * plane[1] * plane[2];
        reflectionMatrix.m13 = -2F * plane[3] * plane[1];

        reflectionMatrix.m20 = -2F * plane[2] * plane[0];
        reflectionMatrix.m21 = -2F * plane[2] * plane[1];
        reflectionMatrix.m22 = 1F - 2F * plane[2] * plane[2];
        reflectionMatrix.m23 = -2F * plane[3] * plane[2];

        reflectionMatrix.m30 = 0F;
        reflectionMatrix.m31 = 0F;
        reflectionMatrix.m32 = 0F;
        reflectionMatrix.m33 = 1F;
    }
}
