using Zenject;
using UnityHTML.Runtime;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static class GameplayHudBindings
    {
        public static void Install(DiContainer container)
        {
            container.Bind<GameplayHtmlAnchor>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();
            container.Bind<IUnityHtmlHost>()
                .To<UnityHtmlHost>()
                .AsSingle();
            container.Bind<GameplayHtmlState>()
                .AsSingle()
                .NonLazy();
            container.Bind<GameplayHudReadModel>()
                .AsSingle();
            container.BindInterfacesAndSelfTo<GameplayCargoPanel>()
                .AsSingle().NonLazy();
            container.BindInterfacesAndSelfTo<GameplayCameraFocusService>()
                .AsSingle();
            container.BindInterfacesAndSelfTo<GameplayWorldFocusPingPresenter>()
                .AsSingle()
                .NonLazy();
            container.BindInterfacesAndSelfTo<GameplayHtmlPresenter>()
                .AsSingle()
                .NonLazy();
            container.BindInterfacesAndSelfTo<BuildingConstructionProgressPresenter>()
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
