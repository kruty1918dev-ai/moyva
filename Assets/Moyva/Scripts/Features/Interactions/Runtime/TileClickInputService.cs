using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class TileClickInputService : ITickable
    {
        private readonly SignalBus _signalBus;

        public TileClickInputService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Tick()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            Vector2 screenPos = mouse.position.ReadValue();
            var worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
            var tilePos = new Vector2Int(
                Mathf.RoundToInt(worldPos.x),
                Mathf.RoundToInt(worldPos.y));

            _signalBus.Fire(new TileClickedSignal { Position = tilePos });
        }
    }
}