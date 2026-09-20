using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>Named impulse feel presets; use instead of raw amplitudes.</summary>
    public enum CameraImpulseProfile
    {
        /// <summary>Subtle accent, e.g. a small impact near the player.</summary>
        Tiny,
        /// <summary>Noticeable jolt, e.g. standard combat impact or capture.</summary>
        Light,
        /// <summary>Strong shake, e.g. demolition or siege hit.</summary>
        Heavy,
        /// <summary>Global-scale event accent (wonder, realm-defining moment).</summary>
        Strategic,
    }

    /// <summary>Request importance; lowest-priority impulses are evicted first.</summary>
    public enum CameraImpulsePriority
    {
        Low,
        Medium,
        High,
        Critical,
    }

    /// <summary>Semantic source category for documentation and profiling.</summary>
    public enum CameraImpulseCategory
    {
        Generic,
        CombatImpact,
        SiegeImpact,
        Demolition,
        ConstructionSettle,
        Capture,
        GameEvent,
    }

    /// <summary>
    /// One-shot camera impulse request. Amplitudes are pre-falloff world
    /// values; the service applies distance falloff, zoom falloff, player
    /// settings scaling and absolute clamps.
    /// </summary>
    public struct CameraImpulseRequest
    {
        /// <summary>Peak positional offset in world units.</summary>
        public float PositionAmplitude;

        /// <summary>Peak rotational offset in degrees.</summary>
        public float RotationAmplitude;

        /// <summary>Total lifetime in seconds (attack + decay).</summary>
        public float Duration;

        /// <summary>Noise frequency in Hz for the positional tremor.</summary>
        public float Frequency;

        /// <summary>World position for distance falloff; ignored when HasWorldPosition is false.</summary>
        public Vector3 WorldPosition;

        /// <summary>Whether WorldPosition participates in distance falloff.</summary>
        public bool HasWorldPosition;

        /// <summary>
        /// Falloff radius in world units; 0 uses the configured default radius.
        /// Only meaningful when HasWorldPosition is set.
        /// </summary>
        public float FalloffRadius;

        /// <summary>
        /// Optional one-shot kick direction in world space (normalized inside).
        /// Zero gives a pure omni-directional tremor.
        /// </summary>
        public Vector3 KickDirection;

        public CameraImpulsePriority Priority;
        public CameraImpulseCategory Category;
    }

    /// <summary>Amplitude presets tuned for restrained strategy-camera feedback.</summary>
    public static class CameraImpulseProfiles
    {
        public static CameraImpulseRequest Create(CameraImpulseProfile profile)
        {
            switch (profile)
            {
                case CameraImpulseProfile.Tiny:
                    return new CameraImpulseRequest
                    {
                        PositionAmplitude = 0.06f,
                        RotationAmplitude = 0.08f,
                        Duration = 0.22f,
                        Frequency = 16f,
                        Priority = CameraImpulsePriority.Low,
                    };
                case CameraImpulseProfile.Light:
                    return new CameraImpulseRequest
                    {
                        PositionAmplitude = 0.14f,
                        RotationAmplitude = 0.25f,
                        Duration = 0.3f,
                        Frequency = 13f,
                        Priority = CameraImpulsePriority.Medium,
                    };
                case CameraImpulseProfile.Heavy:
                    return new CameraImpulseRequest
                    {
                        PositionAmplitude = 0.32f,
                        RotationAmplitude = 0.6f,
                        Duration = 0.42f,
                        Frequency = 10f,
                        Priority = CameraImpulsePriority.High,
                    };
                case CameraImpulseProfile.Strategic:
                    return new CameraImpulseRequest
                    {
                        PositionAmplitude = 0.22f,
                        RotationAmplitude = 0.35f,
                        Duration = 0.55f,
                        Frequency = 7f,
                        Priority = CameraImpulsePriority.High,
                    };
                default:
                    return new CameraImpulseRequest
                    {
                        PositionAmplitude = 0.1f,
                        RotationAmplitude = 0.15f,
                        Duration = 0.25f,
                        Frequency = 14f,
                        Priority = CameraImpulsePriority.Low,
                    };
            }
        }

        /// <summary>Profile request anchored at a world position for distance falloff.</summary>
        public static CameraImpulseRequest At(CameraImpulseProfile profile, Vector3 worldPosition)
        {
            var request = Create(profile);
            request.WorldPosition = worldPosition;
            request.HasWorldPosition = true;
            return request;
        }
    }

    /// <summary>
    /// Camera impulse ("shake") service. Impulses are additive presentation
    /// offsets composed on top of the navigation pose; they never modify the
    /// canonical camera target, and the pose returns cleanly when they end.
    /// Player settings (Camera Effects, Shake Intensity, Reduce Motion) scale
    /// or disable all output.
    /// </summary>
    public interface ICameraFeedbackService
    {
        bool IsImpulseActive { get; }

        void RequestImpulse(CameraImpulseRequest request);
        void RequestImpulse(CameraImpulseProfile profile, Vector3 worldPosition);

        /// <summary>Cancels every active impulse and removes residual offsets.</summary>
        void CancelAllImpulses();
    }
}
