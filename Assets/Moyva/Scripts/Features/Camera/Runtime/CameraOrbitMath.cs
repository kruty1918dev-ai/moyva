using UnityEngine;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>
    /// Frame-rate-independent angular acceleration integration used by keyboard orbit.
    /// </summary>
    internal static class CameraOrbitMath
    {
        public static float Advance(
            float currentVelocity,
            float targetVelocity,
            float acceleration,
            float deltaTime,
            out float angleDelta)
        {
            angleDelta = 0f;
            float resolvedDeltaTime = Mathf.Max(0f, deltaTime);
            if (resolvedDeltaTime <= 0f)
                return currentVelocity;

            float resolvedAcceleration = Mathf.Max(0.0001f, acceleration);
            float velocityDelta = targetVelocity - currentVelocity;
            if (Mathf.Abs(velocityDelta) <= 0.0001f)
            {
                angleDelta = targetVelocity * resolvedDeltaTime;
                return targetVelocity;
            }

            float timeToTarget = Mathf.Abs(velocityDelta) / resolvedAcceleration;
            float acceleratingTime = Mathf.Min(resolvedDeltaTime, timeToTarget);
            float direction = Mathf.Sign(velocityDelta);
            float nextVelocity = currentVelocity
                + direction * resolvedAcceleration * acceleratingTime;

            // Integrate the acceleration section as a trapezoid, then the
            // remaining constant-velocity section. This avoids the systematic
            // 30/60/144 FPS difference caused by end-of-frame Euler integration.
            angleDelta = (currentVelocity + nextVelocity)
                * 0.5f
                * acceleratingTime;

            if (acceleratingTime < resolvedDeltaTime)
            {
                nextVelocity = targetVelocity;
                angleDelta += targetVelocity
                    * (resolvedDeltaTime - acceleratingTime);
            }

            return nextVelocity;
        }
    }
}
