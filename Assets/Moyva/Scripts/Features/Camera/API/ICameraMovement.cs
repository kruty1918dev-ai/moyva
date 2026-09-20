using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    public interface ICameraMovement
    {
        /// <summary>
        /// Raised when the player expresses navigation intent (pan, drag, orbit).
        /// Programmatic navigation (teleports, focus transitions) never raises it.
        /// Automatic camera motion subscribes to this to yield to the player.
        /// </summary>
        event System.Action ManualControlRequested;

        void MoveCamera(Vector3 direction);
        void MoveCameraKeyboard(Vector2 direction, float unscaledDeltaTime);
        void MoveCameraImmediate(Vector3 direction, float speedMultiplier);
        void RotateCameraAroundFocusPoint(float angleDegrees);
        void SetCameraOrbitInput(float normalizedInput);
        void BeginPointerOrbit();
        void RotatePointerOrbit(float horizontalScreenDelta);
        void EndPointerOrbit();
        void ShiftCameraWorld(Vector3 worldDelta, bool immediate);
        void MoveCameraFocusToWorldPoint(Vector3 focusPoint, bool immediate);
        void SetCameraDistanceToNavigationPlane(float distance, bool immediate);
        bool TryScreenPointToNavigationPlane(Vector2 screenPoint, out Vector3 worldPoint);
        void ForceMoveCameraToPosition(Vector3 position);

        /// <summary>
        /// Миттєво телепортує камеру в позицію без жодної плавності.
        /// </summary>
        void TeleportCamera(Vector3 position);

        /// <summary>
        /// Миттєво розміщує камеру так, щоб центр екрана дивився на задану точку світу.
        /// </summary>
        void TeleportCameraToFocusPoint(Vector3 focusPoint, float distance);
    }

    public interface IGameplayCameraFocusService
    {
        void FocusGridPosition(Vector2Int gridPosition, string targetId = null);

        /// <summary>
        /// Focuses the currently selected unit/building/map object, if any.
        /// Bound to the FocusSelected player action (default: F / right stick press).
        /// </summary>
        void FocusSelected();
    }
}
