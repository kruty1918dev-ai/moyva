namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>Спільний стан зуму камери: поточний, нормалізований, згладжений зум і вага far-view вікна для візуалів та аудіо.</summary>
    public interface ICameraZoomState
    {
        /// <summary>Поточний зум камери.</summary>
        float CurrentZoom { get; }

        /// <summary>Мінімальний зум.</summary>
        float MinZoom { get; }

        /// <summary>Максимальний зум.</summary>
        float MaxZoom { get; }

        /// <summary>Нормалізований зум у [0,1].</summary>
        float NormalizedZoom { get; }

        /// <summary>Згладжений нормалізований зум.</summary>
        float SmoothedNormalizedZoom { get; }

        /// <summary>Вага far-view вікна: 0 поза вікном, 1 — повна дальність.</summary>
        float FarViewWeight { get; }

        /// <summary>Чи є камера перспективною.</summary>
        bool IsPerspective { get; }
    }

    /// <summary>Математика нормалізації зуму та ваги far-view.</summary>
    public static class CameraZoomMath
    {
        /// <summary>Нормалізує зум у [0,1] між min і max.</summary>
        public static float NormalizeZoom(float zoom, float minZoom, float maxZoom)
        {
            if (maxZoom <= minZoom)
                return 0f;

            return UnityEngine.Mathf.Clamp01(
                UnityEngine.Mathf.InverseLerp(minZoom, maxZoom, zoom));
        }

        /// <summary>Обчислює вагу far-view за нормалізованим зумом і вікном.</summary>
        public static float EvaluateFarViewWeight(
            float normalizedZoom,
            float start,
            float full,
            float shape = 1f)
        {
            if (full <= start)
                return normalizedZoom >= full ? 1f : 0f;

            float t = UnityEngine.Mathf.Clamp01(
                (normalizedZoom - start) / (full - start));
            float s = t * t * (3f - 2f * t);
            if (UnityEngine.Mathf.Abs(shape - 1f) > 0.001f)
                s = UnityEngine.Mathf.Pow(s, UnityEngine.Mathf.Max(0.05f, shape));
            return UnityEngine.Mathf.Clamp01(s);
        }

        /// <summary>Обчислює коефіцієнт згладжування за кадр.</summary>
        public static float SmoothingFactor(float smoothing, float unscaledDeltaTime)
        {
            if (smoothing <= 0.0001f)
                return 1f;

            return 1f - UnityEngine.Mathf.Exp(
                -UnityEngine.Mathf.Max(0f, unscaledDeltaTime) * smoothing);
        }
    }
}
