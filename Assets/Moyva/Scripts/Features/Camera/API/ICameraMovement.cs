using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    public interface ICameraMovement
    {
        void MoveCamera(Vector3 direction);
        void ForceMoveCameraToPosition(Vector3 position);
    }
}
