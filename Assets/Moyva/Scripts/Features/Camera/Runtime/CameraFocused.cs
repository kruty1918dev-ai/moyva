using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>CameraFocused — class: камери Focused.</summary>
    internal sealed class CameraFocused : ICameraFocused
    {
        private readonly ICameraFocusService _focusService;
        private readonly ICameraMovement _cameraMovement;
        private readonly ICameraZoom _cameraZoom;
        private readonly CameraSettingsSO _settings;
        private readonly UnityEngine.Camera _camera;
        private readonly IGridProjection _gridProjection;

        /// <summary>Виконує CameraFocused.</summary>
        public CameraFocused(
            [InjectOptional] ICameraFocusService focusService = null,
            [InjectOptional] ICameraMovement cameraMovement = null,
            [InjectOptional] ICameraZoom cameraZoom = null,
            [InjectOptional] CameraSettingsSO settings = null,
            [InjectOptional] UnityEngine.Camera camera = null,
            [InjectOptional] IGridProjection gridProjection = null)
        {
            _focusService = focusService;
            _cameraMovement = cameraMovement;
            _cameraZoom = cameraZoom;
            _settings = settings;
            _camera = camera;
            _gridProjection = gridProjection;
        }

        /// <summary>Фокусує Focus.</summary>
        public void Focus(Transform target)
        {
            if (target == null) return;

            if (_focusService != null)
            {
                _focusService.FocusObject(target.gameObject, new CameraFocusRequest { AllowZoomIn = true });
                return;
            }

            // Legacy fallback: keep the pre-overhaul forced behavior.
            Vector3 targetPos = target.position;
            if (_gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ)
                targetPos.y = _camera != null ? _camera.transform.position.y : targetPos.y;
            else
                targetPos.z = _camera != null ? _camera.transform.position.z : _settings.defaultCameraZ;

            _cameraMovement?.ForceMoveCameraToPosition(targetPos);

            if (_settings != null && _cameraZoom != null)
            {
                float focusZoom = (_settings.ResolveMinZoom() + _settings.ResolveMaxZoom()) * 0.5f;
                _cameraZoom.ForceZoomCamera(focusZoom);
            }
        }
    }
}
