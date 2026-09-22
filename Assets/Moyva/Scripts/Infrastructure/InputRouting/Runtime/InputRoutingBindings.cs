using Kruty1918.InputRouting.API;
using Kruty1918.InputRouting.Runtime;
using UnityEngine.EventSystems;
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
                .FromMethod(ctx => (IGameplayInputPolicy)new GameplayInputPolicy(
                    ctx.Container.TryResolve<EventSystem>()))
                .AsSingle();
        }
    }
}
