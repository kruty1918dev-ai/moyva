using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.UiFoundation;
using Zenject;

namespace Kruty1918.Moyva.Animations.Runtime
{
    /// <summary>Адаптер конфігурації gameplay-рухів: віддає профілі з GameplayMotionConfig з урахуванням доступності та якості.</summary>
    internal sealed class GameplayMotionSettingsProvider : IGameplayMotionSettingsProvider
    {
        private readonly GameplayMotionConfig _config;
        private readonly IUiMotionService _uiMotion;
        private readonly IGraphicsSettingsService _graphics;

        /// <summary>Створює провайдера з конфігурацією та джерелом UI-налаштувань руху.</summary>
        [Inject]
        public GameplayMotionSettingsProvider(
            [InjectOptional] GameplayMotionConfig config = null,
            [InjectOptional] IUiMotionService uiMotion = null,
            [InjectOptional] IGraphicsSettingsService graphics = null)
        {
            _config = config ?? new GameplayMotionConfig();
            _uiMotion = uiMotion;
            _graphics = graphics;
        }

        /// <summary>Профіль локомоції юнітів.</summary>
        public UnitLocomotionMotionProfile UnitLocomotion => _config.unitLocomotion;
        /// <summary>Профіль переходів юнітів.</summary>
        public UnitTransitionMotionProfile UnitTransitions => _config.unitTransitions;
        /// <summary>Профіль рухів будівель.</summary>
        public BuildingMotionProfile Building => _config.building;

        /// <summary>Чи ввімкнено режим зменшеного руху.</summary>
        public bool ReducedMotion => _uiMotion?.ReducedMotion ?? false;

        /// <summary>Масштаб вторинного руху за рівнем якості.</summary>
        public float SecondaryMotionScale
        {
            get
            {
                if (ReducedMotion)
                    return 0f;

                switch (_graphics?.Settings.Profile ?? GraphicsQualityProfile.Auto)
                {
                    case GraphicsQualityProfile.Performance:
                        return _config.accessibility.secondaryMotionPerformanceScale;
                    case GraphicsQualityProfile.Quality:
                        return _config.accessibility.secondaryMotionQualityScale;
                    case GraphicsQualityProfile.Balanced:
                    case GraphicsQualityProfile.Auto:
                    case GraphicsQualityProfile.Custom:
                    default:
                        return _config.accessibility.secondaryMotionBalancedScale;
                }
            }
        }

        /// <summary>Масштабує тривалість відповідно до Reduced Motion.</summary>
        public float ScaleDuration(float seconds)
        {
            if (seconds <= 0f)
                return 0f;
            return ReducedMotion
                ? seconds * _config.accessibility.reducedMotionDurationScale
                : seconds;
        }

        /// <summary>Масштабує амплітуду вторинного руху за рівнем якості.</summary>
        public float ScaleSecondaryAmplitude(float amplitude)
            => amplitude * SecondaryMotionScale;
    }
}
