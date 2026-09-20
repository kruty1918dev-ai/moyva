using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Shared.UI;
using Zenject;

namespace Kruty1918.Moyva.Animations.Runtime
{
    /// <summary>
    /// Resolves <see cref="GameplayMotionConfig"/> values against the user's
    /// Reduce Motion preference (shared with <see cref="IUiMotionService"/>)
    /// and the active <see cref="GraphicsQualityProfile"/>.
    /// </summary>
    internal sealed class GameplayMotionSettingsProvider : IGameplayMotionSettingsProvider
    {
        private readonly GameplayMotionConfig _config;
        private readonly IUiMotionService _uiMotion;
        private readonly IGraphicsSettingsService _graphics;

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

        public UnitLocomotionMotionProfile UnitLocomotion => _config.unitLocomotion;
        public UnitTransitionMotionProfile UnitTransitions => _config.unitTransitions;
        public BuildingMotionProfile Building => _config.building;

        public bool ReducedMotion => _uiMotion?.ReducedMotion ?? false;

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

        public float ScaleDuration(float seconds)
        {
            if (seconds <= 0f)
                return 0f;
            return ReducedMotion
                ? seconds * _config.accessibility.reducedMotionDurationScale
                : seconds;
        }

        public float ScaleSecondaryAmplitude(float amplitude)
            => amplitude * SecondaryMotionScale;
    }
}
