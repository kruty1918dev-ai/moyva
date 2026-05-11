using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    public interface ICameraMovement
    {
        void MoveCamera(Vector3 direction);
        void MoveCameraImmediate(Vector3 direction, float speedMultiplier);
        void ShiftCameraWorld(Vector3 worldDelta, bool immediate);
        void ForceMoveCameraToPosition(Vector3 position);

        /// <summary>
        /// Миттєво телепортує камеру в позицію без жодної плавності.
        /// </summary>
        void TeleportCamera(Vector3 position);
    }
}
