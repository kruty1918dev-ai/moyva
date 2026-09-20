namespace Kruty1918.Moyva.Animations.API
{
    /// <summary>
    /// Resolves gameplay motion tuning from <see cref="GameplayMotionConfig"/>,
    /// the user Reduce Motion flag and the active graphics quality profile.
    /// Presentation services consume this instead of hardcoding durations.
    /// </summary>
    public interface IGameplayMotionSettingsProvider
    {
        UnitLocomotionMotionProfile UnitLocomotion { get; }
        UnitTransitionMotionProfile UnitTransitions { get; }
        BuildingMotionProfile Building { get; }

        /// <summary>User Reduced Motion preference (shared with UI motion).</summary>
        bool ReducedMotion { get; }

        /// <summary>0..1 — secondary motion strength (bob, shake, overshoot) for the active quality tier.</summary>
        float SecondaryMotionScale { get; }

        /// <summary>Applies Reduce Motion shortening to a transition duration.</summary>
        float ScaleDuration(float seconds);

        /// <summary>Applies quality/reduced-motion scaling to secondary amplitudes (bob, recoil, shake).</summary>
        float ScaleSecondaryAmplitude(float amplitude);
    }
}
