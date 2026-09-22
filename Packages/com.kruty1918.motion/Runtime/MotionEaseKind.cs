using UnityEngine;

namespace Kruty1918.Motion
{
    /// <summary>Види easing-кривих для gameplay-рухів.</summary>
    public enum MotionEaseKind
    {
        /// <summary>Лінійна інтерполяція.</summary>
        Linear = 0,
        /// <summary>Плавний крок (smoothstep).</summary>
        SmoothStep = 1,
        /// <summary>Квадратичне прискорення.</summary>
        InQuad = 2,
        /// <summary>Квадратичне сповільнення.</summary>
        OutQuad = 3,
        /// <summary>Квадратичне прискорення-сповільнення.</summary>
        InOutQuad = 4,
        /// <summary>Кубічне прискорення.</summary>
        InCubic = 5,
        /// <summary>Кубічне сповільнення.</summary>
        OutCubic = 6,
        /// <summary>Кубічне прискорення-сповільнення.</summary>
        InOutCubic = 7,

        /// <summary>М'який overshoot-вихід.</summary>
        OutBackSoft = 8
    }

    /// <summary>Обчислювач easing-кривих для gameplay-рухів.</summary>
    public static class MotionEaseEvaluator
    {
        /// <summary>Overshoot coefficient for <see cref="MotionEaseKind.OutBackSoft"/> (≈ half of the standard back ease).</summary>
        private const float SoftBackOvershoot = 0.9f;

        /// <summary>Обчислює eased-значення t для вказаної кривої.</summary>
        public static float Evaluate(MotionEaseKind kind, float t)
        {
            t = Mathf.Clamp01(t);
            switch (kind)
            {
                case MotionEaseKind.SmoothStep:
                    return t * t * (3f - 2f * t);
                case MotionEaseKind.InQuad:
                    return t * t;
                case MotionEaseKind.OutQuad:
                    return 1f - (1f - t) * (1f - t);
                case MotionEaseKind.InOutQuad:
                    return t < 0.5f
                        ? 2f * t * t
                        : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
                case MotionEaseKind.InCubic:
                    return t * t * t;
                case MotionEaseKind.OutCubic:
                    return 1f - Mathf.Pow(1f - t, 3f);
                case MotionEaseKind.InOutCubic:
                    return t < 0.5f
                        ? 4f * t * t * t
                        : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
                case MotionEaseKind.OutBackSoft:
                {
                    float c = SoftBackOvershoot + 1f;
                    float u = t - 1f;
                    return 1f + c * u * u * u + SoftBackOvershoot * u * u;
                }
                case MotionEaseKind.Linear:
                default:
                    return t;
            }
        }
    }
}
