using Zenject;
using Kruty1918.Notifications.API;
using Kruty1918.Notifications.Runtime;

namespace Kruty1918.Moyva.Notifications.Runtime
{
    public static class NotificationsInstaller
    {
        public static void Install(DiContainer container)
        {
            if (!container.HasBinding(typeof(GameplayNotificationSettings)))
                container.Bind<GameplayNotificationSettings>().AsSingle();

            if (!container.HasBinding(typeof(IGameplayNotificationService)))
            {
                container.BindInterfacesAndSelfTo<GameplayNotificationService>()
                    .FromMethod(ctx => new GameplayNotificationService(
                        ctx.Container.Resolve<GameplayNotificationSettings>(),
                        ctx.Container.TryResolve<IGameplayNotificationPresenter>()))
                    .AsSingle()
                    .NonLazy();
            }
        }
    }
}
