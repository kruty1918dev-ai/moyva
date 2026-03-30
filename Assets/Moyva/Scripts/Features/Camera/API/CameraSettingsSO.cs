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
    }
}