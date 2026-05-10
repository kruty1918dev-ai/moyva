using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "Moyva/Camera/CameraSettings")]
    public class CameraSettingsSO : ScriptableObject
    {
        [Header("Camera Settings")]
        public float moveSpeed = 5f;
        public float smoothTime = 0.3f;
        public float zoomSpeed = 5f;
        public float minZoom = 2f;
        public float maxZoom = 10f;
        public float defaultCameraZ = -10f;

        [Header("Mobile Touch")]
        [Min(0.01f)] public float touchMoveSpeed = 1f;
        [Min(0.01f)] public float touchPinchZoomSensitivity = 1f;
        [Min(0f)] public float touchDragDeadZonePixels = 0.5f;
        [Min(0f)] public float touchPinchDeadZonePixels = 2f;
        [Min(1f)] public float maxTouchDeltaPixels = 96f;
        public bool useImmediateTouchGestures = true;
        public bool keepPinchFocusUnderFingers = true;

        [Header("Map Render Mask")]
        public bool mapRenderMaskEnabled = true;
        [Min(0.05f)] public float mapMaskRefreshSeconds = 0.5f;
        public LayerMask mapMaskLayers = ~0;
        public string mapMaskSortingLayerName = "Default";
        [Range(-32768, 32767)] public int mapMaskBackSortingOrder = -32768;
        [Range(-32768, 32767)] public int mapMaskFrontSortingOrder = 32767;
        public Vector2 manualMapMaskCenter = new Vector2(4.5f, 4.5f);
        public Vector2 manualMapMaskSize = new Vector2(10f, 10f);
    }
}