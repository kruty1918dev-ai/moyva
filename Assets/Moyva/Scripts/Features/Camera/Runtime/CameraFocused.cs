using Kruty1918.Moyva.Camera.API;
using UnityEngine;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraFocused : ICameraFocused
    {
        private readonly ICameraMovement _cameraMovement;
        private readonly ICameraZoom _cameraZoom;
        private readonly CameraSettingsSO _settings;

        public CameraFocused(
            ICameraMovement cameraMovement, 
            ICameraZoom cameraZoom, 
            CameraSettingsSO settings)
        {
            _cameraMovement = cameraMovement;
            _cameraZoom = cameraZoom;
            _settings = settings;
        }

        public void Focus(Transform target)
        {
            if (target == null) return;

            // 1. Визначаємо цільову позицію. 
            // Оскільки це камера, ми зазвичай хочемо зберегти її поточну висоту (Z або Y),
            // але центрувати по X та Y відносно об'єкта.
            Vector3 targetPos = target.position;
            
            // Якщо у тебе 2D/Top-down, ми зберігаємо Z камери:
            targetPos.z = _settings.defaultCameraZ; // Бажано додати цей параметр у SO

            // 2. Викликаємо форсований рух
            _cameraMovement.ForceMoveCameraToPosition(targetPos);

            // 3. Викликаємо форсований зум. 
            // Можна фокусуватися на "середнє" значення між min та max, 
            // або додати спеціальне фокусне значення в налаштування.
            float focusZoom = (_settings.minZoom + _settings.maxZoom) * 0.5f;
            _cameraZoom.ForceZoomCamera(focusZoom);
            
            Debug.Log($"[CameraFocused] Focusing on {target.name} at {targetPos}");
        }
    }
}