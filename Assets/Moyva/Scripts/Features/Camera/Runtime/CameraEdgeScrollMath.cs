using UnityEngine;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal static class CameraEdgeScrollMath
    {
        public static Vector2 ResolveDirection(
            Vector2 pointerPosition,
            Vector2 screenSize,
            float marginPixels)
        {
            if (screenSize.x <= 0f || screenSize.y <= 0f)
                return Vector2.zero;

            if (pointerPosition.x < 0f
                || pointerPosition.y < 0f
                || pointerPosition.x > screenSize.x
                || pointerPosition.y > screenSize.y)
            {
                return Vector2.zero;
            }

            float margin = Mathf.Clamp(
                marginPixels,
                1f,
                Mathf.Min(screenSize.x, screenSize.y) * 0.25f);
            Vector2 direction = Vector2.zero;

            if (pointerPosition.x <= margin)
                direction.x = 1f;
            else if (pointerPosition.x >= screenSize.x - margin)
                direction.x = -1f;

            if (pointerPosition.y <= margin)
                direction.y = -1f;
            else if (pointerPosition.y >= screenSize.y - margin)
                direction.y = 1f;

            return direction.sqrMagnitude > 1f
                ? direction.normalized
                : direction;
        }
    }
}
