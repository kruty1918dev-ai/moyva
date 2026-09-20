using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>ICameraMovement — interface: I камери руху.</summary>
    public interface ICameraMovement
    {
        /// <summary>ручного керування запитаного.</summary>
        event System.Action ManualControlRequested;

        /// <summary>Переміщує камери.</summary>
        void MoveCamera(Vector3 direction);
        /// <summary>Переміщує камери клавіатури.</summary>
        void MoveCameraKeyboard(Vector2 direction, float unscaledDeltaTime);
        /// <summary>Переміщує камери негайного.</summary>
        void MoveCameraImmediate(Vector3 direction, float speedMultiplier);
        /// <summary>Обертає камери Around фокус точки.</summary>
        void RotateCameraAroundFocusPoint(float angleDegrees);
        /// <summary>Встановлює камери орбіти вводу.</summary>
        void SetCameraOrbitInput(float normalizedInput);
        /// <summary>Починає курсора орбіти.</summary>
        void BeginPointerOrbit();
        /// <summary>Обертає курсора орбіти.</summary>
        void RotatePointerOrbit(float horizontalScreenDelta);
        /// <summary>Завершує курсора орбіти.</summary>
        void EndPointerOrbit();
        /// <summary>Виконує ShiftCameraWorld.</summary>
        void ShiftCameraWorld(Vector3 worldDelta, bool immediate);
        /// <summary>Переміщує камери фокус  світу точки.</summary>
        void MoveCameraFocusToWorldPoint(Vector3 focusPoint, bool immediate);
        /// <summary>Встановлює камери відстані  Navigation площини.</summary>
        void SetCameraDistanceToNavigationPlane(float distance, bool immediate);
        /// <summary>Намагається екрана точки  Navigation площини.</summary>
        bool TryScreenPointToNavigationPlane(Vector2 screenPoint, out Vector3 worldPoint);
        /// <summary>Примусово задає переміщення камери  позицію.</summary>
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

    /// <summary>IGameplayCameraFocusService — interface: I Gameplay камери фокус сервісу.</summary>
    public interface IGameplayCameraFocusService
    {
        /// <summary>Фокусує сітки позицію.</summary>
        void FocusGridPosition(Vector2Int gridPosition, string targetId = null);

        /// <summary>Фокусує вибраного.</summary>
        void FocusSelected();
    }
}
