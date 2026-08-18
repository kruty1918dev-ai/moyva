using Kruty1918.Moyva.InputRouting.API;
using Zenject;

namespace Kruty1918.Moyva.InputRouting.Runtime
{
    public static class InputRoutingBindings
    {
        public static void Install(DiContainer container)
        {
            if (container == null || container.HasBinding<IGameplayInputPolicy>())
                return;

            container.Bind<IGameplayInputPolicy>()
                .To<GameplayInputPolicy>()
                .AsSingle();
        }
    }
}
