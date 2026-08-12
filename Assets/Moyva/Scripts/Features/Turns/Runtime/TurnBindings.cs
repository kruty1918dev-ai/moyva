using Zenject;

namespace Kruty1918.Moyva.Turns.Runtime
{
    public static class TurnBindings
    {
        public static void Install(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<TurnService>().AsSingle().NonLazy();
        }
    }
}
