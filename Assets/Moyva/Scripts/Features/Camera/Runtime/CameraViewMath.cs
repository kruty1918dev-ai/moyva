using Kruty1918.Moyva.Camera.API;
using UnityEngine;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>CameraViewMath — class: камери виду Math.</summary>
    internal static class CameraViewMath
    {
        /// <summary>Плавно змінює у Out Cubic.</summary>
        public static float EaseInOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
        }

        /// <summary>Виконує SmoothingFactor.</summary>
        public static float SmoothingFactor(float speed, float unscaledDeltaTime)
            => 1f - Mathf.Exp(-Mathf.Max(0f, speed) * Mathf.Max(0f, unscaledDeltaTime));

        /// <summary>Нормалізує зум.</summary>
        public static float NormalizeZoom(float zoom, float minZoom, float maxZoom)
            => maxZoom > minZoom ? Mathf.Clamp01(Mathf.InverseLerp(minZoom, maxZoom, zoom)) : 0f;

        /// <summary>Повертає фокус тривалості.</summary>
        public static float ResolveFocusDuration(float distance, CameraFocusSettings settings)
        {
            float t = settings.distanceForMaxDuration > 0.01f
                ? Mathf.Clamp01(distance / settings.distanceForMaxDuration)
                : 1f;
            return Mathf.Lerp(settings.minDuration, settings.maxDuration, t);
        }

        /// <summary>Повертає виду Half Extents.</summary>
        public static void ResolveViewHalfExtents(
            Bounds worldBounds,
            Vector3 cameraRight,
            Vector3 cameraUp,
            out float halfWidth,
            out float halfHeight)
        {
            Vector3 e = worldBounds.extents;
            halfWidth = e.x * Mathf.Abs(cameraRight.x) + e.y * Mathf.Abs(cameraRight.y) + e.z * Mathf.Abs(cameraRight.z);
            halfHeight = e.x * Mathf.Abs(cameraUp.x) + e.y * Mathf.Abs(cameraUp.y) + e.z * Mathf.Abs(cameraUp.z);
        }

        /// <summary>Повертає ортографічного розміру For межі.</summary>
        public static float ResolveOrthographicSizeForBounds(
            float halfWidth, float halfHeight, float aspect, float padding)
        {
            float safeAspect = Mathf.Max(0.01f, aspect);
            return Mathf.Max(halfHeight, halfWidth / safeAspect) * Mathf.Max(1f, padding);
        }

        /// <summary>Повертає перспективи FOV For межі.</summary>
        public static float ResolvePerspectiveFovForBounds(
            float halfWidth, float halfHeight, float aspect, float distance, float padding)
        {
            float safeAspect = Mathf.Max(0.01f, aspect);
            float required = Mathf.Max(halfHeight, halfWidth / safeAspect) * Mathf.Max(1f, padding);
            return 2f * Mathf.Atan(required / Mathf.Max(0.1f, distance)) * Mathf.Rad2Deg;
        }

        /// <summary>Виконує ImpulseEnvelope.</summary>
        public static float ImpulseEnvelope(float elapsed, float duration)
        {
            if (duration <= 0.0001f)
                return 0f;
            float t = Mathf.Clamp01(elapsed / duration);
            const float attack = 0.12f;
            if (t < attack)
                return t / attack;
            return Mathf.Exp(-4.5f * (t - attack) / (1f - attack));
        }

        /// <summary>Виконує ImpulseKickEnvelope.</summary>
        public static float ImpulseKickEnvelope(float elapsed, float duration)
        {
            if (duration <= 0.0001f)
                return 0f;
            return Mathf.Exp(-6f * Mathf.Clamp01(elapsed / duration));
        }

        /// <summary>Обчислює відстані затухання.</summary>
        public static float EvaluateDistanceFalloff(float distance, float radius)
        {
            if (radius <= 0.01f)
                return 1f;
            float t = Mathf.Clamp01(distance / radius);
            if (t <= 0.3f)
                return 1f;
            float u = (t - 0.3f) / 0.7f;
            return 1f - u * u * (3f - 2f * u);
        }
    }
}
