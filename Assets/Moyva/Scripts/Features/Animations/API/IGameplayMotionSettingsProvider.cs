namespace Kruty1918.Moyva.Animations.API
{
    /// <summary>Провайдер налаштувань gameplay-рухів з урахуванням доступності та якості.</summary>
    public interface IGameplayMotionSettingsProvider
    {
        /// <summary>Профіль локомоції юнітів.</summary>
        UnitLocomotionMotionProfile UnitLocomotion { get; }
        /// <summary>Профіль переходів юнітів.</summary>
        UnitTransitionMotionProfile UnitTransitions { get; }
        /// <summary>Профіль рухів будівель.</summary>
        BuildingMotionProfile Building { get; }

        /// <summary>Чи ввімкнено режим зменшеного руху.</summary>
        bool ReducedMotion { get; }

        /// <summary>Масштаб вторинного руху за рівнем якості.</summary>
        float SecondaryMotionScale { get; }

        /// <summary>Масштабує тривалість відповідно до Reduced Motion.</summary>
        float ScaleDuration(float seconds);

        /// <summary>Масштабує амплітуду вторинного руху за рівнем якості.</summary>
        float ScaleSecondaryAmplitude(float amplitude);
    }
}
