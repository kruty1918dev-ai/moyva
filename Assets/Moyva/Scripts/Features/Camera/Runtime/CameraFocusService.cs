using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>
    /// Обробляє сигнал фокусування камери на будівлю.
    /// Переміщує камеру та закриває інформаційні панелі.
    /// </summary>
    internal sealed class CameraFocusService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly ICameraMovement _cameraMovement;

        [Inject]
        public CameraFocusService(SignalBus signalBus, ICameraMovement cameraMovement)
        {
            _signalBus = signalBus;
            _cameraMovement = cameraMovement;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<CameraFocusBuildingSignal>(OnCameraFocusBuilding);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<CameraFocusBuildingSignal>(OnCameraFocusBuilding);
        }

        private void OnCameraFocusBuilding(CameraFocusBuildingSignal signal)
        {
            try
            {
                // Закрити інформаційну панель замку
                _signalBus.Fire(new WorldInfoPanelClosedSignal());

                // Переміститися до будівлі (позиція світу)
                var worldPosition = new Vector3(signal.Position.x, 0f, signal.Position.y);
                _cameraMovement.ForceMoveCameraToPosition(worldPosition);

                Debug.Log($"[CameraFocusService] Камера переміщена на {signal.BuildingId} на позицію {signal.Position}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CameraFocusService] Помилка переміщення камери: {ex.Message}");
            }
        }
    }
}
