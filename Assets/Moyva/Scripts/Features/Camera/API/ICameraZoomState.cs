namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>
    /// Shared far-view ("high altitude") window over normalized zoom.
    /// Neutral camera-side definition: ICameraZoomState.FarViewWeight rises
    /// from 0 at <see cref="start"/> to 1 at <see cref="full"/>. Visual and
    /// audio systems read the same weight so transitions stay synchronized.
    /// </summary>
    [System.Serializable]
    public struct CameraFarViewSettings
    {
        [UnityEngine.Range(0f, 1f)] public float start;
        [UnityEngine.Range(0f, 1f)] public float full;
        [UnityEngine.Min(0.05f)] public float smoothing;
        [UnityEngine.Range(0.2f, 3f)] public float shape;

        public static CameraFarViewSettings CreateDefault()
        {
            return new CameraFarViewSettings
            {
                start = 0.30f,
                full = 0.85f,
                smoothing = 4f,
                shape = 1f,
            };
        }

        public CameraFarViewSettings Normalize()
        {
            float s = UnityEngine.Mathf.Clamp01(start);
            return new CameraFarViewSettings
            {
                start = s,
                full = UnityEngine.Mathf.Clamp(
                    UnityEngine.Mathf.Max(full, s + 0.02f), 0.02f, 1f),
                smoothing = UnityEngine.Mathf.Max(0.05f, smoothing),
                shape = UnityEngine.Mathf.Clamp(
                    shape <= 0f ? 1f : shape, 0.2f, 3f),
            };
        }
    }

    /// <summary>
    /// Canonical normalized camera-distance ("altitude") state.
    /// Single authority for how far the camera currently is from the world:
    /// far-view visuals and zoom-driven audio both consume this so their
    /// presentation stays synchronized. Implementations live in the Camera
    /// feature; consumers must not recompute normalization themselves.
    /// </summary>
    public interface ICameraZoomState
    {
        /// <summary>Raw camera zoom value (orthographicSize or fieldOfView).</summary>
        float CurrentZoom { get; }

        /// <summary>Configured minimum zoom.</summary>
        float MinZoom { get; }

        /// <summary>Configured maximum zoom.</summary>
        float MaxZoom { get; }

        /// <summary>0 = closest configured zoom, 1 = farthest. Unsmoothed.</summary>
        float NormalizedZoom { get; }

        /// <summary>
        /// NormalizedZoom with frame-rate independent smoothing applied.
        /// This is the value presentation systems should sample per frame.
        /// </summary>
        float SmoothedNormalizedZoom { get; }

        /// <summary>
        /// 0..1 high-altitude presentation weight derived from
        /// SmoothedNormalizedZoom through the shared far-view window
        /// (CameraSettingsSO.farView start/full/shape).
        /// </summary>
        float FarViewWeight { get; }

        /// <summary>True when the active camera renders in perspective.</summary>
        bool IsPerspective { get; }
    }

    /// <summary>
    /// Pure zoom math shared by the camera state, audio and far-view visuals.
    /// Kept static + allocation-free so every consumer resolves identical values.
    /// </summary>
    public static class CameraZoomMath
    {
        /// <summary>Normalize zoom over [min,max] to 0..1. Degenerate range → 0.</summary>
        public static float NormalizeZoom(float zoom, float minZoom, float maxZoom)
        {
            if (maxZoom <= minZoom)
                return 0f;

            return UnityEngine.Mathf.Clamp01(
                UnityEngine.Mathf.InverseLerp(minZoom, maxZoom, zoom));
        }

        /// <summary>
        /// Smooth 0..1 far-view weight: 0 at/below start, 1 at/above full,
        /// smoothstep-interpolated between. shape != 1 applies an extra exponent
        /// to bias the curve (&gt;1 = later rise, &lt;1 = earlier rise).
        /// </summary>
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

        /// <summary>Frame-rate independent exponential approach factor.</summary>
        public static float SmoothingFactor(float smoothing, float unscaledDeltaTime)
        {
            if (smoothing <= 0.0001f)
                return 1f;

            return 1f - UnityEngine.Mathf.Exp(
                -UnityEngine.Mathf.Max(0f, unscaledDeltaTime) * smoothing);
        }
    }
}
