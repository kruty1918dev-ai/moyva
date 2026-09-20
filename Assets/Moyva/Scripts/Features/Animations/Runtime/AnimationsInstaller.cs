using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Jsonization;
using Zenject;

namespace Kruty1918.Moyva.Animations.Runtime
{
    /// <summary>Zenject-інсталер модуля анімацій.</summary>
    public class AnimationsInstaller : MonoInstaller
    {
        /// <summary>Реєструє біндінги модуля анімацій.</summary>
        public override void InstallBindings()
        {
            Container.Bind<IMovementAnimationService>()
                .To<MovementAnimationService>()
                .AsSingle();

            Container.Bind<GameplayMotionConfig>()
                .FromMethod(_ => ResolveMotionConfig())
                .AsSingle()
                .IfNotBound();

            Container.Bind<IGameplayMotionSettingsProvider>()
                .To<GameplayMotionSettingsProvider>()
                .AsSingle()
                .IfNotBound();
        }

        private static GameplayMotionConfig ResolveMotionConfig()
        {
            // JSON preset is authoritative; a missing document falls back to
            // the coded defaults so headless/test contexts still work.
            return MoyvaJsonRuntime.GetLegacyResource<GameplayMotionConfig>(nameof(GameplayMotionConfig))
                   ?? new GameplayMotionConfig();
        }
    }
}
