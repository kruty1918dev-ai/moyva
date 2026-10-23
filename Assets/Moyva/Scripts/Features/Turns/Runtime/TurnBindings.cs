using Kruty1918.Moyva.Turns.API;
using Zenject;

namespace Kruty1918.Moyva.Turns.Runtime
{
    public static class TurnBindings
    {
        public static void Install(DiContainer container)
        {
            container.Bind<TurnRules>().FromMethod(_ => TurnRulesResolver.Resolve()).AsSingle();
            container.BindInterfacesAndSelfTo<TurnService>().AsSingle().NonLazy();
            container.BindInterfacesAndSelfTo<TurnRoundTimerService>().AsSingle().NonLazy();
            container.BindInterfacesAndSelfTo<GameplayProgressClock>().AsSingle().NonLazy();
        }
    }
}
