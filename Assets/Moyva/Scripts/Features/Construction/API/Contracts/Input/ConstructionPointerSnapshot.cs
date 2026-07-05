using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public readonly struct ConstructionPointerSnapshot
    {
        public static readonly ConstructionPointerSnapshot None = new ConstructionPointerSnapshot(
            hasPointer: false,
            wasPressedThisFrame: false,
            wasReleasedThisFrame: false,
            isPressed: false,
            position: Vector2.zero,
            pointerId: -1,
            activePointerCount: 0,
            deviceKind: ConstructionPointerDeviceKind.None,
            selectOnRelease: false);

        public ConstructionPointerSnapshot(
            bool hasPointer,
            bool wasPressedThisFrame,
            bool wasReleasedThisFrame,
            bool isPressed,
            Vector2 position,
            int pointerId,
            int activePointerCount,
            ConstructionPointerDeviceKind deviceKind,
            bool selectOnRelease)
        {
            HasPointer = hasPointer;
            WasPressedThisFrame = wasPressedThisFrame;
            WasReleasedThisFrame = wasReleasedThisFrame;
            IsPressed = isPressed;
            Position = position;
            PointerId = pointerId;
            ActivePointerCount = activePointerCount;
            DeviceKind = deviceKind;
            SelectOnRelease = selectOnRelease;
        }

        public bool HasPointer { get; }
        public bool WasPressedThisFrame { get; }
        public bool WasReleasedThisFrame { get; }
        public bool IsPressed { get; }
        public Vector2 Position { get; }
        public int PointerId { get; }
        public int ActivePointerCount { get; }
        public ConstructionPointerDeviceKind DeviceKind { get; }
        public bool SelectOnRelease { get; }

        public bool IsTouch => DeviceKind == ConstructionPointerDeviceKind.Touch;
    }
}
