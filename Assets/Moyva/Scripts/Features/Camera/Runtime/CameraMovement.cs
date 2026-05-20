using Kruty1918.Moyva.Camera.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraMovement : ICameraMovement, IInitializable, ILateTickable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;
        private readonly ICameraBoundsProvider _boundsProvider;

        private Vector3 _targetPosition;
        private Vector3 _currentVelocity; // Необхідно для Vector3.SmoothDamp

        private float _forceBlockTimer;
        private const float ForceBlockDuration = 1.5f; // Час затримки після форсованого руху (можна винести в SO)

        // Zenject автоматично підставить активну камеру та налаштування
        public CameraMovement(
            UnityEngine.Camera camera,
            CameraSettingsSO settings,
            [InjectOptional] ICameraBoundsProvider boundsProvider = null)
        {
            _camera = camera;
            _settings = settings;
            _boundsProvider = boundsProvider;
        }

        public void Initialize()
        {
            // На старті синхронізуємо цільову позицію з поточною, щоб камера не відлітала
            _targetPosition = _camera.transform.position;
            ClampTargetToBounds();
        }

        public void MoveCamera(Vector3 delta) // delta — це чисті пікселі з Action Map
            => ApplyScreenDelta(delta, _settings.ResolveMoveSpeed(), immediate: false);

        public void MoveCameraImmediate(Vector3 delta, float speedMultiplier)
            => ApplyScreenDelta(delta, speedMultiplier, immediate: true);

        public void ShiftCameraWorld(Vector3 worldDelta, bool immediate)
        {
            if (_forceBlockTimer > 0f) return;

            worldDelta.z = 0f;
            _targetPosition += worldDelta;
            _targetPosition.z = _settings.defaultCameraZ;
            ClampTargetToBounds();

            if (!immediate)
                return;

            _currentVelocity = Vector3.zero;
            _camera.transform.position = _targetPosition;
        }

        private void ApplyScreenDelta(Vector3 delta, float speedMultiplier, bool immediate)
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
            _targetPosition += moveDirection * speedMultiplier;

            // Тримаємо Z стабільним для 2D
            _targetPosition.z = _settings.defaultCameraZ;
            ClampTargetToBounds();

            if (!immediate)
                return;

            _currentVelocity = Vector3.zero;
            _camera.transform.position = _targetPosition;
        }

        public void ForceMoveCameraToPosition(Vector3 position)
        {
            // Встановлюємо нову ціль і блокуємо звичайний рух на заданий час
            _targetPosition = position;
            ClampTargetToBounds();
            _forceBlockTimer = ForceBlockDuration;
        }

        public void TeleportCamera(Vector3 position)
        {
            _targetPosition = position;
            ClampTargetToBounds();
            _currentVelocity = Vector3.zero;
            _camera.transform.position = _targetPosition;
        }

        public void LateTick()
        {
            // Зменшуємо таймер блокування
            if (_forceBlockTimer > 0f)
            {
                _forceBlockTimer -= Time.deltaTime;
            }

            // Тримаємо ціль усередині bounds на кожному кадрі, бо зум міг змінити
            // допустиму "напіввисоту" viewport.
            ClampTargetToBounds();

            // Плавно рухаємо камеру до _targetPosition
            _camera.transform.position = Vector3.SmoothDamp(
                _camera.transform.position,
                _targetPosition,
                ref _currentVelocity,
                _settings.ResolveSmoothTime()
            );
        }

        private void ClampTargetToBounds()
        {
            if (_boundsProvider == null || _camera == null)
                return;

            var bounds = _boundsProvider.GetWorldBounds();
            if (!bounds.HasValue)
                return;

            Vector2 overflow = _settings.ResolveBoundsOverflowWorldUnits();
            float minBoundX = bounds.MinX - overflow.x;
            float maxBoundX = bounds.MaxX + overflow.x;
            float minBoundY = bounds.MinY - overflow.y;
            float maxBoundY = bounds.MaxY + overflow.y;

            float halfH = _camera.orthographicSize;
            float halfW = halfH * _camera.aspect;
            float width = Mathf.Max(0.01f, maxBoundX - minBoundX);
            float height = Mathf.Max(0.01f, maxBoundY - minBoundY);

            float minX, maxX;
            if (width <= halfW * 2f)
            {
                // Якщо viewport ширший за bounds, єдина валідна позиція — центр.
                minX = maxX = (minBoundX + maxBoundX) * 0.5f;
            }
            else
            {
                minX = minBoundX + halfW;
                maxX = maxBoundX - halfW;
            }

            float minY, maxY;
            if (height <= halfH * 2f)
            {
                minY = maxY = (minBoundY + maxBoundY) * 0.5f;
            }
            else
            {
                minY = minBoundY + halfH;
                maxY = maxBoundY - halfH;
            }

            _targetPosition.x = Mathf.Clamp(_targetPosition.x, minX, maxX);
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, minY, maxY);
        }
    }
}