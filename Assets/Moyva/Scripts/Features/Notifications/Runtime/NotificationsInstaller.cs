using Zenject;
using Kruty1918.Moyva.Notifications.API;

namespace Kruty1918.Moyva.Notifications.Runtime
{
    public static class NotificationsInstaller
    {
        public static void Install(DiContainer container)
        {
            if (!container.HasBinding(typeof(GameplayNotificationSettings)))
                container.Bind<GameplayNotificationSettings>().AsSingle();

            if (!container.HasBinding(typeof(IGameplayNotificationPresenter)))
            {
                container.BindInterfacesAndSelfTo<GameplayNotificationPresenter>()
                    .FromNewComponentOnNewGameObject()
                    .WithGameObjectName("GameplayNotificationPresenter")
                    .AsSingle()
                    .NonLazy();
            }

            if (!container.HasBinding(typeof(IGameplayNotificationService)))
            {
                container.BindInterfacesAndSelfTo<GameplayNotificationService>()
                    .AsSingle()
                    .NonLazy();
            }
        }
    }
}
