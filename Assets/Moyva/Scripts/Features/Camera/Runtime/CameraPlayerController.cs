using System;
using Kruty1918.Moyva.Camera.API;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraPlayerController : ITickable, IDisposable
    {
        private readonly ICameraMovement _cameraMovement;
        private readonly ICameraZoom _cameraZoom;

        private readonly InputAction _moveAction;
        private readonly InputAction _zoomAction;

        public CameraPlayerController(
            ICameraMovement cameraMovement,
            ICameraZoom cameraZoom,
            InputActionAsset inputAsset)
        {
            _cameraMovement = cameraMovement;
            _cameraZoom = cameraZoom;

            var map = inputAsset.FindActionMap("Player");
            _moveAction = map.FindAction("Move");
            _zoomAction = map.FindAction("Zoom");

            map.Enable();
        }

        public void Tick()
        {
            // Отримуємо сирі пікселі дельти (Mouse Delta або Touch Delta)
            Vector2 moveDelta = _moveAction.ReadValue<Vector2>();

            if (moveDelta.sqrMagnitude > 0.001f)
            {
                // Просто передаємо Vector2. Сервіс сам знає, як його перетворити для 2D.
                _cameraMovement.MoveCamera(moveDelta);
            }

            // Зум (Scroll або Pinch)
            float zoomDelta = _zoomAction.ReadValue<float>();
            if (Mathf.Abs(zoomDelta) > 0.001f)
            {
                _cameraZoom.ZoomCamera(zoomDelta);
            }
        }

        public void Dispose()
        {
            _moveAction?.Disable();
            _zoomAction?.Disable();
        }
    }
}