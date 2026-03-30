using Kruty1918.Moyva.Camera.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraZoom : ICameraZoom, IInitializable, ILateTickable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;

        private float _targetZoom;
        private float _currentVelocity; // Необхідно для Mathf.SmoothDamp

        private float _forceBlockTimer;
        private const float ForceBlockDuration = 1.5f; // Час затримки після форсованого зуму

        public CameraZoom(UnityEngine.Camera camera, CameraSettingsSO settings)
        {
            _camera = camera;
            _settings = settings;
        }

        public void Initialize()
        {
            // На старті синхронізуємо цільовий зум з поточним Field of View або Orthographic Size
            if (_camera.orthographic)
            {
                _targetZoom = _camera.orthographicSize;
            }
            else
            {
                _targetZoom = _camera.fieldOfView;
            }
        }

        public void ZoomCamera(float delta)
        {
            // Якщо діє блокування від форсованого зуму — ігноруємо інпут гравця
            if (_forceBlockTimer > 0f) return;

            // Віднімаємо дельту (щоб скрол вперед наближав, а назад — віддаляв)
            // і одразу обмежуємо в заданих межах SO
            _targetZoom = Mathf.Clamp(
                _targetZoom - delta * _settings.zoomSpeed * Time.deltaTime,
                _settings.minZoom,
                _settings.maxZoom
            );
        }

        public void ForceZoomCamera(float zoomLevel)
        {
            // Встановлюємо новий цільовий зум (теж обмежуємо про всяк випадок)
            _targetZoom = Mathf.Clamp(zoomLevel, _settings.minZoom, _settings.maxZoom);

            // Блокуємо ручне керування на заданий час
            _forceBlockTimer = ForceBlockDuration;
        }

        public void LateTick()
        {
            // Оновлюємо таймер блокування
            if (_forceBlockTimer > 0f)
            {
                _forceBlockTimer -= Time.deltaTime;
            }

            if (_camera.orthographic)
            {
                // Логіка для 2D (Orthographic)
                _camera.orthographicSize = Mathf.SmoothDamp(
                    _camera.orthographicSize,
                    _targetZoom,
                    ref _currentVelocity,
                    _settings.smoothTime
                );
            }
            else
            {
                // Логіка для 3D (Perspective)
                _camera.fieldOfView = Mathf.SmoothDamp(
                    _camera.fieldOfView,
                    _targetZoom,
                    ref _currentVelocity,
                    _settings.smoothTime
                );
            }
        }
    }
}