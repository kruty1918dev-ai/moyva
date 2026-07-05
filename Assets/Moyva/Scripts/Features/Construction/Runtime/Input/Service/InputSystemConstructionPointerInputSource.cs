using Kruty1918.Moyva.Construction.API;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class InputSystemConstructionPointerInputSource : IConstructionPointerInputSource
    {
        public ConstructionPointerSnapshot ReadPointerSnapshot()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                TouchControl activeTouch = null;
                TouchControl pressedTouch = null;
                TouchControl releasedTouch = null;
                int activeTouchCount = 0;

                var touches = touchscreen.touches;
                for (int touchIndex = 0; touchIndex < touches.Count; touchIndex++)
                {
                    TouchControl touch = touches[touchIndex];
                    if (touch.press.isPressed)
                    {
                        activeTouchCount++;
                        activeTouch ??= touch;
                    }

                    if (pressedTouch == null && touch.press.wasPressedThisFrame)
                        pressedTouch = touch;

                    if (releasedTouch == null && touch.press.wasReleasedThisFrame)
                        releasedTouch = touch;
                }

                TouchControl selectedTouch = activeTouch ?? releasedTouch ?? pressedTouch;
                if (selectedTouch != null)
                {
                    return new ConstructionPointerSnapshot(
                        hasPointer: true,
                        wasPressedThisFrame: selectedTouch.press.wasPressedThisFrame,
                        wasReleasedThisFrame: selectedTouch.press.wasReleasedThisFrame,
                        isPressed: selectedTouch.press.isPressed,
                        position: selectedTouch.position.ReadValue(),
                        pointerId: selectedTouch.touchId.ReadValue(),
                        activePointerCount: activeTouchCount,
                        deviceKind: ConstructionPointerDeviceKind.Touch,
                        selectOnRelease: true);
                }
            }

            var mouse = Mouse.current;
            if (mouse == null)
                return ConstructionPointerSnapshot.None;

            return new ConstructionPointerSnapshot(
                hasPointer: true,
                wasPressedThisFrame: mouse.leftButton.wasPressedThisFrame,
                wasReleasedThisFrame: mouse.leftButton.wasReleasedThisFrame,
                isPressed: mouse.leftButton.isPressed,
                position: mouse.position.ReadValue(),
                pointerId: -1,
                activePointerCount: 0,
                deviceKind: ConstructionPointerDeviceKind.Mouse,
                selectOnRelease: false);
        }
    }
}
