using UnityEngine;

namespace Kruty1918.Moyva.Animations.API
{
    /// <summary>
    /// Canonical easing vocabulary for gameplay presentation transitions.
    /// One shared set for units, construction and world feedback so the motion
    /// language stays coherent instead of ad-hoc tween calls per feature.
    /// </summary>
    public enum MotionEaseKind
    {
        Linear = 0,
        SmoothStep = 1,
        InQuad = 2,
        OutQuad = 3,
        InOutQuad = 4,
        InCubic = 5,
        OutCubic = 6,
        InOutCubic = 7,

        /// <summary>Weak overshoot only — Moyva must not feel like a casual UI game.</summary>
        OutBackSoft = 8
    }

    public static class MotionEaseEvaluator
    {
        /// <summary>Overshoot coefficient for <see cref="MotionEaseKind.OutBackSoft"/> (≈ half of the standard back ease).</summary>
        private const float SoftBackOvershoot = 0.9f;

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
