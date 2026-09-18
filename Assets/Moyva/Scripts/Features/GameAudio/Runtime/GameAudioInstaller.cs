using Kruty1918.Moyva.GameAudio.API;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Notifications.API;
using Kruty1918.Moyva.UIActions.API;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Композиція аудіо-шару гри.
    /// InstallShared — у ProjectContext (працює і в меню, і в грі).
    /// InstallGameplay — у сцені геймплею (емітери, zoom, доменні події).
    /// </summary>
    public static class GameAudioInstaller
    {
        /// <summary>Конфігурації + глобальні звуки вказівника. Викликати в ProjectContext.</summary>
        public static void InstallShared(DiContainer container)
        {
            if (!container.HasBinding<AudioFeedbackConfig>())
                container.Bind<AudioFeedbackConfig>()
                    .FromMethod(_ => ResolveConfig<AudioFeedbackConfig>())
                    .AsSingle()
                    .IfNotBound();

            if (!container.HasBinding<AudioAmbienceConfig>())
                container.Bind<AudioAmbienceConfig>()
                    .FromMethod(_ => ResolveConfig<AudioAmbienceConfig>())
                    .AsSingle()
                    .IfNotBound();

            if (!container.HasBinding<UiPointerAudioService>())
                container.BindInterfacesAndSelfTo<UiPointerAudioService>()
                    .AsSingle()
                    .NonLazy();
        }

        /// <summary>Сценові сервіси: zoom-фокус, ambient-шари, емітери, доменний фідбек.</summary>
        public static void InstallGameplay(DiContainer container)
        {
            InstallShared(container);

            container.BindInterfacesAndSelfTo<AudioZoomFocusService>().AsSingle().NonLazy();
            container.BindInterfacesAndSelfTo<AmbienceBedService>().AsSingle().NonLazy();
            container.BindInterfacesAndSelfTo<AmbienceOneShotService>().AsSingle().NonLazy();
            container.BindInterfacesAndSelfTo<AmbientWorldAudioService>().AsSingle().NonLazy();
            container.BindInterfacesAndSelfTo<GameplayAudioFeedbackService>().AsSingle().NonLazy();
            if (!container.HasBinding<IUiActionFeedbackSink>())
                container.Bind<IUiActionFeedbackSink>().To<UiActionAudioSink>().AsSingle();

            // Decorate лінивий: фактичний binding може бути оголошений іншим інсталером пізніше.
            container.Decorate<IGameplayNotificationService>().With<NotificationAudioDecorator>();
        }

        private static T ResolveConfig<T>() where T : class
            => MoyvaJsonRuntime.GetLegacyResource<T>(typeof(T).Name);
    }
}
