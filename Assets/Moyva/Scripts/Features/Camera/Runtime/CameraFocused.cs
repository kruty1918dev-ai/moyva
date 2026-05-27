using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraFocused : ICameraFocused
    {
        private readonly ICameraMovement _cameraMovement;
        private readonly ICameraZoom _cameraZoom;
        private readonly CameraSettingsSO _settings;
        private readonly UnityEngine.Camera _camera;
        private readonly IGridProjection _gridProjection;

        public CameraFocused(
            ICameraMovement cameraMovement, 
            ICameraZoom cameraZoom, 
            CameraSettingsSO settings,
            UnityEngine.Camera camera,
            [InjectOptional] IGridProjection gridProjection = null)
        {
            _cameraMovement = cameraMovement;
            _cameraZoom = cameraZoom;
            _settings = settings;
            _camera = camera;
            _gridProjection = gridProjection;
        }

        public void Focus(Transform target)
        {
            if (target == null) return;

            // 1. Визначаємо цільову позицію. 
            // Оскільки це камера, ми зазвичай хочемо зберегти її поточну висоту (Z або Y),
            // але центрувати по X та Y відносно об'єкта.
            Vector3 targetPos = target.position;
            if (_gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ)
                targetPos.y = _camera != null ? _camera.transform.position.y : targetPos.y;
            else
                targetPos.z = _camera != null ? _camera.transform.position.z : _settings.defaultCameraZ;

            // 2. Викликаємо форсований рух
            _cameraMovement.ForceMoveCameraToPosition(targetPos);

            // 3. Викликаємо форсований зум. 
            // Можна фокусуватися на "середнє" значення між min та max, 
            // або додати спеціальне фокусне значення в налаштування.
            float focusZoom = (_settings.ResolveMinZoom() + _settings.ResolveMaxZoom()) * 0.5f;
            _cameraZoom.ForceZoomCamera(focusZoom);
            
            Debug.Log($"[CameraFocused] Focusing on {target.name} at {targetPos}");
        }
    }
}