using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    public enum CameraRuntimePlatform
    {
        Desktop = 0,
        Mobile = 1,
    }

    [System.Serializable]
    public struct CameraControlProfile
    {
        [Min(0.01f)] public float moveSpeed;
        [Min(0.01f)] public float smoothTime;
        [Min(0.01f)] public float zoomSpeed;
        [Min(0.1f)] public float minZoom;
        [Min(0.2f)] public float maxZoom;

        [Min(0.01f)] public float touchMoveSpeed;
        [Min(0.01f)] public float touchPinchZoomSensitivity;
        [Min(0f)] public float touchDragDeadZonePixels;
        [Min(0f)] public float touchPinchDeadZonePixels;
        [Min(1f)] public float maxTouchDeltaPixels;
        public bool useImmediateTouchGestures;
        public bool keepPinchFocusUnderFingers;

        public CameraControlProfile Normalize()
        {
            float normalizedMinZoom = Mathf.Max(0.1f, minZoom);
            return new CameraControlProfile
            {
                moveSpeed = Mathf.Max(0.01f, moveSpeed),
                smoothTime = Mathf.Max(0.01f, smoothTime),
                zoomSpeed = Mathf.Max(0.01f, zoomSpeed),
                minZoom = normalizedMinZoom,
                maxZoom = Mathf.Max(normalizedMinZoom + 0.1f, maxZoom),
                touchMoveSpeed = Mathf.Max(0.01f, touchMoveSpeed),
                touchPinchZoomSensitivity = Mathf.Max(0.01f, touchPinchZoomSensitivity),
                touchDragDeadZonePixels = Mathf.Max(0f, touchDragDeadZonePixels),
                touchPinchDeadZonePixels = Mathf.Max(0f, touchPinchDeadZonePixels),
                maxTouchDeltaPixels = Mathf.Max(1f, maxTouchDeltaPixels),
                useImmediateTouchGestures = useImmediateTouchGestures,
                keepPinchFocusUnderFingers = keepPinchFocusUnderFingers,
            };
        }

        public static CameraControlProfile CreateDesktopDefaults()
        {
            return new CameraControlProfile
            {
                moveSpeed = 5f,
                smoothTime = 0.3f,
                zoomSpeed = 5f,
                minZoom = 2f,
                maxZoom = 10f,
                touchMoveSpeed = 1f,
                touchPinchZoomSensitivity = 1f,
                touchDragDeadZonePixels = 0.5f,
                touchPinchDeadZonePixels = 2f,
                maxTouchDeltaPixels = 96f,
                useImmediateTouchGestures = true,
                keepPinchFocusUnderFingers = true,
            };
        }

        public static CameraControlProfile CreateMobileDefaults()
        {
            var profile = CreateDesktopDefaults();
            profile.moveSpeed = 4f;
            profile.smoothTime = 0.22f;
            profile.zoomSpeed = 4f;
            profile.touchMoveSpeed = 1.1f;
            profile.touchPinchZoomSensitivity = 1.15f;
            profile.touchDragDeadZonePixels = 1f;
            profile.touchPinchDeadZonePixels = 2.5f;
            return profile;
        }
    }

    [CreateAssetMenu(fileName = "CameraSettings", menuName = "Moyva/Camera/CameraSettings")]
    public class CameraSettingsSO : ScriptableObject
    {
        [Header("Platform Profiles")]
        public CameraControlProfile desktopProfile = CameraControlProfile.CreateDesktopDefaults();
        public CameraControlProfile mobileProfile = CameraControlProfile.CreateMobileDefaults();

        [Header("World Bounds")]
        [Tooltip("How many tile units the camera viewport is allowed to go outside map bounds.")]
        public Vector2 boundsOverflowTiles = Vector2.zero;

        [Header("Shared")]
        public float defaultCameraZ = -10f;

        [Header("3D Project Adaptation")]
        public bool adaptToProject3DMode = true;
        public bool useOrthographicCameraIn3D = true;
        [Min(0.1f)] public float default3DCameraDistance = 35f;
        [Min(0.1f)] public float default3DOrthographicSize = 20f;
        [Range(1f, 179f)] public float default3DFieldOfView = 45f;
        public Vector3 orthographic3DEuler = new Vector3(90f, 0f, 0f);
        public Vector3 isometric3DEuler = new Vector3(60f, 45f, 0f);

        [Header("Shader / Mip Bias")]
        [Tooltip("Applies global automatic mip bias for zoom. Disable to avoid tile atlas artifacts/bleeding on zoom-out.")]
        public bool enableAutomaticMipBias = false;
        [Range(0f, 3f)] public float automaticMipBiasMax = 0.75f;

        [Header("Map Render Mask")]
        public bool mapRenderMaskEnabled = true;
        [Min(0.05f)] public float mapMaskRefreshSeconds = 0.5f;
        public LayerMask mapMaskLayers = ~0;
        public string mapMaskSortingLayerName = "Default";
        [Range(-32768, 32767)] public int mapMaskBackSortingOrder = -32768;
        [Range(-32768, 32767)] public int mapMaskFrontSortingOrder = 32767;
        public Vector2 manualMapMaskCenter = new Vector2(4.5f, 4.5f);
        public Vector2 manualMapMaskSize = new Vector2(10f, 10f);

        public CameraControlProfile ResolveActiveProfile()
        {
            return Application.isMobilePlatform
                ? mobileProfile.Normalize()
                : desktopProfile.Normalize();
        }

        public CameraControlProfile ResolveProfile(CameraRuntimePlatform platform)
        {
            return platform == CameraRuntimePlatform.Mobile
                ? mobileProfile.Normalize()
                : desktopProfile.Normalize();
        }

        public float ResolveMoveSpeed() => ResolveActiveProfile().moveSpeed;
        public float ResolveSmoothTime() => ResolveActiveProfile().smoothTime;
        public float ResolveZoomSpeed() => ResolveActiveProfile().zoomSpeed;
        public float ResolveMinZoom() => ResolveActiveProfile().minZoom;
        public float ResolveMaxZoom() => ResolveActiveProfile().maxZoom;
        public float ResolveTouchMoveSpeed() => ResolveActiveProfile().touchMoveSpeed;
        public float ResolveTouchPinchZoomSensitivity() => ResolveActiveProfile().touchPinchZoomSensitivity;
        public float ResolveTouchDragDeadZonePixels() => ResolveActiveProfile().touchDragDeadZonePixels;
        public float ResolveTouchPinchDeadZonePixels() => ResolveActiveProfile().touchPinchDeadZonePixels;
        public float ResolveMaxTouchDeltaPixels() => ResolveActiveProfile().maxTouchDeltaPixels;
        public bool ResolveUseImmediateTouchGestures() => ResolveActiveProfile().useImmediateTouchGestures;
        public bool ResolveKeepPinchFocusUnderFingers() => ResolveActiveProfile().keepPinchFocusUnderFingers;
        public bool ResolveAdaptToProject3DMode() => adaptToProject3DMode;
        public bool ResolveUseOrthographicCameraIn3D() => useOrthographicCameraIn3D;
        public float ResolveDefault3DCameraDistance() => Mathf.Max(0.1f, default3DCameraDistance);
        public float ResolveDefault3DOrthographicSize() => Mathf.Max(ResolveMinZoom(), default3DOrthographicSize);
        public float ResolveDefault3DFieldOfView() => Mathf.Clamp(default3DFieldOfView, 1f, 179f);
        public Vector2 ResolveBoundsOverflowWorldUnits() => new Vector2(
            Mathf.Max(0f, boundsOverflowTiles.x),
            Mathf.Max(0f, boundsOverflowTiles.y));
        public bool ResolveEnableAutomaticMipBias() => enableAutomaticMipBias;
        public float ResolveAutomaticMipBiasMax() => Mathf.Clamp(automaticMipBiasMax, 0f, 3f);

        private void OnValidate()
        {
            desktopProfile = desktopProfile.Normalize();
            mobileProfile = mobileProfile.Normalize();
            boundsOverflowTiles = new Vector2(
                Mathf.Max(0f, boundsOverflowTiles.x),
                Mathf.Max(0f, boundsOverflowTiles.y));
            default3DCameraDistance = Mathf.Max(0.1f, default3DCameraDistance);
            default3DOrthographicSize = Mathf.Max(ResolveMinZoom(), default3DOrthographicSize);
            default3DFieldOfView = Mathf.Clamp(default3DFieldOfView, 1f, 179f);
            automaticMipBiasMax = Mathf.Clamp(automaticMipBiasMax, 0f, 3f);
        }
    }
}