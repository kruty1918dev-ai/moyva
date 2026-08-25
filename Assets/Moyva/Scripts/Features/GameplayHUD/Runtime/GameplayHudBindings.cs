using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static class GameplayHudBindings
    {
        public static void Install(DiContainer container)
        {
            container.Bind<GameplayTurnHudView>()
                .FromComponentInHierarchy()
                .AsSingle();
            container.BindInterfacesTo<GameplayTurnHudPresenter>()
                .AsSingle()
                .NonLazy();
            container.BindInterfacesAndSelfTo<UnitRecruitmentProgressIndicatorPresenter>()
                .AsSingle()
                .NonLazy();
            container.BindInterfacesAndSelfTo<UnitRecruitmentReadyIndicatorPresenter>()
                .AsSingle()
                .NonLazy();
            container.BindInterfacesAndSelfTo<UnitRecruitmentDeploymentController>()
                .AsSingle()
                .NonLazy();
        }
    }
}
