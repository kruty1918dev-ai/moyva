namespace UnityHTML.Runtime
{
    // Scroll behaviour knobs for MoyvaSmoothScrollRect, applied per host so every
    // mounted scroll control shares one configuration instead of each carrying
    // hand-tuned serialized values. Defaults match upstream SmoothScrollRect.
    public struct UnityHtmlScrollSettings
    {
        // Seconds spent easing each wheel step toward its target. 0 = instant.
        public float Smoothness;

        // Multiplier applied to the control's own wheel sensitivity baseline
        // (the scroll creator ships 50 rather than ScrollRect's 1). 1 = unchanged.
        public float WheelSensitivity;

        // Whether content keeps gliding after a drag is released.
        public bool Inertia;

        // Rate at which drag inertia decays (ScrollRect.decelerationRate).
        public float DecelerationRate;

        // Snap directly to the final scroll position: no easing and no inertia,
        // while wheel/drag input and position preservation stay unchanged.
        public bool ReducedMotion;

        public static UnityHtmlScrollSettings Default => new UnityHtmlScrollSettings
        {
            Smoothness = 0.12f,
            WheelSensitivity = 1f,
            Inertia = true,
            DecelerationRate = 0.135f,
            ReducedMotion = false,
        };

        public UnityHtmlScrollSettings WithReducedMotion(bool reduced)
        {
            UnityHtmlScrollSettings copy = this;
            copy.ReducedMotion = reduced;
            return copy;
        }

        public UnityHtmlScrollSettings WithWheelSensitivity(float sensitivity)
        {
            UnityHtmlScrollSettings copy = this;
            copy.WheelSensitivity = sensitivity;
            return copy;
        }
    }
}
