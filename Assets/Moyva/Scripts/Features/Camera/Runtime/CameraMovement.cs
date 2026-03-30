using Kruty1918.Moyva.Camera.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    public class CameraMovement : ICameraMovement, IInitializable, ILateTickable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;

        private Vector3 _targetPosition;
        private Vector3 _currentVelocity; // Необхідно для Vector3.SmoothDamp

        private float _forceBlockTimer;
        private const float ForceBlockDuration = 1.5f; // Час затримки після форсованого руху (можна винести в SO)

        // Zenject автоматично підставить активну камеру та налаштування
        public CameraMovement(UnityEngine.Camera camera, CameraSettingsSO settings)
        {
            _camera = camera;
            _settings = settings;
        }

        public void Initialize()
        {
            // На старті синхронізуємо цільову позицію з поточною, щоб камера не відлітала
            _targetPosition = _camera.transform.position;
        }

        public void MoveCamera(Vector3 delta) // delta — це чисті пікселі з Action Map
        {
            if (_forceBlockTimer > 0f) return;

            // 1. Отримуємо коефіцієнт: скільки Unit-ів світу в одному пікселі
            // (OrthoSize * 2) — це вся висота екрана в юнітах Unity.
            // Ділимо на Screen.height, щоб отримати "вартість" одного пікселя.
            float unitsPerPixel = (_camera.orthographicSize * 2f) / Screen.height;

            // 2. Розраховуємо фінальний вектор руху
            // Інвертуємо (-), щоб мапа "йшла за пальцем"
            Vector3 moveDirection = new Vector3(
                -delta.x * unitsPerPixel,
                -delta.y * unitsPerPixel,
                0
            );

            // 3. Додаємо до цілі. 
            // ТУТ ВАЖЛИВО: moveSpeed у налаштуваннях тепер має бути в районі 1.0.
            // Якщо поставиш 1.0 — мапа буде ідеально "приклеєна" до пальця/курсора.
            _targetPosition += moveDirection * _settings.moveSpeed;

            // Тримаємо Z стабільним для 2D
            _targetPosition.z = _settings.defaultCameraZ;
        }

        public void ForceMoveCameraToPosition(Vector3 position)
        {
            // Встановлюємо нову ціль і блокуємо звичайний рух на заданий час
            _targetPosition = position;
            _forceBlockTimer = ForceBlockDuration;

            // Якщо ти хочеш, щоб камера переміщалася МИТТЄВО (без плавності), розкоментуй цей рядок:
            // _camera.transform.position = position; 
        }

        public void LateTick()
        {
            // Зменшуємо таймер блокування
            if (_forceBlockTimer > 0f)
            {
                _forceBlockTimer -= Time.deltaTime;
            }

            // Плавно рухаємо камеру до _targetPosition
            _camera.transform.position = Vector3.SmoothDamp(
                _camera.transform.position,
                _targetPosition,
                ref _currentVelocity,
                _settings.smoothTime
            );
        }
    }
}